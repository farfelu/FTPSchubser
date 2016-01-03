using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace FTPSchubser.Helper
{
    class FTPHelper
    {
        public string Host { get; private set; }
        public string Username { get; private set; }
        public string Password { get; private set; }
        public string Path { get; private set; }
        public int Port { get; private set; }
        public bool Pasv { get; private set; }

        public FTPHelper(string host, string username, string password, string path, int port = 21, bool pasv = true)
        {
            Host = host;
            Username = username;
            Password = password;
            Path = path;
            Port = port;
            Pasv = pasv;
        }

        private FtpWebRequest CreateFTPRequest(string url, string method)
        {
            FtpWebRequest request = (FtpWebRequest)WebRequest.Create(url);
            request.Method = method;
            request.UsePassive = Pasv;
            request.Credentials = new NetworkCredential(Username, Password);

            return request;
        }

        public async Task<IEnumerable<FTPFile>> ListFilesAsync()
        {
            var ftpUrl = Utils.FormatFTPUrl(Host, Path, null, Port);
            
            var request = CreateFTPRequest(ftpUrl, WebRequestMethods.Ftp.ListDirectoryDetails);
            FtpWebResponse response = (FtpWebResponse)await request.GetResponseAsync();

            Stream responseStream = response.GetResponseStream();
            StreamReader reader = new StreamReader(responseStream);
            string line = null;

            var ret = new List<FTPFile>();
            while ((line = await reader.ReadLineAsync()) != null)
            {
                var file = FTPFile.CreateFTPFile(line);
                if (file != null)
                {
                    ret.Add(file);
                }
            }

            reader.Close();
            response.Close();

            return ret;
        }

        public async Task<IEnumerable<UploadFile>> GetExistingFiles(IEnumerable<UploadFile> files)
        {
            var ret = new List<UploadFile>(files);

            foreach (var file in files)
            {
                var fileName = System.IO.Path.GetFileName(file.FilePath);
                var ftpUrl = Utils.FormatFTPUrl(Host, Path, fileName, Port);
                
                var request = CreateFTPRequest(ftpUrl, WebRequestMethods.Ftp.GetDateTimestamp);

                try
                {
                    var response = (FtpWebResponse)await request.GetResponseAsync();
                }
                catch (WebException ex)
                {
                    var response = (FtpWebResponse)ex.Response;
                    if (response.StatusCode == FtpStatusCode.ActionNotTakenFileUnavailable)
                    {
                        ret.Remove(file);
                    }
                }
            }

            return ret;
        }

        public async Task<IEnumerable<UploadFile>> UploadFilesAsync(IEnumerable<UploadFile> files, IProgress<FTPProgress> progress = null)
        {
            long bytesTotal = files.Sum(x => x.FileInfo.Length);
            long bytesDone = 0;
            var filesTotal = files.Count();
            var filesDone = 0;

            var ret = new List<UploadFile>();

            foreach (var file in files)
            {
                filesDone++;

                var fileName = file.UploadName;
                var ftpUrl = Utils.FormatFTPUrl(Host, Path, fileName, Port);

                // from https://msdn.microsoft.com/en-us/library/ms229715%28v=vs.110%29.aspx
                var request = CreateFTPRequest(ftpUrl, WebRequestMethods.Ftp.UploadFile);

                using (var fileStream = File.OpenRead(file.FileInfo.FullName))
                using (var requestStream = await request.GetRequestStreamAsync())
                {
                    var buffer = new byte[1024 * 1024]; // 1MiB cache
                    var readBytesCount = 0;
                    
                    //used so we can measure how many bytes per second we transfer
                    var timeHelper = new Stopwatch();                   
                    
                    timeHelper.Start();
                    long bytesCounter = 0;
                    long bytesPerSecond = 0;
                    while ((readBytesCount = await fileStream.ReadAsync(buffer, 0, buffer.Length)) > 0)
                    {
                        await requestStream.WriteAsync(buffer, 0, readBytesCount);
                        bytesDone += readBytesCount;

                        if (progress != null)
                        {
                            bytesCounter += readBytesCount;
                            
                            // reset helper every second
                            if (timeHelper.ElapsedMilliseconds >= 1000)
                            {
                                bytesPerSecond = (long)Math.Floor(bytesCounter / (timeHelper.ElapsedMilliseconds / 1000d));
                                bytesCounter = 0;
                                timeHelper.Restart();
                            }

                            progress.Report(new FTPProgress(filesTotal, filesDone, bytesTotal, bytesDone, bytesPerSecond));
                        }
                    }
                }
                ret.Add(file);
                progress.Report(new FTPProgress(filesTotal, filesDone, bytesTotal, bytesDone));
            }

            return ret;
        }

        public class FTPFile
        {
            public bool IsDirectory { get; set; }
            public string Permissions { get; set; }
            public string Owner { get; set; }
            public string Group { get; set; }
            public int FileCode { get; set; }
            public long Size { get; set; }
            public DateTime? Timestamp { get; set; }
            public string Filename { get; set; }

            public static FTPFile CreateFTPFile(string listLine)
            {
                var regex = new Regex(  @"^" +                                          // start of the line
                                        @"(?<dir>[\-ld])" +                             // match directory flag
                                        @"(?<permissions>([\-r][\-w][\-xs]){3})" +      // match owner/group/other unix permissions
                                        @"\s+" +                                        // match whitespace
                                        @"(?<filecode>\d+)" +                           // match filecode
                                        @"\s+" +                                        //
                                        @"(?<owner>[\w\-]+)" +                          // match owner
                                        @"\s+" +                                        // 
                                        @"(?<group>[\w\-]+)" +                          // match group
                                        @"\s+" +                                        //
                                        @"(?<size>\d+)" +                               // match size in bytes
                                        @"\s+" +                                        //
                                        @"(?<timestamp>" +                              // start match timestamp, two formats
                                            @"(?:" +                                    // non matching group for format: Dec 31 18:27
                                                @"(?<month>\w{3})" +                    // match month name
                                                @"\s+" +                                //
                                                @"(?<day>\d{1,2})" +                    // match day
                                                @"\s+" +                                //
                                                @"(?<hours>\d{1,2}):(?<minutes>\d{2})" +  // match hours:minutes
                                            @")" +                                      //
                                            @"|" +                                      // or other date format without time but with year: Dec 31 2015
                                            @"(?:" +                                    //
                                                @"(?<month>\w{3})" +                    //
                                                @"\s+" +                                //
                                                @"(?<day>\d{1,2})" +                    //
                                                @"\s+" +                                //
                                                @"(?<year>\d{4})" +                     //
                                            @")" +                                      //
                                        @")" +                                          // end of timestamp
                                        @"\s+" +                                        //
                                        @"(?<filename>.+)" +                                // match filename
                                        @"$"                                            // end of line
                                        , RegexOptions.IgnoreCase);

                var match = regex.Match(listLine);

                if (!match.Success)
                {
                    return null;
                }

                // try to figure out timestamp
                var timestampString = match.Groups["timestamp"].Value;

                //clean up timestamp string by removing double spaces
                timestampString = timestampString.Replace("  ", " ");

                DateTime timestampTemp;
                DateTime? timestamp = null;

                if (!Regex.Match(timestampString, "[0-9]{4}").Success)
                {
                    timestampString += $" {DateTime.Now.Year}";
                }

                if (DateTime.TryParseExact(timestampString, new string[] { "MMM dd HH:mm yyyy", "MMM d HH:mm yyyy" }, CultureInfo.InvariantCulture, DateTimeStyles.None, out timestampTemp))
                {
                    timestamp = timestampTemp;
                }
                else if (DateTime.TryParse(timestampString, out timestampTemp))
                {
                    timestamp = timestampTemp;
                }

                // check if it isn't in the future, if so, use the last year...
                if (timestamp.HasValue && DateTime.Now < timestamp.Value)
                {
                    // date is now in the future, so subtract a year.
                    timestamp = timestamp.Value.AddYears(-1);
                }

                return new FTPFile()
                {
                    IsDirectory = match.Groups["dir"].Value == "d",
                    Permissions = match.Groups["permissions"].Value,
                    FileCode = int.Parse(match.Groups["filecode"].Value),
                    Owner = match.Groups["owner"].Value,
                    Group = match.Groups["group"].Value,
                    Size = long.Parse(match.Groups["size"].Value),
                    Timestamp = timestamp,
                    Filename = match.Groups["filename"].Value
                };
            }
        }

        public class FTPProgress
        {
            public int FilesTotal { get; }
            public int FilesDone { get; }
            public long BytesTotal { get; }
            public long BytesDone { get; }
            public double BytesPercent
            {
                get
                {
                    return BytesTotal == 0 ? 0 : ((double)BytesDone / (double)BytesTotal);
                }
            }

            public string TransferSpeed { get; } = "";

            public FTPProgress(int filesTotal, int filesDone, long bytesTotal, long bytesDone, long bytesPerSecond = -1)
            {
                FilesTotal = filesTotal;
                FilesDone = filesDone;
                BytesTotal = bytesTotal;
                BytesDone = bytesDone;
                if (bytesPerSecond > 0)
                {
                    TransferSpeed = $" {Utils.BytesToString(bytesPerSecond)}/s";
                }
            }

            public override string ToString()
            {
                return $"File {FilesDone}/{FilesTotal} {Utils.BytesToString(BytesDone)}/{Utils.BytesToString(BytesTotal)} ({Math.Floor(BytesPercent * 100)}%){TransferSpeed}";
            }
        }
    }
}
