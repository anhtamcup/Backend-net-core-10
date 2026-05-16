using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows;
using System.Windows.Interop;

namespace POS_App.Helpers
{
    public class MonitorUtil
    {
        [DllImport("user32.dll")]
        private static extern IntPtr MonitorFromWindow(IntPtr hwnd, uint dwFlags);

        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        private static extern bool GetMonitorInfo(IntPtr hMonitor, ref MONITORINFO lpmi);

        private const uint MONITOR_DEFAULTTONEAREST = 2;

        [StructLayout(LayoutKind.Sequential)]
        public struct RECT
        {
            public int Left, Top, Right, Bottom;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct MONITORINFO
        {
            public int cbSize;
            public RECT rcMonitor;
            public RECT rcWork;
            public int dwFlags;
        }

        public static RECT GetSecondaryScreen(Window window)
        {
            var hwnd = new WindowInteropHelper(window).Handle;

            var monitor = MonitorFromWindow(hwnd, MONITOR_DEFAULTTONEAREST);

            var info = new MONITORINFO();
            info.cbSize = Marshal.SizeOf(info);

            GetMonitorInfo(monitor, ref info);

            return info.rcWork;
        }
    }
}
