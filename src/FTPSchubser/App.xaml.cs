using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace FTPSchubser
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        internal static Helper.SettingsHelper Settings { get; private set; } 

        public App()
        {
            Settings = new Helper.SettingsHelper();
        }

        private void Application_Startup(object sender, StartupEventArgs e)
        {
            var wnd = new MainWindow(e.Args);
            wnd.Show();
        }
    }
}
