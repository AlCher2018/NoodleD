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
using System.ComponentModel;
using System.Windows.Media.Animation;

namespace WpfClient
{
    /// <summary>
    /// Interaction logic for DishPopup.xaml
    /// </summary>
    public partial class DishPopup : Window
    {
        private bool _dialogResult;
        private DishItem _currentDish;
        List<TextBlock> _tbList;
        List<Viewbox> _vbList;
        SolidColorBrush _notSelTextColor;
        SolidColorBrush _selTextColor;
        // анимация выбора блюда
        Storyboard _animDishSelection;


        public DishPopup(DishItem dishItem)
        {
            InitializeComponent();

            _currentDish = dishItem;
            this.DataContext = _currentDish;
            updatePriceControl();

            _notSelTextColor = new SolidColorBrush(Colors.Black);
            _selTextColor = (SolidColorBrush)AppLib.GetAppGlobalValue("addButtonBackgroundTextColor");
        }

        private void _animDishSelection_Completed(object sender, EventArgs e)
        {
            WpfClient.MainWindow mm = (WpfClient.MainWindow)Application.Current.MainWindow;
            mm.animateOrderPrice();

            closeWin(true);
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

            // размеры изображения блюда
            Rectangle dishImage = (AppLib.FindVisualChildren<Rectangle>(gridMain)).FirstOrDefault(r => r.Name == "dishImage");
            if (dishImage == null) return;
            double dishImageHeight = dishImage.ActualHeight;
            double dishImageWidth = dishImage.ActualWidth;
            double dishImageCornerRadius = dishImage.RadiusX;
            // объект перемещения
            // размеры прямоугольника и углы закругления для изображения и описания блюда берем из разметки
            RectangleGeometry rGeom = (animImage.Data as RectangleGeometry);
            rGeom.Rect = new Rect(0, 0, dishImageWidth, dishImageHeight);
            rGeom.RadiusX = dishImageCornerRadius;
            rGeom.RadiusY = dishImageCornerRadius;
            Canvas.SetLeft(canvasDish, -dishImageWidth / 2d);
            Canvas.SetTop(canvasDish, -dishImageHeight / 2d);

            //   конечный элемент анимации выбора блюда, это Point3 в BezierSegment
            Border brdMakeOrder = ((WpfClient.MainWindow)Application.Current.MainWindow).brdMakeOrder;
            Point toBasePoint = brdMakeOrder.PointToScreen(new Point(0, 0));
            Size toSize = brdMakeOrder.RenderSize;
            Point endPoint = new Point(toBasePoint.X + toSize.Width / 2.0, toBasePoint.Y + toSize.Height / 2.0);
            // установить для сегмента анимации конечную точку
            PathFigure pf = (animPath.Data as PathGeometry).Figures[0];
            BezierSegment bs = (pf.Segments[0] as BezierSegment);
            bs.Point3 = endPoint;

            createDishSelectStoryBoard();
        }

        private void createDishSelectStoryBoard()
        {
            // раскадровка
            _animDishSelection = new Storyboard() { FillBehavior = FillBehavior.Stop, AccelerationRatio = 1, AutoReverse = false };
            _animDishSelection.Completed += _animDishSelection_Completed;

            PathGeometry pGeom = animPath.Data as PathGeometry;
            // анимации перемещения
            DoubleAnimationUsingPath aMoveX = new DoubleAnimationUsingPath()
            {
                Source = PathAnimationSource.X,
                PathGeometry = pGeom
            };
            Storyboard.SetTarget(aMoveX, canvasDish);
            Storyboard.SetTargetProperty(aMoveX, new PropertyPath("RenderTransform.X"));
            _animDishSelection.Children.Add(aMoveX);
            DoubleAnimationUsingPath aMoveY = new DoubleAnimationUsingPath()
            {
                Source = PathAnimationSource.Y,
                PathGeometry = pGeom
            };
            Storyboard.SetTarget(aMoveY, canvasDish);
            Storyboard.SetTargetProperty(aMoveY, new PropertyPath("RenderTransform.Y"));
            _animDishSelection.Children.Add(aMoveY);
            // анимация вращения
            //DoubleAnimation aRotate = new DoubleAnimation() { From=0, To=360};
            //Storyboard.SetTarget(aRotate, animImage);
            //Storyboard.SetTargetProperty(aRotate, new PropertyPath("(Path.RenderTransform).(TransformGroup.Children)[0].Angle"));
            //_animDishSelection.Children.Add(aRotate);
            // анимация масштабирования
            DoubleAnimation aScaleX = new DoubleAnimation() { From = 1, To = 0.2 };
            Storyboard.SetTarget(aScaleX, animImage);
            Storyboard.SetTargetProperty(aScaleX, new PropertyPath("(Path.RenderTransform).(TransformGroup.Children)[1].ScaleX"));
            _animDishSelection.Children.Add(aScaleX);
            DoubleAnimation aScaleY = new DoubleAnimation() { From = 1, To = 0.2 };
            Storyboard.SetTarget(aScaleY, animImage);
            Storyboard.SetTargetProperty(aScaleY, new PropertyPath("(Path.RenderTransform).(TransformGroup.Children)[1].ScaleY"));
            _animDishSelection.Children.Add(aScaleY);
            // анимация прозрачности
            DoubleAnimation aOpacity = new DoubleAnimation() { From = 1, To = 0.2 };
            Storyboard.SetTarget(aOpacity, animImage);
            Storyboard.SetTargetProperty(aOpacity, new PropertyPath("(Path.Opacity)"));
            _animDishSelection.Children.Add(aOpacity);
        }


