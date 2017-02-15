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

namespace WpfClient
{
    /// <summary>
    /// Interaction logic for Cart.xaml
    /// </summary>
    public partial class Cart : Window
    {
        // dragging
        Point? lastDragPoint, initDragPoint;
        protected DateTime _dateTime;
        OrderItem _currentOrder;

        public Cart()
        {
            InitializeComponent();

            _currentOrder = (OrderItem)AppLib.GetAppGlobalValue("currentOrder");
            this.lstDishes.ItemsSource = _currentOrder.Dishes;

            updatePriceOrder();
        }

        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape) closeWin();
        }

        private void txtbackToMenu_MouseUp(object sender, MouseButtonEventArgs e)
        {
            closeWin();
        }



        #region dish list behaviour
        private void lstDishes_RequestBringIntoView(object sender, RequestBringIntoViewEventArgs e)
        {
            e.Handled = true;
        }

        private void initDrag(Point mousePos)
        {
            //_dateTime = DateTime.Now;
            //make sure we still can use the scrollbars
            if (mousePos.X <= scrollDishes.ViewportWidth && mousePos.Y < scrollDishes.ViewportHeight)
            {
                //scrollViewer.Cursor = Cursors.SizeAll;
                initDragPoint = mousePos;
                lastDragPoint = initDragPoint;
                //Mouse.Capture(scrollViewer);
            }
        }
        private void endDrag()
        {
            //scrollViewer.Cursor = Cursors.Arrow;
            //scrollViewer.ReleaseMouseCapture();
            //lastDragPoint = null;
        }
        private void doMove(Point posNow)
        {
            double dX = posNow.X - lastDragPoint.Value.X;
            double dY = posNow.Y - lastDragPoint.Value.Y;

            lastDragPoint = posNow;

            scrollDishes.ScrollToHorizontalOffset(scrollDishes.HorizontalOffset - dX);
            scrollDishes.ScrollToVerticalOffset(scrollDishes.VerticalOffset - dY);
        }
        private void scrollDishes_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.StylusDevice != null) return;

            initDrag(e.GetPosition(scrollDishes));
        }

        private void scrollDishes_PreviewTouchDown(object sender, TouchEventArgs e)
        {
            initDrag(e.GetTouchPoint(scrollDishes).Position);
        }

        private void scrollDishes_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (e.StylusDevice != null) return;

            endDrag();
        }

        private void scrollDishes_PreviewTouchUp(object sender, TouchEventArgs e)
        {
            endDrag();
        }

        private void scrollDishes_MouseMove(object sender, MouseEventArgs e)
        {
            if (e.StylusDevice != null) return;

            if (lastDragPoint.HasValue && e.LeftButton == MouseButtonState.Pressed)
            {
                doMove(e.GetPosition(sender as IInputElement));
            }
        }

        private void scrollDishes_PreviewTouchMove(object sender, TouchEventArgs e)
        {
            if (lastDragPoint.HasValue)
            {
                doMove(e.GetTouchPoint((sender as IInputElement)).Position);
            }
        }

        #endregion

        private void closeWin()
        {
            this.Close();
            // DEBUG Cart
            //foreach (Window item in Application.Current.Windows)
            //{
            //    item.Close();
            //}
            //Application.Current.Shutdown();
        }


        #region portion Count

        private void portionCountAdd_MouseUp(object sender, MouseButtonEventArgs e)
        {
            if ((lastDragPoint != null) && lastDragPoint.Equals(initDragPoint) == false) { lastDragPoint = null; return; }
            lock (this)
            {
                portionCountAdd();
            }
            e.Handled = true;
        }

        private void portionCountDel_MouseUp(object sender, MouseButtonEventArgs e)
        {
            if ((lastDragPoint != null) && lastDragPoint.Equals(initDragPoint) == false) { lastDragPoint = null; return; }
            lock (this)
            {
                portionCountDel();
            }
            e.Handled = true;
        }

        private void dishDel_MouseUp(object sender, MouseButtonEventArgs e)
        {
            if ((lastDragPoint != null) && lastDragPoint.Equals(initDragPoint) == false) { lastDragPoint = null; return; }
            lock (this)
            {
                dishDel();
            }
            e.Handled = true;
        }

        private void ingrDel_MouseUp(object sender, MouseButtonEventArgs e)
        {
            if ((lastDragPoint != null) && lastDragPoint.Equals(initDragPoint) == false) { lastDragPoint = null; return; }
            lock (this)
            {
                ingrDel(sender);
            }
            e.Handled = true;

        }

        private void ingrDel(object sender)
        {
            ListBox ingrList = (ListBox)AppLib.GetAncestorByType(sender as DependencyObject, typeof(ListBox));
            ListBoxItem lbItem = (ListBoxItem)AppLib.GetAncestorByType(ingrList as DependencyObject, typeof(ListBoxItem));
            DishItem dishItem = (DishItem)lstDishes.ItemContainerGenerator.ItemFromContainer(lbItem);

            lstDishes.SelectedItem = dishItem;
            DishAdding ingrItem = (DishAdding)ingrList.SelectedItem;
            if (ingrItem == null) return;


            MsgBoxExt mBox = getDelMsgBox();
            mBox.Title = AppLib.GetLangTextFromAppProp("cartDelIngrTitle");
            mBox.MessageText = string.Format("{0} \"{1}\" ?", AppLib.GetLangTextFromAppProp("cartDelIngrQuestion"), AppLib.GetLangText(ingrItem.langNames));
            MessageBoxResult result = mBox.ShowDialog();
            mBox = null;

            if (result == MessageBoxResult.Yes)
            {
                dishItem.SelectedIngredients.Remove(ingrItem);
                ingrList.Items.Refresh();

                updatePriceControls();
            }
        }

        private void dishDel()
        {
            DishItem dishItem = (DishItem)lstDishes.SelectedItem;
            if (dishItem == null) return;
            OrderItem order = (OrderItem)AppLib.GetAppGlobalValue("currentOrder");

            MsgBoxExt mBox = getDelMsgBox();
            mBox.Title = AppLib.GetLangTextFromAppProp("cartDelDishTitle");
            mBox.MessageText = string.Format("{0} \"{1}\" ?", AppLib.GetLangTextFromAppProp("cartDelDishQuestion"), AppLib.GetLangText(dishItem.langNames));
            MessageBoxResult result = mBox.ShowDialog();
            mBox = null;

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
            }
        }
        private void portionCountAdd()
        {
            DishItem curDish = (DishItem)lstDishes.SelectedItem;
            if (curDish == null) return;

            curDish.Count++;
            updatePriceControls();
        }

        #endregion

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

        // ПЕЧАТЬ чека
        private void btnPrintCheck_MouseDown(object sender, MouseButtonEventArgs e)
        {
            // если стоимость чека == 0, то выйти
            if (_currentOrder.GetOrderValue() == 0) return;

            TakeOrder takeOrderWin = new TakeOrder();
            takeOrderWin.ShowDialog();
            TakeOrderEnum takeMode = takeOrderWin.TakeOrderMode;
            takeOrderWin = null;

            if (takeMode != TakeOrderEnum.None)
            {
                // сохранить в заказе флажок "с собой"
                _currentOrder.takeAway = (takeMode == TakeOrderEnum.TakeAway);

                PrintBill prn = new PrintBill(_currentOrder, takeMode);
                string userErrMsg = null;
                bool result = prn.CreateBill(out userErrMsg);

                string title = (string)AppLib.GetLangTextFromAppProp("printOrderTitle");
                string msgText;
                // формирование чека и печать завершилась успешно - сохраняем заказ в БД
                if (result == true)
                {
                    bool saveRes = _currentOrder.SaveToDB(out userErrMsg);
                    if (saveRes == true)
                    {
                        msgText = (string)AppLib.GetLangTextFromAppProp("lblGoText");
                        int delayInfoWin = (int)AppLib.GetAppGlobalValue("AutoCloseMsgBoxAfterPrintOrder", 0);
                        AppLib.ShowMessage(title, msgText, delayInfoWin);

                        // вернуть интерфейс в исходное состояние и очистить заказ
                        AppLib.ReDrawApp(false, true);

                    }
                    // ошибка сохранения в БД
                    else
                    {
                        AppLib.WriteLogErrorMessage(userErrMsg);
                        msgText = (string)AppLib.GetLangTextFromAppProp("printOrderErrorMessage");
                        AppLib.ShowMessage(title, msgText);
                    }
                }

                // ошибка формирования чека и/или печати - сообщение пользователю на экран и запись в лог
                else
                {
                    AppLib.ShowMessage(title, userErrMsg);
                }
            }  // if

        }  // method

        private void updatePriceOrder()
        {
            decimal orderValue = _currentOrder.GetOrderValue();
            txtOrderPrice.Text = AppLib.GetCostUIText(orderValue);

            // также обновить на главном меню
            MainWindow mainWin = (MainWindow)Application.Current.MainWindow;
            mainWin.txtOrderPrice.Text = AppLib.GetCostUIText(_currentOrder.GetOrderValue());

            if (orderValue == 0) closeWin();
        }

    }
}
