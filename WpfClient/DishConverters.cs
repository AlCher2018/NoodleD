using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using AppModel;
using System.Windows;

namespace WpfClient
{

    public class GetDishMargin : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            double marg = (double)AppLib.GetAppGlobalValue("dishPanelMargin");

            return new Thickness(marg, -marg, marg, 0);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }


    [ValueConversion(typeof(DishItem), typeof(decimal))]
    public class GetOrderPrice : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            decimal retVal = 0;

            OrderItem curOrder = (OrderItem)AppLib.GetAppGlobalValue("currentOrder");
            if (curOrder == null) return retVal;

            if (curOrder.Dishes != null)
            {
                foreach (DishItem item in curOrder.Dishes)
                {
                    retVal += item.GetTotalPrice();
                }
            }

            return retVal;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }


    [ValueConversion(typeof(DishItem), typeof(decimal))]
    public class GetDishPriceTotal : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            decimal retVal = 0;

            if (value is DishItem)
            {
                DishItem currentDish = (DishItem)value;

                retVal = currentDish.GetTotalPrice();
            }

            return retVal;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
