using SIClient.Net;
using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Management;
using System.Threading.Tasks;
using System.Windows;
using static SIClient.NativeDefinitions;
using static SIClient.NativeMethods;

namespace SIClient
{
    internal class ScreenshotManager
    {
        public event Action<String> ScreenshotDone;

        private bool isW10;
        private NetClient client;
        private ScreenshotTaker scr;

        public ScreenshotManager(NetClient c)
        {
            this.client = c;
            this.scr = new ScreenshotTaker();

            // .NET methods return Windows 8 info on Windows 10, so we need to use WMI
            this.isW10 = (from x in new ManagementObjectSearcher("SELECT * FROM Win32_OperatingSystem").Get().OfType<ManagementObject>() select x.GetPropertyValue("Caption")).First().ToString().Contains("Windows 10");
        }

        #region Toplevel screenshot methods

        public void WholescreenScreenshot()
        {
            var s = this.scr.GetScreenshot();
            this.UploadScreenshot(s);
        }

        public void RegionScreenshot()
        {
            var regSelect = new RegionSelectionWindow();

            regSelect.Left = SystemParameters.VirtualScreenLeft;
            regSelect.Top = SystemParameters.VirtualScreenTop;
            regSelect.Width = SystemParameters.VirtualScreenWidth;
            regSelect.Height = SystemParameters.VirtualScreenHeight;

            regSelect.RegionSelected += RegionSelected;
            regSelect.Show();
            regSelect.Activate();
            regSelect.Focus();
        }

        private void RegionSelected(int x0, int y0, int x1, int y1)
        {
            var s = this.scr.GetScreenshot(x0, y0, x1, y1);
            this.UploadScreenshot(s);
        }

        public void WindowScreenshot()
        {
            ManagedRect r;
            var hwnd = GetForegroundWindow();

            if (GetWindowRect(hwnd, out r))
            {
                // On Windows 10, GetWindowRect behaves as on Windows 8, causing the borders to be too big
                if (this.isW10)
                {
                    ManagedWindowPlacement p;
                    if (GetWindowPlacement(hwnd, out p))
                        if (p.showCmd == 3)
                        {
                            r.Top += 8;
                        }

                    r.Left += 7;
                    r.Bottom -= 7;
                    r.Right -= 7;
                }

                var s = this.scr.GetScreenshot(r.Left, r.Top, r.Right, r.Bottom);
                this.UploadScreenshot(s);
            }
        }

        #endregion Toplevel screenshot methods

        #region Screenshot management

        private void UploadScreenshot(Bitmap s)
        {
            if (s != null)
            {
                byte[] b = this.GetImageBytes(s);

                Task.Run(async () =>
                {
                    var name = await this.client.UploadImageAsync(b);
                    this.ScreenshotDone?.Invoke(name);
                });
            }
        }

        private byte[] GetImageBytes(Bitmap b)
        {
            byte[] buf;

            using (MemoryStream ms = new MemoryStream())
            {
                b.Save(ms, ImageFormat.Png);
                buf = ms.ToArray();
            }

            return buf;
        }

        #endregion Screenshot management
    }
}