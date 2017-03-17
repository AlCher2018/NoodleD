using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media.Imaging;
using System.Windows.Threading;

namespace WpfClient
{

    public static class StringExtensions
    {
        // convert string to bool
        public static bool ToBool(this string source)
        {
            bool retValue = false;
            if (string.IsNullOrEmpty(source)) return retValue;

            string sLower = source.ToLower();

            if (sLower.Equals("true") || sLower.Equals("да"))
                retValue = true;
            else
            {
                int iBuf = 0;
                if (int.TryParse(source, out iBuf) == true) retValue = (iBuf != 0);
            }

            return retValue;
        }  // method

        public static double ToDouble(this string sParam)
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

        public static int ToInt(this string source)
        {
            if (source == null) return 0;

            List<char> chList = new List<char>();
            foreach (char c in source)
            {
                if (System.Globalization.CharUnicodeInfo.GetUnicodeCategory(c) == System.Globalization.UnicodeCategory.DecimalDigitNumber) chList.Add(c);
            }
            return (chList.Count > 0) ? int.Parse(string.Join("", chList.ToArray())) : 0;
        }

        public static bool IsNumber(this string source)
        {
            return source.All(c => char.IsDigit(c));
        }

    } // class

    public static class IntExtensions
    {
        public static int SetBit(this int bitMask, int bit)
        {
            return (bitMask |= (1 << bit));
        }
        public static int ClearBit(this int bitMask, int bit)
        {
            return (bitMask &= ~(1 << bit));
        }
        public static bool IsSetBit(this int bitMask, int bit)
        {
            int val = (1 << bit);
            return (bitMask & val) == val;
        }

    }

    public static class UIElementExtensions
    {
        private static Action EmptyDelegate = delegate () { };

        public static void Refresh(this UIElement uiElement)
        {
            uiElement.Dispatcher.Invoke(DispatcherPriority.Render, EmptyDelegate);
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
