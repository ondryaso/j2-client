using SIClient.Net;
using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Management;
using System.Threading.Tasks;
using System.Windows.Interop;
using static SIClient.NativeDefinitions;
using static SIClient.NativeMethods;

namespace SIClient
{
    internal class ScreenshotManager
    {
        public event Action<String> ScreenshotDone;

        public event Action<String> Error;

        private bool isW10;
        private INetClient client;
        private ScreenshotTaker scr;

        internal INetClient Client { set { this.client = value; } }

        public ScreenshotManager()
        {
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

            regSelect.RegionSelected += RegionSelected;
            regSelect.Show();
            regSelect.Activate();
            regSelect.Focus();

            ManagedRect r;
            var hwnd = new WindowInteropHelper(regSelect).Handle;

            if (GetWindowRect(hwnd, out r))
            {
                regSelect.XOffset = r.Left;
                regSelect.YOffset = r.Top;
            }
        }

        private void RegionSelected(RegionSelectionWindow.RegionSelectedEventArgs e, object sender)
        {
            var s = this.scr.GetScreenshot(e.X0 + e.XOffset, e.Y0 + e.YOffset, e.X1 + e.XOffset, e.Y1 + e.YOffset);
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

        public void UploadFile(string fileName)
        {
            try
            {
                Image img = Image.FromFile(fileName);
                this.UploadScreenshot(img);
            }
            catch
            {
                this.Error?.Invoke("File is not an image. Sry m8");
            }
        }

        #endregion Toplevel screenshot methods

        #region Screenshot management

        private void UploadScreenshot(Image s)
        {
            if (s != null)
            {
                byte[] b = this.GetImageBytes(s);

                Task.Run(async () =>
                {
                    try
                    {
                        var name = await this.client.UploadImageAsync(b);
                        this.ScreenshotDone?.Invoke(name);
                    }
                    catch (Exception e)
                    {
                        this.Error?.Invoke(e.Message);
                    }
                });
            }
            else
            {
                this.Error?.Invoke("Error, I don't understand :(");
            }
        }

        private byte[] GetImageBytes(Image b)
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