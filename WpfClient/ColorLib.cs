using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;

namespace WpfClient
{
    public static class ColorLib
    {
        public static Dictionary<string, SolidColorBrush> GetDefaultAppColors()
        {
            Dictionary<string, SolidColorBrush> retVal = new Dictionary<string, SolidColorBrush>();

            retVal.Add("appBackgroundColor", (SolidColorBrush)Application.Current.Resources["appBackgroundColor"]);
            retVal.Add("appNotSelectedItemColor", (SolidColorBrush)Application.Current.Resources["appNotSelectedItemColor"]);
            retVal.Add("appSelectedItemColor", (SolidColorBrush)Application.Current.Resources["appSelectedItemColor"]);

            retVal.Add("mainMenuImageColor", (SolidColorBrush)Application.Current.Resources["mainMenuImageColor"]);
            retVal.Add("mainMenuTextColor", (SolidColorBrush)Application.Current.Resources["mainMenuTextColor"]);
            retVal.Add("mainMenuSelectedItemColor", (SolidColorBrush)Application.Current.Resources["mainMenuSelectedItemColor"]);

            return retVal;
        }

        public static SolidColorBrush GetAppColorFromConfig(string appSettingName)
        {
            AppSettingsReader ar = new AppSettingsReader();

            string s = (string)ar.GetValue(appSettingName, typeof(string));
            if (s == null) return null;

            return new SolidColorBrush(getColorFromRGBString(s));
        }

        private static Color getColorFromRGBString(string rgba)
        {
            string[] sArr = rgba.Split(',');
            if (sArr.Count() != 4) return Color.FromArgb(0, 0, 0, 0);

            byte r=0, g=0, b=0, a = 0;
            byte.TryParse(sArr[0], out r);
            byte.TryParse(sArr[1], out g);
            byte.TryParse(sArr[2], out b);
            byte.TryParse(sArr[3], out a);

            return Color.FromArgb(a, r, g, b);
        }

    }

}
