using FTPSchubser.Properties;
using ICSharpCode.SharpZipLib.Core;
using ICSharpCode.SharpZipLib.Zip;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web;
using System.Windows.Forms;
using System.Windows.Shell;
using System.Xml;

namespace FTPSchubser
{
    public partial class frm_main : Form
    {
        private const string BITLYUSER = "xxx";
        private const string BITLYKEY = "xxx";
        private const string GOOGLKEY = "xxx";

        private string[] file;
        private bool showsettings;
        private bool arg;
        private List<string> clip;
        private List<string> clipshort;
        private int filecount;
        private int curfilecount;
        private XmlDocument xmlDoc;
        private TaskbarItemInfo tbInfo;

        public frm_main(string[] args)
        {
            this.InitializeComponent();
            this.tbInfo = new TaskbarItemInfo();
            this.Height = 165;
            this.filecount = 0;
            this.curfilecount = 0;
            this.clip = new List<string>();
            if (args.Length <= 0)
                return;
            if (string.IsNullOrWhiteSpace(Settings.Default.host))
            {
                BackgroundThreadMessageBox(this, "You have not yet set up a host in the settings.", "No host", MessageBoxButtons.OK, MessageBoxIcon.Error, MessageBoxDefaultButton.Button1);
                return;
            }
            this.arg = true;
            this.file = args;
            this.tbInfo.ProgressState = TaskbarItemProgressState.Normal;
            this.th_upload.RunWorkerAsync();
        }

       


        private void p_dropzone_DragEnter(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
                e.Effect = DragDropEffects.Move;
            else
                e.Effect = DragDropEffects.None;
        }

        private void p_dropzone_DragDrop(object sender, DragEventArgs e)
        {
            this.file = (string[])e.Data.GetData(DataFormats.FileDrop);
            this.tbInfo.ProgressState = TaskbarItemProgressState.Normal;
            this.th_upload.RunWorkerAsync();
        }

        /* ZIP THINGS */

        private int _ZipUptoFileCount;
        private int _ZipTotalFileCount;
        private void ZipFolder(string path, ZipOutputStream zipStream, int folderOffset = 0)
        {
            if (folderOffset == 0)
            {
                folderOffset = path.Length;
            }

            string[] files = Directory.GetFiles(path);

            foreach (string filename in files)
            {


                _ZipUptoFileCount++;
                var progress = _ZipUptoFileCount * 100 / _ZipTotalFileCount;
                th_upload.ReportProgress(progress, string.Format("Zipping {0}/{1}", _ZipUptoFileCount, _ZipTotalFileCount));


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
            }
            string[] folders = Directory.GetDirectories(path);
            foreach (string folder in folders)
            {
                ZipFolder(folder, zipStream, folderOffset);
            }
        }
        // Returns the number of files in this and all subdirectories
        private int ZipFolderContentsCount(string path)
        {
            int result = Directory.GetFiles(path).Length;
            string[] subFolders = Directory.GetDirectories(path);
            foreach (string subFolder in subFolders)
            {
                result += ZipFolderContentsCount(subFolder);
            }
            return result;
        }

        private DialogResult BackgroundThreadMessageBox(IWin32Window owner, string text, string caption, MessageBoxButtons buttons, MessageBoxIcon icon, MessageBoxDefaultButton defaultbutton)
        {
            if (this.InvokeRequired)
            {
                return (DialogResult)this.Invoke(new Func<DialogResult>(
                                       () => { return MessageBox.Show(owner, text, caption, buttons,  icon, defaultbutton); }));
            }
            else
            {
                return MessageBox.Show(owner, text, caption, buttons, icon, defaultbutton);
            }
        }

