using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using AppModel;
using WpfClient.Lib;
using System.Windows.Media;
using System.Windows.Media.Animation;
using AppActionNS;
using System.ComponentModel;
using UserActionLog;
using WpfClient.Model;
using IntegraLib;

namespace WpfClient.Views
{
    /// <summary>
    /// Interaction logic for Cart.xaml
    /// </summary>
    public partial class Cart : Window
    {
        // dragging
        Point? lastDragPoint, initDragPoint;
        protected DateTime _dateTime;
        private OrderItem _currentOrder;
        private bool _isDrag;

        private UserActionsLog _eventsLog;

        private double dishBorderHeight;

        public Cart()
        {
            InitializeComponent();

            this.Loaded += Cart_Loaded;

            _currentOrder = AppLib.GetCurrentOrder();
            this.lstDishes.ItemsSource = _currentOrder.Dishes;

            initUI();

            if (AppLib.GetAppSetting("IsWriteWindowEvents").ToBool())
            {
                _eventsLog = new UserActionsLog(this, EventsMouseEnum.Bubble, EventsKeyboardEnum.None, EventsTouchEnum.Bubble, UserActionLog.LogFilesPathLocationEnum.App_Logs, true, false);
            }

            updatePriceOrder();
        }

        private void Cart_Loaded(object sender, RoutedEventArgs e)
        {
            AppLib.WriteLogTraceMessage(string.Format("Открывается корзина для заказа {0} ...", _currentOrder.OrderNumberForPrint));
            AppLib.WriteAppAction(this.Name, AppActionsEnum.CartWinOpen);
        }
        protected override void OnClosing(CancelEventArgs e)
        {
            AppLib.WriteLogTraceMessage(string.Format("Закрывается корзина для заказа {0} ...", _currentOrder.OrderNumberForPrint));
            AppLib.WriteAppAction(this.Name, AppActionsEnum.CartWinClose);

            base.OnClosing(e);
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


        private void initUI()
        {
            double pnlWidth = (double)AppLib.GetAppGlobalValue("categoriesPanelWidth");
            double pnlHeight = (double)AppLib.GetAppGlobalValue("categoriesPanelHeight");
            double promoFontSize, dH, d1, dKoefPortionCount;
            double pnlW, titleFontSize;
            // грид блюд
            double dishesPanelWidth = (double)AppLib.GetAppGlobalValue("dishesPanelWidth");
            double dishesPanelHeight = (double)AppLib.GetAppGlobalValue("dishesPanelHeight");
            scrollDishes.Height = dishesPanelHeight; scrollDishes.Width = dishesPanelWidth;
            double dishNameFontSize, dishUnitFontSize;
            string backgroundImage;
            Style stl; Setter str;

            // дизайн вертикальный: панель меню СВЕРХУ
            if (AppLib.IsAppVerticalLayout)
            {
                DockPanel.SetDock(gridMenuSide, Dock.Top);
                //                menuSidePanelLogo.Background = (Brush)AppLib.GetAppGlobalValue("appBackgroundColor");
                // грид меню
                //pnlHeight *= 0.8;
                gridMenuSide.Height = pnlHeight;
                gridMenuSide.Width = pnlWidth;

                // панель меню на всю ширину экрана
                dH = pnlHeight / 10d;
                gridMenuSide.RowDefinitions[0].Height = new GridLength(3.0 * dH);
                gridMenuSide.RowDefinitions[1].Height = new GridLength(0.0 * dH);
                gridMenuSide.RowDefinitions[2].Height = new GridLength(3.5 * dH);
                gridMenuSide.RowDefinitions[3].Height = new GridLength(0.0 * dH);
                gridMenuSide.RowDefinitions[4].Height = new GridLength(0.0 * dH);
                gridMenuSide.RowDefinitions[5].Height = new GridLength(3.5 * dH);

                // stackPanel для Logo
                gridMenuSide.Children.Remove(imageLogo);
                StackPanel pnlLogo = new StackPanel();
                pnlLogo.Orientation = Orientation.Horizontal;
                pnlLogo.Background = new SolidColorBrush(Color.FromRgb(0x62, 0x1C, 0x55));

                pnlW = gridMenuSide.Width - 2.0 * dH;
                // перенести кнопку Назад
                gridMenuSide.Children.Remove(btnReturn);
                pnlLogo.Children.Add(btnReturn);
                btnReturn.Width = 0.3 * pnlW;  // общая ширина = ширина элемента + отступы справа/слева
                txtReturn.HorizontalAlignment = HorizontalAlignment.Left;
                btnReturn.Margin = new Thickness(dH,0,0,0);
                btnReturn.Background = pnlLogo.Background;
                //   языковые кнопки
                gridMenuSide.Children.Remove(gridLang);
                gridLang.Height = 2.0 * dH;  // необходимо для расчета размера внутренних кнопок
                gridLang.Width = 0.2 * pnlW;
                gridLang.Margin = new Thickness(0.1 * pnlW, 0, 0.1 * pnlW, 0);
                pnlLogo.Children.Add(gridLang);
                gridLang.Visibility = Visibility.Visible;
                // языковые кнопки, фон для внешних Border, чтобы они были кликабельные
                btnLangUa.Background = pnlLogo.Background;
                btnLangRu.Background = pnlLogo.Background;
                btnLangEn.Background = pnlLogo.Background;
                double dMin = Math.Min(gridLang.Height, gridMenuSide.Width / (0.3 + 1.0 + 0.3 + 1.0 + 0.3 + 1.0 + 0.3));
                double dLangSize = 0.7 * dMin;
                setLngInnerBtnSizes(btnLangUaInner, lblLangUa, dLangSize);
                setLngInnerBtnSizes(btnLangRuInner, lblLangRu, dLangSize);
                setLngInnerBtnSizes(btnLangEnInner, lblLangEn, dLangSize);

                // перенести промокод
                gridMenuSide.Children.Remove(gridPromoCode);
                gridPromoCode.ColumnDefinitions[3].Width = new GridLength(0.0 * dH);
                gridPromoCode.Width = 0.3 * pnlW;
                gridPromoCode.HorizontalAlignment = HorizontalAlignment.Right;
                gridPromoCode.Height = 1.5 * dH;
                promoFontSize = 0.5 * dH;
                pnlLogo.Children.Add(gridPromoCode);

                Grid.SetRow(pnlLogo, 0);
                gridMenuSide.Children.Add(pnlLogo);

                // строка с общей стоимостью
                pnlTotal.Orientation = Orientation.Horizontal;
                txtOrderPrice.Margin = new Thickness(20,0,0,0);
                pnlTotalLabel.FontSize = (double)AppLib.GetAppGlobalValue("appFontSize2");
                txtOrderPrice.FontSize = (double)AppLib.GetAppGlobalValue("appFontSize1");

                // кнопка Оформить
                btnPrintCheck.Margin = new Thickness(dH, 0.0 * dH, dH, 0.8 * dH);
                btnPrintCheck.CornerRadius = new CornerRadius((double)AppLib.GetAppGlobalValue("cornerRadiusButton"));
                txtPrintCheck.FontSize = (double)AppLib.GetAppGlobalValue("appFontSize4");
                txtPrintCheck.FontWeight = FontWeights.Bold;
                gridMenuSide.Children.Remove(txtCashier);
                pnlPrintCheck.Children.Add(txtCashier);
                txtCashier.Style = (Style)this.Resources["goToCashierVer"];

                // фон
                backgroundImage = AppLib.GetImageFullFileName((string)AppLib.GetAppGlobalValue("BackgroundImageVertical"));

                setLangButtonStyle(true);   // "включить" текущую языковую кнопку

                // панели блюд
                dishBorderHeight = (dishesPanelHeight - dishesListTitle.ActualHeight) / 6.0;
                titleFontSize = 0.015 * dishesPanelHeight;
                dishNameFontSize = 0.09 * dishBorderHeight;
                dishUnitFontSize = 0.08 * dishBorderHeight;
                dKoefPortionCount = 1.5d;
                // отступы кнопок изменения кол-ва блюда
                setStylePropertyValue("dishPortionImageStyle", "Margin", new Thickness(0.03 * dishBorderHeight));  
                setStylePropertyValue("dishDelImageStyle", "Margin", new Thickness(0.1 * dishBorderHeight));
            }

            // дизайн горизонтальный: панель меню слева
            else
            {
                DockPanel.SetDock(dockMain, Dock.Left);

                // грид меню
                gridMenuSide.Height = pnlHeight;
                gridMenuSide.Width = pnlWidth;
                dH = pnlHeight / 13d;

                // промокод
                gridPromoCode.Height = 0.6 * dH;
                gridPromoCode.Margin = new Thickness(0, 0, 0, 0.4 * dH);
                promoFontSize = 0.3 * gridPromoCode.Height;

                txtCashier.Margin = new Thickness(0.5*dH,0,0.5*dH,0);

                // фон
                backgroundImage = AppLib.GetImageFullFileName((string)AppLib.GetAppGlobalValue("BackgroundImageHorizontal"));

                dishBorderHeight = (dishesPanelHeight - dishesListTitle.ActualHeight) / 4.0;
                titleFontSize = 0.02 * dishesPanelHeight;
                dishNameFontSize = 0.09 * dishBorderHeight;
                dishUnitFontSize = 0.08 * dishBorderHeight;
                dKoefPortionCount = 1.5;
                // отступы кнопок изменения кол-ва блюда
                setStylePropertyValue("dishPortionImageStyle", "Margin", new Thickness(0.03 * dishBorderHeight));
                setStylePropertyValue("dishDelImageStyle", "Margin", new Thickness(0.15 * dishBorderHeight));
            }

            // фон
            imgBackground.Source = ImageHelper.GetBitmapImage(backgroundImage);
            // яркость фона
            imgBackground.Opacity = (double)AppLib.GetAppGlobalValue("BackgroundImageBrightness", 0.3);

            // высота рамки блюда в заказе, из стиля
            setStylePropertyValue("dishBorderStyle", "MinHeight", dishBorderHeight);
            // элементы в строке блюда, относительно ее высоты dishBorderHeight
            // внутренние поля в рамке блюда
            setStylePropertyValue("dishItemStyle", "Padding",  new Thickness(0.06 * dishBorderHeight, 0.05 * dishBorderHeight, 0, 0.05 * dishBorderHeight));
            
            // изображение блюда (1:1.33)
            setStylePropertyValue("dishImageBorderStyle", "Height", 1.0 * dishBorderHeight);
            setStylePropertyValue("dishImageBorderStyle", "Width", 1.33 * dishBorderHeight);
            
            //  наименование блюда
            setStylePropertyValue("dishNameStyle", "FontSize", dishNameFontSize);
            // ед.изм. блюда
            setStylePropertyValue("dishUnitStyle", "FontSize", dishUnitFontSize);
            // маркеры блюда
            setStylePropertyValue("dishMarksItemStyle", "Height", 1.2*dishUnitFontSize);
            
            // заголовок ингредиентов
            setStylePropertyValue("dishIngrTitleStyle", "FontSize", dishUnitFontSize);
            // наименование и цена ингредиента
            setStylePropertyValue("dishIngrStyle", "FontSize", dishUnitFontSize);
            setStylePropertyValue("dishIngrDelImageStyle", "Width", 2.5 * dishUnitFontSize);
            setStylePropertyValue("dishIngrDelImageStyle", "Padding", new Thickness(0.5 * dishUnitFontSize, 0.2 * dishUnitFontSize, 0.5 * dishUnitFontSize, 0.2 * dishUnitFontSize));
            // количество и цена порции
            setStylePropertyValue("dishPortionTextStyle", "FontSize", dKoefPortionCount * dishNameFontSize);

            // промокод
            AppLib.SetPromocodeTextStyle(txtPromoCode);

            // яркость фона
            string opacity = AppLib.GetAppSetting("BackgroundImageBrightness");
            if (opacity != null) imgBackground.Opacity = opacity.ToDouble();

            // заголовок списка
            dishesListTitle.Margin = new Thickness(0,titleFontSize,0, titleFontSize);
            dishesListTitle.FontSize = 1.5* titleFontSize;

            // большие кнопки скроллинга
            var v = Enum.Parse(typeof(HorizontalAlignment), (string)AppLib.GetAppGlobalValue("dishesPanelScrollButtonHorizontalAlignment"));
            btnScrollDown.Width = (double)AppLib.GetAppGlobalValue("dishesPanelScrollButtonSize");
            btnScrollDown.Height = (double)AppLib.GetAppGlobalValue("dishesPanelScrollButtonSize");
            btnScrollDown.HorizontalAlignment = (HorizontalAlignment)v;
            btnScrollUp.Width = (double)AppLib.GetAppGlobalValue("dishesPanelScrollButtonSize");
            btnScrollUp.Height = (double)AppLib.GetAppGlobalValue("dishesPanelScrollButtonSize");
            btnScrollUp.HorizontalAlignment = (HorizontalAlignment)v;

        }  // method

        private void setStylePropertyValue(string styleName, string propName, object value)
        {
            Style stl = (Style)this.Resources[styleName];
            Setter str = (Setter)stl.Setters.FirstOrDefault(s => (s is Setter) ? (s as Setter).Property.Name == propName : false);

            if (str != null) str.Value = value;
        }

        private void setLngInnerBtnSizes(Border btnLangInner, TextBlock lblLang, double dLangSize)
        {
            double dMargin = (1.0 - dLangSize) / 2.0;
            btnLangInner.Height = dLangSize; btnLangInner.Width = dLangSize;
            btnLangInner.CornerRadius = new CornerRadius(dLangSize / 2.0);
            btnLangInner.Margin = new Thickness(dMargin);

            // размер шрифта на язык.кнопках
            double txtFontSize = 0.35 * dLangSize;
            lblLang.FontSize = txtFontSize;

        }

        #region работа с промокодом
        private void brdPromoCode_PreviewTouchDown(object sender, TouchEventArgs e)
        {
            //MessageBox.Show("promocode win");
        }
        private void brdPromoCode_PreviewMouseUp(object sender, MouseButtonEventArgs e)
        {
            //if (e.StylusDevice != null) return;

            AppLib.WriteAppAction(this.Name, AppActionsEnum.ButtonPromocode);

            string preText = App.PromocodeNumber ?? "";

            Promocode promoWin = new Promocode();
            promoWin.ShowDialog();

            e.Handled = true;
            // чтобы не срабатывали обработчики нижележащих контролов
            AppLib.IsEventsEnable = false;

            if (!(App.PromocodeNumber ?? "").Equals(preText))
            {
                AppLib.SetPromocodeTextStyle(this.txtPromoCode);
                AppLib.SetPromocodeTextStyle((App.Current.MainWindow as MainWindow).txtPromoCode);
            }
        }
        #endregion

        #region dish list behaviour
        private void lstDishes_RequestBringIntoView(object sender, RequestBringIntoViewEventArgs e)
        {
            e.Handled = true;
        }

        private void scrollDishes_PreviewTouchDown(object sender, TouchEventArgs e)
        {
            initDrag(e.GetTouchPoint(scrollDishes).Position);
        }
        private void scrollDishes_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
//            if (e.StylusDevice != null) return;

            initDrag(e.GetPosition(scrollDishes));
        }

