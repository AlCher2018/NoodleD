using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace WpfClient.Lib
{
    public static class PreventTouchToMousePromotion
    {
        public static void Register(FrameworkElement root)
        {
            root.PreviewMouseDown += Evaluate;
            root.PreviewMouseMove += Evaluate;
            root.PreviewMouseUp += Evaluate;
        }

        private static void Evaluate(object sender, MouseEventArgs e)
        {
            if (e.StylusDevice != null)
            {
                e.Handled = true;
            }
        }
    }
}
