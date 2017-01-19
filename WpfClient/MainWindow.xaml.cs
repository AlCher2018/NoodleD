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
using System.Windows.Navigation;
using System.Windows.Shapes;

using AppModel;
using System.Configuration;
using System.Collections.ObjectModel;
using NLog;
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
        static MainWindow()
        {
            double dVar;
            
            double dishPanelHeaderFontSize, dishPanelTextFontSize;
            double dishPanelDescrButtonSize;
            double dishPanelHeaderRowHeight;
            double dishPanelImageRowHeight;
            double dishPanelGarnishesRowHeight, dishPanelGarnishBaseWidth;
            double dishPanelAddButtonRowHeight, dishPanelAddButtonHeight, dishPanelAddButtonTextSize;
            double dishPanelAddButtonShadowDepth, dishPanelAddButtonShadowBlurRadius, dishPanelAddButtonShadowCornerRadius;
            double dishPanelRowMargin1, dishPanelRowMargin2;

            double screenWidth, screenHeight;
            screenWidth = SystemParameters.PrimaryScreenWidth;
            //            screenWidth = SystemParameters.VirtualScreenWidth;
            screenHeight = SystemParameters.PrimaryScreenHeight;
            //            screenHeight = SystemParameters.VirtualScreenHeight;

            AppLib.SetAppGlobalValue("screenWidth", screenWidth);
            AppLib.SetAppGlobalValue("screenHeight", screenHeight);

            // углы закругления
            dVar = 0.005 * screenWidth;
            AppLib.SetAppGlobalValue("cornerRadiusButton", dVar);
            AppLib.SetAppGlobalValue("cornerRadiusGarnish", 0.5 * dVar);
            AppLib.SetAppGlobalValue("cornerRadiusDishPanel", 2 * dVar);

            dVar = 0.5 * screenWidth;
            AppLib.SetAppGlobalValue("maxDialogWindowWidth", dVar);

            // РАЗМЕРЫ ШРИФТОВ
            double appFontSize0, appFontSize1, appFontSize2, appFontSize3, appFontSize4, appFontSize5, appFontSize6, appFontSize7;
            double minVal = Math.Min(screenWidth, screenHeight);
            appFontSize0 = 0.055 * minVal;
            appFontSize1 = 0.04 * minVal;
            appFontSize2 = 0.8 * appFontSize1;
            appFontSize3 = 0.8 * appFontSize2;
            appFontSize4 = 0.8 * appFontSize3;
            appFontSize5 = 0.8 * appFontSize4;
            appFontSize6 = 0.8 * appFontSize5;
            appFontSize7 = 0.8 * appFontSize6;
            AppLib.SetAppGlobalValue("appFontSize0", appFontSize0);
            AppLib.SetAppGlobalValue("appFontSize1", appFontSize1);
            AppLib.SetAppGlobalValue("appFontSize2", appFontSize2);
            AppLib.SetAppGlobalValue("appFontSize3", appFontSize3);
            AppLib.SetAppGlobalValue("appFontSize4", appFontSize4);
            AppLib.SetAppGlobalValue("appFontSize5", appFontSize5);
            AppLib.SetAppGlobalValue("appFontSize6", appFontSize6);
            AppLib.SetAppGlobalValue("appFontSize7", appFontSize7);

            //  РАЗМЕРЫ ПАНЕЛИ БЛЮД(А)
            double dishesPanelWidth = (screenWidth / 6.0 * 5.0);
            AppLib.SetAppGlobalValue("dishesPanelWidth", dishesPanelWidth);
            AppLib.SetAppGlobalValue("dishesPanelScrollButtonSize", 0.15 * dishesPanelWidth); 
            // расчет ширины панели блюда
            double dishPanelWidth;
//            dishPanelWidth = 0.95 * dishesPanelWidth / 3.18;  // 3x + 6*0.03x - ширина панелей + отступ слева/справа 
            dishPanelWidth = 0.95 * dishesPanelWidth / 3;
            AppLib.SetAppGlobalValue("dishPanelWidth", dishPanelWidth);

            // высота строки заголовка
            dishPanelHeaderRowHeight = 0.15 * dishPanelWidth;
            AppLib.SetAppGlobalValue("dishPanelHeaderRowHeight", dishPanelHeaderRowHeight);
            // высота строки изображения
            dishPanelImageRowHeight = 0.7 * dishPanelWidth;
            AppLib.SetAppGlobalValue("dishPanelImageRowHeight", dishPanelImageRowHeight);
            // высота строки гарниров
            dishPanelGarnishesRowHeight = 0.2 * dishPanelWidth;
            AppLib.SetAppGlobalValue("dishPanelGarnishesRowHeight", dishPanelGarnishesRowHeight);
            // ширина подложки гарнира (см. соотн.сторон в Canvas x:Key="garnBase" Width="130" Height="100")
            dishPanelGarnishBaseWidth = 1.3 * dishPanelGarnishesRowHeight;
            AppLib.SetAppGlobalValue("dishPanelGarnishBaseWidth", dishPanelGarnishBaseWidth);

            // высота строки кнопки добавления
            dishPanelAddButtonRowHeight = 0.18 * dishPanelWidth;
            AppLib.SetAppGlobalValue("dishPanelAddButtonRowHeight", dishPanelAddButtonRowHeight);
            dishPanelAddButtonHeight = 0.6 * dishPanelAddButtonRowHeight;
            AppLib.SetAppGlobalValue("dishPanelAddButtonHeight", dishPanelAddButtonHeight);
            dishPanelAddButtonTextSize = 0.3 * dishPanelAddButtonRowHeight;
            AppLib.SetAppGlobalValue("dishPanelAddButtonTextSize", dishPanelAddButtonTextSize);
            // размеры тени под кнопками (от высоты самой кнопки, dishPanelAddButtonHeight)
            dishPanelAddButtonShadowDepth = 0.15 * dishPanelAddButtonHeight;
            dishPanelAddButtonShadowBlurRadius = 0.6 * dishPanelAddButtonHeight;
            dishPanelAddButtonShadowCornerRadius = 0.25 * dishPanelWidth;
            AppLib.SetAppGlobalValue("dishPanelAddButtonShadowDepth", dishPanelAddButtonShadowDepth);
            AppLib.SetAppGlobalValue("dishPanelAddButtonShadowBlurRadius", dishPanelAddButtonShadowBlurRadius);
            AppLib.SetAppGlobalValue("dishPanelAddButtonShadowCornerRadius", dishPanelAddButtonShadowCornerRadius);

            // расстояния между строками панели блюда
            dishPanelRowMargin1 = 0.01 * dishPanelWidth;
            dishPanelRowMargin2 = 0.02 * dishPanelWidth;
            AppLib.SetAppGlobalValue("dishPanelRowMargin1", dishPanelRowMargin1);
            AppLib.SetAppGlobalValue("dishPanelRowMargin2", dishPanelRowMargin2);
            // размер шрифтов
            dishPanelHeaderFontSize = 0.06 * dishPanelWidth;
            AppLib.SetAppGlobalValue("dishPanelHeaderFontSize", dishPanelHeaderFontSize);
            dishPanelTextFontSize = 0.6 * dishPanelHeaderFontSize;
            AppLib.SetAppGlobalValue("dishPanelTextFontSize", dishPanelTextFontSize);
            // размер кнопки описания блюда
            dishPanelDescrButtonSize = 0.1 * dishPanelWidth;
            AppLib.SetAppGlobalValue("dishPanelDescrButtonSize", dishPanelDescrButtonSize);

            // высота панелей
            double dishPanelHeight, dishPanelHeightWithGarnish;
            dishPanelHeight = dishPanelHeaderRowHeight + dishPanelRowMargin1 + dishPanelImageRowHeight + dishPanelRowMargin2 + dishPanelAddButtonRowHeight;
            dishPanelHeightWithGarnish = dishPanelHeight + dishPanelGarnishesRowHeight + dishPanelRowMargin2;
            AppLib.SetAppGlobalValue("dishPanelHeight", dishPanelHeight);
            AppLib.SetAppGlobalValue("dishPanelHeightWithGarnish", dishPanelHeightWithGarnish);
        }

        private static Logger logger = LogManager.GetCurrentClassLogger();

        // runtime
        private OrderItem _currentOrder;
        private DishItem _curDishItem;
        // visual elements
        Border _curDescrBorder;
        TextBlock _curDescrTextBlock;
        System.Windows.Shapes.Path _curGarnishBorder;
        TextBlock _curGarnishTextBlock;
        Border _curAddButton;
        SolidColorBrush _brushSelectedItem;
        // animations
        DoubleAnimation _daCommon1, _daCommon2;
        DoubleAnimation _daDishDescrBackgroundOpacity;

        // scroll viewer animate
        List<double> animDishRows;

        // dragging
        Point? lastDragPoint, initDragPoint;
        protected DateTime _dateTime;

        private List<Canvas> _dishCanvas;


        public MainWindow()
        {
            InitializeComponent();
            
            animDishRows = new List<double>();
            _dishCanvas = new List<Canvas>();

            appInit();

            _brushSelectedItem = (SolidColorBrush)AppLib.GetAppGlobalValue("appSelectedItemColor");

            _daDishDescrBackgroundOpacity = new DoubleAnimation()
            {
                Duration = new Duration(new TimeSpan(0, 0, 0, 1)),
                EasingFunction = new CubicEase() { EasingMode = EasingMode.EaseOut }
            };
            _daCommon1 = new DoubleAnimation() { Duration = new Duration(new TimeSpan(0, 0, 0, 1)), };
            _daCommon2 = new DoubleAnimation() { Duration = new Duration(new TimeSpan(0, 0, 0, 1)), };

        }


        private void appInit()
        {
            //this.Cursor = Cursors.None;
            //Mouse.OverrideCursor = Cursors.None;

            //TestData.mainProc();
            //TestData.setInrgImages();

            logger.Info("Start application");

            string logMsg = "Проверяю соединение с источником данных...";
            try
            {
                checkDBConnection();
            }
            catch (Exception e)
            {
                logger.Trace(logMsg);
                logger.Fatal(e.Message);
                throw;
            }
            logger.Trace(logMsg + " Ok");

            logger.Trace("Получаю данные от SQL Server...");
            ObservableCollection<AppModel.MenuItem> mFolders = null;
            try
            {
                using (NoodleDContext db = new NoodleDContext())
                {
                    logger.Trace("EntityFramework connection string: {0}", db.Database.Connection.ConnectionString);
                    logger.Trace("invoke initCurrentSettings(db)");
                    initCurrentSettings(db);

                    // получить язык UI из config-файла и сохранить его 
                    AppLib.AppLang = AppLib.GetAppSetting("langDefault");

                    // получить данные с SQL во внутренние объекты
                    logger.Trace("invoke MenuLib.GetMenuMainFolders()");
                    mFolders = MenuLib.GetMenuMainFolders();

                    if (mFolders == null) throw new Exception("Ошибка создания меню");
                }
                logger.Trace("Получаю данные от SQL Server - READY");
            }
            catch (Exception e)
            {
                logger.Fatal("Fatal error: {0}\nSource: {1}\nStackTrace: {2}", e.Message, e.Source, e.StackTrace);
                MessageBox.Show("Ошибка доступа к данным: " + e.Message + "\nПрограмма будет закрыта.");
                Application.Current.Shutdown(1);
            }

            // прочие настройки
            AppLib.SaveAppSettingToProps("ssdID", null);   // идентификатор устройства самообслуживания
            AppLib.SaveAppSettingToProps("CurrencyChar", null);   // символ денежной единицы
            AppLib.SaveAppSettingToProps("BillPageWidht", typeof(int));
            AppLib.SaveAppSettingToPropTypeBool("IsPrintBarCode");
            AppLib.SaveAppSettingToPropTypeBool("IsIncludeBarCodeLabel");
            
            // добавить некоторые постоянные тексты
            setAppLangString();

            if (mFolders != null) initUI(mFolders);

            // создать текущий заказ
            _currentOrder = new OrderItem();
            AppLib.SetAppGlobalValue("currentOrder", _currentOrder);

            updatePrice();
        }

        private void setAppLangString()
        {
            parseAndSetAllLangString("dialogBoxYesText");
            parseAndSetAllLangString("dialogBoxNoText");
            parseAndSetAllLangString("cartDelIngrTitle");
            parseAndSetAllLangString("cartDelIngrQuestion");
            parseAndSetAllLangString("cartDelDishTitle");
            parseAndSetAllLangString("cartDelDishQuestion");
            parseAndSetAllLangString("wordOr");
            parseAndSetAllLangString("takeOrderOut");
            parseAndSetAllLangString("takeOrderIn");

            parseAndSetAllLangString("CurrencyName");
        }

        private void parseAndSetAllLangString(string resKey)
        {
            string resValue = AppLib.GetAppSetting(resKey);
            if (string.IsNullOrEmpty(resValue) == true) return;

            string[] aStr = resValue.Split('|');
            if (aStr.Length != 3) return;

            Dictionary<string, string> d = new Dictionary<string, string>();
            d.Add("ru", aStr[0]); d.Add("ua", aStr[1]); d.Add("en", aStr[2]);
            AppLib.SetAppGlobalValue(resKey, d);
        }

        private void initUI(ObservableCollection<AppModel.MenuItem> mFolders)
        {
            logger.Trace("Настраиваю визуальные элементы...");

            btnScrollDown.Width = (double)AppLib.GetAppGlobalValue("dishesPanelScrollButtonSize");
            btnScrollDown.Height = (double)AppLib.GetAppGlobalValue("dishesPanelScrollButtonSize");
            btnScrollUp.Width = (double)AppLib.GetAppGlobalValue("dishesPanelScrollButtonSize");
            btnScrollUp.Height = (double)AppLib.GetAppGlobalValue("dishesPanelScrollButtonSize");

            // добавить к блюдам надписи на кнопках
            Dictionary<string, string> langSelGarnishDict = (Dictionary<string, string>)AppLib.GetAppGlobalValue("btnSelectGarnishText");
            Dictionary<string, string> langAddDishDict = (Dictionary<string, string>)AppLib.GetAppGlobalValue("btnSelectDishText");

            createDishesCanvas(mFolders);

            AppLib.SetAppGlobalValue("mainMenu", mFolders);
            lstMenuFolders.Focus();
            lstMenuFolders.ItemsSource = mFolders;
            lstMenuFolders.SelectedIndex = 0;

            // установить язык UI
            selectAppLang(null);

            logger.Trace("Настраиваю визуальные элементы - READY");
        }

        // **************************************************************************************
        //       TO DO
        // **************************************************************************************
        private void createDishesCanvas(ObservableCollection<AppModel.MenuItem> mFolders)
        {
            #region privates vars
            double screenWidth, screenHeight;
            screenWidth = SystemParameters.PrimaryScreenWidth;
            screenHeight = SystemParameters.PrimaryScreenHeight;

            // углы закругления
            double dVar = 0.005 * screenWidth;
            double cornerRadiusButton =dVar;
            double cornerRadiusDishPanel = 2 * dVar;

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

        //*************************************************
        //  ADD DISH TO ORDER
        //*************************************************
        private void BtnAddDish_PreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            var tagValue = ((FrameworkElement)sender).Tag;
            if (tagValue == null) return;

            string sGuid = tagValue.ToString();

            DishItem selDishItem = AppLib.GetDishItemByRowGUID(sGuid);

            if (selDishItem == null) return;

            // если нет ингредиентов, то сразу в корзину
            if ((selDishItem.Ingredients == null) || (selDishItem.Ingredients.Count == 0))
            {
                DishItem orderDish = selDishItem.GetCopyForOrder();
                _currentOrder.Dishes.Add(orderDish);

                // снять выделение
                this.clearSelectedDish();

                // нарисовать путь
                FrameworkElement dishPanel = AppLib.FindLogicalParentByName((FrameworkElement)sender, "gridDish", 4);
                if (dishPanel != null)
                {
                    System.Windows.Shapes.Path animPath = getAnimPath(dishPanel);

                    //cnvAnim.Children.Add(animPath);

                    //ColorAnimation colorAnim = new ColorAnimation(Colors.Aqua, Colors.Red, TimeSpan.FromMilliseconds(2000));
                    //colorAnim.Completed += ColorAnim_Completed;
                    //spotTo.Fill.BeginAnimation(SolidColorBrush.ColorProperty, colorAnim);

                    //Storyboard sb = new Storyboard();
                    //PropertyPath colorTargetPath = new PropertyPath("(Ellipse.Fill).(SolidColorBrush.Color)");
                    //Storyboard.SetTarget(colorAnim, spotTo);
                    //Storyboard.SetTargetProperty(colorAnim, colorTargetPath);
                    //sb.Children.Add(colorAnim);
                    //sb.AutoReverse = true;
                    //sb.Begin();

                }

                // и обновить стоимость заказа
                updatePrice();
            }
            else
            {
                // иначе через "всплывашку"
                DishPopup popupWin = new DishPopup(_curDishItem);
                // размеры
                FrameworkElement pnlClient = this.Content as FrameworkElement;
                popupWin.Height = pnlClient.ActualHeight;
                popupWin.Width = pnlClient.ActualWidth;
                // и положение
                Point p = this.PointToScreen(new Point(0, 0));
                popupWin.Left = p.X;
                popupWin.Top = p.Y;

                popupWin.ShowDialog();
            }
        }

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
                        markImage.Effect = new DropShadowEffect() { Opacity=0.7};
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
            System.Windows.Shapes.Path pathImage = new System.Windows.Shapes.Path();
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
                Opacity = 0.01,
                Visibility = Visibility.Hidden
            };
            // добавить в контейнер
            Grid.SetRow(brdDescrText, 2); dGrid.Children.Add(brdDescrText);
            TextBlock tbDescrText = new TextBlock()
            {
                Name = "descrText",
                Width = dGrid.Width,
                Opacity = 0.01,
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
            // добавить в контейнер
            Grid.SetRow(tbDescrText, 2); dGrid.Children.Add(tbDescrText);
        }

        private void CanvDescr_PreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            Border btnDescr = (Border)sender;
            Grid parentGrid = (Grid)btnDescr.Parent;

            int tagVal = System.Convert.ToInt32(btnDescr.Tag ?? 0);
            tagVal = (tagVal == 0) ? 1 : 0;
            btnDescr.Tag = tagVal;

            // цвет фона кнопки описания блюда
            btnDescr.Background = (tagVal == 0) ? Brushes.White : _brushSelectedItem;

            // видимость описания
            var descrTextBorder = AppLib.GetUIElementFromPanel(parentGrid, "descrTextBorder");
            var descrText = AppLib.GetUIElementFromPanel(parentGrid, "descrText");
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
                    _daDishDescrBackgroundOpacity.To = 0.01;
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
                    _daCommon1.To = 20; _daCommon2.To = 0.01;
                }
                _curDescrTextBlock.Effect.BeginAnimation(BlurEffect.RadiusProperty, _daCommon1);
                _curDescrTextBlock.BeginAnimation(TextBlock.OpacityProperty, _daCommon2);
            }
        }

        private void _daDishDescrBackground_Completed(object sender, EventArgs e)
        {
            _curDescrBorder.Visibility = Visibility.Hidden;
            _curDescrTextBlock.Visibility = Visibility.Hidden;
        }


        private void checkDBConnection()
        {
            Configuration con = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);

            string connectionName = null;
            string connectionString = null;
            foreach (ConnectionStringSettings item in ConfigurationManager.ConnectionStrings)
            {
                if (item.ProviderName == "System.Data.EntityClient")
                {
                    connectionString = item.ConnectionString;
                    int i = connectionString.IndexOf("connection string=", 0);
                    connectionString = connectionString.Substring(i+19);
                    connectionString = connectionString.Substring(0, connectionString.Length - 1);
                    connectionName = item.Name; break;
                }
            }
            if (connectionName == null)
            {
                throw new Exception("Cannot find EntityClient connection string in application config file.");
            }

            SqlConnection conn = new SqlConnection(connectionString);
            conn.Open();
            if (conn.State == ConnectionState.Open)
            {
                conn.Close();
            }
            else
            {
                throw new Exception("Cannot open EF connection " + connectionName + " by its connection string: " + connectionString);
            }
        }

        private void initCurrentSettings(NoodleDContext db)
        {
            // получить настройки приложения из таблицы Setting
            List<Setting> setList = db.Setting.ToList();
            foreach (Setting item in setList)
            {
                AppLib.SetAppGlobalValue(item.UniqName, item.Value);
                if (item.UniqName.EndsWith("Color") == true)  // преобразовать и переопределить цвета
                {
                    convertAppColor(item.UniqName);
                    checkAppColor(item.UniqName);
                }
                if (item.Value == "StringValue")    // заменить в Application.Properties строку StringValue на словарь языковых строк
                {
                    Dictionary<string, string> d = MenuLib.getLangTextDict(item.RowGUID, 1);
                    AppLib.SetAppGlobalValue(item.UniqName, d);
                }
            }

            // надписи на языковых кнопках
            lblLangUa.Text = (string)AppLib.GetAppGlobalValue("langButtonTextUa");
            lblLangRu.Text = (string)AppLib.GetAppGlobalValue("langButtonTextRu");
            lblLangEng.Text = (string)AppLib.GetAppGlobalValue("langButtonTextEn");
        }

        // преобразование строки цветов (R,G,B) в SolidColorBrush
        private void convertAppColor(string setName)
        {
            var buf = AppLib.GetAppGlobalValue(setName);
            if ((buf is string) == false) return;

            // если цвет задан строкой
            string sBuf = (string)buf;
            string[] sRGB = sBuf.Split(',');
            byte r= 0, g = 0, b = 0;
            byte.TryParse(sRGB[0], out r);
            byte.TryParse(sRGB[1], out g);
            byte.TryParse(sRGB[2], out b);
            SolidColorBrush brush = new SolidColorBrush(new Color() { A=255, R=r, G=g, B=b });

            AppLib.SetAppGlobalValue(setName, brush);
        }

        // установка цвета ресурса приложения (Application.Resources) в цвет из свойств приложения (Application.Properties)
        private void checkAppColor(string setName)
        {
            SolidColorBrush bRes= (SolidColorBrush)Application.Current.Resources[setName];
            SolidColorBrush bProp = (SolidColorBrush)AppLib.GetAppGlobalValue(setName);

            if (bRes.Color.Equals(bProp.Color) == false)  // если не равны
            {
                Application.Current.Resources[setName] = bProp;   // то переопределить ресурсную кисть
            }
        }

        #region language bottons

        private void lblButtonLang_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.StylusDevice != null) return;

            selectAppLang((FrameworkElement)sender);
        }
        private void lblButtonLang_TouchDown(object sender, TouchEventArgs e)
        {
            selectAppLang((FrameworkElement)sender);
        }

        // установить язык по умолчанию или по имени элемента, или по Ид языка
        private void selectAppLang(FrameworkElement langControl)
        {
            // сохранить выбранный пункт меню
            int selMenuItem = lstMenuFolders.SelectedIndex;
            AppLib.SetAppGlobalValue("selectedMenuIndex", lstMenuFolders.SelectedIndex);

            if (langControl == null)  // установка языка из глобального свойства
            {
                langControl = getLangButton(AppLib.AppLang);
                if (langControl == null) return;
            }
            else            // установка языка из имени нажатой кнопки
            {
                setCheckedLangButton(false);
                string langId = getLangIdByButtonName(langControl.Name);
                AppLib.AppLang = langId;
            }

            // установка текстов на выбранном языке
            setCheckedLangButton(true);

            BindingExpression be = txtPromoCode.GetBindingExpression(TextBox.TextProperty);
            be.UpdateTarget();
            be = lblMakeOrderText.GetBindingExpression(TextBlock.TextProperty);
            be.UpdateTarget();

            lstMenuFolders.Items.Refresh();
//            lstDishes.Items.Refresh();

            // сбросить выбор блюда
            clearSelectedDish();
            
            // восстановить выбранный пункт главного меню
            if (selMenuItem >= 0) selMenuItem = 0;
            lstMenuFolders.SelectedIndex = (int)(AppLib.GetAppGlobalValue("selectedMenuIndex")??0);
        }

        private void setCheckedLangButton(bool checkedMode)
        {
            Border langBorder = getInnerLangBorder();
            if (langBorder != null)
                langBorder.Style = (checkedMode) ? (Style)this.Resources["langButtonBorderCheckedStyle"] : (Style)this.Resources["langButtonBorderUncheckedStyle"];
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

        // боковое меню выбора категории блюд
        private void lstMenuFolders_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
//            clearSelectedDish();
            e.Handled = true;
            scrollDishes.Content = _dishCanvas[lstMenuFolders.SelectedIndex];

            // очистить привязку изображения  в списке блюд
            //ListBoxItem container;
            //IEnumerable<FrameworkElement> visItems;
            //Image img;

            //if (lstDishes.ItemsSource != null)
            //{
            //    foreach (var item in lstDishes.Items)
            //    {
            //        container = (ListBoxItem)lstDishes.ItemContainerGenerator.ContainerFromItem(item);
            //        if (container != null)
            //        {
            //            IEnumerable<FrameworkElement> cpArr = AppLib.FindVisualChildren<FrameworkElement>(container);
            //            foreach (FrameworkElement fe in cpArr)
            //            {
            //                if (fe.Name == "rectDishImage")
            //                {
            //                    Rectangle rect = (fe as Rectangle);
            //                    rect.Fill.ClearValue(ImageBrush.ImageSourceProperty);
            //                }
            //                else if (fe.Name == "dishHeaderMarkImage")
            //                {
            //                    img = (Image)fe;
            //                    img.ClearValue(Image.SourceProperty);
            //                }
            //            }
            //        }
            //    }
            //}

            //lstDishes.ClearValue(ListBox.ItemsSourceProperty);
            //lstDishes.ItemsSource = null;

            //GC.Collect();
            //GC.WaitForPendingFinalizers();

//            lstDishes.ItemsSource = ((AppModel.MenuItem)lstMenuFolders.SelectedItem).Dishes;

           scrollDishes.ScrollToTop();
        }

        #region кнопки гарниров
        // приходит Grid
        private void borderGarnish_MouseUp(object sender, MouseButtonEventArgs e)
        {
            if (e.StylusDevice != null) return;
            if ((lastDragPoint != null) && lastDragPoint.Equals(initDragPoint) == false) { lastDragPoint = null; return; }

            borderGarnisgHandler(sender);
        }
        private void brdGarnish_TouchDown(object sender, TouchEventArgs e)
        {
            if ((lastDragPoint != null) && lastDragPoint.Equals(initDragPoint) == false) { lastDragPoint = null; return; }

            borderGarnisgHandler(sender);
        }

        // клик по гарниру
        private void borderGarnisgHandler(object sender)
        {
            //// визуальные элементы
            //FrameworkElement gridGarn = (FrameworkElement)sender;   // кликнутый грид
            //int garnIndex = int.Parse(gridGarn.Uid);  // его индекс
            //// изменяемые элементы
            //System.Windows.Shapes.Path borderGarn = AppLib.FindLogicalChildren<System.Windows.Shapes.Path>(gridGarn).ToList()[0];  // кликнутый бордер
            //TextBlock garnName = AppLib.FindLogicalChildren<TextBlock>(gridGarn).ToList()[0];

            //Grid gridGarnAll = (Grid)VisualTreeHelper.GetParent(gridGarn);   // родительский грид, в котором все три гарнира
            //IEnumerable<Viewbox> bordersGarn = AppLib.FindVisualChildren<Viewbox>(gridGarnAll).ToList();  // все бордеры гарниров
            
            //// родительский грид
            //Grid gridPar = (Grid)LogicalTreeHelper.GetParent(gridGarnAll); 
            //// грид с большими кнопками
            //Grid gridBigBut = (Grid)(AppLib.FindLogicalChildren<Grid>(gridPar).First(g => g.Name== "gridDishBottomButtons"));  
            //// бордер кнопки добавления блюда
            //Border borderAddBut = AppLib.FindVisualChildren<Border>(gridBigBut).First(g => g.Name=="txtDishWithIngr");  

            //if (_curDishItem == null)
            //{
            //    _curDishItem = (DishItem)lstDishes.SelectedItem;
            //    if (_curDishItem.SelectedGarnishes == null) _curDishItem.SelectedGarnishes = new List<DishAdding>();

            //    DishAdding da = _curDishItem.Garnishes[garnIndex];
            //    da.Uid = gridGarn.Uid;
            //    _curDishItem.SelectedGarnishes.Add(da);

            //    _curGarnishBorder = borderGarn;
            //    _curGarnishTextBlock = garnName;
            //    _curAddButton = borderAddBut;

            //    updateVisualGarnish(true);
            //}
            //else
            //{
            //    if ((_curDishItem == (DishItem)lstDishes.SelectedItem) && (_curDishItem.SelectedGarnishes != null))  // клик по гарниру в том же блюде
            //    {
            //        DishAdding da = _curDishItem.SelectedGarnishes.FirstOrDefault(g => g.Uid == gridGarn.Uid);
            //        if (da == null)
            //        {
            //            if (_curDishItem.SelectedGarnishes.Count > 0)
            //            {
            //                _curDishItem.SelectedGarnishes.Clear();
            //                updateVisualGarnish(false);
            //            }

            //            da = _curDishItem.Garnishes[garnIndex];
            //            da.Uid = gridGarn.Uid;
            //            _curDishItem.SelectedGarnishes.Add(da);

            //            _curGarnishBorder = borderGarn;
            //            _curGarnishTextBlock = garnName;
            //            updateVisualGarnish(true);
            //        }
            //        else
            //        {
            //            _curDishItem.SelectedGarnishes.Clear();
            //            updateVisualGarnish(true);
            //        }
            //    }
            //    else  // клик по гарниру в другом блюде 
            //    {
            //        if ((_curDishItem.SelectedGarnishes != null) && _curDishItem.SelectedGarnishes.Count > 0)
            //        {
            //            _curDishItem.SelectedGarnishes.Clear();
            //            updateVisualGarnish(true);
            //        }

            //        _curDishItem = (DishItem)lstDishes.SelectedItem;
            //        if (_curDishItem.SelectedGarnishes == null) _curDishItem.SelectedGarnishes = new List<DishAdding>();

            //        DishAdding da = _curDishItem.Garnishes[garnIndex];
            //        da.Uid = gridGarn.Uid;
            //        _curDishItem.SelectedGarnishes.Add(da);

            //        _curGarnishBorder = borderGarn;
            //        _curGarnishTextBlock = garnName;
            //        _curAddButton = borderAddBut;

            //        updateVisualGarnish(true);

            //    }
            //}
        }
        private void updateVisualGarnish(bool isUpdAddBut)
        {
            if ((_curGarnishBorder == null) || (_curGarnishTextBlock == null) || (_curAddButton == null)) return;

            SolidColorBrush selBase = (SolidColorBrush)AppLib.GetAppGlobalValue("appSelectedItemColor");
            SolidColorBrush notSelBase = (SolidColorBrush)AppLib.GetAppGlobalValue("selectGarnishBackgroundColor");
            SolidColorBrush notSelText = (SolidColorBrush)AppLib.GetAppGlobalValue("appNotSelectedItemColor");

            List<DishAdding> garList = _curDishItem.SelectedGarnishes;  // SelectedGarnishes
            if (garList==null || garList.Count == 0)  // ничего не выбрано
            {
                _curGarnishBorder.Fill = notSelBase;
                _curGarnishTextBlock.Foreground = notSelText;
                if (isUpdAddBut)
                {
                    _curAddButton.Visibility = Visibility.Hidden;
                }
            }
            else
            {
                _curGarnishBorder.Fill = selBase;
                _curGarnishTextBlock.Foreground = Brushes.Black;
                if (isUpdAddBut)
                {
                    _curAddButton.Visibility = Visibility.Visible;
                }
            }
        }

        #endregion

        #region выбор блюда
        private void txtDishAdd_MouseDown(object sender, MouseButtonEventArgs e)
        {
//            if (e.StylusDevice != null) return;
            if ((lastDragPoint != null) && lastDragPoint.Equals(initDragPoint) == false) { lastDragPoint = null; return; }

            txtDishWithIngrHandler(sender);
        }
        private void txtDishAdd_TouchDown(object sender, TouchEventArgs e)
        {
            if ((lastDragPoint != null) && lastDragPoint.Equals(initDragPoint) == false) { lastDragPoint = null; return; }

            txtDishWithIngrHandler(sender);
        }

        // клик по кнопке
        private void txtDishWithIngrHandler(object sender)
        {
            //DishItem selDishItem = (DishItem)lstDishes.SelectedItem;
            //if (selDishItem == null) return;

            //// если нет ингредиентов, то сразу в корзину
            //if ((selDishItem.Ingredients == null) || (selDishItem.Ingredients.Count == 0))
            //{
            //    DishItem orderDish = selDishItem.GetCopyForOrder();
            //    _currentOrder.Dishes.Add(orderDish);

            //    // снять выделение
            //    this.clearSelectedDish();

            //    // нарисовать путь
            //    FrameworkElement dishPanel = AppLib.FindLogicalParentByName((FrameworkElement)sender, "gridDish", 4);
            //    if (dishPanel != null)
            //    {
            //        System.Windows.Shapes.Path animPath = getAnimPath(dishPanel);

            //        //cnvAnim.Children.Add(animPath);

            //        //ColorAnimation colorAnim = new ColorAnimation(Colors.Aqua, Colors.Red, TimeSpan.FromMilliseconds(2000));
            //        //colorAnim.Completed += ColorAnim_Completed;
            //        //spotTo.Fill.BeginAnimation(SolidColorBrush.ColorProperty, colorAnim);
                    
            //        //Storyboard sb = new Storyboard();
            //        //PropertyPath colorTargetPath = new PropertyPath("(Ellipse.Fill).(SolidColorBrush.Color)");
            //        //Storyboard.SetTarget(colorAnim, spotTo);
            //        //Storyboard.SetTargetProperty(colorAnim, colorTargetPath);
            //        //sb.Children.Add(colorAnim);
            //        //sb.AutoReverse = true;
            //        //sb.Begin();

            //    }

            //    // и обновить стоимость заказа
            //    updatePrice();
            //}
            //else
            //{
            //    // иначе через "всплывашку"
            //    DishPopup popupWin = new DishPopup(_curDishItem);
            //    // размеры
            //    FrameworkElement pnlClient = this.Content as FrameworkElement;
            //    popupWin.Height = pnlClient.ActualHeight;
            //    popupWin.Width = pnlClient.ActualWidth;
            //    // и положение
            //    Point p = this.PointToScreen(new Point(0, 0));
            //    popupWin.Left = p.X;
            //    popupWin.Top = p.Y;

            //    popupWin.ShowDialog();
            //}
        }

        private void ColorAnim_Completed(object sender, EventArgs e)
        {
            cnvAnim.Children.Clear();
            Panel.SetZIndex(cnvAnim, -1);
        }
        private System.Windows.Shapes.Path getAnimPath(FrameworkElement dishPanel)
        {
            Point fromCenterPoint, toCenterPoint;
            Point fromBasePoint = dishPanel.PointToScreen(new Point(0, 0));
            Size fromSize = dishPanel.RenderSize;
            fromCenterPoint = new Point(fromBasePoint.X + fromSize.Width / 2.0, fromBasePoint.Y + fromSize.Height / 2.0);

            Point toBasePoint = brdMakeOrder.PointToScreen(new Point(0, 0));
            Size toSize = brdMakeOrder.RenderSize;
            toCenterPoint = new Point(toBasePoint.X + toSize.Width / 2.0, toBasePoint.Y + toSize.Height / 2.0);

            //Panel.SetZIndex(cnvAnim, 10);
            Ellipse spotFrom = new Ellipse();
            spotFrom.Height = 50; spotFrom.Width = 50;
            spotFrom.Fill = new SolidColorBrush(Colors.Aqua);
            //cnvAnim.Children.Add(spotFrom);
            Canvas.SetLeft(spotFrom, fromCenterPoint.X - spotFrom.Width / 2f);
            Canvas.SetTop(spotFrom, fromCenterPoint.Y - spotFrom.Height / 2f);

            Ellipse spotTo = new Ellipse();
            spotTo.Height = 50; spotTo.Width = 50;
            spotTo.Fill = new SolidColorBrush(Colors.Aqua);
            //cnvAnim.Children.Add(spotTo);
            Canvas.SetLeft(spotTo, toCenterPoint.X - spotTo.Width / 2f);
            Canvas.SetTop(spotTo, toCenterPoint.Y - spotTo.Height / 2f);

            PathGeometry path = new PathGeometry();
            PathFigure pathFigure = new PathFigure();
            pathFigure.StartPoint = fromCenterPoint;
            pathFigure.IsClosed = false;

            double dX = fromCenterPoint.X - toCenterPoint.X;
            double dY = toCenterPoint.Y - fromCenterPoint.Y;
            Point p1 = new Point(fromCenterPoint.X - 0.3 * dX, fromCenterPoint.Y - 0.3 * dY);
            Point p2 = new Point(toCenterPoint.X + 0.05 * dX, toCenterPoint.Y - 0.8 * dY);
            BezierSegment curve = new BezierSegment(p1, p2, toCenterPoint, true);
            pathFigure.Segments.Add(curve);
            path.Figures.Add(pathFigure);

            System.Windows.Shapes.Path p = new System.Windows.Shapes.Path();
            p.Stroke = Brushes.Red;
            p.StrokeThickness = 4f;
            p.Data = path;

            return p;
        }
        #endregion

        #region btnShowDishDescriptionHandler
        private void btnShowDishDescription_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if ((lastDragPoint != null) && lastDragPoint.Equals(initDragPoint) == false) { lastDragPoint = null; return; }
            
            btnShowDishDescriptionHandler(sender);
        }

        private void btnShowDishDescriptionHandler(object sender)
        {
            FrameworkElement vBox = (FrameworkElement)sender;
            if (vBox.Name != "vboxDishDescrButton")
            {
                FrameworkElement gridDescr = AppLib.FindLogicalParentByName(vBox, "gridDescr", 3);
                Viewbox[] vbArr = AppLib.FindLogicalChildren<Viewbox>(gridDescr).ToArray();
                Viewbox vbText = vbArr[0];
                Viewbox vbButton = vbArr[1];

                vBox = vbButton;
            }

            string tagValue = (vBox.Tag ?? "0").ToString();
            vBox.Tag = (tagValue == "0") ? "1" : "0";
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
            Debug.Print(posNow.ToString());
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

        private void Window_Closed(object sender, EventArgs e)
        {
            logger.Info("End application");
        }

        // сбросить выбор блюда
        public void clearSelectedDish()
        {
            if (_curDishItem != null)
            {
                if (_curDishItem.SelectedGarnishes != null && (_curDishItem.SelectedGarnishes.Count > 0))
                {
                    _curDishItem.SelectedGarnishes = null;
                    updateVisualGarnish(true);
                }
                if (_curDishItem.SelectedIngredients != null) _curDishItem.SelectedIngredients.Clear();
                if (_curDishItem.SelectedRecommends != null) _curDishItem.SelectedRecommends.Clear();

                _curDishItem = null; _curGarnishBorder = null; _curAddButton = null;

            }
        }
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
