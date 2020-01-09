#if WINDOWS && !MONOGAME
using System;
using System.Runtime.InteropServices;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace FEGame
{
    class FullscreenService : FEXNA.Rendering.IFullscreenService
    {
        private Game Game;
        private bool IsFullscreen = false;

        public FullscreenService(Game game)
        {
            Game = game;
        }

        public void SetFullscreen(bool value, GraphicsDeviceManager graphics)
        {
            // Resize window
            if (IsFullscreen != value)
            {
                IsFullscreen = value;
#if !MONOGAME
                if (IsFullscreen)
                    FullScreen();
                else
                    Restore();
#elif MONOMAC || WINDOWS
                bool regainFocus = IsFullscreen != graphics.IsFullScreen;
                graphics.IsFullScreen = IsFullscreen;
#if MONOMAC
                // going to or from fullscreen loses focus on the window, it's still on the program? //@Yeti
                //if (regainFocus)
                //    Game.Window.MakeCurrent();
#endif
#endif
            }
        }

        public bool NeedsRefresh(bool value)
        {
            return IsFullscreen != value;
        }

        public int WindowWidth(GraphicsDevice device)
        {
#if !MONOGAME
            return FEGame.Fullscreen.ScreenX(Game.Window.Handle);
#else
            return device.DisplayMode.Width;
#endif
        }
        public int WindowHeight(GraphicsDevice device)
        {
#if !MONOGAME
            return FEGame.Fullscreen.ScreenY(Game.Window.Handle);
#else
            return device.DisplayMode.Height;
#endif
        }

        public void MinimizeFullscreen(Game game)
        {
#if !MONOGAME
                System.Windows.Forms.Form.FromHandle(Game.Window.Handle).FindForm().WindowState =
                    System.Windows.Forms.FormWindowState.Minimized;
#elif MONOMAC
                    game.Window.WindowState = MonoMac.OpenGL.WindowState.Minimized;
#endif
        }

#if WINDOWS && !MONOGAME
        private void FullScreen()
        {
            Fullscreen.fullscreen(Game.Window.Handle);
        }
        private void Restore()
        {
            System.Windows.Forms.Form.FromHandle(Game.Window.Handle).FindForm().WindowState =
                System.Windows.Forms.FormWindowState.Normal;
            System.Windows.Forms.Form.FromHandle(Game.Window.Handle).FindForm().FormBorderStyle =
                System.Windows.Forms.FormBorderStyle.FixedDialog;
            System.Windows.Forms.Form.FromHandle(Game.Window.Handle).FindForm().TopMost = false;
        }
#endif
    }

    static class Fullscreen
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
