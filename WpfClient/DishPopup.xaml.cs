using AppModel;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
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
        private DishItem _currentDish;
        List<TextBlock> _tbList;
        List<Viewbox> _vbList;
        SolidColorBrush _notSelTextColor;
        SolidColorBrush _selTextColor;


        public DishPopup(DishItem dishItem)
        {
            InitializeComponent();

            _isClose = true;
            _currentDish = dishItem;
            this.DataContext = _currentDish;
            updatePriceControl();

            _notSelTextColor = new SolidColorBrush(Colors.Black);
            _selTextColor = (SolidColorBrush)AppLib.GetAppGlobalValue("addButtonBackgroundTextColor");
        }

        // после загрузки окна
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            // обновить ListBox-ы, если есть выбранные ингредиенты и рекомендации
            if (_currentDish.SelectedIngredients != null)
            {
                foreach (DishAdding item in _currentDish.SelectedIngredients.ToList())
                {
                    listIngredients.SelectedItems.Add(item);
                }
            }
            if (_currentDish.SelectedRecommends != null)
            {
                foreach (DishItem item in _currentDish.SelectedRecommends.ToList())
                {
                    listRecommends.SelectedItems.Add(item);
                }
            }
        }

        private void Window_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape) this.Close();
        }

        #region закрытие всплывашки
//    On Touch Devices:
//TouchDown > PreviewMouseDown > TouchUp > PreviewMouseUp
//    On Non Touch:
//PreviewMouseDown > PreviewMouseUp
        private void btnClose_PreviewMouseUp(object sender, MouseButtonEventArgs e)
        {
            closeWin(e);
        }

        private void closeThisWindowHandler(object sender, MouseButtonEventArgs e)
        {
            closeWin();
        }

        private void closeWin(RoutedEventArgs e = null)
        {
            if (e != null) e.Handled = true;
            this.Close();
        }
        #endregion


        private void btnAddDish_MouseUp(object sender, MouseButtonEventArgs e)
        {
            addDishToOrder();
            e.Handled = true;
        }


        private void listIngredients_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            // view level
            changeViewAdding(listIngredients, e);

            // model level
            if (_currentDish.SelectedIngredients == null) _currentDish.SelectedIngredients = new List<DishAdding>();
            else _currentDish.SelectedIngredients.Clear();

            foreach (DishAdding item in listIngredients.SelectedItems)
            {
                _currentDish.SelectedIngredients.Add(item);
            }
            updatePriceControl();
        }

        private void changeViewAdding(ListBox listBox, SelectionChangedEventArgs e)
        {
            TextBlock tbText;
            Viewbox vBox;

            foreach (var item in e.RemovedItems)
            {
                ListBoxItem vRemoved = (ListBoxItem)listBox.ItemContainerGenerator.ContainerFromItem(item);
                _tbList = AppLib.FindVisualChildren<TextBlock>(vRemoved).ToList();
                _vbList = AppLib.FindVisualChildren<Viewbox>(vRemoved).ToList();
                tbText = _tbList.Find(t => t.Name == "txtName");
                tbText.Foreground = _notSelTextColor;
                vBox = _vbList.Find(v => v.Name == "garnBaseColorBrush");
                vBox.Visibility = Visibility.Hidden;
            }
            foreach (var item in e.AddedItems)
            {
                ListBoxItem vAdded = (ListBoxItem)listBox.ItemContainerGenerator.ContainerFromItem(item);
                _tbList = AppLib.FindVisualChildren<TextBlock>(vAdded).ToList();
                _vbList = AppLib.FindVisualChildren<Viewbox>(vAdded).ToList();
                tbText = _tbList.Find(t => t.Name == "txtName");
                tbText.Foreground = _selTextColor;
                vBox = _vbList.Find(v => v.Name == "garnBaseColorBrush");
                vBox.Visibility = Visibility.Visible;
            }
        }

        private void listRecommends_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            // view level
            changeViewAdding(listRecommends, e);

            // model level
            if (_currentDish.SelectedRecommends == null) _currentDish.SelectedRecommends = new List<DishItem>();
            else _currentDish.SelectedRecommends.Clear();

            foreach (DishItem item in listRecommends.SelectedItems)
            {
                _currentDish.SelectedRecommends.Add(item);
            }
            updatePriceControl();
        }

        private void updatePriceControl()
        {
            decimal dishValue = _currentDish.GetPrice();  // цена блюда (самого или с гарниром) плюс ингредиенты
            // в данном объекте добавить еще и рекомендации
            if (_currentDish.SelectedRecommends != null)
                foreach (DishItem item in _currentDish.SelectedRecommends) dishValue += item.Price;
            
            txtDishPrice.Text = AppLib.GetCostUIText(dishValue);
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
            // и обновить стоимость заказа
            mainWindow.updatePrice();

            closeWin();
        }

    }
}
