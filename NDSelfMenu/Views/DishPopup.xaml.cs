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
using AppActionNS;
using UserActionLog;

namespace WpfClient.Views
{
    /// <summary>
    /// Interaction logic for DishPopup.xaml
    /// </summary>
    public partial class DishPopup : Window
    {
        private DishItem _currentDish;
        List<TextBlock> _tbList;
        List<Viewbox> _vbList;
        SolidColorBrush _notSelTextColor;
        SolidColorBrush _selTextColor;
        // анимация выбора блюда
        Storyboard _animDishSelection;

        private UserActionsLog _eventsLog;

        // причина закрытия окна
        string _closeCause;

        public DishPopup(DishItem dishItem, BitmapImage img)
        {
            InitializeComponent();

            AppLib.WriteLogTraceMessage(string.Format("Открывается всплывашка для \"{0}\" ...", dishItem.langNames["ru"]));

            this.Loaded += DishPopup_Loaded;

            // init private vars
            _notSelTextColor = new SolidColorBrush(Colors.Black);
            _selTextColor = (SolidColorBrush)AppLib.GetAppGlobalValue("addButtonBackgroundTextColor");

            setWinLayout();

            // set Win data
            _currentDish = dishItem;
            this.DataContext = _currentDish;

            dishImage.Fill = new ImageBrush(img);

            if (AppLib.GetAppSetting("IsWriteWindowEvents").ToBool())
            {
                _eventsLog = new UserActionsLog(this, EventsMouseEnum.Bubble, EventsKeyboardEnum.None, EventsTouchEnum.Bubble, UserActionLog.LogFilesPathLocationEnum.App_Logs, true, false);
            }

            updatePriceControl();
        }

        // после загрузки окна
        private void DishPopup_Loaded(object sender, RoutedEventArgs e)
        {
            AppLib.WriteAppAction(this.Name, AppActionsEnum.DishPopupOpen, _currentDish.langNames["ru"]);

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
            Border brdMakeOrder = ((MainWindow)Application.Current.MainWindow).brdMakeOrder;
            Point toBasePoint = brdMakeOrder.PointToScreen(new Point(0, 0));
            Size toSize = brdMakeOrder.RenderSize;
            Point endPoint = new Point(toBasePoint.X + toSize.Width / 2.0, toBasePoint.Y + toSize.Height / 2.0);
            if (AppLib.ScreenScale != 1d) endPoint = PointFromScreen(endPoint);
            // установить для сегмента анимации конечную точку
            PathFigure pf = (animPath.Data as PathGeometry).Figures[0];
            BezierSegment bs = (pf.Segments[0] as BezierSegment);
            bs.Point3 = endPoint;

            createDishSelectStoryBoard();
        }

