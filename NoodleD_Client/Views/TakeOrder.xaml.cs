using AppActionNS;
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
using UserActionLog;
using WpfClient.Model;


namespace WpfClient.Views
{
    /// <summary>
    /// Interaction logic for TakeOrder.xaml
    /// </summary>
    public partial class TakeOrder : Window
    {
        private UserActionsLog _eventsLog;

        private TakeOrderEnum _takeOrder = TakeOrderEnum.None;
        public TakeOrderEnum TakeOrderMode { get { return _takeOrder; } }

        public TakeOrder()
        {
            InitializeComponent();
            this.Activated += TakeOrder_Activated;

            setWinLayout();

            if (AppLib.GetAppSetting("IsWriteWindowEvents").ToBool())
            {
                _eventsLog = new UserActionsLog(new FrameworkElement[] { this, btnTakeOut, btnTakeIn }, EventsMouseEnum.Bubble, EventsKeyboardEnum.None, EventsTouchEnum.Bubble, UserActionLog.LogFilesPathLocationEnum.App_Logs, true, false);
            }

        }

        private void TakeOrder_Activated(object sender, EventArgs e)
        {
            AppLib.WriteAppAction(this.Name, AppActionsEnum.TakeOrderWinOpen);
            _takeOrder = TakeOrderEnum.None;
        }

        public void ResetLang()
        {
            // напрямую
            // окно создается и хранится, как глобальная переменная и переключение языка будет осуществляться извне при смене языка UI
            txtTakeOut.Text = AppLib.GetLangTextFromAppProp("takeOrderOut");
            txtWordOr.Text = AppLib.GetLangTextFromAppProp("wordOr");
            txtTakeIn.Text = AppLib.GetLangTextFromAppProp("takeOrderIn");

            #region через связанное свойство - ОТКЛЮЧЕНО, т.к. иногда не переключается
            /*
            Text = "{Binding Converter={StaticResource langDictToText}, ConverterParameter=appSet.takeOrderOut}"
            Text="{Binding Converter={StaticResource langDictToText}, ConverterParameter=appSet.wordOr}"
            Text = "{Binding Converter={StaticResource langDictToText}, ConverterParameter=appSet.takeOrderIn}"
            BindingExpression be;
            // установка текстов на выбранном языке
            //    с собой
            be = txtTakeOut.GetBindingExpression(TextBlock.TextProperty);
            if (be != null) be.UpdateTarget();
            //    или
            be = txtWordOr.GetBindingExpression(TextBlock.TextProperty);
            if (be != null) be.UpdateTarget();
            //   в ресторане
            be = txtTakeIn.GetBindingExpression(TextBlock.TextProperty);
            if (be != null) be.UpdateTarget();
             */
            #endregion
        }

        #region активация ожидашки
        protected override void OnActivated(EventArgs e)
        {
            App.IdleTimerStart(this);
            base.OnActivated(e);
        }
        protected override void OnDeactivated(EventArgs e)
        {
            App.IdleTimerStop();
            base.OnDeactivated(e);
        }
        #endregion


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
            if (e.Key == Key.Escape) closeWin(e);
        }

        private void btnClose_MouseDown(object sender, MouseButtonEventArgs e)
        {
//            if (e.StylusDevice != null) return;

            closeWin(e);
        }
        private void btnClose_PreviewTouchDown(object sender, TouchEventArgs e)
        {
            closeWin(e);
        }


        private void btnTakeOut_MouseDown(object sender, MouseButtonEventArgs e)
        {
            //if (e.StylusDevice != null) return;

            _takeOrder = TakeOrderEnum.TakeAway;
            closeWin(e);
        }
        private void btnTakeOut_PreviewTouchDown(object sender, TouchEventArgs e)
        {
            _takeOrder = TakeOrderEnum.TakeAway;
            closeWin(e);
        }


        private void btnTakeIn_MouseDown(object sender, MouseButtonEventArgs e)
        {
            //if (e.StylusDevice != null) return;

            _takeOrder = TakeOrderEnum.TakeInRestaurant;
            closeWin(e);
        }

        private void btnTakeIn_PreviewTouchDown(object sender, TouchEventArgs e)
        {
            _takeOrder = TakeOrderEnum.TakeInRestaurant;
            closeWin(e);
        }

        private void closeWin(RoutedEventArgs e)
        {
            AppLib.WriteAppAction(this.Name, AppActionsEnum.TakeOrderWinClose, _takeOrder.ToString());

            e.Handled = true;
            this.Hide();
        }

    }  // class

}