        private void scrollDishes_PreviewTouchUp(object sender, TouchEventArgs e)
        {
            endDrag();
        }
        private void scrollDishes_PreviewMouseUp(object sender, MouseButtonEventArgs e)
        {
//            if (e.StylusDevice != null) return;

            endDrag();
        }

        private void scrollDishes_PreviewTouchMove(object sender, TouchEventArgs e)
        {
            if (lastDragPoint.HasValue)
            {
                doMove(e.GetTouchPoint((sender as IInputElement)).Position);
            }
        }
        private void scrollDishes_PreviewMouseMove(object sender, MouseEventArgs e)
        {
//            if (e.StylusDevice != null) return;

            if (lastDragPoint.HasValue && e.LeftButton == MouseButtonState.Pressed)
            {
                doMove(e.GetPosition(sender as IInputElement));
            }
        }


        private void initDrag(Point mousePos)
        {
            if (AppLib.IsEventsEnable == false) { AppLib.IsEventsEnable = true; }

            //_dateTime = DateTime.Now;
            //make sure we still can use the scrollbars
            if (mousePos.X <= scrollDishes.ViewportWidth && mousePos.Y < scrollDishes.ViewportHeight)
            {
                //scrollDishes.Cursor = Cursors.SizeAll;
                initDragPoint = mousePos;
                lastDragPoint = initDragPoint;
                //Mouse.Capture(scrollViewer);
            }
        }

