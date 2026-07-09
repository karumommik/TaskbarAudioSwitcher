using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace TaskbarAudioSwitcher.Controls
{
    public class ModernToolStripRenderer : ToolStripRenderer
    {
        private bool isDarkMode;
        private Color bgColor;
        private Color textColor;
        private Color hoverColor;
        private Color borderColor;
        private Color separatorColor;

        public ModernToolStripRenderer(bool isDarkMode)
        {
            this.isDarkMode = isDarkMode;
            if (isDarkMode)
            {
                bgColor = Color.FromArgb(32, 32, 32);
                textColor = Color.FromArgb(235, 235, 235);
                hoverColor = Color.FromArgb(50, 50, 50);
                borderColor = Color.FromArgb(55, 55, 55);
                separatorColor = Color.FromArgb(60, 60, 60);
            }
            else
            {
                bgColor = Color.FromArgb(243, 243, 243);
                textColor = Color.FromArgb(40, 40, 40);
                hoverColor = Color.FromArgb(220, 220, 220);
                borderColor = Color.FromArgb(210, 210, 210);
                separatorColor = Color.FromArgb(200, 200, 200);
            }
        }

        protected override void OnRenderToolStripBackground(ToolStripRenderEventArgs e)
        {
            using (var brush = new SolidBrush(bgColor))
            {
                e.Graphics.FillRectangle(brush, e.AffectedBounds);
            }
        }

        protected override void OnRenderToolStripBorder(ToolStripRenderEventArgs e)
        {
            using (var pen = new Pen(borderColor, 1))
            {
                e.Graphics.DrawRectangle(pen, 0, 0, e.ToolStrip.Width - 1, e.ToolStrip.Height - 1);
            }
        }

        protected override void OnRenderImageMargin(ToolStripRenderEventArgs e)
        {
            // Do not draw the default gray vertical line/bar, keep margin area flat and clean
        }

        protected override void OnRenderMenuItemBackground(ToolStripItemRenderEventArgs e)
        {
            var g = e.Graphics;
            var item = e.Item;

            if (item.Selected && item.Enabled)
            {
                var rect = new Rectangle(2, 1, item.Width - 4, item.Height - 2);
                using (var brush = new SolidBrush(hoverColor))
                {
                    g.FillRectangle(brush, rect);
                }
            }
        }

        protected override void OnRenderItemText(ToolStripItemTextRenderEventArgs e)
        {
            e.TextColor = e.Item.Enabled ? textColor : Color.Gray;
            base.OnRenderItemText(e);
        }

        protected override void OnRenderSeparator(ToolStripSeparatorRenderEventArgs e)
        {
            using (var pen = new Pen(separatorColor, 1))
            {
                int middle = e.Item.Height / 2;
                e.Graphics.DrawLine(pen, 10, middle, e.Item.Width - 10, middle);
            }
        }

        protected override void OnRenderItemCheck(ToolStripItemImageRenderEventArgs e)
        {
            var g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;

            var rect = e.ImageRectangle;
            Color checkColor = Color.FromArgb(0, 120, 215); // Accent blue

            using (var pen = new Pen(checkColor, 2f))
            {
                pen.StartCap = LineCap.Round;
                pen.EndCap = LineCap.Round;
                pen.LineJoin = LineJoin.Round;

                // Draw a clean flat checkmark path inside the reserved icon area
                float x = rect.X + rect.Width / 2f - 4f;
                float y = rect.Y + rect.Height / 2f - 1f;
                g.DrawLine(pen, x, y, x + 3f, y + 3f);
                g.DrawLine(pen, x + 3f, y + 3f, x + 8f, y - 4f);
            }
        }
    }
}
