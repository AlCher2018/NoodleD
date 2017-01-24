using AppModel;
using System;
using System.Collections.Generic;
using System.Diagnostics;
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
            if (e.StylusDevice != null) return;

            closeWin();
        }
        private void txtbackToMenu_TouchUp(object sender, TouchEventArgs e)
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
            portionCountAdd();
        }

        private void portionCountDel_MouseUp(object sender, MouseButtonEventArgs e)
        {
            if ((lastDragPoint != null) && lastDragPoint.Equals(initDragPoint) == false) { lastDragPoint = null; return; }
            portionCountDel();
        }

        private void dishDel_MouseUp(object sender, MouseButtonEventArgs e)
        {
            if ((lastDragPoint != null) && lastDragPoint.Equals(initDragPoint) == false) { lastDragPoint = null; return; }
            dishDel();
        }

        private void ingrDel_MouseUp(object sender, MouseButtonEventArgs e)
        {
            if ((lastDragPoint != null) && lastDragPoint.Equals(initDragPoint) == false) { lastDragPoint = null; return; }

            ingrDel(sender);
        }

        private void ingrDel(object sender)
        {
            ListBox ingrList = (ListBox)AppLib.GetAncestorByType(sender as DependencyObject, typeof(ListBox));
            ListBoxItem lbItem = (ListBoxItem)AppLib.GetAncestorByType(ingrList as DependencyObject, typeof(ListBoxItem));
            DishItem dishItem = (DishItem)lstDishes.ItemContainerGenerator.ItemFromContainer(lbItem);

            lstDishes.SelectedItem = dishItem;
            DishAdding ingrItem = (DishAdding)ingrList.SelectedItem;
            if (ingrItem == null) return;

            MessageBoxDialog msg = new MessageBoxDialog();
            Dictionary<string, string> v = (Dictionary<string, string>)AppLib.GetAppGlobalValue("cartDelIngrTitle");
            msg.Title = AppLib.GetLangText(v);
            v = (Dictionary<string, string>)AppLib.GetAppGlobalValue("cartDelIngrQuestion");
            msg.MessageText = string.Format("{0}\n{1} ?", AppLib.GetLangText(v), AppLib.GetLangText(ingrItem.langNames));
            bool result = msg.ShowDialog();
            msg = null;

            if (result == true)
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

            MessageBoxDialog msg = new MessageBoxDialog();
            Dictionary<string, string> v = (Dictionary<string, string>)AppLib.GetAppGlobalValue("cartDelDishTitle");
            msg.Title = AppLib.GetLangText(v);
            v = (Dictionary<string, string>)AppLib.GetAppGlobalValue("cartDelDishQuestion");
            msg.MessageText = string.Format("{0}\n{1} ?", AppLib.GetLangText(v), AppLib.GetLangText(dishItem.langNames));
            bool result = msg.ShowDialog();
            msg = null;

            if (result == true)
            {
                order.Dishes.Remove(dishItem);
                lstDishes.Items.Refresh();
                scrollDishes.ScrollToTop();

                updatePriceOrder();
            }
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
                PrintBill prn = new PrintBill(_currentOrder, takeMode);
                string userErrMsg = null;
                bool result = prn.CreateBill(out userErrMsg);

                // формирование чека и печать завершилась успешно - сохраняем заказ в БД
                if (result == true)
                {
                    bool saveRes = _currentOrder.SaveToDB(out userErrMsg);
                    if (saveRes == true)
                    {
                        string goText = (string)AppLib.GetLangTextFromAppProp("lblGoText");
                        AppLib.ShowMessage(goText);

                        // очистить заказ...
                        _currentOrder.Clear();
                        // вернуть интерфейс в исходное состояние
                        AppLib.ReDrawApp(false, true);

                    }
                    // ошибка сохранения в БД
                    else
                    {
                        AppLib.AppLogger.Error(userErrMsg);
                        AppLib.ShowMessage("Ошибка сохранения заказа!\nЗаказ не был сохранен. Обратитесь к администратору приложения.");
                    }
                }
                // ошибка формирования чека и/или печати - сообщение пользователю на экран и запись в лог
                else
                {
                    AppLib.ShowMessage(userErrMsg);
                }
            }
        }

        private void updatePriceOrder()
        {
            txtOrderPrice.Text = AppLib.GetCostUIText(_currentOrder.GetOrderValue());

            // также обновить на главном меню
            MainWindow mainWin = (MainWindow)Application.Current.MainWindow;
            mainWin.txtOrderPrice.Text = AppLib.GetCostUIText(_currentOrder.GetOrderValue());
        }

    }
}
