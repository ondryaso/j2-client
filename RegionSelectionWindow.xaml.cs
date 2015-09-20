using System;
using System.Drawing;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media.Imaging;

namespace SIClient
{
    /// <summary>
    /// Interaction logic for RegionSelectionWindow.xaml
    /// </summary>
    public partial class RegionSelectionWindow : Window
    {
        public event Action<int, int, int, int, int, int> RegionSelected;

        private bool selStarted = false;
        private bool magnifying = false;
        private System.Windows.Point start;
        private System.Windows.Point local;
        private ScreenshotTaker scr;
        private IntPtr lastMagHnd;

        public int XOffset { get; set; }
        public int YOffset { get; set; }

        public RegionSelectionWindow()
        {
            InitializeComponent();

            this.scr = new ScreenshotTaker();
        }

        private void Window_MouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (!this.selStarted)
            {
                this.selStarted = true;
                this.selectRectangle.Visibility = Visibility.Visible;
                this.start = e.GetPosition(this);

                Canvas.SetLeft(this.selectRectangle, this.start.X);
                Canvas.SetTop(this.selectRectangle, this.start.Y);
            }
        }

        private void Window_MouseUp(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (this.selStarted)
            {
                this.Hide();
                this.RegionSelected?.Invoke((int)(this.start.X > this.local.X ? this.local.X : this.start.X),
                    (int)(this.start.Y > this.local.Y ? this.local.Y : this.start.Y),
                    (int)(this.start.X > this.local.X ? this.start.X : this.local.X),
                    (int)(this.start.Y > this.local.Y ? this.start.Y : this.local.Y),
                    this.XOffset, this.YOffset);
                this.Close();
            }
        }

        private void Window_MouseMove(object sender, MouseEventArgs e)
        {
            this.local = e.GetPosition(this);

            if (this.selStarted)
            {
                if (this.local.X >= this.start.X)
                {
                    Canvas.SetLeft(this.selectRectangle, this.start.X);
                    this.selectRectangle.Width = this.local.X - this.start.X;
                }
                else
                {
                    Canvas.SetLeft(this.selectRectangle, this.local.X);
                    this.selectRectangle.Width = this.start.X - this.local.X;
                }

                if (this.local.Y >= this.start.Y)
                {
                    Canvas.SetTop(this.selectRectangle, this.start.Y);
                    this.selectRectangle.Height = this.local.Y - this.start.Y;
                }
                else
                {
                    Canvas.SetTop(this.selectRectangle, this.local.Y);
                    this.selectRectangle.Height = this.start.Y - this.local.Y;
                }
            }

            if (this.magnifying)
            {
                Canvas.SetLeft(this.magImgBorder, this.local.X + 10);
                Canvas.SetTop(this.magImgBorder, this.local.Y + 10);
                Canvas.SetLeft(this.magImgCross, this.local.X + 54);
                Canvas.SetTop(this.magImgCross, this.local.Y + 54);
            }
        }

        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape)
            {
                this.Close();
            }

            if (e.Key == Key.M)
            {
                this.magnifying = !this.magnifying;

                this.magImg.Visibility = this.magnifying ? Visibility.Visible : Visibility.Hidden;
                this.magImgBorder.Visibility = this.magImg.Visibility;
                this.magImgCross.Visibility = this.magImg.Visibility;

                if (this.magnifying)
                {
                    Task.Run(() =>
                    {
                        while (this.magnifying)
                        {
                            if (this.lastMagHnd != IntPtr.Zero)
                                NativeMethods.DeleteObject(this.lastMagHnd);

                            Bitmap dataBmp = new Bitmap(10, 10, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
                            this.scr.CopyScreen((int)this.local.X - 5 + this.XOffset, (int)this.local.Y - 5 + this.YOffset, ref dataBmp);

                            this.lastMagHnd = dataBmp.GetHbitmap();

                            Application.Current.Dispatcher.Invoke(() =>
                            this.magImg.Source = Imaging.CreateBitmapSourceFromHBitmap(this.lastMagHnd,
                                 IntPtr.Zero, Int32Rect.Empty, BitmapSizeOptions.FromEmptyOptions()));
                        }
                    });
                }
            }
        }

        private void Window_SourceInitialized(object sender, EventArgs e)
        {
            this.WindowState = WindowState.Maximized;
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            this.magnifying = false;
        }
    }
}