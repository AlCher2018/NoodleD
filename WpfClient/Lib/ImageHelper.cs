﻿using System;
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
            System.Windows.Media.Imaging.BitmapImage image = null;
            
            // если нет изображения, то вернуть из файла
            if (imageData == null || imageData.Length == 0)
            {
                string filePath = ImageHelper.GetFileNameBy(@"AppImages\no_image.png");
                if (System.IO.File.Exists(filePath) == true)
                {
                    image = new System.Windows.Media.Imaging.BitmapImage(new Uri(filePath, UriKind.Absolute));
                }
            }
            else
            {
                image = new System.Windows.Media.Imaging.BitmapImage();
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
                if (imagePath == null) return null;
                if (!File.Exists(imagePath)) return null;

                BitmapImage bi = null;
                // full path (absolutely)
                if (imagePath[1].Equals(':'))
                    bi = new System.Windows.Media.Imaging.BitmapImage(new Uri(imagePath, UriKind.Absolute));
                else
                    bi = new System.Windows.Media.Imaging.BitmapImage(new Uri(imagePath, UriKind.Relative));

                _images.Add(imagePath,bi);
                return _images[imagePath];
            }
                   
        }
    }
}
