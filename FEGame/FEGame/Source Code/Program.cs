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

namespace FEGame
{
#if __ANDROID__
    [Activity(Label = "FEGame"
        , MainLauncher = true
        , Icon = "@drawable/icon"
        , Theme = "@style/Theme.Splash"
        , AlwaysRetainTaskState = true
        , LaunchMode = Android.Content.PM.LaunchMode.SingleInstance
        , ScreenOrientation = ScreenOrientation.SensorLandscape
        , ConfigurationChanges = ConfigChanges.Orientation | ConfigChanges.Keyboard | ConfigChanges.KeyboardHidden | ConfigChanges.ScreenSize)]
	/*[Activity(Label = "FEGame", 
		MainLauncher = true, 
		Icon ="@drawable/icon",
		ConfigurationChanges=ConfigChanges.Orientation|ConfigChanges.Keyboard|ConfigChanges.KeyboardHidden)]*/
	public class Activity1 : Microsoft.Xna.Framework.AndroidGameActivity
	{
		Game1 g; //Debug
		protected override void OnCreate(Bundle bundle)
		{
            base.OnCreate(bundle);
			// FEGame.Game1.Activity = this; //Debug
			Game1.STATUS_BAR_HEIGHT = getStatusBarHeight();
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

namespace FEGame
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
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        static void Main(string[] args)
        {
            if (!test_for_openal())
            {
                System.Windows.Forms.MessageBox.Show("FEXNA uses OpenAL for audio, and needs it to be installed.\nPlease install it by running 'oalinst.exe' from the\ngame folder before you can play this game.", "OpenAL is not installed");
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
                using (Game1 game = new Game1(args))
                {
                    game.Run();
                }
            }
        }

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
        }
#endif

        public static bool test_for_openal()
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
