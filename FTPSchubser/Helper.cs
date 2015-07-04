using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace FTPSchubser
{
    public static class Helper
    {
        public static string EncodeFileName(string fileName)
        {
            return Uri.EscapeDataString(fileName);
        }
    }
}
