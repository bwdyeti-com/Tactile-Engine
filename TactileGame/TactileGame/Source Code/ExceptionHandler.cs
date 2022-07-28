using System;
using System.IO;

namespace TactileGame
{
    static class ExceptionLogger
    {
        private const int ERROR_EDITOR = 0x74;

        public static void Handler(object sender, UnhandledExceptionEventArgs args)
        {
            Exception e = (Exception)args.ExceptionObject;

#if !DEBUG // skip when debugging, the debugger is probably attached and can handle it
#if WINDOWS
            // print out the exception stack trace to a log
            using (StreamWriter writer = new StreamWriter("exception.log", true))
            {
                writer.Write(string.Format("{0}\r\n{1}\r\n\r\n",
                    DateTime.Now.ToString(), e.ToString()));
            }
#endif
#endif

#if WINDOWS && DEBUG
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
    }
}
