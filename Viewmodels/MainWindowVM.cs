using Hardcodet.Wpf.TaskbarNotification;
using Microsoft.Win32;
using NHotkey.Wpf;
using SIClient.Managers;
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
        private NetClientSelector client;
        private readonly ScreenshotManager screen;
        private RegistryKey regKey;

        #region ViewModel

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

        #endregion ViewModel

        public MainWindowVM()
        {
            this.client = new NetClientSelector();
            this.screen = new ScreenshotManager();

            this.client.OnClientChanged += OnClientChanged;
            this.ChangeServerAddress(Settings.Default.Address);
            this.ChangeTcpServerAddress(Settings.Default.TcpAddress);

            this.client.HttpClient.DefaultResponseManagerName = Settings.Default.ResponseManagerName;

            this.screen.ScreenshotDone += this.NotifyScreenshotDone;

            this.InitKeys();
        }

        private void NotifyScreenshotDone(string name)
        {
            var link = this.client.HttpClient.ServerAddress + "i" + name;

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
                    this.ChangeServerAddress(value);
                }
            }
        }

        private void ChangeServerAddress(String addr)
        {
            if (Uri.IsWellFormedUriString(addr, UriKind.Absolute))
            {
                this.client.HttpAddress = addr;
            }
        }

        public string TcpServerAddress
        {
            get
            {
                return Settings.Default.TcpAddress;
            }

            set
            {
                String addr = Settings.Default.TcpAddress;
                if (this.SetField(ref addr, value))
                {
                    Settings.Default.TcpAddress = addr;
                    this.ChangeTcpServerAddress(value);
                }
            }
        }

        private void ChangeTcpServerAddress(String addr)
        {
            if (Settings.Default.EnableTcp)
            {
                if (Uri.IsWellFormedUriString(addr, UriKind.Absolute))
                {
                    this.client.TcpAddress = new Uri(addr);
                }
                else
                {
                    this.client.ForceHttp();
                }
            }
        }

        private bool enableTcp = Settings.Default.EnableTcp;

        public bool EnableTcpAddress
        {
            get
            {
                return this.enableTcp;
            }
            set
            {
                if (this.SetField(ref this.enableTcp, value))
                {
                    Settings.Default.EnableTcp = this.enableTcp;

                    if (value)
                    {
                        this.ChangeTcpServerAddress(Settings.Default.TcpAddress);
                    }
                    else
                    {
                        this.client.ForceHttp();
                    }
                }
            }
        }

        private void OnClientChanged(NetClientSelector.ClientChangedEventArgs e, object sender)
        {
            lock (sender)
            {
                this.screen.Client = e.Client;
            }
        }
    }
}