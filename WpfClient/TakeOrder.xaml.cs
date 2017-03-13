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

            setWinLayout();
        }

        private void setWinLayout()
        {
            // размеры
            this.Width = (double)AppLib.GetAppGlobalValue("screenWidth");
            this.Height = (double)AppLib.GetAppGlobalValue("screenHeight");
            this.Top = 0; this.Left = 0;

            double pnlMenuWidth = (double)AppLib.GetAppGlobalValue("categoriesPanelWidth");
            double pnlMenuHeight = (double)AppLib.GetAppGlobalValue("categoriesPanelHeight");
            brdAboveFolderMenu.Height = pnlMenuHeight;
            brdAboveFolderMenu.Width = pnlMenuWidth;
            // грид блюд
            double pnlDishesWidth = (double)AppLib.GetAppGlobalValue("dishesPanelWidth");
            double pnlDishesHeight = (double)AppLib.GetAppGlobalValue("dishesPanelHeight");
            double pnlContentWidth, pnlContentHeight, pnlContentTop, pnlContentLeft;

            double dH;
            double btnTextFontSize;

            // вертикальное размещение
            if (AppLib.IsAppVerticalLayout == true)
            {
                DockPanel.SetDock(brdAboveFolderMenu, Dock.Top);
                pnlContentWidth = 0.8 * pnlDishesWidth;
                pnlContentHeight = 0.1 * pnlDishesHeight;
                pnlContentTop = (pnlDishesHeight - pnlContentHeight) / 2d;
                pnlContentTop *= 0.5;
                pnlContentLeft = (pnlDishesWidth - pnlContentWidth) / 2d;
            }
            // горизонтальное размещение
            else
            {
                DockPanel.SetDock(brdAboveFolderMenu, Dock.Left);
                pnlContentWidth = 0.7 * pnlDishesWidth;
                pnlContentHeight = 0.2 * pnlDishesHeight;
                pnlContentTop = (pnlDishesHeight - pnlContentHeight) / 2d;
                pnlContentLeft = (pnlDishesWidth - pnlContentWidth) / 2d;
            }

            // панель содержания
            brdDialog.Width = pnlContentWidth; brdDialog.Height = pnlContentHeight;
            brdDialog.Margin = new Thickness(pnlContentLeft, pnlContentTop, 
                pnlDishesWidth - pnlContentWidth - pnlContentLeft, 
                pnlDishesHeight - pnlContentHeight - pnlContentTop);
            dH = pnlContentHeight / gridDialog.RowDefinitions.Sum(r => r.Height.Value);

            btnTextFontSize = 0.5 * dH;
            txtTakeOut.FontSize = btnTextFontSize;
            txtWordOr.FontSize = btnTextFontSize;
            txtTakeIn.FontSize = btnTextFontSize;

        }  // method

        private void setStylePropertyValue(string styleName, string propName, object value)
        {
            Style stl = (Style)this.Resources[styleName];
            Setter str = (Setter)stl.Setters.FirstOrDefault(s => (s is Setter) ? (s as Setter).Property.Name == propName : false);

            if (str != null) str.Value = value;
        }

        private void Window_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape) this.Close();
            e.Handled = true;
        }

        private void btnClose_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.StylusDevice != null) return;

            e.Handled = true;
            this.Close();
        }
        private void btnClose_PreviewTouchDown(object sender, TouchEventArgs e)
        {
            this.Close();
        }


        private void btnTakeOut_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.StylusDevice != null) return;

            _takeOrder = TakeOrderEnum.TakeAway;
            this.Close();
        }
        private void btnTakeOut_PreviewTouchDown(object sender, TouchEventArgs e)
        {
            _takeOrder = TakeOrderEnum.TakeAway;
            this.Close();
        }


        private void btnTakeIn_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.StylusDevice != null) return;

            _takeOrder = TakeOrderEnum.TakeInRestaurant;
            this.Close();
        }

        private void btnTakeIn_PreviewTouchDown(object sender, TouchEventArgs e)
        {
            _takeOrder = TakeOrderEnum.TakeInRestaurant;
            this.Close();
        }

    }  // class

}
