using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using AppModel;

namespace WpfClient
{

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
