using System.Windows;

namespace SIClient
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private void Application_Exit(object sender, ExitEventArgs e)
        {
            NHotkey.Wpf.HotkeyManager.Current.Remove("Shortcut.ScreenWindow");
            NHotkey.Wpf.HotkeyManager.Current.Remove("Shortcut.ScreenWholeScreen");
            NHotkey.Wpf.HotkeyManager.Current.Remove("Shortcut.ScreenArea");
            Settings.Default.Save();
        }

        private void Application_Startup(object sender, StartupEventArgs e)
        {
            this.FindResource("notifyicon");
        }

        private void LinkBalloonClicked(object sender, RoutedEventArgs e)
        {
            System.Diagnostics.Process.Start(Clipboard.GetText());
        }
    }
}