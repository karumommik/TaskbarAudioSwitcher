using System;
using System.Drawing;
using System.Runtime.InteropServices;
using Microsoft.Win32;

namespace TaskbarAudioSwitcher.Core
{
    internal static class ThemeHelper
    {
        [DllImport("dwmapi.dll", EntryPoint = "DwmGetColorizationColor", PreserveSig = true)]
        private static extern int DwmGetColorizationColor(out uint pcrColorization, [MarshalAs(UnmanagedType.Bool)] out bool pfOpaqueBlend);

        /// <summary>
        /// Determines if Windows taskbar / system theme is set to Dark Mode.
        /// Primary check is SystemUsesLightTheme (taskbar theme), fallback to AppsUseLightTheme.
        /// </summary>
        public static bool IsDarkMode()
        {
            try
            {
                using (var key = Registry.CurrentUser.OpenSubKey(@"Software\Microsoft\Windows\CurrentVersion\Themes\Personalize"))
                {
                    if (key != null)
                    {
                        var systemVal = key.GetValue("SystemUsesLightTheme");
                        if (systemVal != null && systemVal is int sysInt)
                        {
                            return sysInt == 0;
                        }

                        var appVal = key.GetValue("AppsUseLightTheme");
                        if (appVal != null && appVal is int appInt)
                        {
                            return appInt == 0;
                        }
                    }
                }
            }
            catch { }

            return true; // Default to Dark mode
        }

        /// <summary>
        /// Retrieves the active Windows Accent Color from DWM or Registry.
        /// Falls back to default Windows Blue (#0078D7) if unavailable.
        /// </summary>
        public static Color GetAccentColor()
        {
            try
            {
                // 1. Try DWM API first
                int hr = DwmGetColorizationColor(out uint colorization, out _);
                if (hr == 0 && colorization != 0)
                {
                    byte r = (byte)((colorization >> 16) & 0xFF);
                    byte g = (byte)((colorization >> 8) & 0xFF);
                    byte b = (byte)(colorization & 0xFF);
                    return Color.FromArgb(255, r, g, b);
                }

                // 2. Try Explorer AccentColorMenu (stored as ABGR DWORD: 0xAABBGGRR)
                using (var key = Registry.CurrentUser.OpenSubKey(@"Software\Microsoft\Windows\CurrentVersion\Explorer\Accent"))
                {
                    if (key != null)
                    {
                        var val = key.GetValue("AccentColorMenu");
                        if (val is int intVal)
                        {
                            uint uVal = (uint)intVal;
                            byte b = (byte)((uVal >> 16) & 0xFF);
                            byte g = (byte)((uVal >> 8) & 0xFF);
                            byte r = (byte)(uVal & 0xFF);
                            return Color.FromArgb(255, r, g, b);
                        }
                    }
                }

                // 3. Try DWM Registry ColorizationColor (stored as ARGB DWORD)
                using (var key = Registry.CurrentUser.OpenSubKey(@"Software\Microsoft\Windows\DWM"))
                {
                    if (key != null)
                    {
                        var val = key.GetValue("ColorizationColor");
                        if (val is int intVal)
                        {
                            uint uVal = (uint)intVal;
                            byte r = (byte)((uVal >> 16) & 0xFF);
                            byte g = (byte)((uVal >> 8) & 0xFF);
                            byte b = (byte)(uVal & 0xFF);
                            return Color.FromArgb(255, r, g, b);
                        }
                    }
                }
            }
            catch { }

            // Default Windows Accent Blue (#0078D7)
            return Color.FromArgb(0, 120, 215);
        }

        /// <summary>
        /// Extracts the most dominant vibrant color from an image/icon bitmap.
        /// Falls back to defaultAccent if no vibrant color is found.
        /// </summary>
        public static Color GetDominantColor(Image? image, Color defaultAccent)
        {
            if (image == null) return defaultAccent;

            try
            {
                using (Bitmap bmp = new Bitmap(image, new Size(24, 24)))
                {
                    int count = 0;
                    float maxSat = 0f;
                    Color bestColor = defaultAccent;

                    for (int y = 0; y < bmp.Height; y++)
                    {
                        for (int x = 0; x < bmp.Width; x++)
                        {
                            Color pixel = bmp.GetPixel(x, y);
                            if (pixel.A < 160) continue; // Ignore transparent/semi-transparent pixels

                            float sat = pixel.GetSaturation();
                            float bri = pixel.GetBrightness();

                            // Filter out near-black, near-white, and low-saturation pixels
                            if (sat > 0.18f && bri > 0.15f && bri < 0.92f)
                            {
                                if (sat > maxSat)
                                {
                                    maxSat = sat;
                                    bestColor = pixel;
                                }
                                count++;
                            }
                        }
                    }

                    if (count > 0 && maxSat > 0.18f)
                    {
                        return Color.FromArgb(255, bestColor.R, bestColor.G, bestColor.B);
                    }
                }
            }
            catch { }

            return defaultAccent;
        }
    }
}
