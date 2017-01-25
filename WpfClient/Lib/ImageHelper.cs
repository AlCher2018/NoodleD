using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WpfClient
{
    public static class ImageHelper
    {
        private static Dictionary<string, System.Windows.Media.Imaging.BitmapImage> _images = new Dictionary<string, System.Windows.Media.Imaging.BitmapImage>();

        internal static void CheckDirectory(bool isDeleteFiles)
        {
            var directoryName = AppDomain.CurrentDomain.BaseDirectory + "images\\";
            DirectoryInfo di = new DirectoryInfo(directoryName);
            if (!di.Exists)
            {
                di.Create();
            }
            else
            {
                foreach (FileInfo fi in di.GetFiles())
                {
                    try
                    {
                        if (isDeleteFiles)
                        {
                            fi.Delete();
                        }
                        
                    }
                    catch (System.IO.IOException e) { }
                }
            }
            di = null;
        }

        internal static void SaveImageFile(byte[] imageByteArr,string filePath,string fileName )
        {
            System.Drawing.Image im = ImageHelper.FromByteArray(imageByteArr);
            if (im != null)
            {
                im.Save(fileName);
            }

        }

        public static System.Drawing.Image FromByteArray(byte[] byteArrayIn)
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

        public static System.Windows.Media.Imaging.BitmapImage ByteArrayToBitmapImage(byte[] imageData)
        {
            if (imageData == null || imageData.Length == 0) return null;

            var image = new System.Windows.Media.Imaging.BitmapImage();

            using (var stream = new MemoryStream(imageData))
            {
                stream.Seek(0, SeekOrigin.Begin);

                image.BeginInit();
                image.CreateOptions = System.Windows.Media.Imaging.BitmapCreateOptions.PreservePixelFormat;
                image.CacheOption = System.Windows.Media.Imaging.BitmapCacheOption.OnLoad;
                image.UriSource = null;
                image.StreamSource = stream;
                image.EndInit();
            }

            image.Freeze();
            return image;
        }

        public static System.Windows.Media.Imaging.BitmapImage DrawingImageToBitmapImage(System.Drawing.Image dImage)
        {
            if (dImage == null) return null;

            var image = new System.Windows.Media.Imaging.BitmapImage();

            using (var stream = new MemoryStream())
            {
                dImage.Save(stream, System.Drawing.Imaging.ImageFormat.Png);

                stream.Seek(0, SeekOrigin.Begin);

                image.BeginInit();
                image.CreateOptions = System.Windows.Media.Imaging.BitmapCreateOptions.PreservePixelFormat;
                image.CacheOption = System.Windows.Media.Imaging.BitmapCacheOption.OnLoad;
                image.UriSource = null;
                image.StreamSource = stream;
                image.EndInit();
            }

            image.Freeze();
            return image;
        }


        public static void SetImageToCash(string imagePath)
        { 
            if (!_images.Any(i => i.Key == imagePath))
            {
                _images.Add(imagePath,
                    new System.Windows.Media.Imaging.BitmapImage(new Uri(imagePath, UriKind.Absolute)));
            }
        }

        public static string GetFileNameBy(string fileName)
        {
            return string.Format(@"{0}{1}", AppDomain.CurrentDomain.BaseDirectory, fileName);
        }

        //public static string GetFileNameBy(ExchangeModelLibrary.DataTools.PromotionalOffer offer)
        //{
        //    return string.Format(@"{0}Images\{1}_{2}.png", AppDomain.CurrentDomain.BaseDirectory , offer.Id, offer.Uid);
        //}

        //public static string GetFileNameBy(ExchangeModelLibrary.DataTools.MainSetting mainSetting)
        //{
        //    return string.Format(@"{0}Images\{1}_{2}.png", AppDomain.CurrentDomain.BaseDirectory, mainSetting.Id, "banner");
        //}

        //public static string GetFileNameBy(ExchangeModelLibrary.DataTools.OrderItem orderItem)
        //{
        //    return string.Format(@"{0}Images\{1}_{2}.png", AppDomain.CurrentDomain.BaseDirectory,orderItem.Dish.GroupTypeId,orderItem.Dish.Id);
        //}

        public static string NoImagePath { 
            get 
            {
                return string.Format(@"{0}HeaderImages\{1}.png", AppDomain.CurrentDomain.BaseDirectory, "no_images");
            }
        }

        internal static System.Windows.Media.ImageSource GetBitmapImage(string imagePath)
        {
            if (_images.Any(i => i.Key == imagePath))
            {
                return _images[imagePath];
            }
            else
            {
                _images.Add(imagePath,
                    new System.Windows.Media.Imaging.BitmapImage(new Uri(imagePath, UriKind.Absolute)));
                return _images[imagePath];
            }
                   
        }
    }
}
