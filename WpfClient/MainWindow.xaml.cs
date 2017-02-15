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
using System.Timers;

namespace WpfClient
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        // подложка списка блюд
        private List<MainMenuDishesCanvas> _dishCanvas;
        // текущий заказ
        private OrderItem _currentOrder;
        // visual elements
        private byte _langButtonPress = 0;
        private Border _curDescrBorder;
        private TextBlock _curDescrTextBlock;

        // dish description animations
        private DoubleAnimation _daCommon1, _daCommon2;
        private Storyboard _animDishSelection;
        private ColorAnimation _animOrderPriceBackgroundColor;
        private Effect _orderPriceEffectShadow;
        private Effect _orderPriceEffectBlur;

        // dragging
        private Point? lastDragPoint, initDragPoint;
        private DateTime _dateTime;

        public List<MainMenuDishesCanvas> DishesPanels { get { return _dishCanvas; } }


        public MainWindow()
        {
            InitializeComponent();

            // для настройки элементов после отрисовки окна
            Loaded += MainWindow_Loaded;

            // инициализация локальных переменных
            _dishCanvas = new List<MainMenuDishesCanvas>();
            _daCommon1 = new DoubleAnimation();
            _daCommon2 = new DoubleAnimation();

            // создать текущий заказ
            _currentOrder = new OrderItem();
            AppLib.SetAppGlobalValue("currentOrder", _currentOrder);
            updatePrice();

            // отслеживание бездействия
            App.IdleHandler.AnyActionWindow = this;

            initUI();
        }

        protected override void OnClosed(EventArgs e)
        {
            base.OnClosed(e);
            AppLib.WriteLogInfoMessage("************  End application  ************");
        }

        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            //   конечный элемент анимации выбора блюда, это Point3 в BezierSegment
            Point toBasePoint = brdMakeOrder.PointToScreen(new Point(0, 0));
            Size toSize = brdMakeOrder.RenderSize;
            Point endPoint = new Point(toBasePoint.X + toSize.Width / 2.0, toBasePoint.Y + toSize.Height / 2.0);
            // установить для сегмента анимации конечную точку
            PathFigure pf = (this.animPath.Data as PathGeometry).Figures[0];
            BezierSegment bs = (pf.Segments[0] as BezierSegment);
            bs.Point3 = endPoint;

            string sBuf = AppLib.GetAppSetting("UserIdleTime");
            if ((sBuf != null) && (sBuf != "0"))
            {
                int idleSec = int.Parse(sBuf);
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
            if (mFolders == null) return;

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

        private void createCategoriesList()
        {
            var v = AppLib.GetAppGlobalValue("mainMenu");
            if (v == null) return;

            // стиль содержания пункта меню
            bool isScrollingList = AppLib.GetAppSetting("IsAllowScrollingDishCategories").IsTrueString();
            // без скроллинга, поэтому настраиваем поля и отступы
            // если категорий меньше 6, то оставляем по умолчанию, из разметки
            if (isScrollingList == false)
            {
                List<AppModel.MenuItem> mFolders = (List<AppModel.MenuItem>)v;
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
            selectAppLang(langId);
            //e.Handled = true;

            string dev = "";
            if (e.StylusDevice != null) dev = e.StylusDevice.Name;
            AppLib.WriteLogTraceMessage("lblButtonLang_MouseDown, StylusDevice - " + dev);
        }

        private void lblButtonLang_MouseUp(object sender, MouseButtonEventArgs e)
        {
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
        private void brdPromoCode_PreviewMouseUp(object sender, MouseButtonEventArgs e)
        {
            Promocode promoWin = new Promocode();
            promoWin.ShowDialog();

            string retVal = promoWin.InputValue;
            promoWin = null;
            GC.Collect();

            //MessageBox.Show(retVal??"Null");
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
            _orderPriceEffectShadow = new DropShadowEffect() { Direction = 315, Color = Colors.DarkGreen, ShadowDepth = 5, BlurRadius = 10 };
            _orderPriceEffectBlur = new BlurEffect() { Radius = 0 };
            txtOrderPrice.Effect = _orderPriceEffectShadow;
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

        public void animateOrderPrice()
        {
            // анимация фона
            if ((_currentOrder.GetOrderValue() == 0) && (_animOrderPriceBackgroundColor != null))
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

            if ((_dishCanvas.Count > 0) && (lstMenuFolders.SelectedIndex <= (_dishCanvas.Count-1)))
            {
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
            txtOrderPrice.Text = AppLib.GetCostUIText(_currentOrder.GetOrderValue());
        }

        private void Image_MouseUp(object sender, MouseButtonEventArgs e)
        {
//            this.Close();
        }

        private void btnLang_TouchUp(object sender, TouchEventArgs e)
        {
            AppLib.WriteLogTraceMessage(string.Format("{0} - TouchUp, _langButtonPress = {1}", ((FrameworkElement)sender).Name, _langButtonPress.ToString()));

            _langButtonPress = 0;

            AppLib.WriteLogTraceMessage(string.Format("{0} - TouchUp, _langButtonPress = {1}", ((FrameworkElement)sender).Name, _langButtonPress.ToString()));
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
            AppLib.WriteLogTraceMessage(string.Format("{0} - TouchDown, _langButtonPress = {1}", ((FrameworkElement)sender).Name, _langButtonPress.ToString()));
            if (_langButtonPress == 7) App.Current.Shutdown(3);
        }

        private void Grid_TouchUp(object sender, TouchEventArgs e)
        {
            AppLib.WriteLogTraceMessage(string.Format("{0} - mainGrid_PreviewMouseUp, _langButtonPress = {1}", ((FrameworkElement)sender).Name, _langButtonPress.ToString()));

            _langButtonPress = 0;

            AppLib.WriteLogTraceMessage(string.Format("{0} - mainGrid_PreviewMouseUp, _langButtonPress = {1}", ((FrameworkElement)sender).Name, _langButtonPress.ToString()));
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
