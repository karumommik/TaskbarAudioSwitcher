using System;
using System.Drawing;
using System.Windows.Forms;
using System.Diagnostics;

namespace TaskbarAudioSwitcher.UI
{
    public class UpdateForm : Form
    {
        private Color themeBgColor;
        private Color themeTextColor;
        private Color themeBorderColor;
        private Color themeActiveColor;

        public UpdateForm(string currentVersion, string latestVersion, bool isDarkMode)
        {
            float scale = TaskbarAudioSwitcher.Core.DpiHelper.GetScale(this.Handle);
            this.themeActiveColor = TaskbarAudioSwitcher.Core.ThemeHelper.GetAccentColor();
            this.Text = "Update Available";
            this.AutoScaleMode = AutoScaleMode.None;
            this.Font = new Font("Segoe UI", 9f);
            this.ClientSize = new Size((int)(380 * scale), (int)(200 * scale));
            this.FormBorderStyle = FormBorderStyle.None;
            this.StartPosition = FormStartPosition.CenterScreen;
            this.ShowInTaskbar = true;
            this.TopMost = true;

            // Theme colors
            if (isDarkMode)
            {
                themeBgColor = Color.FromArgb(28, 28, 28);
                themeTextColor = Color.FromArgb(235, 235, 235);
                themeBorderColor = Color.FromArgb(55, 55, 55);
            }
            else
            {
                themeBgColor = Color.FromArgb(243, 243, 243);
                themeTextColor = Color.FromArgb(40, 40, 40);
                themeBorderColor = Color.FromArgb(210, 210, 210);
            }

            this.BackColor = themeBgColor;

            // Draw border manually in Paint event
            this.Paint += (s, e) =>
            {
                using (Pen pen = new Pen(themeBorderColor, 1))
                {
                    e.Graphics.DrawRectangle(pen, 0, 0, this.Width - 1, this.Height - 1);
                }
            };

            // Custom Title
            Label lblTitle = new Label
            {
                Text = "New version available!",
                Font = new Font("Segoe UI", 12f, FontStyle.Bold),
                ForeColor = themeActiveColor,
                Location = new Point((int)(20 * scale), (int)(20 * scale)),
                Size = new Size((int)(340 * scale), (int)(30 * scale)),
                TextAlign = ContentAlignment.MiddleLeft
            };
            this.Controls.Add(lblTitle);

            // Message Body
            Label lblMessage = new Label
            {
                Text = string.Format(
                    "A newer version of the application was found on GitHub.\n\nYour version:   v{0}\nNew version:    v{1}",
                    currentVersion, latestVersion
                ),
                Font = new Font("Segoe UI", 9f),
                ForeColor = themeTextColor,
                Location = new Point((int)(20 * scale), (int)(58 * scale)),
                Size = new Size((int)(340 * scale), (int)(75 * scale)),
                TextAlign = ContentAlignment.TopLeft
            };
            this.Controls.Add(lblMessage);

            // Cancel Button
            Button btnCancel = new Button
            {
                Text = "Close",
                Font = new Font("Segoe UI", 9f),
                ForeColor = themeTextColor,
                BackColor = Color.Transparent,
                FlatStyle = FlatStyle.Flat,
                Location = new Point((int)(250 * scale), (int)(145 * scale)),
                Size = new Size((int)(110 * scale), (int)(34 * scale)),
                Cursor = Cursors.Hand
            };
            btnCancel.FlatAppearance.BorderColor = themeBorderColor;
            btnCancel.FlatAppearance.MouseOverBackColor = Color.FromArgb(20, isDarkMode ? Color.White : Color.Black);
            btnCancel.Click += (s, e) => this.Close();
            this.Controls.Add(btnCancel);

            // Action Button (GitHub)
            Button btnGitHub = new Button
            {
                Text = "Open GitHub",
                Font = new Font("Segoe UI", 9f, FontStyle.Bold),
                ForeColor = Color.White,
                BackColor = themeActiveColor,
                FlatStyle = FlatStyle.Flat,
                Location = new Point((int)(130 * scale), (int)(145 * scale)),
                Size = new Size((int)(110 * scale), (int)(34 * scale)),
                Cursor = Cursors.Hand
            };
            btnGitHub.FlatAppearance.BorderSize = 0;
            btnGitHub.Click += (s, e) =>
            {
                try
                {
                    Process.Start(new ProcessStartInfo("https://github.com/karumommik/TaskbarAudioSwitcher/releases") { UseShellExecute = true });
                }
                catch { }
                this.Close();
            };
            this.Controls.Add(btnGitHub);
        }
    }
}
