using System;
using System.Collections.Generic;

namespace TaskbarAudioSwitcher.Core
{
    internal class AppSettings
    {
        public string DisplayDevices = "";
        public int ScreenIndex = 0;
        public string ScreenDeviceName = "";
        public string Alignment = "Right";
        public bool AlwaysOnTop = true;
        public bool MoveOnFullscreen = false;
        public bool FilterDevices = false;
        public string DeviceNicknames = "";
        public bool HideSilentApps = false;
        public int ScrollStep = 2;
        public bool ShowScreenMoveButton = false;
        public bool ShowMicrophoneButton = true;
        public bool MonitorMicrophoneState = true;

        private static string GetFilePath()
        {
            return System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "settings.txt");
        }

        public static AppSettings Load()
        {
            var s = new AppSettings();
            string path = GetFilePath();
            if (System.IO.File.Exists(path))
            {
                try
                {
                    var lines = System.IO.File.ReadAllLines(path);
                    foreach (var line in lines)
                    {
                        var parts = line.Split(new char[] { '=' }, 2);
                        if (parts.Length == 2)
                        {
                            string key = parts[0].Trim();
                            string val = parts[1].Trim();
                            if (key == "DisplayDevices") s.DisplayDevices = val;
                            else if (key == "ScreenIndex") int.TryParse(val, out s.ScreenIndex);
                            else if (key == "ScreenDeviceName") s.ScreenDeviceName = val;
                            else if (key == "Alignment") s.Alignment = val;
                            else if (key == "AlwaysOnTop") bool.TryParse(val, out s.AlwaysOnTop);
                            else if (key == "MoveOnFullscreen") bool.TryParse(val, out s.MoveOnFullscreen);
                            else if (key == "FilterDevices") bool.TryParse(val, out s.FilterDevices);
                            else if (key == "DeviceNicknames") s.DeviceNicknames = val;
                            else if (key == "HideSilentApps") bool.TryParse(val, out s.HideSilentApps);
                            else if (key == "ScrollStep") int.TryParse(val, out s.ScrollStep);
                            else if (key == "ShowScreenMoveButton") bool.TryParse(val, out s.ShowScreenMoveButton);
                            else if (key == "ShowMicrophoneButton") bool.TryParse(val, out s.ShowMicrophoneButton);
                            else if (key == "MonitorMicrophoneState") bool.TryParse(val, out s.MonitorMicrophoneState);
                        }
                    }
                }
                catch { }
            }
            return s;
        }

        public void Save()
        {
            try
            {
                var lines = new List<string>
                {
                    "DisplayDevices=" + DisplayDevices,
                    "ScreenIndex=" + ScreenIndex,
                    "ScreenDeviceName=" + ScreenDeviceName,
                    "Alignment=" + Alignment,
                    "AlwaysOnTop=" + AlwaysOnTop,
                    "MoveOnFullscreen=" + MoveOnFullscreen,
                    "FilterDevices=" + FilterDevices,
                    "DeviceNicknames=" + DeviceNicknames,
                    "HideSilentApps=" + HideSilentApps,
                    "ScrollStep=" + ScrollStep,
                    "ShowScreenMoveButton=" + ShowScreenMoveButton,
                    "ShowMicrophoneButton=" + ShowMicrophoneButton,
                    "MonitorMicrophoneState=" + MonitorMicrophoneState
                };
                System.IO.File.WriteAllLines(GetFilePath(), lines.ToArray());
            }
            catch { }
        }

        public string GetDeviceNickname(string devId, string defaultName)
        {
            if (!string.IsNullOrEmpty(DeviceNicknames))
            {
                var pairs = DeviceNicknames.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
                foreach (var pair in pairs)
                {
                    var parts = pair.Split(new char[] { ':' }, 2);
                    if (parts.Length == 2 && parts[0] == devId)
                    {
                        return parts[1];
                    }
                }
            }
            string clean = defaultName.Trim();
            return clean.Length > 0 ? clean.Substring(0, Math.Min(3, clean.Length)).ToUpper() : "";
        }
    }
}
