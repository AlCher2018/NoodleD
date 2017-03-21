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
using AppActionNS;
using System.Configuration;
using System.Data.SqlClient;
using System.Data;
using System.Diagnostics;
using System.Windows.Media.Animation;
using System.Windows.Media.Effects;
using System.Timers;
using System.ComponentModel;

namespace WpfClient
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        // подложка списка блюд
        private List<MainMenuDishesCanvas> _dishCanvas;
        private int _dishColCount;  // кол-во колонок блюд
        // текущий заказ
        private OrderItem _currentOrder = null;
        // visual elements
        private byte _langButtonPress = 0;
        private double _orderPriceFontSize;

        // dish description animations
        private DoubleAnimation _daCommon1, _daCommon2;
        private Storyboard _animDishSelection;
        private ColorAnimation _animOrderPriceBackgroundColor;
        private Effect _orderPriceEffectShadow;
        private Effect _orderPriceEffectBlur;

#if (enableTimer)
        // логгер нажатий
        NLog.Logger _touchLogger = NLog.LogManager.GetLogger("touchTrace");
        TextBlock tbTimer;
        Timer _tmr;
#endif

        // dragging
        private Point? lastDragPoint, initDragPoint;
        private DateTime _dateTime;

        public List<MainMenuDishesCanvas> DishesPanels { get { return _dishCanvas; } }
        public OrderItem CurrentOrder {
            get { return _currentOrder; }
            set { _currentOrder = value; }
        }

        public MainWindow()
        {
            InitializeComponent();

            // вспомогательные окна 
            // создавать только здесь, чтобы в App.Current.Windows главное окно было на ПЕРВОМ месте!!!! - Обязательно!!!
            AppLib.PromoCodeWindow = new Promocode();
            AppLib.TakeOrderWindow = new TakeOrder();
            AppLib.CreateMsgBox();
            AppLib.CreateChoiceBox();

            // для настройки элементов после отрисовки окна
            this.Loaded += MainWindow_Loaded;

            // инициализация локальных переменных
            _dishCanvas = new List<MainMenuDishesCanvas>();
            _dishColCount = AppLib.GetAppGlobalValue("dishesColumnsCount").ToString().ToInt();
            _daCommon1 = new DoubleAnimation();
            _daCommon2 = new DoubleAnimation();

            updatePrice();

            initUI();
        }

        #region активация ожидашки
        protected override void OnActivated(EventArgs e)
        {
            App.IdleHandler.CurrentWindow = this;
        }
        protected override void OnDeactivated(EventArgs e)
        {
            App.IdleHandler.CurrentWindow = null;
        }
        #endregion

        protected override void OnClosing(CancelEventArgs e)
        {
            AppLib.CloseChildWindows(true);

            AppLib.WriteAppAction(this.Name, AppActionsEnum.MainWindowClose);
            base.OnClosing(e);
        }

        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            AppLib.WriteAppAction(this.Name, AppActionsEnum.MainWindowOpen);

            Point pt = PointFromScreen(new Point(1920, 1080));
            AppLib.ScreenScale = 1920.0 / pt.X;

            //   конечный элемент анимации выбора блюда, это Point3 в BezierSegment
            Point toBasePoint = brdMakeOrder.PointToScreen(new Point(0, 0));
            Size toSize = brdMakeOrder.RenderSize;
            Point endPoint = new Point(toBasePoint.X + toSize.Width / 2.0, toBasePoint.Y + toSize.Height / 2.0);
            if (AppLib.ScreenScale != 1d) endPoint = PointFromScreen(endPoint);
            // установить для сегмента анимации конечную точку
            PathFigure pf = (this.animPath.Data as PathGeometry).Figures[0];
            BezierSegment bs = (pf.Segments[0] as BezierSegment);
            bs.Point3 = endPoint;

            string sBuf = AppLib.GetAppSetting("UserIdleTime");
            if ((sBuf != null) && (sBuf != "0"))
            {
                int idleSec = int.Parse(sBuf);
            }

#if (enableTimer)
            menuSidePanelLogo.Children.Remove(imageLogo);
            tbTimer = new TextBlock()
            {
                FontSize=24, FontWeight=FontWeights.Bold, Foreground = Brushes.Yellow, HorizontalAlignment = HorizontalAlignment.Center, VerticalAlignment = VerticalAlignment.Center, Margin = new Thickness(0,10,0,0)
            };
            menuSidePanelLogo.Children.Insert(0, tbTimer);

            //enableEvents(lstMenuFolders);

            _tmr = new Timer();
            _tmr.Elapsed += _tmr_Elapsed;
            _tmr.Interval = 100;
            _tmr.Start();
#endif
        }  // method

