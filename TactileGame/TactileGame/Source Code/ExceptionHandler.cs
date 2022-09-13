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
            string version = Tactile.Global.RUNNING_VERSION.ToString();
            string gameStateInfo = Tactile.Global.GetExceptionMetrics();
            string exceptionStr = string.Format(
                "{0}\r\nVersion: {1}\r\n{2}\r\n{3}\r\n\r\n",
                DateTime.Now.ToString(),
                version.ToString(),
                gameStateInfo,
                e.ToString());

#if WINDOWS
            // print out the exception stack trace to a log
            using (StreamWriter writer = new StreamWriter("exception.log", true))
            {
                writer.Write(exceptionStr);
            }
#endif

            // Send exception to remote server
            if (Tactile.Global.ShouldSendException())
            {
                var exceptionSender = new ExceptionSender();
                if (exceptionSender.test_connection())
                {
                    exceptionSender.send_data("", exceptionStr);
                }
            }
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
