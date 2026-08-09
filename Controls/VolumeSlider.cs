using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using TaskbarAudioSwitcher.Core;

namespace TaskbarAudioSwitcher.Controls
{
    public class VolumeSlider : Control
    {
        private float value = 0.5f; // 0.0 to 1.0
        public event EventHandler? ValueChanged;
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
        public Orientation Orientation { get; set; } = Orientation.Horizontal;

        private bool isDarkMode = true;
        public bool IsDarkMode
        {
            get => isDarkMode;
            set
            {
                if (isDarkMode != value)
                {
                    isDarkMode = value;
                    Invalidate();
                }
            }
        }

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

            float scale = DpiHelper.GetScale(this.Handle);

            if (Orientation == Orientation.Vertical)
            {
                int trackWidth = Math.Min(Width, (int)(10 * scale));
                int x = (Width - trackWidth) / 2;
                int yMargin = (int)(3 * scale);
                int h = Height - yMargin * 2;
                if (h <= 0) return;

                int cornerRadius = (int)(4 * scale);

                // Inactive track background (high contrast in light mode)
                Color bgTrackColor = IsDarkMode ? InactiveColor : Color.FromArgb(170, 200, 200, 200);
                using (var brush = new SolidBrush(bgTrackColor))
                {
                    g.FillRoundRectangle(brush, x, yMargin, trackWidth, h, cornerRadius);
                }

                // Active track (rising filled volume level column)
                int activeHeight = (int)(h * value);
                if (activeHeight > 0)
                {
                    int activeY = yMargin + (h - activeHeight);
                    using (var brush = new SolidBrush(ActiveColor))
                    {
                        g.FillRoundRectangle(brush, x, activeY, trackWidth, activeHeight, cornerRadius);
                    }
                }

                // Crisp outline border for clear visibility on light backgrounds
                Color outlineColor = IsDarkMode ? Color.FromArgb(50, 255, 255, 255) : Color.FromArgb(110, 80, 80, 80);
                using (var pen = new Pen(outlineColor, 1f))
                {
                    g.DrawRoundRectangle(pen, x, yMargin, trackWidth - 1, h - 1, cornerRadius);
                }
            }
            else
            {
                int trackHeight = (int)(4 * scale);
                int y = (Height - trackHeight) / 2;
                int x = (int)(6 * scale);
                int w = Width - x * 2;
                if (w <= 0) return;

                // Inactive track (high contrast in light mode)
                Color bgTrackColor = IsDarkMode ? InactiveColor : Color.FromArgb(170, 200, 200, 200);
                using (var brush = new SolidBrush(bgTrackColor))
                {
                    g.FillRoundRectangle(brush, x, y, w, trackHeight, (int)(2 * scale));
                }

                // Active track
                int activeWidth = (int)(w * value);
                if (activeWidth > 0)
                {
                    using (var brush = new SolidBrush(ActiveColor))
                    {
                        g.FillRoundRectangle(brush, x, y, activeWidth, trackHeight, (int)(2 * scale));
                    }
                }

                // Crisp outline border for clear visibility on light backgrounds
                Color outlineColor = IsDarkMode ? Color.FromArgb(50, 255, 255, 255) : Color.FromArgb(110, 80, 80, 80);
                using (var pen = new Pen(outlineColor, 1f))
                {
                    g.DrawRoundRectangle(pen, x, y, w - 1, trackHeight - 1, (int)(2 * scale));
                }

                // Thumb
                int thumbRadius = (int)(6 * scale);
                int thumbX = x + activeWidth;
                int thumbY = Height / 2;

                Color thumbBg = IsDarkMode ? Color.White : Color.FromArgb(245, 245, 245);
                using (var brush = new SolidBrush(thumbBg))
                {
                    g.FillEllipse(brush, thumbX - thumbRadius, thumbY - thumbRadius, thumbRadius * 2, thumbRadius * 2);
                }
                using (var pen = new Pen(ActiveColor, (int)(2 * scale)))
                {
                    g.DrawEllipse(pen, thumbX - thumbRadius, thumbY - thumbRadius, thumbRadius * 2, thumbRadius * 2);
                }
            }
        }

        protected override void OnMouseDown(MouseEventArgs e)
        {
            base.OnMouseDown(e);
            if (e.Button == MouseButtons.Left)
            {
                IsDragging = true;
                UpdateValueFromMouse(e.Location);
            }
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);
            if (IsDragging)
            {
                UpdateValueFromMouse(e.Location);
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

        private void UpdateValueFromMouse(Point pt)
        {
            float scale = DpiHelper.GetScale(this.Handle);
            float val;
            if (Orientation == Orientation.Vertical)
            {
                int yMargin = (int)(3 * scale);
                int h = Height - yMargin * 2;
                if (h <= 0) return;
                val = 1.0f - ((float)(pt.Y - yMargin) / h);
            }
            else
            {
                int x = (int)(6 * scale);
                int w = Width - x * 2;
                if (w <= 0) return;
                val = (float)(pt.X - x) / w;
            }
            val = Math.Max(0.0f, Math.Min(1.0f, val));
            if (value != val)
            {
                value = val;
                Invalidate();
                if (IsDragging && ValueChanged != null) ValueChanged(this, EventArgs.Empty);
            }
        }
    }
}
