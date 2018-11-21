//#if WINDOWS || MONOMAC
using System;
using System.Linq;
using System.Text.RegularExpressions;

namespace FE7x
{
    class Update_Checker
    {
        //internal readonly static string GAME_DOWNLOAD = "yoursite.herethough/yourgame";
        //readonly static string UPDATE_URL = "http://put.yoursite.herethough/yourgame/check_update.php";

        internal readonly static string GAME_DOWNLOAD = "bwdyeti.com/fe7x"; //FEGame
        readonly static string UPDATE_URL = "http://www.bwdyeti.com/fe7x/check_update.php";
        const int UPDATE_CHECK_TIMEOUT_SECONDS = 5;
        const int DESCRIPTION_LENGTH_CAP = 128;
        const int REMOTE_RESPONSE_LENGTH_LIMIT = 256;
        readonly static string UPDATE_REGEX = @"[^\w.,\s\r\n]";

        internal static Tuple<Version, DateTime, string> check_for_update()
        {
#if DEBUG //FEGame
            // Just some code to ensure I don't accidently distribute FE7x urls, remove after scrubbing
            System.Reflection.Assembly assembly = System.Reflection.Assembly.GetExecutingAssembly();
            System.Diagnostics.Debug.Assert(assembly.ManifestModule.Name == "FE7x.exe", "whoops mistakes");
#endif

            string update_data = NetConnection.webPost(UPDATE_URL,
                timeout: new TimeSpan(0, 0, UPDATE_CHECK_TIMEOUT_SECONDS),
                responseLengthLimit: REMOTE_RESPONSE_LENGTH_LIMIT);
            if (string.IsNullOrEmpty(update_data))
                return null;

#if DEBUG
            // Update test data
            if (false)
            {
                System.Threading.Thread.Sleep(2000);

                update_data = 
                    "0.6.0.0\r\n" +
                    "2017, 10, 19\r\n" +
                    "This is an update";
            }
#endif

            update_data = Regex.Replace(update_data, UPDATE_REGEX, "");

            var result = parse_update_data(update_data);
            return result;
        }

        private static Tuple<Version, DateTime, string> parse_update_data(string update_data)
        {
            string[] result = update_data.Split(new string[] { "\r\n" }, StringSplitOptions.RemoveEmptyEntries);
            if (result.Length < 2)
                return null;

            int test;
            // Confirm version length
            string[] version = result[0].Split('.');
            if (version.Length != 4 || version.All(x => !int.TryParse(x, out test)))
                return null;
            // Confirm date length
            string[] date = result[1].Split(new string[] { ", " }, StringSplitOptions.RemoveEmptyEntries);
            if (date.Length != 3 || date.All(x => !int.TryParse(x, out test)))
                return null;

            Version v = new Version(
                Convert.ToInt32(version[0]), Convert.ToInt32(version[1]),
                Convert.ToInt32(version[2]), Convert.ToInt32(version[3]));
            DateTime dt = new DateTime(
                Convert.ToInt32(date[0]), Convert.ToInt32(date[1]), Convert.ToInt32(date[2]));
            string description = result.Length < 3 ? null : result[2];
            if (!string.IsNullOrEmpty(description) && description.Length > DESCRIPTION_LENGTH_CAP)
                description = description.Substring(0, DESCRIPTION_LENGTH_CAP);

            return Tuple.Create(v, dt, description);
        }

        internal static bool test_connection()
        {
            return NetConnection.test_connection_dns(UPDATE_URL);
        }
    }
}
//#endif