        private void scrollDishes_ScrollChanged(object sender, ScrollChangedEventArgs e)
        {
            // debug
            //return;

            Visibility visButtonTop, visButtonBottom;

            if (e.VerticalOffset == 0)
            {
                visButtonTop = Visibility.Hidden;
                visButtonBottom = (pnlDishes.ActualHeight == scrollDishes.ActualHeight) ? Visibility.Hidden : Visibility.Visible;
            }
            else if (e.VerticalOffset == (pnlDishes.ActualHeight - scrollDishes.ActualHeight))
            {
                visButtonTop = Visibility.Visible;
                visButtonBottom = Visibility.Hidden;
            }
            else
            {
                visButtonTop = Visibility.Visible;
                visButtonBottom = Visibility.Visible;
            }

            if (btnScrollDown.Visibility != visButtonBottom) btnScrollDown.Visibility = visButtonBottom;
            if (btnScrollUp.Visibility != visButtonTop) btnScrollUp.Visibility = visButtonTop;
        }

        private void endDrag()
        {
            if ((lastDragPoint == null) || (initDragPoint == null))
            {
                _isDrag = false;
            }
            else
            {
                _isDrag = (Math.Abs(lastDragPoint.Value.X - initDragPoint.Value.X) > 10) || (Math.Abs(lastDragPoint.Value.Y - initDragPoint.Value.Y) > 15);
            }
        }
        private void doMove(Point posNow)
        {
            if (AppLib.IsEventsEnable == false) { return; }

            double dX = posNow.X - lastDragPoint.Value.X;
            double dY = posNow.Y - lastDragPoint.Value.Y;

            lastDragPoint = posNow;
            //Debug.Print(posNow.ToString());
            scrollDishes.ScrollToHorizontalOffset(scrollDishes.HorizontalOffset - dX);
            scrollDishes.ScrollToVerticalOffset(scrollDishes.VerticalOffset - dY);
        }
        #endregion

