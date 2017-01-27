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
using System.Configuration;
using System.Data.SqlClient;
using System.Data;
using System.Diagnostics;
using System.Windows.Media.Animation;
using System.Windows.Media.Effects;

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
        Border _curDescrBorder;
        TextBlock _curDescrTextBlock;
        SolidColorBrush _brushSelectedItem;
        
        // dish description animations
        DoubleAnimation _daCommon1, _daCommon2;
        DoubleAnimation _daDishDescrBackgroundOpacity;  // анимашка прозрачности описания блюда
        // dish select animation
        PathGeometry _animSelectDishPath;
        Storyboard _animSelectDishStoryBoard;

        // dragging
        Point? lastDragPoint, initDragPoint;
        protected DateTime _dateTime;


        public MainWindow()
        {
            InitializeComponent();

            // для настройки элементов после отрисовки окна
            Loaded += MainWindow_Loaded;

            // инициализация локальных переменных
            _dishCanvas = new List<Canvas>();
            _daDishDescrBackgroundOpacity = new DoubleAnimation()
            {
                Duration = new Duration(new TimeSpan(0, 0, 0, 1)),
                EasingFunction = new CubicEase() { EasingMode = EasingMode.EaseOut }
            };
            _daCommon1 = new DoubleAnimation() { Duration = new Duration(new TimeSpan(0, 0, 0, 1)), };
            _daCommon2 = new DoubleAnimation() { Duration = new Duration(new TimeSpan(0, 0, 0, 1)), };
            _brushSelectedItem = (SolidColorBrush)AppLib.GetAppGlobalValue("appSelectedItemColor");

            // создать текущий заказ
            _currentOrder = new OrderItem();
            AppLib.SetAppGlobalValue("currentOrder", _currentOrder);
            updatePrice();

            initUI();
        }

        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            //   конечный элемент анимации выбора блюда, это Point3 в BezierSegment
            Point toBasePoint = brdMakeOrder.PointToScreen(new Point(0, 0));
            Size toSize = brdMakeOrder.RenderSize;
            Point endPoint = new Point(toBasePoint.X + toSize.Width / 2.0, toBasePoint.Y + toSize.Height / 2.0);
            // установить для сегмента анимации конечную точку
            PathFigure pf = _animSelectDishPath.Figures[0];
            BezierSegment bs = (pf.Segments[0] as BezierSegment);
            bs.Point3 = endPoint;
        }

        private void initUI()
        {
            AppLib.AppLogger.Trace("Настраиваю визуальные элементы...");
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
            // создать канву со списком блюд
            createDishesCanvas(mFolders);
            
            lstMenuFolders.Focus();
            lstMenuFolders.ItemsSource = mFolders;
            lstMenuFolders.SelectedIndex = 0;

            // установить язык UI
            selectAppLang(AppLib.AppLang);

            // анимация выбора блюда
            // используется анимашка пути в панели анимации (cnvAnim)
            //TransformGroup transGroup = new TransformGroup()
            //{
            //    Children = new TransformCollection {
            //        new TranslateTransform(),
            //        new RotateTransform(),
            //        new ScaleTransform()
            //    }
            //};
            // путь перемещения
            _animSelectDishPath = new PathGeometry();
            _animSelectDishPath.Figures.Add(new PathFigure()
            {
                IsClosed = false,
                Segments = new PathSegmentCollection { new BezierSegment() { IsStroked = true } }
            });
            // отобразить путь перемещения на канве
            Path visPath = new Path()
            {
                Data = _animSelectDishPath,
                Stroke = Brushes.Red,
                StrokeThickness = 1
            };
            cnvAnim.Children.Add(visPath);

            //Path spotFrom = new Path()
            //{
            //    Fill = Brushes.Blue,
            //    Data = new RectangleGeometry(new Rect(0,0,200,150), 10,10),
            //    //RenderTransformOrigin = new Point(0.5,0.5),
            //    RenderTransform = transGroup
            //};
            //Canvas.SetLeft(spotFrom, -100); Canvas.SetTop(spotFrom, -75);
            //cnvAnim.Children.Add(spotFrom);

            int animSpeed = int.Parse(AppLib.GetAppSetting("SelectDishAnimationSpeed"));  // in msec
            TimeSpan ts = new TimeSpan(0, 0, 0, 0, animSpeed);

            // создать две анимации перемещения, для координат X и Y
            DoubleAnimationUsingPath daX = new DoubleAnimationUsingPath();
            daX.PathGeometry = _animSelectDishPath;
            daX.Duration = ts;
            daX.Source = PathAnimationSource.X;
            Storyboard.SetTarget(daX, animDishImage);
            Storyboard.SetTargetProperty(daX, new PropertyPath("(UIElement.RenderTransform).(TransformGroup.Children)[0].(TranslateTransform.X)"));
            DoubleAnimationUsingPath daY = new DoubleAnimationUsingPath();
            daY.PathGeometry = _animSelectDishPath;
            daY.Duration = ts;
            daY.Source = PathAnimationSource.Y;
            Storyboard.SetTarget(daY, animDishImage);
            Storyboard.SetTargetProperty(daY, new PropertyPath("(UIElement.RenderTransform).(TransformGroup.Children)[0].(TranslateTransform.Y)"));

            // анимация прозрачности
            DoubleAnimation daOpacity = new DoubleAnimation(1, 0, ts);
            Storyboard.SetTarget(daOpacity, animDishImage);
            Storyboard.SetTargetProperty(daOpacity, new PropertyPath("(UIElement.Opacity)"));

            // анимация масштабирования
            DoubleAnimation daScaleX = new DoubleAnimation(1, 0.4, ts);
            Storyboard.SetTarget(daScaleX, animDishImage);
            Storyboard.SetTargetProperty(daScaleX, new PropertyPath("(UIElement.RenderTransform).(TransformGroup.Children)[2].(ScaleTransform.ScaleX)"));
            //DoubleAnimation daScaleY = new DoubleAnimation(1, 0.4, ts);
            //Storyboard.SetTarget(daScaleY, spotFrom);
            //Storyboard.SetTargetProperty(daScaleY, new PropertyPath("(UIElement.RenderTransform).(TransformGroup.Children)[2].(ScaleTransform.ScaleY)"));


            // создать две анимации масштабирования, для ширины и высоты
            //DoubleAnimationUsingPath daX = new DoubleAnimationUsingPath();
            //daX.PathGeometry = _animSelectDishPath;
            //daX.Duration = new Duration(new TimeSpan(0, 0, 0, 0, animSpeed));
            //daX.Source = PathAnimationSource.X;
            //Storyboard.SetTarget(daX, spotFrom);
            //Storyboard.SetTargetProperty(daX, new PropertyPath("(UIElement.RenderTransform).(TransformGroup.Children)[0].(TranslateTransform.X)"));
            //DoubleAnimationUsingPath daY = new DoubleAnimationUsingPath();
            //daY.PathGeometry = _animSelectDishPath;
            //daY.Duration = new Duration(new TimeSpan(0, 0, 0, 0, animSpeed));
            //daY.Source = PathAnimationSource.Y;
            //Storyboard.SetTarget(daY, spotFrom);
            //Storyboard.SetTargetProperty(daY, new PropertyPath("(UIElement.RenderTransform).(TransformGroup.Children)[0].(TranslateTransform.Y)"));

            // создать раскладку и добавить в нее анимации
            _animSelectDishStoryBoard = new Storyboard();
            _animSelectDishStoryBoard.Children.Add(daX);
            _animSelectDishStoryBoard.Children.Add(daY);
            _animSelectDishStoryBoard.Children.Add(daOpacity);
            //_animSelectDishStoryBoard.Children.Add(daScaleX);
            //_animSelectDishStoryBoard.Children.Add(daScaleY);
            _animSelectDishStoryBoard.Completed += animSelectDishStoryBoard_Completed;

            // выключить курсор мыши
            if ((bool)AppLib.GetAppGlobalValue("MouseCursor") == false)
            {
                this.Cursor = Cursors.None;
                Mouse.OverrideCursor = Cursors.None;
            }

            AppLib.AppLogger.Trace("Настраиваю визуальные элементы - READY");
        }

        public List<Canvas> DishesPanels { get { return _dishCanvas; } }

        #region работа со списком блюд

        private void createDishesCanvas(List<AppModel.MenuItem> mFolders)
        {
            #region privates vars
            double screenWidth, screenHeight;
            screenWidth = (double)AppLib.GetAppGlobalValue("screenWidth");
            screenHeight = (double)AppLib.GetAppGlobalValue("screenHeight");

            // углы закругления
            double cornerRadiusButton = (double)AppLib.GetAppGlobalValue("cornerRadiusButton");
            double cornerRadiusDishPanel = (double)AppLib.GetAppGlobalValue("cornerRadiusDishPanel");

            //  РАЗМЕРЫ ПАНЕЛИ БЛЮД(А)
            double dishesPanelWidth = (screenWidth / 6d * 5d);
            double dishPanelWidth = 0.95d * dishesPanelWidth / 3d;
            double dKoefContentWidth = 0.9d;
            double contentPanelWidth = dKoefContentWidth * dishPanelWidth;
            // высота строки заголовка
            double dishPanelHeaderRowHeight = 0.17d * dishPanelWidth;
            // высота строки изображения
            double dishPanelImageRowHeight = 0.7d * dishPanelWidth;
            // высота строки гарниров
            double dishPanelGarnishesRowHeight = 0.2d * dishPanelWidth;
            // высота строки кнопки добавления
            double dishPanelAddButtonRowHeight = 0.15d * dishPanelWidth;
            double dishPanelAddButtonTextSize = 0.3d * dishPanelAddButtonRowHeight;
            // расстояния между строками панели блюда
            double dishPanelRowMargin1 = 0.01d * dishPanelWidth;
            double dishPanelRowMargin2 = 0.02d * dishPanelWidth;
            // размер шрифтов
            double dishPanelHeaderFontSize = 0.06d * dishPanelWidth;
            double dishPanelTextFontSize = 0.8d * dishPanelHeaderFontSize;
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
                if (mItem.Dishes.Count == 0) continue;
                canvas = new Canvas();
                canvas.Width = dishesPanelWidth;
                int iRowsCount = ((mItem.Dishes.Count-1) / 3) + 1;

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
                    setDishDescription(dish, dGrid, cornerRadiusDishPanel, dishPanelDescrButtonSize, dishPanelTextFontSize, dishPanelHeaderFontSize);

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
                    }

                    // изображения кнопок добавления
                    setDishAddButton(dish, dGrid, dishPanelAddButtonRowHeight, dKoefContentHeight, cornerRadiusButton, dishPanelTextFontSize);

                    brd.Children.Add(dGrid); Grid.SetRow(dGrid, 1); Grid.SetColumn(dGrid, 1);
                    canvas.Children.Add(brd);
                }

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
                        markImage.Effect = new DropShadowEffect() { Opacity = 0.7 };
                        markImage.Width = dishPanelHeaderFontSize; markImage.Height = dishPanelHeaderFontSize;
                        markImage.Source = ImageHelper.ByteArrayToBitmapImage(markItem.Image);
                        InlineUIContainer iuc = new InlineUIContainer(markImage);
                        markImage.Margin = new Thickness(0, 0, 5, 10);
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

        private void setDishDescription(DishItem dish, Grid dGrid, double cornerRadiusDishPanel, double dishPanelDescrButtonSize, double dishPanelTextFontSize, double dishPanelHeaderFontSize)
        {
            if (dish.Image == null) return;

            double dishPanelImageRowHeight = dGrid.RowDefinitions[2].Height.Value;
            Rect rect = new Rect(0, 0, dGrid.Width, dishPanelImageRowHeight);
            // изображение
            Path pathImage = new Path();
            pathImage.Data = new RectangleGeometry(rect, cornerRadiusDishPanel, cornerRadiusDishPanel);
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
                Height = dishPanelImageRowHeight,
                CornerRadius = new CornerRadius(cornerRadiusDishPanel),
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
                mItem = mFolders[iMenu];
                for (int iDish = 0; iDish < mItem.Dishes.Count; iDish++)
                {
                    dItem = mItem.Dishes[iDish];
                    isExistGarnishes = (dItem.Garnishes != null);

                    // visual elements
                    Grid dishPanel = ((_dishCanvas[iMenu] as Canvas).Children[iDish] as Grid);
                    Grid panelContent = (dishPanel.Children[0] as Grid);
                    List<TextBlock> tbList = AppLib.FindLogicalChildren<TextBlock>(panelContent).ToList();

                    // заголовок (состоит из элементов Run)
                    var hdRuns = tbList[0].Inlines.Where(t => (t is Run)).ToList();
                    ((Run)hdRuns[0]).Text = AppLib.GetLangText(dItem.langNames);
                    ((Run)hdRuns[2]).Text = " " + AppLib.GetLangText(dItem.langUnitNames);
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
            //            if (e.StylusDevice != null) return;

            string langId = getLangIdByButtonName(((FrameworkElement)sender).Name);
            selectAppLang(langId);
        }
        //private void lblButtonLang_TouchDown(object sender, TouchEventArgs e)
        //{
        //    selectAppLang((FrameworkElement)sender);
        //}

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

            string sGuid = tagValue.ToString();  // GUID блюда в теге кнопки добавления
            DishItem curDishItem = AppLib.GetDishItemByRowGUID(sGuid);

            if (curDishItem == null) return;

            // если нет ингредиентов, то сразу в корзину
            if ((curDishItem.Ingredients == null) || (curDishItem.Ingredients.Count == 0))
            {
                DishItem orderDish = curDishItem.GetCopyForOrder();
                _currentOrder.Dishes.Add(orderDish);

                // нарисовать путь
                Grid gridContent = ((FrameworkElement)sender).Parent as Grid;
                List<Path> pathArr = gridContent.Children.OfType<Path>().ToList();
                FrameworkElement dishImage = pathArr[0];
                if (dishImage != null)
                {
                    // установить параметры пути движения изображения
                    PathFigure pf = _animSelectDishPath.Figures[0];
                    BezierSegment bezierSeg = (pf.Segments[0] as BezierSegment);

                    // получить точку начала анимации: центр панели блюда
                    Point fromBasePoint = dishImage.PointToScreen(new Point(0, 0));
                    Size fromSize = dishImage.RenderSize;
                    Point fromPoint = new Point(fromBasePoint.X + fromSize.Width / 2.0, fromBasePoint.Y + fromSize.Height / 2.0);
                    Point toPoint = bezierSeg.Point3;

                    // установить значения пути движения
                    pf.StartPoint = fromPoint;   // начальная точка
                    // и опорные точки кривой Безье
                    double dX = fromPoint.X - toPoint.X;
                    double dY = toPoint.Y - fromPoint.Y;
                    Point p1 = new Point(fromPoint.X - 0.3 * dX, fromPoint.Y - 0.3 * dY);
                    Point p2 = new Point(toPoint.X + 0.05 * dX, toPoint.Y - 0.8 * dY);
                    bezierSeg.Point1 = p1;
                    bezierSeg.Point2 = p2;

                    Rect rect = (animDishImage.Data as RectangleGeometry).Rect;
                    rect.Width = 150; rect.Height = 100;
                    animDishImage.Width = 150;
                    animDishImage.Height = 100;

                    cnvAnim.Visibility = Visibility.Visible;
                    _animSelectDishStoryBoard.Begin();

                    //            double dishPanelImageRowHeight = dGrid.RowDefinitions[2].Height.Value;
                    //Rect rect = new Rect(0, 0, dGrid.Width, dishPanelImageRowHeight);
                    //// изображение
                    //System.Windows.Shapes.Path pathImage = new System.Windows.Shapes.Path();
                    //pathImage.Data = new RectangleGeometry(rect, cornerRadiusDishPanel, cornerRadiusDishPanel);
                    //pathImage.Fill = new DrawingBrush(
                    //    new ImageDrawing() { ImageSource = ImageHelper.ByteArrayToBitmapImage(dish.Image), Rect = rect }
                    //    );
                    ////pathImage.Effect = new DropShadowEffect();
                    //// добавить в контейнер
                    //Grid.SetRow(pathImage, 2); dGrid.Children.Add(pathImage);

                }

                // и обновить стоимость заказа
                updatePrice();
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

                bool? dialogResult = popupWin.ShowDialog();

                if ((dialogResult??false) == true)
                {
                    // добавить блюдо в заказ
                    OrderItem curOrder = (OrderItem)AppLib.GetAppGlobalValue("currentOrder");
                    DishItem orderDish = curDishItem.GetCopyForOrder();   // сделать копию блюда со всеми добавками
                    curOrder.Dishes.Add(orderDish);
                    // добавить в заказ рекомендации
                    if ((curDishItem.SelectedRecommends != null) && (curDishItem.SelectedRecommends.Count > 0))
                    {
                        foreach (DishItem item in curDishItem.SelectedRecommends)
                        {
                            curOrder.Dishes.Add(item);
                        }
                    }

                    curDishItem.ClearAllSelections();   // очистить добавки в текущем блюде
                    // и обновить стоимость заказа
                    updatePrice();
                }   // if

            } // if else
        }

        // после завершения анимации выбора блюда скрыть панель анимации
        private void animSelectDishStoryBoard_Completed(object sender, EventArgs e)
        {
            cnvAnim.Visibility = Visibility.Hidden;
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

        // сбросить выбор блюда
        // обновить стоимость заказа
        public void updatePrice()
        {
            txtOrderPrice.Text = AppLib.GetCostUIText(_currentOrder.GetOrderValue());
        }

        private void Image_MouseUp(object sender, MouseButtonEventArgs e)
        {
            this.Close();
        }

        private void btnShowCart_MouseUp(object sender, MouseButtonEventArgs e)
        {
            showCartWindow();
        }


        private void showCartWindow()
        {
            Cart cart = new Cart();
            cart.ShowDialog();

            cart = null;
        }

    } // class MainWindow

}
