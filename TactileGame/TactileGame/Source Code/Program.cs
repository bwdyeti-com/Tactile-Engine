#if WINDOWS // && DEBUG
#define LOGGING
#endif

#if __ANDROID__
using Android.App;
using Android.Content.PM;
using Android.OS;
using Android.Views;
#else
using System;
#if LOGGING
using System.IO;
#endif
#endif

namespace TactileGame
{
#if __ANDROID__
    [Activity(Label = "TactileGame"
        , MainLauncher = true
        , Icon = "@drawable/icon"
        , Theme = "@style/Theme.Splash"
        , AlwaysRetainTaskState = true
        , LaunchMode = Android.Content.PM.LaunchMode.SingleInstance
        , ScreenOrientation = ScreenOrientation.SensorLandscape
        , ConfigurationChanges = ConfigChanges.Orientation | ConfigChanges.Keyboard | ConfigChanges.KeyboardHidden | ConfigChanges.ScreenSize)]
	public class Activity1 : Microsoft.Xna.Framework.AndroidGameActivity
	{
		Game1 g; //Debug
		protected override void OnCreate(Bundle bundle)
		{
            base.OnCreate(bundle);
			// Game1.Activity = this; //Debug
            Tactile.Rendering.GameRenderer.SetStatusBarHeight(getStatusBarHeight());
            g = new Game1(new string[0]); //Debug

            //SetContentView(g.Window); //Debug
            SetContentView((View)g.Services.GetService(typeof(View)));

			g.Run();
		}

		public int getStatusBarHeight() {
			int result = 0;
			int resourceId = Resources.GetIdentifier("status_bar_height", "dimen", "android");
			if (resourceId > 0) {
				result = Resources.GetDimensionPixelSize(resourceId);
			}
			return result;
		}

		protected override void OnPause ()
		{
			base.OnPause();
			g.dispose_render_targets();
			g.move_to_background();
		}

		protected override void OnResume ()
		{
			base.OnResume();
			if (g.started)
				g.resume_processing();
		}
	}
#endif
#if MONOMAC
using MonoMac.AppKit;
using MonoMac.Foundation;

namespace TactileGame
{
	class Program
	{
		static void Main (string [] args)
		{
			NSApplication.Init ();
			
			using (var p = new NSAutoreleasePool ()) {
				NSApplication.SharedApplication.Delegate = new AppDelegate();
				NSApplication.Main(args);
			}
		}
	}
	
	class AppDelegate : NSApplicationDelegate
	{
		Game1 game;
		public override void FinishedLaunching (MonoMac.Foundation.NSObject notification)
		{			
            game = new Game1(new string[] { });
			game.Run ();
		}
		
		public override bool ApplicationShouldTerminateAfterLastWindowClosed (NSApplication sender)
		{
			return true;
		}
	}	
}
#endif
#if WINDOWS || XBOX
    static class Program
    {
        private const int ERROR_EDITOR = 0x74;

        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        static void Main(string[] args)
        {
            if (!TestForXNA())
            {
                System.Windows.Forms.MessageBox.Show("Microsoft XNA Framework not found.\nPlease install it by running 'xnafx40_redist.msi' from\nthe 'Installers' folder before you can play this game.", "XNA is not installed");
            }
            else if (!TestForOpenAL())
            {
                System.Windows.Forms.MessageBox.Show("Tactile uses OpenAL for audio, and needs it to be installed.\nPlease install it by running 'oalinst.exe' from\nthe 'Installers' folder before you can play this game.", "OpenAL is not installed");
            }
            else
            {
#if LOGGING
                AppDomain currentDomain = AppDomain.CurrentDomain;
                currentDomain.UnhandledException += new UnhandledExceptionEventHandler(MyHandler);
                /*if (File.Exists("temp.log"))
                {
                    File.Copy("temp.log", "exception.log", true);
                    File.Delete("temp.log");
                }*/
#endif
                RunGame(args);
            }
        }

        private static void RunGame(string[] args)
        {
            using (Game1 game = new Game1(args))
            {
#if WINDOWS
                SquareWindowCorners(game.Window.Handle);
#endif

                game.Run();
            }
        }

#if WINDOWS
        private static void SquareWindowCorners(IntPtr hWnd)
        {
            try
            {
                // Do not round window corners on Windows 11 etc
                var attribute = DWMWINDOWATTRIBUTE.DWMWA_WINDOW_CORNER_PREFERENCE;
                var preference = DWM_WINDOW_CORNER_PREFERENCE.DWMWCP_DONOTROUND;
                NativeMethods.DwmSetWindowAttribute(hWnd, attribute, ref preference, sizeof(uint));
            }
            catch (Exception ex) { }
        }
#endif

#if LOGGING
        static void MyHandler(object sender, UnhandledExceptionEventArgs args)
        {
            Exception e = (Exception)args.ExceptionObject;
            // print out the exception stack trace to a log
            using (StreamWriter writer = new StreamWriter("exception.log", true))
            {
                writer.Write(string.Format("{0}\r\n{1}\r\n\r\n",
                    DateTime.Now.ToString(), e.ToString()));
            }

#if DEBUG
            // Probably running from the editor, which would like to report exceptions
            if (Tactile.Global.OutputEditorException)
            {
                Environment.ExitCode = ERROR_EDITOR;

                // Save to a log that the Tactile Editor can read
                using (StreamWriter writer = new StreamWriter("EditorException.log", false))
                {
                    writer.Write(string.Format("{0}\r\n{1}\r\n\r\n",
                        DateTime.Now.ToString(), e.ToString()));
                }
            }
#endif
        }
#endif

        public static bool TestForXNA()
        {
            string baseKeyName = @"SOFTWARE\Microsoft\XNA\Framework";
            //string baseKeyName = @"SOFTWARE\Wow6432Node\Microsoft\XNA\Framework"; //@Debug: actual location on x64, doesn't need checked in 32 bit programs like this one
            //string baseKeyName = @"SOFTWARE\Microsoft\XNA\Game Studio"; //@Debug: can check if installed for devs
            Microsoft.Win32.RegistryKey installedFrameworkVersions = Microsoft.Win32.Registry.LocalMachine.OpenSubKey(baseKeyName);

            if (installedFrameworkVersions != null)
            {
                string[] versionNames = installedFrameworkVersions.GetSubKeyNames();

                foreach (string s in versionNames)
                {
                    if (s == "v4.0")
                    {
                        return true;
                    }
                }
            }

            return false;
        }
        public static bool TestForOpenAL()
        {
            try
            {
                IntPtr handle = NativeMethods.LoadLibrary("openal32.dll");
                if (handle == IntPtr.Zero)
                    return false;
                bool success = NativeMethods.FreeLibrary(handle);
            }
            catch (DllNotFoundException)
            {
                // Handle your logic here
                return false;
            }
            return true;
        }
    }
#endif
}
