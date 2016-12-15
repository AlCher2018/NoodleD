using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WpfClient
{
    public static class StringExtensions
    {
        public static double GetDoubleValue(this string sParam)
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
    }
}
