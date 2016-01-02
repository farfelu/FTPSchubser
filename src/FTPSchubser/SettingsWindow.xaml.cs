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
using System.Windows.Shapes;

namespace FTPSchubser
{
    /// <summary>
    /// Interaction logic for SettingsWindow.xaml
    /// </summary>
    public partial class SettingsWindow : Window
    {
        public SettingsWindow(Window owner)
        {
            this.Owner = owner;
            InitializeComponent();

            txtHost.Text = App.Settings.Host;
            txtPort.Text = App.Settings.Port.ToString();
            cbPassiveMode.IsChecked = App.Settings.PassiveMode;
            txtUser.Text = App.Settings.User;
            txtPassword.Password = App.Settings.Password;
            txtServerPath.Text = App.Settings.ServerPath;
            txtUrlPath.Text = App.Settings.UrlPath;
            cbCheckOverwrite.IsChecked = App.Settings.CheckOverwrite;
            cbCopyClipboard.IsChecked = App.Settings.CopyClipboard;
            cbMinify.IsChecked = App.Settings.Minify;
            cbKeepWindowOpen.IsChecked = App.Settings.KeepWindowOpen;

            txtHost.Focus();
            txtHost.SelectAll();
        }

        private void updateServerUrlPathLabel(object sender, TextChangedEventArgs e)
        {
            int? port = null;
            int temp = 0;
            if (int.TryParse(txtPort.Text, out temp))
            {
                port = temp;
            }

            lblServerPathPreview.Content = Helper.Utils.FormatFTPUrl(txtHost.Text, txtServerPath.Text, "example.jpg", port);
            lblUrlPathPreview.Content = Helper.Utils.FormatHTTPUrl(txtUrlPath.Text, txtHost.Text, txtServerPath.Text, "example.jpg");
        }

        private void btn_save_Click(object sender, RoutedEventArgs e)
        {
            App.Settings.Host = txtHost.Text;
            App.Settings.Port = int.Parse(txtPort.Text);
            App.Settings.PassiveMode = cbPassiveMode.IsChecked.Value;
            App.Settings.User = txtUser.Text;
            App.Settings.Password = txtPassword.Password;
            App.Settings.ServerPath = txtServerPath.Text;
            App.Settings.UrlPath = txtUrlPath.Text;
            App.Settings.CheckOverwrite = cbCheckOverwrite.IsChecked.Value;
            App.Settings.CopyClipboard = cbCopyClipboard.IsChecked.Value;
            App.Settings.Minify = cbMinify.IsChecked.Value;
            App.Settings.KeepWindowOpen = cbKeepWindowOpen.IsChecked.Value;

            App.Settings.Save();

            this.Close();
        }

        private void btn_cancel_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void txtPort_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            // check if entered text is a valid number
            var temp = 0;
            e.Handled = !int.TryParse(e.Text, out temp);
        }

        private void SelectAllKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
        {
            if ((sender as TextBox) != null)
            {
                (sender as TextBox).SelectAll();
            }
            else if ((sender as PasswordBox) != null)
            {
                (sender as PasswordBox).SelectAll();
            }
        }
    }
}
