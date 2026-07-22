using System;
using System.Runtime.InteropServices;

namespace TaskbarAudioSwitcher.Native
{
    // ==========================================
    // CORE AUDIO & IPOLICYCONFIG COM INTERFACES
    // ==========================================

    [Guid("A95664D2-9614-4F35-A746-DE8DB63617E6"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    internal interface IMMDeviceEnumerator
    {
        [PreserveSig] int EnumAudioEndpoints(int dataFlow, int stateMask, out IMMDeviceCollection devices);
        [PreserveSig] int GetDefaultAudioEndpoint(int dataFlow, int role, out IMMDevice endpoint);
        [PreserveSig] int GetDevice([MarshalAs(UnmanagedType.LPWStr)] string id, out IMMDevice device);
        [PreserveSig] int RegisterEndpointNotificationCallback(IntPtr client);
        [PreserveSig] int UnregisterEndpointNotificationCallback(IntPtr client);
    }

    [Guid("0BD7A1BE-7A1A-44DB-8397-CC5392387B5E"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    internal interface IMMDeviceCollection
    {
        [PreserveSig] int GetCount(out uint pcDevices);
        [PreserveSig] int Item(uint nDevice, out IMMDevice ppDevice);
    }

    [Guid("D666063F-1587-4E43-81F1-B948E807363F"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    internal interface IMMDevice
    {
        [PreserveSig] int Activate(ref Guid iid, int dwClsCtx, IntPtr pActivationParams, [MarshalAs(UnmanagedType.IUnknown)] out object ppInterface);
        [PreserveSig] int OpenPropertyStore(int stgmAccess, out IPropertyStore ppProperties);
        [PreserveSig] int GetId([MarshalAs(UnmanagedType.LPWStr)] out string ppstrId);
        [PreserveSig] int GetState(out int pdwState);
    }

    [Guid("886D8EEB-8CF2-4446-8D02-CDBA1DBDCF99"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    internal interface IPropertyStore
    {
        [PreserveSig] int GetCount(out uint propertyCount);
        [PreserveSig] int GetAt(uint propertyIndex, out PROPERTYKEY key);
        [PreserveSig] int GetValue(ref PROPERTYKEY key, out PROPVARIANT pv);
        [PreserveSig] int SetValue(ref PROPERTYKEY key, ref PROPVARIANT pv);
        [PreserveSig] int Commit();
    }

    [Guid("5CDF2C82-841E-4546-9722-0CF74078229A"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    internal interface IAudioEndpointVolume
    {
        [PreserveSig] int RegisterControlChangeNotify(IntPtr pNotify);
        [PreserveSig] int UnregisterControlChangeNotify(IntPtr pNotify);
        [PreserveSig] int GetChannelCount(out int pnChannelCount);
        [PreserveSig] int SetMasterVolumeLevel(float fLevelDB, ref Guid pguidEventContext);
        [PreserveSig] int SetMasterVolumeLevelScalar(float fLevel, ref Guid pguidEventContext);
        [PreserveSig] int GetMasterVolumeLevel(out float pfLevelDB);
        [PreserveSig] int GetMasterVolumeLevelScalar(out float pfLevel);
        [PreserveSig] int SetChannelVolumeLevel(int nChannel, float fLevelDB, ref Guid pguidEventContext);
        [PreserveSig] int SetChannelVolumeLevelScalar(int nChannel, float fLevel, ref Guid pguidEventContext);
        [PreserveSig] int GetChannelVolumeLevel(int nChannel, out float pfLevelDB);
        [PreserveSig] int GetChannelVolumeLevelScalar(int nChannel, out float pfLevel);
        [PreserveSig] int SetMute([MarshalAs(UnmanagedType.Bool)] bool bMute, ref Guid pguidEventContext);
        [PreserveSig] int GetMute([MarshalAs(UnmanagedType.Bool)] out bool pbMute);
        [PreserveSig] int GetVolumeStepInfo(out uint pnStep, out uint pnStepCount);
        [PreserveSig] int VolumeStepDown(ref Guid pguidEventContext);
        [PreserveSig] int VolumeStepUp(ref Guid pguidEventContext);
        [PreserveSig] int QueryHardwareSupport(out uint pdwHardwareSupportMask);
        [PreserveSig] int GetVolumeRange(out float pflVolumeMindB, out float pflVolumeMaxdB, out float pflVolumeIncrementdB);
    }

    [StructLayout(LayoutKind.Sequential)]
    internal struct PROPERTYKEY
    {
        public Guid fmtid;
        public uint pid;
        public PROPERTYKEY(Guid fmtid, uint pid) { this.fmtid = fmtid; this.pid = pid; }
    }

    [StructLayout(LayoutKind.Sequential)]
    internal struct PROPVARIANT
    {
        public ushort vt;
        public ushort wReserved1;
        public ushort wReserved2;
        public ushort wReserved3;
        public IntPtr pointerVal;
    }

    [Guid("f8679f50-850a-41cf-9c72-430f290290c8"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    internal interface IPolicyConfig
    {
        [PreserveSig] int GetMixFormat([MarshalAs(UnmanagedType.LPWStr)] string pszDeviceName, out IntPtr ppFormat);
        [PreserveSig] int GetDeviceFormat([MarshalAs(UnmanagedType.LPWStr)] string pszDeviceName, bool bDefault, out IntPtr ppFormat);
        [PreserveSig] int ResetDeviceFormat([MarshalAs(UnmanagedType.LPWStr)] string pszDeviceName);
        [PreserveSig] int SetDeviceFormat([MarshalAs(UnmanagedType.LPWStr)] string pszDeviceName, IntPtr pEndpointFormat, IntPtr MixFormat);
        [PreserveSig] int GetProcessingPeriod([MarshalAs(UnmanagedType.LPWStr)] string pszDeviceName, bool bDefault, out long pmftDefaultPeriod, out long pmftMinimumPeriod);
        [PreserveSig] int SetProcessingPeriod([MarshalAs(UnmanagedType.LPWStr)] string pszDeviceName, long pmftPeriod);
        [PreserveSig] int GetShareMode([MarshalAs(UnmanagedType.LPWStr)] string pszDeviceName, out int pMode);
        [PreserveSig] int SetShareMode([MarshalAs(UnmanagedType.LPWStr)] string pszDeviceName, int mode);
        [PreserveSig] int GetPropertyValue([MarshalAs(UnmanagedType.LPWStr)] string pszDeviceName, bool bFxStore, IntPtr key, IntPtr pv);
        [PreserveSig] int SetPropertyValue([MarshalAs(UnmanagedType.LPWStr)] string pszDeviceName, bool bFxStore, IntPtr key, IntPtr pv);
        [PreserveSig] int SetDefaultEndpoint([MarshalAs(UnmanagedType.LPWStr)] string pszDeviceName, int role);
        [PreserveSig] int SetEndpointVisibility([MarshalAs(UnmanagedType.LPWStr)] string wszDeviceId, bool bVisible);
    }

    [ComImport, Guid("870af99c-171d-4f9e-af0d-e63df40c2bc9")]
    internal class CPolicyConfigClient { }

    [ComImport, Guid("BCDE0395-E52F-467C-8E3D-C4579291692E")]
    internal class MMDeviceEnumeratorCom { }

    // ==========================================
    // WASAPI SESSION COM INTERFACES
    // ==========================================

    [ComImport]
    [Guid("77AA99A0-1BD6-484F-8BC7-2C654C9A9B6F")]
    [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    internal interface IAudioSessionManager2
    {
        int GetAudioSessionControl(IntPtr AudioSessionGuid, uint StreamFlags, out object SessionControl);
        int GetSimpleAudioVolume(IntPtr AudioSessionGuid, uint StreamFlags, out object AudioVolume);
        int GetSessionEnumerator(out IAudioSessionEnumerator SessionEnum);
        int RegisterSessionNotification(IntPtr SessionNotification);
        int UnregisterSessionNotification(IntPtr SessionNotification);
        int RegisterDuckNotification(IntPtr SessionID, IntPtr DuckNotification);
        int UnregisterDuckNotification(IntPtr DuckNotification);
    }

    [ComImport]
    [Guid("E2F5BB11-0570-40CA-ACDD-3AA01277DEE8")]
    [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    internal interface IAudioSessionEnumerator
    {
        [PreserveSig] int GetCount(out int SessionCount);
        [PreserveSig] int GetSession(int SessionIndex, out IAudioSessionControl Session);
    }

    [ComImport]
    [Guid("F4B1A599-7266-4319-A8CA-E70ACB11E8CD")]
    [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    internal interface IAudioSessionControl
    {
        [PreserveSig] int GetState(out int State);
        [PreserveSig] int GetDisplayName([MarshalAs(UnmanagedType.LPWStr)] out string DisplayName);
        [PreserveSig] int SetDisplayName([MarshalAs(UnmanagedType.LPWStr)] string DisplayName, ref Guid EventContext);
        [PreserveSig] int GetIconPath([MarshalAs(UnmanagedType.LPWStr)] out string IconPath);
        [PreserveSig] int SetIconPath([MarshalAs(UnmanagedType.LPWStr)] string IconPath, ref Guid EventContext);
        [PreserveSig] int GetGroupingParam(out Guid GroupingParam);
        [PreserveSig] int SetGroupingParam(ref Guid GroupingParam, ref Guid EventContext);
        [PreserveSig] int RegisterAudioSessionNotification(IntPtr SessionNotification);
        [PreserveSig] int UnregisterAudioSessionNotification(IntPtr SessionNotification);
    }

    [ComImport]
    [Guid("bfb7ff88-7239-4fc9-8fa2-07c950be9c6d")]
    [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    internal interface IAudioSessionControl2
    {
        [PreserveSig] int GetState(out int State);
        [PreserveSig] int GetDisplayName([MarshalAs(UnmanagedType.LPWStr)] out string DisplayName);
        [PreserveSig] int SetDisplayName([MarshalAs(UnmanagedType.LPWStr)] string DisplayName, ref Guid EventContext);
        [PreserveSig] int GetIconPath([MarshalAs(UnmanagedType.LPWStr)] out string IconPath);
        [PreserveSig] int SetIconPath([MarshalAs(UnmanagedType.LPWStr)] string IconPath, ref Guid EventContext);
        [PreserveSig] int GetGroupingParam(out Guid GroupingParam);
        [PreserveSig] int SetGroupingParam(ref Guid GroupingParam, ref Guid EventContext);
        [PreserveSig] int RegisterAudioSessionNotification(IntPtr SessionNotification);
        [PreserveSig] int UnregisterAudioSessionNotification(IntPtr SessionNotification);

        [PreserveSig] int GetSessionIdentifier(out IntPtr SessionIdentifier);
        [PreserveSig] int GetSessionInstanceIdentifier(out IntPtr SessionInstanceIdentifier);
        [PreserveSig] int GetProcessId(out int ProcessId);
        [PreserveSig] int IsSystemSoundsSession();
        [PreserveSig] int SetDuckingPreference(bool optOut);
    }

    [ComImport]
    [Guid("87CE5498-68D6-44E5-9215-6DA47EF883D8")]
    [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    internal interface ISimpleAudioVolume
    {
        [PreserveSig] int SetMasterVolume(float fLevel, ref Guid EventContext);
        [PreserveSig] int GetMasterVolume(out float pfLevel);
        [PreserveSig] int SetMute(bool bMute, ref Guid EventContext);
        [PreserveSig] int GetMute(out bool pbMute);
    }

    internal static class ComNative
    {
        [DllImport("ole32.dll")]
        public static extern void PropVariantClear(ref PROPVARIANT pvar);
    }

    internal static class Combase
    {
        [DllImport("combase.dll", PreserveSig = true)]
        public static extern int RoGetActivationFactory(
            IntPtr activatableClassId,
            [In] ref Guid iid,
            out IntPtr factory);

        [DllImport("combase.dll", CallingConvention = CallingConvention.StdCall)]
        public static extern int WindowsCreateString(
            [MarshalAs(UnmanagedType.LPWStr)] string sourceString,
            uint length,
            out IntPtr hstring);

        [DllImport("combase.dll", CallingConvention = CallingConvention.StdCall)]
        public static extern int WindowsDeleteString(IntPtr hstring);

        [DllImport("combase.dll", CallingConvention = CallingConvention.StdCall)]
        public static extern IntPtr WindowsGetStringRawBuffer(IntPtr hstring, out uint length);
    }

    internal static class AudioPolicyConfigHelper
    {
        private static IntPtr _factory21H2 = IntPtr.Zero;
        private static IntPtr _factoryDownlevel = IntPtr.Zero;
        private static bool _initialized = false;
        private static bool _is21H2OrNewer = false;

        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        delegate int SetPersistedDefaultAudioEndpointDelegate(IntPtr thisPtr, uint processId, int flow, int role, IntPtr deviceId);

        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        delegate int GetPersistedDefaultAudioEndpointDelegate(IntPtr thisPtr, uint processId, int flow, int role, out IntPtr deviceId);

        private static SetPersistedDefaultAudioEndpointDelegate? _set21H2;
        private static GetPersistedDefaultAudioEndpointDelegate? _get21H2;

        private static SetPersistedDefaultAudioEndpointDelegate? _setDownlevel;
        private static GetPersistedDefaultAudioEndpointDelegate? _getDownlevel;

        private static void Log(string message)
        {
            try
            {
                string path = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "mixerlog.txt");
                System.IO.File.AppendAllText(path, string.Format("[APCH] {0:yyyy-MM-dd HH:mm:ss} - {1}\n", DateTime.Now, message));
            }
            catch {}
        }

        private static void Initialize()
        {
            if (_initialized) return;
            try
            {
                var version = Environment.OSVersion.Version;
                _is21H2OrNewer = version.Major >= 10 && version.Build >= 22000;
                Log(string.Format("Initialize: Major={0}, Build={1}, is21H2OrNewer={2}", version.Major, version.Build, _is21H2OrNewer));

                string className = "Windows.Media.Internal.AudioPolicyConfig";
                IntPtr hClass = IntPtr.Zero;
                int hrString = Combase.WindowsCreateString(className, (uint)className.Length, out hClass);
                Log(string.Format("Initialize: WindowsCreateString hr=0x{0:X8}", hrString));

                if (hrString == 0 && hClass != IntPtr.Zero)
                {
                    try
                    {
                        if (_is21H2OrNewer)
                        {
                            Guid iid21H2 = new Guid("ab3d4648-e242-459f-b02f-541c70306324");
                            IntPtr pFactory = IntPtr.Zero;
                            int hr = Combase.RoGetActivationFactory(hClass, ref iid21H2, out pFactory);
                            Log(string.Format("Initialize: RoGetActivationFactory 21H2 hr=0x{0:X8}, pFactory=0x{1:X}", hr, pFactory.ToInt64()));
                            if (hr == 0 && pFactory != IntPtr.Zero)
                            {
                                _factory21H2 = pFactory;
                                IntPtr pVtbl = Marshal.ReadIntPtr(_factory21H2);
                                
                                IntPtr pSet = Marshal.ReadIntPtr(pVtbl, 25 * IntPtr.Size);
                                IntPtr pGet = Marshal.ReadIntPtr(pVtbl, 26 * IntPtr.Size);
                                
                                _set21H2 = Marshal.GetDelegateForFunctionPointer<SetPersistedDefaultAudioEndpointDelegate>(pSet);
                                _get21H2 = Marshal.GetDelegateForFunctionPointer<GetPersistedDefaultAudioEndpointDelegate>(pGet);
                                
                                Log("Initialize: 21H2 function delegates compiled successfully.");
                            }
                        }
                        else
                        {
                            Guid iidDownlevel = new Guid("2a59116d-6c4f-45e0-a74f-707e3fef9258");
                            IntPtr pFactory = IntPtr.Zero;
                            int hr = Combase.RoGetActivationFactory(hClass, ref iidDownlevel, out pFactory);
                            Log(string.Format("Initialize: RoGetActivationFactory Downlevel hr=0x{0:X8}, pFactory=0x{1:X}", hr, pFactory.ToInt64()));
                            if (hr == 0 && pFactory != IntPtr.Zero)
                            {
                                _factoryDownlevel = pFactory;
                                IntPtr pVtbl = Marshal.ReadIntPtr(_factoryDownlevel);
                                
                                IntPtr pSet = Marshal.ReadIntPtr(pVtbl, 22 * IntPtr.Size);
                                IntPtr pGet = Marshal.ReadIntPtr(pVtbl, 23 * IntPtr.Size);
                                
                                _setDownlevel = Marshal.GetDelegateForFunctionPointer<SetPersistedDefaultAudioEndpointDelegate>(pSet);
                                _getDownlevel = Marshal.GetDelegateForFunctionPointer<GetPersistedDefaultAudioEndpointDelegate>(pGet);
                                
                                Log("Initialize: Downlevel function delegates compiled successfully.");
                            }
                        }
                    }
                    finally
                    {
                        Combase.WindowsDeleteString(hClass);
                    }
                }
            }
            catch (Exception ex)
            {
                Log("Initialize Error: " + ex.ToString());
            }
            _initialized = true;
        }

        public static string? GetApplicationOutputDevice(uint processId)
        {
            Initialize();
            string? deviceId = null;
            try
            {
                // Get all candidate PIDs (the given processId + all PIDs with the same ProcessName)
                System.Collections.Generic.List<uint> pidsToTry = new System.Collections.Generic.List<uint> { processId };
                try
                {
                    using (var targetProc = System.Diagnostics.Process.GetProcessById((int)processId))
                    {
                        string procName = targetProc.ProcessName;
                        foreach (var p in System.Diagnostics.Process.GetProcessesByName(procName))
                        {
                            uint pid = (uint)p.Id;
                            if (!pidsToTry.Contains(pid))
                            {
                                pidsToTry.Add(pid);
                            }
                        }
                    }
                }
                catch { }

                foreach (uint pid in pidsToTry)
                {
                    IntPtr pDeviceId = IntPtr.Zero;
                    int hr = -1;
                    if (_is21H2OrNewer && _factory21H2 != IntPtr.Zero && _get21H2 != null)
                    {
                        hr = _get21H2(_factory21H2, pid, 0, 0, out pDeviceId); // 0 = eRender, 0 = eConsole
                        if (hr == 0 && pDeviceId != IntPtr.Zero)
                        {
                            uint len = 0;
                            IntPtr rawBuffer = Combase.WindowsGetStringRawBuffer(pDeviceId, out len);
                            if (rawBuffer != IntPtr.Zero && len > 0)
                            {
                                deviceId = Marshal.PtrToStringUni(rawBuffer, (int)len);
                            }
                            Combase.WindowsDeleteString(pDeviceId);
                        }
                        
                        if (deviceId != null && deviceId.StartsWith(@"\\?\SWD#MMDEVAPI#", StringComparison.OrdinalIgnoreCase))
                        {
                            int start = deviceId.IndexOf('{');
                            if (start >= 0)
                            {
                                int end = deviceId.IndexOf('}', start);
                                if (end > start)
                                {
                                    int nextStart = deviceId.IndexOf('{', end + 1);
                                    if (nextStart >= 0)
                                    {
                                        int nextEnd = deviceId.IndexOf('}', nextStart);
                                        if (nextEnd > nextStart)
                                        {
                                            deviceId = deviceId.Substring(start, nextEnd - start + 1);
                                        }
                                    }
                                }
                            }
                        }
                        if (!string.IsNullOrEmpty(deviceId))
                        {
                            Log(string.Format("GetApplicationOutputDevice (21H2+): PID={0}, hr=0x{1:X8}, DevId={2}", pid, hr, deviceId));
                            break;
                        }
                    }
                    else if (_factoryDownlevel != IntPtr.Zero && _getDownlevel != null)
                    {
                        hr = _getDownlevel(_factoryDownlevel, pid, 0, 0, out pDeviceId); // 0 = eRender, 0 = eConsole
                        if (hr == 0 && pDeviceId != IntPtr.Zero)
                        {
                            uint len = 0;
                            IntPtr rawBuffer = Combase.WindowsGetStringRawBuffer(pDeviceId, out len);
                            if (rawBuffer != IntPtr.Zero && len > 0)
                            {
                                deviceId = Marshal.PtrToStringUni(rawBuffer, (int)len);
                            }
                            Combase.WindowsDeleteString(pDeviceId);
                        }
                        if (!string.IsNullOrEmpty(deviceId))
                        {
                            Log(string.Format("GetApplicationOutputDevice (Downlevel): PID={0}, hr=0x{1:X8}, DevId={2}", pid, hr, deviceId));
                            break;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Log(string.Format("GetApplicationOutputDevice Error: PID={0}, {1}", processId, ex.ToString()));
            }
            return deviceId;
        }

        public static int SetApplicationOutputDevice(uint processId, string? deviceId)
        {
            Initialize();
            Log(string.Format("SetApplicationOutputDevice Request: PID={0}, TargetDev={1}", processId, deviceId ?? "RESET"));
            
            // Collect all PIDs for this application (the session PID + all processes matching process name)
            System.Collections.Generic.List<uint> targetPids = new System.Collections.Generic.List<uint> { processId };
            try
            {
                using (var proc = System.Diagnostics.Process.GetProcessById((int)processId))
                {
                    string procName = proc.ProcessName;
                    foreach (var p in System.Diagnostics.Process.GetProcessesByName(procName))
                    {
                        uint pId = (uint)p.Id;
                        if (!targetPids.Contains(pId))
                        {
                            targetPids.Add(pId);
                        }
                    }
                }
            }
            catch { }

            int hr = -1;
            try
            {
                if (_is21H2OrNewer && _factory21H2 != IntPtr.Zero && _set21H2 != null)
                {
                    string? devicePath = null;
                    if (!string.IsNullOrEmpty(deviceId))
                    {
                        // Windows 11 expects the SWD MMDEVAPI path
                        devicePath = $@"\\?\SWD#MMDEVAPI#{deviceId}#{{e6327cad-dcec-4949-ae8a-991e976a79d2}}";
                    }

                    IntPtr hString = IntPtr.Zero;
                    if (!string.IsNullOrEmpty(devicePath))
                    {
                        Combase.WindowsCreateString(devicePath, (uint)devicePath.Length, out hString);
                    }

                    try
                    {
                        foreach (uint pid in targetPids)
                        {
                            for (int role = 0; role <= 2; role++)
                            {
                                int res = _set21H2(_factory21H2, pid, 0, role, hString);
                                if (res == 0) hr = 0;
                            }
                            Log(string.Format("SetPersistedDefaultAudioEndpoint (21H2+): PID={0}, roles 0-2 hr=0x{1:X8}", pid, hr));
                        }
                    }
                    finally
                    {
                        if (hString != IntPtr.Zero)
                        {
                            Combase.WindowsDeleteString(hString);
                        }
                    }
                }
                else if (_factoryDownlevel != IntPtr.Zero && _setDownlevel != null)
                {
                    IntPtr hString = IntPtr.Zero;
                    if (!string.IsNullOrEmpty(deviceId))
                    {
                        Combase.WindowsCreateString(deviceId, (uint)deviceId.Length, out hString);
                    }

                    try
                    {
                        foreach (uint pid in targetPids)
                        {
                            for (int role = 0; role <= 2; role++)
                            {
                                int res = _setDownlevel(_factoryDownlevel, pid, 0, role, hString);
                                if (res == 0) hr = 0;
                            }
                            Log(string.Format("SetPersistedDefaultAudioEndpoint (Downlevel): PID={0}, roles 0-2 hr=0x{1:X8}", pid, hr));
                        }
                    }
                    finally
                    {
                        if (hString != IntPtr.Zero)
                        {
                            Combase.WindowsDeleteString(hString);
                        }
                    }
                }
                else
                {
                    Log("SetApplicationOutputDevice: Delegate or factory is null!");
                }
            }
            catch (Exception ex)
            {
                Log(string.Format("SetApplicationOutputDevice Error: PID={0}, {1}", processId, ex.ToString()));
                hr = Marshal.GetHRForException(ex);
            }
            return hr;
        }
    }
}
