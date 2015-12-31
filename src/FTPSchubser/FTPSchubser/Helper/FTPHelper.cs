using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
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

        public async Task<bool> FileExistsAsync(string fileName)
        {
            return false;
        }

        public async Task UploadFilesAsync(IEnumerable<string> files, IProgress<FTPProgress> progress = null)
        {
            var fileInfos = new List<FileInfo>();

            foreach (var file in files)
            {
                if (File.Exists(file))
                {
                    fileInfos.Add(new FileInfo(file));
                }
            }

            long bytesTotal = fileInfos.Sum(x => x.Length);
            long bytesDone = 0;
            var filesTotal = fileInfos.Count;
            var filesDone = 0;

            foreach (var fileInfo in fileInfos)
            {
                filesDone++;

                var fileName = System.IO.Path.GetFileName(fileInfo.FullName);
                var ftpUrl = Utils.FormatFTPUrl(Host, Path, fileName, Port);

                // from https://msdn.microsoft.com/en-us/library/ms229715%28v=vs.110%29.aspx
                // Get the object used to communicate with the server.
                var request = (FtpWebRequest)WebRequest.Create(ftpUrl);

                request.Method = WebRequestMethods.Ftp.UploadFile;
                request.UsePassive = Pasv;
                request.Credentials = new NetworkCredential(Username, Password);

                using (var fileStream = File.OpenRead(fileInfo.FullName))
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
                progress.Report(new FTPProgress(filesTotal, filesDone, bytesTotal, bytesDone));
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