#if (enableTimer)
        private void _tmr_Elapsed(object sender, ElapsedEventArgs e)
        {
            this.Dispatcher.Invoke(() => tbTimer.Text = e.SignalTime.ToString("yyyy.MM.dd HH:mm:ss.f00"));
        }

        protected override void OnClosing(CancelEventArgs e)
        {
            base.OnClosing(e);
            _tmr.Stop();
            _tmr.Dispose();
        }
        private void enableEvents(FrameworkElement ctl)
        {
            ctl.PreviewTouchDown += lstEvent; 
//            ctl.PreviewTouchMove += lstEvent;
            ctl.PreviewTouchUp += lstEvent;

            ctl.TouchEnter += lstEvent;
            ctl.TouchDown += lstEvent; 
            ctl.TouchMove += lstEvent;
            ctl.TouchUp += lstEvent;
            ctl.TouchLeave += lstEvent;

            ctl.PreviewMouseDown += lstEvent;
//            ctl.PreviewMouseMove += lstEvent;
            ctl.PreviewMouseUp += lstEvent;

            ctl.MouseEnter += lstEvent;
            ctl.MouseDown += lstEvent;
            ctl.MouseMove += lstEvent;
            ctl.MouseUp += lstEvent;
            ctl.MouseLeave += lstEvent;
        }
        private void lstEvent(object sender, RoutedEventArgs e)
        {
            if (e.RoutedEvent.Name.Equals("PreviewTouchEnter")) outTouchTrace("\n");
            else if (e.RoutedEvent.Name.Equals("TouchEnter")) outTouchTrace("\n");
            else if (e.RoutedEvent.Name.Equals("PreviewMouseEnter")) outTouchTrace("\n");
            else if (e.RoutedEvent.Name.Equals("MouseEnter")) outTouchTrace("\n");

            outTouchTrace(e.RoutedEvent.Name);
        }
        private void outTouchTrace(string msg)
        {
            _touchLogger.Trace(msg);
        }
