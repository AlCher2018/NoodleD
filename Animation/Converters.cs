using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

namespace Animation
{
    [ValueConversion(typeof(FrameworkElement), typeof(Point))]
    public class GetCentralPointConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null) return null;

            FrameworkElement fe = (FrameworkElement)value;
            double left = Canvas.GetLeft(fe);
            double top = Canvas.GetTop(fe);
            double width = (fe.Width == 0)?fe.ActualWidth: fe.Width;
            double height = (fe.Height == 0)?fe.ActualHeight : fe.Height;

            Point retVal = new Point(left+width/2d, top+height/2d);

            return retVal;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }  // class

    // two double to pint
    [ValueConversion(typeof(string), typeof(Point))]
    public class StringToPointConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null) return null;

            string[] aStr = ((string)value).Split(';');
            double x = double.Parse(aStr[0]);
            double y = double.Parse(aStr[1]);

            return new Point(x,y);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    [ValueConversion(typeof(FrameworkElement), typeof(Rect))]
    public class ControlToRectConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null) return null;

            FrameworkElement fe = (FrameworkElement)value;
            double width = (fe.Width == 0) ? fe.ActualWidth : fe.Width;
            double height = (fe.Height == 0) ? fe.ActualHeight : fe.Height;

            Rect retVal = new Rect(0,0, width, height);

            return retVal;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }  // class


    [ValueConversion(typeof(FrameworkElement), typeof(Rect))]
    public class NumberToDurationConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null) return null;

            double d = System.Convert.ToDouble(value);

            return new Duration(TimeSpan.FromSeconds(d));
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }  // class


} // namespace
