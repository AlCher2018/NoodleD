using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace WpfClient
{
    /// <summary>
    /// Interaction logic for TakeOrder.xaml
    /// </summary>
    public partial class TakeOrder : Window
    {
        private TakeOrderEnum _takeOrder = TakeOrderEnum.None;
        public TakeOrderEnum TakeOrderMode { get { return _takeOrder; } }

        public TakeOrder()
        {
            InitializeComponent();
        }

        protected override void OnRender(DrawingContext drawingContext)
        {
            base.OnRender(drawingContext);

            // уголки
            //CornerRadius = "{Binding Converter={StaticResource getAppSetValue}, ConverterParameter=cornerRadiusButton, Mode=OneWay}"
            double rad = (double)AppLib.GetAppGlobalValue("cornerRadiusButton");
            CornerRadius cr = new CornerRadius(rad);
            brdDialog.CornerRadius = cr;
            btnTakeOut.CornerRadius = cr;
            btnTakeIn.CornerRadius = cr;

            // размеры и положение кнопки закрытия окна
            double dW = gridCloseButton.ActualWidth;
            double dH = gridCloseButton.ActualHeight;

            if (dW != double.NaN && dH != double.NaN)
            {
                double dMin = Math.Min(dW, dH) / 2.0;
                if (btnClose.Width != dMin)
                {
                    btnClose.Width = dMin;
                    btnClose.Height = dMin;
                    btnClose.Margin = new Thickness(0, dMin/2f, dMin/2f, 0);
                }
            }

            // тексты
            //Text = "{Binding Converter={StaticResource langDictToUpperText}, ConverterParameter=appSet.takeDishOut, Mode=OneWay}"
            // <Setter Property="FontSize" Value="{Binding Converter={StaticResource getAppSetValue}, ConverterParameter=appFontSize3}"/>
            double fSize = System.Convert.ToDouble(AppLib.GetAppGlobalValue("appFontSize3"));
            this.FontSize = fSize;
            txtTakeOut.Text = AppLib.GetLangText((Dictionary<string, string>)AppLib.GetAppGlobalValue("takeOrderOut")).ToUpper();
            txtWordOr.Text = AppLib.GetLangText((Dictionary<string, string>)AppLib.GetAppGlobalValue("wordOr")).ToUpper();
            txtTakeIn.Text = AppLib.GetLangText((Dictionary<string, string>)AppLib.GetAppGlobalValue("takeOrderIn")).ToUpper();
        }

        private void Window_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape) this.Close();
            e.Handled = true;
        }

        private void btnClose_MouseDown(object sender, MouseButtonEventArgs e)
        {
            e.Handled = true;
            this.Close();
        }

        private void btnTakeOut_MouseDown(object sender, MouseButtonEventArgs e)
        {
            _takeOrder = TakeOrderEnum.TakeAway;
            this.Close();
        }

        private void btnTakeIn_MouseDown(object sender, MouseButtonEventArgs e)
        {
            _takeOrder = TakeOrderEnum.TakeInRestaurant;
            this.Close();
        }
    }
}
