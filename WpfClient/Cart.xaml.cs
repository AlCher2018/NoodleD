using AppModel;
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
    /// Interaction logic for Cart.xaml
    /// </summary>
    public partial class Cart : Window
    {
        // dragging
        Point? lastDragPoint;
        protected DateTime _dateTime;
        bool _isMoved = false;

        public Cart()
        {
            InitializeComponent();

            OrderItem currentOrder = (OrderItem)AppLib.GetAppGlobalValue("currentOrder");
            this.lstDishes.ItemsSource = currentOrder.Dishes;
        }

        private void Window_PreviewKeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape) closeWin();
        }

        private void txtbackToMenu_MouseUp(object sender, MouseButtonEventArgs e)
        {
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
            _dateTime = DateTime.Now;
            _isMoved = false;
            //make sure we still can use the scrollbars
            if (mousePos.X <= scrollDishes.ViewportWidth && mousePos.Y < scrollDishes.ViewportHeight)
            {
                //scrollViewer.Cursor = Cursors.SizeAll;
                lastDragPoint = mousePos;
                //Mouse.Capture(scrollViewer);
            }
        }
        private void endDrag()
        {
            //scrollViewer.Cursor = Cursors.Arrow;
            //scrollViewer.ReleaseMouseCapture();
            lastDragPoint = null;
        }
        private void doMove(Point posNow)
        {
            double dX = posNow.X - lastDragPoint.Value.X;
            double dY = posNow.Y - lastDragPoint.Value.Y;

            lastDragPoint = posNow;

            scrollDishes.ScrollToHorizontalOffset(scrollDishes.HorizontalOffset - dX);
            scrollDishes.ScrollToVerticalOffset(scrollDishes.VerticalOffset - dY);
            _isMoved = true;
        }
        private void scrollDishes_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            initDrag(e.GetPosition(scrollDishes));
        }

        private void scrollDishes_PreviewTouchDown(object sender, TouchEventArgs e)
        {
            initDrag(e.GetTouchPoint(scrollDishes).Position);
        }

        private void scrollDishes_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            endDrag();
        }

        private void scrollDishes_PreviewTouchUp(object sender, TouchEventArgs e)
        {
            endDrag();
        }

        private void scrollDishes_MouseMove(object sender, MouseEventArgs e)
        {
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
            portionCountAdd();
        }

        private void portionCountAdd_TouchUp(object sender, TouchEventArgs e)
        {
            portionCountAdd();
        }

        private void portionCountDel_MouseUp(object sender, MouseButtonEventArgs e)
        {
            portionCountDel();
        }

        private void portionCountDel_TouchUp(object sender, TouchEventArgs e)
        {
            portionCountDel();
        }

        private void ingrDel(object sender)
        {
            ListBox ingrList = (ListBox)AppLib.GetAncestorByType(sender as DependencyObject, typeof(ListBox));
            ListBoxItem lbItem = (ListBoxItem)AppLib.GetAncestorByType(ingrList as DependencyObject, typeof(ListBoxItem));
            DishItem dishItem = (DishItem)lstDishes.ItemContainerGenerator.ItemFromContainer(lbItem);

            lstDishes.SelectedItem = dishItem;
            DishAdding ingrItem = (DishAdding)ingrList.SelectedItem;

            MessageBoxResult result = MessageBox.Show(string.Format("Удалить ингредиент\n{0} ?", ingrItem.langNames["ru"]), "Удаление ингредиента", MessageBoxButton.YesNo);
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
            OrderItem order = (OrderItem)AppLib.GetAppGlobalValue("currentOrder");

            MessageBoxResult result = MessageBox.Show(string.Format("Удалить блюдо\n{0} ?", dishItem.langNames["ru"]), "Удаление блюда из заказа", MessageBoxButton.YesNo);
            if (result == MessageBoxResult.Yes)
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

            if (curDish.Count > 1)
            {
                curDish.Count--;
                updatePriceControls();
            }
        }
        private void portionCountAdd()
        {
            DishItem curDish = (DishItem)lstDishes.SelectedItem;

            curDish.Count++;
            updatePriceControls();
        }

        #endregion

        private void ingrDel_MouseUp(object sender, MouseButtonEventArgs e)
        {
            ingrDel(sender);
        }

        private void ingrDel_TouchUp(object sender, TouchEventArgs e)
        {
            ingrDel(sender);
        }

        private void dishDel_MouseUp(object sender, MouseButtonEventArgs e)
        {
            dishDel();
        }

        private void dishDel_TouchUp(object sender, TouchEventArgs e)
        {
            dishDel();
        }


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
            BindingExpression be = txtOrderPrice.GetBindingExpression(TextBlock.TextProperty);
            be.UpdateTarget();

            // также обновить на главном меню
            MainWindow mainWin = (MainWindow)Application.Current.MainWindow;
            be = mainWin.txtOrderPrice.GetBindingExpression(TextBlock.TextProperty);
            be.UpdateTarget();
        }

    }
}
