using Hardcodet.Wpf.TaskbarNotification;
using Microsoft.Win32;
using NHotkey.Wpf;
using SIClient.Net;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Security.Principal;
using System.Windows;

namespace SIClient
{
    internal class MainWindowVM : INotifyPropertyChanged, IDisposable
    {
        private NetClient client;
        private readonly ScreenshotManager screen;
        private RegistryKey regKey;

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName)
        {
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        protected bool SetField<T>(ref T field, T value, [CallerMemberName] string propertyName = "")
        {
            if (EqualityComparer<T>.Default.Equals(field, value)) return false;
            field = value;
            this.OnPropertyChanged(propertyName);
            return true;
        }

        protected void SetField([CallerMemberName] string propertyName = "")
        {
            this.OnPropertyChanged(propertyName);
        }

        public MainWindowVM()
        {
            this.client = new NetClient(Settings.Default.Address);
            this.client.DefaultResponseManagerName = Settings.Default.ResponseManagerName;

            this.screen = new ScreenshotManager(this.client);
            this.screen.ScreenshotDone += this.NotifyScreenshotDone;

            this.InitKeys();
        }

        private void NotifyScreenshotDone(string name)
        {
#if REL_HOME
            var link = (this.client.ServerAddress.Contains("10.0.0.40") ?
                    "http://home.ondryaso.eu/" : this.client.ServerAddress) + name;
#else
            var link = this.client.ServerAddress + name;
#endif

            Application.Current.Dispatcher.Invoke(() =>
            {
                Clipboard.SetText(link);
                ((TaskbarIcon)Application.Current.FindResource("notifyicon")).ShowBalloonTip("Uploaded", link, BalloonIcon.Info);
            });
        }

        private void InitKeys(bool window = true, bool whole = true, bool area = true)
        {
            try
            {
                if (window)
                    HotkeyManager.Current.AddOrReplace("Shortcut.ScreenWindow", Settings.Default.GetKey(Settings.Default.WindowKeys),
                    Settings.Default.GetModifier(Settings.Default.WindowKeys), (s, e) => this.screen.WindowScreenshot());

                if (whole)
                    HotkeyManager.Current.AddOrReplace("Shortcut.ScreenWholeScreen", Settings.Default.GetKey(Settings.Default.WholeScreenKeys),
                        Settings.Default.GetModifier(Settings.Default.WholeScreenKeys), (s, e) => this.screen.WholescreenScreenshot());

                if (area)
                    HotkeyManager.Current.AddOrReplace("Shortcut.ScreenArea", Settings.Default.GetKey(Settings.Default.CutKeys),
                                    Settings.Default.GetModifier(Settings.Default.CutKeys), (s, e) => this.screen.RegionScreenshot());
            }
            catch
            {
                MessageBox.Show("Couldn't register keys");
            }
        }

        public void Dispose()
        {
            this.regKey.Dispose();
        }

        private RegistryKey RegistryKey
        {
            get
            {
                if (this.regKey == null)
                    this.regKey = Registry.CurrentUser.OpenSubKey("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run", this.RunOnStartupEnabled);

                return this.regKey;
            }
        }

        public bool? RunOnStartup
        {
            get
            {
                return !String.IsNullOrWhiteSpace(this.RegistryKey.GetValue("J2_client", null) as string);
            }

            set
            {
                if (value == true)
                    this.RegistryKey.SetValue("J2_client", System.Reflection.Assembly.GetExecutingAssembly().Location);
                else
                    this.RegistryKey.DeleteValue("J2_client");
            }
        }

        public bool RunOnStartupEnabled
        {
            get
            {
                WindowsPrincipal principal = new WindowsPrincipal(WindowsIdentity.GetCurrent());
                return principal.IsInRole(WindowsBuiltInRole.Administrator);
            }
        }

        public string ScreenWindowHotkey
        {
            get
            {
                return Settings.Default.WindowKeys;
            }

            set
            {
                Settings.Default.WindowKeys = value;
                this.SetField();
                this.InitKeys(true, false, false);
            }
        }

        public string ScreenWholeScreenHotkey
        {
            get
            {
                return Settings.Default.WholeScreenKeys;
            }

            set
            {
                Settings.Default.WholeScreenKeys = value;
                this.SetField();
                this.InitKeys(false, true, false);
            }
        }

        public string ScreenAreaHotkey
        {
            get
            {
                return Settings.Default.CutKeys;
            }

            set
            {
                Settings.Default.CutKeys = value;
                this.SetField();
                this.InitKeys(false, false, true);
            }
        }

        public string ServerAddress
        {
            get
            {
                return Settings.Default.Address;
            }

            set
            {
                String addr = Settings.Default.Address;
                if (this.SetField(ref addr, value))
                {
                    Settings.Default.Address = addr;

                    if (Uri.IsWellFormedUriString(value, UriKind.Absolute))
                        this.client.ServerAddress = value;
                }
            }
        }
    }
}