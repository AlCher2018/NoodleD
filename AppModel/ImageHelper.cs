using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;

namespace AppModel
{
    public static class ImageHelper
    {

        public static byte[] getImageFromFilePath(string filePath)
        {
            byte[] retVal;
            FileStream fs = File.Open(filePath, FileMode.Open);
            BinaryReader reader = new BinaryReader(fs);
            retVal = reader.ReadBytes((int)fs.Length);
            reader.Close(); reader.Dispose();
            fs.Close(); fs.Dispose();
            return retVal;
        }

        public static BitmapImage ByteArrayToBitmapImage(byte[] imageData)
        {
            BitmapImage image = null;
            
            // если нет изображения, то вернуть из файла
            if (imageData == null || imageData.Length == 0)
            {
                string filePath = ImageHelper.GetFileNameBy(@"AppImages\no_image.png");
                if (System.IO.File.Exists(filePath) == true)
                {
                    image = new BitmapImage(new Uri(filePath, UriKind.Absolute));
                }
            }
            else
            {
                image = new System.Windows.Media.Imaging.BitmapImage();
                using (var stream = new MemoryStream(imageData))
                {
                    stream.Seek(0, SeekOrigin.Begin);

                    image.BeginInit();
                    image.CreateOptions = BitmapCreateOptions.PreservePixelFormat;
                    image.CacheOption = BitmapCacheOption.OnLoad;
                    image.UriSource = null;
                    image.StreamSource = stream;
                    image.EndInit();
                }
            }

            image.Freeze();
            return image;
        }


        public static string GetFileNameBy(string fileName)
        {
            return string.Format(@"{0}{1}", AppDomain.CurrentDomain.BaseDirectory, fileName);
        }

    }  // class
}
