using System;
using System.Windows.Input;

namespace SIClient
{
    internal class ShowMainCommand : ICommand
    {
        public event EventHandler CanExecuteChanged;

        public bool CanExecute(object parameter)
        {
            return true;
        }

        public void Execute(object parameter)
        {
            var w = App.Current.MainWindow;
            w.Show();
            w.WindowState = System.Windows.WindowState.Normal;
            w.Activate();
            w.Topmost = true;
            w.Topmost = false;
            w.Focus();
        }
    }
}