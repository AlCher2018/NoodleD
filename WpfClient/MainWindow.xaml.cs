using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;

using AppModel;
using UserActionLog;
using System.Configuration;
using System.Data.SqlClient;
using System.Data;
using System.Diagnostics;
using System.Windows.Media.Animation;
using System.Windows.Media.Effects;
using System.Timers;

namespace WpfClient
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        // подложка списка блюд
        private List<Canvas> _dishCanvas;
        // текущий заказ
        private OrderItem _currentOrder;
        // visual elements
        private byte _langButtonPress = 0;
        private Border _curDescrBorder;
        private TextBlock _curDescrTextBlock;
        private SolidColorBrush _brushSelectedItem;

        // dish description animations
        private DoubleAnimation _daCommon1, _daCommon2;
        private DoubleAnimation _daDishDescrBackgroundOpacity;  // анимашка прозрачности описания блюда
        private Storyboard _animDishSelection;
        private ColorAnimation _animOrderPriceBackgroundColor;
        private Effect _orderPriceEffectShadow;
        private Effect _orderPriceEffectBlur;

        // dragging
        private Point? lastDragPoint, initDragPoint;
        private DateTime _dateTime;

        public List<Canvas> DishesPanels { get { return _dishCanvas; } }


        public MainWindow()
        {
            InitializeComponent();

            // для настройки элементов после отрисовки окна
            Loaded += MainWindow_Loaded;

            // инициализация локальных переменных
            _dishCanvas = new List<Canvas>();
            _daDishDescrBackgroundOpacity = new DoubleAnimation()
            {
                EasingFunction = new CubicEase() { EasingMode = EasingMode.EaseOut }
            };
            _daCommon1 = new DoubleAnimation();
            _daCommon2 = new DoubleAnimation();
            _brushSelectedItem = (SolidColorBrush)AppLib.GetAppGlobalValue("appSelectedItemColor");

            // создать текущий заказ
            _currentOrder = new OrderItem();
            AppLib.SetAppGlobalValue("currentOrder", _currentOrder);
            updatePrice();

            initUI();
        }

        protected override void OnClosed(EventArgs e)
        {
            base.OnClosed(e);
            AppLib.WriteLogInfoMessage("************  End application  ************");

            UserActionsWPF actionIdle = (UserActionsWPF)AppLib.GetAppGlobalValue("actionIdle");
            if (actionIdle != null)
            {
                actionIdle.FinishLoggingUserActions();
            }
        }

        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            //   конечный элемент анимации выбора блюда, это Point3 в BezierSegment
            Point toBasePoint = brdMakeOrder.PointToScreen(new Point(0, 0));
            Size toSize = brdMakeOrder.RenderSize;
            Point endPoint = new Point(toBasePoint.X + toSize.Width / 2.0, toBasePoint.Y + toSize.Height / 2.0);
            // установить для сегмента анимации конечную точку
            PathFigure pf = (animPath.Data as PathGeometry).Figures[0];
            BezierSegment bs = (pf.Segments[0] as BezierSegment);
            bs.Point3 = endPoint;

            string sBuf = AppLib.GetAppSetting("UserIdleTime");
            if ((sBuf != null) && (sBuf != "0"))
            {
                int idleSec = int.Parse(sBuf);
                UserActionsWPF actionIdle = (UserActionsWPF)AppLib.GetAppGlobalValue("actionIdle");
                actionIdle.AnyActionWindow = this;
                actionIdle.IdleSeconds = idleSec;
            }
        }

        private void initUI()
        {
            AppLib.WriteLogTraceMessage("Настраиваю визуальные элементы...");
            AppLib.AppLang = AppLib.GetAppSetting("langDefault");

            // надписи на языковых кнопках
            lblLangUa.Text = (string)AppLib.GetAppGlobalValue("langButtonTextUa");
            lblLangRu.Text = (string)AppLib.GetAppGlobalValue("langButtonTextRu");
            lblLangEng.Text = (string)AppLib.GetAppGlobalValue("langButtonTextEn");

            // большие кнопки скроллинга
            var v = Enum.Parse(typeof(HorizontalAlignment), (string)AppLib.GetAppGlobalValue("dishesPanelScrollButtonHorizontalAlignment"));
            btnScrollDown.Width = (double)AppLib.GetAppGlobalValue("dishesPanelScrollButtonSize");
            btnScrollDown.Height = (double)AppLib.GetAppGlobalValue("dishesPanelScrollButtonSize");
            btnScrollDown.HorizontalAlignment = (HorizontalAlignment)v;
            btnScrollUp.Width = (double)AppLib.GetAppGlobalValue("dishesPanelScrollButtonSize");
            btnScrollUp.Height = (double)AppLib.GetAppGlobalValue("dishesPanelScrollButtonSize");
            btnScrollUp.HorizontalAlignment = (HorizontalAlignment)v;

            List<AppModel.MenuItem> mFolders = (List<AppModel.MenuItem>)AppLib.GetAppGlobalValue("mainMenu");

            // создать список категорий блюд
            AppLib.WriteLogTraceMessage(" - создаю список категорий блюд...");
            createCategoriesList();
            AppLib.WriteLogTraceMessage(" - создаю список категорий блюд... READY");

            AppLib.WriteLogTraceMessage(" - создаю списки блюд по категориям...");
            // создать канву со списком блюд
            createDishesCanvas(mFolders);
            AppLib.WriteLogTraceMessage(" - создаю списки блюд по категориям... READY");

            lstMenuFolders.Focus();
            lstMenuFolders.ItemsSource = mFolders;
            lstMenuFolders.SelectedIndex = 0;

            // установить язык UI
            selectAppLang(AppLib.AppLang);

            // анимация выбора блюда
            AppLib.WriteLogTraceMessage(" - создаю Объекты анимации выбора блюда...");
            createObjectsForDishAnimation();
            AppLib.WriteLogTraceMessage(" - создаю Объекты анимации выбора блюда... READY");

            // выключить курсор мыши
            if (AppLib.GetAppSetting("MouseCursor").IsTrueString() == false)
            {
                this.Cursor = Cursors.None;
                Mouse.OverrideCursor = Cursors.None;
            }

            AppLib.WriteLogTraceMessage("Настраиваю визуальные элементы - READY");
        }

        private void createObjectsForDishAnimation()
        {
            // объект перемещения
            // размеры прямоугольника и углы закругления для изображения и описания блюда берем из свойств приложения
            double dishImageHeight = (double)AppLib.GetAppGlobalValue("dishImageHeight");
            double dishImageWidth = (double)AppLib.GetAppGlobalValue("dishImageWidth");
            double dishImageCornerRadius = (double)AppLib.GetAppGlobalValue("cornerRadiusDishPanel");
            RectangleGeometry r = (animImage.Data as RectangleGeometry);
            r.Rect = new Rect(0, 0, dishImageWidth, dishImageHeight);
            r.RadiusX = dishImageCornerRadius;
            r.RadiusY = dishImageCornerRadius;
            Canvas.SetLeft(canvasDish, -dishImageWidth / 2d);
            Canvas.SetTop(canvasDish, -dishImageHeight / 2d);

            // раскадровка
            _animDishSelection = new Storyboard() { FillBehavior= FillBehavior.Stop, AccelerationRatio=1, AutoReverse=false};
            _animDishSelection.Completed += _aminDishSelection_Completed;

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
            DoubleAnimation aScaleX = new DoubleAnimation() { From=1, To = 0.2};
            Storyboard.SetTarget(aScaleX, animImage);
            Storyboard.SetTargetProperty(aScaleX, new PropertyPath("(Path.RenderTransform).(TransformGroup.Children)[1].ScaleX"));
            _animDishSelection.Children.Add(aScaleX);
            DoubleAnimation aScaleY = new DoubleAnimation() { From = 1, To = 0.2 };
            Storyboard.SetTarget(aScaleY, animImage);
            Storyboard.SetTargetProperty(aScaleY, new PropertyPath("(Path.RenderTransform).(TransformGroup.Children)[1].ScaleY"));
            _animDishSelection.Children.Add(aScaleY);
            // анимация прозрачности
            DoubleAnimation aOpacity = new DoubleAnimation() { From=1, To = 0.2};
            Storyboard.SetTarget(aOpacity, animImage);
            Storyboard.SetTargetProperty(aOpacity, new PropertyPath("(Path.Opacity)"));
            _animDishSelection.Children.Add(aOpacity);

            // для анимации фона с ценой заказа
            _animOrderPriceBackgroundColor = new ColorAnimation(Colors.Magenta, TimeSpan.FromMilliseconds(50), FillBehavior.Stop) { AutoReverse = true, RepeatBehavior = new RepeatBehavior(5)};
            _orderPriceEffectShadow = new DropShadowEffect() { Direction=315, Color = Colors.DarkGreen, ShadowDepth=5, BlurRadius=10 };
            _orderPriceEffectBlur = new BlurEffect() { Radius = 0}; 
            txtOrderPrice.Effect = _orderPriceEffectShadow;
            Color c = ((SolidColorBrush)Application.Current.Resources["cartButtonBackgroundColor"]).Color;
            brdMakeOrder.Background = new SolidColorBrush(c); //Do not use a frozen instance  (Colors.Orange)
        }

        private void _aminDishSelection_Completed(object sender, EventArgs e)
        {
            canvasAnim.Visibility = Visibility.Hidden;

            animateOrderPrice();
        }

        public void animateOrderPrice()
        {
            // анимация фона
            if (_currentOrder.GetOrderValue() == 0)
            {
                brdMakeOrder.Background.BeginAnimation(SolidColorBrush.ColorProperty, _animOrderPriceBackgroundColor);
            }
            // анимация цены
            else
            {
                //   размер шрифта
                _daCommon1.Duration = TimeSpan.FromMilliseconds(400);
                _daCommon1.To = 1.5 * txtOrderPrice.FontSize;
                txtOrderPrice.BeginAnimation(TextBlock.FontSizeProperty, _daCommon1);
                //   расплывчатость текста
                txtOrderPrice.Effect = _orderPriceEffectBlur;
                _daCommon2.Duration = TimeSpan.FromMilliseconds(400);
                _daCommon2.To = 80;
                _daCommon2.Completed += _daCommon1_Completed;
                txtOrderPrice.Effect.BeginAnimation(BlurEffect.RadiusProperty, _daCommon2);
            }
        }

        private void _daCommon1_Completed(object sender, EventArgs e)
        {
            updatePrice();

            _daCommon1.To = (double)AppLib.GetAppGlobalValue("appFontSize1");
            txtOrderPrice.BeginAnimation(TextBlock.FontSizeProperty, _daCommon1);

            _daCommon2.To = 0;
            _daCommon2.Completed -= _daCommon1_Completed;
            _daCommon2.Completed += _daCommon2_Completed;
            txtOrderPrice.Effect.BeginAnimation(BlurEffect.RadiusProperty, _daCommon2);
        }

        private void _daCommon2_Completed(object sender, EventArgs e)
        {
            _daCommon2.Completed -= _daCommon2_Completed;
            txtOrderPrice.Effect = _orderPriceEffectShadow;
        }

        private void createCategoriesList()
        {
            // стиль содержания пункта меню
            bool isScrollingList = AppLib.GetAppSetting("IsAllowScrollingDishCategories").IsTrueString();
            if (isScrollingList == true)
            {

            }
            // без скроллинга, поэтому настраиваем поля и отступы
            // если категорий меньше 6, то оставляем по умолчанию, из разметки
            else
            {
                List<AppModel.MenuItem> mFolders = (List<AppModel.MenuItem>)AppLib.GetAppGlobalValue("mainMenu");
                int iCatCount = mFolders.Count;
                if (iCatCount > 6)
                {
                    Style brdStyle = (Style)this.Resources["menuItemStyle"];
                    Style imgStyle = (Style)this.Resources["menuItemImageStyle"];
                    Style txtStyle = (Style)this.Resources["menuItemTextStyle"];

                    double listWidth = (double)AppLib.GetAppGlobalValue("categoriesPanelWidth");
                    double listHeight = getHeightCatList();
                    listHeight *= (double)AppLib.GetAppGlobalValue("screenHeight");
                    double itemHeight = listHeight / iCatCount;
                    double marginBase = 0.09 * itemHeight;
                    double imageSize = 0.6 * itemHeight;
                    double txtFontSize = 0.3 * itemHeight;

                    // поле вокруг рамки
                    SetterBase sb = brdStyle.Setters.FirstOrDefault(s => (s as Setter).Property.Name == "Margin");
                    if (sb != null) (sb as Setter).Value = new Thickness(marginBase, 0, marginBase, 1.5 * marginBase);
                    // отступ внутри рамки
                    sb = brdStyle.Setters.FirstOrDefault(s => (s as Setter).Property.Name == "Padding");
                    if (sb != null) (sb as Setter).Value = new Thickness(marginBase, marginBase, marginBase, marginBase);
                    // размер изображения категории
                    sb = imgStyle.Setters.FirstOrDefault(s => (s as Setter).Property.Name == "Width");
                    if (sb != null) (sb as Setter).Value = imageSize;
                    sb = imgStyle.Setters.FirstOrDefault(s => (s as Setter).Property.Name == "Height");
                    if (sb != null) (sb as Setter).Value = imageSize;
                    // размер текста
                    sb = txtStyle.Setters.FirstOrDefault(s => (s as Setter).Property.Name == "FontSize");
                    if (sb != null) (sb as Setter).Value = txtFontSize;
                }  // if
            }  // else
        }  // method

        private double getHeightCatList()
        {
            double iStarsSum = 0d, iStarCatListValue = 0d;
            foreach (RowDefinition rowDef in gridMenuSide.RowDefinitions)
            {
                iStarsSum += rowDef.Height.Value;
                if (rowDef.Name == "rowCatList") iStarCatListValue = rowDef.Height.Value;
            }
            if (iStarCatListValue != 0) return (iStarCatListValue / iStarsSum);
            else return 0d;
        }


        #region работа со списком блюд
        //*********************************
        //     работа со списком блюд
        //*********************************
        private void createDishesCanvas(List<AppModel.MenuItem> mFolders)
        {
            #region privates vars
            double screenWidth, screenHeight;
            screenWidth = (double)AppLib.GetAppGlobalValue("screenWidth");
            screenHeight = (double)AppLib.GetAppGlobalValue("screenHeight");

            // углы закругления
            double cornerRadiusButton = (double)AppLib.GetAppGlobalValue("cornerRadiusButton");

            //  РАЗМЕРЫ ПАНЕЛИ БЛЮД(А)
            double dishesPanelWidth = (screenWidth / 6d * 5d);
            double dishPanelWidth = 0.95d * dishesPanelWidth / 3d;
            double dKoefContentWidth = 0.9d;
            double contentPanelWidth = dKoefContentWidth * dishPanelWidth;
            AppLib.SetAppGlobalValue("dishImageWidth", contentPanelWidth);
            // высота строки заголовка
            double dishPanelHeaderRowHeight = 0.17d * dishPanelWidth;
            // высота строки изображения
            double dishPanelImageRowHeight = 0.7d * dishPanelWidth;
            AppLib.SetAppGlobalValue("dishImageHeight", dishPanelImageRowHeight);
            // высота строки гарниров
            double dishPanelGarnishesRowHeight = 0.2d * dishPanelWidth;
            // высота строки кнопки добавления
            double dishPanelAddButtonRowHeight = 0.15d * dishPanelWidth;
            double dishPanelAddButtonTextSize = 0.3d * dishPanelAddButtonRowHeight;
            // расстояния между строками панели блюда
            double dishPanelRowMargin1 = 0.01d * dishPanelWidth;
            double dishPanelRowMargin2 = 0.02d * dishPanelWidth;
            // размер шрифтов
            double dishPanelHeaderFontSize =  Convert.ToDouble(AppLib.GetAppGlobalValue("dishPanelHeaderFontSize"));
            double dishPanelTextFontSize = Convert.ToDouble(AppLib.GetAppGlobalValue("dishPanelFontSize"));
            // размер кнопки описания блюда
            double dishPanelDescrButtonSize = 0.085d * dishPanelWidth;

            // высота панелей
            double dishPanelHeight, dishPanelHeightWithGarnish;
            dishPanelHeight = Math.Ceiling(dishPanelHeaderRowHeight + dishPanelRowMargin1 + dishPanelImageRowHeight + dishPanelRowMargin2 + dishPanelAddButtonRowHeight);
            dishPanelHeightWithGarnish = Math.Ceiling(dishPanelHeight + dishPanelGarnishesRowHeight + dishPanelRowMargin2);
            double dKoefContentHeight = 1d;
            double contentPanelHeight = Math.Ceiling(dKoefContentHeight * dishPanelHeight);
            double contentPanelHeightWithGarnish = Math.Ceiling(dKoefContentHeight * dishPanelHeightWithGarnish);

            #endregion

            Canvas canvas;
            double leftMargin = (dishesPanelWidth - 3 * dishPanelWidth) / 2;
            double currentPanelHeight, currentContentPanelHeight;
            double leftPos, topPos;
            double d1;
            //Random rnd = new Random();

            foreach (AppModel.MenuItem mItem in mFolders)
            {
                canvas = new Canvas();
                canvas.Width = dishesPanelWidth;

                int iRowsCount = 0; 
                if (mItem.Dishes.Count > 0) iRowsCount = ((mItem.Dishes.Count - 1) / 3) + 1;
                int iRow, iCol;
                for (int i=0; i < mItem.Dishes.Count; i++)
                {
                    DishItem dish = mItem.Dishes[i];
                    bool isExistGarnishes = (dish.Garnishes != null);
                    currentPanelHeight = ((isExistGarnishes) ? dishPanelHeightWithGarnish : dishPanelHeight);
                    currentContentPanelHeight = ((isExistGarnishes) ? contentPanelHeightWithGarnish : contentPanelHeight);
                    canvas.Height = iRowsCount * currentPanelHeight;

                    // положение панели блюда
                    iRow = i / 3; iCol = i % 3;
                    leftPos = (leftMargin + iCol * dishPanelWidth);
                    topPos = iRow * currentPanelHeight;

                    // декоратор для панели блюда (должен быть для корректной работы ручного скроллинга)
                    Grid brd = new Grid();
                    //brd.Background = new SolidColorBrush(Colors.LightCyan);
                    brd.SnapsToDevicePixels = true;
                    brd.Width = dishPanelWidth;
                    brd.Height = currentPanelHeight;
                    brd.SetValue(Canvas.LeftProperty, leftPos);
                    brd.SetValue(Canvas.TopProperty, topPos);

                    d1 = (dishPanelWidth - contentPanelWidth) / 2d;
                    brd.ColumnDefinitions.Add(new ColumnDefinition() {Width = new GridLength(d1, GridUnitType.Pixel) });
                    brd.ColumnDefinitions.Add(new ColumnDefinition() {Width = new GridLength(contentPanelWidth, GridUnitType.Pixel) });
                    brd.ColumnDefinitions.Add(new ColumnDefinition() {Width = new GridLength(d1, GridUnitType.Pixel) });
                    d1 = (currentPanelHeight - currentContentPanelHeight) / 2d;
                    brd.RowDefinitions.Add(new RowDefinition() { Height = new GridLength(d1, GridUnitType.Pixel) });
                    brd.RowDefinitions.Add(new RowDefinition() { Height = new GridLength(currentContentPanelHeight, GridUnitType.Pixel)});
                    brd.RowDefinitions.Add(new RowDefinition() { Height = new GridLength(d1, GridUnitType.Pixel) });

                    // панель содержания
                    Grid dGrid = new Grid();
                    dGrid.Width = contentPanelWidth;
                    dGrid.Height = currentContentPanelHeight;
                    //dGrid.Background = Brushes.Blue;
                    //   Определение строк
                    // 0. строка заголовка
                    dGrid.RowDefinitions.Add(new RowDefinition() { Height = new GridLength(dKoefContentHeight * dishPanelHeaderRowHeight, GridUnitType.Pixel) });
                    // 1. разделитель
                    dGrid.RowDefinitions.Add(new RowDefinition() { Height = new GridLength(dKoefContentHeight * dishPanelRowMargin1, GridUnitType.Pixel) });
                    // 2. строка изображения
                    dGrid.RowDefinitions.Add(new RowDefinition() { Height = new GridLength(dKoefContentHeight * dishPanelImageRowHeight, GridUnitType.Pixel) });
                    // 3. разделитель
                    dGrid.RowDefinitions.Add(new RowDefinition() { Height = new GridLength(dKoefContentHeight * dishPanelRowMargin2, GridUnitType.Pixel) });
                    if (isExistGarnishes)
                    {
                        // 4. строка гарниров
                        dGrid.RowDefinitions.Add(new RowDefinition() { Height = new GridLength(dKoefContentHeight * dishPanelGarnishesRowHeight, GridUnitType.Pixel) });
                        // 5. разделитель
                        dGrid.RowDefinitions.Add(new RowDefinition() { Height = new GridLength(dKoefContentHeight * dishPanelRowMargin2, GridUnitType.Pixel) });
                    }
                    // 6. строка кнопок
                    dGrid.RowDefinitions.Add(new RowDefinition() { Height = new GridLength(dKoefContentHeight * dishPanelAddButtonRowHeight, GridUnitType.Pixel) });

                    // **********************************
                    // Заголовок панели
                    setDishPanelHeader(dish, dGrid, dishPanelHeaderFontSize, dishPanelTextFontSize);

                    // изображение блюда и описание
                    setDishDescription(dish, dGrid, dishPanelDescrButtonSize, dishPanelTextFontSize, dishPanelHeaderFontSize);

                    // гарниры для Воков
                    if (isExistGarnishes == true)
                    {
                        double grnColWidth = Math.Ceiling(contentPanelWidth / 3d);
                        double grnH = dGrid.RowDefinitions[4].Height.Value, grnW = 1.3 * grnH; // пропорции кнопки

                        Grid grdGarnishes = new Grid();
                        grdGarnishes.ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(grnColWidth, GridUnitType.Pixel) });
                        grdGarnishes.ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(grnColWidth, GridUnitType.Pixel) });
                        grdGarnishes.ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(grnColWidth, GridUnitType.Pixel) });

                        MainMenuGarnish grdGarn = new MainMenuGarnish(dish, 0, grnH, grnW, dGrid);
                        grdGarn.HorizontalAlignment = HorizontalAlignment.Left;
                        grdGarn.SetValue(Grid.ColumnProperty, 0);
                        grdGarnishes.Children.Add(grdGarn);

                        if (dish.Garnishes.Count >= 2)
                        {
                            grdGarn = new MainMenuGarnish(dish, 1, grnH, grnW, dGrid);
                            grdGarn.HorizontalAlignment = HorizontalAlignment.Center;
                            grdGarn.SetValue(Grid.ColumnProperty, 1);
                            grdGarnishes.Children.Add(grdGarn);
                        }
                        if (dish.Garnishes.Count >= 3)
                        {
                            grdGarn = new MainMenuGarnish(dish, 2, grnH, grnW, dGrid);
                            grdGarn.HorizontalAlignment = HorizontalAlignment.Right;
                            grdGarn.SetValue(Grid.ColumnProperty, 2);
                            grdGarnishes.Children.Add(grdGarn);
                        }

                        Grid.SetRow(grdGarnishes, 4); dGrid.Children.Add(grdGarnishes);
                    }  // if

                    // изображения кнопок добавления
                    setDishAddButton(dish, dGrid, dishPanelAddButtonRowHeight, dKoefContentHeight, cornerRadiusButton, dishPanelTextFontSize);

                    brd.Children.Add(dGrid); Grid.SetRow(dGrid, 1); Grid.SetColumn(dGrid, 1);
                    canvas.Children.Add(brd);
                }  // for

                //canvas.Background = new SolidColorBrush(new Color() { R = (byte)rnd.Next(0,254), G= (byte)rnd.Next(0, 254), B= (byte)rnd.Next(0, 254), A=0xFF });
                _dishCanvas.Add(canvas);
            }
        }  // createDishesCanvas

        private void setDishPanelHeader(DishItem dish, Grid dGrid, double dishPanelHeaderFontSize, double dishPanelTextFontSize)
        {
            List<Inline> inlines = new List<Inline>();
            if (dish.Marks != null)
            {
                foreach (DishAdding markItem in dish.Marks)
                {
                    if (markItem.Image != null)
                    {
                        System.Windows.Controls.Image markImage = new System.Windows.Controls.Image();
                        //markImage.Effect = new DropShadowEffect() { Opacity = 0.7 };
                        markImage.Width = 1.5 * dishPanelHeaderFontSize;
                        //markImage.Height = 2*dishPanelHeaderFontSize;
                        markImage.Source = ImageHelper.ByteArrayToBitmapImage(markItem.Image);
                        markImage.Stretch = Stretch.Uniform;
                        InlineUIContainer iuc = new InlineUIContainer(markImage);
                        markImage.Margin = new Thickness(0, 0, 5, 5);
                        inlines.Add(iuc);
                    }
                }
            }
            inlines.Add(new Run()
            {
                Text = AppLib.GetLangText(dish.langNames),
                FontWeight = FontWeights.Bold,
                FontSize = dishPanelHeaderFontSize
            });

            if (dish.UnitCount != 0)
            {
                inlines.Add(new Run()
                {
                    Text = "  " + dish.UnitCount.ToString(),
                    FontStyle = FontStyles.Italic,
                    FontSize = dishPanelTextFontSize
                });
                inlines.Add(new Run()
                {
                    Text = " " + AppLib.GetLangText(dish.langUnitNames),
                    FontSize = dishPanelTextFontSize
                });
            }

            TextBlock tb = new TextBlock()
            {
                Foreground = new SolidColorBrush(Colors.Black),
                TextWrapping = TextWrapping.Wrap,
                VerticalAlignment = VerticalAlignment.Center,
                TextAlignment = TextAlignment.Center
            };
            tb.Inlines.AddRange(inlines);

            Grid.SetRow(tb, 0); dGrid.Children.Add(tb);
        }

        private void setDishDescription(DishItem dish, Grid dGrid, double dishPanelDescrButtonSize, double dishPanelTextFontSize, double dishPanelHeaderFontSize)
        {
            if (dish.Image == null) return;

            // размеры прямоугольника и углы закругления для изображения и описания блюда
            double dishImageHeight = (double)AppLib.GetAppGlobalValue("dishImageHeight");
            double dishImageWidth = (double)AppLib.GetAppGlobalValue("dishImageWidth");
            double dishImageCornerRadius = (double)AppLib.GetAppGlobalValue("cornerRadiusDishPanel");

            Rect rect = new Rect(0, 0, dGrid.Width, dishImageHeight);
            // изображение
            Path pathImage = new Path();
            pathImage.Data = new RectangleGeometry(rect, dishImageCornerRadius, dishImageCornerRadius);
            pathImage.Fill = new DrawingBrush(
                new ImageDrawing() { ImageSource = ImageHelper.ByteArrayToBitmapImage(dish.Image), Rect = rect }
                );
            //pathImage.Effect = new DropShadowEffect();
            // добавить в контейнер
            Grid.SetRow(pathImage, 2); dGrid.Children.Add(pathImage);

            // кнопка отображения описания
            Border btnDescr = new Border()
            {
                Name = "btnDescr",
                Width = dishPanelDescrButtonSize,
                Height = dishPanelDescrButtonSize,
                HorizontalAlignment = HorizontalAlignment.Right,
                VerticalAlignment = VerticalAlignment.Top,
                Tag = 0,
                Background = Brushes.White,
                CornerRadius = new CornerRadius(0.5 * dishPanelDescrButtonSize),
                Margin = new Thickness(0, 0.3 * dishPanelDescrButtonSize, 0.3 * dishPanelDescrButtonSize, 0)
            };
            btnDescr.PreviewMouseLeftButtonUp += CanvDescr_PreviewMouseLeftButtonUp;

            //   буковка i
            TextBlock btnDescrText = new TextBlock(new Run("i"))
            {
                FontSize = dishPanelHeaderFontSize,
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center
            };
            btnDescr.Child = btnDescrText;
            // добавить в контейнер
            Grid.SetRow(btnDescr, 2); dGrid.Children.Add(btnDescr);
            Grid.SetZIndex(btnDescr, 10);

            // описание блюда
            LinearGradientBrush lgBrush = new LinearGradientBrush()
            {
                StartPoint = new Point(0.5, 0),
                EndPoint = new Point(0.5, 1)
            };
            lgBrush.GradientStops.Add(new GradientStop(Colors.Black, 0));
            lgBrush.GradientStops.Add(new GradientStop(Colors.Black, 0.5));
            lgBrush.GradientStops.Add(new GradientStop((Color)AppLib.GetAppResource("appColorDarkPink"), 1));
            Border brdDescrText = new Border()
            {
                Name = "descrTextBorder",
                Width = dGrid.Width,
                Height = dishImageHeight,
                CornerRadius = new CornerRadius(dishImageCornerRadius),
                Background = lgBrush,
                Opacity = 0,
                Visibility = Visibility.Hidden
            };
            brdDescrText.PreviewMouseLeftButtonUp += CanvDescr_PreviewMouseLeftButtonUp;
            // добавить в контейнер
            Grid.SetRow(brdDescrText, 2); dGrid.Children.Add(brdDescrText);
            TextBlock tbDescrText = new TextBlock()
            {
                Name = "descrText",
                Width = dGrid.Width,
                Opacity = 0,
                Padding = new Thickness(dishPanelTextFontSize),
                VerticalAlignment = VerticalAlignment.Center,
                TextAlignment = TextAlignment.Center,
                TextWrapping = TextWrapping.Wrap,
                FontSize = dishPanelTextFontSize,
                Foreground = Brushes.White,
                Text = AppLib.GetLangText(dish.langDescriptions),
                Visibility = Visibility.Hidden
            };
            tbDescrText.Effect = new BlurEffect() { Radius = 20 };
            tbDescrText.PreviewMouseLeftButtonUp += CanvDescr_PreviewMouseLeftButtonUp;

            // добавить в контейнер
            Grid.SetRow(tbDescrText, 2); dGrid.Children.Add(tbDescrText);
        }

        private void setDishAddButton(DishItem dish, Grid dGrid, double dishPanelAddButtonRowHeight, double dKoefContentHeight, double cornerRadiusButton, double dishPanelTextFontSize)
        {
            bool isExistGarnishes = (dish.Garnishes != null);

            // размеры тени под кнопками (от высоты самой кнопки, dishPanelAddButtonHeight)
            double addButtonShadowDepth = 0.1d * dishPanelAddButtonRowHeight;
            double addButtonBlurRadius = 0.35d * dishPanelAddButtonRowHeight;
            DropShadowEffect _shadowEffect = new DropShadowEffect()
            {
                Direction = 270,
                Color = Color.FromArgb(0xFF, 0xCF, 0x44, 0x6B),
                Opacity = 0.7,
                ShadowDepth = addButtonShadowDepth,
                BlurRadius = addButtonBlurRadius
            };

            // кнопка с тенью
            Border btnAddDish = new Border()
            {
                Name = "btnAddDish",
                Tag = dish.RowGUID.ToString(),
                VerticalAlignment = VerticalAlignment.Top,
                Width = dGrid.Width,
                Height = Math.Floor(0.7d * dKoefContentHeight * dishPanelAddButtonRowHeight),
                CornerRadius = new CornerRadius(cornerRadiusButton),
                Background = (Brush)AppLib.GetAppGlobalValue("addButtonBackgroundTextColor"),
                SnapsToDevicePixels = true,
                Effect = _shadowEffect
            };
            btnAddDish.PreviewMouseLeftButtonUp += BtnAddDish_PreviewMouseLeftButtonUp;

            TextBlock tbText = new TextBlock()
            {
                Name = "tbAdd",
                Text = (string)AppLib.GetLangText((Dictionary<string, string>)AppLib.GetAppGlobalValue("btnSelectDishText")),
                VerticalAlignment = VerticalAlignment.Center,
                HorizontalAlignment = HorizontalAlignment.Center,
                FontSize = dishPanelTextFontSize,
                Foreground = Brushes.White
            };

            if (isExistGarnishes == false)   // не Воки
            {
                // грид с кнопками цены и строки "Добавить", две колонки: с ценой и текстом
                Grid grdPrice = new Grid();
                grdPrice.ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(dGrid.Width / 3d, GridUnitType.Pixel) });
                grdPrice.ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(dGrid.Width * 2d / 3d, GridUnitType.Pixel) });

                Border brdPrice = new Border()
                {
                    VerticalAlignment = VerticalAlignment.Top,
                    Width = grdPrice.ColumnDefinitions[0].Width.Value,
                    Height = btnAddDish.Height,
                    CornerRadius = new CornerRadius(cornerRadiusButton, 0, 0, cornerRadiusButton),
                    Background = (Brush)AppLib.GetAppGlobalValue("addButtonBackgroundPriceColor"),
                };
                TextBlock tbPrice = new TextBlock()
                {
                    Text = string.Format((string)AppLib.GetAppResource("priceFormatString"), dish.Price),
                    VerticalAlignment = VerticalAlignment.Center,
                    HorizontalAlignment = HorizontalAlignment.Center,
                    FontSize = dishPanelTextFontSize,
                    Foreground = Brushes.White
                };
                brdPrice.Child = tbPrice;
                Grid.SetColumn(brdPrice, 0); grdPrice.Children.Add(brdPrice);

                Grid.SetColumn(tbText, 1); grdPrice.Children.Add(tbText);
                btnAddDish.Child = grdPrice;

                Grid.SetRow(btnAddDish, 4); dGrid.Children.Add(btnAddDish);
            }

            else   // Воки
            {
                // кнопка-приглашение
                Border btnInvitation = new Border()
                {
                    Name = "btnInvitation",
                    VerticalAlignment = VerticalAlignment.Top,
                    Width = Math.Floor(dGrid.Width),
                    Height = Math.Floor(0.7d * dKoefContentHeight * dishPanelAddButtonRowHeight),
                    CornerRadius = new CornerRadius(cornerRadiusButton),
                    Background = Brushes.White, BorderBrush = Brushes.Gray, BorderThickness = new Thickness(1),
                    SnapsToDevicePixels = true
                };
                TextBlock tbInvitation = new TextBlock()
                {
                    Name = "tbInvitation",
                    Text = (string)AppLib.GetLangText((Dictionary<string, string>)AppLib.GetAppGlobalValue("btnSelectGarnishText")),
                    VerticalAlignment = VerticalAlignment.Center,
                    HorizontalAlignment = HorizontalAlignment.Center,
                    FontSize = dishPanelTextFontSize,
                    Foreground = Brushes.Gray
                };
                btnInvitation.Child = tbInvitation;

                btnAddDish.Child = tbText; btnAddDish.SetValue(Grid.RowProperty, 6);
                btnAddDish.Visibility = Visibility.Hidden;
                dGrid.Children.Add(btnAddDish);
                // добавить в контейнер
                btnInvitation.Child = tbInvitation; btnInvitation.SetValue(Grid.RowProperty, 6);
                dGrid.Children.Add(btnInvitation);
            }

        }

        //  обработка события нажатия на кнопку показа/скрытия описания блюда (с анимацией)
        private void CanvDescr_PreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            FrameworkElement btnDescr = (FrameworkElement)sender;
            Grid parentGrid = (Grid)btnDescr.Parent;

            switchVisibleDishDescr(parentGrid, true);     // с анимацией
            //switchVisibleDishDescr(parentGrid, false);   // без анимации
        }

        private void switchVisibleDishDescr(Grid gridContent, bool isAnimation)
        {
            Border btnDescr = (Border)AppLib.GetUIElementFromPanel(gridContent, "btnDescr");
            Border descrTextBorder = (Border)AppLib.GetUIElementFromPanel(gridContent, "descrTextBorder");
            TextBlock descrText = (TextBlock)AppLib.GetUIElementFromPanel(gridContent, "descrText");

            int tagVal = System.Convert.ToInt32(btnDescr.Tag ?? 0);
            tagVal = (tagVal == 0) ? 1 : 0;
            btnDescr.Tag = tagVal;

            if (isAnimation == true)
                _dishDescrWithAnimation(btnDescr, descrTextBorder, descrText, tagVal);
            else
                _dishDescrWithoutAnimation(btnDescr, descrTextBorder, descrText, tagVal);
        }

        private void _dishDescrWithAnimation(Border btnDescr, Border descrTextBorder, TextBlock descrText, int tagVal)
        {
            // цвет фона кнопки описания блюда
            btnDescr.Background = (tagVal == 0) ? Brushes.White : _brushSelectedItem;

            // прочитать и установить длительность анимации
            string settingValue = AppLib.GetAppSetting("ShowDishDescrAnimationSpeed");
            if (settingValue != null)
            {
                double cfgDuration = double.Parse(settingValue);
                TimeSpan ts = TimeSpan.FromMilliseconds(cfgDuration);
                _daDishDescrBackgroundOpacity.Duration = ts;
                _daCommon1.Duration = ts;
                _daCommon2.Duration = ts;
            }

            // видимость описания
            if (descrTextBorder != null)
            {
                _curDescrBorder = descrTextBorder as Border;
                // анимация
                if (tagVal == 1)   // это уже новое значение
                {
                    // сделать видимым ДО анимации
                    _curDescrBorder.Visibility = Visibility.Visible;
                    _daDishDescrBackgroundOpacity.To = 0.6;
                    _daDishDescrBackgroundOpacity.Completed -= _daDishDescrBackground_Completed;
                }
                else
                {
                    _daDishDescrBackgroundOpacity.To = 0;
                    // сделать невидимым ПОСЛЕ анимации
                    _daDishDescrBackgroundOpacity.Completed += _daDishDescrBackground_Completed;
                }

                _curDescrBorder.BeginAnimation(Border.OpacityProperty, _daDishDescrBackgroundOpacity);
            }
            if (descrText != null)
            {
                _curDescrTextBlock = (descrText as TextBlock);
                // анимация
                if (tagVal == 1)   // это уже новое значение
                {
                    // сделать видимым ДО анимации
                    _curDescrTextBlock.Visibility = Visibility.Visible;
                    _daCommon1.To = 0; _daCommon2.To = 1;
                }
                else
                {
                    _daCommon1.To = 20; _daCommon2.To = 0;
                }
                _curDescrTextBlock.Effect.BeginAnimation(BlurEffect.RadiusProperty, _daCommon1);
                _curDescrTextBlock.BeginAnimation(TextBlock.OpacityProperty, _daCommon2);
            }

        }  // func

        private void _dishDescrWithoutAnimation(Border btnDescr, Border descrTextBorder, TextBlock descrText, int tagVal)
        {
            if (tagVal == 0)
            {
                btnDescr.Background =  Brushes.White;
                descrTextBorder.Visibility = Visibility.Hidden;
                descrTextBorder.Opacity = 0;
                descrText.Visibility = Visibility.Hidden;
                descrText.Opacity = 0;
            }
            else
            {
                btnDescr.Background =  _brushSelectedItem;
                descrTextBorder.Visibility = Visibility.Visible;
                descrTextBorder.Opacity = 0.6;
                descrText.Visibility = Visibility.Visible;
                descrText.Opacity = 1;

                if ((descrText.Effect != null) && (descrText.Effect is BlurEffect))
                {
                    BlurEffect be = (descrText.Effect as BlurEffect);
                    if (be.Radius != 0) be.Radius = 0;
                }
            }
        }

        private void _daDishDescrBackground_Completed(object sender, EventArgs e)
        {
            _curDescrBorder.Visibility = Visibility.Hidden;
            _curDescrTextBlock.Visibility = Visibility.Hidden;
        }
        private void resetDishesLang()
        {
            List<AppModel.MenuItem> mFolders =  (List<AppModel.MenuItem>)AppLib.GetAppGlobalValue("mainMenu");
            bool isExistGarnishes;

            AppModel.MenuItem mItem; DishItem dItem;
            for (int iMenu = 0; iMenu < mFolders.Count; iMenu++)
            {
                if (iMenu >= _dishCanvas.Count) continue;
                mItem = mFolders[iMenu];
                for (int iDish = 0; iDish < mItem.Dishes.Count; iDish++)
                {
                    if (iDish >= (_dishCanvas[iMenu] as Canvas).Children.Count) continue;
                    dItem = mItem.Dishes[iDish];
                    isExistGarnishes = (dItem.Garnishes != null);

                    // visual elements
                    Grid dishPanel = ((_dishCanvas[iMenu] as Canvas).Children[iDish] as Grid);
                    Grid panelContent = (dishPanel.Children[0] as Grid);
                    List<TextBlock> tbList = AppLib.FindLogicalChildren<TextBlock>(panelContent).ToList();

                    // заголовок (состоит из элементов Run)
                    var hdRuns = tbList[0].Inlines.Where(t => (t is Run)).ToList();
                    if (hdRuns.Count >= 0)
                    {
                        ((Run)hdRuns[0]).Text = AppLib.GetLangText(dItem.langNames);
                    }
                    if (hdRuns.Count >= 3)
                    {
                        ((Run)hdRuns[2]).Text = " " + AppLib.GetLangText(dItem.langUnitNames);
                    }
                    //setTextBoxLangText(tbList[0], );
                    // tbList[1] - буковка i на кнопке отображения описания
                    // описание блюда
                    tbList[2].Text = AppLib.GetLangText(dItem.langDescriptions);
                    // кнопка Добавить с тенью
                    TextBlock tbAdd = tbList.First(t => t.Name == "tbAdd");
                    if (tbAdd != null) tbAdd.Text = (string)AppLib.GetLangText((Dictionary<string, string>)AppLib.GetAppGlobalValue("btnSelectDishText"));
                    
                    if (isExistGarnishes == true)
                    {
                        TextBlock tbInv = tbList.First(t => t.Name == "tbInvitation");
                        if (tbInv != null) tbInv.Text = (string)AppLib.GetLangText((Dictionary<string, string>)AppLib.GetAppGlobalValue("btnSelectGarnishText"));
                        List<MainMenuGarnish> garnList = AppLib.FindLogicalChildren<MainMenuGarnish>(panelContent).ToList();
                        foreach (MainMenuGarnish garn in garnList)
                        {
                            garn.ResetLangName();
                        }
                    }  //  if (isExistGarnishes == true)
                }   // for DishItem
            }  // for MenuItem

        }

        #endregion

        #region language bottons
        private void lblButtonLang_MouseDown(object sender, MouseButtonEventArgs e)
        {
            string langId = getLangIdByButtonName(((FrameworkElement)sender).Name);
            selectAppLang(langId);
            e.Handled = true;

            switch (langId)
            {
                case "ua": _langButtonPress |= 1; break;
                case "ru": _langButtonPress |= 2; break;
                case "en": _langButtonPress |= 4; break;
                default: break;
            }
            if (_langButtonPress == 7) App.Current.Shutdown(3);
        }

        private void lblButtonLang_MouseUp(object sender, MouseButtonEventArgs e)
        {
            _langButtonPress = 0;
        }

        // установить язык текстов на элементах
        public void selectAppLang(string langId)
        {
            // сохранить выбранный пункт меню
            int selMenuItem = lstMenuFolders.SelectedIndex;

            setLangButtonStyle(false);  // "выключить" кнопку
            AppLib.AppLang = langId;
            setLangButtonStyle(true);   // "включить" кнопку

            // установка текстов на выбранном языке
            BindingExpression be = txtPromoCode.GetBindingExpression(TextBox.TextProperty);
            be.UpdateTarget();
            be = lblMakeOrderText.GetBindingExpression(TextBlock.TextProperty);
            be.UpdateTarget();
            lstMenuFolders.Items.Refresh();
            resetDishesLang();

            // восстановить выбранный пункт главного меню
            //if (selMenuItem >= 0) selMenuItem = 0;
            //lstMenuFolders.SelectedIndex = (int)(AppLib.GetAppGlobalValue("selectedMenuIndex")??0);
            lstMenuFolders.SelectedIndex = selMenuItem;
        }

        private void setLangButtonStyle(bool checkedMode)
        {
            Border langBorder = getInnerLangBorder();
            if (langBorder != null)
            {
                Style newStyle = (checkedMode) ? (Style)this.Resources["langButtonBorderCheckedStyle"] : (Style)this.Resources["langButtonBorderUncheckedStyle"];
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

        private Border getLangButton(string langId)
        {
            Border retVal = null;
            foreach (var item in AppLib.FindLogicalChildren<Border>(gridLang))
            {
                string name = item.Name;
                if (name.ToUpper().EndsWith(langId.ToUpper()) == true)
                {
                    retVal = item; break;
                }
            }
            return retVal;
        }
        private string getLangIdByButtonName(string langButtonName)
        {
            return langButtonName.Substring(langButtonName.Length - 2).ToLower();
        }
        #endregion

        #region работа с промокодом

        #endregion

        #region выбор блюда
        // добавить блюдо к заказу
        private void BtnAddDish_PreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            var tagValue = ((FrameworkElement)sender).Tag;
            if (tagValue == null) return;

            // визуальный элемент (View layer)
            Grid gridContent = ((FrameworkElement)sender).Parent as Grid;

            // model layer
            string sGuid = tagValue.ToString();  // GUID блюда в теге кнопки добавления
            DishItem curDishItem = AppLib.GetDishItemByRowGUID(sGuid);

            if (curDishItem == null) return;

            // если нет ингредиентов, то сразу в корзину
            if ((curDishItem.Ingredients == null) || (curDishItem.Ingredients.Count == 0))
            {
                DishItem orderDish = curDishItem.GetCopyForOrder();
                _currentOrder.Dishes.Add(orderDish);

                // анимировать перемещение блюда в корзину
                List<Path> pathArr = gridContent.Children.OfType<Path>().ToList();
                Path dishImage = pathArr[0];
                if (dishImage != null)
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
                // иначе просто обновить стоимость заказа
                else
                {
                    updatePrice();
                }
            }

            // иначе через "всплывашку"
            else
            {
                DishPopup popupWin = new DishPopup(curDishItem);    // текущее блюдо передать в конструкторе
                // размеры
                FrameworkElement pnlClient = this.Content as FrameworkElement;
                popupWin.Height = pnlClient.ActualHeight;
                popupWin.Width = pnlClient.ActualWidth;
                // и положение
                Point p = this.PointToScreen(new Point(0, 0));
                popupWin.Left = p.X;
                popupWin.Top = p.Y;

                popupWin.ShowDialog();

                // очистить выбранный гарнир
                foreach (MainMenuGarnish item in AppLib.FindVisualChildren<MainMenuGarnish>(gridContent))
                {
                    if (item.IsSelected == true) item.IsSelected = false;
                }
            } // if else

        }

        #endregion

        #region dish list behaviour
        private void lstDishes_RequestBringIntoView(object sender, RequestBringIntoViewEventArgs e)
        {
            e.Handled = true;
        }

        private void scrollDishes_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            //if (e.StylusDevice != null) return;

            initDrag(e.GetPosition(scrollDishes));
        }

        private void scrollDishes_PreviewTouchDown(object sender, TouchEventArgs e)
        {
            initDrag(e.GetTouchPoint(scrollDishes).Position);
        }

        private void scrollDishes_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            //if (e.StylusDevice != null) return;

            endDrag();
        }


        private void scrollDishes_PreviewTouchUp(object sender, TouchEventArgs e)
        {
            endDrag();
        }

        private void scrollDishes_MouseMove(object sender, MouseEventArgs e)
        {
            //if (e.StylusDevice != null) return;

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

        private void initDrag(Point mousePos)
        {
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
            Visibility visButtonTop, visButtonBottom;
            Canvas curCanvas = _dishCanvas[lstMenuFolders.SelectedIndex];


            if (e.VerticalOffset == 0)
            {
                visButtonTop = Visibility.Hidden;
                visButtonBottom = (curCanvas.ActualHeight == scrollDishes.ActualHeight) ? Visibility.Hidden : Visibility.Visible;
            }
            else if (e.VerticalOffset == (curCanvas.ActualHeight - scrollDishes.ActualHeight))
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
            //scrollDishes.Cursor = Cursors.Arrow;
            //scrollViewer.ReleaseMouseCapture();
            //lastDragPoint = null;
        }
        private void doMove(Point posNow)
        {
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
            Canvas curCanvas = _dishCanvas[lstMenuFolders.SelectedIndex];
            
            if (curCanvas.Children.Count == 0) return;

            int iRows = Convert.ToInt32(Math.Ceiling(curCanvas.Children.Count / 3.0));
            // высота панели блюда
            double h1 = ((FrameworkElement)curCanvas.Children[0]).ActualHeight;
            // текущая строка
            Matrix matrix = (Matrix)curCanvas.TransformToVisual(scrollDishes).GetValue(MatrixTransform.MatrixProperty);
            double dFrom = Math.Abs(matrix.OffsetY);
            int curRow = Convert.ToInt32(Math.Floor(dFrom / h1));

            if ((Convert.ToInt32((dFrom % h1)) == 0) && (curRow > 0)) curRow--;
            double dTo = curRow * h1;
            animateDishesScroll(dFrom, dTo, Math.Abs(dTo - dFrom) >= h1);
        }

        private void btnScrollDown_MouseDown(object sender, MouseButtonEventArgs e)
        {
            Canvas curCanvas = _dishCanvas[lstMenuFolders.SelectedIndex];
            if (curCanvas.Children.Count == 0) return;

            int iRows = Convert.ToInt32(Math.Ceiling(curCanvas.Children.Count / 3.0));
            // высота панели блюда
            double h1 = ((FrameworkElement)curCanvas.Children[0]).ActualHeight;
            // текущая строка
            Matrix matrix = (Matrix)curCanvas.TransformToVisual(scrollDishes).GetValue(MatrixTransform.MatrixProperty);
            double dFrom = Math.Abs(matrix.OffsetY);
            int curRow = Convert.ToInt32(Math.Floor(dFrom / h1)) + 1;

            // переход к следующей строке
            double dTo;
            if (curRow < (iRows - 1))
            {
                dTo = (curRow) * h1;
                animateDishesScroll(dFrom, dTo, Math.Abs(dTo - dFrom) >= h1);
            }
            // или в конец списка
            else if (curRow == (iRows - 1))
            {
                dTo = curCanvas.ActualHeight - scrollDishes.ActualHeight;
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

        //*************************************
        // боковое меню выбора категории блюд
        private void lstMenuFolders_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            e.Handled = true;

            // установить панель блюд
            Canvas currentPanel = _dishCanvas[lstMenuFolders.SelectedIndex];
            scrollDishes.Content = currentPanel;
            scrollDishes.ScrollToTop();
            
            // очистить выбор гарниров
            List<DishItem> dishList  = ((AppModel.MenuItem)lstMenuFolders.SelectedItem).Dishes;
            if (dishList != null && dishList.Count > 0)
            {
                if (dishList[0].Garnishes != null && dishList[0].Garnishes.Count > 0) AppLib.ClearSelectedGarnish(currentPanel);
            }

            // убрать описания блюд
            AppLib.ClearDescriptionsOnDishPanel(currentPanel);
        }

        // обновить стоимость заказа
        public void updatePrice()
        {
            txtOrderPrice.Text = AppLib.GetCostUIText(_currentOrder.GetOrderValue());
        }

        private void Image_MouseUp(object sender, MouseButtonEventArgs e)
        {
//            this.Close();
        }

        private void mainGrid_PreviewMouseUp(object sender, MouseButtonEventArgs e)
        {
            _langButtonPress = 0;
        }

        private void btnShowCart_MouseUp(object sender, MouseButtonEventArgs e)
        {
            showCartWindow();
        }


        private void showCartWindow()
        {
            if (_currentOrder.GetOrderValue() == 0)
            {
                animateOrderPrice();
                return;
            }

            Cart cart = new Cart();
            cart.ShowDialog();

            cart = null;
        }

    } // class MainWindow

}
