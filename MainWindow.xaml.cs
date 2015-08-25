using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace SIClient
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private Button settingButton;
        private List<Key> keys = new List<Key>();

        private Dictionary<Key, ModifierKeys> mods = new Dictionary<Key, ModifierKeys>()
        {
            [Key.LeftCtrl] = ModifierKeys.Control,
            [Key.RightCtrl] = ModifierKeys.Control,
            [Key.LeftShift] = ModifierKeys.Shift,
            [Key.RightShift] = ModifierKeys.Shift,
            [Key.LeftAlt] = ModifierKeys.Alt,
            [Key.RightAlt] = ModifierKeys.Alt,
            [Key.LWin] = ModifierKeys.Windows,
            [Key.RWin] = ModifierKeys.Windows
        };

        private MainWindowVM viewModel;

        public MainWindow()
        {
            InitializeComponent();
            this.viewModel = Application.Current.FindResource("viewmodel") as MainWindowVM;
        }

        private void btn_Click(object sender, RoutedEventArgs e)
        {
            if (this.settingButton == sender)
            {
                this.settingButton.Background = Brushes.LightGray;
                this.settingButton = null;
            }
            else
            {
                this.settingButton = (Button)sender;
                this.settingButton.Background = Brushes.LightSkyBlue;
            }
        }

        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            if (this.settingButton != null)
            {
                if (!this.keys.Contains(e.Key))
                    this.keys.Add(e.Key);
            }
        }

        private void Window_KeyUp(object sender, KeyEventArgs e)
        {
            if (this.settingButton != null)
            {
                string keyString = "";
                bool containsKey = false;

                foreach (Key k in this.keys)
                {
                    if (this.mods.ContainsKey(k))
                    {
                        keyString += this.mods[k].ToString() + "+";
                    }
                    else
                    {
                        if (!containsKey)
                        {
                            keyString += k.ToString();
                            containsKey = true;
                        }
                    }
                }

                if (this.settingButton == this.wholeScreenBtn)
                    this.viewModel.ScreenWholeScreenHotkey = keyString;
                if (this.settingButton == this.cutBtn)
                    this.viewModel.ScreenAreaHotkey = keyString;
                if (this.settingButton == this.windowBtn)
                    this.viewModel.ScreenWindowHotkey = keyString;

                this.settingButton.Background = Brushes.LightGray;
                this.settingButton = null;

                this.keys.Clear();
            }
        }

        private void Window_StateChanged(object sender, System.EventArgs e)
        {
            if (this.WindowState == WindowState.Minimized)
                this.Visibility = Visibility.Hidden;
        }

        private void Window_Closed(object sender, System.EventArgs e)
        {
            Application.Current.Shutdown();
        }
    }
}