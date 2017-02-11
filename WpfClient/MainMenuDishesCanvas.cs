using AppModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Effects;
using System.Windows.Shapes;

namespace WpfClient
{
    public class MainMenuDishesCanvas: Canvas
    {
        #region privates vars
        private static double screenWidth = (double)AppLib.GetAppGlobalValue("screenWidth");
        private static double screenHeight = (double)AppLib.GetAppGlobalValue("screenHeight");
        // углы закругления
        private static double cornerRadiusButton = (double)AppLib.GetAppGlobalValue("cornerRadiusButton");
        //  РАЗМЕРЫ ПАНЕЛИ БЛЮД(А)
        private static double dishesPanelWidth = (screenWidth / 6d * 5d);
        private static double dishPanelWidth = 0.95d * dishesPanelWidth / 3d;
        private static double dishPanelLeftMargin = (dishesPanelWidth - 3 * dishPanelWidth) / 2;
        private static double dKoefContentWidth = 0.9d;
        private static double contentPanelWidth = dKoefContentWidth * dishPanelWidth;

        // высота строки заголовка
        private static double dishPanelHeaderRowHeight = 0.17d * dishPanelWidth;
        // высота строки изображения
        private static double dishPanelImageRowHeight = 0.7d * dishPanelWidth;
        // высота строки гарниров
        private static double dishPanelGarnishesRowHeight = 0.2d * dishPanelWidth;
        // высота строки кнопки добавления
        private static double dishPanelAddButtonRowHeight = 0.15d * dishPanelWidth;
        private static double dishPanelAddButtonTextSize = 0.3d * dishPanelAddButtonRowHeight;
        // расстояния между строками панели блюда
        private static double dishPanelRowMargin1 = 0.01d * dishPanelWidth;
        private static double dishPanelRowMargin2 = 0.02d * dishPanelWidth;

        //// размер шрифтов
        //double dishPanelHeaderFontSize = Convert.ToDouble(AppLib.GetAppGlobalValue("dishPanelHeaderFontSize"));
        //double dishPanelUnitCountFontSize = Convert.ToDouble(AppLib.GetAppGlobalValue("dishPanelUnitCountFontSize"));
        //double dishPanelDescriptionFontSize = Convert.ToDouble(AppLib.GetAppGlobalValue("dishPanelDescriptionFontSize"));
        //double dishPanelAddButtoFontSize = Convert.ToDouble(AppLib.GetAppGlobalValue("dishPanelAddButtoFontSize"));
        //double dishPanelTextFontSize = Convert.ToDouble(AppLib.GetAppGlobalValue("dishPanelFontSize"));

        // размер кнопки описания блюда
        private static double dishPanelDescrButtonSize = 0.085d * dishPanelWidth;

        // высота панелей
        private static double dishPanelHeight = Math.Ceiling(dishPanelHeaderRowHeight + dishPanelRowMargin1 + dishPanelImageRowHeight + dishPanelRowMargin2 + dishPanelAddButtonRowHeight);
        private static double dishPanelHeightWithGarnish = Math.Ceiling(dishPanelHeight + dishPanelGarnishesRowHeight + dishPanelRowMargin2);
        private static double dKoefContentHeight = 1d;
        private static double contentPanelHeight = Math.Ceiling(dKoefContentHeight * dishPanelHeight);
        private static double contentPanelHeightWithGarnish = Math.Ceiling(dKoefContentHeight * dishPanelHeightWithGarnish);

        static MainMenuDishesCanvas()
        {
            AppLib.SetAppGlobalValue("dishImageWidth", contentPanelWidth);
            AppLib.SetAppGlobalValue("dishImageHeight", dishPanelImageRowHeight);
        }
        #endregion

        MainMenuGarnish _selectedGarnish;
        AppModel.MenuItem mItem;

        public MainMenuDishesCanvas(AppModel.MenuItem menuItem)
        {
            mItem = menuItem;
        }

