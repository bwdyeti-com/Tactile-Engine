#if WINDOWS && !MONOGAME
using System;
using System.Runtime.InteropServices;

namespace FE7x
{
    class Fullscreen
    {
        const int MONITOR_DEFAULTTONULL = 0;
        const int MONITOR_DEFAULTTOPRIMARY = 1;
        const int MONITOR_DEFAULTTONEAREST = 2;

        public static void fullscreen(IntPtr hWnd)
        {
            System.Windows.Forms.Form.FromHandle(hWnd).FindForm().WindowState =
                System.Windows.Forms.FormWindowState.Maximized;
            System.Windows.Forms.Form.FromHandle(hWnd).FindForm().FormBorderStyle =
                System.Windows.Forms.FormBorderStyle.None;
            System.Windows.Forms.Form.FromHandle(hWnd).FindForm().TopMost = true;
            NativeMethods.SetWindowPos(hWnd, HWND_TOP, 0, 0, PrimaryScreenX(hWnd), PrimaryScreenY(hWnd), SWP_SHOWWINDOW);
        }

        private const int SM_CXSCREEN = 0;
        private const int SM_CYSCREEN = 1;
        private readonly static IntPtr HWND_TOP = IntPtr.Zero;
        private const int SWP_SHOWWINDOW = 64;

        public static int ScreenX(IntPtr hWnd)
        {
            IntPtr handle = NativeMethods.MonitorFromWindow(hWnd, MONITOR_DEFAULTTONEAREST);
            MONITORINFOEX info = new MONITORINFOEX();
            NativeMethods.GetMonitorInfo(handle, info);
            return info.rcMonitor.right - info.rcMonitor.left;
        }
        public static int ScreenY(IntPtr hWnd)
        {
            IntPtr handle = NativeMethods.MonitorFromWindow(hWnd, MONITOR_DEFAULTTONEAREST);
            MONITORINFOEX info = new MONITORINFOEX();
            NativeMethods.GetMonitorInfo(handle, info);
            return info.rcMonitor.bottom - info.rcMonitor.top;
        }

        private static int PrimaryScreenX(IntPtr hWnd)
        {
            return NativeMethods.GetSystemMetrics(SM_CXSCREEN);
        }
        private static int PrimaryScreenY(IntPtr hWnd)
        {
            return NativeMethods.GetSystemMetrics(SM_CYSCREEN);
        }
    }
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto, Pack = 4)]
    public class MONITORINFOEX
    {
        public int cbSize = Marshal.SizeOf(typeof(MONITORINFOEX));
        public RECT rcMonitor = new RECT();
        public RECT rcWork = new RECT();
        public int dwFlags = 0;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 32)]
        public char[] szDevice = new char[32];
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct POINTSTRUCT
    {
        public int x;
        public int y;
        public POINTSTRUCT(int x, int y)
        {
            this.x = x;
            this.y = y;
        }
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct RECT
    {
        public int left;
        public int top;
        public int right;
        public int bottom;
    }
}
#endif