        #region animated scrolling
        private void btnScrollUp_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (lstDishes.Items.Count == 0) return;

            int iRows = lstDishes.Items.Count;
            // высота панели блюда
            double h1 = (lstDishes.ItemContainerGenerator.ContainerFromIndex(0) as ListBoxItem).ActualHeight;
            double dH = dishesListTitle.ActualHeight;
            // текущая строка
            Matrix matrix = (Matrix)pnlDishes.TransformToVisual(scrollDishes).GetValue(MatrixTransform.MatrixProperty);
            double dFrom = Math.Abs(matrix.OffsetY);
            int curRow = Convert.ToInt32(Math.Floor((dFrom - dH) / h1));

            if ((Convert.ToInt32(((dFrom- dH) % h1)) == 0) && (curRow > 0)) curRow--;

            double dTo = curRow * h1 + ((curRow == 0) ? 0d : dH);
            animateDishesScroll(dFrom, dTo, Math.Abs(dTo - dFrom) >= h1);
        }

        private void btnScrollDown_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (lstDishes.Items.Count == 0) return;

            int iRows = lstDishes.Items.Count;
            // высота панели блюда
            double h1 = (lstDishes.ItemContainerGenerator.ContainerFromIndex(0) as ListBoxItem).ActualHeight;
            double dH = dishesListTitle.ActualHeight;
            // текущая строка
            Matrix matrix = (Matrix)pnlDishes.TransformToVisual(scrollDishes).GetValue(MatrixTransform.MatrixProperty);
            double dFrom = Math.Abs(matrix.OffsetY);
            int curRow = Convert.ToInt32(Math.Floor(dFrom / h1)) + 1;