        private void createDishesCanvas()
        {
            double currentPanelHeight, currentContentPanelHeight;
            double leftPos, topPos;
            double d1;

            base.Width = dishesPanelWidth;

            int iRowsCount = 0;
            if (mItem.Dishes.Count > 0) iRowsCount = ((mItem.Dishes.Count - 1) / 3) + 1;
            int iRow, iCol;
            for (int i = 0; i < mItem.Dishes.Count; i++)
            {
                DishItem dish = mItem.Dishes[i];
                bool isExistGarnishes = (dish.Garnishes != null);
                currentPanelHeight = ((isExistGarnishes) ? dishPanelHeightWithGarnish : dishPanelHeight);
                currentContentPanelHeight = ((isExistGarnishes) ? contentPanelHeightWithGarnish : contentPanelHeight);
                base.Height = iRowsCount * currentPanelHeight;

                // положение панели блюда
                iRow = i / 3; iCol = i % 3;
                leftPos = (dishPanelLeftMargin + iCol * dishPanelWidth);
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
                brd.ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(d1, GridUnitType.Pixel) });
                brd.ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(contentPanelWidth, GridUnitType.Pixel) });
                brd.ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(d1, GridUnitType.Pixel) });
                d1 = (currentPanelHeight - currentContentPanelHeight) / 2d;
                brd.RowDefinitions.Add(new RowDefinition() { Height = new GridLength(d1, GridUnitType.Pixel) });
                brd.RowDefinitions.Add(new RowDefinition() { Height = new GridLength(currentContentPanelHeight, GridUnitType.Pixel) });
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
                setDishPanelHeader(dish, dGrid);

                // изображение блюда и описание
                setDishDescription(dish, dGrid, dishPanelDescrButtonSize);

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
                setDishAddButton(dish, dGrid, dishPanelAddButtonRowHeight, dKoefContentHeight, cornerRadiusButton);

                brd.Children.Add(dGrid); Grid.SetRow(dGrid, 1); Grid.SetColumn(dGrid, 1);

                base.Children.Add(brd);

            }  // for dishes

            //canvas.Background = new SolidColorBrush(new Color() { R = (byte)rnd.Next(0,254), G= (byte)rnd.Next(0, 254), B= (byte)rnd.Next(0, 254), A=0xFF });

        } // method

        private void setDishPanelHeader(DishItem dish, Grid dGrid)
        {
            // размер шрифтов
            double dishPanelHeaderFontSize = Convert.ToDouble(AppLib.GetAppGlobalValue("dishPanelHeaderFontSize"));
            double dishPanelUnitCountFontSize = Convert.ToDouble(AppLib.GetAppGlobalValue("dishPanelUnitCountFontSize"));

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
                    FontSize = dishPanelUnitCountFontSize
                });
                inlines.Add(new Run()
                {
                    Text = " " + AppLib.GetLangText(dish.langUnitNames),
                    FontSize = dishPanelUnitCountFontSize
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
        }  // method

        private void setDishDescription(DishItem dish, Grid dGrid, double dishPanelDescrButtonSize)
        {
            if (dish.Image == null) return;

            // размер шрифтов
            double dishPanelDescriptionFontSize = Convert.ToDouble(AppLib.GetAppGlobalValue("dishPanelDescriptionFontSize"));
            double dishPanelTextFontSize = Convert.ToDouble(AppLib.GetAppGlobalValue("dishPanelFontSize"));

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
                FontSize = dishPanelTextFontSize,
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
                Padding = new Thickness(dishPanelDescriptionFontSize),
                VerticalAlignment = VerticalAlignment.Center,
                TextAlignment = TextAlignment.Center,
                TextWrapping = TextWrapping.Wrap,
                FontSize = dishPanelDescriptionFontSize,
                Foreground = Brushes.White,
                Text = AppLib.GetLangText(dish.langDescriptions),
                Visibility = Visibility.Hidden
            };
            tbDescrText.Effect = new BlurEffect() { Radius = 20 };
            tbDescrText.PreviewMouseLeftButtonUp += CanvDescr_PreviewMouseLeftButtonUp;

            // добавить в контейнер
            Grid.SetRow(tbDescrText, 2); dGrid.Children.Add(tbDescrText);
        }

        private void setDishAddButton(DishItem dish, Grid dGrid, double dishPanelAddButtonRowHeight, double dKoefContentHeight, double cornerRadiusButton)
        {
            // размер шрифтов
            double dishPanelAddButtoFontSize = Convert.ToDouble(AppLib.GetAppGlobalValue("dishPanelAddButtoFontSize"));

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
                FontSize = dishPanelAddButtoFontSize,
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
                    FontSize = dishPanelAddButtoFontSize,
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
                    Background = Brushes.White,
                    BorderBrush = Brushes.Gray,
                    BorderThickness = new Thickness(1),
                    SnapsToDevicePixels = true
                };
                TextBlock tbInvitation = new TextBlock()
                {
                    Name = "tbInvitation",
                    Text = (string)AppLib.GetLangText((Dictionary<string, string>)AppLib.GetAppGlobalValue("btnSelectGarnishText")),
                    VerticalAlignment = VerticalAlignment.Center,
                    HorizontalAlignment = HorizontalAlignment.Center,
                    FontSize = dishPanelAddButtoFontSize,
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

        }  // method

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


    } // class
}