        private void th_upload_DoWork(object sender, DoWorkEventArgs e)
        {
            try
            {
                this.clip.Clear();
                this.filecount = 0;
                this.curfilecount = 0;
                this.filecount = this.file.Length;
                foreach (string str1 in this.file)
                {
                    ++this.curfilecount;

                    FileAttributes attr = File.GetAttributes(str1);

                    string fileToUpload = str1;
                    string filenameToUpload = str1;
                    bool zipped = false;
                    if ((attr & FileAttributes.Directory) == FileAttributes.Directory)
                    {
                        //when directory, zip it up.
                        var tempName = Path.GetRandomFileName();
                        fileToUpload = Path.Combine(Path.GetTempPath(), tempName) + ".zip";
                        filenameToUpload = str1 + ".zip";

                        _ZipTotalFileCount = ZipFolderContentsCount(str1);



                        using (var zipStream = new ZipOutputStream(File.Create(fileToUpload)))
                        {
                            zipStream.SetLevel(0); //0-9, 9 being the highest level of compression
                            ZipFolder(str1, zipStream);
                        }

                        zipped = true;

                    }

                    var ftpFilePath = string.Format("ftp://{0}:{1}/{2}{3}",
                            (object)Settings.Default.host,
                            (object)Settings.Default.port.ToString(),
                            Settings.Default.folder == "" ? (object)"" : (object)(Settings.Default.folder + "/")
                            , Path.GetFileName(filenameToUpload));





                    if (Settings.Default.prompt_overwrite)
                    {
                        FtpWebRequest ftpWebRequest = (FtpWebRequest)WebRequest.Create(ftpFilePath);
                        ftpWebRequest.Credentials = (ICredentials)new NetworkCredential(Settings.Default.user, Settings.Default.password);
                        ftpWebRequest.UsePassive = Settings.Default.pasv;
                        ftpWebRequest.UseBinary = true;
                        ftpWebRequest.KeepAlive = false;
                        ftpWebRequest.Method = WebRequestMethods.Ftp.GetDateTimestamp;

                        var fileExists = false;
                        try
                        {
                            FtpWebResponse response = (FtpWebResponse)ftpWebRequest.GetResponse();
                            fileExists = true;
                        }
                        catch (WebException ex)
                        {
                            FtpWebResponse response = (FtpWebResponse)ex.Response;
                            if (response.StatusCode ==
                                FtpStatusCode.ActionNotTakenFileUnavailable)
                            {
                                fileExists = false;
                            }
                        }

                        if (fileExists)
                        {
                            if (BackgroundThreadMessageBox(this,
                                                        string.Format("The file\r\n{0}\r\nalready exists.\r\n\r\nDo you want to overwrite it?", Path.GetFileName(filenameToUpload)),
                                                        string.Format("{0} already exists", Path.GetFileName(filenameToUpload)),
                                                        MessageBoxButtons.YesNo,
                                                        MessageBoxIcon.Question,
                                                        MessageBoxDefaultButton.Button2) == System.Windows.Forms.DialogResult.No)
                            {
                                continue;
                            }
                        }
                    }

                    {
                        FtpWebRequest ftpWebRequest = (FtpWebRequest)WebRequest.Create(ftpFilePath);
                        ftpWebRequest.Credentials = (ICredentials)new NetworkCredential(Settings.Default.user, Settings.Default.password);
                        ftpWebRequest.UsePassive = Settings.Default.pasv;
                        ftpWebRequest.UseBinary = true;
                        ftpWebRequest.KeepAlive = false;
                        ftpWebRequest.Method = WebRequestMethods.Ftp.UploadFile;

                        long length = new FileInfo(fileToUpload).Length;
                        string fileSize = frm_main.GetFileSize(length);
                        int count1 = 1024 * 10;
                        int num1 = 0;
                        int num2 = 50;
                        long numBytes = 0L;
                        byte[] buffer = new byte[count1];
                        using (Stream requestStream = ftpWebRequest.GetRequestStream())
                        {
                            using (FileStream fileStream1 = System.IO.File.Open(fileToUpload, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                            {
                                for (int count2 = fileStream1.Read(buffer, 0, count1); count2 > 0; count2 = fileStream1.Read(buffer, 0, count1))
                                {
                                    try
                                    {
                                        requestStream.Write(buffer, 0, count2);
                                        numBytes += (long)count2;
                                        string str2 = string.Format("{0} / {1}", (object)frm_main.GetFileSize(numBytes), (object)fileSize);
                                        this.th_upload.ReportProgress((int)((Decimal)numBytes / (Decimal)length * new Decimal(100)), (object)str2);
                                    }
                                    catch (Exception ex)
                                    {
                                        BackgroundThreadMessageBox(this, "Exception: " + ((object)ex).ToString(), "Error", MessageBoxButtons.OK, MessageBoxIcon.Error, MessageBoxDefaultButton.Button1);
                                        if (num1++ >= num2)
                                            throw new Exception(string.Format("Error occurred during upload, too many retries. \n{0}", (object)((object)ex).ToString()));
                                        FileStream fileStream2 = fileStream1;
                                        long num4 = fileStream2.Position - (long)count2;
                                        fileStream2.Position = num4;
                                    }
                                }
                            }
                        }
                    }

                    if (zipped)
                    {
                        File.Delete(fileToUpload);
                    }


                    if (this.txt_url.Text != string.Empty)
                    {
                        this.clip.Add(string.Format("{0}{1}", (object)Settings.Default.url, HttpUtility.UrlEncode(Path.GetFileName(filenameToUpload))));
                    }
                    else
                    {
                        this.clip.Add(string.Format("http://{0}/{1}{2}", (object)Settings.Default.host, Settings.Default.folder == "" ? (object)"" : (object)(Settings.Default.folder + "/"), HttpUtility.UrlEncode(Path.GetFileName(filenameToUpload))));
                    }
                }
            }
            catch (Exception ex)
            {
                BackgroundThreadMessageBox(this, ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error, MessageBoxDefaultButton.Button1);
            }
        }

        public static string GetFileSize(long numBytes)
        {
            string str = numBytes <= 1073741824L ? (numBytes <= 1048576L ? string.Format("{0:0} Kb", (object)((double)numBytes / 1024.0)) : string.Format("{0:0.00} Mb", (object)((double)numBytes / 1048576.0))) : string.Format("{0:0.00} Gb", (object)((double)numBytes / 1073741824.0));
            if (str == "0 Kb")
                str = "1 Kb";
            return str;
        }

        private void th_upload_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            this.Text = "FTPSchubser";
            this.pb_upload.Value = 0;
            this.tbInfo.ProgressState = TaskbarItemProgressState.None;
            if (Settings.Default.clipboard)
            {
                if (Settings.Default.shorten)
                {
                    this.Text = "Shortening...";
                    this.tbInfo.ProgressState = TaskbarItemProgressState.Indeterminate;
                    this.th_shorten.RunWorkerAsync();
                }
                else
                    this.exitProcedure();
            }
            else
                this.exitProcedure();
        }

        private void exitProcedure()
        {
            this.Text = "FTPSchubser";
            if (Settings.Default.clipboard)
            {
                string str1 = string.Empty;
                foreach (string str2 in this.clip)
                    str1 = string.Format("{0}{1}\n", (object)str1, (object)str2);
                Clipboard.SetDataObject((object)str1, true);
            }
            if (!this.arg)
                return;
            Environment.Exit(0);
        }

        private void t_slider_Tick(object sender, EventArgs e)
        {
            int num1 = 7;
            if (!this.showsettings)
            {
                int num2 = 460;
                if (this.Height < num2)
                {
                    frm_main frmMain = this;
                    int num3 = frmMain.Height + num1;
                    frmMain.Height = num3;
                }
                else
                {
                    this.Height = num2;
                    this.showsettings = !this.showsettings;
                    this.t_slider.Stop();
                }
            }
            else
            {
                int num2 = 165;
                if (this.Height > num2)
                {
                    frm_main frmMain = this;
                    int num3 = frmMain.Height - num1;
                    frmMain.Height = num3;
                }
                else
                {
                    this.Height = num2;
                    this.showsettings = !this.showsettings;
                    this.t_slider.Stop();
                }
            }
        }

        private void btn_settings_Click(object sender, EventArgs e)
        {
            if (!this.showsettings)
                this.btn_settings.Text = "Hide";
            else
                this.btn_settings.Text = "Settings";
            this.t_slider.Start();
        }

        private void btn_save_Click(object sender, EventArgs e)
        {
            try
            {
                Settings.Default.host = this.txt_host.Text;
                Settings.Default.port = Convert.ToInt32(this.txt_port.Text);
                Settings.Default.user = this.txt_user.Text;
                Settings.Default.password = this.txt_password.Text;
                Settings.Default.pasv = this.cb_passive.Checked;
                Settings.Default.clipboard = this.cb_clip.Checked;
                Settings.Default.folder = this.txt_folder.Text;
                Settings.Default.shorten = this.cb_shorten.Checked;
                Settings.Default.url = this.txt_url.Text;
                Settings.Default.shortenservice = this.rdb_googl.Checked ? 1 : 0;
                Settings.Default.prompt_overwrite = this.cb_overwrite.Checked;
                Settings.Default.Save();
                Settings.Default.Reload();
                if (!this.showsettings)
                    this.btn_settings.Text = "Hide";
                else
                    this.btn_settings.Text = "Settings";
                this.t_slider.Start();
            }
            catch (Exception ex)
            {
                BackgroundThreadMessageBox(this, ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error, MessageBoxDefaultButton.Button1);
            }
        }

        private void frm_main_Load(object sender, EventArgs e)
        {
            if (Settings.Default.UpgradeTo2101)
            {
                Settings.Default.Upgrade();
                Settings.Default.UpgradeTo2101 = false;
                Settings.Default.Save();
            }
            this.txt_host.Text = Settings.Default.host;
            this.txt_port.Text = Settings.Default.port.ToString();
            this.txt_user.Text = Settings.Default.user;
            this.txt_password.Text = Settings.Default.password;
            this.cb_passive.Checked = Settings.Default.pasv;
            this.cb_shorten.Checked = Settings.Default.shorten;
            this.txt_folder.Text = Settings.Default.folder;
            this.cb_overwrite.Checked = Settings.Default.prompt_overwrite;
            this.cb_clip.Checked = Settings.Default.clipboard;
            this.cb_shorten.Enabled = Settings.Default.clipboard;
            this.rdb_bitly.Enabled = this.cb_shorten.Enabled && this.cb_shorten.Checked;
            this.rdb_googl.Enabled = this.cb_shorten.Enabled && this.cb_shorten.Checked;
            if (Settings.Default.shortenservice == 1)
            {
                this.rdb_bitly.Checked = false;
                this.rdb_googl.Checked = true;
            }
            else
            {
                this.rdb_bitly.Checked = true;
                this.rdb_googl.Checked = false;
            }
            this.txt_url.Text = Settings.Default.url;
            this.update_preview(sender, e);

            if (string.IsNullOrWhiteSpace(Settings.Default.host))
            {
                btn_settings_Click(null, null);
            }
        }

        private void update_preview(object sender, EventArgs e)
        {
            this.lbl_preview.Text = string.Format("ftp://{0}:{1}{2}/example.jpg", (object)this.txt_host.Text, (object)this.txt_port.Text, this.txt_folder.Text == "" ? (object)"" : (object)("/" + this.txt_folder.Text));
            if (this.txt_url.Text != string.Empty)
                this.lbl_urlpreview.Text = string.Format("{0}example.jpg", (object)this.txt_url.Text);
            else
                this.lbl_urlpreview.Text = string.Format("http://{0}{2}/example.jpg", (object)this.txt_host.Text, (object)this.txt_port.Text, this.txt_folder.Text == "" ? (object)"" : (object)("/" + this.txt_folder.Text));
        }

        private void th_upload_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            if (this.filecount > 1)
                this.Text = this.curfilecount + "/" + this.filecount + " " + e.UserState.ToString();
            else
                this.Text = e.UserState.ToString();
            this.pb_upload.Value = Math.Min(this.pb_upload.Maximum, e.ProgressPercentage);
            this.tbInfo.ProgressValue = (double)e.ProgressPercentage;
        }

