using System.Drawing;
using System.Drawing.Drawing2D;

namespace TaskbarAudioSwitcher.Controls
{
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
}
