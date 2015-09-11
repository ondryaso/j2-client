using FMUtils.Screenshot;
using System.Drawing;
using System.Windows;

namespace SIClient
{
    internal class ScreenshotTaker
    {
        public Bitmap GetScreenshot()
        {
            var screen = new ComposedScreenshot(new Rectangle((int)SystemParameters.VirtualScreenLeft, (int)SystemParameters.VirtualScreenTop,
               (int)SystemParameters.VirtualScreenWidth, (int)SystemParameters.VirtualScreenHeight));
            screen.withCursor = true;

            return screen.ComposedScreenshotImage;
        }

        public Bitmap GetScreenshot(int x0, int y0, int x1, int y1)
        {
            var screen = new ComposedScreenshot(new Rectangle(x0, y0, x1 - x0, y1 - y0));
            screen.withCursor = true;

            return screen.ComposedScreenshotImage;
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