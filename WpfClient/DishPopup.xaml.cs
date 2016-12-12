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


        private void closeWin(RoutedEventArgs e = null)
        {
            if (e != null) e.Handled = true;
            this.Close();
        }

        private void btnAddDish_MouseUp(object sender, MouseButtonEventArgs e)
        {
            MessageBox.Show("добавление блюда в корзину");
        }

        private void borderMain_MouseUp(object sender, MouseButtonEventArgs e)
        {
            e.Handled = true;
        }

        private void borderMain_TouchUp(object sender, TouchEventArgs e)
        {
            e.Handled = true;
        }

        private void qqq(object sender, MouseButtonEventArgs e)
        {
            Debug.Print("source {0}: {1}", e.Source.ToString(), e.RoutedEvent.Name);
        }

        private void listIngredients_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

        }
    }
}
