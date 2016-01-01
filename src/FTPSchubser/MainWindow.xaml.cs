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
        public MainWindow()
        {
            InitializeComponent();
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
            var fileList = files.ToList();
            var ftp = new Helper.FTPHelper(App.Settings.Host, App.Settings.User, App.Settings.Password, App.Settings.ServerPath, App.Settings.Port, App.Settings.PassiveMode);

            if (App.Settings.CheckOverwrite)
            {
                progress_bar.IsIndeterminate = true;
                var existingFiles = await ftp.GetExistingFiles(fileList);
                if (existingFiles.Any())
                {
                    var result = MessageBox.Show(this, "The following files will be overwritten:\r\n" + string.Join("\r\n", existingFiles.Select(x => System.IO.Path.GetFileName(x))), "Confirm overwrite", MessageBoxButton.YesNo, MessageBoxImage.Information);
                    if (result != MessageBoxResult.Yes)
                    {
                        // remove the files from our list if we don't want to overwrite them
                        fileList.RemoveAll(x => existingFiles.Contains(x));
                    }
                }
                progress_bar.IsIndeterminate = false;
            }

            await ftp.UploadFilesAsync(fileList, new Progress<Helper.FTPHelper.FTPProgress>((x =>
            {
                progress_bar.Maximum = x.BytesTotal;
                progress_bar.Value = x.BytesDone;
                progress_label.Content = x.ToString();
            })));

            resetProgressBar();

            if (fromCommandline && !App.Settings.KeepWindowOpen)
            {
                Application.Current.Shutdown();
            }
        }
    }
}
