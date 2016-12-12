using AppModel;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;

namespace WpfClient
{
    /// <summary>
    /// Классы конвертеров для гарниров блюд
    /// </summary>

    // Видимость строки гарниров или отдельного гарнира
    [ValueConversion(typeof(List<DishAdding>), typeof(Visibility))]
    public class GarnishVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null) return Visibility.Collapsed;

            int mode = 0;  // =0 - видимость всей строки гарниров; 1,2,3 - соотв.номер гарнира
            int.TryParse(parameter.ToString(), out mode);

            List<DishAdding> garList = (List<DishAdding>)value;
            Visibility retVal = Visibility.Visible;
            if (mode == 0)
            {
                if (garList.Count == 0) retVal = Visibility.Collapsed;
            }
            else
            {
                retVal = (mode <= garList.Count) ? Visibility.Visible : Visibility.Hidden;
            }

            return retVal;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value;
        }
    }

    // Текст гарнира
    [ValueConversion(typeof(List<DishAdding>), typeof(string))]
    public class GarnishLangTextConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null) return null;

            List<DishAdding> var = (List<DishAdding>)value;
            int garnishIndex = 0;
            int.TryParse(parameter.ToString(), out garnishIndex);

            string retVal = null;
            if (garnishIndex <= var.Count)
            {
                string langId = AppLib.AppLang;
                Dictionary<string, string> lDict = var[garnishIndex - 1].langNames;
                if (lDict.TryGetValue(langId, out retVal) == false) retVal = "no value";
            }
            return retVal;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value;
        }
    }

    // Цвет фона или текста гарнира (value = SelectedGarnishes)
    [ValueConversion(typeof(List<DishAdding>), typeof(Brush))]
    public class GarnishBrushConverter : IValueConverter
    {
        public string Mode { get; set; }

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            int mode = (Mode.ToLower().StartsWith("back") == true) ? 0 : 1; // 0-цвет фона, 1-цвет текста

            Brush retVal = (mode == 0) ? (SolidColorBrush)AppLib.GetAppGlobalValue("appBackgroundColor") : new SolidColorBrush(Colors.White);

            if (value == null) return retVal;
            List<DishAdding> garList = (List<DishAdding>)value;  // SelectedGarnishes
            if (garList.Count == 0) return retVal;

            foreach (DishAdding item in garList)
            {
                if (parameter == null) { }
                else
                {
                    if (item.Uid == parameter.ToString())
                    {
                        retVal = (mode == 0) ? (SolidColorBrush)AppLib.GetAppGlobalValue("appSelectedItemColor") : new SolidColorBrush(Colors.Black);
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

    // Стоимость гарнира в виде строки
    [ValueConversion(typeof(List<DishAdding>), typeof(string))]
    public class GarnishPriceConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null) return null;

            List<DishAdding> garList = (List<DishAdding>)value;
            if (garList.Count == 0) return null;

            int garnishIndex = 0;
            int.TryParse(parameter.ToString(), out garnishIndex);

            string retVal = null;
            if (garnishIndex <= garList.Count)
            {
                retVal = garList[garnishIndex-1].Price.ToString("#0 ₴");
            }
            return retVal;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value;
        }
    }



}
