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
        private Color themeActiveColor = Color.FromArgb(0, 120, 215);

        public UpdateForm(string currentVersion, string latestVersion, bool isDarkMode)
        {
            this.Text = "Uuendus saadaval";
            this.Size = new Size(360, 200);
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
                Text = "Uus versioon on saadaval!",
                Font = new Font("Segoe UI", 12f, FontStyle.Bold),
                ForeColor = themeActiveColor,
                Location = new Point(20, 20),
                Size = new Size(320, 30),
                TextAlign = ContentAlignment.MiddleLeft
            };
            this.Controls.Add(lblTitle);

            // Message Body
            Label lblMessage = new Label
            {
                Text = string.Format(
                    "Märgati rakenduse uuemat versiooni GitHubis.\n\nSinu versioon:  v{0}\nUus versioon:   v{1}",
                    currentVersion, latestVersion
                ),
                Font = new Font("Segoe UI", 9f),
                ForeColor = themeTextColor,
                Location = new Point(20, 60),
                Size = new Size(320, 70),
                TextAlign = ContentAlignment.TopLeft
            };
            this.Controls.Add(lblMessage);

            // Cancel Button
            Button btnCancel = new Button
            {
                Text = "Sule",
                Font = new Font("Segoe UI", 9f),
                ForeColor = themeTextColor,
                BackColor = Color.Transparent,
                FlatStyle = FlatStyle.Flat,
                Location = new Point(240, 145),
                Size = new Size(100, 32),
                Cursor = Cursors.Hand
            };
            btnCancel.FlatAppearance.BorderColor = themeBorderColor;
            btnCancel.FlatAppearance.MouseOverBackColor = Color.FromArgb(20, isDarkMode ? Color.White : Color.Black);
            btnCancel.Click += (s, e) => this.Close();
            this.Controls.Add(btnCancel);

            // Action Button (GitHub)
            Button btnGitHub = new Button
            {
                Text = "Ava GitHub",
                Font = new Font("Segoe UI", 9f, FontStyle.Bold),
                ForeColor = Color.White,
                BackColor = themeActiveColor,
                FlatStyle = FlatStyle.Flat,
                Location = new Point(130, 145),
                Size = new Size(100, 32),
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
