using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Web.Script.Serialization;

namespace FTPSchubser.Helper
{
    class URLHelper
    {
        public async static Task<IEnumerable<string>> ShortenUrlsAsync(IEnumerable<string> urls)
        {
            var serializer = new JavaScriptSerializer();

            var ret = new List<string>();

            foreach (var url in urls)
            {
                var httpWebRequest = (HttpWebRequest)WebRequest.Create("https://www.googleapis.com/urlshortener/v1/url?key=API_KEY");
                httpWebRequest.ContentType = "application/json";
                httpWebRequest.Method = "POST";
                using (var streamWriter = new StreamWriter(await httpWebRequest.GetRequestStreamAsync()))
                {
                    var jsonObject = new { longUrl = url };

                    var jsonString = serializer.Serialize(jsonObject);

                    streamWriter.Write(jsonString);
                    streamWriter.Flush();
                    streamWriter.Close();
                }

                var httpResponse = (HttpWebResponse)await httpWebRequest.GetResponseAsync();
                using (var streamReader = new StreamReader(httpResponse.GetResponseStream()))
                {
                    var result = await streamReader.ReadToEndAsync();

                    var json = serializer.Deserialize<GooGlResponse>(result);

                    ret.Add(json.id);
                }
            }

            return ret;
        }

        private class GooGlResponse
        {
            public string kind { get; set; }
            public string id { get; set; }
            public string longUrl { get; set; }
        }
    }
}
