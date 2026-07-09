using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using TaskbarAudioSwitcher.Core;

namespace TaskbarAudioSwitcher.Controls
{
    public class MicrophoneButton : Control
    {
        private bool isMuted = false;
        private bool isInUse = false;
        private int volumePercent = 50;

        public bool IsMuted
        {
            get => isMuted;
            set { if (isMuted != value) { isMuted = value; Invalidate(); } }
        }

        public bool IsInUse
        {
            get => isInUse;
            set { if (isInUse != value) { isInUse = value; Invalidate(); } }
        }

        public int VolumePercent
        {
            get => volumePercent;
            set { if (volumePercent != value) { volumePercent = value; Invalidate(); } }
        }

        public Color ActiveBgColor { get; set; } = Color.FromArgb(0, 120, 215);
        public Color ActiveFgColor { get; set; } = Color.White;
        public Color HoverBgColor { get; set; } = Color.FromArgb(20, 128, 128, 128);

        private bool isHovered = false;

        public MicrophoneButton()
        {
            DoubleBuffered = true;
            Size = new Size(28, 28);
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

            if (isHovered)
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

            // Draw Microphone Custom Icon
            float cx = Width / 2f;
            float cy = 8.5f * scale; // Center of top half

            // Colors
            Color outlineColor = ForeColor; // White in dark mode, dark in light mode
            Color fillColor = Color.FromArgb(231, 76, 60); // Red for in-use

            // 1. Draw Stand Base & Cup
            using (var pen = new Pen(outlineColor, 1.25f * scale))
            {
                pen.StartCap = LineCap.Round;
                pen.EndCap = LineCap.Round;

                // Cup U-shape: from -180 to 0 degrees (bottom half of circle)
                float cupR = 4.5f * scale;
                g.DrawArc(pen, cx - cupR, cy - 2.5f * scale, cupR * 2, cupR * 2, 0, 180);

                // Stand stem
                g.DrawLine(pen, cx, cy + 2f * scale, cx, cy + 5f * scale);

                // Stand base
                float baseW = 3.5f * scale;
                g.DrawLine(pen, cx - baseW, cy + 5f * scale, cx + baseW, cy + 5f * scale);
            }

            // 2. Draw/Fill Mic Body (cylinder)
            float bodyW = 5f * scale;
            float bodyH = 8.5f * scale;
            float bodyX = cx - bodyW / 2f;
            float bodyY = cy - 5f * scale;
            float bodyR = 2.5f * scale; // corner radius for cylinder top/bottom

            using (var path = new GraphicsPath())
            {
                // Create a rounded rectangle for the mic body
                path.AddArc(bodyX, bodyY, bodyR * 2, bodyR * 2, 180, 90);
                path.AddArc(bodyX + bodyW - bodyR * 2, bodyY, bodyR * 2, bodyR * 2, 270, 90);
                path.AddArc(bodyX + bodyW - bodyR * 2, bodyY + bodyH - bodyR * 2, bodyR * 2, bodyR * 2, 0, 90);
                path.AddArc(bodyX, bodyY + bodyH - bodyR * 2, bodyR * 2, bodyR * 2, 90, 90);
                path.CloseAllFigures();

                if (isInUse)
                {
                    using (var fillBrush = new SolidBrush(fillColor))
                    {
                        g.FillPath(fillBrush, path);
                    }
                }

                using (var pen = new Pen(outlineColor, 1.25f * scale))
                {
                    g.DrawPath(pen, path);
                }
            }

            // 3. Draw Red Mute Slash (strike-through) if muted
            if (isMuted)
            {
                using (var slashPen = new Pen(Color.FromArgb(231, 76, 60), 1.8f * scale))
                {
                    slashPen.StartCap = LineCap.Round;
                    slashPen.EndCap = LineCap.Round;
                    // Draw diagonal line from top-right to bottom-left of the microphone icon bounds
                    g.DrawLine(slashPen, cx + 5.5f * scale, cy - 6f * scale, cx - 5.5f * scale, cy + 6f * scale);
                }
            }

            // 4. Draw Volume Text at the bottom
            Color textColor = isMuted ? Color.FromArgb(120, outlineColor) : outlineColor;
            using (var brush = new SolidBrush(textColor))
            {
                using (var textFont = new Font("Segoe UI", 6.2f * scale, FontStyle.Regular))
                {
                    var sfText = new StringFormat
                    {
                        Alignment = StringAlignment.Center,
                        LineAlignment = StringAlignment.Far
                    };
                    string volText = volumePercent + "%";
                    g.DrawString(volText, textFont, brush, new RectangleF(0, 15 * scale, Width, 13 * scale), sfText);
                }
            }
        }
    }
}