        public static XmlDocument GetData(string xmlUrl)
        {
            HttpWebRequest httpWebRequest = (HttpWebRequest)WebRequest.Create(xmlUrl);
            httpWebRequest.UserAgent = "FTPSchubser/" + Application.ProductVersion;
            HttpWebResponse httpWebResponse = (HttpWebResponse)httpWebRequest.GetResponse();
            XmlDocument xmlDocument = new XmlDocument();
            xmlDocument.Load(httpWebResponse.GetResponseStream());
            return xmlDocument;
        }
        
        private void th_shorten_DoWork(object sender, DoWorkEventArgs e)
        {
            try
            {
                this.clipshort = new List<string>();
                if (this.rdb_googl.Checked)
                {
                    foreach (string str1 in this.clip)
                    {
                        string str2 = this.googlShort(str1);
                        if (str2 != string.Empty)
                            this.clipshort.Add(str2);
                    }
                }
                else
                {
                    string xmlUrl = "http://api.bit.ly/shorten?version=2.0.1&format=xml&login=" + BITLYUSER + "&apiKey=" + BITLYKEY;
                    foreach (string str in this.clip)
                        xmlUrl = xmlUrl + string.Format("&longUrl={0}", (object)str);
                    this.xmlDoc = frm_main.GetData(xmlUrl);
                    foreach (XmlNode xmlNode in this.xmlDoc.SelectNodes("bitly/results/nodeKeyVal/shortUrl"))
                        this.clipshort.Add(xmlNode.InnerText);
                }
            }
            catch (Exception ex)
            {
                BackgroundThreadMessageBox(this, ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error, MessageBoxDefaultButton.Button1);
            }
        }

