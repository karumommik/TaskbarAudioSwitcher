using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using System.Runtime.InteropServices;
using Microsoft.Win32;
using TaskbarAudioSwitcher.Core;
using TaskbarAudioSwitcher.Native;
using TaskbarAudioSwitcher.Controls;

namespace TaskbarAudioSwitcher.UI
{
    public class AudioWidgetForm : Form
    {
        private IMMDeviceEnumerator enumerator = null!;
        private IPolicyConfig policyConfig = null!;
        private AppSettings settings;
        
        // UI Controls
        private List<IconButton> btnDevices = new List<IconButton>();
        private IconButton btnMute = null!;
        private VolumeSlider sliderVolume = null!;
        private Label lblVolumeText = null!;
        private System.Windows.Forms.Timer updateTimer = null!;
        private ToolTip toolTip = null!;
        private NotifyIcon notifyIcon = null!;

        // Mixer panel variables
        private bool isExpanded = false;
        private Panel pnlMixer = null!;
        private IconButton btnMixer = null!;
        private IconButton btnScreenMove = null!;
        private List<MixerRow> mixerRows = new List<MixerRow>();
        private int gcCounter = 0;
        private int themeColorsTickCounter = 0;
        private int audioStateTickCounter = 0;
        private uint activeFullscreenProcessId = 0;
        private string? activeFullscreenScreenDeviceName = null;
        private class ProcessCacheItem
        {
            public string Name { get; set; } = string.Empty;
            public Icon? Icon { get; set; }
        }
        private Dictionary<int, ProcessCacheItem> processCache = new Dictionary<int, ProcessCacheItem>();

        private class MixerRow
        {
            public string SessionId { get; set; } = string.Empty;
            public Panel RowPanel { get; set; } = null!;
            public PictureBox? IconBox { get; set; }
            public Label? IconLabel { get; set; }
            public Label NameLabel { get; set; } = null!;
            public VolumeSlider Slider { get; set; } = null!;
            public Label VolLabel { get; set; } = null!;
        }

        private struct SessionData
        {
            public string SessionId;
            public int ProcessId;
            public bool IsSystemSounds;
            public float Volume;
            public bool Mute;
            public string Name;
            public Icon? Icon;
        }

        // Layout variables
        private int separatorX = 0;
        private string[] activeDeviceIds = Array.Empty<string>();
        private string currentDefaultId = string.Empty;
        
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

            // Auto-migrate ScreenDeviceName if empty and screens exist
            if (string.IsNullOrEmpty(settings.ScreenDeviceName))
            {
                var screens = Screen.AllScreens;
                if (settings.ScreenIndex >= 0 && settings.ScreenIndex < screens.Length)
                {
                    settings.ScreenDeviceName = screens[settings.ScreenIndex].DeviceName;
                    settings.Save();
                }
            }

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

            btnDevices = new List<IconButton>();

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

            btnScreenMove = new IconButton
            {
                Glyph = "\uE7F4", // Monitor symbol
                ForeColor = themeTextColor,
                HoverBgColor = themeHoverBgColor,
                ActiveBgColor = Color.Transparent,
                IsActive = false,
                Visible = settings.ShowScreenMoveButton
            };
            btnScreenMove.MouseDown += BtnScreenMove_MouseDown;
            this.Controls.Add(btnScreenMove);

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
            btnScreenMove.MouseWheel += Form_MouseWheel;
            pnlMixer.MouseWheel += Form_MouseWheel;

            // Auto-collapse mixer when clicking outside (deactivation)
            this.Deactivate += (s, e) => {
                if (isExpanded) ToggleMixer();
            };

            updateTimer = new System.Windows.Forms.Timer { Interval = 100 };
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
            ContextMenuStrip contextMenu = new ContextMenuStrip();
            
            ToolStripMenuItem titleItem = new ToolStripMenuItem("Taskbar Audio Switcher");
            titleItem.Enabled = false;
            contextMenu.Items.Add(titleItem);

