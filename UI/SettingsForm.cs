using System;
using System.Collections.Generic;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using TaskbarAudioSwitcher.Core;
using TaskbarAudioSwitcher.Native;

namespace TaskbarAudioSwitcher.UI
{
    internal class SettingsForm : Form
    {
        private AppSettings settings;
        private IMMDeviceEnumerator enumerator;
        private bool isDarkMode;

        // UI Controls
        private CheckBox cbFilterDevices;
        private Panel pnlDevices;
        private ComboBox cmbScreen;
        private ComboBox cmbAlignment;
        private CheckBox cbAlwaysOnTop;
        private CheckBox cbMoveOnFullscreen;
        private CheckBox cbShowScreenMove;
        private ComboBox cmbScrollStep;
        private Button btnSave;
        private Button btnCancel;
        
        private class DeviceSettingRow
        {
            public CheckBox CheckBox { get; set; } = null!;
            public TextBox TextBox { get; set; } = null!;
            public string DeviceId { get; set; } = string.Empty;
        }
        private List<DeviceSettingRow> deviceRows;

        public SettingsForm(AppSettings settings, IMMDeviceEnumerator enumerator, bool isDarkMode)
        {
            this.settings = settings;
            this.enumerator = enumerator;
            this.isDarkMode = isDarkMode;
            this.deviceRows = new List<DeviceSettingRow>();

            // Setup Window
            this.Text = "Settings - Taskbar Audio Switcher";
            this.Size = new Size(380, 560);
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

            // Filter Checkbox
            cbFilterDevices = new CheckBox
            {
                Text = "Show only selected devices (filter active)",
                Location = new Point(20, 15),
                Size = new Size(325, 24),
                Checked = settings.FilterDevices,
                Font = new Font("Segoe UI", 9f, FontStyle.Bold),
                FlatStyle = FlatStyle.Flat
            };
            this.Controls.Add(cbFilterDevices);

            // Title Label for Devices
            Label lblDevices = new Label
            {
                Text = "Devices to show when filter is active:",
                Location = new Point(20, 45),
                Size = new Size(340, 20),
                Font = new Font("Segoe UI", 8.5f)
            };
            this.Controls.Add(lblDevices);

            // Devices Panel
            pnlDevices = new Panel
            {
                Location = new Point(20, 68),
                Size = new Size(325, 130),
                BorderStyle = BorderStyle.FixedSingle,
                BackColor = controlBg,
                AutoScroll = true,
                Enabled = settings.FilterDevices
            };
            this.Controls.Add(pnlDevices);

            cbFilterDevices.CheckedChanged += (s, e) => {
                pnlDevices.Enabled = cbFilterDevices.Checked;
            };

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
                Location = new Point(20, 320),
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
                Location = new Point(20, 345),
                Size = new Size(325, 24),
                Checked = settings.MoveOnFullscreen,
                Font = new Font("Segoe UI", 9f, FontStyle.Bold),
                FlatStyle = FlatStyle.Flat
            };
            this.Controls.Add(cbMoveOnFullscreen);

            // Show Screen Move Checkbox
            cbShowScreenMove = new CheckBox
            {
                Text = "Show monitor switch button on the bar",
                Location = new Point(20, 370),
                Size = new Size(325, 24),
                Checked = settings.ShowScreenMoveButton,
                Font = new Font("Segoe UI", 9f, FontStyle.Bold),
                FlatStyle = FlatStyle.Flat
            };
            this.Controls.Add(cbShowScreenMove);

            // Description Label for Screen Move
            Label lblScreenMoveDesc = new Label
            {
                Text = "Left-click: Move to next monitor on the right (docked to left edge).\r\nRight-click: Move to next monitor on the left (docked to right edge).",
                Location = new Point(40, 394),
                Size = new Size(305, 30),
                Font = new Font("Segoe UI", 7.5f, FontStyle.Regular),
                ForeColor = isDarkMode ? Color.FromArgb(170, 170, 170) : Color.FromArgb(100, 100, 100)
            };
            this.Controls.Add(lblScreenMoveDesc);

            // Scroll Step Label & Dropdown
            Label lblScrollStep = new Label
            {
                Text = "Scroll volume step:",
                Location = new Point(20, 432),
                Size = new Size(170, 20),
                Font = new Font("Segoe UI", 9f, FontStyle.Bold)
            };
            this.Controls.Add(lblScrollStep);

