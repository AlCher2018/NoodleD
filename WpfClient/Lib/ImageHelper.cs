using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;

namespace WpfClient
{
    public static class ImageHelper
    {
        private static Dictionary<string, BitmapImage> _images = new Dictionary<string, BitmapImage>();
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

        internal static void SaveImageFile(byte[] image,string filePath,string fileName )
        {
            var im = byteArrayToImage(image);
            if (im != null)
            {
                im.Save(fileName);
            }
            
        }

        public static System.Drawing.Image byteArrayToImage(byte[] byteArrayIn)
        {
            if (byteArrayIn!=null)
            {
                MemoryStream ms = new MemoryStream(byteArrayIn);
                System.Drawing.Image returnImage = System.Drawing.Image.FromStream(ms);
                ms.Dispose();
                ms = null;
                return returnImage;
            }
            return null;
        }

        //public static string GetFileNameBy(ExchangeModelLibrary.DataTools.Dish dish)
        //{
        //    var imagePath = string.Format(@"{0}Images\{1}_{2}.png", AppDomain.CurrentDomain.BaseDirectory, dish.GroupTypeId, dish.Id);
        //    if (@"E:\Projects\WinOs\_HG\RestaurantMenu_132\WPFMenu\bin\Debug\Images\1_2042.png"==imagePath)
        //    {
        //        var o="";
        //    }
        //    return imagePath;
        //}

        public static void SetImageToCash(string imagePath)
        { 
            if (!_images.Any(i => i.Key == imagePath))
            {
                _images.Add(imagePath,
                    new BitmapImage(new Uri(imagePath, UriKind.Absolute)));
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
                    new BitmapImage(new Uri(imagePath, UriKind.Absolute)));
                return _images[imagePath];
            }
                   
        }
    }
}