            ToolStripMenuItem startupItem = new ToolStripMenuItem("Run at Windows Startup", null, (s, e) => {
                ToolStripMenuItem mi = (ToolStripMenuItem)s!;
                mi.Checked = !mi.Checked;
                SetStartup(mi.Checked);
            });
            startupItem.Checked = IsStartupEnabled();
            contextMenu.Items.Add(startupItem);

            ToolStripMenuItem settingsItem = new ToolStripMenuItem("Settings", null, (s, e) => ShowSettings());
            contextMenu.Items.Add(settingsItem);

            ToolStripMenuItem repositionItem = new ToolStripMenuItem("Reposition Now", null, (s, e) => {
                UpdatePosition();
            });
            contextMenu.Items.Add(repositionItem);

            contextMenu.Items.Add(new ToolStripSeparator());

            ToolStripMenuItem exitItem = new ToolStripMenuItem("Exit", null, (s, e) => {
                notifyIcon.Visible = false;
                Application.Exit();
            });
            contextMenu.Items.Add(exitItem);

            notifyIcon.ContextMenuStrip = contextMenu;
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
                foreach (var btn in btnDevices)
                {
                    if (btn != null)
                    {
                        btn.BackColor = themeBgColor;
                        btn.ForeColor = themeTextColor;
                        btn.HoverBgColor = themeHoverBgColor;
                        btn.ActiveBgColor = themeActiveBgColor;
                    }
                }
            }
            if (btnMixer != null)
            {
                btnMixer.BackColor = themeBgColor;
                btnMixer.ForeColor = themeTextColor;
                btnMixer.HoverBgColor = themeHoverBgColor;
            }
            if (btnScreenMove != null)
            {
                btnScreenMove.BackColor = themeBgColor;
                btnScreenMove.ForeColor = themeTextColor;
                btnScreenMove.HoverBgColor = themeHoverBgColor;
            }
            if (pnlMixer != null)
            {
                pnlMixer.BackColor = themeBgColor;
            }

