using FTPSchubser.Helper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace FTPSchubser
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow(string[] args = null)
        {
            InitializeComponent();

            if (args != null && args.Length > 0)
            {
                UploadFilesAsync(args, true);
            }
        }

        private void btn_settings_Click(object sender, RoutedEventArgs e)
        {
            var settingsWindow = new SettingsWindow(this);
            settingsWindow.ShowDialog();
        }

        private void Window_Drop(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);
                UploadFilesAsync(files);       
            }
        }

        private void resetProgressBar()
        {
            progress_bar.Value = 0;
            progress_bar.Maximum = 100;
            progress_bar.IsIndeterminate = false;
            progress_label.Content = " ";
        }

        private async Task UploadFilesAsync(IEnumerable<string> files, bool fromCommandline = false)
        {
            resetProgressBar();
            var ftp = new FTPHelper(App.Settings.Host, App.Settings.User, App.Settings.Password, App.Settings.ServerPath, App.Settings.Port, App.Settings.PassiveMode);

            // create file and directory list
            var fileList = files.OrderBy(x => x).ToList();
            var dirList = fileList.Where(x => Utils.IsDirectory(x)).ToList();

            // remove directories from filelist
            fileList.RemoveAll(x => dirList.Contains(x));

            var uploadList = new List<UploadFile>(fileList.Select(x => new UploadFile(x)));

            if (dirList.Count > 0)
            {
                progress_bar.IsIndeterminate = true;
                var zippedFiles = await ZIPHelper.ZipDirectoriesAsync(dirList, new Progress<ZIPHelper.ZIPProgress>((x =>
                {
                    progress_bar.IsIndeterminate = false;
                    progress_bar.Maximum = x.BytesTotal;
                    progress_bar.Value = x.BytesDone;
                    progress_label.Content = "ZIP: " + x.ToString();
                })));
                uploadList.AddRange(zippedFiles);
                progress_bar.IsIndeterminate = false;
            }

            uploadList = uploadList.OrderBy(x => x.FileInfo.Name).ToList();

            if (App.Settings.CheckOverwrite)
            {
                progress_bar.IsIndeterminate = true;
                var existingFiles = (await ftp.GetExistingFiles(uploadList)).ToList();
                if (existingFiles.Any())
                {
                    var result = MessageBox.Show(this, "The following files will be overwritten:\r\n" + string.Join("\r\n", existingFiles.Select(x => x.FileInfo.Name)), "Confirm overwrite", MessageBoxButton.YesNo, MessageBoxImage.Information);
                    if (result != MessageBoxResult.Yes)
                    {
                        // remove the files from our list if we don't want to overwrite them
                        uploadList.RemoveAll(x => existingFiles.Contains(x));
                    }
                }
                progress_bar.IsIndeterminate = false;
            }

            var uploadedFiles = await ftp.UploadFilesAsync(uploadList, new Progress<Helper.FTPHelper.FTPProgress>((x =>
            {
                progress_bar.Maximum = x.BytesTotal;
                progress_bar.Value = x.BytesDone;
                progress_label.Content = "Upload: " + x.ToString();
            })));

            if (App.Settings.CopyClipboard)
            {
                var urls = uploadedFiles.Select(x => Utils.FormatHTTPUrl(App.Settings.UrlPath, App.Settings.Host, App.Settings.ServerPath, x.UploadName));

                if (App.Settings.Minify)
                {
                    urls = await URLHelper.ShortenUrlsAsync(urls);
                }

                // always add newline at the end
                Clipboard.SetText(string.Join("\r\n", urls) + "\r\n");
            }

            // clean up temp files
            foreach (var uf in uploadedFiles.Where(x => x.IsTemp))
            {
                System.IO.File.Delete(uf.FilePath);
            }            

            resetProgressBar();

            if (fromCommandline && !App.Settings.KeepWindowOpen)
            {
                Application.Current.Shutdown();
            }
        }
    }
}
