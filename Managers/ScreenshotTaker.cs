using System.Drawing;
using System.Drawing.Imaging;
using System.Windows;

namespace SIClient
{
    internal class ScreenshotTaker
    {
        public Bitmap GetScreenshot()
        {
            Bitmap bmp = new Bitmap((int)SystemParameters.VirtualScreenWidth,
                (int)SystemParameters.VirtualScreenHeight, PixelFormat.Format32bppArgb);

            this.CopyScreen((int)SystemParameters.VirtualScreenLeft, (int)SystemParameters.VirtualScreenTop, ref bmp);
            return bmp;
        }

        public Bitmap GetScreenshot(int x0, int y0, int x1, int y1)
        {
            try
            {
                Bitmap bmp = new Bitmap(x1 - x0, y1 - y0, PixelFormat.Format32bppArgb);
                this.CopyScreen(x0, y0, ref bmp);
                return bmp;
            }
            catch
            {
                return null;
            }
        }

        public void CopyScreen(int x, int y, ref Bitmap dataBmp)
        {
            Graphics g = Graphics.FromImage(dataBmp);

            g.CopyFromScreen(x, y, 0, 0, dataBmp.Size, CopyPixelOperation.SourceCopy);
            g.Flush();
            g.Dispose();
        }
    }
}