            this.Invalidate();
        }

        private void UpdateTimer_Tick(object? sender, EventArgs e)
        {
            // 1. Throttled Theme Check (every 2 seconds / 20 ticks of 100ms)
            themeColorsTickCounter++;
            if (themeColorsTickCounter >= 20)
            {
                themeColorsTickCounter = 0;
                UpdateThemeColors();
            }

            // 2. Throttled Audio State Check (every 500ms / 5 ticks of 100ms)
            audioStateTickCounter++;
            if (audioStateTickCounter >= 5)
            {
                audioStateTickCounter = 0;
                RefreshAudioState();
            }

            // 3. Fast Mixer & Position check (every tick / 100ms)
            if (isExpanded)
            {
                // Refresh mixer sessions at same rate as before (every 500ms / 5 ticks of 100ms)
                if (audioStateTickCounter == 0)
                {
                    RefreshMixerSessions();
                }

                IntPtr activeHwnd = Win32.GetForegroundWindow();
                if (activeHwnd != IntPtr.Zero && activeHwnd != this.Handle && !Win32.IsChild(this.Handle, activeHwnd))
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

            // Periodic Garbage Collection every 60 seconds (600 ticks * 100ms)
            gcCounter++;
            if (gcCounter >= 600)
            {
                gcCounter = 0;
                GC.Collect();
                GC.WaitForPendingFinalizers();
                GC.Collect();
            }
        }

        private void RefreshAudioState()
        {
            IMMDeviceCollection? coll = null;
            IMMDevice? defaultDev = null;
            object? volumeObj = null;
            IAudioEndpointVolume? volume = null;

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
                        IMMDevice? dev = null;
                        IPropertyStore? props = null;
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
                                        name = Marshal.PtrToStringUni(pv.pointerVal) ?? "Audio output";
                                        ComNative.PropVariantClear(ref pv);
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
                    var finalIds = new List<string>();
                    var finalNames = new List<string>();

                    if (settings.FilterDevices)
                    {
                        var selectedIds = new List<string>(settings.DisplayDevices.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries));
                        for (int i = 0; i < allActiveIds.Count; i++)
                        {
                            if (selectedIds.Contains(allActiveIds[i]))
                            {
                                finalIds.Add(allActiveIds[i]);
                                finalNames.Add(allActiveNames[i]);
                            }
                        }
                        // Fallback if filter is active but no selected devices are active/connected
                        if (finalIds.Count == 0)
                        {
                            for (int i = 0; i < allActiveIds.Count; i++)
                            {
                                finalIds.Add(allActiveIds[i]);
                                finalNames.Add(allActiveNames[i]);
                            }
                        }
                    }
                    else
                    {
                        for (int i = 0; i < allActiveIds.Count; i++)
                        {
                            finalIds.Add(allActiveIds[i]);
                            finalNames.Add(allActiveNames[i]);
                        }
                    }

                    int visibleCount = finalIds.Count;
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

                        // Ensure we have enough buttons in the list
                        while (btnDevices.Count < visibleCount)
                        {
                            int index = btnDevices.Count;
                            IconButton btn = new IconButton
                            {
                                Glyph = "\uE767",
                                Visible = false,
                                ForeColor = themeTextColor,
                                HoverBgColor = themeHoverBgColor,
                                ActiveBgColor = themeActiveBgColor,
                                ActiveFgColor = Color.White
                            };
                            btn.Click += (s, e) => SwitchDevice(index);
                            btn.MouseWheel += Form_MouseWheel;
                            this.Controls.Add(btn);
                            btnDevices.Add(btn);
                        }

                        // Update buttons
                        for (int i = 0; i < btnDevices.Count; i++)
                        {
                            if (i < visibleCount)
                            {
                                string devId = activeDeviceIds[i];
                                string name = newNames[i];
                                btnDevices[i].Visible = true;
                                btnDevices[i].IsActive = (devId == currentDefaultId);
                                toolTip.SetToolTip(btnDevices[i], name);

                                string abbrev = settings.GetDeviceNickname(devId, name);
                                btnDevices[i].DeviceAbbreviation = abbrev;

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
            float scale = DpiHelper.GetScale(this.Handle);
            int collapsedH = (int)(36 * scale);
            int btnSize = (int)(28 * scale);
            int sliderW = (int)(70 * scale);
            int margin = (int)(4 * scale);
            int padding = (int)(8 * scale);
            int textW = (int)(32 * scale);

            int baseY = this.Height - collapsedH;
            int currentX = padding;
            int visibleCount = 0;

            foreach (var btn in btnDevices)
            {
                if (btn.Visible)
                {
                    btn.Size = new Size(btnSize, btnSize);
                    btn.Location = new Point(currentX, baseY + margin);
                    currentX += btnSize + margin;
                    visibleCount++;
                }
            }

            if (visibleCount > 0)
            {
                currentX -= margin;
            }

            currentX += padding;
            separatorX = currentX;

            currentX += (int)(1 * scale) + padding;
            btnMute.Size = new Size(btnSize, btnSize);
            btnMute.Location = new Point(currentX, baseY + margin);

            currentX += btnSize + margin;
            sliderVolume.Size = new Size(sliderW, (int)(20 * scale));
            sliderVolume.Location = new Point(currentX, baseY + (int)(8 * scale));

            currentX += sliderW + margin;
            lblVolumeText.Font = new Font("Segoe UI", 8f * scale, FontStyle.Regular);
            lblVolumeText.Size = new Size(textW, (int)(16 * scale));
            lblVolumeText.Location = new Point(currentX, baseY + (int)(10 * scale));

            currentX += textW + margin;
            if (btnMixer != null)
            {
                btnMixer.Size = new Size(btnSize, btnSize);
                btnMixer.Location = new Point(currentX, baseY + margin);
                currentX += btnSize + margin;
            }

            if (btnScreenMove != null)
            {
                btnScreenMove.Visible = settings.ShowScreenMoveButton;
                if (btnScreenMove.Visible)
                {
                    btnScreenMove.Size = new Size(btnSize, btnSize);
                    btnScreenMove.Location = new Point(currentX, baseY + margin);
                    currentX += btnSize + margin;
                }
            }

            int totalWidth = currentX + margin;
            if (this.Width != totalWidth)
            {
                this.Width = totalWidth;
                this.Invalidate();
                UpdatePosition();
            }

            if (pnlMixer != null)
            {
                pnlMixer.Location = new Point(margin, margin);
                pnlMixer.Size = new Size(totalWidth - margin * 2, Math.Max(10, baseY - margin * 2));
                try {
                    System.IO.File.AppendAllText(
                        System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "mixerlog.txt"),
                        string.Format("Layout: FormHeight={0}, baseY={1}, pnlMixerLoc={2}, pnlMixerSize={3}, btnMuteLoc={4}, btnMixerLoc={5}\n",
                            this.Height, baseY, pnlMixer?.Location, pnlMixer?.Size, btnMute?.Location, btnMixer?.Location)
                    );
                } catch {}
            }
        }

        private void UpdatePosition()
        {
            float scale = DpiHelper.GetScale(this.Handle);

            // Restore window if minimized or hidden (e.g. by Win+D / Show Desktop)
            if (this.WindowState == FormWindowState.Minimized)
            {
                if (isExpanded)
                {
                    isExpanded = false;
                    pnlMixer.Visible = false;
                    this.Height = (int)(36 * scale);
                    UpdateLayout();
                    ClearMixerRows();
                }
                this.WindowState = FormWindowState.Normal;
            }
            if (!this.Visible)
            {
                this.Visible = true;
            }

            Screen? scr = null;
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

                Screen? gameScreen = null;
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
                        activeFullscreenScreenDeviceName = gameScreen?.DeviceName;
                    }
                    else
                    {
                        activeFullscreenProcessId = 0;
                        activeFullscreenScreenDeviceName = null;
                    }
                }

                if (gameScreen != null)
                {
                    Screen? targetScr = null;
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

            // Use DPI-aware Screen WorkingArea to get taskbar position and height
            int taskbarTop = scr.WorkingArea.Bottom;
            int taskbarHeight = scr.Bounds.Bottom - scr.WorkingArea.Bottom;

            IntPtr trayHwnd = Win32.FindWindow("Shell_TrayWnd", null);

            int collapsedHeight = (int)(36 * scale);
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
                    targetLeft = bounds.Left + (int)(84 * scale);
                else
                    targetLeft = bounds.Left + (int)(12 * scale);
            }
            else // "Right"
            {
                if (scr.Primary && trayHwnd != IntPtr.Zero)
                {
                    IntPtr notifyHwnd = Win32.FindWindowEx(trayHwnd, IntPtr.Zero, "TrayNotifyWnd", null);
                    Win32.RECT rectTray;
                    if (notifyHwnd != IntPtr.Zero && Win32.GetWindowRect(notifyHwnd, out rectTray))
                    {
                        targetLeft = (int)(rectTray.Left / scale) - this.Width - (int)(12 * scale);
                    }
                    else
                    {
                        targetLeft = bounds.Right - this.Width - (int)(200 * scale);
                    }
                }
                else
                {
                    targetLeft = bounds.Right - this.Width - (int)(16 * scale);
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
            IntPtr topmostFlag = settings.AlwaysOnTop ? Win32.HWND_TOPMOST : Win32.HWND_NOTOPMOST;
            Win32.SetWindowPos(this.Handle, topmostFlag, 0, 0, 0, 0, Win32.SWP_NOMOVE | Win32.SWP_NOSIZE | Win32.SWP_NOACTIVATE | Win32.SWP_SHOWWINDOW);
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

        private void SliderVolume_ValueChanged(object? sender, EventArgs e)
        {
            SetSystemVolume(sliderVolume.Value);
        }

        private void SetSystemVolume(float val)
        {
            IMMDevice? defaultDev = null;
            object? volumeObj = null;
            IAudioEndpointVolume? volume = null;
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

        private void BtnMute_Click(object? sender, EventArgs e)
        {
            IMMDevice? defaultDev = null;
            object? volumeObj = null;
            IAudioEndpointVolume? volume = null;
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

        private void Form_MouseWheel(object? sender, MouseEventArgs e)
        {
            var hme = e as HandledMouseEventArgs;
            if (hme != null && hme.Handled) return;

            Control? senderControl = sender as Control;
            MixerRow? matchedRow = null;
            if (senderControl != null && isExpanded)
            {
                foreach (var row in mixerRows)
                {
                    if (senderControl == row.RowPanel || 
                        senderControl == row.Slider || 
                        senderControl == row.IconBox || 
                        senderControl == row.IconLabel || 
                        senderControl == row.NameLabel || 
                        senderControl == row.VolLabel)
                    {
                        matchedRow = row;
                        break;
                    }
                }
            }

            if (matchedRow != null)
            {
                float currentVal = matchedRow.Slider.Value;
                float step = (e.Delta > 0) ? (settings.ScrollStep / 100f) : -(settings.ScrollStep / 100f);
                float newVal = Math.Max(0.0f, Math.Min(1.0f, currentVal + step));

                SetSessionVolume(matchedRow.SessionId, newVal);
                matchedRow.Slider.UpdateValue(newVal);
                matchedRow.VolLabel.Text = string.Format("{0:0}%", newVal * 100);

                if (hme != null)
                {
                    hme.Handled = true;
                }
                return;
            }

            if (hme != null)
            {
                hme.Handled = true;
            }

            IMMDevice? defaultDev = null;
            object? volumeObj = null;
            IAudioEndpointVolume? volume = null;
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

                        float step = (e.Delta > 0) ? (settings.ScrollStep / 100f) : -(settings.ScrollStep / 100f);
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

        private void BtnScreenMove_MouseDown(object? sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                ShiftScreen(true);
            }
            else if (e.Button == MouseButtons.Right)
            {
                ShiftScreen(false);
            }
        }

        private void ShiftScreen(bool nextRight)
        {
            var screens = Screen.AllScreens;
            if (screens.Length <= 1) return;

            // Sort screens horizontally by their Left boundary
            var sorted = new List<Screen>(screens);
            sorted.Sort((a, b) => a.Bounds.Left.CompareTo(b.Bounds.Left));

            // Find current screen index in sorted list
            int currentIdx = 0;
            string currentDeviceName = settings.ScreenDeviceName;
            
            for (int i = 0; i < sorted.Count; i++)
            {
                if (sorted[i].DeviceName == currentDeviceName)
                {
                    currentIdx = i;
                    break;
                }
            }

            int nextIdx;
            if (nextRight)
            {
                nextIdx = (currentIdx + 1) % sorted.Count;
                settings.Alignment = "Left";
            }
            else
            {
                nextIdx = (currentIdx - 1 + sorted.Count) % sorted.Count;
                settings.Alignment = "Right";
            }

            settings.ScreenIndex = nextIdx;
            settings.ScreenDeviceName = sorted[nextIdx].DeviceName;
            settings.Save();

            // Refresh layout & position
            UpdateLayout();
            UpdatePosition();
        }

        private int GetCollapsedHeight()
        {
            return (int)(36 * DpiHelper.GetScale(this.Handle));
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
                this.Height = GetCollapsedHeight();
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
            processCache.Clear();
            foreach (var row in mixerRows)
            {
                if (row.RowPanel != null)
                {
                    pnlMixer.Controls.Remove(row.RowPanel);
                    row.RowPanel.Dispose();
                }
                row.IconBox?.Dispose();
                row.IconLabel?.Dispose();
                row.NameLabel?.Dispose();
                row.Slider?.Dispose();
                row.VolLabel?.Dispose();
            }
            mixerRows.Clear();
        }

        private void RefreshMixerSessions()
        {
            if (!isExpanded) return;

            try { System.IO.File.AppendAllText(System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "mixerlog.txt"), "--- Refresh Sessions Start ---\n"); } catch {}

            IMMDevice? defaultDev = null;
            IAudioSessionManager2? manager = null;
            IAudioSessionEnumerator? sessionEnum = null;
            object? volumeObj = null;
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
                            IAudioSessionControl? sessionCtrl = null;
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
                                    if (settings.HideSilentApps && state == 0)
                                    {
                                        try { System.IO.File.AppendAllText(System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "mixerlog.txt"), string.Format("Session {0}: Skipped state={1} (Inactive/Silent mode active)\n", i, state)); } catch {}
                                        SafeRelease(sessionCtrl);
                                        continue;
                                    }

                                    IAudioSessionControl2? sessionCtrl2 = sessionCtrl as IAudioSessionControl2;
                                    ISimpleAudioVolume? simpleVol = sessionCtrl as ISimpleAudioVolume;
                                    
                                    if (sessionCtrl2 == null || simpleVol == null)
                                    {
                                        try { System.IO.File.AppendAllText(System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "mixerlog.txt"), string.Format("Session {0}: sessionCtrl2 is null={1}, simpleVol is null={2}\n", i, sessionCtrl2 == null, simpleVol == null)); } catch {}
                                    }

                                    if (sessionCtrl2 != null && simpleVol != null)
                                    {
                                        int pid = 0;
                                        sessionCtrl2.GetProcessId(out pid);
                                        
                                        IntPtr idPtr = IntPtr.Zero;
                                        string? sessionId = null;
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
                                        Icon? icon;
                                        GetSessionInfo(pid, isSystem, out name, out icon);

                                        if (sessionId != null)
                                        {
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

            // Find current screen to limit height to screen bounds dynamically
            Screen? currentScr = null;
            if (!string.IsNullOrEmpty(settings.ScreenDeviceName))
            {
                foreach (var s in Screen.AllScreens)
                {
                    if (s.DeviceName == settings.ScreenDeviceName)
                    {
                        currentScr = s;
                        break;
                    }
                }
            }
            if (currentScr == null)
            {
                int scrIdx = Math.Max(0, Math.Min(Screen.AllScreens.Length - 1, settings.ScreenIndex));
                currentScr = Screen.AllScreens[scrIdx];
            }

            float scale = DpiHelper.GetScale(this.Handle);
            int maxPanelHeight = currentScr.Bounds.Height - (int)(120 * scale); // safe margin for taskbar and padding
            int displayCount = activeSessions.Count;
            int headerH = (int)(24 * scale); // Height of the "Hide silent apps" checkbox at the top
            
            // Adjust minimum height to prevent scrollbars when there are no active sessions to display
            int minHeight = (displayCount == 0) ? (int)(68 * scale) : (int)(52 * scale);
            int calculatedPanelHeight = Math.Max(minHeight, Math.Min(maxPanelHeight, displayCount * (int)(36 * scale) + (int)(16 * scale) + headerH));
            int newHeight = calculatedPanelHeight + GetCollapsedHeight();
            
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
            float scale = DpiHelper.GetScale(this.Handle);
            int rowHeight = (int)(36 * scale);
            int checkboxH = (int)(24 * scale);
            
            pnlMixer.SuspendLayout();

            // Add the "Hide silent apps" checkbox
            CheckBox cbHideSilent = new CheckBox
            {
                Text = "Hide silent apps",
                Location = new Point((int)(8 * scale), (int)(4 * scale)),
                Size = new Size((int)(180 * scale), (int)(18 * scale)),
                Checked = settings.HideSilentApps,
                ForeColor = themeTextColor,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 7.5f * scale)
            };
            cbHideSilent.CheckedChanged += (s, e) => {
                settings.HideSilentApps = cbHideSilent.Checked;
                settings.Save();
                RefreshMixerSessions();
            };
            pnlMixer.Controls.Add(cbHideSilent);

            int y = checkboxH;
            
            if (sessions.Count == 0)
            {
                Label lblNoSessions = new Label
                {
                    Location = new Point((int)(10 * scale), checkboxH + (int)(10 * scale)),
                    Size = new Size(pnlMixer.Width - (int)(20 * scale), (int)(24 * scale)),
                    Font = new Font("Segoe UI", 8f * scale, FontStyle.Italic),
                    ForeColor = themeTextColor,
                    Text = "No active applications found",
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
                    
                    int rw = pnlMixer.Width - (int)(24 * scale); // Ensure safe margin for vertical scrollbar and prevent horizontal scrollbar
                    Panel rowPanel = new Panel
                    {
                        Location = new Point(0, y),
                        Size = new Size(rw, rowHeight),
                        BackColor = themeBgColor
                    };
                    
                    PictureBox? iconBox = null;
                    Label? iconLabel = null;
                    
                    if (data.Icon != null)
                    {
                        iconBox = new PictureBox
                        {
                            Location = new Point((int)(8 * scale), (int)(10 * scale)),
                            Size = new Size((int)(16 * scale), (int)(16 * scale)),
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
                            Location = new Point((int)(8 * scale), (int)(8 * scale)),
                            Size = new Size((int)(16 * scale), (int)(16 * scale)),
                            Font = new Font("Segoe MDL2 Assets", 8f * scale),
                            ForeColor = themeTextColor,
                            Text = data.IsSystemSounds ? "\uE767" : "\uE715",
                            TextAlign = ContentAlignment.MiddleCenter,
                            BackColor = themeBgColor
                        };
                        rowPanel.Controls.Add(iconLabel);
                    }
                    
                    Label nameLabel = new Label
                    {
                        Location = new Point((int)(28 * scale), (int)(10 * scale)),
                        Size = new Size((int)(80 * scale), (int)(16 * scale)),
                        Font = new Font("Segoe UI", 8f * scale),
                        ForeColor = themeTextColor,
                        Text = data.Name,
                        TextAlign = ContentAlignment.MiddleLeft,
                        BackColor = themeBgColor
                    };
                    rowPanel.Controls.Add(nameLabel);
                    
                    int sliderWidth = rw - (int)(112 * scale) - (int)(36 * scale) - (int)(8 * scale);
                    VolumeSlider slider = new VolumeSlider
                    {
                        Location = new Point((int)(112 * scale), (int)(14 * scale)),
                        Size = new Size(sliderWidth, (int)(8 * scale)),
                        ActiveColor = themeActiveBgColor,
                        InactiveColor = Color.FromArgb(80, 128, 128, 128),
                        BackColor = themeBgColor
                    };
                    slider.UpdateValue(data.Volume);
                    
                    Label volLabel = new Label
                    {
                        Location = new Point(rw - (int)(36 * scale), (int)(10 * scale)),
                        Size = new Size((int)(30 * scale), (int)(16 * scale)),
                        Font = new Font("Segoe UI", 8f * scale),
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
            IMMDevice? defaultDev = null;
            IAudioSessionManager2? manager = null;
            IAudioSessionEnumerator? sessionEnum = null;
            object? volumeObj = null;
            try
            {
                int hr = enumerator.GetDefaultAudioEndpoint(0, 0, out defaultDev);
                if (hr == 0 && defaultDev != null)
                {
                    Guid iid = new Guid("77AA99A0-1BD6-484F-8BC7-2C654C9A9B6F");
                    defaultDev.Activate(ref iid, 1, IntPtr.Zero, out volumeObj);
                    manager = volumeObj as IAudioSessionManager2;
                    if (manager != null && manager.GetSessionEnumerator(out sessionEnum) == 0 && sessionEnum != null)
                    {
                        int count;
                        sessionEnum.GetCount(out count);
                        for (int i = 0; i < count; i++)
                        {
                            IAudioSessionControl? sessionCtrl = null;
                            try
                            {
                                if (sessionEnum.GetSession(i, out sessionCtrl) == 0 && sessionCtrl != null)
                                {
                                    IAudioSessionControl2? sessionCtrl2 = sessionCtrl as IAudioSessionControl2;
                                    ISimpleAudioVolume? simpleVol = sessionCtrl as ISimpleAudioVolume;
                                    if (sessionCtrl2 != null && simpleVol != null)
                                    {
                                        IntPtr idPtr;
                                        sessionCtrl2.GetSessionIdentifier(out idPtr);
                                        string? sid = Marshal.PtrToStringUni(idPtr);
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
                SafeRelease(volumeObj);
                SafeRelease(defaultDev);
            }
        }

        private void GetSessionInfo(int pid, bool isSystem, out string name, out Icon? icon)
        {
            if (processCache.TryGetValue(pid, out var cachedItem))
            {
                name = cachedItem.Name;
                icon = cachedItem.Icon;
                return;
            }

            bool isEst = System.Globalization.CultureInfo.CurrentCulture.TwoLetterISOLanguageName.Equals("et", StringComparison.OrdinalIgnoreCase);
            name = isEst ? "Süsteemi helid" : "System Sounds";
            icon = null;

            if (isSystem || pid == 0)
            {
                processCache[pid] = new ProcessCacheItem { Name = name, Icon = icon };
                return;
            }

            try
            {
                using (var proc = System.Diagnostics.Process.GetProcessById(pid))
                {
                    string path = proc.MainModule?.FileName ?? string.Empty;
                    name = proc.ProcessName;
                    
                    if (name.Length > 0)
                    {
                        name = char.ToUpper(name[0]) + name.Substring(1);
                    }

                    if (!string.IsNullOrEmpty(path))
                    {
                        icon = Icon.ExtractAssociatedIcon(path);
                    }
                }
            }
            catch
            {
                name = isEst ? ("Rakendus (PID " + pid + ")") : ("Application (PID " + pid + ")");
            }

            processCache[pid] = new ProcessCacheItem { Name = name, Icon = icon };
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

        private void SafeRelease(object? obj)
        {
            if (obj != null && Marshal.IsComObject(obj))
            {
                try { Marshal.ReleaseComObject(obj); } catch { }
            }
        }

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

        private bool IsForegroundWindowFullscreen(out Screen? onScreen, out uint processId)
        {
            onScreen = null;
            processId = 0;
            IntPtr hwnd = Win32.GetForegroundWindow();
            if (hwnd == IntPtr.Zero || hwnd == this.Handle) return false;

            // Skip desktop and taskbar windows
            string className = Win32.GetClassNameOfWindow(hwnd);
            if (className == "Shell_TrayWnd" || className == "WorkerW" || className == "Progman" || className == "CabinetWClass")
            {
                return false;
            }

            Win32.RECT rect;
            if (Win32.GetWindowRect(hwnd, out rect))
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
                        Win32.GetWindowThreadProcessId(hwnd, out processId);
                        try
                        {
                            using (var proc = System.Diagnostics.Process.GetProcessById((int)processId))
                            {
                                string name = proc.ProcessName.ToLower();
                                if (name == "lockapp" || name == "logonui" || name == "explorer" || name == "dwm" || 
                                    name == "shellexperiencehost" || name == "searchhost" || name == "startmenuexperiencehost" ||
                                    name == "chrome" || name == "firefox" || name == "msedge" || name == "opera" || 
                                    name == "brave" || name == "vivaldi" || name == "discord" || name == "teams" || name == "zoom")
                                {
                                    return false;
                                }
                            }
                        }
                        catch { }
                        return true;
                    }
                }
            }
            return false;
        }
    }
}