        private string googlShort(string str)
        {
            string str1 = string.Empty;
            WebRequest webRequest = WebRequest.Create("https://www.googleapis.com/urlshortener/v1/url?key=" + GOOGLKEY);
            webRequest.Method = "POST";
            byte[] bytes = Encoding.UTF8.GetBytes("{\"longUrl\": \"" + str + "\"}");
            webRequest.ContentType = "application/json";
            webRequest.ContentLength = (long)bytes.Length;
            Stream requestStream = webRequest.GetRequestStream();
            requestStream.Write(bytes, 0, bytes.Length);
            requestStream.Close();
            WebResponse response = webRequest.GetResponse();
            Stream responseStream = response.GetResponseStream();
            StreamReader streamReader = new StreamReader(responseStream);
            string input = streamReader.ReadToEnd();
            streamReader.Close();
            responseStream.Close();
            response.Close();
            try
            {
                str1 = Regex.Match(input, "\"id\": \"(.*?)\",", RegexOptions.Singleline).Groups[1].Value;
            }
            catch
            {
            }
            return str1;
        }

        private void th_shorten_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            this.tbInfo.ProgressState = TaskbarItemProgressState.None;
            this.clip = this.clipshort;
            this.exitProcedure();
        }

        private void cb_clip_CheckedChanged(object sender, EventArgs e)
        {
            if (!this.cb_clip.Checked)
            {
                this.cb_shorten.Checked = false;
                this.cb_shorten.Enabled = false;
                this.rdb_googl.Enabled = false;
                this.rdb_bitly.Enabled = false;
            }
            else
            {
                this.cb_shorten.Enabled = true;
                if (this.cb_shorten.Checked)
                {
                    this.rdb_googl.Enabled = true;
                    this.rdb_bitly.Enabled = true;
                }
                else
                {
                    this.rdb_googl.Enabled = false;
                    this.rdb_bitly.Enabled = false;
                }
            }
        }

        private void cb_shorten_CheckedChanged(object sender, EventArgs e)
        {
            if (this.cb_shorten.Checked)
            {
                this.rdb_googl.Enabled = true;
                this.rdb_bitly.Enabled = true;
            }
            else
            {
                this.rdb_googl.Enabled = false;
                this.rdb_bitly.Enabled = false;
            }
        }
    }
}
