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
    /// Interaction logic for DishPopup.xaml
    /// </summary>
    public partial class DishPopup : Window
    {
        private bool _isClose;
        private bool _isTouchOnly;

        public DishPopup()
        {
            InitializeComponent();
            _isClose = true;
            _isTouchOnly = true;
        }

        private void Window_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape) this.Close();
        }

        #region закрытие всплывашки

        private void btnClose_PreviewMouseUp(object sender, MouseButtonEventArgs e)
        {
            if (_isTouchOnly == true) return;

            closeWin(e);
        }
        private void btnClose_PreviewTouchUp(object sender, TouchEventArgs e)
        {
            closeWin(e);
        }

        private void gridWindow_MouseUp(object sender, MouseButtonEventArgs e)
        {
            if (_isTouchOnly == true) return;

            if (_isClose == true) closeWin(e);
            else _isClose = true;
        }
        private void gridWindow_TouchUp(object sender, TouchEventArgs e)
        {
            if (_isClose == true) closeWin(e);
            else _isClose = true;
        }
        private void borderMain_MouseUp(object sender, MouseButtonEventArgs e)
        {
            if (_isTouchOnly == true) return;

            _isClose = false;
        }

        private void borderMain_TouchUp(object sender, TouchEventArgs e)
        {
            _isClose = false;
        }

        private void closeWin(RoutedEventArgs e = null)
        {
            if (e != null) e.Handled = true;
            this.Close();
        }
        #endregion


        private void btnAddDish_MouseUp(object sender, MouseButtonEventArgs e)
        {
            if (_isTouchOnly == true) return;

            addDishToOrder();
        }
        private void btnAddDish_PreviewTouchUp(object sender, TouchEventArgs e)
        {
            addDishToOrder();
        }


        private void listIngredients_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            DishItem dishItem = (DishItem)DataContext;

            if (dishItem.SelectedIngredients == null) dishItem.SelectedIngredients = new List<DishAdding>();
            else dishItem.SelectedIngredients.Clear();

            foreach (DishAdding item in listIngredients.SelectedItems)
            {
                dishItem.SelectedIngredients.Add(item);
            }
            updatePriceControl();
        }

        private void listRecommends_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            DishItem dishItem = (DishItem)DataContext;

            if (dishItem.SelectedRecommends == null) dishItem.SelectedRecommends = new List<DishItem>();
            else dishItem.SelectedRecommends.Clear();

            foreach (DishItem item in listRecommends.SelectedItems)
            {
                dishItem.SelectedRecommends.Add(item);
            }
            updatePriceControl();
        }

        private void updatePriceControl()
        {
            BindingExpression be = txtDishPrice.GetBindingExpression(TextBlock.TextProperty);
            be.UpdateTarget();
        }

        private void addDishToOrder()
        {
            OrderItem curOrder = (OrderItem)AppLib.GetAppGlobalValue("currentOrder");
            DishItem curDish = (DishItem)DataContext;
            DishItem orderDish = curDish.GetCopyForOrder();
            curOrder.Dishes.Add(orderDish);
            // добавить в заказ рекомендации
            if ((curDish.SelectedRecommends != null) && (curDish.SelectedRecommends.Count > 0))
            {
                foreach (DishItem item in curDish.SelectedRecommends)
                {
                    curOrder.Dishes.Add(item);
                }
            }

            MainWindow mainWindow = (MainWindow)Application.Current.MainWindow;
            // снять выделение
            mainWindow.clearSelectedDish();
            // и обновить стоимость заказа
            mainWindow.updatePrice();

            closeWin();
        }

    }
}
