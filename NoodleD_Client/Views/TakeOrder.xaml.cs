using IntegraLib;
using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using UserActionLog;
using WpfClient.Model;


namespace WpfClient.Views
{
    /// <summary>
    /// Interaction logic for TakeOrder.xaml
    /// </summary>
    public partial class TakeOrder : Window
    {
        private string _closeResult;
        //private UserActionsLog _eventsLog;

        private TakeOrderEnum _takeOrder = TakeOrderEnum.None;
        public TakeOrderEnum TakeOrderMode { get { return _takeOrder; } }

        public TakeOrder()
        {
            InitializeComponent();
            this.Loaded += TakeOrder_Loaded;

            setWinLayout();
        }

        private void TakeOrder_Loaded(object sender, RoutedEventArgs e)
        {
            AppLib.WriteAppAction("TakeOrderWin|Загружено окно (Loaded)");

            resetLang();
        }

        private void resetLang()
        {
            AppLib.WriteLogTraceMessage(" - язык приложения: " + AppLib.AppLang);
            string textOut = AppLib.GetLangTextFromAppProp("takeOrderOut");
            string textOr = AppLib.GetLangTextFromAppProp("wordOr");
            string textIn = AppLib.GetLangTextFromAppProp("takeOrderIn");
            AppLib.WriteLogTraceMessage($" - тексты на кнопках: takeOut='{textOut}', wordOr='{textOr}', takeIn='{textIn}'");

            txtTakeOut.Text = textOut;
            txtWordOr.Text = textOr;
            txtTakeIn.Text = textIn;
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
            if (e.Key == Key.Escape)
            {
                _closeResult = "нажата клавиша Esc";
                closeWin(e);
            }
        }

        private void btnClose_MouseDown(object sender, MouseButtonEventArgs e)
        {
            //            if (e.StylusDevice != null) return;
            _closeResult = "нажата кнопка Close (mouse)";

            closeWin(e);
        }
        private void btnClose_PreviewTouchDown(object sender, TouchEventArgs e)
        {
            _closeResult = "нажата кнопка Close (touch)";
            closeWin(e);
        }


        private void btnTakeOut_MouseDown(object sender, MouseButtonEventArgs e)
        {
            //if (e.StylusDevice != null) return;
            _closeResult = "нажата кнопка TakeAway (mouse)";
            _takeOrder = TakeOrderEnum.TakeAway;
            closeWin(e);
        }
        private void btnTakeOut_PreviewTouchDown(object sender, TouchEventArgs e)
        {
            _closeResult = "нажата кнопка TakeAway (touch)";
            _takeOrder = TakeOrderEnum.TakeAway;
            closeWin(e);
        }


        private void btnTakeIn_MouseDown(object sender, MouseButtonEventArgs e)
        {
            //if (e.StylusDevice != null) return;
            _closeResult = "нажата кнопка TakeInRest (mouse)";
            _takeOrder = TakeOrderEnum.TakeInRestaurant;
            closeWin(e);
        }

        private void btnTakeIn_PreviewTouchDown(object sender, TouchEventArgs e)
        {
            _closeResult = "нажата кнопка TakeInRest (touch)";
            _takeOrder = TakeOrderEnum.TakeInRestaurant;
            closeWin(e);
        }

        private void closeWin(RoutedEventArgs e)
        {
            AppLib.WriteAppAction($"TakeOrderWin|Закрыто окно ({_closeResult??"-"})");

            e.Handled = true;
            this.Hide();
        }

    }  // class

}