            cmbScrollStep = new ComboBox
            {
                Location = new Point(200, 430),
                Size = new Size(145, 24),
                DropDownStyle = ComboBoxStyle.DropDownList,
                BackColor = controlBg,
                ForeColor = textColor,
                FlatStyle = FlatStyle.Flat
            };
            cmbScrollStep.Items.Add("1%");
            cmbScrollStep.Items.Add("2%");
            cmbScrollStep.Items.Add("5%");
            cmbScrollStep.Items.Add("10%");
            this.Controls.Add(cmbScrollStep);

            if (settings.ScrollStep == 1) cmbScrollStep.SelectedIndex = 0;
            else if (settings.ScrollStep == 5) cmbScrollStep.SelectedIndex = 2;
            else if (settings.ScrollStep == 10) cmbScrollStep.SelectedIndex = 3;
            else cmbScrollStep.SelectedIndex = 1; // Default 2%

            // Save button
            btnSave = new Button
            {
                Text = "Save",
                Location = new Point(155, 475),
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
                Location = new Point(255, 475),
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
                                    name = Marshal.PtrToStringUni(pv.pointerVal) ?? "Audio output";
                                    ComNative.PropVariantClear(ref pv);
                                }
                                Marshal.ReleaseComObject(props);
                            }

                            CheckBox cb = new CheckBox
                            {
                                Text = name,
                                Location = new Point(10, y),
                                Size = new Size(225, 24),
                                Checked = selectedIds.Contains(devId),
                                ForeColor = textColor,
                                FlatStyle = FlatStyle.Flat
                            };
                            TextBox tb = new TextBox
                            {
                                Location = new Point(245, y + 2),
                                Size = new Size(45, 20),
                                MaxLength = 3,
                                Text = settings.GetDeviceNickname(devId, name),
                                BackColor = isDarkMode ? Color.FromArgb(45, 45, 45) : Color.White,
                                ForeColor = textColor,
                                BorderStyle = isDarkMode ? BorderStyle.FixedSingle : BorderStyle.Fixed3D
                            };

                            pnlDevices.Controls.Add(cb);
                            pnlDevices.Controls.Add(tb);
                            deviceRows.Add(new DeviceSettingRow { CheckBox = cb, TextBox = tb, DeviceId = devId });
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

        private void BtnSave_Click(object? sender, EventArgs e)
        {
            // Collect checked device IDs and nickname mappings
            var checkedIds = new List<string>();
            var nicknames = new List<string>();
            foreach (var row in deviceRows)
            {
                if (row.CheckBox.Checked)
                {
                    checkedIds.Add(row.DeviceId);
                }
                string nick = row.TextBox.Text.Trim().ToUpper();
                if (!string.IsNullOrEmpty(nick))
                {
                    nicknames.Add(row.DeviceId + ":" + nick);
                }
            }

            settings.DisplayDevices = string.Join(",", checkedIds.ToArray());
            settings.DeviceNicknames = string.Join(",", nicknames.ToArray());
            settings.FilterDevices = cbFilterDevices.Checked;
            settings.ScreenIndex = cmbScreen.SelectedIndex;
            if (cmbScreen.SelectedIndex >= 0 && cmbScreen.SelectedIndex < Screen.AllScreens.Length)
            {
                settings.ScreenDeviceName = Screen.AllScreens[cmbScreen.SelectedIndex].DeviceName;
            }
            settings.Alignment = cmbAlignment.SelectedIndex == 1 ? "Left" : "Right";
            settings.AlwaysOnTop = cbAlwaysOnTop.Checked;
            settings.MoveOnFullscreen = cbMoveOnFullscreen.Checked;
            settings.ShowScreenMoveButton = cbShowScreenMove.Checked;

            if (cmbScrollStep.SelectedIndex == 0) settings.ScrollStep = 1;
            else if (cmbScrollStep.SelectedIndex == 2) settings.ScrollStep = 5;
            else if (cmbScrollStep.SelectedIndex == 3) settings.ScrollStep = 10;
            else settings.ScrollStep = 2; // Default 2%

            settings.Save();
            this.DialogResult = DialogResult.OK;
            this.Close();
        }
    }
}
