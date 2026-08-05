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
    }
}
