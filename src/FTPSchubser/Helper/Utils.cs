using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FTPSchubser.Helper
{
    class Utils
    {

        public static string FormatUrl(string protocol, string host, string path, string fileName, int? port = null)
        {
            if (string.IsNullOrWhiteSpace(host))
            {
                return null;
            }

            return $"{protocol}://{host}{(port == null ? "" : ":" + port)}{(string.IsNullOrWhiteSpace(path) ? "" : "/" + path)}{(string.IsNullOrWhiteSpace(fileName) ? "" : "/" + fileName)}";
        }

        public static string FormatFTPUrl(string host, string path = null, string fileName = null, int? port = 21)
        {
            return FormatUrl("ftp", host, path, fileName, port);
        }

        public static string FormatHTTPUrl(string url, string fileName)
        {
            return $"{url}{fileName}";
        }

        // from http://stackoverflow.com/a/4975942
        public static string BytesToString(long byteCount)
        {
            string[] suf = { "B", "KB", "MB", "GB", "TB", "PB", "EB" }; //Longs run out around EB
            if (byteCount == 0)
                return "0" + suf[0];
            long bytes = Math.Abs(byteCount);
            int place = Convert.ToInt32(Math.Floor(Math.Log(bytes, 1024)));
            double num = Math.Round(bytes / Math.Pow(1024, place), 1);
            return (Math.Sign(byteCount) * num).ToString() + suf[place];
        }
    }
}
