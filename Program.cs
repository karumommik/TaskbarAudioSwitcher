using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using System.Runtime.InteropServices;
using Microsoft.Win32;

namespace TaskbarAudioSwitcher
{
    // ==========================================
    // CORE AUDIO & IPOLICYCONFIG COM INTERFACES
    // ==========================================

    [Guid("A95664D2-9614-4F35-A746-DE8DB63617E6"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    internal interface IMMDeviceEnumerator
    {
        [PreserveSig] int EnumAudioEndpoints(int dataFlow, int stateMask, out IMMDeviceCollection devices);
        [PreserveSig] int GetDefaultAudioEndpoint(int dataFlow, int role, out IMMDevice endpoint);
        [PreserveSig] int GetDevice([MarshalAs(UnmanagedType.LPWStr)] string id, out IMMDevice device);
        [PreserveSig] int RegisterEndpointNotificationCallback(IntPtr client);
        [PreserveSig] int UnregisterEndpointNotificationCallback(IntPtr client);
    }

    [Guid("0BD7A1BE-7A1A-44DB-8397-CC5392387B5E"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    internal interface IMMDeviceCollection
    {
        [PreserveSig] int GetCount(out uint pcDevices);
        [PreserveSig] int Item(uint nDevice, out IMMDevice ppDevice);
    }

    [Guid("D666063F-1587-4E43-81F1-B948E807363F"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    internal interface IMMDevice
    {
        [PreserveSig] int Activate(ref Guid iid, int dwClsCtx, IntPtr pActivationParams, [MarshalAs(UnmanagedType.IUnknown)] out object ppInterface);
        [PreserveSig] int OpenPropertyStore(int stgmAccess, out IPropertyStore ppProperties);
        [PreserveSig] int GetId([MarshalAs(UnmanagedType.LPWStr)] out string ppstrId);
        [PreserveSig] int GetState(out int pdwState);
    }

    [Guid("886D8EEB-8CF2-4446-8D02-CDBA1DBDCF99"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    internal interface IPropertyStore
    {
        [PreserveSig] int GetCount(out uint propertyCount);
        [PreserveSig] int GetAt(uint propertyIndex, out PROPERTYKEY key);
        [PreserveSig] int GetValue(ref PROPERTYKEY key, out PROPVARIANT pv);
        [PreserveSig] int SetValue(ref PROPERTYKEY key, ref PROPVARIANT pv);
        [PreserveSig] int Commit();
    }

    [Guid("5CDF2C82-841E-4546-9722-0CF74078229A"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    internal interface IAudioEndpointVolume
    {
        [PreserveSig] int RegisterControlChangeNotify(IntPtr pNotify);
        [PreserveSig] int UnregisterControlChangeNotify(IntPtr pNotify);
        [PreserveSig] int GetChannelCount(out int pnChannelCount);
        [PreserveSig] int SetMasterVolumeLevel(float fLevelDB, ref Guid pguidEventContext);
        [PreserveSig] int SetMasterVolumeLevelScalar(float fLevel, ref Guid pguidEventContext);
        [PreserveSig] int GetMasterVolumeLevel(out float pfLevelDB);
        [PreserveSig] int GetMasterVolumeLevelScalar(out float pfLevel);
        [PreserveSig] int SetChannelVolumeLevel(int nChannel, float fLevelDB, ref Guid pguidEventContext);
        [PreserveSig] int SetChannelVolumeLevelScalar(int nChannel, float fLevel, ref Guid pguidEventContext);
        [PreserveSig] int GetChannelVolumeLevel(int nChannel, out float pfLevelDB);
        [PreserveSig] int GetChannelVolumeLevelScalar(int nChannel, out float pfLevel);
        [PreserveSig] int SetMute([MarshalAs(UnmanagedType.Bool)] bool bMute, ref Guid pguidEventContext);
        [PreserveSig] int GetMute([MarshalAs(UnmanagedType.Bool)] out bool pbMute);
        [PreserveSig] int GetVolumeStepInfo(out uint pnStep, out uint pnStepCount);
        [PreserveSig] int VolumeStepDown(ref Guid pguidEventContext);
        [PreserveSig] int VolumeStepUp(ref Guid pguidEventContext);
        [PreserveSig] int QueryHardwareSupport(out uint pdwHardwareSupportMask);
        [PreserveSig] int GetVolumeRange(out float pflVolumeMindB, out float pflVolumeMaxdB, out float pflVolumeIncrementdB);
    }

    [StructLayout(LayoutKind.Sequential)]
    internal struct PROPERTYKEY
    {
        public Guid fmtid;
        public uint pid;
        public PROPERTYKEY(Guid fmtid, uint pid) { this.fmtid = fmtid; this.pid = pid; }
    }

    [StructLayout(LayoutKind.Sequential)]
    internal struct PROPVARIANT
    {
        public ushort vt;
        public ushort wReserved1;
        public ushort wReserved2;
        public ushort wReserved3;
        public IntPtr pointerVal;
    }

    [Guid("f8679f50-850a-41cf-9c72-430f290290c8"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    internal interface IPolicyConfig
    {
        [PreserveSig] int GetMixFormat([MarshalAs(UnmanagedType.LPWStr)] string pszDeviceName, out IntPtr ppFormat);
        [PreserveSig] int GetDeviceFormat([MarshalAs(UnmanagedType.LPWStr)] string pszDeviceName, bool bDefault, out IntPtr ppFormat);
        [PreserveSig] int ResetDeviceFormat([MarshalAs(UnmanagedType.LPWStr)] string pszDeviceName);
        [PreserveSig] int SetDeviceFormat([MarshalAs(UnmanagedType.LPWStr)] string pszDeviceName, IntPtr pEndpointFormat, IntPtr MixFormat);
        [PreserveSig] int GetProcessingPeriod([MarshalAs(UnmanagedType.LPWStr)] string pszDeviceName, bool bDefault, out long pmftDefaultPeriod, out long pmftMinimumPeriod);
        [PreserveSig] int SetProcessingPeriod([MarshalAs(UnmanagedType.LPWStr)] string pszDeviceName, long pmftPeriod);
        [PreserveSig] int GetShareMode([MarshalAs(UnmanagedType.LPWStr)] string pszDeviceName, out int pMode);
        [PreserveSig] int SetShareMode([MarshalAs(UnmanagedType.LPWStr)] string pszDeviceName, int mode);
        [PreserveSig] int GetPropertyValue([MarshalAs(UnmanagedType.LPWStr)] string pszDeviceName, bool bFxStore, IntPtr key, IntPtr pv);
        [PreserveSig] int SetPropertyValue([MarshalAs(UnmanagedType.LPWStr)] string pszDeviceName, bool bFxStore, IntPtr key, IntPtr pv);
        [PreserveSig] int SetDefaultEndpoint([MarshalAs(UnmanagedType.LPWStr)] string pszDeviceName, int role);
        [PreserveSig] int SetEndpointVisibility([MarshalAs(UnmanagedType.LPWStr)] string wszDeviceId, bool bVisible);
    }

    [ComImport, Guid("870af99c-171d-4f9e-af0d-e63df40c2bc9")]
    internal class CPolicyConfigClient { }

    [ComImport, Guid("BCDE0395-E52F-467C-8E3D-C4579291692E")]
    internal class MMDeviceEnumeratorCom { }

    // ==========================================
    // DRAWING EXTENSIONS FOR ROUNDED CORNERS
    // ==========================================

    public static class GraphicsExtensions
    {
        public static void FillRoundRectangle(this Graphics g, Brush brush, float x, float y, float width, float height, float radius)
        {
            using (var path = GetRoundRectPath(x, y, width, height, radius))
            {
                g.FillPath(brush, path);
            }
        }

        public static void DrawRoundRectangle(this Graphics g, Pen pen, float x, float y, float width, float height, float radius)
        {
            using (var path = GetRoundRectPath(x, y, width, height, radius))
            {
                g.DrawPath(pen, path);
            }
        }

        private static GraphicsPath GetRoundRectPath(float x, float y, float width, float height, float radius)
        {
            float r2 = radius * 2;
            var path = new GraphicsPath();
            if (radius <= 0)
            {
                path.AddRectangle(new RectangleF(x, y, width, height));
                return path;
            }
            path.AddArc(x, y, r2, r2, 180, 90);
            path.AddArc(x + width - r2, y, r2, r2, 270, 90);
            path.AddArc(x + width - r2, y + height - r2, r2, r2, 0, 90);
            path.AddArc(x, y + height - r2, r2, r2, 90, 90);
            path.CloseFigure();
            return path;
        }
    }

    // ==========================================
    // CUSTOM CONTROLS (BUTTON & SLIDER)
    // ==========================================

    public class IconButton : Control
    {
        public string Glyph { get; set; }
        public bool IsActive { get; set; }
        public Color ActiveBgColor { get; set; }
        public Color ActiveFgColor { get; set; }
        public Color HoverBgColor { get; set; }

        private bool isHovered = false;

        public IconButton()
        {
            DoubleBuffered = true;
            Size = new Size(28, 28);
            ActiveBgColor = Color.FromArgb(0, 120, 215);
            ActiveFgColor = Color.White;
            HoverBgColor = Color.FromArgb(20, 128, 128, 128);
        }

        protected override void OnMouseEnter(EventArgs e)
        {
            base.OnMouseEnter(e);
            isHovered = true;
            Invalidate();
        }

        protected override void OnMouseLeave(EventArgs e)
        {
            base.OnMouseLeave(e);
            isHovered = false;
            Invalidate();
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            var g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;

            Color bg = Color.Transparent;
            Color border = Color.Transparent;

            if (IsActive)
            {
                bg = ActiveBgColor;
                border = ActiveBgColor;
            }
            else if (isHovered)
            {
                bg = HoverBgColor;
            }

            if (bg != Color.Transparent)
            {
                using (var brush = new SolidBrush(bg))
                {
                    g.FillRoundRectangle(brush, 0, 0, Width - 1, Height - 1, 4);
                }
            }

            if (border != Color.Transparent)
            {
                using (var pen = new Pen(border, 1f))
                {
                    g.DrawRoundRectangle(pen, 0, 0, Width - 1, Height - 1, 4);
                }
            }

            Font fontToUse = null;
            try { fontToUse = new Font("Segoe MDL2 Assets", 10f); }
            catch { fontToUse = new Font("Segoe UI", 10f); }

            using (fontToUse)
            {
                Color textColor = IsActive ? ActiveFgColor : ForeColor;
                using (var brush = new SolidBrush(textColor))
                {
                    var sf = new StringFormat
                    {
                        Alignment = StringAlignment.Center,
                        LineAlignment = StringAlignment.Center
                    };
                    g.DrawString(Glyph, fontToUse, brush, new RectangleF(0, 0, Width, Height), sf);
                }
            }
        }
    }

    public class VolumeSlider : Control
    {
        private float value = 0.5f; // 0.0 to 1.0
        public event EventHandler ValueChanged;
        public bool IsDragging { get; private set; }

        public float Value
        {
            get { return value; }
            set
            {
                float val = Math.Max(0.0f, Math.Min(1.0f, value));
                if (this.value != val)
                {
                    this.value = val;
                    Invalidate();
                }
            }
        }

        // Programmatic update that doesn't trigger loop loops
        public void UpdateValue(float val)
        {
            val = Math.Max(0.0f, Math.Min(1.0f, val));
            if (this.value != val)
            {
                this.value = val;
                Invalidate();
            }
        }

        public Color ActiveColor { get; set; }
        public Color InactiveColor { get; set; }

        public VolumeSlider()
        {
            DoubleBuffered = true;
            Height = 20;
            Width = 70;
            ActiveColor = Color.FromArgb(0, 120, 215);
            InactiveColor = Color.FromArgb(80, 128, 128, 128);
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            var g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;

            int trackHeight = 4;
            int y = (Height - trackHeight) / 2;
            int x = 6;
            int w = Width - 12;

            // Inactive track
            using (var brush = new SolidBrush(InactiveColor))
            {
                g.FillRoundRectangle(brush, x, y, w, trackHeight, 2);
            }

            // Active track
            int activeWidth = (int)(w * value);
            if (activeWidth > 0)
            {
                using (var brush = new SolidBrush(ActiveColor))
                {
                    g.FillRoundRectangle(brush, x, y, activeWidth, trackHeight, 2);
                }
            }

            // Thumb
            int thumbRadius = 6;
            int thumbX = x + activeWidth;
            int thumbY = Height / 2;

            using (var brush = new SolidBrush(Color.White))
            {
                g.FillEllipse(brush, thumbX - thumbRadius, thumbY - thumbRadius, thumbRadius * 2, thumbRadius * 2);
            }
            using (var pen = new Pen(ActiveColor, 2))
            {
                g.DrawEllipse(pen, thumbX - thumbRadius, thumbY - thumbRadius, thumbRadius * 2, thumbRadius * 2);
            }
        }

        protected override void OnMouseDown(MouseEventArgs e)
        {
            base.OnMouseDown(e);
            if (e.Button == MouseButtons.Left)
            {
                IsDragging = true;
                UpdateValueFromMouse(e.X);
            }
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);
            if (IsDragging)
            {
                UpdateValueFromMouse(e.X);
            }
        }

        protected override void OnMouseUp(MouseEventArgs e)
        {
            base.OnMouseUp(e);
            if (IsDragging)
            {
                IsDragging = false;
                if (ValueChanged != null) ValueChanged(this, EventArgs.Empty);
            }
        }

        private void UpdateValueFromMouse(int mouseX)
        {
            int x = 6;
            int w = Width - 12;
            float val = (float)(mouseX - x) / w;
            val = Math.Max(0.0f, Math.Min(1.0f, val));
            if (value != val)
            {
                value = val;
                Invalidate();
                if (IsDragging && ValueChanged != null) ValueChanged(this, EventArgs.Empty);
            }
        }
    }

    // ==========================================
    // MAIN APP WIDGET FORM
    // ==========================================

    public class AudioWidgetForm : Form
    {
        private IMMDeviceEnumerator enumerator;
        private IPolicyConfig policyConfig;
        private AppSettings settings;

        // UI Controls
        private IconButton[] btnDevices;
        private IconButton btnMute;
        private VolumeSlider sliderVolume;
        private Label lblVolumeText;
        private System.Windows.Forms.Timer updateTimer;
        private ToolTip toolTip;
        private NotifyIcon notifyIcon;

        // Mixer panel variables
        private bool isExpanded = false;
        private Panel pnlMixer;
        private IconButton btnMixer;
        private List<MixerRow> mixerRows = new List<MixerRow>();
        private int gcCounter = 0;
        private uint activeFullscreenProcessId = 0;
        private string activeFullscreenScreenDeviceName = null;

        private class MixerRow
        {
            public string SessionId;
            public Panel RowPanel;
            public PictureBox IconBox;
            public Label IconLabel;
            public Label NameLabel;
            public VolumeSlider Slider;
            public Label VolLabel;
        }

        private struct SessionData
        {
            public string SessionId;
            public int ProcessId;
            public bool IsSystemSounds;
            public float Volume;
            public bool Mute;
            public string Name;
            public Icon Icon;
        }

        // Layout variables
        private int separatorX = 0;
        private string[] activeDeviceIds = new string[0];
        private string currentDefaultId = "";
        
        // Theme Colors
        private Color themeBgColor;
        private Color themeBorderColor;
        private Color themeTextColor;
        private Color themeHoverBgColor;
        private Color themeActiveBgColor = Color.FromArgb(0, 120, 215);
        private bool isDarkMode = true;

        private const string RunRegistryKey = @"Software\Microsoft\Windows\CurrentVersion\Run";
        private const string AppName = "TaskbarAudioSwitcher";

        // Prevent taking window focus so active windows don't lose focus
        protected override CreateParams CreateParams
        {
            get
            {
                CreateParams cp = base.CreateParams;
                // WS_EX_NOACTIVATE = 0x08000000
                // WS_EX_TOPMOST = 0x00000008
                // WS_EX_TOOLWINDOW = 0x00000080
                cp.ExStyle |= 0x08000000 | 0x00000008 | 0x00000080;
                return cp;
            }
        }

        protected override bool ShowWithoutActivation
        {
            get { return true; }
        }

        public AudioWidgetForm()
        {
            // Load Settings
            settings = AppSettings.Load();

            // Windows settings
            this.FormBorderStyle = FormBorderStyle.None;
            this.ShowInTaskbar = false;
            this.TopMost = true;
            this.Height = 36;
            this.BackColor = Color.Fuchsia;
            this.TransparencyKey = Color.Fuchsia;
            this.DoubleBuffered = true;

            // Initialize COM interfaces
            try
            {
                System.IO.File.WriteAllText(System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "comlog.txt"), "Starting COM init...\n");
                enumerator = (IMMDeviceEnumerator)new MMDeviceEnumeratorCom();
                System.IO.File.AppendAllText(System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "comlog.txt"), "Enumerator created.\n");
                policyConfig = (IPolicyConfig)new CPolicyConfigClient();
                System.IO.File.AppendAllText(System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "comlog.txt"), "PolicyConfig created.\n");
            }
            catch (Exception ex)
            {
                System.IO.File.AppendAllText(System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "comlog.txt"), "ERROR: " + ex.ToString() + "\n");
                MessageBox.Show("Failed to initialize Windows Audio COM components: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                Application.Exit();
            }

            // Create tray icon
            InitializeTrayIcon();

            // Setup Theme Colors
            UpdateThemeColors();

            // Create controls
            toolTip = new ToolTip();
            toolTip.InitialDelay = 400;
            toolTip.ReshowDelay = 100;

            btnDevices = new IconButton[3];
            for (int i = 0; i < 3; i++)
            {
                int index = i;
                btnDevices[i] = new IconButton
                {
                    Glyph = "\uE767",
                    Visible = false,
                    ForeColor = themeTextColor,
                    HoverBgColor = themeHoverBgColor,
                    ActiveBgColor = themeActiveBgColor,
                    ActiveFgColor = Color.White
                };
                btnDevices[i].Click += (s, e) => SwitchDevice(index);
                this.Controls.Add(btnDevices[i]);
            }

            btnMute = new IconButton
            {
                Glyph = "\uE767",
                ForeColor = themeTextColor,
                HoverBgColor = themeHoverBgColor,
                ActiveBgColor = Color.Transparent,
                IsActive = false
            };
            btnMute.Click += BtnMute_Click;
            this.Controls.Add(btnMute);

            sliderVolume = new VolumeSlider
            {
                ActiveColor = themeActiveBgColor,
                InactiveColor = Color.FromArgb(80, 128, 128, 128)
            };
            sliderVolume.ValueChanged += SliderVolume_ValueChanged;
            this.Controls.Add(sliderVolume);

            lblVolumeText = new Label
            {
                Font = new Font("Segoe UI", 8f),
                ForeColor = themeTextColor,
                TextAlign = ContentAlignment.MiddleLeft,
                Size = new Size(32, 16)
            };
            this.Controls.Add(lblVolumeText);

            btnMixer = new IconButton
            {
                Glyph = "\uE71D",
                ForeColor = themeTextColor,
                HoverBgColor = themeHoverBgColor,
                ActiveBgColor = Color.Transparent,
                IsActive = false
            };
            btnMixer.Click += (s, e) => ToggleMixer();
            this.Controls.Add(btnMixer);

            pnlMixer = new Panel
            {
                Location = new Point(0, 0),
                Size = new Size(100, 100),
                BackColor = themeBgColor,
                Visible = false,
                AutoScroll = true
            };
            this.Controls.Add(pnlMixer);

            // Register MouseWheel events
            this.MouseWheel += Form_MouseWheel;
            sliderVolume.MouseWheel += Form_MouseWheel;
            btnMute.MouseWheel += Form_MouseWheel;
            btnMixer.MouseWheel += Form_MouseWheel;
            pnlMixer.MouseWheel += Form_MouseWheel;
            foreach (var btn in btnDevices) btn.MouseWheel += Form_MouseWheel;
            // Auto-collapse mixer when clicking outside (deactivation)
            this.Deactivate += (s, e) => {
                if (isExpanded) ToggleMixer();
            };
            updateTimer = new System.Windows.Forms.Timer { Interval = 500 };
            updateTimer.Tick += UpdateTimer_Tick;
            updateTimer.Start();

            // Initial alignment and populate
            RefreshAudioState();
            UpdateLayout();
            UpdatePosition();

            // Track form closing reason
            this.FormClosing += (s, e) => {
                try
                {
                    System.IO.File.WriteAllText(
                        System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "closelog.txt"),
                        string.Format("Form closing. Reason: {0}\nStack Trace:\n{1}", e.CloseReason, Environment.StackTrace)
                    );
                }
                catch { }
            };
        }

        private void InitializeTrayIcon()
        {
            notifyIcon = new NotifyIcon();
            try
            {
                Bitmap bmp = new Bitmap(16, 16);
                using (Graphics g = Graphics.FromImage(bmp))
                {
                    g.SmoothingMode = SmoothingMode.AntiAlias;
                    g.FillEllipse(Brushes.DodgerBlue, 1, 1, 14, 14);
                    using (Font font = new Font("Segoe MDL2 Assets", 8f))
                    {
                        g.DrawString("\uE767", font, Brushes.White, new RectangleF(-1, 2, 18, 16));
                    }
                }
                notifyIcon.Icon = Icon.FromHandle(bmp.GetHicon());
            }
            catch
            {
                notifyIcon.Icon = SystemIcons.Application;
            }

            notifyIcon.Text = "Taskbar Audio Switcher";
            notifyIcon.Visible = true;

            // Context Menu
            ContextMenu contextMenu = new ContextMenu();
            
            MenuItem titleItem = new MenuItem("Taskbar Audio Switcher");
            titleItem.Enabled = false;
            contextMenu.MenuItems.Add(titleItem);

            MenuItem startupItem = new MenuItem("Run at Windows Startup", (s, e) => {
                MenuItem mi = (MenuItem)s;
                mi.Checked = !mi.Checked;
                SetStartup(mi.Checked);
            });
            startupItem.Checked = IsStartupEnabled();
            contextMenu.MenuItems.Add(startupItem);

            MenuItem settingsItem = new MenuItem("Settings", (s, e) => ShowSettings());
            contextMenu.MenuItems.Add(settingsItem);

            MenuItem repositionItem = new MenuItem("Reposition Now", (s, e) => {
                UpdatePosition();
            });
            contextMenu.MenuItems.Add(repositionItem);

            contextMenu.MenuItems.Add("-");

            MenuItem exitItem = new MenuItem("Exit", (s, e) => {
                notifyIcon.Visible = false;
                Application.Exit();
            });
            contextMenu.MenuItems.Add(exitItem);

            notifyIcon.ContextMenu = contextMenu;
            notifyIcon.DoubleClick += (s, e) => UpdatePosition();
        }

        private void ShowSettings()
        {
            foreach (Form openForm in Application.OpenForms)
            {
                if (openForm is SettingsForm)
                {
                    openForm.BringToFront();
                    return;
                }
            }

            var form = new SettingsForm(settings, enumerator, isDarkMode);
            if (form.ShowDialog() == DialogResult.OK)
            {
                this.settings = AppSettings.Load();
                RefreshAudioState();
                UpdateLayout();
                UpdatePosition();
            }
        }

        private void UpdateThemeColors()
        {
            bool systemDarkMode = true;
            try
            {
                using (var key = Registry.CurrentUser.OpenSubKey(@"Software\Microsoft\Windows\CurrentVersion\Themes\Personalize"))
                {
                    if (key != null)
                    {
                        var value = key.GetValue("AppsUseLightTheme");
                        if (value != null)
                        {
                            systemDarkMode = (int)value == 0;
                        }
                    }
                }
            }
            catch { }

            this.isDarkMode = systemDarkMode;

            if (isDarkMode)
            {
                themeBgColor = Color.FromArgb(28, 28, 28);
                themeBorderColor = Color.FromArgb(55, 55, 55);
                themeTextColor = Color.FromArgb(235, 235, 235);
                themeHoverBgColor = Color.FromArgb(20, 255, 255, 255);
            }
            else
            {
                themeBgColor = Color.FromArgb(243, 243, 243);
                themeBorderColor = Color.FromArgb(210, 210, 210);
                themeTextColor = Color.FromArgb(40, 40, 40);
                themeHoverBgColor = Color.FromArgb(15, 0, 0, 0);
            }

            // Apply to active controls
            if (lblVolumeText != null)
            {
                lblVolumeText.BackColor = themeBgColor;
                lblVolumeText.ForeColor = themeTextColor;
            }
            if (btnMute != null)
            {
                btnMute.BackColor = themeBgColor;
                btnMute.ForeColor = themeTextColor;
                btnMute.HoverBgColor = themeHoverBgColor;
            }
            if (sliderVolume != null)
            {
                sliderVolume.BackColor = themeBgColor;
            }
            if (btnDevices != null)
            {
                for (int i = 0; i < 3; i++)
                {
                    if (btnDevices[i] != null)
                    {
                        btnDevices[i].BackColor = themeBgColor;
                        btnDevices[i].ForeColor = themeTextColor;
                        btnDevices[i].HoverBgColor = themeHoverBgColor;
                    }
                }
            }
            if (btnMixer != null)
            {
                btnMixer.BackColor = themeBgColor;
                btnMixer.ForeColor = themeTextColor;
                btnMixer.HoverBgColor = themeHoverBgColor;
            }
            if (pnlMixer != null)
            {
                pnlMixer.BackColor = themeBgColor;
            }

            this.Invalidate();
        }

        private void UpdateTimer_Tick(object sender, EventArgs e)
        {
            UpdateThemeColors();
            RefreshAudioState();
            if (isExpanded)
            {
                RefreshMixerSessions();

                IntPtr activeHwnd = GetForegroundWindow();
                if (activeHwnd != IntPtr.Zero && activeHwnd != this.Handle && !IsChild(this.Handle, activeHwnd))
                {
                    bool settingsOpen = false;
                    foreach (Form openForm in Application.OpenForms)
                    {
                        if (openForm != this && openForm.Handle == activeHwnd)
                        {
                            settingsOpen = true;
                            break;
                        }
                    }
                    if (!settingsOpen)
                    {
                        Point clientMousePos = this.PointToClient(Cursor.Position);
                        if (!this.ClientRectangle.Contains(clientMousePos))
                        {
                            ToggleMixer();
                        }
                    }
                }
            }
            UpdatePosition();

            // Periodic Garbage Collection every 60 seconds (120 ticks * 500ms)
            gcCounter++;
            if (gcCounter >= 120)
            {
                gcCounter = 0;
                GC.Collect();
                GC.WaitForPendingFinalizers();
                GC.Collect();
            }
        }

        private void RefreshAudioState()
        {
            IMMDeviceCollection coll = null;
            IMMDevice defaultDev = null;
            object volumeObj = null;
            IAudioEndpointVolume volume = null;

            try
            {
                int hr = enumerator.EnumAudioEndpoints(0, 1, out coll); // 0 = eRender, 1 = DEVICE_STATE_ACTIVE
                if (hr == 0 && coll != null)
                {
                    uint count;
                    coll.GetCount(out count);
                    
                    var allActiveIds = new List<string>();
                    var allActiveNames = new List<string>();

                    for (uint i = 0; i < count; i++)
                    {
                        IMMDevice dev = null;
                        IPropertyStore props = null;
                        try
                        {
                            coll.Item(i, out dev);
                            if (dev != null)
                            {
                                string devId;
                                dev.GetId(out devId);
                                
                                int hrProps = dev.OpenPropertyStore(0, out props);
                                string name = "Audio output";
                                if (hrProps == 0 && props != null)
                                {
                                    var key = new PROPERTYKEY(new Guid("a45c254e-df1c-4efd-8020-67d146a850e0"), 14);
                                    PROPVARIANT pv;
                                    props.GetValue(ref key, out pv);
                                    if (pv.vt == 31)
                                    {
                                        name = Marshal.PtrToStringUni(pv.pointerVal);
                                        PropVariantClear(ref pv);
                                    }
                                }
                                allActiveIds.Add(devId);
                                allActiveNames.Add(name);
                            }
                        }
                        finally
                        {
                            SafeRelease(props);
                            SafeRelease(dev);
                        }
                    }

                    // Filter based on settings
                    var selectedIds = new List<string>(settings.DisplayDevices.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries));
                    var finalIds = new List<string>();
                    var finalNames = new List<string>();

                    for (int i = 0; i < allActiveIds.Count; i++)
                    {
                        if (selectedIds.Contains(allActiveIds[i]))
                        {
                            finalIds.Add(allActiveIds[i]);
                            finalNames.Add(allActiveNames[i]);
                        }
                    }

                    // Fallback to first 3 active devices if the filtered list is empty
                    if (finalIds.Count == 0)
                    {
                        for (int i = 0; i < Math.Min(3, allActiveIds.Count); i++)
                        {
                            finalIds.Add(allActiveIds[i]);
                            finalNames.Add(allActiveNames[i]);
                        }
                    }

                    int visibleCount = Math.Min(3, finalIds.Count);
                    string[] newIds = new string[visibleCount];
                    string[] newNames = new string[visibleCount];

                    for (int i = 0; i < visibleCount; i++)
                    {
                        newIds[i] = finalIds[i];
                        newNames[i] = finalNames[i];
                    }

                    hr = enumerator.GetDefaultAudioEndpoint(0, 0, out defaultDev); // 0 = eRender, 0 = eConsole
                    if (hr == 0 && defaultDev != null)
                    {
                        defaultDev.GetId(out currentDefaultId);

                        Guid iid = new Guid("5CDF2C82-841E-4546-9722-0CF74078229A");
                        defaultDev.Activate(ref iid, 1, IntPtr.Zero, out volumeObj);
                        volume = volumeObj as IAudioEndpointVolume;
                        if (volume != null)
                        {
                            float level;
                            volume.GetMasterVolumeLevelScalar(out level);
                            bool mute;
                            volume.GetMute(out mute);

                            if (!sliderVolume.IsDragging)
                            {
                                sliderVolume.UpdateValue(level);
                            }

                            lblVolumeText.Text = string.Format("{0:0}%", level * 100);

                            if (mute)
                            {
                                btnMute.Glyph = "\uE74F"; // Mute
                            }
                            else
                            {
                                if (level < 0.33f) btnMute.Glyph = "\uE993"; // Volume low
                                else if (level < 0.66f) btnMute.Glyph = "\uE994"; // Volume medium
                                else btnMute.Glyph = "\uE767"; // Volume high
                            }
                        }
                    }

                    bool changed = (newIds.Length != activeDeviceIds.Length);
                    if (!changed)
                    {
                        for (int i = 0; i < newIds.Length; i++)
                        {
                            if (newIds[i] != activeDeviceIds[i])
                            {
                                changed = true;
                                break;
                            }
                        }
                    }

                    if (changed || true)
                    {
                        activeDeviceIds = newIds;
                        for (int i = 0; i < 3; i++)
                        {
                            if (i < visibleCount)
                            {
                                string devId = activeDeviceIds[i];
                                string name = newNames[i];
                                btnDevices[i].Visible = true;
                                btnDevices[i].IsActive = (devId == currentDefaultId);
                                toolTip.SetToolTip(btnDevices[i], name);

                                string lowerName = name.ToLower();
                                if (lowerName.Contains("headphone") || lowerName.Contains("headset") || lowerName.Contains("earphone") || lowerName.Contains("klapid"))
                                {
                                    btnDevices[i].Glyph = "\uE7F6"; // Headphones
                                }
                                else if (lowerName.Contains("hdmi") || lowerName.Contains("tv") || lowerName.Contains("monitor") || lowerName.Contains("displayport") || lowerName.Contains("display"))
                                {
                                    btnDevices[i].Glyph = "\uE7F4"; // TV/Monitor
                                }
                                else if (lowerName.Contains("bluetooth"))
                                {
                                    btnDevices[i].Glyph = "\uE702"; // Bluetooth
                                }
                                else
                                {
                                    btnDevices[i].Glyph = "\uE767"; // Speaker
                                }
                                btnDevices[i].Invalidate();
                            }
                            else
                            {
                                btnDevices[i].Visible = false;
                            }
                        }

                        if (changed)
                        {
                            UpdateLayout();
                        }
                    }
                }
            }
            catch { }
            finally
            {
                SafeRelease(volume);
                SafeRelease(volumeObj);
                SafeRelease(defaultDev);
                SafeRelease(coll);
            }
        }

        private void UpdateLayout()
        {
            int baseY = this.Height - 36;
            int currentX = 8;
            int visibleCount = 0;

            for (int i = 0; i < 3; i++)
            {
                if (btnDevices[i].Visible)
                {
                    btnDevices[i].Location = new Point(currentX, baseY + 4);
                    currentX += 28 + 4;
                    visibleCount++;
                }
            }

            if (visibleCount > 0)
            {
                currentX -= 4;
            }

            currentX += 8;
            separatorX = currentX;

            currentX += 1 + 8;
            btnMute.Location = new Point(currentX, baseY + 4);

            currentX += 28 + 4;
            sliderVolume.Location = new Point(currentX, baseY + 8);

            currentX += 70 + 4;
            lblVolumeText.Location = new Point(currentX, baseY + 10);

            currentX += 32 + 4;
            if (btnMixer != null)
            {
                btnMixer.Location = new Point(currentX, baseY + 4);
                currentX += 28 + 4;
            }

            int totalWidth = currentX + 4;
            if (this.Width != totalWidth)
            {
                this.Width = totalWidth;
                this.Invalidate();
                UpdatePosition();
            }

            if (pnlMixer != null)
            {
                pnlMixer.Location = new Point(4, 4);
                pnlMixer.Size = new Size(totalWidth - 8, Math.Max(10, baseY - 8));
                try {
                    System.IO.File.AppendAllText(
                        System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "mixerlog.txt"),
                        string.Format("Layout: FormHeight={0}, baseY={1}, pnlMixerLoc={2}, pnlMixerSize={3}, btnMuteLoc={4}, btnMixerLoc={5}\n",
                            this.Height, baseY, pnlMixer.Location, pnlMixer.Size, btnMute.Location, btnMixer.Location)
                    );
                } catch {}
            }
        }

        private void UpdatePosition()
        {
            // Restore window if minimized or hidden (e.g. by Win+D / Show Desktop)
            if (this.WindowState == FormWindowState.Minimized)
            {
                if (isExpanded)
                {
                    isExpanded = false;
                    pnlMixer.Visible = false;
                    this.Height = 36;
                    UpdateLayout();
                    ClearMixerRows();
                }
                this.WindowState = FormWindowState.Normal;
            }
            if (!this.Visible)
            {
                this.Visible = true;
            }

            Screen scr = null;
            if (!string.IsNullOrEmpty(settings.ScreenDeviceName))
            {
                foreach (var s in Screen.AllScreens)
                {
                    if (s.DeviceName == settings.ScreenDeviceName)
                    {
                        scr = s;
                        break;
                    }
                }
            }
            if (scr == null)
            {
                int scrIdx = Math.Max(0, Math.Min(Screen.AllScreens.Length - 1, settings.ScreenIndex));
                scr = Screen.AllScreens[scrIdx];
            }

            bool isMovedToSecond = false;
            if (settings.MoveOnFullscreen && Screen.AllScreens.Length > 1)
            {
                bool processIsStillActive = false;
                if (activeFullscreenProcessId != 0)
                {
                    processIsStillActive = IsProcessRunning(activeFullscreenProcessId);
                }

                Screen gameScreen = null;
                if (processIsStillActive && !string.IsNullOrEmpty(activeFullscreenScreenDeviceName))
                {
                    foreach (var s in Screen.AllScreens)
                    {
                        if (s.DeviceName == activeFullscreenScreenDeviceName)
                        {
                            gameScreen = s;
                            break;
                        }
                    }
                }

                if (gameScreen == null)
                {
                    uint newProcId;
                    if (IsForegroundWindowFullscreen(out gameScreen, out newProcId))
                    {
                        activeFullscreenProcessId = newProcId;
                        activeFullscreenScreenDeviceName = gameScreen != null ? gameScreen.DeviceName : null;
                    }
                    else
                    {
                        activeFullscreenProcessId = 0;
                        activeFullscreenScreenDeviceName = null;
                    }
                }

                if (gameScreen != null)
                {
                    Screen targetScr = null;
                    foreach (var s in Screen.AllScreens)
                    {
                        if (s.Bounds.Left > gameScreen.Bounds.Left)
                        {
                            if (targetScr == null || s.Bounds.Left < targetScr.Bounds.Left)
                            {
                                targetScr = s;
                            }
                        }
                    }
                    if (targetScr == null)
                    {
                        foreach (var s in Screen.AllScreens)
                        {
                            if (s.Bounds.Left != gameScreen.Bounds.Left)
                            {
                                targetScr = s;
                                break;
                            }
                        }
                    }
                    if (targetScr != null)
                    {
                        scr = targetScr;
                        isMovedToSecond = true;
                    }
                }
            }

            var bounds = scr.Bounds;

            IntPtr trayHwnd = FindWindow("Shell_TrayWnd", null);
            RECT rectTaskbar = new RECT();
            bool foundTaskbar = false;

            if (scr.Primary && trayHwnd != IntPtr.Zero)
            {
                GetWindowRect(trayHwnd, out rectTaskbar);
                foundTaskbar = true;
            }

            int taskbarTop = bounds.Bottom - 48;
            int taskbarHeight = 48;

            if (foundTaskbar)
            {
                taskbarTop = rectTaskbar.Top;
                taskbarHeight = rectTaskbar.Bottom - rectTaskbar.Top;
            }

            int collapsedHeight = 36;
            int targetTop = taskbarTop + (taskbarHeight - collapsedHeight) / 2;
            if (isExpanded)
            {
                targetTop = targetTop + collapsedHeight - this.Height;
            }

            int targetLeft = 0;
            string currentAlignment = isMovedToSecond ? "Left" : settings.Alignment;

            if (currentAlignment == "Left")
            {
                if (scr.Primary)
                    targetLeft = bounds.Left + 84;
                else
                    targetLeft = bounds.Left + 12;
            }
            else // "Right"
            {
                if (scr.Primary && trayHwnd != IntPtr.Zero)
                {
                    IntPtr notifyHwnd = FindWindowEx(trayHwnd, IntPtr.Zero, "TrayNotifyWnd", null);
                    RECT rectTray;
                    if (notifyHwnd != IntPtr.Zero && GetWindowRect(notifyHwnd, out rectTray))
                    {
                        targetLeft = rectTray.Left - this.Width - 12;
                    }
                    else
                    {
                        targetLeft = bounds.Right - this.Width - 200;
                    }
                }
                else
                {
                    targetLeft = bounds.Right - this.Width - 16;
                }
            }

            try {
                System.IO.File.AppendAllText(
                    System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "mixerlog.txt"),
                    string.Format("UpdatePosition: isExpanded={0}, Height={1}, targetLeft={2}, targetTop={3}, currentLeft={4}, currentTop={5}\n",
                        isExpanded, this.Height, targetLeft, targetTop, this.Left, this.Top)
                );
            } catch {}

            // Update managed bounds first so WinForms doesn't fight back!
            if (this.Left != targetLeft || this.Top != targetTop)
            {
                this.Bounds = new Rectangle(targetLeft, targetTop, this.Width, this.Height);
            }

            // Set Z-order (topmost) natively
            IntPtr topmostFlag = settings.AlwaysOnTop ? HWND_TOPMOST : HWND_NOTOPMOST;
            SetWindowPos(this.Handle, topmostFlag, 0, 0, 0, 0, SWP_NOMOVE | SWP_NOSIZE | SWP_NOACTIVATE | SWP_SHOWWINDOW);
        }

        private void SwitchDevice(int index)
        {
            if (index >= 0 && index < activeDeviceIds.Length)
            {
                string id = activeDeviceIds[index];
                try
                {
                    policyConfig.SetDefaultEndpoint(id, 0); // eConsole
                    policyConfig.SetDefaultEndpoint(id, 1); // eMultimedia
                    policyConfig.SetDefaultEndpoint(id, 2); // eCommunications
                    currentDefaultId = id;
                    RefreshAudioState();
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Switch error: " + ex.Message);
                }
            }
        }

        private void SliderVolume_ValueChanged(object sender, EventArgs e)
        {
            SetSystemVolume(sliderVolume.Value);
        }

        private void SetSystemVolume(float val)
        {
            IMMDevice defaultDev = null;
            object volumeObj = null;
            IAudioEndpointVolume volume = null;
            try
            {
                int hr = enumerator.GetDefaultAudioEndpoint(0, 0, out defaultDev);
                if (hr == 0 && defaultDev != null)
                {
                    Guid iid = new Guid("5CDF2C82-841E-4546-9722-0CF74078229A");
                    defaultDev.Activate(ref iid, 1, IntPtr.Zero, out volumeObj);
                    volume = volumeObj as IAudioEndpointVolume;
                    if (volume != null)
                    {
                        Guid eventContext = Guid.Empty;
                        volume.SetMasterVolumeLevelScalar(val, ref eventContext);
                        lblVolumeText.Text = string.Format("{0:0}%", val * 100);
                    }
                }
            }
            catch { }
            finally
            {
                SafeRelease(volume);
                SafeRelease(volumeObj);
                SafeRelease(defaultDev);
            }
        }

        private void BtnMute_Click(object sender, EventArgs e)
        {
            IMMDevice defaultDev = null;
            object volumeObj = null;
            IAudioEndpointVolume volume = null;
            try
            {
                int hr = enumerator.GetDefaultAudioEndpoint(0, 0, out defaultDev);
                if (hr == 0 && defaultDev != null)
                {
                    Guid iid = new Guid("5CDF2C82-841E-4546-9722-0CF74078229A");
                    defaultDev.Activate(ref iid, 1, IntPtr.Zero, out volumeObj);
                    volume = volumeObj as IAudioEndpointVolume;
                    if (volume != null)
                    {
                        bool currentMute;
                        volume.GetMute(out currentMute);
                        Guid eventContext = Guid.Empty;
                        volume.SetMute(!currentMute, ref eventContext);
                        RefreshAudioState();
                    }
                }
            }
            catch { }
            finally
            {
                SafeRelease(volume);
                SafeRelease(volumeObj);
                SafeRelease(defaultDev);
            }
        }

        private void Form_MouseWheel(object sender, MouseEventArgs e)
        {
            IMMDevice defaultDev = null;
            object volumeObj = null;
            IAudioEndpointVolume volume = null;
            try
            {
                int hr = enumerator.GetDefaultAudioEndpoint(0, 0, out defaultDev);
                if (hr == 0 && defaultDev != null)
                {
                    Guid iid = new Guid("5CDF2C82-841E-4546-9722-0CF74078229A");
                    defaultDev.Activate(ref iid, 1, IntPtr.Zero, out volumeObj);
                    volume = volumeObj as IAudioEndpointVolume;
                    if (volume != null)
                    {
                        float currentVal;
                        volume.GetMasterVolumeLevelScalar(out currentVal);

                        float step = (e.Delta > 0) ? 0.02f : -0.02f;
                        float newVal = Math.Max(0.0f, Math.Min(1.0f, currentVal + step));

                        Guid eventContext = Guid.Empty;
                        volume.SetMasterVolumeLevelScalar(newVal, ref eventContext);
                        
                        sliderVolume.UpdateValue(newVal);
                        lblVolumeText.Text = string.Format("{0:0}%", newVal * 100);
                    }
                }
            }
            catch { }
            finally
            {
                SafeRelease(volume);
                SafeRelease(volumeObj);
                SafeRelease(defaultDev);
            }
        }

        private void ToggleMixer()
        {
            isExpanded = !isExpanded;

            if (isExpanded)
            {
                try { System.IO.File.WriteAllText(System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "mixerlog.txt"), "--- Mixer Opened ---\n"); } catch {}
                pnlMixer.Visible = true;
                RefreshMixerSessions();
            }
            else
            {
                try { System.IO.File.WriteAllText(System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "mixerlog.txt"), "--- Mixer Closed ---\n"); } catch {}
                pnlMixer.Visible = false;
                this.Height = 36;
                UpdateLayout();
                UpdatePosition();
                ClearMixerRows();

                // Immediately sweep memory when mixer closes
                GC.Collect();
                GC.WaitForPendingFinalizers();
                GC.Collect();
            }
        }

        private void ClearMixerRows()
        {
            foreach (var row in mixerRows)
            {
                if (row.RowPanel != null)
                {
                    pnlMixer.Controls.Remove(row.RowPanel);
                    row.RowPanel.Dispose();
                }
                if (row.IconBox != null) row.IconBox.Dispose();
                if (row.IconLabel != null) row.IconLabel.Dispose();
                if (row.NameLabel != null) row.NameLabel.Dispose();
                if (row.Slider != null) row.Slider.Dispose();
                if (row.VolLabel != null) row.VolLabel.Dispose();
            }
            mixerRows.Clear();
        }

        private void RefreshMixerSessions()
        {
            if (!isExpanded) return;

            try { System.IO.File.AppendAllText(System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "mixerlog.txt"), "--- Refresh Sessions Start ---\n"); } catch {}

            IMMDevice defaultDev = null;
            IAudioSessionManager2 manager = null;
            IAudioSessionEnumerator sessionEnum = null;
            object volumeObj = null;
            var activeSessions = new List<SessionData>();
            int count = -1;

            try
            {
                int hr = enumerator.GetDefaultAudioEndpoint(0, 0, out defaultDev);
                int hrActivate = -1;
                if (hr == 0 && defaultDev != null)
                {
                    Guid iid = new Guid("77AA99A0-1BD6-484F-8BC7-2C654C9A9B6F");
                    hrActivate = defaultDev.Activate(ref iid, 1, IntPtr.Zero, out volumeObj);
                    manager = volumeObj as IAudioSessionManager2;
                }

                if (hr == 0 && defaultDev != null && manager != null && manager.GetSessionEnumerator(out sessionEnum) == 0 && sessionEnum != null)
                {
                    if (sessionEnum.GetCount(out count) == 0)
                    {
                        for (int i = 0; i < count; i++)
                        {
                            IAudioSessionControl sessionCtrl = null;
                            try
                            {
                                int hrGet = sessionEnum.GetSession(i, out sessionCtrl);
                                if (hrGet != 0 || sessionCtrl == null)
                                {
                                    try { System.IO.File.AppendAllText(System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "mixerlog.txt"), string.Format("GetSession {0} failed: hr=0x{1:X8}, sessionCtrl is null={2}\n", i, hrGet, sessionCtrl == null)); } catch {}
                                }

                                if (hrGet == 0 && sessionCtrl != null)
                                {
                                    int state = 0;
                                    sessionCtrl.GetState(out state);
                                    if (state == 2)
                                    {
                                        try { System.IO.File.AppendAllText(System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "mixerlog.txt"), string.Format("Session {0}: Skipped state={1} (Expired)\n", i, state)); } catch {}
                                        SafeRelease(sessionCtrl);
                                        continue;
                                    }

                                    IAudioSessionControl2 sessionCtrl2 = sessionCtrl as IAudioSessionControl2;
                                    ISimpleAudioVolume simpleVol = sessionCtrl as ISimpleAudioVolume;
                                    
                                    if (sessionCtrl2 == null || simpleVol == null)
                                    {
                                        try { System.IO.File.AppendAllText(System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "mixerlog.txt"), string.Format("Session {0}: sessionCtrl2 is null={1}, simpleVol is null={2}\n", i, sessionCtrl2 == null, simpleVol == null)); } catch {}
                                    }

                                    if (sessionCtrl2 != null && simpleVol != null)
                                    {
                                        int pid = 0;
                                        sessionCtrl2.GetProcessId(out pid);
                                        
                                        IntPtr idPtr = IntPtr.Zero;
                                        string sessionId = null;
                                        int hrId = sessionCtrl2.GetSessionIdentifier(out idPtr);
                                        if (hrId == 0 && idPtr != IntPtr.Zero)
                                        {
                                            sessionId = Marshal.PtrToStringUni(idPtr);
                                            Marshal.FreeCoTaskMem(idPtr);
                                        }
                                        else
                                        {
                                            sessionId = "Session_PID_" + pid + "_" + i;
                                            try { System.IO.File.AppendAllText(System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "mixerlog.txt"), string.Format("Session {0}: GetSessionIdentifier failed hr=0x{1:X8}, using fallback '{2}'\n", i, hrId, sessionId)); } catch {}
                                        }

                                        bool isSystem = (sessionCtrl2.IsSystemSoundsSession() == 0);
                                        
                                        float vol = 1.0f;
                                        simpleVol.GetMasterVolume(out vol);
                                        
                                        bool mute = false;
                                        simpleVol.GetMute(out mute);
                                        
                                        string name;
                                        Icon icon;
                                        GetSessionInfo(pid, isSystem, out name, out icon);

                                        activeSessions.Add(new SessionData
                                        {
                                            SessionId = sessionId,
                                            ProcessId = pid,
                                            IsSystemSounds = isSystem,
                                            Volume = vol,
                                            Mute = mute,
                                            Name = name,
                                            Icon = icon
                                        });
                                    }
                                    SafeRelease(sessionCtrl);
                                }
                            }
                            catch (Exception ex)
                            {
                                SafeRelease(sessionCtrl);
                                try { System.IO.File.AppendAllText(System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "mixerlog.txt"), "Loop error: " + ex.ToString() + "\n"); } catch {}
                            }
                        }
                    }
                }

                try
                {
                    System.IO.File.AppendAllText(System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "mixerlog.txt"),
                        string.Format("Summary: hr=0x{0:X8}, Activate hr=0x{1:X8}, count={2}, active={3}\n", 
                            hr, hrActivate, count, activeSessions.Count));
                    foreach (var s in activeSessions)
                    {
                        System.IO.File.AppendAllText(System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "mixerlog.txt"),
                            string.Format("Session: Name='{0}', PID={1}, System={2}, Vol={3}\n", s.Name, s.ProcessId, s.IsSystemSounds, s.Volume));
                    }
                }
                catch {}
            }
            catch (Exception ex)
            {
                try { System.IO.File.AppendAllText(System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "mixerlog.txt"), "Outer error: " + ex.Message + "\n"); } catch {}
            }
            finally
            {
                SafeRelease(sessionEnum);
                SafeRelease(manager);
                SafeRelease(volumeObj);
                SafeRelease(defaultDev);
            }

            // Adjust form height dynamically based on session count
            int displayCount = activeSessions.Count;
            int calculatedPanelHeight = Math.Max(52, Math.Min(300, displayCount * 36 + 16));
            int newHeight = calculatedPanelHeight + 36;
            
            try {
                System.IO.File.AppendAllText(
                    System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "mixerlog.txt"),
                    string.Format("HeightCheck: CurrentHeight={0}, NewHeight={1}, isExpanded={2}\n", 
                        this.Height, newHeight, isExpanded)
                );
            } catch {}

            if (this.Height != newHeight)
            {
                this.Height = newHeight;
                UpdateLayout();
                UpdatePosition();
            }

            // Check if sessions changed
            bool changed = (activeSessions.Count != mixerRows.Count);
            if (!changed)
            {
                for (int i = 0; i < activeSessions.Count; i++)
                {
                    if (activeSessions[i].SessionId != mixerRows[i].SessionId)
                    {
                        changed = true;
                        break;
                    }
                }
            }

            if (changed)
            {
                ClearMixerRows();
                BuildMixerRows(activeSessions);
            }
            else
            {
                // Just update volumes
                for (int i = 0; i < activeSessions.Count; i++)
                {
                    var row = mixerRows[i];
                    var data = activeSessions[i];
                    if (row.Slider != null && !row.Slider.IsDragging)
                    {
                        row.Slider.UpdateValue(data.Volume);
                        row.VolLabel.Text = string.Format("{0:0}%", data.Volume * 100);
                    }
                }
            }
        }

        private void BuildMixerRows(List<SessionData> sessions)
        {
            int y = 8;
            int rowHeight = 36;
            
            pnlMixer.SuspendLayout();
            
            if (sessions.Count == 0)
            {
                Label lblNoSessions = new Label
                {
                    Location = new Point(10, 16),
                    Size = new Size(pnlMixer.Width - 20, 24),
                    Font = new Font("Segoe UI", 8f, FontStyle.Italic),
                    ForeColor = themeTextColor,
                    Text = "Ühtegi heli tekitavat rakendust ei leitud",
                    TextAlign = ContentAlignment.MiddleCenter,
                    BackColor = themeBgColor
                };
                pnlMixer.Controls.Add(lblNoSessions);
                
                mixerRows.Add(new MixerRow { SessionId = "placeholder", RowPanel = new Panel() });
            }
            else
            {
                foreach (var data in sessions)
                {
                    string sid = data.SessionId;
                    
                    Panel rowPanel = new Panel
                    {
                        Location = new Point(0, y),
                        Size = new Size(pnlMixer.Width - 16, rowHeight),
                        BackColor = themeBgColor
                    };
                    
                    PictureBox iconBox = null;
                    Label iconLabel = null;
                    
                    if (data.Icon != null)
                    {
                        iconBox = new PictureBox
                        {
                            Location = new Point(8, 10),
                            Size = new Size(16, 16),
                            Image = data.Icon.ToBitmap(),
                            SizeMode = PictureBoxSizeMode.StretchImage,
                            BackColor = themeBgColor
                        };
                        rowPanel.Controls.Add(iconBox);
                    }
                    else
                    {
                        iconLabel = new Label
                        {
                            Location = new Point(8, 8),
                            Size = new Size(16, 16),
                            Font = new Font("Segoe MDL2 Assets", 8f),
                            ForeColor = themeTextColor,
                            Text = data.IsSystemSounds ? "\uE767" : "\uE715",
                            TextAlign = ContentAlignment.MiddleCenter,
                            BackColor = themeBgColor
                        };
                        rowPanel.Controls.Add(iconLabel);
                    }
                    
                    Label nameLabel = new Label
                    {
                        Location = new Point(28, 10),
                        Size = new Size(80, 16),
                        Font = new Font("Segoe UI", 8f),
                        ForeColor = themeTextColor,
                        Text = data.Name,
                        TextAlign = ContentAlignment.MiddleLeft,
                        BackColor = themeBgColor
                    };
                    rowPanel.Controls.Add(nameLabel);
                    
                    int sliderWidth = this.Width - 112 - 40;
                    VolumeSlider slider = new VolumeSlider
                    {
                        Location = new Point(112, 14),
                        Size = new Size(sliderWidth, 8),
                        ActiveColor = themeActiveBgColor,
                        InactiveColor = Color.FromArgb(80, 128, 128, 128),
                        BackColor = themeBgColor
                    };
                    slider.UpdateValue(data.Volume);
                    
                    Label volLabel = new Label
                    {
                        Location = new Point(this.Width - 36, 10),
                        Size = new Size(30, 16),
                        Font = new Font("Segoe UI", 8f),
                        ForeColor = themeTextColor,
                        Text = string.Format("{0:0}%", data.Volume * 100),
                        TextAlign = ContentAlignment.MiddleLeft,
                        BackColor = themeBgColor
                    };
                    
                    slider.ValueChanged += (s, ev) => {
                        SetSessionVolume(sid, slider.Value);
                        volLabel.Text = string.Format("{0:0}%", slider.Value * 100);
                    };
                    
                    rowPanel.Controls.Add(slider);
                    rowPanel.Controls.Add(volLabel);

                    rowPanel.MouseWheel += Form_MouseWheel;
                    slider.MouseWheel += Form_MouseWheel;
                    if (iconBox != null) iconBox.MouseWheel += Form_MouseWheel;
                    if (iconLabel != null) iconLabel.MouseWheel += Form_MouseWheel;
                    nameLabel.MouseWheel += Form_MouseWheel;
                    volLabel.MouseWheel += Form_MouseWheel;

                    pnlMixer.Controls.Add(rowPanel);
                    
                    mixerRows.Add(new MixerRow
                    {
                        SessionId = sid,
                        RowPanel = rowPanel,
                        IconBox = iconBox,
                        IconLabel = iconLabel,
                        NameLabel = nameLabel,
                        Slider = slider,
                        VolLabel = volLabel
                    });
                    
                    y += rowHeight;
                }
            }
            
            pnlMixer.ResumeLayout(true);
        }

        private void SetSessionVolume(string sessionId, float volumeScalar)
        {
            IMMDevice defaultDev = null;
            IAudioSessionManager2 manager = null;
            IAudioSessionEnumerator sessionEnum = null;
            try
            {
                int hr = enumerator.GetDefaultAudioEndpoint(0, 0, out defaultDev);
                if (hr == 0 && defaultDev != null)
                {
                    Guid iid = new Guid("77AA99A0-1BD6-484F-8BC7-2C654C9A9B6F");
                    object volumeObj;
                    defaultDev.Activate(ref iid, 1, IntPtr.Zero, out volumeObj);
                    manager = volumeObj as IAudioSessionManager2;
                    if (manager != null && manager.GetSessionEnumerator(out sessionEnum) == 0 && sessionEnum != null)
                    {
                        int count;
                        sessionEnum.GetCount(out count);
                        for (int i = 0; i < count; i++)
                        {
                            IAudioSessionControl sessionCtrl = null;
                            try
                            {
                                if (sessionEnum.GetSession(i, out sessionCtrl) == 0 && sessionCtrl != null)
                                {
                                    IAudioSessionControl2 sessionCtrl2 = sessionCtrl as IAudioSessionControl2;
                                    ISimpleAudioVolume simpleVol = sessionCtrl as ISimpleAudioVolume;
                                    if (sessionCtrl2 != null && simpleVol != null)
                                    {
                                        IntPtr idPtr;
                                        sessionCtrl2.GetSessionIdentifier(out idPtr);
                                        string sid = Marshal.PtrToStringUni(idPtr);
                                        Marshal.FreeCoTaskMem(idPtr);
                                        
                                        if (sid == sessionId)
                                        {
                                            Guid guid = Guid.Empty;
                                            simpleVol.SetMasterVolume(volumeScalar, ref guid);
                                            SafeRelease(sessionCtrl);
                                            break;
                                        }
                                    }
                                    SafeRelease(sessionCtrl);
                                }
                            }
                            catch
                            {
                                SafeRelease(sessionCtrl);
                            }
                        }
                    }
                }
            }
            catch { }
            finally
            {
                SafeRelease(sessionEnum);
                SafeRelease(manager);
                SafeRelease(defaultDev);
            }
        }

        private void GetSessionInfo(int pid, bool isSystem, out string name, out Icon icon)
        {
            name = "Süsteemi helid";
            icon = null;

            if (isSystem || pid == 0)
            {
                return;
            }

            try
            {
                using (var proc = System.Diagnostics.Process.GetProcessById(pid))
                {
                    string path = proc.MainModule.FileName;
                    name = proc.ProcessName;
                    
                    if (name.Length > 0)
                    {
                        name = char.ToUpper(name[0]) + name.Substring(1);
                    }

                    icon = Icon.ExtractAssociatedIcon(path);
                }
            }
            catch
            {
                name = "Rakendus (PID " + pid + ")";
            }
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            var g = e.Graphics;
            g.SmoothingMode = SmoothingMode.None;

            using (var brush = new SolidBrush(themeBgColor))
            {
                g.FillRoundRectangle(brush, 0, 0, Width - 1, Height - 1, 8);
            }

            using (var pen = new Pen(themeBorderColor, 1f))
            {
                g.DrawRoundRectangle(pen, 0, 0, Width - 1, Height - 1, 8);
            }

            int baseY = isExpanded ? 204 : 0;

            if (isExpanded)
            {
                using (var pen = new Pen(themeBorderColor, 1f))
                {
                    g.DrawLine(pen, 0, 204, Width, 204);
                }
            }

            if (separatorX > 0)
            {
                using (var pen = new Pen(themeBorderColor, 1f))
                {
                    g.DrawLine(pen, separatorX, baseY + 8, separatorX, baseY + 28);
                }
            }
        }

        private bool IsStartupEnabled()
        {
            try
            {
                using (var key = Registry.CurrentUser.OpenSubKey(RunRegistryKey))
                {
                    return key != null && key.GetValue(AppName) != null;
                }
            }
            catch { return false; }
        }

        private void SetStartup(bool enable)
        {
            try
            {
                using (var key = Registry.CurrentUser.OpenSubKey(RunRegistryKey, true))
                {
                    if (key != null)
                    {
                        if (enable)
                            key.SetValue(AppName, "\"" + Application.ExecutablePath + "\"");
                        else
                            key.DeleteValue(AppName, false);
                    }
                }
            }
            catch { }
        }

        private void SafeRelease(object obj)
        {
            if (obj != null && Marshal.IsComObject(obj))
            {
                try { Marshal.ReleaseComObject(obj); } catch { }
            }
        }

        [DllImport("ole32.dll")]
        private static extern void PropVariantClear(ref PROPVARIANT pvar);

        [DllImport("user32.dll", SetLastError = true)]
        private static extern IntPtr FindWindow(string lpClassName, string lpWindowName);

        [DllImport("user32.dll", SetLastError = true)]
        private static extern IntPtr FindWindowEx(IntPtr hwndParent, IntPtr hwndChildAfter, string lpszClass, string lpszWindow);

        [StructLayout(LayoutKind.Sequential)]
        private struct RECT
        {
            public int Left;
            public int Top;
            public int Right;
            public int Bottom;
        }

        [DllImport("user32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool GetWindowRect(IntPtr hWnd, out RECT lpRect);

        [DllImport("user32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter, int x, int y, int cx, int cy, uint uFlags);

        [DllImport("user32.dll")]
        private static extern IntPtr GetForegroundWindow();

        [DllImport("user32.dll")]
        private static extern bool IsChild(IntPtr hWndParent, IntPtr hWndChild);

        [DllImport("user32.dll", SetLastError = true, CharSet = CharSet.Auto)]
        private static extern int GetClassName(IntPtr hWnd, System.Text.StringBuilder lpClassName, int nMaxCount);

        private string GetClassNameOfWindow(IntPtr hwnd)
        {
            var sb = new System.Text.StringBuilder(256);
            GetClassName(hwnd, sb, sb.Capacity);
            return sb.ToString();
        }

        [DllImport("user32.dll", SetLastError = true)]
        private static extern uint GetWindowThreadProcessId(IntPtr hWnd, out uint lpdwProcessId);

        private bool IsProcessRunning(uint processId)
        {
            try
            {
                using (var proc = System.Diagnostics.Process.GetProcessById((int)processId))
                {
                    return !proc.HasExited;
                }
            }
            catch
            {
                return false;
            }
        }

        private bool IsForegroundWindowFullscreen(out Screen onScreen, out uint processId)
        {
            onScreen = null;
            processId = 0;
            IntPtr hwnd = GetForegroundWindow();
            if (hwnd == IntPtr.Zero || hwnd == this.Handle) return false;

            // Skip desktop and taskbar windows
            string className = GetClassNameOfWindow(hwnd);
            if (className == "Shell_TrayWnd" || className == "WorkerW" || className == "Progman" || className == "CabinetWClass")
            {
                return false;
            }

            RECT rect;
            if (GetWindowRect(hwnd, out rect))
            {
                // Find which screen this window is on
                foreach (var scr in Screen.AllScreens)
                {
                    // Check if it covers the entire screen bounds with a small tolerance
                    if (rect.Left <= scr.Bounds.Left + 4 &&
                        rect.Top <= scr.Bounds.Top + 4 &&
                        rect.Right >= scr.Bounds.Right - 4 &&
                        rect.Bottom >= scr.Bounds.Bottom - 4)
                    {
                        onScreen = scr;
                        GetWindowThreadProcessId(hwnd, out processId);
                        return true;
                    }
                }
            }
            return false;
        }

        private static readonly IntPtr HWND_TOPMOST = new IntPtr(-1);
        private static readonly IntPtr HWND_NOTOPMOST = new IntPtr(-2);
        private const uint SWP_NOSIZE = 0x0001;
        private const uint SWP_NOMOVE = 0x0002;
        private const uint SWP_NOACTIVATE = 0x0010;
        private const uint SWP_SHOWWINDOW = 0x0040;
    }

    // ==========================================
    // APP SETTINGS PERSISTENCE
    // ==========================================

    class AppSettings
    {
        public string DisplayDevices = "";
        public int ScreenIndex = 0;
        public string ScreenDeviceName = "";
        public string Alignment = "Right";
        public bool AlwaysOnTop = true;
        public bool MoveOnFullscreen = false;

        private static string GetFilePath()
        {
            return System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "settings.txt");
        }

        public static AppSettings Load()
        {
            var s = new AppSettings();
            string path = GetFilePath();
            if (System.IO.File.Exists(path))
            {
                try
                {
                    var lines = System.IO.File.ReadAllLines(path);
                    foreach (var line in lines)
                    {
                        var parts = line.Split(new char[] { '=' }, 2);
                        if (parts.Length == 2)
                        {
                            string key = parts[0].Trim();
                            string val = parts[1].Trim();
                            if (key == "DisplayDevices") s.DisplayDevices = val;
                            else if (key == "ScreenIndex") int.TryParse(val, out s.ScreenIndex);
                            else if (key == "ScreenDeviceName") s.ScreenDeviceName = val;
                            else if (key == "Alignment") s.Alignment = val;
                            else if (key == "AlwaysOnTop") bool.TryParse(val, out s.AlwaysOnTop);
                            else if (key == "MoveOnFullscreen") bool.TryParse(val, out s.MoveOnFullscreen);
                        }
                    }
                }
                catch { }
            }
            return s;
        }

        public void Save()
        {
            try
            {
                var lines = new List<string>();
                lines.Add("DisplayDevices=" + DisplayDevices);
                lines.Add("ScreenIndex=" + ScreenIndex);
                lines.Add("ScreenDeviceName=" + ScreenDeviceName);
                lines.Add("Alignment=" + Alignment);
                lines.Add("AlwaysOnTop=" + AlwaysOnTop);
                lines.Add("MoveOnFullscreen=" + MoveOnFullscreen);
                System.IO.File.WriteAllLines(GetFilePath(), lines.ToArray());
            }
            catch { }
        }
    }

    // ==========================================
    // CONFIGURATION DIALOG
    // ==========================================

    class SettingsForm : Form
    {
        private AppSettings settings;
        private IMMDeviceEnumerator enumerator;
        private bool isDarkMode;

        // UI Controls
        private Panel pnlDevices;
        private ComboBox cmbScreen;
        private ComboBox cmbAlignment;
        private CheckBox cbAlwaysOnTop;
        private CheckBox cbMoveOnFullscreen;
        private Button btnSave;
        private Button btnCancel;
        
        private List<CheckBox> deviceCheckBoxes;

        [DllImport("ole32.dll")]
        private static extern void PropVariantClear(ref PROPVARIANT pvar);

        public SettingsForm(AppSettings settings, IMMDeviceEnumerator enumerator, bool isDarkMode)
        {
            this.settings = settings;
            this.enumerator = enumerator;
            this.isDarkMode = isDarkMode;
            this.deviceCheckBoxes = new List<CheckBox>();

            // Setup Window
            this.Text = "Settings - Taskbar Audio Switcher";
            this.Size = new Size(380, 520);
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.StartPosition = FormStartPosition.CenterScreen;
            this.ShowInTaskbar = true;

            Color bgColor = isDarkMode ? Color.FromArgb(32, 32, 32) : Color.FromArgb(240, 240, 240);
            Color textColor = isDarkMode ? Color.FromArgb(235, 235, 235) : Color.FromArgb(40, 40, 40);
            Color controlBg = isDarkMode ? Color.FromArgb(45, 45, 45) : Color.White;
            Color btnBg = isDarkMode ? Color.FromArgb(60, 60, 60) : Color.FromArgb(225, 225, 225);

            this.BackColor = bgColor;
            this.ForeColor = textColor;

            // Title Label for Devices
            Label lblDevices = new Label
            {
                Text = "Select audio devices to display (up to 3):",
                Location = new Point(20, 15),
                Size = new Size(340, 20),
                Font = new Font("Segoe UI", 9f, FontStyle.Bold)
            };
            this.Controls.Add(lblDevices);

            // Devices Panel
            pnlDevices = new Panel
            {
                Location = new Point(20, 40),
                Size = new Size(325, 160),
                BorderStyle = BorderStyle.FixedSingle,
                BackColor = controlBg,
                AutoScroll = true
            };
            this.Controls.Add(pnlDevices);

            // Populate Devices
            PopulateDevices(textColor);

            // Monitor Label & Dropdown
            Label lblScreen = new Label
            {
                Text = "Display on taskbar screen:",
                Location = new Point(20, 215),
                Size = new Size(325, 20),
                Font = new Font("Segoe UI", 9f, FontStyle.Bold)
            };
            this.Controls.Add(lblScreen);

            cmbScreen = new ComboBox
            {
                Location = new Point(20, 235),
                Size = new Size(325, 24),
                DropDownStyle = ComboBoxStyle.DropDownList,
                BackColor = controlBg,
                ForeColor = textColor,
                FlatStyle = FlatStyle.Flat
            };
            this.Controls.Add(cmbScreen);

            var screens = Screen.AllScreens;
            int selectedIdx = 0;
            for (int i = 0; i < screens.Length; i++)
            {
                string suffix = screens[i].Primary ? " (Primary)" : "";
                cmbScreen.Items.Add(string.Format("Screen {0}{1} - {2}x{3}", i + 1, suffix, screens[i].Bounds.Width, screens[i].Bounds.Height));
                if (!string.IsNullOrEmpty(settings.ScreenDeviceName) && screens[i].DeviceName == settings.ScreenDeviceName)
                {
                    selectedIdx = i;
                }
            }
            if (selectedIdx == 0 && settings.ScreenIndex >= 0 && settings.ScreenIndex < screens.Length)
            {
                selectedIdx = settings.ScreenIndex;
            }
            cmbScreen.SelectedIndex = selectedIdx;

            // Alignment Label & Dropdown
            Label lblAlignment = new Label
            {
                Text = "Taskbar alignment:",
                Location = new Point(20, 275),
                Size = new Size(325, 20),
                Font = new Font("Segoe UI", 9f, FontStyle.Bold)
            };
            this.Controls.Add(lblAlignment);

            cmbAlignment = new ComboBox
            {
                Location = new Point(20, 295),
                Size = new Size(325, 24),
                DropDownStyle = ComboBoxStyle.DropDownList,
                BackColor = controlBg,
                ForeColor = textColor,
                FlatStyle = FlatStyle.Flat
            };
            cmbAlignment.Items.Add("Next to clock (Right)");
            cmbAlignment.Items.Add("Next to Start button (Left)");
            this.Controls.Add(cmbAlignment);

            if (settings.Alignment == "Left")
                cmbAlignment.SelectedIndex = 1;
            else
                cmbAlignment.SelectedIndex = 0;

            // Always on Top Checkbox
            cbAlwaysOnTop = new CheckBox
            {
                Text = "Always on Top (even over fullscreen)",
                Location = new Point(20, 330),
                Size = new Size(325, 24),
                Checked = settings.AlwaysOnTop,
                Font = new Font("Segoe UI", 9f, FontStyle.Bold),
                FlatStyle = FlatStyle.Flat
            };
            this.Controls.Add(cbAlwaysOnTop);

            // Move on Fullscreen Checkbox
            cbMoveOnFullscreen = new CheckBox
            {
                Text = "Move to second screen on game launch",
                Location = new Point(20, 360),
                Size = new Size(325, 24),
                Checked = settings.MoveOnFullscreen,
                Font = new Font("Segoe UI", 9f, FontStyle.Bold),
                FlatStyle = FlatStyle.Flat
            };
            this.Controls.Add(cbMoveOnFullscreen);

            // Save button
            btnSave = new Button
            {
                Text = "Save",
                Location = new Point(155, 425),
                Size = new Size(90, 30),
                BackColor = Color.FromArgb(0, 120, 215),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat
            };
            btnSave.FlatAppearance.BorderSize = 0;
            btnSave.Click += BtnSave_Click;
            this.Controls.Add(btnSave);

            // Cancel button
            btnCancel = new Button
            {
                Text = "Cancel",
                Location = new Point(255, 425),
                Size = new Size(90, 30),
                BackColor = btnBg,
                ForeColor = textColor,
                FlatStyle = FlatStyle.Flat
            };
            btnCancel.FlatAppearance.BorderSize = 0;
            btnCancel.Click += (s, e) => { this.DialogResult = DialogResult.Cancel; this.Close(); };
            this.Controls.Add(btnCancel);
        }

        private void PopulateDevices(Color textColor)
        {
            var selectedIds = new List<string>(settings.DisplayDevices.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries));

            try
            {
                IMMDeviceCollection coll;
                int hr = enumerator.EnumAudioEndpoints(0, 1, out coll); // 0 = eRender, 1 = DEVICE_STATE_ACTIVE
                if (hr == 0 && coll != null)
                {
                    uint count;
                    coll.GetCount(out count);
                    int y = 5;

                    for (uint i = 0; i < count; i++)
                    {
                        IMMDevice dev;
                        coll.Item(i, out dev);
                        if (dev != null)
                        {
                            string devId;
                            dev.GetId(out devId);
                            
                            IPropertyStore props;
                            dev.OpenPropertyStore(0, out props);
                            string name = "Audio output";
                            if (props != null)
                            {
                                var key = new PROPERTYKEY(new Guid("a45c254e-df1c-4efd-8020-67d146a850e0"), 14);
                                PROPVARIANT pv;
                                props.GetValue(ref key, out pv);
                                if (pv.vt == 31)
                                {
                                    name = Marshal.PtrToStringUni(pv.pointerVal);
                                    PropVariantClear(ref pv);
                                }
                                Marshal.ReleaseComObject(props);
                            }

                            CheckBox cb = new CheckBox
                            {
                                Text = name,
                                Tag = devId,
                                Location = new Point(10, y),
                                Size = new Size(280, 24),
                                Checked = selectedIds.Contains(devId),
                                ForeColor = textColor,
                                FlatStyle = FlatStyle.Flat
                            };
                            pnlDevices.Controls.Add(cb);
                            deviceCheckBoxes.Add(cb);
                            y += 28;

                            Marshal.ReleaseComObject(dev);
                        }
                    }
                    Marshal.ReleaseComObject(coll);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error reading devices: " + ex.Message);
            }
        }

        private void BtnSave_Click(object sender, EventArgs e)
        {
            // Collect checked device IDs
            var checkedIds = new List<string>();
            foreach (var cb in deviceCheckBoxes)
            {
                if (cb.Checked)
                {
                    checkedIds.Add(cb.Tag.ToString());
                }
            }

            settings.DisplayDevices = string.Join(",", checkedIds.ToArray());
            settings.ScreenIndex = cmbScreen.SelectedIndex;
            if (cmbScreen.SelectedIndex >= 0 && cmbScreen.SelectedIndex < Screen.AllScreens.Length)
            {
                settings.ScreenDeviceName = Screen.AllScreens[cmbScreen.SelectedIndex].DeviceName;
            }
            settings.Alignment = cmbAlignment.SelectedIndex == 1 ? "Left" : "Right";
            settings.AlwaysOnTop = cbAlwaysOnTop.Checked;
            settings.MoveOnFullscreen = cbMoveOnFullscreen.Checked;

            settings.Save();
            this.DialogResult = DialogResult.OK;
            this.Close();
        }
    }

    static class Program
    {
        [DllImport("user32.dll")]
        private static extern bool SetProcessDPIAware();

        [STAThread]
        static void Main()
        {
            try { SetProcessDPIAware(); } catch { }

            try
            {
                Application.EnableVisualStyles();
                Application.SetCompatibleTextRenderingDefault(false);
                Application.Run(new AudioWidgetForm());
            }
            catch (Exception ex)
            {
                try
                {
                    System.IO.File.WriteAllText(
                        System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "crashlog.txt"),
                        ex.ToString()
                    );
                }
                catch { }
                MessageBox.Show(ex.ToString(), "Application Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }

    // ==========================================
    // WASAPI SESSION COM INTERFACES
    // ==========================================

    [ComImport]
    [Guid("77AA99A0-1BD6-484F-8BC7-2C654C9A9B6F")]
    [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    interface IAudioSessionManager2
    {
        int GetAudioSessionControl(IntPtr AudioSessionGuid, uint StreamFlags, out object SessionControl);
        int GetSimpleAudioVolume(IntPtr AudioSessionGuid, uint StreamFlags, out object AudioVolume);
        int GetSessionEnumerator(out IAudioSessionEnumerator SessionEnum);
        int RegisterSessionNotification(IntPtr SessionNotification);
        int UnregisterSessionNotification(IntPtr SessionNotification);
        int RegisterDuckNotification(IntPtr SessionID, IntPtr DuckNotification);
        int UnregisterDuckNotification(IntPtr DuckNotification);
    }

    [ComImport]
    [Guid("E2F5BB11-0570-40CA-ACDD-3AA01277DEE8")]
    [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    interface IAudioSessionEnumerator
    {
        [PreserveSig] int GetCount(out int SessionCount);
        [PreserveSig] int GetSession(int SessionIndex, out IAudioSessionControl Session);
    }

    [ComImport]
    [Guid("F4B1A599-7266-4319-A8CA-E70ACB11E8CD")]
    [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    interface IAudioSessionControl
    {
        [PreserveSig] int GetState(out int State);
        [PreserveSig] int GetDisplayName([MarshalAs(UnmanagedType.LPWStr)] out string DisplayName);
        [PreserveSig] int SetDisplayName([MarshalAs(UnmanagedType.LPWStr)] string DisplayName, ref Guid EventContext);
        [PreserveSig] int GetIconPath([MarshalAs(UnmanagedType.LPWStr)] out string IconPath);
        [PreserveSig] int SetIconPath([MarshalAs(UnmanagedType.LPWStr)] string IconPath, ref Guid EventContext);
        [PreserveSig] int GetGroupingParam(out Guid GroupingParam);
        [PreserveSig] int SetGroupingParam(ref Guid GroupingParam, ref Guid EventContext);
        [PreserveSig] int RegisterAudioSessionNotification(IntPtr SessionNotification);
        [PreserveSig] int UnregisterAudioSessionNotification(IntPtr SessionNotification);
    }

    [ComImport]
    [Guid("bfb7ff88-7239-4fc9-8fa2-07c950be9c6d")]
    [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    interface IAudioSessionControl2
    {
        [PreserveSig] int GetState(out int State);
        [PreserveSig] int GetDisplayName([MarshalAs(UnmanagedType.LPWStr)] out string DisplayName);
        [PreserveSig] int SetDisplayName([MarshalAs(UnmanagedType.LPWStr)] string DisplayName, ref Guid EventContext);
        [PreserveSig] int GetIconPath([MarshalAs(UnmanagedType.LPWStr)] out string IconPath);
        [PreserveSig] int SetIconPath([MarshalAs(UnmanagedType.LPWStr)] string IconPath, ref Guid EventContext);
        [PreserveSig] int GetGroupingParam(out Guid GroupingParam);
        [PreserveSig] int SetGroupingParam(ref Guid GroupingParam, ref Guid EventContext);
        [PreserveSig] int RegisterAudioSessionNotification(IntPtr SessionNotification);
        [PreserveSig] int UnregisterAudioSessionNotification(IntPtr SessionNotification);

        [PreserveSig] int GetSessionIdentifier(out IntPtr SessionIdentifier);
        [PreserveSig] int GetSessionInstanceIdentifier(out IntPtr SessionInstanceIdentifier);
        [PreserveSig] int GetProcessId(out int ProcessId);
        [PreserveSig] int IsSystemSoundsSession();
        [PreserveSig] int SetDuckingPreference(bool optOut);
    }

    [ComImport]
    [Guid("87CE5498-68D6-44E5-9215-6DA47EF883D8")]
    [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    interface ISimpleAudioVolume
    {
        [PreserveSig]
        int SetMasterVolume(float fLevel, ref Guid EventContext);

        [PreserveSig]
        int GetMasterVolume(out float pfLevel);

        [PreserveSig]
        int SetMute(bool bMute, ref Guid EventContext);

        [PreserveSig]
        int GetMute(out bool pbMute);
    }
}
