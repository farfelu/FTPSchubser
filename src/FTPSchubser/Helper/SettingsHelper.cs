using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FTPSchubser.Helper
{
    // helper to wrap old settings
    class SettingsHelper
    {
        public string Host { get; set; }
        public int Port { get; set; }
        public bool PassiveMode { get; set; }
        public string User { get; set; }
        public string Password { get; set; }
        public string ServerPath { get; set; }
        public string UrlPath { get; set; }
        public bool CheckOverwrite { get; set; }
        public bool CopyClipboard { get; set; }
        public bool Minify { get; set; }
        public bool KeepWindowOpen { get; set; }

        private Properties.Settings Settings { get; set; }
        public SettingsHelper()
        {
            Settings = Properties.Settings.Default;

            if (Settings.RequireUpgrade)
            {
                Settings.Upgrade();
                Settings.RequireUpgrade = false;
                Settings.Save();
            }
            
            // secure the password so it is no longer plaintext in the config
            if (!string.IsNullOrEmpty(Settings.password))
            {
                using (var secureString = Settings.password.ToSecureString())
                {
                    Settings.password_secure = secureString.EncryptString();
                }

                Settings.password = null;

                Settings.Save();
            }

            Reload();
        }

        public void Reload()
        {
            Host = Settings.host;
            Port = Settings.port;
            PassiveMode = Settings.pasv;
            User = Settings.user;

            using (var secureString = Settings.password_secure.DecryptString())
            {
                Password = secureString.ToInsecureString();
            }
            
            ServerPath = Settings.folder;
            UrlPath = Settings.url;
            CheckOverwrite = Settings.prompt_overwrite;
            CopyClipboard = Settings.clipboard;
            Minify = Settings.shorten;
            KeepWindowOpen = Settings.keep_window_open;
        }

        public void Save()
        {
            Settings.host = Host;
            Settings.port = Port;
            Settings.pasv = PassiveMode;
            Settings.user = User;

            using (var secureString = Password.ToSecureString())
            {
                Settings.password_secure = secureString.EncryptString();
            }

            Settings.folder = ServerPath;
            Settings.url = UrlPath;
            Settings.prompt_overwrite = CheckOverwrite;
            Settings.clipboard = CopyClipboard;
            Settings.shorten = Minify;
            Settings.keep_window_open = KeepWindowOpen;

            Settings.Save();
        }
    }
}
