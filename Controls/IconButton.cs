using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using TaskbarAudioSwitcher.Core;

namespace TaskbarAudioSwitcher.Controls
{
    public class IconButton : Control
    {
        public string Glyph { get; set; } = string.Empty;
        public bool IsActive { get; set; }
        public Color ActiveBgColor { get; set; }
        public Color ActiveFgColor { get; set; }
        public Color HoverBgColor { get; set; }
        public string DeviceAbbreviation { get; set; } = string.Empty;
        public float WidgetFontSize { get; set; } = 8.0f;

        private bool isMuted = false;
        public bool IsMuted
        {
            get => isMuted;
            set
            {
                if (isMuted != value)
                {
                    isMuted = value;
                    Invalidate();
                }
            }
        }

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

            float scale = DpiHelper.GetScale(this.Handle);
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

            int radius = (int)(4 * scale);
            if (bg != Color.Transparent)
            {
                using (var brush = new SolidBrush(bg))
                {
                    g.FillRoundRectangle(brush, 0, 0, Width - 1, Height - 1, radius);
                }
            }

            if (border != Color.Transparent)
            {
                using (var pen = new Pen(border, 1f))
                {
                    g.DrawRoundRectangle(pen, 0, 0, Width - 1, Height - 1, radius);
                }
            }

            Font? fontToUse = null;
            try { fontToUse = new Font("Segoe MDL2 Assets", 10f * scale); }
            catch { fontToUse = new Font("Segoe UI", 10f * scale); }

            using (fontToUse)
            {
                Color textColor = IsActive ? ActiveFgColor : ForeColor;
                using (var brush = new SolidBrush(textColor))
                {
                    if (string.IsNullOrEmpty(DeviceAbbreviation))
                    {
                        var sf = new StringFormat
                        {
                            Alignment = StringAlignment.Center,
                            LineAlignment = StringAlignment.Center
                        };
                        g.DrawString(Glyph, fontToUse, brush, new RectangleF(0, 0, Width, Height), sf);
                    }
                    else
                    {
                        // Draw icon slightly shifted up
                        var sfIcon = new StringFormat
                        {
                            Alignment = StringAlignment.Center,
                            LineAlignment = StringAlignment.Near
                        };
                        g.DrawString(Glyph, fontToUse, brush, new RectangleF(0, 2 * scale, Width, Height - 12 * scale), sfIcon);

                        // Draw 3-letter abbreviation at the bottom
                        float badgeFontSize = Math.Max(4.5f, WidgetFontSize - 1.8f) * scale;
                        using (var textFont = new Font("Segoe UI", badgeFontSize, FontStyle.Regular))
                        {
                            var sfText = new StringFormat
                            {
                                Alignment = StringAlignment.Center,
                                LineAlignment = StringAlignment.Far
                            };
                            g.DrawString(DeviceAbbreviation, textFont, brush, new RectangleF(0, 15 * scale, Width, 13 * scale), sfText);
                        }
                    }
                }
            }

            if (IsMuted)
            {
                using (var pen = new Pen(Color.FromArgb(232, 17, 35), Math.Max(2f, 2f * scale)))
                {
                    pen.StartCap = LineCap.Round;
                    pen.EndCap = LineCap.Round;
                    g.DrawLine(pen, (int)(5 * scale), (int)(5 * scale), Width - (int)(6 * scale), Height - (int)(6 * scale));
                }
            }
        }
    }
}
