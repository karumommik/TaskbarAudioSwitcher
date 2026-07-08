using System;
using TaskbarAudioSwitcher.Native;

namespace TaskbarAudioSwitcher.Core
{
    internal static class DpiHelper
    {
        public static float GetScale(IntPtr hwnd)
        {
            try
            {
                return Win32.GetDpiForWindow(hwnd) / 96.0f;
            }
            catch
            {
                return 1.0f;
            }
        }
    }
}
