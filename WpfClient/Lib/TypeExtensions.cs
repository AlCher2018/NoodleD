using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;

namespace WpfClient
{
    public static class StringExtensions
    {
        public static double GetDoubleValue(this string sParam)
        {
            double retVal = 0;
            double.TryParse(sParam, out retVal);
            if (retVal == 0)
            {
                if (sParam.Contains(".")) sParam = sParam.Replace('.', ',');
                else if (sParam.Contains(",")) sParam = sParam.Replace(',', '.');
                double.TryParse(sParam, out retVal);
            }
            return retVal;
        }
    }

    // расширения класса System.Windows.Media.ImageSource / System.Windows.Controls.Image
    public static class BitmapImageExtensions
    {
        public static BitmapImage FromByteArray(this BitmapImage src, Byte[] bytes)
        {
            var stream = new System.IO.MemoryStream(bytes);
            stream.Seek(0, System.IO.SeekOrigin.Begin);
            var image = new BitmapImage();
            image.BeginInit();
            image.StreamSource = stream;
            image.EndInit();
            return image;
        }
    }


    // расширения класса System.Drawing.Image
    public static class DrawingImageExtension
    {
        public static System.Drawing.Image FromByteArray(this System.Drawing.Image source, byte[] byteArrayIn)
        {
            if (byteArrayIn != null)
            {
                System.Drawing.Image returnImage;
                using (System.IO.MemoryStream ms = new System.IO.MemoryStream(byteArrayIn))
                {
                    returnImage = System.Drawing.Image.FromStream(ms);
                }
                return returnImage;
            }
            return null;
        }

    }

}