        protected override void OnClosing(CancelEventArgs e)
        {
            AppLib.WriteLogTraceMessage("Закрывается всплывашка...");
            AppLib.WriteAppAction(this.Name, AppActionsEnum.DishPopupClose, _closeCause);

            _currentDish.ClearAllSelections();
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

        private void setWinLayout()
        {
            // размеры
            this.Width = (double)AppLib.GetAppGlobalValue("screenWidth");
            this.Height = (double)AppLib.GetAppGlobalValue("screenHeight");
            this.Top = 0; this.Left = 0;

            double pnlMenuWidth, pnlMenuHeight;
            //pnlMenuHeight = (double)AppLib.GetAppGlobalValue("categoriesPanelHeight");
            //pnlMenuWidth = (double)AppLib.GetAppGlobalValue("categoriesPanelWidth");
            // высоту и ширину панели управления взять из главного окна
            MainWindow mainWin = (MainWindow)App.Current.MainWindow;
            pnlMenuHeight = mainWin.gridMenuSide.ActualHeight;
            pnlMenuWidth = mainWin.gridMenuSide.ActualWidth;
            brdAboveFolderMenu.Height = pnlMenuHeight;
            brdAboveFolderMenu.Width = pnlMenuWidth;

            double dW, dH, d1;
            double grdContentWidth = getAbsColWidth(gridWindow, 1, ((this.Width <= pnlMenuWidth) ? this.Width : this.Width - pnlMenuWidth));
            double grdContentHeight = AppLib.GetRowHeightAbsValue(gridWindow, 1, ((this.Height <= pnlMenuHeight) ? this.Height : this.Height - pnlMenuHeight));
            double itemWidth = 10, itemHeight=10, garnTextFontSize=10;

            Setter st; Thickness thMargin;
            Style lbiStyle = (Style)this.Resources["addingsListBoxItemStyle"];
            Style garnTextStyle = (Style)this.Resources["garnishTextStyle"];
            
            // вертикальное размещение
            if (AppLib.IsAppVerticalLayout == true)
            {
                DockPanel.SetDock(brdAboveFolderMenu, Dock.Top);

                gridWindow.RowDefinitions[1].Height = new GridLength(5d, GridUnitType.Star);
                gridWindow.ColumnDefinitions[1].Width = new GridLength(10d, GridUnitType.Star);

                // строка изображения
                gridMain.RowDefinitions[2].Height = new GridLength(2.5, GridUnitType.Star);
                // строки добавок
                gridMain.RowDefinitions[5].Height = new GridLength(1.2, GridUnitType.Star);
                gridMain.RowDefinitions[8].Height = new GridLength(1.2, GridUnitType.Star);
                // Кнопка Добавить
                gridAddButtonSection.RowDefinitions[1].Height = new GridLength(2.0, GridUnitType.Star);

                grdContentWidth = getAbsColWidth(gridWindow, 1, ((this.Width == pnlMenuWidth) ? this.Width : this.Width - pnlMenuWidth));
                grdContentHeight = AppLib.GetRowHeightAbsValue(gridWindow, 1, ((this.Height == pnlMenuHeight) ? this.Height : this.Height - pnlMenuHeight));

                // ширина изображения
                dW = getAbsColWidth(gridMain, 1, grdContentWidth);
                dishImage.Width = 0.4d * dW;
                // текст добавки
                dH = AppLib.GetRowHeightAbsValue(gridMain, 5, grdContentHeight);
                garnTextFontSize = (0.4d * dH) * 0.3d;
            }

            // горизонтальное размещение
            else
            {
                DockPanel.SetDock(brdAboveFolderMenu, Dock.Left);

                // ширина изображения
                dW = getAbsColWidth(gridMain, 1, grdContentWidth);
                dishImage.Width = 0.35d * dW;

                dH = AppLib.GetRowHeightAbsValue(gridMain, 5, grdContentHeight);
                garnTextFontSize = (0.4d * dH) * 0.3d;
            }

            // ширина элемента списка добавок
            dW = getAbsColWidth(gridMain, 1, grdContentWidth); // + getAbsColWidth(gridMain, 2, grdContentWidth);
            dW /= 5d;
            itemWidth = 0.9d * dW;
            thMargin = new Thickness(0, 0, 0.1 * dW, 0);

            st = (Setter)lbiStyle.Setters.FirstOrDefault(s => (s as Setter).Property.Name == "Margin");
            st.Value = thMargin;

            st = (Setter)lbiStyle.Setters.FirstOrDefault(s => (s as Setter).Property.Name == "Width");
            st.Value = itemWidth;

            st = (Setter)garnTextStyle.Setters.FirstOrDefault(s => (s as Setter).Property.Name == "FontSize");
            st.Value = garnTextFontSize;
            if (AppLib.IsAppVerticalLayout)
            {
                st = (Setter)garnTextStyle.Setters.FirstOrDefault(s => (s as Setter).Property.Name == "Margin");
                thMargin = new Thickness(- 0.4* 0.1 * dW, 0, -0.4 * 0.1 * dW, 0);
                st.Value = thMargin;
            }
        }  // method

        private double getAbsColWidth(Grid grid, int iCol, double totalWidth)
        {
            double cntStars = grid.ColumnDefinitions.Sum(c => c.Width.Value);
            return grid.ColumnDefinitions[iCol].Width.Value / cntStars * totalWidth;
        }

        private void _animDishSelection_Completed(object sender, EventArgs e)
        {
            updatePriceAndClose(false);
        }

        private void updatePriceAndClose(bool isAnimate)
        {
            //AppLib.WriteLogTraceMessage("Выбор Вока: обновление стоимости заказа в главном окне");
            MainWindow mm = (MainWindow)Application.Current.MainWindow;
            if (isAnimate == true)
                mm.animateOrderPrice();
            else
                mm.updatePrice();

            //AppLib.WriteLogTraceMessage("Выбор Вока: закрытие всплывашки");
            closeWin();
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
            if (e.Key == Key.Escape) closeWin();
        }

        private void btnClose_PreviewMouseUp(object sender, MouseButtonEventArgs e)
        {
            _closeCause = "ButtonCloseWin";
            closeWin(e);
        }

        // from XAML
        private void closeThisWindowHandler(object sender, MouseButtonEventArgs e)
        {
            _closeCause = "ClickAround";
            closeWin(e);
        }


        // выбор блюда
        // ****** ??? процедура вызывается 2 раза!!!
        // если это MouseUp, если PreviewMouseUp - то один раз!!
        private void btnAddDish_MouseUp(object sender, MouseButtonEventArgs e)
        {
            _closeCause = "ButtonAddDish";
            AppLib.WriteAppAction(this.Name, AppActionsEnum.DishPopupAddButton);
            e.Handled = true;

            // добавить блюдо в заказ
            OrderItem curOrder = AppLib.GetCurrentOrder();
            DishItem orderDish = _currentDish.GetCopyForOrder();   // сделать копию блюда со всеми добавками

            // изображение взять из элемента
            ImageBrush imgBrush = (ImageBrush)dishImage.Fill;
            BitmapImage bmpImage = (imgBrush.ImageSource as BitmapImage);
            orderDish.Image = bmpImage;

            curOrder.Dishes.Add(orderDish);

            //Debug.Print("order.Dishes.Count = " + curOrder.Dishes.Count.ToString());

            // добавить в заказ рекомендации
            if ((_currentDish.SelectedRecommends != null) && (_currentDish.SelectedRecommends.Count > 0))
            {
                foreach (DishItem item in _currentDish.SelectedRecommends)
                {
                    curOrder.Dishes.Add(item);
                }
            }

            if ((bool)AppLib.GetAppGlobalValue("isAnimatedSelectVoki"))
            {
                // закрыть окно после завершения анимации
                animateDishSelection();
            }
            else
            {
                updatePriceAndClose(false);
            }

        }

        private void closeWin(RoutedEventArgs e = null)
        {
            if (e != null) e.Handled = true;
            this.Close();
        }

        #endregion


        // анимировать перемещение блюда в тележку
        private void animateDishSelection()
        {
//            AppLib.WriteLogTraceMessage("Выбор Вока: вычисление геометрии анимации");
            // перемещаемое изображение
            (animImage.Fill as VisualBrush).Visual = dishImage;
            //animImage.Fill = Brushes.Green;  // debug

            // обновление пути анимации
            PathFigure pf = (animPath.Data as PathGeometry).Figures[0];
            BezierSegment bezierSeg = (pf.Segments[0] as BezierSegment);
            // получить точку начала анимации: центр панели блюда
            Point fromPoint = dishImage.PointToScreen(new Point(dishImage.ActualWidth / 2d, dishImage.ActualHeight / 2d));
            if (AppLib.ScreenScale != 1d) fromPoint = PointFromScreen(fromPoint);
            Point toPoint = bezierSeg.Point3;
            pf.StartPoint = fromPoint;
            // и опорные точки кривой Безье
            double dX, dY; Point p1, p2;
            if (AppLib.IsAppVerticalLayout)
            {
                dX = fromPoint.X - toPoint.X;
                dY = fromPoint.Y - toPoint.Y;
                // блюдо справа
                if (dX > 0)
                {
                    p1 = new Point(0.5 * toPoint.X, 1.3 * fromPoint.Y);
                    p2 = new Point(0.8 * toPoint.X, 0.5 * fromPoint.Y);
                }
                // блюдо слева
                else
                {
                    p1 = new Point(1.2 * toPoint.X, 1.3 * fromPoint.Y);
                    p2 = new Point(1.2 * toPoint.X, 0.5 * fromPoint.Y);
                }
            }
            else
            {
                dX = fromPoint.X - toPoint.X;
                dY = toPoint.Y - fromPoint.Y;
                p1 = new Point(fromPoint.X - 0.3 * dX, 0.3 * fromPoint.Y);
                p2 = new Point(toPoint.X + 0.05 * dX, toPoint.Y - 0.8 * dY);
            }
            bezierSeg.Point1 = p1;
            bezierSeg.Point2 = p2;

//            AppLib.WriteLogTraceMessage("Выбор Вока: сделать видимой панель анимации");
            canvasAnim.Visibility = Visibility.Visible;

            // установить скорость анимации
//            AppLib.WriteLogTraceMessage("Выбор Вока: установить скорость анимации");
            double animSpeed = double.Parse(AppLib.GetAppSetting("SelectDishAnimationSpeed"));  // in msec
            TimeSpan ts = TimeSpan.FromMilliseconds(animSpeed);
            foreach (Timeline item in _animDishSelection.Children)
            {
                item.Duration = ts;
            }
            // обновление стоимости заказа в анимациях
//            AppLib.WriteLogTraceMessage("Выбор Вока: старт анимации");
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

        private void changeViewAdding(ListBox listBox, SelectionChangedEventArgs e)
        {
            TextBlock tbText;
            Viewbox vBox;

            // снять выделение
            foreach (var item in e.RemovedItems)
            {
                ListBoxItem vRemoved = (ListBoxItem)listBox.ItemContainerGenerator.ContainerFromItem(item);
                _tbList = AppLib.FindVisualChildren<TextBlock>(vRemoved).ToList();
                _vbList = AppLib.FindVisualChildren<Viewbox>(vRemoved).ToList();
                tbText = _tbList.Find(t => t.Name == "txtName");
                tbText.Foreground = _notSelTextColor;
                vBox = _vbList.Find(v => v.Name == "garnBaseColorBrush");
                vBox.Visibility = Visibility.Hidden;

                // записать в лог снятие выделения  добавки
                if (listBox.Equals(listIngredients))
                    AppLib.WriteAppAction(this.Name, AppActionsEnum.DishPopupIngrDeselect, (vRemoved.Content as DishAdding).langNames["ru"]);
                else
                    AppLib.WriteAppAction(this.Name, AppActionsEnum.DishPopupRecommendDeselect, (vRemoved.Content as DishItem).langNames["ru"]);
            }

            // выделение добавки
            foreach (var item in e.AddedItems)
            {
                ListBoxItem vAdded = (ListBoxItem)listBox.ItemContainerGenerator.ContainerFromItem(item);
                _tbList = AppLib.FindVisualChildren<TextBlock>(vAdded).ToList();
                _vbList = AppLib.FindVisualChildren<Viewbox>(vAdded).ToList();
                tbText = _tbList.Find(t => t.Name == "txtName");
                tbText.Foreground = _selTextColor;
                vBox = _vbList.Find(v => v.Name == "garnBaseColorBrush");
                vBox.Visibility = Visibility.Visible;

                // записать в лог выделение добавки
                if (listBox.Equals(listIngredients))
                    AppLib.WriteAppAction(this.Name, AppActionsEnum.DishPopupIngrSelect, (vAdded.Content as DishAdding).langNames["ru"]);
                else
                    AppLib.WriteAppAction(this.Name, AppActionsEnum.DishPopupRecommendSelect, (vAdded.Content as DishItem).langNames["ru"]);
            }
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