            // переход к следующей строке
            double dTo;
            if (curRow < (iRows - 1))
            {
                dTo = (curRow) * h1 + dH;
                animateDishesScroll(dFrom, dTo, Math.Abs(dTo - dFrom) >= h1);
            }
            // или в конец списка
            else if (curRow == (iRows - 1))
            {
                dTo = pnlDishes.ActualHeight - dH;
                animateDishesScroll(dFrom, dTo, Math.Abs(dTo - dFrom) >= h1);
            }
        }

        private void animateDishesScroll(double dFrom, double dTo, bool isEasing)
        {
            double mSec = (isEasing) ? 1000 : 200;

            DoubleAnimation vertAnim = new DoubleAnimation();
            vertAnim.From = dFrom;
            vertAnim.To = dTo;
            vertAnim.DecelerationRatio = .2;
            vertAnim.Duration = new Duration(TimeSpan.FromMilliseconds(mSec));
            if (isEasing) vertAnim.EasingFunction = new ExponentialEase() { EasingMode = EasingMode.EaseInOut };

            Storyboard sb = new Storyboard();
            sb.Children.Add(vertAnim);
            Storyboard.SetTarget(vertAnim, scrollDishes);
            Storyboard.SetTargetProperty(vertAnim, new PropertyPath(AniScrollViewer.CurrentVerticalOffsetProperty));
            sb.Begin();
        }

        #endregion


        #region close win
        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape) closeWin();
        }

        private void btnReturn_MouseUp(object sender, MouseButtonEventArgs e)
        {
            //if (e.StylusDevice != null) return;
            closeWin();
        }
        private void btnReturn_PreviewTouchDown(object sender, TouchEventArgs e)
        {
            closeWin();
        }

        private void closeWin()
        {
            this.Close();
        }
        #endregion

        #region portion Count

        private void portionCountDel_TouchUp(object sender, TouchEventArgs e)
        {
            if (_isDrag) return;

            // touch-события контролов не вызывают событие SelectionChanged списка !!!!
            AppLib.SelectListBoxItemByHisInnerControl((FrameworkElement)sender, lstDishes); // делаем это принудетельно

            lock (this)
            {
                portionCountDel();
            }
        }
        private void portionCountDel_MouseUp(object sender, MouseButtonEventArgs e)
        {
            //if (e.StylusDevice != null) return;
            if (_isDrag) return;

            // touch-события контролов не вызывают событие SelectionChanged списка !!!!
            //AppLib.SelectListBoxItemByHisInnerControl((FrameworkElement)sender, lstDishes); // делаем это принудетельно

            lock (this)
            {
                portionCountDel();
            }
        }

        private void portionCountAdd_TouchUp(object sender, TouchEventArgs e)
        {
            //MessageBox.Show("portionCountAdd_TouchUp: e.TouchDevice - " + ((e.TouchDevice == null) ? "null" : e.TouchDevice.ToString()) + " [e.Device - " + e.Device.ToString() + "]");

            if (_isDrag) return;

            // touch-события контролов не вызывают событие SelectionChanged списка !!!!
            AppLib.SelectListBoxItemByHisInnerControl((FrameworkElement)sender, lstDishes); // делаем это принудетельно

            lock (this)
            {
                portionCountAdd();
            }
        }
        private void portionCountAdd_MouseUp(object sender, MouseButtonEventArgs e)
        {
            //MessageBox.Show("portionCountAdd_MouseUp: e.StylusDevice - " + ((e.StylusDevice == null) ? "null" : e.StylusDevice.ToString()) + " [e.Device - " + e.Device.ToString() + "]");

            //if (e.StylusDevice != null) return;

            // touch-события контролов не вызывают событие SelectionChanged списка !!!!
            //AppLib.SelectListBoxItemByHisInnerControl((FrameworkElement)sender, lstDishes); // делаем это принудетельно

            if (_isDrag) return;

            lock (this)
            {
                portionCountAdd();
            }
        }

        private void dishDel_TouchUp(object sender, TouchEventArgs e)
        {
            //MessageBox.Show("dishDel_TouchUp: e.TouchDevice - " + ((e.TouchDevice == null)?"null":e.TouchDevice.ToString()) + " [e.Device - " + e.Device.ToString() + "]");
            e.Handled = true;

            if (_isDrag) return;

            // touch-события контролов не вызывают событие SelectionChanged списка !!!!
            AppLib.SelectListBoxItemByHisInnerControl((FrameworkElement)sender, lstDishes); // делаем это принудетельно

            lock (this)
            {
                dishDel();
            }
        }
        private void dishDel_MouseUp(object sender, MouseButtonEventArgs e)
        {
            //MessageBox.Show("dishDel_MouseUp: e.StylusDevice - " + ((e.StylusDevice==null)?"null":e.StylusDevice.ToString()) + " [e.Device - " + e.Device.ToString() + "]");
            //if (e.StylusDevice != null) return;

            // touch-события контролов не вызывают событие SelectionChanged списка !!!!
            //AppLib.SelectListBoxItemByHisInnerControl((FrameworkElement)sender, lstDishes); // делаем это принудетельно

            if (_isDrag) return;

            lock (this)
            {
                dishDel();
            }
        }

        private void ingrDel_TouchUp(object sender, TouchEventArgs e)
        {
            MessageBox.Show("ingrDel_TouchUp: e.TouchDevice-" + ((e.TouchDevice==null)?"null":e.TouchDevice.ToString()) + ", e.Device" + e.Device.ToString());
            if (_isDrag) return;

            // touch-события контролов не вызывают событие SelectionChanged списка !!!!
            //AppLib.SelectListBoxItemByHisInnerConttrol((FrameworkElement)sender, lstDishes); // делаем это принудетельно

            //lock (this)
            //{
            //    ingrDel(sender, e.GetTouchPoint(this).Position);
            //}
        }
        private void ingrDel_MouseUp(object sender, MouseButtonEventArgs e)
        {
            //MessageBox.Show("ingrDel_MouseUp: e.StylusDevice-" + ((e.StylusDevice==null)?"null":e.StylusDevice.ToString()) + ", e.Device" + e.Device.ToString());
            //            if (e.StylusDevice != null) return;

            if (_isDrag) return;

            lock (this)
            {
                ingrDel(sender, e.GetPosition(this));
            }
        }

        private void ingrDel(object sender, Point point)
        {
            // получить контролы из нажатого элемента
            //    listbox ингредиентов
            AppLib.WriteLogTraceMessage("");
            AppLib.WriteLogTraceMessage(string.Format("нажата кнопка с координатами ({0}, {1}), type - {2}",point.X, point.Y, sender.GetType().Name));
            ListBoxItem lbiIngr = (ListBoxItem)AppLib.FindVisualParentByType((FrameworkElement)sender, typeof(ListBoxItem));

            ListBox lbIngr = (ListBox)AppLib.FindVisualParentByType((FrameworkElement)lbiIngr, typeof(ListBox));
//            AppLib.WriteLogTraceMessage(string.Format("найден список ингредиентов - {0}",lbIngr));
            DishAdding tmpIngr = (DishAdding)lbIngr.ItemContainerGenerator.ItemFromContainer(lbiIngr);
//            AppLib.WriteLogTraceMessage(string.Format("найден элемент в списке - {0}, {1}", lbiIngr.ToString(), tmpIngr.langNames["ru"]));

            int iCnt = lbIngr.ItemContainerGenerator.IndexFromContainer(lbiIngr);
//            AppLib.WriteLogTraceMessage(string.Format("индекс контейнера в списке - {0}",iCnt));

            if (lbIngr.SelectedIndex != iCnt) lbIngr.SelectedIndex = iCnt;
//            AppLib.WriteLogTraceMessage(string.Format("индекс элемента в списке - {0}", lbIngr.SelectedIndex));
            
            //    listbox блюд
            AppLib.SelectListBoxItemByHisInnerControl((FrameworkElement)lbIngr, lstDishes); // делаем это принудетельно
//            AppLib.WriteLogTraceMessage(string.Format("индекс блюда в списке - {0}", lstDishes.SelectedIndex));

            DishItem dishItem = (DishItem)lstDishes.SelectedItem;
            DishAdding ingrItem = (DishAdding)lbIngr.SelectedItem;

            //MessageBox.Show(string.Format("ингредиент - {0}, блюдо - {1}", ingrItem.langNames["ru"], dishItem.langNames["ru"]));
            string actLogMsg = string.Format("{0};{1}", ingrItem.langNames["ru"], dishItem.langNames["ru"]);
            AppLib.WriteAppAction(this.Name, AppActionsEnum.ButtonIngredientRemove, actLogMsg);

            string title = AppLib.GetLangTextFromAppProp("cartDelIngrTitle");
            string msg = string.Format("{0} \"{1}\" ?", AppLib.GetLangTextFromAppProp("cartDelIngrQuestion"), AppLib.GetLangText(ingrItem.langNames));

            MessageBoxResult result = AppLib.ShowChoiceBox(title, msg);

            AppLib.WriteAppAction(this.Name, AppActionsEnum.ButtonIngredientRemoveResult, result.ToString() + ";" + actLogMsg);
            if (result == MessageBoxResult.Yes)
            {
                dishItem.SelectedIngredients.Remove(ingrItem);
                lbIngr.Items.Refresh();

                updatePriceControls();
            }
            this.Activate();
        }

        private void dishDel()
        {
            DishItem dishItem = (DishItem)lstDishes.SelectedItem;
            if (dishItem == null) return;
            OrderItem order = AppLib.GetCurrentOrder();

            string actLogMsg = string.Format("{0}", dishItem.langNames["ru"]);
            AppLib.WriteAppAction(this.Name, AppActionsEnum.ButtonDishRemove, actLogMsg);

            string title = AppLib.GetLangTextFromAppProp("cartDelDishTitle");
            string msg = string.Format("{0} \"{1}\" ?", AppLib.GetLangTextFromAppProp("cartDelDishQuestion"), AppLib.GetLangText(dishItem.langNames));

            MessageBoxResult result = AppLib.ShowChoiceBox(title, msg);

            AppLib.WriteAppAction(this.Name, AppActionsEnum.ButtonDishRemoveResult, result.ToString() + ";" + actLogMsg);
            if (result == MessageBoxResult.Yes)
            {
                order.Dishes.Remove(dishItem);
                lstDishes.Items.Refresh();
                scrollDishes.ScrollToTop();

                updatePriceOrder();
            }
        }
        private MsgBoxExt getDelMsgBox()
        {
            string sYes = AppLib.GetLangTextFromAppProp("dialogBoxYesText");
            string sNo = AppLib.GetLangTextFromAppProp("dialogBoxNoText");

            MsgBoxExt mBox = new MsgBoxExt()
            {
                TitleFontSize = (double)AppLib.GetAppGlobalValue("appFontSize6"),
                MessageFontSize = (double)AppLib.GetAppGlobalValue("appFontSize2"),
                MsgBoxButton = MessageBoxButton.YesNo,
                ButtonsText = string.Format(";;{0};{1}", sYes, sNo),
                ButtonBackground = (System.Windows.Media.Brush)AppLib.GetAppGlobalValue("appBackgroundColor"),
                ButtonBackgroundOver = (System.Windows.Media.Brush)AppLib.GetAppGlobalValue("appBackgroundColor"),
                ButtonForeground = Brushes.White,
                ButtonFontSize = (double)AppLib.GetAppGlobalValue("appFontSize4")
            };

            return mBox;
        }

        private void portionCountDel()
        {
            DishItem curDish = (DishItem)lstDishes.SelectedItem;
            if (curDish == null) return;

            if (curDish.Count > 1)
            {
                curDish.Count--;
                updatePriceControls();

                string actLogMsg = string.Format("{0};{1}", curDish.Count.ToString(), curDish.langNames["ru"]);
                AppLib.WriteAppAction(this.Name, AppActionsEnum.DishPortionDel, actLogMsg);
            }
        }
        private void portionCountAdd()
        {
            DishItem curDish = (DishItem)lstDishes.SelectedItem;
            if (curDish == null) return;

            curDish.Count++;
            updatePriceControls();

            string actLogMsg = string.Format("{0};{1}", curDish.Count.ToString(), curDish.langNames["ru"]);
            AppLib.WriteAppAction(this.Name, AppActionsEnum.DishPortionAdd, actLogMsg);
        }

        #endregion

        // ПЕЧАТЬ чека
        #region print invoice
        private void btnPrintCheck_PreviewTouchDown(object sender, TouchEventArgs e)
        {
            printClientInvoice();
        }
        private void btnPrintCheck_MouseDown(object sender, MouseButtonEventArgs e)
        {
            //            if (e.StylusDevice != null) return;

            printClientInvoice();
        }  // method

        private void printClientInvoice()
        {
            decimal orderValue = _currentOrder.GetOrderValue();
            AppLib.WriteAppAction(this.Name, AppActionsEnum.ButtonPrintOrder, orderValue.ToString());

            // если стоимость чека == 0, то выйти
            if (orderValue == 0) return;

            TakeOrder takeOrderWin = new TakeOrder();
            takeOrderWin.ShowDialog();

            // сохранить в заказе флажок "с собой"
            _currentOrder.takeAway = (takeOrderWin.TakeOrderMode == TakeOrderEnum.TakeAway);

            if (takeOrderWin.TakeOrderMode != TakeOrderEnum.None)
            {
                PrintBill prn = new PrintBill(_currentOrder);
                string userErrMsg = null;

                bool result = prn.CreateBill(out userErrMsg);

                string actLogMsg = string.Format("{0};{1};{2};{3};{4}",result.ToString(), _currentOrder.OrderNumberForPrint.ToString(), _currentOrder.OrderDate, _currentOrder.DeviceID,  (result ? "" : "(" + userErrMsg + ")"));
                AppLib.WriteAppAction(this.Name, AppActionsEnum.OrderPrintResult, actLogMsg);

                string title = (string)AppLib.GetLangTextFromAppProp("printOrderTitle");
                string msgText;
                // формирование чека и печать завершилась успешно - сохраняем заказ в БД
                if (result == true)
                {
                    bool saveRes = _currentOrder.SaveToDB(out userErrMsg);

                    actLogMsg = string.Format("{0};{1}", result.ToString(), (result ? "" : "(" + userErrMsg + ")"));
                    AppLib.WriteAppAction(this.Name, AppActionsEnum.OrderSaveToDBResult, actLogMsg);

                    if (saveRes == true)
                    {
                        //msgText = (string)AppLib.GetLangTextFromAppProp("lblGoText");
                        //int delayInfoWin = AppLib.GetAppSetting("AutoCloseMsgBoxAfterPrintOrder").ToInt();
                        // 2017-02-17 убрать окно "Теперь можете подходить с чеком к кассе для оплаты"
                        //AppLib.ShowMessage(title, msgText, delayInfoWin);

                        // вернуть интерфейс в исходное состояние и создать новый заказ
                        AppLib.ReStartApp(false, true, true);
                        // поставить таймер бездействия на паузу
                        if (App.IdleHandler != null) App.IdleHandler.SetPause();
                    }
                    // ошибка сохранения в БД
                    else
                    {
                        AppLib.WriteLogErrorMessage(userErrMsg);
                        msgText = (string)AppLib.GetLangTextFromAppProp("saveOrderErrorMessage");
                        msgText = msgText.Replace("\\n", Environment.NewLine);
                        AppLib.ShowMessageBox(title, msgText);
                    }
                }

                // ошибка формирования чека и/или печати - сообщение пользователю на экран
                else
                {
                    AppLib.ShowMessageBox(title, userErrMsg);
                }
            }  // if
        }

        #endregion

        #region price funcs
        private void updatePriceControls()
        {
            var container = lstDishes.ItemContainerGenerator.ContainerFromIndex(lstDishes.SelectedIndex) as FrameworkElement;
            if (container != null)
            {
                ContentPresenter queueListBoxItemCP = AppLib.FindVisualChildren<ContentPresenter>(container).First();
                if (queueListBoxItemCP != null)
                {
                    DataTemplate dataTemplate = queueListBoxItemCP.ContentTemplate;
                    TextBlock txtDishCount = (TextBlock)dataTemplate.FindName("txtDishCount", queueListBoxItemCP);
                    TextBlock txtDishPrice = (TextBlock)dataTemplate.FindName("txtDishPrice", queueListBoxItemCP);

                    BindingExpression be = txtDishCount.GetBindingExpression(TextBlock.TextProperty);
                    be.UpdateTarget();
                    be = txtDishPrice.GetBindingExpression(TextBlock.TextProperty);
                    be.UpdateTarget();

                    updatePriceOrder();
                }
            }
        }  // updatePriceControl()

        private void updatePriceOrder()
        {
            decimal orderValue = _currentOrder.GetOrderValue();
            txtOrderPrice.Text = AppLib.GetCostUIText(orderValue);

            // также обновить на главном меню
            MainWindow mainWin = (MainWindow)Application.Current.MainWindow;
            mainWin.lblOrderPrice.Text = AppLib.GetCostUIText(_currentOrder.GetOrderValue());

            if (orderValue == 0) closeWin();
        }
        #endregion

        #region lang buttons
        private void btnLang_PreviewTouchDown(object sender, TouchEventArgs e)
        {
            string langId = getLangIdByButtonName(((FrameworkElement)sender).Name);
            selectAppLang(langId);
        }
        private void btnLang_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
