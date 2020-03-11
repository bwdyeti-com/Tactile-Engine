//#if WINDOWS || MONOMAC
using System;
using System.Collections;
using System.IO;
using System.Linq;

namespace FEGame
{
    enum OperatingSystems { Windows, OSX, Android, Linux }

    class Metrics_Handler : FEXNA.IMetricsService
    {
        readonly static string PRIVATE_KEY = "put the private key from your php file here, compeltely random characters is most secure!";
        readonly static string ADD_SCORE_URL = "http://put.yoursite.herethough/yourgame/ranking_analytics.php?"; // The question mark lets you pass variables in, okay
        
        const int REMOTE_RESPONSE_LENGTH_LIMIT = 256;

        public FEXNA_Library.Maybe<bool> send_data(string query, string post)
        {
            string identifier = FEXNA.Metrics.Metrics_Data.UserIdentifier;
            string hash = hashString(identifier + PRIVATE_KEY);
            string region = System.Globalization.RegionInfo.CurrentRegion.Name;
#if WINDOWS
            int system = (int)OperatingSystems.Windows;
#elif MONOMAC
            int system = (int)OperatingSystems.OSX;
#elif __ANDROID__
            int system = (int)OperatingSystems.Android;
#endif
            string version = FEXNA.Global.RUNNING_VERSION.ToString();

            string result = NetConnection.webPost(
                ADD_SCORE_URL + string.Format(
                    "identifier={0}&{1}&version={5}&region={2}&system={3}&hash={4}",
                    identifier, query, region, system, hash, version), post,
                    responseLengthLimit: REMOTE_RESPONSE_LENGTH_LIMIT);

            if (string.IsNullOrEmpty(result))
                return default(FEXNA_Library.Maybe<bool>);
            return result.Split('\n').Last() == "Success";
        }

        double[] prefixAverages1(int[] X, int n)
        {
            double[] A = new double[n];
            for(int i  = 0; i < n; i++)
            {
                int s = 0;
                for(int j = 0; j < i; j++)
                    s+= X[j];
                A[i] = s / (i + 1);
            }
            return A;
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
            return NetConnection.test_connection_dns(ADD_SCORE_URL);
        }
    }
}
//#endif
