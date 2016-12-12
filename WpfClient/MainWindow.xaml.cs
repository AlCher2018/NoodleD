﻿using System;
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
        Border _curAddButton;
        SolidColorBrush brushBlack;

        // dragging
        Point? lastDragPoint;
        protected DateTime _dateTime;
        bool _isMoved;

        public MainWindow()
        {
            InitializeComponent();

            //TestData.mainProc();

            logger.Info("Start application");

            logger.Trace("Получаю данные от SQL Server...");
            ObservableCollection<AppModel.MenuItem> mFolders;
            using (NoodleDContext db = new NoodleDContext())
            {
                initCurrentSettings(db);

                // получить язык UI
                string langId = (string)AppLib.GetAppGlobalValue("langButtonDefaultId");
                AppLib.AppLang = langId;

                // получить данные с SQL во внутренние объекты
                mFolders = MenuLib.GetMenuMainFolders();
            }
            logger.Trace("Получаю данные от SQL Server - READY");

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
            CurrentOrder curOrder = new CurrentOrder();
            AppLib.SetAppGlobalValue("currentOrder", curOrder);
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
                setUncheckedAllLangButtons();
                string langId = getLangIdByButtonName(langControl.Name);
                AppLib.AppLang = langId;
            }

            // установка текстов на выбранном языке
            setCheckedLangButton(langControl);
            txtPromoCode.Text = AppLib.GetLangText((Dictionary<string, string>)AppLib.GetAppGlobalValue("invitePromoText"));
            lblOrderPrice.Text = "0 ₴ " + AppLib.GetLangText((Dictionary<string, string>)AppLib.GetAppGlobalValue("btnCreateOrderText"));
            lstMenuFolders.Items.Refresh();
            lstDishes.Items.Refresh();
            // сбросить выбор блюда
            if (_curDishItem != null)
            {
                if (_curDishItem.SelectedGarnishes != null && (_curDishItem.SelectedGarnishes.Count > 0))
                {
                    _curDishItem.SelectedGarnishes = null;
                    updateVisualGarnish(true);
                }
                _curDishItem = null; _curGarnishBorder = null; _curAddButton = null;
            }

            // восстановить выбранный пункт главного меню
            if (selMenuItem >= 0) selMenuItem = 0;
            lstMenuFolders.SelectedIndex = (int)(AppLib.GetAppGlobalValue("selectedMenuIndex")??0);
        }
        private void setCheckedLangButton(FrameworkElement langControl)
        {
            langControl.Style = (Style)this.Resources["langButtonBorderCheckedStyle"];
        }
        private void setUncheckedAllLangButtons()
        {
            btnLangUa.Style = (Style)this.Resources["langButtonBorderUncheckedStyle"];
            btnLangRu.Style = (Style)this.Resources["langButtonBorderUncheckedStyle"];
            btnLangEn.Style = (Style)this.Resources["langButtonBorderUncheckedStyle"];
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

        private void txtPromoCode_GotFocus(object sender, RoutedEventArgs e)
        {
            brdPromoCode.BorderThickness = new Thickness(3);
        }

        private void txtPromoCode_LostFocus(object sender, RoutedEventArgs e)
        {
            brdPromoCode.BorderThickness = new Thickness(0);
        }
        #endregion

        // боковое меню выбора категории блюд
        private void lstMenuFolders_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            // очистить выбранный гарнир
            ObservableCollection<AppModel.MenuItem> mFolders = (ObservableCollection<AppModel.MenuItem>)AppLib.GetAppGlobalValue("mainMenu");
            foreach (AppModel.MenuItem item in mFolders)
            {
                foreach (DishItem dish in item.Dishes)
                {
                    if ((dish.SelectedGarnishes != null) && (dish.SelectedGarnishes.Count() > 0)) dish.SelectedGarnishes.Clear();
                }
            }

            //lstDishes.Items.Refresh();
            lstDishes.ItemsSource = ((AppModel.MenuItem)lstMenuFolders.SelectedItem).Dishes;

            scrollDishes.ScrollToTop();
            if (lstDishes.Items.Count > 0) lstDishes.ScrollIntoView(lstDishes.Items[0]);
        }


        #region кнопки гарниров
        // приходит Grid
        private void borderGarnish_MouseUp(object sender, MouseButtonEventArgs e)
        {
            if (_isMoved == false) borderGarnisgHandler(sender);
        }
        private void brdGarnish_TouchDown(object sender, TouchEventArgs e)
        {
            if (_isMoved == false) borderGarnisgHandler(sender);
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

            Grid gridPar = (Grid)LogicalTreeHelper.GetParent(gridGarnAll); // родительский грид
            Grid gridBigBut = (Grid)(AppLib.FindLogicalChildren<Grid>(gridPar).First(g => g.Name== "gridDishBottomButtons"));  // грид с большими кнопками
            Border borderAddBut = AppLib.FindVisualChildren<Border>(gridBigBut).First(g => g.Name=="gridDishAddButton");  // бордер кнопки добавления блюда

            if (_curDishItem == null)
            {
                _curDishItem = (DishItem)lstDishes.SelectedItem;
                _curDishItem.SelectedGarnishes = new List<DishAdding>();

                DishAdding da = _curDishItem.Garnishes[garnIndex];
                da.Uid = gridGarn.Uid;
                _curDishItem.SelectedGarnishes.Add(da);

                _curGarnishBorder = borderGarn;
                _curGarnishTextBlock = garnName;
                _curAddButton = borderAddBut;

                updateVisualGarnish(true);
            }
            else
            {
                if (_curDishItem == (DishItem)lstDishes.SelectedItem)  // клик по гарниру в том же блюде
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

                    updateVisualGarnish(true);

                }
            }
        }
        private void updateVisualGarnish(bool isUpdAddBut)
        {
            SolidColorBrush selBase = (SolidColorBrush)AppLib.GetAppGlobalValue("appSelectedItemColor");
            SolidColorBrush notSelBase = (SolidColorBrush)AppLib.GetAppGlobalValue("appBackgroundColor");
            SolidColorBrush notSelText = (SolidColorBrush)AppLib.GetAppGlobalValue("appNotSelectedItemColor");

            List<DishAdding> garList = _curDishItem.SelectedGarnishes;  // SelectedGarnishes
            if (garList==null || garList.Count == 0)  // ничего не выбрано
            {
                _curGarnishBorder.Fill = notSelBase;
                _curGarnishTextBlock.Foreground = notSelText;
            }
            else
            {
                _curGarnishBorder.Fill = selBase;
                _curGarnishTextBlock.Foreground = brushBlack;
            }

            BindingExpression be;
            //be = _curGarnishBorder.GetBindingExpression(Border.BackgroundProperty);
            //be.UpdateTarget();
            //be = _curGarnishBorder.GetBindingExpression(TextBlock.ForegroundProperty);
            //be.UpdateTarget();
            if (isUpdAddBut)
            {
                be = _curAddButton.GetBindingExpression(Border.VisibilityProperty);
                be.UpdateTarget();
            }
        }

        #endregion

        #region выбор блюда
        private void gridDishAddButton_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (_isMoved == false) gridDishAddButtonHandler(sender);
        }
        private void gridDishAddButton_TouchDown(object sender, TouchEventArgs e)
        {
            if (_isMoved == false) gridDishAddButtonHandler(sender);
        }
        private void gridDishAddButtonHandler(object sender)
        {
            //MessageBox.Show("Under construction");
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
            DishItem curDI = (DishItem)lstDishes.SelectedItem;
            if (curDI.SelectedGarnishes == null) _curDishItem = curDI;

            popupWin.DataContext = _curDishItem;

            popupWin.ShowDialog();
        }
        #endregion

        #region btnShowDishDescriptionHandler
        private void btnShowDishDescription_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (_isMoved == false) btnShowDishDescriptionHandler(sender);
        }

        private void btnShowDishDescription_TouchDown(object sender, TouchEventArgs e)
        {
            if (_isMoved == false) btnShowDishDescriptionHandler(sender);
        }
        private void btnShowDishDescriptionHandler(object sender)
        {
            FrameworkElement vBox = (FrameworkElement)sender;
            FrameworkElement gridDish = AppLib.FindLogicalParentByName(vBox, "gridDish", 3);

            List<Grid> vbList = AppLib.FindLogicalChildren<Grid>(gridDish).ToList();
            Grid vbButton = vbList.FirstOrDefault(g => g.Name == "btnShowDishDescription"); 
            Grid vbText = vbList.FirstOrDefault(g => g.Name == "dishDescrText");
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

        private void initDrag(Point mousePos)
        {
            _dateTime = DateTime.Now;
            _isMoved = false;
            //make sure we still can use the scrollbars
            if (mousePos.X <= scrollDishes.ViewportWidth && mousePos.Y < scrollDishes.ViewportHeight)
            {
                //scrollViewer.Cursor = Cursors.SizeAll;
                lastDragPoint = mousePos;
                //Mouse.Capture(scrollViewer);
            }
        }
        private void endDrag()
        {
            //scrollViewer.Cursor = Cursors.Arrow;
            //scrollViewer.ReleaseMouseCapture();
            lastDragPoint = null;
        }
        private void doMove(Point posNow)
        {
            double dX = posNow.X - lastDragPoint.Value.X;
            double dY = posNow.Y - lastDragPoint.Value.Y;

            lastDragPoint = posNow;

            scrollDishes.ScrollToHorizontalOffset(scrollDishes.HorizontalOffset - dX);
            scrollDishes.ScrollToVerticalOffset(scrollDishes.VerticalOffset - dY);
            _isMoved = true;
        }
        private void scrollDishes_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            initDrag(e.GetPosition(scrollDishes));
        }

        private void scrollDishes_PreviewTouchDown(object sender, TouchEventArgs e)
        {
            initDrag(e.GetTouchPoint(scrollDishes).Position);
        }

        private void scrollDishes_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            endDrag();
        }

        private void scrollDishes_PreviewTouchUp(object sender, TouchEventArgs e)
        {
            endDrag();
        }

        private void scrollDishes_MouseMove(object sender, MouseEventArgs e)
        {
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

        #endregion

        private void Window_Closed(object sender, EventArgs e)
        {
            logger.Info("End application");
        }



    } // class MainWindow

}