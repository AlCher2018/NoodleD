using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using System.Drawing.Imaging;
using System.Windows.Forms;
using System.IO;

namespace UserActionLog
{
    public static class Screenshot
    {
        public static string PutScreenshotToFile(LogFilesPathLocationEnum filePath = LogFilesPathLocationEnum.App_Logs)
        {
            int screenLeft = SystemInformation.VirtualScreen.Left;
            int screenTop = SystemInformation.VirtualScreen.Top;
            int screenWidth = SystemInformation.VirtualScreen.Width;
            int screenHeight = SystemInformation.VirtualScreen.Height;

            // Create a bitmap of the appropriate size to receive the screenshot.
            using (Bitmap bitmap = new Bitmap(screenWidth, screenHeight))
            {
                // Draw the screenshot into our bitmap.
                using (Graphics g = Graphics.FromImage(bitmap))
                {
                    g.CopyFromScreen(screenLeft, screenTop, 0, 0, bitmap.Size);
                }

                // file name to screenshot
                string uniqueFileName = getScreenshotFileName(filePath);

                try
                {
                    bitmap.Save(uniqueFileName, ImageFormat.Jpeg);
                }
                catch (Exception)
                {
                    return string.Empty;
                }
                return uniqueFileName;
            }
        } // PutScreenshotToFile method

        private static string getScreenshotFileName(LogFilesPathLocationEnum filePathEnum)
        {
            string sysPath = LibFuncs.getLogFilesPath(filePathEnum);
            string sysFileName = Path.GetRandomFileName().Replace(".", string.Empty) + ".jpeg";

            if (sysPath != string.Empty)
                return sysPath + sysFileName;
            else
                return string.Empty;
        }

    }  // class

}