#endif

        private void initUI()
        {

            setAppLayout();

            AppLib.WriteLogTraceMessage("Настраиваю визуальные элементы...");
            AppLib.AppLang = AppLib.GetAppSetting("langDefault");

            // надписи на языковых кнопках
            lblLangUa.Text = (string)AppLib.GetAppGlobalValue("langButtonTextUa");
            lblLangRu.Text = (string)AppLib.GetAppGlobalValue("langButtonTextRu");
            lblLangEn.Text = (string)AppLib.GetAppGlobalValue("langButtonTextEn");

            // большие кнопки скроллинга
            var v = Enum.Parse(typeof(HorizontalAlignment), (string)AppLib.GetAppGlobalValue("dishesPanelScrollButtonHorizontalAlignment"));
            btnScrollDown.Width = (double)AppLib.GetAppGlobalValue("dishesPanelScrollButtonSize");
            btnScrollDown.Height = (double)AppLib.GetAppGlobalValue("dishesPanelScrollButtonSize");
            btnScrollDown.HorizontalAlignment = (HorizontalAlignment)v;
            btnScrollUp.Width = (double)AppLib.GetAppGlobalValue("dishesPanelScrollButtonSize");
            btnScrollUp.Height = (double)AppLib.GetAppGlobalValue("dishesPanelScrollButtonSize");
            btnScrollUp.HorizontalAlignment = (HorizontalAlignment)v;

            List<AppModel.MenuItem> mFolders = (List<AppModel.MenuItem>)AppLib.GetAppGlobalValue("mainMenu");
            if (mFolders == null) return;

            AppLib.WriteLogTraceMessage(" - создаю списки блюд по категориям...");
            // создать канву со списком блюд
            createDishesCanvas(mFolders);
            AppLib.WriteLogTraceMessage(" - создаю списки блюд по категориям... READY");

            // анимация выбора блюда
            AppLib.WriteLogTraceMessage(" - создаю Объекты анимации выбора блюда...");
            createObjectsForDishAnimation();
            AppLib.WriteLogTraceMessage(" - создаю Объекты анимации выбора блюда... READY");

            lstMenuFolders.Focus();
            lstMenuFolders.ItemsSource = mFolders;
            AppLib.IsEventsEnable = false;  // для предотвращения логирования НЕпользовательского действия
            lstMenuFolders.SelectedIndex = 0;

            // установить язык UI
            selectAppLang(AppLib.AppLang);

            // выключить курсор мыши
            if (AppLib.GetAppSetting("MouseCursor").ToBool() == false)
            {
                this.Cursor = Cursors.None;
                Mouse.OverrideCursor = Cursors.None;
            }

            AppLib.WriteLogTraceMessage("Настраиваю визуальные элементы - READY");
        }

        private void setAppLayout()
        {
            AppLib.WriteLogTraceMessage("Настраиваю дизайн приложения...");

            double screenWidth = (double)AppLib.GetAppGlobalValue("screenWidth");
            double screenHeight = (double)AppLib.GetAppGlobalValue("screenHeight");

            double pnlWidth, pnlHeight;
            double pnlDishWidth, pnlDishHeight;
            double lstFldWidth, lstFldHeight;
            double promoFontSize, dH;

            //clearMenuSideLayout();
            // вертикальное размещение: панель меню сверху
            if (AppLib.IsAppVerticalLayout == true)
            {
                AppLib.WriteLogTraceMessage("\t- дизайн вертикальный");

                // грид меню
                DockPanel.SetDock(gridMenuSide, Dock.Top);

                // панель категорий
                pnlWidth = screenWidth;
                pnlHeight = screenHeight / 6d * 1.2d;
                // панель блюд
                pnlDishWidth = screenWidth;
                pnlDishHeight = screenHeight / 6d * 4.8d;
                lstFldWidth = pnlWidth;

                // грид меню
                gridMenuSide.Height = pnlHeight;
                gridMenuSide.Width = pnlWidth;
                menuSidePanelLogo.Background = new SolidColorBrush(Color.FromRgb(0x62, 0x1C, 0x55));

                // панель меню на всю ширину экрана
                dH = pnlHeight / 10d;
                gridMenuSide.RowDefinitions[0].Height = new GridLength(3.0 * dH);
                gridMenuSide.RowDefinitions[1].Height = new GridLength(4.0 * dH);
                gridMenuSide.RowDefinitions[2].Height = new GridLength(0.0 * dH);
                gridMenuSide.RowDefinitions[3].Height = new GridLength(3.0 * dH);

                // stackPanel для Logo
                menuSidePanelLogo.Orientation = Orientation.Horizontal;
                //   logo
                imageLogo.Height = 1.5 * dH;
                imageLogo.Width = 0.333 * gridMenuSide.Width;
                imageLogo.HorizontalAlignment = HorizontalAlignment.Left;
                imageLogo.Margin = new Thickness(dH, 0, 0, 0);

                //   языковые кнопки
                gridLang.Height = 2.0 * dH;  // необходимо для расчета размера внутренних кнопок
                gridLang.Width = 0.3 * gridMenuSide.Width;
                //gridLang.Background = Brushes.Yellow;
                //gridLang.Margin = new Thickness(0.1 * gridMenuSide.Width, 0, 0.1 * gridMenuSide.Width, 0);
                
                // перенести промокод
                gridMenuSide.Children.Remove(gridPromoCode);
                //gridPromoCode.ColumnDefinitions[3].Width = new GridLength(0.0 * dH);
                menuSidePanelLogo.Children.Add(gridPromoCode);
                gridPromoCode.Width = 0.333 * gridMenuSide.Width;
                gridPromoCode.Height = 1.5 * dH;
//                gridPromoCode.Background = Brushes.Green;
                promoFontSize = 0.5 * dH;
                gridPromoCode.HorizontalAlignment = HorizontalAlignment.Right;

                lstFldHeight = 3d * dH;
                lstMenuFolders.ItemsPanel = GetItemsPanelTemplate(Orientation.Horizontal);
                lstMenuFolders.Margin = new Thickness(dH, 0.5*dH, 0, 0);
                lstFldWidth -= 1.2*dH;
                ScrollViewer.SetHorizontalScrollBarVisibility(lstMenuFolders, ScrollBarVisibility.Auto);
                ScrollViewer.SetVerticalScrollBarVisibility(lstMenuFolders, ScrollBarVisibility.Disabled);

                // кнопка Оформить
                brdMakeOrder.Margin = new Thickness(dH, 0.4*dH, dH, 0.4 * dH);
                brdMakeOrder.CornerRadius = new CornerRadius((double)AppLib.GetAppGlobalValue("cornerRadiusButton"));
                pnlMakeOrder.Orientation = Orientation.Horizontal;
                _orderPriceFontSize = 0.3 * gridMenuSide.RowDefinitions[3].Height.Value;
                lblOrderPrice.FontSize = _orderPriceFontSize;
                lblOrderPrice.FontWeight = FontWeights.Bold;
                lblOrderPrice.Margin = new Thickness(0,0,dH,0);
                lblMakeOrderText.FontSize = 0.8 * _orderPriceFontSize;
                lblMakeOrderText.FontWeight = FontWeights.Normal;

                // фон
                dishesPanelBackground.Source = ImageHelper.GetBitmapImage(@"AppImages\bg 3ver 1080x1920 background.png");
                scrollDishes.Margin = new Thickness(0,0.5*dH,0,0.5*dH);
            }

            // иначе дизайн горизонтальный: меню категорий справа
            else
            {
                AppLib.WriteLogTraceMessage("\t- дизайн горизонтальный");
                DockPanel.SetDock(gridMenuSide, Dock.Left);

                // панель категорий
                pnlWidth = screenWidth / 6d * 1.0d;
                pnlHeight = screenHeight;
                // панель блюд
                pnlDishWidth = screenWidth / 6d * 5.0d;
                pnlDishHeight = screenHeight;
                lstFldWidth = pnlWidth;

                // грид меню
                gridMenuSide.Height = pnlHeight;
                gridMenuSide.Width = pnlWidth;
                menuSidePanelLogo.Background = (Brush)AppLib.GetAppGlobalValue("appBackgroundColor");

                // панель меню на всю высоту экрана
                dH = pnlHeight / 13d;
                gridMenuSide.RowDefinitions[0].Height = new GridLength(2.2 * dH);
                gridMenuSide.RowDefinitions[1].Height = new GridLength(8.0 * dH);
                gridMenuSide.RowDefinitions[2].Height = new GridLength(1.0 * dH);
                gridMenuSide.RowDefinitions[3].Height = new GridLength(1.8 * dH);

                // stackPanel для Logo
                menuSidePanelLogo.Orientation = Orientation.Vertical;
                imageLogo.Height = dH;
                imageLogo.Margin = new Thickness(0.07 * pnlWidth, 0, 0.07 * pnlWidth, 0);

                gridLang.Height = 1.2 * dH; gridLang.Width = gridMenuSide.Width;

                // список категорий
                lstFldHeight = 8d * dH;
                lstMenuFolders.ItemsPanel = GetItemsPanelTemplate(Orientation.Vertical);
                lstMenuFolders.Margin = new Thickness(0, 0.01 * lstFldHeight, 0, 0.01 * lstFldHeight);
                lstFldHeight *= 0.98;
                ScrollViewer.SetVerticalScrollBarVisibility(lstMenuFolders, ScrollBarVisibility.Auto);
                ScrollViewer.SetHorizontalScrollBarVisibility(lstMenuFolders, ScrollBarVisibility.Disabled);

                // промокод
                gridPromoCode.Height = 0.6 * dH;
                gridPromoCode.Margin = new Thickness(0, 0, 0, 0.4 * dH);
                promoFontSize = 0.3 * gridPromoCode.Height;

                // кнопка Оформить
                _orderPriceFontSize = 0.3 * gridMenuSide.RowDefinitions[3].Height.Value;
                lblOrderPrice.FontSize = _orderPriceFontSize;
                lblOrderPrice.FontWeight = FontWeights.Bold;
                lblMakeOrderText.FontSize = 0.2 * gridMenuSide.RowDefinitions[3].Height.Value;
                lblMakeOrderText.FontWeight = FontWeights.Bold;
                
                // фон
                dishesPanelBackground.Source = ImageHelper.GetBitmapImage(@"AppImages\bg 3hor 1920x1080 background.png");

            }

            // яркость фона
            string opacity = AppLib.GetAppSetting("MenuBackgroundBrightness");
            if (opacity != null)
            {
                dishesPanelBackground.Opacity = opacity.ToDouble();
            }

            // создать список категорий блюд
            AppLib.WriteLogTraceMessage(" - создаю список категорий блюд...");
            createCategoriesList(lstFldWidth, lstFldHeight, dH);
            AppLib.WriteLogTraceMessage(" - создаю список категорий блюд... READY");

            // языковые кнопки, фон для внешних Border, чтобы они были кликабельные
            btnLangUa.Background = menuSidePanelLogo.Background;
            btnLangRu.Background = menuSidePanelLogo.Background;
            btnLangEn.Background = menuSidePanelLogo.Background;
            double dMin = Math.Min(gridLang.Height, gridMenuSide.Width / (0.3 + 1.0 + 0.3 + 1.0 + 0.3 + 1.0 + 0.3));
            double dLangSize = Math.Floor(0.7 * dMin);
            setLngInnerBtnSizes(btnLangUaInner, lblLangUa, dLangSize);
            setLngInnerBtnSizes(btnLangRuInner, lblLangRu, dLangSize);
            setLngInnerBtnSizes(btnLangEnInner, lblLangEn, dLangSize);

            txtPromoCode.FontSize = promoFontSize;

            // грид блюд
            gridDishesSide.Height = pnlDishHeight; gridDishesSide.Width = pnlDishWidth;

            AppLib.WriteLogTraceMessage("Настраиваю дизайн приложения... READY");
        }  // method

        private void setLngInnerBtnSizes(Border btnLangInner, TextBlock lblLang, double dLangSize)
        {
            double dMargin = Math.Floor((1.0 - dLangSize) / 2.0) + 1d;
            btnLangInner.Height = dLangSize; btnLangInner.Width = dLangSize;
            btnLangInner.CornerRadius = new CornerRadius(dLangSize / 2.0);
            btnLangInner.Margin = new Thickness(dMargin);

            // размер шрифта на язык.кнопках
            double txtFontSize = 0.35 * dLangSize;
            lblLang.FontSize = txtFontSize;

        }

        private ItemsPanelTemplate GetItemsPanelTemplate(Orientation orientation)
        {
            string xaml = @"<ItemsPanelTemplate xmlns='http://schemas.microsoft.com/winfx/2006/xaml/presentation' xmlns:x='http://schemas.microsoft.com/winfx/2006/xaml'> <StackPanel Orientation = """ + ((orientation == Orientation.Vertical) ? "Vertical" : "Horizontal") + "\"/>                </ItemsPanelTemplate>";
            return System.Windows.Markup.XamlReader.Parse(xaml) as ItemsPanelTemplate;
        }

        private void clearMenuSideLayout()
        {
            Thickness tn = new Thickness(0);
            imageLogo.Margin = tn; gridLang.Margin = tn; lstMenuFolders.Margin = tn;
            //gridPromoCode.Margin = tn;
            brdMakeOrder.Margin = tn;

            Grid.SetColumn(imageLogo, 0); Grid.SetRow(imageLogo, 0);
            Grid.SetColumn(gridLang, 0); Grid.SetRow(gridLang, 0);
            Grid.SetColumn(lstMenuFolders, 0); Grid.SetRow(lstMenuFolders, 0);
            Grid.SetColumn(gridPromoCode, 0); Grid.SetRow(gridPromoCode, 0);
            Grid.SetColumn(brdMakeOrder, 0); Grid.SetRow(brdMakeOrder, 0);

            gridMenuSide.ColumnDefinitions.Clear();
            gridMenuSide.RowDefinitions.Clear();
            //gridMenuSideSub1.ColumnDefinitions.Clear();
            //gridMenuSideSub1.RowDefinitions.Clear();
            //gridMenuSideSub1.Children.Clear();
        }

        private void createCategoriesList(double pnlWidth, double pnlHeight, double dH)
        {
            List<AppModel.MenuItem> mFolders = (List<AppModel.MenuItem>)AppLib.GetAppGlobalValue("mainMenu");
            if (mFolders == null) return;

            bool isVerticalFolderMenu = (pnlWidth < pnlHeight);
            bool isScrollingList = AppLib.GetAppSetting("IsAllowScrollingDishCategories").ToBool();
            // кол-во пунктов для расчета из размера
            int iCount = (isScrollingList == true)? 6: mFolders.Count;
            if (iCount < 6) iCount = 6;

            // стиль содержания пункта меню
            Style brdStyle = (Style)this.Resources["menuItemStyle"];
            Style imgStyle = (Style)this.Resources["menuItemImageStyle"];
            Style txtStyle = (Style)this.Resources["menuItemTextStyle"];
            Setter st;

            double itemHeight, itemWidth, marginBase, imageSize, txtFontSize;
            double d1, d2, d3;
            Thickness thMargin, thPadding;
            object o1;

            // меню категорий размещено вертикально, справа
            if (isVerticalFolderMenu)
            {
                itemHeight = pnlHeight / iCount; itemWidth = pnlWidth;
                marginBase = 0.1 * itemHeight;
                thMargin = new Thickness(marginBase, 0, marginBase, 1.4 * marginBase);
                thPadding = new Thickness(0, 2.0 * marginBase, 0, 2.0 * marginBase);
                o1 = this.TryFindResource("menuDataTemplate");
                if (o1 != null) lstMenuFolders.ItemTemplate = (DataTemplate)o1;
                imageSize = 0.4 * itemHeight;
                txtFontSize = 0.25 * itemHeight;

            }
            // меню категорий размещено горизонтально, вверху экрана
            else
            {
                d1 = Math.Floor(0.8 * dH);
                itemHeight = pnlHeight;
                itemWidth = pnlWidth / iCount;
                itemWidth -= Math.Floor(d1);
                thMargin = new Thickness(0, 0, d1, 0);
                thPadding = new Thickness(0);
                o1 = this.TryFindResource("menuDataTemplateHor");
                if (o1 != null) lstMenuFolders.ItemTemplate = (DataTemplate)o1;

                brdStyle.Setters.Add(new Setter(ListBoxItem.WidthProperty, itemWidth));

                imageSize = 0.5 * itemHeight;
                txtFontSize = 0.2 * itemHeight;
            }

            // поле вокруг рамки
            st = (Setter)brdStyle.Setters.FirstOrDefault(s => (s as Setter).Property.Name == "Margin");
            if (st == null)
                brdStyle.Setters.Add(new Setter(ListBoxItem.MarginProperty, thMargin));
            else
                st.Value = thMargin;
            // отступ внутри рамки
            st = (Setter)brdStyle.Setters.FirstOrDefault(s => (s as Setter).Property.Name == "Padding");
            if (st == null)
                brdStyle.Setters.Add(new Setter(ListBoxItem.PaddingProperty, thPadding));
            else
                st.Value = thPadding;
            // размер изображения категории
            st = (Setter)imgStyle.Setters.FirstOrDefault(s => (s as Setter).Property.Name == "Width");
            if (st == null)
                imgStyle.Setters.Add(new Setter(Image.WidthProperty, imageSize));
            else
                st.Value = imageSize;
            st = (Setter)imgStyle.Setters.FirstOrDefault(s => (s as Setter).Property.Name == "Height");
            if (st == null)
                imgStyle.Setters.Add(new Setter(Image.HeightProperty, imageSize));
            else
                st.Value = imageSize;
            // размер текста
            st = (Setter)txtStyle.Setters.FirstOrDefault(s => (s as Setter).Property.Name == "FontSize");
            if (st == null)
                txtStyle.Setters.Add(new Setter(TextBlock.FontSizeProperty, txtFontSize));
            else
                st.Value = txtFontSize;

            //SetterBase bs = brdStyle.Setters.FirstOrDefault(s => (s as Setter).Property.Name == "MouseDown");

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
            if (mFolders == null) return;

            foreach (AppModel.MenuItem mItem in mFolders)
            {
                MainMenuDishesCanvas canvas = new MainMenuDishesCanvas(mItem);
                _dishCanvas.Add(canvas);
            }
        }  // createDishesCanvas


        //  обработка события нажатия на кнопку показа/скрытия описания блюда (с анимацией)

        private void resetDishesLang()
        {
            foreach (MainMenuDishesCanvas dCanv in _dishCanvas)
            {
                dCanv.ResetLang();
            }
        }

        public void ClearSelectedGarnish()
        {
            foreach (MainMenuDishesCanvas dCanv in _dishCanvas)
            {
                dCanv.ClearSelectedGarnish();
            }
        }
        public void HideDishesDescriptions()
        {
            foreach (MainMenuDishesCanvas dCanv in _dishCanvas)
            {
                dCanv.HideDishesDescriptions();
            }
        }
        #endregion

        #region language bottons
        private void lblButtonLang_MouseDown(object sender, MouseButtonEventArgs e)
        {
            string langId = getLangIdByButtonName(((FrameworkElement)sender).Name);
            AppLib.WriteAppAction(this.Name, AppActionsEnum.SelectLang, langId);

            selectAppLang(langId);
            //e.Handled = true;

            string dev = "";
            if (e.StylusDevice != null) dev = e.StylusDevice.Name;
        }

        // установить язык текстов на элементах
        public void selectAppLang(string langId)
        {
            // сохранить выбранный пункт меню
            int selMenuItem = lstMenuFolders.SelectedIndex;

            setLangButtonStyle(false);  // "выключить" кнопку
            AppLib.AppLang = langId;
            setLangButtonStyle(true);   // "включить" кнопку

            AppLib.SetPromocodeTextStyle(txtPromoCode);

            BindingExpression be;
            // установка текстов на выбранном языке
            be = lblMakeOrderText.GetBindingExpression(TextBlock.TextProperty);
            if (be != null) be.UpdateTarget();

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
                Style newStyle = (checkedMode) ? (Style)App.Current.Resources["langButtonBorderCheckedStyle"] : (Style)App.Current.Resources["langButtonBorderUncheckedStyle"];
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

        private void brdPromoCode_PreviewMouseUp(object sender, MouseButtonEventArgs e)
        {
            AppLib.WriteAppAction(this.Name, AppActionsEnum.ButtonPromocode);

            string preText = App.PromocodeNumber??"";

            AppLib.PromoCodeWindow.ShowDialog();
            e.Handled = true;
            // чтобы не срабатывали обработчики нижележащих контролов
            AppLib.IsEventsEnable = false;

            if (!(App.PromocodeNumber??"").Equals(preText)) AppLib.SetPromocodeTextStyle(this.txtPromoCode);
        }
        #endregion

        #region анимация выбора блюда и стоимости заказа

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
            _animDishSelection = new Storyboard() { FillBehavior = FillBehavior.Stop, AccelerationRatio = 1, AutoReverse = false };
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

            // для анимации фона с ценой заказа
            _animOrderPriceBackgroundColor = new ColorAnimation(Colors.Magenta, TimeSpan.FromMilliseconds(50), FillBehavior.Stop) { AutoReverse = true, RepeatBehavior = new RepeatBehavior(5) };
            //  сохранить анимацию в App.Properties
            AppLib.SetAppGlobalValue("AddDishButtonBackgroundColorAnimation", _animOrderPriceBackgroundColor);
            // анимация любого текста: размера и размытие
            AppLib.SetAppGlobalValue("AddDishButtonTextAnimation", new TextAnimation()
            {
                IsAnimFontSize = true, DurationFontSize = 200, FontSizeKoef = 1.2, RepeatBehaviorFontSize = 3,
                IsAnimTextBlur = false, DurationTextBlur = 200, TextBlurTo = 10,  RepeatBehaviorTextBlur = 3
            });

            _orderPriceEffectShadow = new DropShadowEffect() { Direction = 315, Color = Colors.DarkGreen, ShadowDepth = 5, BlurRadius = 10 };
            _orderPriceEffectBlur = new BlurEffect() { Radius = 0 };
            //lblOrderPrice.Effect = _orderPriceEffectShadow;
            Color c = ((SolidColorBrush)Application.Current.Resources["cartButtonBackgroundColor"]).Color;
            brdMakeOrder.Background = new SolidColorBrush(c); //Do not use a frozen instance  (Colors.Orange)
        }

        private void _aminDishSelection_Completed(object sender, EventArgs e)
        {
            canvasAnim.Visibility = Visibility.Hidden;

            animateOrderPrice();
        }

        public void animateSelectDish(Path pathImage)
        {
            (animImage.Fill as VisualBrush).Visual = pathImage;

            // обновление пути анимации
            PathFigure pf = (animPath.Data as PathGeometry).Figures[0];
            BezierSegment bezierSeg = (pf.Segments[0] as BezierSegment);
            // получить точку начала анимации: центр панели блюда
            Point fromPoint = pathImage.PointToScreen(new Point(pathImage.ActualWidth / 2d, pathImage.ActualHeight / 2d));
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
                    p1 = new Point(1.5 * toPoint.X, 1.3 * fromPoint.Y);
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

        public void animateOrderPrice()
        {
            // анимация фона
            if ((_currentOrder == null) || 
                ((_currentOrder.GetOrderValue() == 0) && (_animOrderPriceBackgroundColor != null)))
            {
                brdMakeOrder.Background.BeginAnimation(SolidColorBrush.ColorProperty, _animOrderPriceBackgroundColor);
            }
            // анимация цены
            else
            {
                //   размер шрифта
                _daCommon1.Duration = TimeSpan.FromMilliseconds(400);
                _daCommon1.To = 1.5 * _orderPriceFontSize;
                lblOrderPrice.BeginAnimation(TextBlock.FontSizeProperty, _daCommon1);
                //   расплывчатость текста
                lblOrderPrice.Effect = _orderPriceEffectBlur;
                _daCommon2.Duration = TimeSpan.FromMilliseconds(400);
                _daCommon2.To = 80;
                _daCommon2.Completed += _daCommon1_Completed;
                lblOrderPrice.Effect.BeginAnimation(BlurEffect.RadiusProperty, _daCommon2);
            }
        }

        private void _daCommon1_Completed(object sender, EventArgs e)
        {
            updatePrice();

            _daCommon1.To = _orderPriceFontSize;
            lblOrderPrice.BeginAnimation(TextBlock.FontSizeProperty, _daCommon1);

            _daCommon2.To = 0;
            _daCommon2.Completed -= _daCommon1_Completed;
            _daCommon2.Completed += _daCommon2_Completed;
            lblOrderPrice.Effect.BeginAnimation(BlurEffect.RadiusProperty, _daCommon2);
        }

        private void _daCommon2_Completed(object sender, EventArgs e)
        {
            _daCommon2.Completed -= _daCommon2_Completed;
            //lblOrderPrice.Effect = _orderPriceEffectShadow;
        }

        #endregion

        #region dish list behaviour
        private void lstDishes_RequestBringIntoView(object sender, RequestBringIntoViewEventArgs e)
        {
            e.Handled = true;
        }
        private void scrollDishes_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.StylusDevice != null) return;

            initDrag(e.GetPosition(scrollDishes));
        }

        private void scrollDishes_PreviewTouchDown(object sender, TouchEventArgs e)
        {
            initDrag(e.GetTouchPoint(scrollDishes).Position);
        }

        private void scrollDishes_PreviewMouseUp(object sender, MouseButtonEventArgs e)
        {
            if (e.StylusDevice != null) return;

            endDrag();
        }

        private void scrollDishes_PreviewTouchUp(object sender, TouchEventArgs e)
        {
            endDrag();
        }

        private void scrollDishes_MouseMove(object sender, MouseEventArgs e)
        {
            if (e.StylusDevice != null) return;

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
            if (AppLib.IsEventsEnable == false) { AppLib.IsEventsEnable = true; }

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
            if ((lastDragPoint == null) || (initDragPoint == null))
            {
                AppLib.IsDrag = false;
            }
            else
            {
                AppLib.IsDrag = (Math.Abs(lastDragPoint.Value.X - initDragPoint.Value.X) > 3) || (Math.Abs(lastDragPoint.Value.Y - initDragPoint.Value.Y) > 3);
            }
        }
        private void doMove(Point posNow)
        {
            if (AppLib.IsEventsEnable == false) { return; }

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

            int iRows = Convert.ToInt32(Math.Ceiling(1d * curCanvas.Children.Count / _dishColCount));
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

            int iRows = Convert.ToInt32(Math.Ceiling(1d * curCanvas.Children.Count / _dishColCount));
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
#if (enableTimer)
            AppModel.MenuItem mi = e.AddedItems[0] as AppModel.MenuItem;
            outTouchTrace(string.Format("выбрана кнопка: {0}", mi.langNames["ru"]));
#endif
            e.Handled = true;

            if ((_dishCanvas.Count > 0) && (lstMenuFolders.SelectedIndex <= (_dishCanvas.Count-1)))
            {
                AppModel.MenuItem mi = lstMenuFolders.SelectedItem as AppModel.MenuItem;

                if (AppLib.IsEventsEnable)
                {
                    AppLib.WriteAppAction(this.Name, AppActionsEnum.SelectDishCategory, mi.langNames["ru"]);
                }
                else
                    AppLib.IsEventsEnable = true;

                // установить панель блюд
                MainMenuDishesCanvas currentPanel = _dishCanvas[lstMenuFolders.SelectedIndex];
                // очистить выбор гарниров
                currentPanel.ClearSelectedGarnish();
                // убрать описания блюд
                currentPanel.HideDishesDescriptions();

                scrollDishes.Content = currentPanel;
                scrollDishes.ScrollToTop();
            }
        }

        // обновить стоимость заказа
        public void updatePrice()
        {
            decimal dPrice = 0m;
            if (_currentOrder != null) dPrice = _currentOrder.GetOrderValue();

            lblOrderPrice.Text = AppLib.GetCostUIText(dPrice);
        }

        private void Image_MouseUp(object sender, MouseButtonEventArgs e)
        {
//            this.Close();
        }

        private void btnLang_TouchUp(object sender, TouchEventArgs e)
        {
            //AppLib.WriteLogTraceMessage(string.Format("{0} - TouchUp, _langButtonPress = {1}", ((FrameworkElement)sender).Name, _langButtonPress.ToString()));

            _langButtonPress = 0;

           // AppLib.WriteLogTraceMessage(string.Format("{0} - TouchUp, _langButtonPress = {1}", ((FrameworkElement)sender).Name, _langButtonPress.ToString()));
        }

        private void btnLang_TouchDown(object sender, TouchEventArgs e)
        {
            string langId = getLangIdByButtonName(((FrameworkElement)sender).Name);
            switch (langId)
            {
                case "ua": _langButtonPress |= 1; break;
                case "ru": _langButtonPress |= 2; break;
                case "en": _langButtonPress |= 4; break;
                default: break;
            }
            //AppLib.WriteLogTraceMessage(string.Format("{0} - TouchDown, _langButtonPress = {1}", ((FrameworkElement)sender).Name, _langButtonPress.ToString()));
            if (_langButtonPress == 7) App.Current.Shutdown(3);
        }

        private void Grid_TouchUp(object sender, TouchEventArgs e)
        {
//            AppLib.WriteLogTraceMessage(string.Format("{0} - mainGrid_PreviewMouseUp, _langButtonPress = {1}", ((FrameworkElement)sender).Name, _langButtonPress.ToString()));

            _langButtonPress = 0;

//            AppLib.WriteLogTraceMessage(string.Format("{0} - mainGrid_PreviewMouseUp, _langButtonPress = {1}", ((FrameworkElement)sender).Name, _langButtonPress.ToString()));
        }

        private void brdMakeOrder_PreviewTouchDown(object sender, TouchEventArgs e)
        {
            showCartWindow();
        }

        private void btnShowCart_MouseUp(object sender, MouseButtonEventArgs e)
        {
//            if (e.StylusDevice != null) return;
            showCartWindow();
        }


        private void showCartWindow()
        {
            AppLib.WriteAppAction(this.Name, AppActionsEnum.ButtonMakeOrder);

            if ((_currentOrder == null) || (_currentOrder.GetOrderValue() == 0))
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