        #region все события закрытие всплывашки
        private void Window_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape) closeWin(false);
        }

        private void btnClose_PreviewMouseUp(object sender, MouseButtonEventArgs e)
        {
            closeWin(false, e);
        }

        // from XAML
        private void closeThisWindowHandler(object sender, MouseButtonEventArgs e)
        {
            closeWin(false, e);
        }


        // выбор блюда
        private void btnAddDish_MouseUp(object sender, MouseButtonEventArgs e)
        {
            // добавить блюдо в заказ
            OrderItem curOrder = (OrderItem)AppLib.GetAppGlobalValue("currentOrder");
            DishItem orderDish = _currentDish.GetCopyForOrder();   // сделать копию блюда со всеми добавками
            curOrder.Dishes.Add(orderDish);
            // добавить в заказ рекомендации
            if ((_currentDish.SelectedRecommends != null) && (_currentDish.SelectedRecommends.Count > 0))
            {
                foreach (DishItem item in _currentDish.SelectedRecommends)
                {
                    curOrder.Dishes.Add(item);
                }
            }
            // очистить добавки в текущем блюде
            _currentDish.ClearAllSelections();

            // закрыть окно после завершения анимации
            animateDishSelection();
        }

        private void closeWin(bool result, RoutedEventArgs e = null)
        {
            _dialogResult = result;

            // если просто закрываем окно, то удалить из блюда выбранные добавки и рекомендации
            if (_dialogResult == false)
            {
                _currentDish.ClearAllSelections();
            }

            if (e != null) e.Handled = true;
            this.Close();
        }
        protected override void OnClosing(CancelEventArgs e)
        {
            base.OnClosing(e);
            this.DialogResult = _dialogResult;
        }

        #endregion


        // анимировать перемещение блюда в тележку
        private void animateDishSelection()
        {
            // перемещаемое изображение
            (animImage.Fill as VisualBrush).Visual = dishImage;
            //animImage.Fill = Brushes.Green;  // debug

            // обновление пути анимации
            PathFigure pf = (animPath.Data as PathGeometry).Figures[0];
            BezierSegment bezierSeg = (pf.Segments[0] as BezierSegment);
            // получить точку начала анимации: центр панели блюда
            Point fromPoint = dishImage.PointToScreen(new Point(dishImage.ActualWidth / 2d, dishImage.ActualHeight / 2d));
            Point toPoint = bezierSeg.Point3;
            pf.StartPoint = fromPoint;
            // и опорные точки кривой Безье
            double dX = fromPoint.X - toPoint.X;
            double dY = toPoint.Y - fromPoint.Y;
            Point p1 = new Point(fromPoint.X - 0.3 * dX, 0.3 * fromPoint.Y);
            Point p2 = new Point(toPoint.X + 0.05 * dX, toPoint.Y - 0.8 * dY);
            bezierSeg.Point1 = p1;
            bezierSeg.Point2 = p2;

            canvasAnim.Visibility = Visibility.Visible;

            // установить скорость анимации
            double animSpeed = double.Parse(AppLib.GetAppSetting("SelectDishAnimationSpeed"));  // in msec
            TimeSpan ts = TimeSpan.FromMilliseconds(animSpeed);
            foreach (Timeline item in _animDishSelection.Children)
            {
                item.Duration = ts;
            }
            // обновление стоимости заказа в анимациях
            _animDishSelection.Begin();

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

    }  // class
}
