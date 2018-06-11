using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace WpfClient.Lib
{
    public static class BarCodeLib
    {
        public static Image GetBarcodeImage(string barCodeValue, int width, int height)
        {
            if (barCodeValue.Length == 12) barCodeValue += getUPCACheckDigit(barCodeValue);

            com.google.zxing.oned.EAN13Writer writer = new com.google.zxing.oned.EAN13Writer();
            com.google.zxing.common.ByteMatrix matrix = writer.encode(barCodeValue, com.google.zxing.BarcodeFormat.EAN_13, width, height);
            System.Drawing.Image drawingBarCode = matrix.ToBitmap();

            // convert drawing to imageSource
            Image imageBarCode = new Image();
            imageBarCode.Source = ImageHelper.DrawingImageToBitmapImage(drawingBarCode);

            return imageBarCode;
        }

        // расчет контрольного числа для UPC-A (EAN-13) кода
        // проверка: 021701250458 => 1
        //           670000099149 => 3
        public static string getUPCACheckDigit(string bcOrig)
        {
            int[] a = bcOrig.Select((char c) => int.Parse(c.ToString())).ToArray<int>();

            int sum1 = (a[0] + a[2] + a[4] + a[6] + a[8] + a[10]);
            int sum2 = 3 * (a[1] + a[3] + a[5] + a[7] + a[9] + a[11]);
            int d = 10 - ((sum1+sum2) % 10);

            if (d > 10) d = (d % 10);
            if (d == 10) d = 0;

            return d.ToString();
        }


    }  // class
}
