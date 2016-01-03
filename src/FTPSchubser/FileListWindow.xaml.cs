using FTPSchubser.Helper;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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

        private IEnumerable<FTPFileListItem> Files { get; set; }

        private async Task FillDatagrid()
        {
            var ftp = new FTPHelper(App.Settings.Host, App.Settings.User, App.Settings.Password, App.Settings.ServerPath, App.Settings.Port, App.Settings.PassiveMode);
            var files = (await ftp.ListFilesAsync())
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

            gridFiles.Visibility = Visibility.Visible;
            progress.Visibility = Visibility.Hidden;
            this.Cursor = null;
        }

        private void btnListcopy_Click(object sender, RoutedEventArgs e)
        {
            var obj = ((FrameworkElement)sender).DataContext as FTPFileListItem;
            System.Diagnostics.Process.Start(obj.Url);
        }

        private void btnCopy_Click(object sender, RoutedEventArgs e)
        {
            var checkedFiles = Files.Where(x => x.ToCopy).ToList();
        }

        private void btnRefresh_Click(object sender, RoutedEventArgs e)
        {

        }

        private void btnDelete_Click(object sender, RoutedEventArgs e)
        {

        }

        private class FTPFileListItem
        {
            public DateTime? Timestamp { get; set; }
            public long Size { get; set; }
            public string SizeReadable { get; set; }
            public string Filename { get; set; }
            public string Url { get; set; }
            public bool ToCopy { get; set; }
            public bool ToDelete { get; set; }
        }
    }
}