//            if (e.StylusDevice != null) return;

            string langId = getLangIdByButtonName(((FrameworkElement)sender).Name);
            selectAppLang(langId);
        }
        private string getLangIdByButtonName(string langButtonName)
        {
            return langButtonName.Substring(langButtonName.Length - 2).ToLower();
        }
        public void selectAppLang(string langId)
        {
            AppLib.WriteAppAction(this.Name, AppActionsEnum.SelectLang, langId);

            setLangButtonStyle(false);  // "выключить" кнопку
            (App.Current.MainWindow as MainWindow).selectAppLang(langId);
            setLangButtonStyle(true);   // "включить" кнопку

            AppLib.SetPromocodeTextStyle(txtPromoCode);

            // установка текстов на выбранном языке
            updateBindingText(txtReturn);
            updateBindingText(pnlTotalLabel);
            updateBindingText(txtCashier);
            updateBindingText(txtPrintCheck);
            updateBindingText(dishesListTitle);

            lstDishes.Items.Refresh();
        }
        private void updateBindingText(TextBlock textBlock)
        {
            BindingExpression be = textBlock.GetBindingExpression(TextBlock.TextProperty);
            if (be != null) be.UpdateTarget();
        }

        private void setLangButtonStyle(bool checkedMode)
        {
            Border langBorder = getInnerLangBorder();
            if (langBorder != null)
            {
                Style newStyle = (checkedMode) ? (Style)App.Current.Resources["langButtonBorderCheckedStyle"] : (Style)App.Current.Resources["langButtonBorderUncheckedStyle"];
                if (langBorder.Style.Equals(newStyle) == false) langBorder.Style = newStyle;
            }
        }

        private Border getInnerLangBorder()
        {
            Border retVal = null;
            switch (AppLib.AppLang)
            {
                case "ua": retVal = btnLangUaInner; break;
                case "ru": retVal = btnLangRuInner; break;
                case "en": retVal = btnLangEnInner; break;
                default:
                    break;
            }
            return retVal;
        }
        #endregion

    }  // class
}
