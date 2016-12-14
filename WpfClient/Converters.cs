using AppModel;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace WpfClient
{
    // умножающий число конвертер
    [ValueConversion(typeof(double), typeof(double))]
    public class MultiplyValueConverter : IValueConverter
    {
        public double Multiplier { get; set; }
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return Multiplier * ((double)value);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value;
        }
    }

    [ValueConversion(typeof(double), typeof(double))]
    public class MultiplyParamValueConverter : IValueConverter
    {
        public double DefaultValue { get; set; }

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            double dBuf = 0f, retVal = 0f;
            string sParam = parameter.ToString();

            double.TryParse(sParam, out dBuf);
            if (dBuf == 0)
            {
                if (sParam.Contains(".")) sParam = sParam.Replace('.', ',');
                else if (sParam.Contains(",")) sParam = sParam.Replace(',', '.');
                double.TryParse(sParam, out dBuf);
            }

            retVal = dBuf * (double)value;
            if ((retVal == 0) && (DefaultValue != 0)) retVal = DefaultValue;

            return retVal;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value;
        }
    }

    public class GetMinValue : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            double v1 = (double)values[0];
            double v2 = (double)values[1];

            v2 = (v2 / 4.0) - 4.0;
            return (v1 < v2) ? v1 : v2;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    // получить строку из связанного словаря по глобальному Ид языка
    [ValueConversion(typeof(Dictionary<string, string>), typeof(string))]
    public class LangDictToTextConverter : IValueConverter
    {
        public bool IsUpper { get; set; }
        public bool IsLower { get; set; }

        public LangDictToTextConverter()
        {
            IsLower = false; IsUpper = false;
        }

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            string retVal = null;
            if (parameter == null)
            {
                retVal = AppLib.GetLangText((Dictionary<string, string>)value);
                if (IsUpper == true) retVal = retVal.ToUpper(culture);
                if (IsLower == true) retVal = retVal.ToLower(culture);
            }
            else
            {
                string mode = (string)parameter;
                int i1 = mode.IndexOf('.');
                if (i1 > -1)
                {
                    string key = mode.Substring(0, i1);
                    string val = mode.Substring(i1 + 1);
                    if (key == "appSet")
                    {
                        Dictionary<string, string> lDict = (Dictionary<string, string>)AppLib.GetAppGlobalValue(val);
                        retVal = AppLib.GetLangText(lDict);
                        if (IsUpper == true) retVal = retVal.ToUpper(culture);
                        if (IsLower == true) retVal = retVal.ToLower(culture);
                    }
                }
            }

            return retVal;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value;
        }
    }

    [ValueConversion(typeof(string), typeof(object))]
    public class GetAppSetValue : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            object retVal = AppLib.GetAppGlobalValue(parameter.ToString());
            Debug.Print("GetAppSetValue: " + parameter ?? "no value");
            return retVal;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }



    [ValueConversion(typeof(DishItem), typeof(Visibility))]
    public class AddDishButtonVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            DishItem di = (DishItem)value;
            if ((di.Garnishes == null) || di.Garnishes.Count == 0)
                return Visibility.Visible;
            else
            {
                return ((di.SelectedGarnishes == null) || (di.SelectedGarnishes.Count == 0)) ? Visibility.Hidden : Visibility.Visible;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value;
        }
    }


    // получить отступ слева панелей блюд
    [ValueConversion(typeof(double), typeof(Thickness))]
    public class WidthToMarginConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            double dMarg = (double)value / 3.0;
            dMarg *= 0.15;
            dMarg /= 2.0;

            return new Thickness(dMarg, dMarg / 2, dMarg, dMarg / 2);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value;
        }
    }

    [ValueConversion(typeof(double), typeof(CornerRadius))]
    public class CornerRadiusConverter : IValueConverter
    {
        public string Side { get; set; }

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            double d1 = 0, d2 = 0;
            double.TryParse((value??"0").ToString(), out d1);
            string p = (parameter ?? "0").ToString();
            double.TryParse(p.ToString(), out d2);
            if (d2 == 0)
            {
                if (p.Contains(".")) p = p.Replace('.', ',');
                else if (p.Contains(",")) p = p.Replace(',', '.');
                double.TryParse(p, out d2);
            }
            d1 *= d2;  // радиус

            CornerRadius retVal;
            string side = (Side == null) ? "all" : Side.ToLower();
            retVal = new CornerRadius(d1);
            if (side == "left") { retVal.TopRight = 0; retVal.BottomRight = 0; }
            else if (side == "right") { retVal.TopLeft = 0; retVal.BottomLeft = 0; }
            return retVal;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class IsNullValueConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return (value == null) ? true : false;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    [ValueConversion(typeof(IEnumerable<object>), typeof(bool))]
    public class IsEmptyEnumerator : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null) return true;

            if (((IEnumerable<object>)value).Count() == 0) return true;

            return false;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    [ValueConversion(typeof(string), typeof(string))]
    public class UpperCaseConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null) return "";
            else return ((string)value).ToUpper(CultureInfo.CurrentCulture);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }


    /// <summary>Represents a chain of <see cref="IValueConverter"/>s to be executed in succession.</summary>
    /// http://stackoverflow.com/questions/1594357/wpf-how-to-use-2-converters-in-1-binding
    /// https://www.codeproject.com/kb/wpf/pipingvalueconverters_wpf.aspx
    [ContentProperty("Converters")]
    [ContentWrapper(typeof(ValueConverterCollection))]
    public class ConverterChain : IValueConverter
    {
        private readonly ValueConverterCollection _converters = new ValueConverterCollection();

        /// <summary>Gets the converters to execute.</summary>
        public ValueConverterCollection Converters
        {
            get { return _converters; }
        }

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return Converters
                .Aggregate(value, (current, converter) => converter.Convert(current, targetType, parameter, culture));
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return Converters
                .Reverse()
                .Aggregate(value, (current, converter) => converter.Convert(current, targetType, parameter, culture));
        }
    }

    /// <summary>Represents a collection of <see cref="IValueConverter"/>s.</summary>
    public sealed class ValueConverterCollection : Collection<IValueConverter> { }


}
