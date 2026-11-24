using System.Runtime.InteropServices;
using System.Windows;

namespace Autokolcsonzo.Views;

internal static class MonitorWorkArea
{
    internal static Rect? Get(nint window)
    {
        if (window == 0) return null;

        const uint nearestMonitor = 2;
        nint monitor = MonitorFromWindow(window, nearestMonitor);
        var info = new MonitorInfo { Size = Marshal.SizeOf<MonitorInfo>() };
        if (monitor == 0 || !GetMonitorInfo(monitor, ref info)) return null;

        return new Rect(new Point(info.WorkArea.Left, info.WorkArea.Top), new Point(info.WorkArea.Right, info.WorkArea.Bottom));
    }

    [DllImport("user32.dll")]
    private static extern nint MonitorFromWindow(nint window, uint flags);

    [DllImport("user32.dll", EntryPoint = "GetMonitorInfoW")]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool GetMonitorInfo(nint monitor, ref MonitorInfo info);

    [StructLayout(LayoutKind.Sequential)]
    private struct MonitorInfo
    {
        public int Size;
        public NativeRect Monitor;
        public NativeRect WorkArea;
        public uint Flags;
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct NativeRect
    {
        public int Left;
        public int Top;
        public int Right;
        public int Bottom;
    }
}
