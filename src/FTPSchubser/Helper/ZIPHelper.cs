using ICSharpCode.SharpZipLib.Core;
using ICSharpCode.SharpZipLib.Zip;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FTPSchubser.Helper
{
    class ZIPHelper
    {
        public static async Task<IEnumerable<UploadFile>> ZipDirectoriesAsync(IEnumerable<string> directories, IProgress<ZIPProgress> progress = null)
        {
            var ret = new List<UploadFile>();
            await Task.Run(() =>
            {
                foreach (var dir in directories)
                {
                    var directoryInfo = new DirectoryInfo(dir);
                    var zipName = directoryInfo.Name + ".zip";
                    var tempFile = Path.Combine(Path.GetTempPath(), Path.GetTempFileName());

                    int filesToZip = 0;
                    long bytesToZip = 0;
                    int filesDone = 0;
                    long bytesDone = 0;
                    ZipFolderContentsCount(dir, ref filesToZip, ref bytesToZip);
                    

                    using (var zipStream = new ZipOutputStream(File.Create(tempFile)))
                    {
                        zipStream.SetLevel(0); //0-9, 9 being the highest level of compression
                        ZipFolder(dir, zipStream, new Progress<ZIPProgress>((x) =>
                        {
                            if (progress != null)
                            {
                                filesDone += x.FilesDone;
                                bytesDone += x.BytesDone;
                                progress.Report(new ZIPProgress(filesToZip, filesDone, bytesToZip, bytesDone));
                            }
                        }));
                    }

                    ret.Add(new UploadFile(tempFile, zipName, true));
                }
            });

            return ret;
        }

        private static void ZipFolder(string path, ZipOutputStream zipStream, IProgress<ZIPProgress> progress = null, int folderOffset = 0)
        {
            if (folderOffset == 0)
            {
                folderOffset = path.Length;
            }

            string[] files = Directory.GetFiles(path);

            var filesDone = 0;

            foreach (string filename in files)
            {
                filesDone++;

                FileInfo fi = new FileInfo(filename);

                string entryName = filename.Substring(folderOffset); // Makes the name in zip based on the folder
                entryName = ZipEntry.CleanName(entryName); // Removes drive from name and fixes slash direction
                ZipEntry newEntry = new ZipEntry(entryName);
                newEntry.DateTime = fi.LastWriteTime; // Note the zip format stores 2 second granularity
                newEntry.IsUnicodeText = true;

                // Specifying the AESKeySize triggers AES encryption. Allowable values are 0 (off), 128 or 256.
                // A password on the ZipOutputStream is required if using AES.
                //   newEntry.AESKeySize = 256;

                // To permit the zip to be unpacked by built-in extractor in WinXP and Server2003, WinZip 8, Java, and other older code,
                // you need to do one of the following: Specify UseZip64.Off, or set the Size.
                // If the file may be bigger than 4GB, or you do not need WinXP built-in compatibility, you do not need either,
                // but the zip will be in Zip64 format which not all utilities can understand.
                //   zipStream.UseZip64 = UseZip64.Off;
                newEntry.Size = fi.Length;

                zipStream.PutNextEntry(newEntry);

                // Zip the file in buffered chunks
                // the "using" will close the stream even if an exception occurs
                byte[] buffer = new byte[4096];
                using (FileStream streamReader = File.OpenRead(filename))
                {
                    StreamUtils.Copy(streamReader, zipStream, buffer);
                }
                zipStream.CloseEntry();

                if (progress != null)
                {
                    progress.Report(new ZIPProgress(0, filesDone, 0, fi.Length));
                }
            }
            string[] folders = Directory.GetDirectories(path);
            foreach (string folder in folders)
            {
                ZipFolder(folder, zipStream, progress, folderOffset);
            }
        }

        // Returns the number of files in this and all subdirectories
        private static void ZipFolderContentsCount(string path, ref int fileCount, ref long fileSize)
        {
            var files = Directory.GetFiles(path);
            foreach (var file in files)
            {
                var fi = new FileInfo(file);

                fileCount++;
                fileSize += fi.Length;
            }

            fileCount += Directory.GetFiles(path).Length;
            string[] subFolders = Directory.GetDirectories(path);
            foreach (string subFolder in subFolders)
            {
                ZipFolderContentsCount(subFolder, ref fileCount, ref fileSize);
            }
        }

        public class ZIPProgress
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

            public ZIPProgress(int filesTotal, int filesDone, long bytesTotal, long bytesDone)
            {
                FilesTotal = filesTotal;
                FilesDone = filesDone;
                BytesTotal = bytesTotal;
                BytesDone = bytesDone;
            }

            public override string ToString()
            {
                return $"File {FilesDone}/{FilesTotal} {Utils.BytesToString(BytesDone)}/{Utils.BytesToString(BytesTotal)} ({Math.Floor(BytesPercent * 100)}%)";
            }
        }
    }
}
