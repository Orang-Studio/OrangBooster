using System;
using System.Runtime.InteropServices;
namespace OrangBooster
{
    static class DisplayConfig
    {
        const int ENUM_CURRENT_SETTINGS = -1;
        const uint DM_DISPLAYFREQUENCY = 0x400000;
        const uint CDS_UPDATEREGISTRY = 0x01;
        const uint DISPLAY_DEVICE_ATTACHED_TO_DESKTOP = 0x01;
        const int DISP_CHANGE_SUCCESSFUL = 0;
        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
        struct DISPLAY_DEVICE
        {
            public int cb;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 32)] public string DeviceName;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 128)] public string DeviceString;
            public uint StateFlags;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 128)] public string DeviceID;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 128)] public string DeviceKey;
        }
        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
        struct DEVMODE
        {
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 32)] public string dmDeviceName;
            public ushort dmSpecVersion;
            public ushort dmDriverVersion;
            public ushort dmSize;
            public ushort dmDriverExtra;
            public uint dmFields;
            public int dmPositionX;
            public int dmPositionY;
            public uint dmDisplayOrientation;
            public uint dmDisplayFixedOutput;
            public short dmColor;
            public short dmDuplex;
            public short dmYResolution;
            public short dmTTOption;
            public short dmCollate;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 32)] public string dmFormName;
            public ushort dmLogPixels;
            public uint dmBitsPerPel;
            public uint dmPelsWidth;
            public uint dmPelsHeight;
            public uint dmDisplayFlags;
            public uint dmDisplayFrequency;
            public uint dmICMMethod;
            public uint dmICMIntent;
            public uint dmMediaType;
            public uint dmDitherType;
            public uint dmReserved1;
            public uint dmReserved2;
            public uint dmPanningWidth;
            public uint dmPanningHeight;
        }
        [DllImport("user32.dll", CharSet = CharSet.Unicode)]
        static extern bool EnumDisplayDevices(string? lpDevice, uint iDevNum, ref DISPLAY_DEVICE lpDisplayDevice, uint dwFlags);
        [DllImport("user32.dll", CharSet = CharSet.Unicode)]
        static extern bool EnumDisplaySettings(string lpszDeviceName, int iModeNum, ref DEVMODE lpDevMode);
        [DllImport("user32.dll", CharSet = CharSet.Unicode)]
        static extern int ChangeDisplaySettingsEx(string lpszDeviceName, ref DEVMODE lpDevMode, IntPtr hwnd, uint dwflags, IntPtr lParam);
        public static int SetMaxRefreshAllMonitors()
        {
            int touched = 0;
            var dev = new DISPLAY_DEVICE { cb = Marshal.SizeOf<DISPLAY_DEVICE>() };
            for (uint i = 0; EnumDisplayDevices(null, i, ref dev, 0); i++)
            {
                if ((dev.StateFlags & DISPLAY_DEVICE_ATTACHED_TO_DESKTOP) == 0)
                {
                    dev.cb = Marshal.SizeOf<DISPLAY_DEVICE>();
                    continue;
                }
                string name = dev.DeviceName;
                var cur = new DEVMODE { dmSize = (ushort)Marshal.SizeOf<DEVMODE>() };
                if (!EnumDisplaySettings(name, ENUM_CURRENT_SETTINGS, ref cur))
                {
                    dev.cb = Marshal.SizeOf<DISPLAY_DEVICE>();
                    continue;
                }
                uint best = cur.dmDisplayFrequency;
                var mode = new DEVMODE { dmSize = (ushort)Marshal.SizeOf<DEVMODE>() };
                for (int m = 0; EnumDisplaySettings(name, m, ref mode); m++)
                {
                    if (mode.dmPelsWidth == cur.dmPelsWidth &&
                        mode.dmPelsHeight == cur.dmPelsHeight &&
                        mode.dmBitsPerPel == cur.dmBitsPerPel &&
                        mode.dmDisplayFrequency > best)
                    {
                        best = mode.dmDisplayFrequency;
                    }
                    mode.dmSize = (ushort)Marshal.SizeOf<DEVMODE>();
                }
                if (best > cur.dmDisplayFrequency)
                {
                    cur.dmDisplayFrequency = best;
                    cur.dmFields = DM_DISPLAYFREQUENCY;
                    int r = ChangeDisplaySettingsEx(name, ref cur, IntPtr.Zero, CDS_UPDATEREGISTRY, IntPtr.Zero);
                    if (r == DISP_CHANGE_SUCCESSFUL)
                    {
                        Logger.Info($"Display {name}: refresh set to {best} Hz");
                        touched++;
                    }
                    else Logger.Warn($"Display {name}: ChangeDisplaySettingsEx returned {r}");
                }
                else Logger.Info($"Display {name}: already at max {cur.dmDisplayFrequency} Hz");
                dev.cb = Marshal.SizeOf<DISPLAY_DEVICE>();
            }
            return touched;
}   }   }