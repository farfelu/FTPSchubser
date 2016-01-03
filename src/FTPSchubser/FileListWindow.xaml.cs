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

        private async Task FillDatagrid()
        {
            var ftp = new FTPHelper(App.Settings.Host, App.Settings.User, App.Settings.Password, App.Settings.ServerPath, App.Settings.Port, App.Settings.PassiveMode);
            var files = await ftp.ListFilesAsync();
            
            gridFiles.ItemsSource = files.Select(x => new
            {
                x.IsDirectory,
                x.Timestamp,
                Size = Utils.BytesToString(x.Size),
                x.Filename,
                URL = Utils.FormatHTTPUrl(App.Settings.UrlPath, App.Settings.Host, App.Settings.ServerPath, x.Filename)
            });
        }
    }
}
