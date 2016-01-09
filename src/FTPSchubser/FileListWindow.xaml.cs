using FTPSchubser.Helper;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace FTPSchubser
{
    /// <summary>
    /// Interaction logic for FileListWindow.xaml
    /// </summary>
    public partial class FileListWindow : Window
    {
        public FileListWindow(Window owner)
        {
            this.Owner = owner;
            InitializeComponent();

            FillDatagrid();
        }

        private ObservableCollection<FTPFileListItem> Files { get; set; }

        private FTPHelper FTP { get; set; } = new FTPHelper(App.Settings.Host, App.Settings.User, App.Settings.Password, App.Settings.ServerPath, App.Settings.Port, App.Settings.PassiveMode);

        private void ShowProgress(string text = null)
        {
            gridFiles.Visibility = Visibility.Hidden;
            progress_bar.Visibility = Visibility.Visible;
            progress_label.Visibility = Visibility.Visible;
            progress_label.Content = text;
            this.Cursor = Cursors.Wait;

            btnCopy.IsEnabled = false;
            btnDelete.IsEnabled = false;
            btnRefresh.IsEnabled = false;
        }

        private void HideProgress()
        {
            gridFiles.Visibility = Visibility.Visible;
            progress_bar.Visibility = Visibility.Hidden;
            progress_label.Visibility = Visibility.Hidden;
            progress_label.Content = null;
            this.Cursor = null;

            btnCopy.IsEnabled = true;
            btnDelete.IsEnabled = true;
            btnRefresh.IsEnabled = true;
        }

        private async Task FillDatagrid()
        {
            ShowProgress("looking up files");

            Files = null;
            
            var files = (await FTP.ListFilesAsync())
                        .Where(x => !x.IsDirectory)
                        .OrderByDescending(x => x.Timestamp)
                        .Select(x => new FTPFileListItem()
            {
                Timestamp = x.Timestamp,
                Size = x.Size,
                SizeReadable = Utils.BytesToString(x.Size),
                Filename = x.Filename,
                Url = Utils.FormatHTTPUrl(App.Settings.UrlPath, App.Settings.Host, App.Settings.ServerPath, x.Filename)
            });
            Files = new ObservableCollection<FTPFileListItem>(files);
            gridFiles.ItemsSource = Files;
            var broken = Files.Where(x => string.IsNullOrWhiteSpace(x.Filename)).ToList();
            HideProgress();
        }

        private async Task CopyLinksAsync(IEnumerable<FTPFileListItem> files)
        {
            var urls = files.Select(x => x.Url);

            if (App.Settings.Minify)
            {
                ShowProgress("minifying links");

                urls = await URLHelper.ShortenUrlsAsync(urls);

                HideProgress();
            }

            // always add newline at the end
            Clipboard.SetText(string.Join("\r\n", urls) + "\r\n");

            // uncheck the copied files and mark as copied
            foreach (var file in files)
            {
                file.ToCopy = false;
                file.IsCopied = true;
            }
        }

        private void btnListcopy_Click(object sender, RoutedEventArgs e)
        {
            var obj = ((FrameworkElement)sender).DataContext as FTPFileListItem;
            if (obj != null)
            {
                System.Diagnostics.Process.Start(obj.Url);
            }
        }

        private async void btnCopy_Click(object sender, RoutedEventArgs e)
        {
            if (!Files.Any(x => x.ToCopy))
            {
                return;
            }

            foreach (var copied in Files.Where(x => x.IsCopied))
            {
                copied.IsCopied = false;
            }

            var files = Files.Where(x => x.ToCopy);

            await CopyLinksAsync(files);
        }

        private void btnRefresh_Click(object sender, RoutedEventArgs e)
        {
            FillDatagrid();
        }

        private async void btnDelete_Click(object sender, RoutedEventArgs e)
        {
            if (!Files.Any(x => x.ToDelete))
            {
                return;
            }

            var files = Files.Where(x => x.ToDelete).Select(x => x.Filename);

            await FTP.DeleteFilesAsync(files);
            FillDatagrid();
        }

        private async void gridFiles_RowDoubleClick(object sender, MouseButtonEventArgs e)
        {
            var row = sender as DataGridRow;
            if (row == null)
            {
                return;
            }

            var file = row.Item as FTPFileListItem;

            if (file == null)
            {
                return;
            }
            
            await CopyLinksAsync(new FTPFileListItem[] { file });
        }

        private class FTPFileListItem : INotifyPropertyChanged
        {
            public DateTime? Timestamp { get; set; }
            public long Size { get; set; }
            public string SizeReadable { get; set; }
            public string Filename { get; set; }
            public string Url { get; set; }

            private bool toCopy = false;
            public bool ToCopy
            {
                get { return toCopy; }
                set
                {
                    if (value)
                    {
                        ToDelete = false;
                    }

                    SetField(ref toCopy, value);
                }
            }

            private bool toDelete = false;
            public bool ToDelete
            {
                get { return toDelete; }
                set
                {
                    if (value)
                    {
                        ToCopy = false;
                    }

                    SetField(ref toDelete, value);
                }
            }

            private bool isCopied = false;
            public bool IsCopied
            {
                get { return isCopied; }
                set { SetField(ref isCopied, value); }
            }

            public event PropertyChangedEventHandler PropertyChanged;
            protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
            {
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
            }
            protected bool SetField<T>(ref T field, T value, [CallerMemberName] string propertyName = null)
            {
                if (EqualityComparer<T>.Default.Equals(field, value)) return false;
                field = value;
                OnPropertyChanged(propertyName);
                return true;
            }

        }
    }
}
