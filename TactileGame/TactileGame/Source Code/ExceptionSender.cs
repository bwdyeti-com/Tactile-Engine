//#if WINDOWS || MONOMAC
using System;
using System.Collections;
using System.IO;
using System.Linq;

namespace TactileGame
{
    class ExceptionSender : Tactile.IMetricsService
    {
        readonly static string SEND_EXCEPTION_URL = "http://put.yoursite.herethough/yourgame/log_exception.php?"; // The question mark lets you pass variables in, okay
        
        const int REMOTE_RESPONSE_LENGTH_LIMIT = 256;

        public TactileLibrary.Maybe<bool> send_data(string query, string post)
        {
            string identifier = Tactile.Metrics.Metrics_Data.UserIdentifier;
            string hash = hashString(identifier + Metrics_Handler.PRIVATE_KEY);
#if WINDOWS
            int system = (int)OperatingSystems.Windows;
#elif MONOMAC
            int system = (int)OperatingSystems.OSX;
#elif __ANDROID__
            int system = (int)OperatingSystems.Android;
#endif

            string result = NetConnection.webPost(
                SEND_EXCEPTION_URL + string.Format(
                    "identifier={0}&system={1}&hash={2}",
                    identifier, system, hash), post,
                    responseLengthLimit: REMOTE_RESPONSE_LENGTH_LIMIT);

            if (string.IsNullOrEmpty(result))
                return default(TactileLibrary.Maybe<bool>);
            return result.Split('\n').Last() == "Success";
        }

        private static string hashString(string _value)
        {
            System.Security.Cryptography.MD5CryptoServiceProvider x = new System.Security.Cryptography.MD5CryptoServiceProvider();
            byte[] data = System.Text.Encoding.ASCII.GetBytes(_value);
            data = x.ComputeHash(data);
            string ret = "";
            for (int i = 0; i < data.Length; i++) ret += data[i].ToString("x2").ToLower();
            return ret;
        }

        public bool test_connection()
        {
            return NetConnection.test_connection_dns(SEND_EXCEPTION_URL);
        }
    }
}
//#endif