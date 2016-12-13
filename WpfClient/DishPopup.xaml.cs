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
        public DishPopup()
        {
            InitializeComponent();
        }

        private void Window_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape) this.Close();
        }

        #region закрытие всплывашки

        private void btnClose_PreviewMouseUp(object sender, MouseButtonEventArgs e)
        {
            closeWin(e);
        }
        private void btnClose_PreviewTouchUp(object sender, TouchEventArgs e)
        {
            closeWin(e);
        }

        private void gridWindow_MouseUp(object sender, MouseButtonEventArgs e)
        {
            closeWin(e);
        }
        private void gridWindow_TouchUp(object sender, TouchEventArgs e)
        {
            closeWin(e);
        }
        #endregion

        private void closeWin(RoutedEventArgs e = null)
        {
            if (e != null) e.Handled = true;
            this.Close();
        }

        private void btnAddDish_MouseUp(object sender, MouseButtonEventArgs e)
        {
            addDishToOrder();
        }
        private void btnAddDish_PreviewTouchUp(object sender, TouchEventArgs e)
        {
            addDishToOrder();
        }

        private void borderMain_MouseUp(object sender, MouseButtonEventArgs e)
        {
            e.Handled = true;
        }

        private void borderMain_TouchUp(object sender, TouchEventArgs e)
        {
            e.Handled = true;
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
            MessageBox.Show("add to Order");
        }
    }
}
