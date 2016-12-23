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
using System.IO;
using System.Collections.ObjectModel;
using NLog;
using System.Data.SqlClient;
using System.Data;
using System.Diagnostics;

namespace WpfClient
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private static Logger logger = LogManager.GetCurrentClassLogger();

        // runtime
        private DishItem _curDishItem;
        // visual elements
        System.Windows.Shapes.Path _curGarnishBorder;
        TextBlock _curGarnishTextBlock;
        Border _curAddButton, _curAddButtonShadow;
        SolidColorBrush brushBlack;

        // dragging
        Point? lastDragPoint, initDragPoint;
        protected DateTime _dateTime;
        bool _isTouchHandled;

        // static constants
        static double screenWidth, screenHeight;
        static double dishPanelWidth, dishPanelHeight1, dishPanelHeight2, dishPanelMargin;
        static double dishPanelHeaderFontSize, dishPanelTextFontSize;
        static double dishPanelDescrButtonSize;
        static double dishPanelHeaderRowHeight;
        static double dishPanelImageRowHeight, dishPanelImageCornerRadius;
        static double dishPanelGarnishesRowHeight, dishPanelGarnishBaseWidth;
        static double dishPanelAddButtonRowHeight, dishPanelAddButtonHeight;
        static double dishPanelAddButtonShadowDepth, dishPanelAddButtonShadowBlurRadius, dishPanelAddButtonShadowCornerRadius;
        static double dishPanelRowMargin1, dishPanelRowMargin2;

        static MainWindow()
        {
            screenWidth = SystemParameters.PrimaryScreenWidth;
            //            screenWidth = SystemParameters.VirtualScreenWidth;
            screenHeight = SystemParameters.PrimaryScreenHeight;
            //            screenHeight = SystemParameters.VirtualScreenHeight;

            double dishesPanelWidth = (screenWidth / 6.0 * 5.0);
            AppLib.SetAppGlobalValue("dishesPanelWidth", dishesPanelWidth);

            //  РАЗМЕРЫ ПАНЕЛИ БЛЮДА
            // расчет ширины панели блюда
            dishPanelWidth = 0.95 * dishesPanelWidth / 3.18;  // 3x + 6*0.03x - ширина панелей + отступ слева/справа 
            AppLib.SetAppGlobalValue("dishPanelWidth", dishPanelWidth);
            // расстояние между панелями
            dishPanelMargin = 0.03 * dishPanelWidth;
            AppLib.SetAppGlobalValue("dishPanelMargin", dishPanelMargin);
            // высота панелей
            dishPanelHeight1 = dishPanelWidth * 1.5;
            AppLib.SetAppGlobalValue("dishPanelHeight1", dishPanelHeight1);
            dishPanelHeight2 = dishPanelWidth * 1.3;
            AppLib.SetAppGlobalValue("dishPanelHeight2", dishPanelHeight2);
            // высота строки заголовка
            dishPanelHeaderRowHeight = 0.15 * dishPanelWidth;
            AppLib.SetAppGlobalValue("dishPanelHeaderRowHeight", dishPanelHeaderRowHeight);
            // высота строки изображения
            dishPanelImageRowHeight = 0.83 * dishPanelWidth;
            AppLib.SetAppGlobalValue("dishPanelImageRowHeight", dishPanelImageRowHeight);
            // радиус загругления уголков изображения
            dishPanelImageCornerRadius = 0.05 * dishPanelWidth;
            AppLib.SetAppGlobalValue("dishPanelImageCornerRadius", dishPanelImageCornerRadius);
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
        }

        public MainWindow()
        {
            InitializeComponent();

            appInit();
        }

        private void appInit()
        {
            //this.Cursor = Cursors.None;
            //Mouse.OverrideCursor = Cursors.None;

            //TestData.mainProc();

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

                    // получить язык UI
                    string langId = (string)AppLib.GetAppGlobalValue("langButtonDefaultId");
                    AppLib.AppLang = langId;

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

            if (mFolders != null) initUI(mFolders);

        }

        private void initUI(ObservableCollection<AppModel.MenuItem> mFolders)
        {
            logger.Trace("Настраиваю визуальные элементы...");

            // добавить к блюдам надписи на кнопках
            Dictionary<string, string> langSelGarnishDict = (Dictionary<string, string>)AppLib.GetAppGlobalValue("btnSelectGarnishText");
            Dictionary<string, string> langAddDishDict = (Dictionary<string, string>)AppLib.GetAppGlobalValue("btnSelectDishText");
            foreach (AppModel.MenuItem menuItem in mFolders)
            {
                foreach (DishItem dishItem in menuItem.Dishes)
                {
                    dishItem.langBtnSelGarnishText = langSelGarnishDict;
                    dishItem.langBtnAddDishText = langAddDishDict;
                }
            }
            brushBlack = new SolidColorBrush(Colors.Black);

            AppLib.SetAppGlobalValue("mainMenu", mFolders);
            lstMenuFolders.ItemsSource = mFolders;
            lstMenuFolders.SelectedIndex = 0;

            // установить язык UI
            selectAppLang(null);

            // создать текущий заказ
            OrderItem curOrder = new OrderItem();
            AppLib.SetAppGlobalValue("currentOrder", curOrder);

            logger.Trace("Настраиваю визуальные элементы - READY");

            // DEBUG Cart
            //DishItem curDish = mFolders[0].Dishes[6];
            //// блюдо с гарнирами
            //curDish.SelectedGarnishes.Add(curDish.Garnishes[1]);
            //// и ингредиентами
            //curDish.SelectedIngredients = new List<DishAdding>();
            //curDish.SelectedIngredients.Add(curDish.Ingredients[0]);
            //curDish.SelectedIngredients.Add(curDish.Ingredients[1]);
            //curDish.SelectedIngredients.Add(curDish.Ingredients[2]);
            //curDish.SelectedIngredients.Add(curDish.Ingredients[3]);

            //DishItem orderDish = curDish.GetCopyForOrder();
            //curOrder.Dishes.Add(orderDish);

            //curOrder.Dishes.Add(mFolders[3].Dishes[0].GetCopyForOrder());
            //curOrder.Dishes.Add(mFolders[3].Dishes[0].GetCopyForOrder());
            //curOrder.Dishes.Add(mFolders[3].Dishes[0].GetCopyForOrder());
            //curOrder.Dishes.Add(mFolders[3].Dishes[0].GetCopyForOrder());
            //curOrder.Dishes.Add(mFolders[3].Dishes[0].GetCopyForOrder());
            //curOrder.Dishes.Add(mFolders[3].Dishes[0].GetCopyForOrder());

            //updatePrice();
            //showCartWindow();

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
            lstDishes.Items.Refresh();

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
            clearSelectedDish();
            //lstDishes.ItemsSource = ((AppModel.MenuItem)lstMenuFolders.SelectedItem).Dishes;
            
            scrollDishes.ScrollToTop();
            if (lstDishes.Items.Count > 0) lstDishes.ScrollIntoView(lstDishes.Items[0]);
            e.Handled = true;
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
            // визуальные элементы
            FrameworkElement gridGarn = (FrameworkElement)sender;   // кликнутый грид
            int garnIndex = int.Parse(gridGarn.Uid);  // его индекс
            // изменяемые элементы
            System.Windows.Shapes.Path borderGarn = AppLib.FindLogicalChildren<System.Windows.Shapes.Path>(gridGarn).ToList()[0];  // кликнутый бордер
            TextBlock garnName = AppLib.FindLogicalChildren<TextBlock>(gridGarn).ToList()[0];

            Grid gridGarnAll = (Grid)LogicalTreeHelper.GetParent(gridGarn);   // родительский грид, в котором все три гарнира
            IEnumerable<Viewbox> bordersGarn = AppLib.FindVisualChildren<Viewbox>(gridGarnAll).ToList();  // все бордеры гарниров
            
            // родительский грид
            Grid gridPar = (Grid)LogicalTreeHelper.GetParent(gridGarnAll); 
            // грид с большими кнопками
            Grid gridBigBut = (Grid)(AppLib.FindLogicalChildren<Grid>(gridPar).First(g => g.Name== "gridDishBottomButtons"));  
            // бордер кнопки добавления блюда
            Border borderAddBut = AppLib.FindVisualChildren<Border>(gridBigBut).First(g => g.Name=="txtDishWithIngr");  
            // бордер тени кнопки добавления блюда
            Border borderAddButShadow = AppLib.FindVisualChildren<Border>(gridBigBut).First(g => g.Name== "shadowedBorder");  

            if (_curDishItem == null)
            {
                _curDishItem = (DishItem)lstDishes.SelectedItem;
                if (_curDishItem.SelectedGarnishes == null) _curDishItem.SelectedGarnishes = new List<DishAdding>();

                DishAdding da = _curDishItem.Garnishes[garnIndex];
                da.Uid = gridGarn.Uid;
                _curDishItem.SelectedGarnishes.Add(da);

                _curGarnishBorder = borderGarn;
                _curGarnishTextBlock = garnName;
                _curAddButton = borderAddBut;
                _curAddButtonShadow = borderAddButShadow;

                updateVisualGarnish(true);
            }
            else
            {
                if ((_curDishItem == (DishItem)lstDishes.SelectedItem) && (_curDishItem.SelectedGarnishes != null))  // клик по гарниру в том же блюде
                {
                    DishAdding da = _curDishItem.SelectedGarnishes.FirstOrDefault(g => g.Uid == gridGarn.Uid);
                    if (da == null)
                    {
                        if (_curDishItem.SelectedGarnishes.Count > 0)
                        {
                            _curDishItem.SelectedGarnishes.Clear();
                            updateVisualGarnish(false);
                        }

                        da = _curDishItem.Garnishes[garnIndex];
                        da.Uid = gridGarn.Uid;
                        _curDishItem.SelectedGarnishes.Add(da);

                        _curGarnishBorder = borderGarn;
                        _curGarnishTextBlock = garnName;
                        updateVisualGarnish(true);
                    }
                    else
                    {
                        _curDishItem.SelectedGarnishes.Clear();
                        updateVisualGarnish(true);
                    }
                }
                else  // клик по гарниру в другом блюде 
                {
                    if ((_curDishItem.SelectedGarnishes != null) && _curDishItem.SelectedGarnishes.Count > 0)
                    {
                        _curDishItem.SelectedGarnishes.Clear();
                        updateVisualGarnish(true);
                    }

                    _curDishItem = (DishItem)lstDishes.SelectedItem;
                    if (_curDishItem.SelectedGarnishes == null) _curDishItem.SelectedGarnishes = new List<DishAdding>();

                    DishAdding da = _curDishItem.Garnishes[garnIndex];
                    da.Uid = gridGarn.Uid;
                    _curDishItem.SelectedGarnishes.Add(da);

                    _curGarnishBorder = borderGarn;
                    _curGarnishTextBlock = garnName;
                    _curAddButton = borderAddBut;
                    _curAddButtonShadow = borderAddButShadow;

                    updateVisualGarnish(true);

                }
            }
        }
        private void updateVisualGarnish(bool isUpdAddBut)
        {
            if ((_curGarnishBorder == null) || (_curGarnishTextBlock == null) || (_curAddButton == null)) return;

            SolidColorBrush selBase = (SolidColorBrush)AppLib.GetAppGlobalValue("appSelectedItemColor");
            SolidColorBrush notSelBase = (SolidColorBrush)AppLib.GetAppGlobalValue("appBackgroundColor");
            SolidColorBrush notSelText = (SolidColorBrush)AppLib.GetAppGlobalValue("appNotSelectedItemColor");

            List<DishAdding> garList = _curDishItem.SelectedGarnishes;  // SelectedGarnishes
            if (garList==null || garList.Count == 0)  // ничего не выбрано
            {
                _curGarnishBorder.Fill = notSelBase;
                _curGarnishTextBlock.Foreground = notSelText;
                if (isUpdAddBut)
                {
                    _curAddButton.Visibility = Visibility.Hidden;
                    _curAddButtonShadow.Visibility = Visibility.Hidden;
                }
            }
            else
            {
                _curGarnishBorder.Fill = selBase;
                _curGarnishTextBlock.Foreground = brushBlack;
                if (isUpdAddBut)
                {
                    _curAddButton.Visibility = Visibility.Visible;
                    _curAddButtonShadow.Visibility = Visibility.Visible;
                }
            }
        }

        #endregion

        #region выбор блюда
        private void txtDishAdd_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.StylusDevice != null) return;
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
            DishItem selDishItem = (DishItem)lstDishes.SelectedItem;
            if (selDishItem == null) return;

            // если нет ингредиентов, то сразу в корзину
            if ((selDishItem.Ingredients == null) || (selDishItem.Ingredients.Count == 0))
            {
                OrderItem curOrder = (OrderItem)AppLib.GetAppGlobalValue("currentOrder");
                DishItem orderDish = selDishItem.GetCopyForOrder();
                curOrder.Dishes.Add(orderDish);

                // снять выделение
                this.clearSelectedDish();
                // и обновить стоимость заказа
                updatePrice();
            }
            else
            {
                // иначе через "всплывашку"
                DishPopup popupWin = new DishPopup();
                // размеры
                FrameworkElement pnlClient = this.Content as FrameworkElement;
                popupWin.Height = pnlClient.ActualHeight;
                popupWin.Width = pnlClient.ActualWidth;
                // и положение
                Point p = this.PointToScreen(new Point(0, 0));
                popupWin.Left = p.X;
                popupWin.Top = p.Y;

                // установить контекст окна - текущий DishItem
                popupWin.DataContext = _curDishItem;  // контекст данных

                popupWin.ShowDialog();
            }
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
            FrameworkElement gridDescr = AppLib.FindLogicalParentByName(vBox, "dishDescrText", 3);

            Viewbox[] vbArr = AppLib.FindLogicalChildren<Viewbox>(gridDescr).ToArray();
            Viewbox vbText = vbArr[0];
            Viewbox vbButton = vbArr[1];
            System.Windows.Shapes.Path path = AppLib.FindLogicalChildren<System.Windows.Shapes.Path>(vbButton).First();

            int tagValue = (int)(vbButton.Tag ?? 0);   // переключатель в теге кнопки
            vbButton.Tag = (tagValue == 0) ? 1 : 0;

            if ((int)vbButton.Tag == 0)
            {
                path.Fill = (SolidColorBrush)AppLib.GetAppGlobalValue("appNotSelectedItemColor");
                vbText.Visibility = Visibility.Collapsed;
            }
            else
            {
                path.Fill = (SolidColorBrush)AppLib.GetAppGlobalValue("appSelectedItemColor");
                vbText.Visibility = Visibility.Visible;
            }
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

            Debug.Print("MouseDown" + ", StylusDevice - " + ((e.StylusDevice==null)?"null":"touchDevice"));
            initDrag(e.GetPosition(scrollDishes));
        }

        private void scrollDishes_PreviewTouchDown(object sender, TouchEventArgs e)
        {
            Debug.Print("TouchDown");
            initDrag(e.GetTouchPoint(scrollDishes).Position);
        }

        private void scrollDishes_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            //if (e.StylusDevice != null) return;

            Debug.Print("-MouseUp" + ", StylusDevice - " + ((e.StylusDevice == null) ? "null" : "touchDevice"));
            endDrag();
        }

        private void scrollDishes_PreviewTouchUp(object sender, TouchEventArgs e)
        {
            Debug.Print("-TouchUp");
            endDrag();
        }

        private void scrollDishes_MouseMove(object sender, MouseEventArgs e)
        {
            //if (e.StylusDevice != null) return;

            if (lastDragPoint.HasValue && e.LeftButton == MouseButtonState.Pressed)   
            {
                Debug.Print("     MouseMove" + ", StylusDevice - " + ((e.StylusDevice == null) ? "null" : "touchDevice"));
                doMove(e.GetPosition(sender as IInputElement));
            }
        }

        private void scrollDishes_PreviewTouchMove(object sender, TouchEventArgs e)
        {
            if (lastDragPoint.HasValue)
            {
                Debug.Print("     TouchMove");
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
            BindingExpression be = this.txtOrderPrice.GetBindingExpression(TextBlock.TextProperty);
            be.UpdateTarget();
        }

        private void btnShowCart_MouseUp(object sender, MouseButtonEventArgs e)
        {
            if (e.StylusDevice != null) return;
            showCartWindow();
        }

        private void btnShowCart_TouchUp(object sender, TouchEventArgs e)
        {
            showCartWindow();
        }

        protected override void OnRender(DrawingContext drawingContext)
        {
            //WrapPanel wp = (WrapPanel)AppLib.FindVisualChildren<WrapPanel>(this.dishItemsBorder).First();
            //wp.Width = (screenWidth / 6.0) * 5.0 * 0.95;

            base.OnRender(drawingContext);
        }


        private void showCartWindow()
        {
            Cart cart = new Cart();

            cart.ShowDialog();
        }

    } // class MainWindow

}
