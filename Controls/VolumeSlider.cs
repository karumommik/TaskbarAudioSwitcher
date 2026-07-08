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
            int trackHeight = (int)(4 * scale);
            int y = (Height - trackHeight) / 2;
            int x = (int)(6 * scale);
            int w = Width - x * 2;

            // Inactive track
            using (var brush = new SolidBrush(InactiveColor))
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

            // Thumb
            int thumbRadius = (int)(6 * scale);
            int thumbX = x + activeWidth;
            int thumbY = Height / 2;

            using (var brush = new SolidBrush(Color.White))
            {
                g.FillEllipse(brush, thumbX - thumbRadius, thumbY - thumbRadius, thumbRadius * 2, thumbRadius * 2);
            }
            using (var pen = new Pen(ActiveColor, (int)(2 * scale)))
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
            float scale = DpiHelper.GetScale(this.Handle);
            int x = (int)(6 * scale);
            int w = Width - x * 2;
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
}
