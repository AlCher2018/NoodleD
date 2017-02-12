using AppModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;
using System.Windows.Shapes;
using System.Windows.Input;
using System.Windows.Media.Effects;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;

namespace WpfClient
{
    public class MainMenuDishPanel: Grid
    {
        #region privates vars
        private double dishPanelWidth = (double)AppLib.GetAppGlobalValue("dishPanelWidth");
        private double contentPanelWidth = (double)AppLib.GetAppGlobalValue("contentPanelWidth");
        private double currentPanelHeight, currentContentPanelHeight;

        private DishItem _dishItem;
        private double _leftPos, _topPos;

        private bool _hasGarnishes;
        private int _selectedGarnIndex = -1;

        // панель содержания информации о блюде
        private Grid dGrid;
        private Grid _grdGarnishes;
        private Path _pathImage = null;
        private bool _showDescription = false;
        private Border _btnDescr;
        private Border _descrTextBorder;
        private TextBlock _descrText;
        private SolidColorBrush _brushSelectedItem;

        private Border _btnAddDish;
        private Border _btnInvitation;

        // раскадровки для анимации описания
        private Storyboard _sbDescrShow, _sbDescrHide;

        #endregion

        public bool HasGarnishes { get { return _hasGarnishes; } }
        public int SelectedGarnishIndex { get { return _selectedGarnIndex; } }
        public bool IsDescriptionShow { get { return _showDescription; } }


        public MainMenuDishPanel(DishItem dishItem, double leftPos, double topPos)
        {
            _dishItem = dishItem;
            _leftPos = leftPos; _topPos = topPos;
            _hasGarnishes = (_dishItem.Garnishes != null);
            currentPanelHeight = (_hasGarnishes) ? (double)AppLib.GetAppGlobalValue("dishPanelHeightWithGarnish") : (double)AppLib.GetAppGlobalValue("dishPanelHeight");
            _brushSelectedItem = (SolidColorBrush)AppLib.GetAppGlobalValue("appSelectedItemColor");

            // декоратор для панели блюда (должен быть для корректной работы ручного скроллинга)
            setDishPanel();
            // панель содержания
            createContentPanel();
            // Заголовок панели
            setDishPanelHeader();
            // изображение блюда и описание
            setDishDescription();
            // гарниры для Воков
            if (_hasGarnishes) createGarnisheButtons();
            // изображения кнопок добавления
            setDishAddButton();

            // прочитать и установить длительность анимации
            double cfgDuration = getDuration();
            if (cfgDuration > 0)
            {
                // показать подсказку
                _sbDescrShow = getDescrStoryboard(true, cfgDuration);
                // скрыть подсказку
                _sbDescrHide = getDescrStoryboard(false, 1.5 * cfgDuration);
                _sbDescrHide.Completed += _sbDescrHide_Completed;
            }

            base.Children.Add(dGrid); Grid.SetRow(dGrid, 1); Grid.SetColumn(dGrid, 1);
        }


        private void setDishPanel()
        {
            currentContentPanelHeight = (_hasGarnishes) ? (double)AppLib.GetAppGlobalValue("contentPanelHeightWithGarnish") : (double)AppLib.GetAppGlobalValue("contentPanelHeight");

            base.SnapsToDevicePixels = true;
            base.Width = dishPanelWidth;
            base.Height = currentPanelHeight;
            base.SetValue(Canvas.LeftProperty, _leftPos);
            base.SetValue(Canvas.TopProperty, _topPos);

            // сетка 3х3
            double d1 = (dishPanelWidth - contentPanelWidth) / 2d;
            base.ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(d1, GridUnitType.Pixel) });
            base.ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(contentPanelWidth, GridUnitType.Pixel) });
            base.ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(d1, GridUnitType.Pixel) });
            d1 = (currentPanelHeight - currentContentPanelHeight) / 2d;
            base.RowDefinitions.Add(new RowDefinition() { Height = new GridLength(d1, GridUnitType.Pixel) });
            base.RowDefinitions.Add(new RowDefinition() { Height = new GridLength(currentContentPanelHeight, GridUnitType.Pixel) });
            base.RowDefinitions.Add(new RowDefinition() { Height = new GridLength(d1, GridUnitType.Pixel) });
        }

        private void createContentPanel()
        {
            // высота строки заголовка
            double dishPanelHeaderRowHeight = (double)AppLib.GetAppGlobalValue("dishPanelHeaderRowHeight");
            // высота строки изображения
            double dishPanelImageRowHeight = (double)AppLib.GetAppGlobalValue("dishPanelImageRowHeight");
            // высота строки гарниров
            double dishPanelGarnishesRowHeight = (double)AppLib.GetAppGlobalValue("dishPanelGarnishesRowHeight");
            // высота строки кнопки добавления
            double dishPanelAddButtonRowHeight = (double)AppLib.GetAppGlobalValue("dishPanelAddButtonRowHeight");
            //private double dishPanelAddButtonTextSize;
            // расстояния между строками панели блюда
            double dishPanelRowMargin1 = (double)AppLib.GetAppGlobalValue("dishPanelRowMargin1"),
                dishPanelRowMargin2 = (double)AppLib.GetAppGlobalValue("dishPanelRowMargin2");

            dGrid = new Grid();
            dGrid.Width = contentPanelWidth;
            dGrid.Height = currentContentPanelHeight;
            //dGrid.Background = Brushes.Blue;
            //   Определение строк
            // 0. строка заголовка
            dGrid.RowDefinitions.Add(new RowDefinition() { Height = new GridLength(dishPanelHeaderRowHeight, GridUnitType.Pixel) });
            // 1. разделитель
            dGrid.RowDefinitions.Add(new RowDefinition() { Height = new GridLength(dishPanelRowMargin1, GridUnitType.Pixel) });
            // 2. строка изображения
            dGrid.RowDefinitions.Add(new RowDefinition() { Height = new GridLength(dishPanelImageRowHeight, GridUnitType.Pixel) });
            // 3. разделитель
            dGrid.RowDefinitions.Add(new RowDefinition() { Height = new GridLength(dishPanelRowMargin2, GridUnitType.Pixel) });
            if (_hasGarnishes)
            {
                // 4. строка гарниров
                dGrid.RowDefinitions.Add(new RowDefinition() { Height = new GridLength(dishPanelGarnishesRowHeight, GridUnitType.Pixel) });
                // 5. разделитель
                dGrid.RowDefinitions.Add(new RowDefinition() { Height = new GridLength(dishPanelRowMargin2, GridUnitType.Pixel) });
            }
            // 6. строка кнопок
            dGrid.RowDefinitions.Add(new RowDefinition() { Height = new GridLength(dishPanelAddButtonRowHeight, GridUnitType.Pixel) });
        }  // method

        private void setDishPanelHeader()
        {
            // размер шрифтов
            double dishPanelHeaderFontSize = Convert.ToDouble(AppLib.GetAppGlobalValue("dishPanelHeaderFontSize"));
            double dishPanelUnitCountFontSize = Convert.ToDouble(AppLib.GetAppGlobalValue("dishPanelUnitCountFontSize"));

            List<Inline> inlines = new List<Inline>();
            if (_dishItem.Marks != null)
            {
                foreach (DishAdding markItem in _dishItem.Marks)
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
                Text = AppLib.GetLangText(_dishItem.langNames),
                FontWeight = FontWeights.Bold,
                FontSize = dishPanelHeaderFontSize
            });

            if (_dishItem.UnitCount != 0)
            {
                inlines.Add(new Run()
                {
                    Text = "  " + _dishItem.UnitCount.ToString(),
                    FontStyle = FontStyles.Italic,
                    FontSize = dishPanelUnitCountFontSize
                });
                inlines.Add(new Run()
                {
                    Text = " " + AppLib.GetLangText(_dishItem.langUnitNames),
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

        private void setDishDescription()
        {
            // размер шрифтов
            double dishPanelDescriptionFontSize = Convert.ToDouble(AppLib.GetAppGlobalValue("dishPanelDescriptionFontSize"));
            double dishPanelTextFontSize = Convert.ToDouble(AppLib.GetAppGlobalValue("dishPanelFontSize"));
            double dishPanelDescrButtonSize = (double)AppLib.GetAppGlobalValue("dishPanelDescrButtonSize");
            // размеры прямоугольника и углы закругления для изображения и описания блюда
            double dishImageHeight = (double)AppLib.GetAppGlobalValue("dishImageHeight");
            double dishImageWidth = (double)AppLib.GetAppGlobalValue("dishImageWidth");
            double dishImageCornerRadius = (double)AppLib.GetAppGlobalValue("cornerRadiusDishPanel");

            Rect rect = new Rect(0, 0, dGrid.Width, dishImageHeight);

            // изображение
            BitmapImage bi = null;
            if (_dishItem.Image == null)
            {
                string filePath = ImageHelper.GetFileNameBy(@"AppImages\noimage.png");
                if (System.IO.File.Exists(filePath) == true)
                {
                    bi = new BitmapImage(new Uri(filePath, UriKind.Absolute));
                }
            }
            else
            {
                bi = ImageHelper.ByteArrayToBitmapImage(_dishItem.Image);
            }
            if (bi != null)
            {
                _pathImage = new Path();
                _pathImage.Data = new RectangleGeometry(rect, dishImageCornerRadius, dishImageCornerRadius);
                _pathImage.Fill = new DrawingBrush(
                    new ImageDrawing() { ImageSource = bi, Rect = rect }
                    );
                //pathImage.Effect = new DropShadowEffect();
                // добавить в контейнер
                Grid.SetRow(_pathImage, 2); dGrid.Children.Add(_pathImage);
            }

            // кнопка отображения описания
            _btnDescr = new Border()
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
            _btnDescr.PreviewMouseLeftButtonUp += CanvDescr_PreviewMouseLeftButtonUp;

            //   буковка i
            TextBlock btnDescrText = new TextBlock(new Run("i"))
            {
                FontSize = dishPanelTextFontSize,
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center
            };
            _btnDescr.Child = btnDescrText;
            // добавить в контейнер
            Grid.SetRow(_btnDescr, 2); dGrid.Children.Add(_btnDescr);
            Grid.SetZIndex(_btnDescr, 10);

            // описание блюда
            LinearGradientBrush lgBrush = new LinearGradientBrush()
            {
                StartPoint = new Point(0.5, 0),
                EndPoint = new Point(0.5, 1)
            };
            lgBrush.GradientStops.Add(new GradientStop(Colors.Black, 0));
            lgBrush.GradientStops.Add(new GradientStop(Colors.Black, 0.5));
            lgBrush.GradientStops.Add(new GradientStop((Color)AppLib.GetAppResource("appColorDarkPink"), 1));

            _descrTextBorder = new Border()
            {
                Width = dGrid.Width,
                Height = dishImageHeight,
                CornerRadius = new CornerRadius(dishImageCornerRadius),
                Background = lgBrush,
                Opacity = 0,
                Visibility = Visibility.Hidden
            };
            _descrTextBorder.PreviewMouseLeftButtonUp += CanvDescr_PreviewMouseLeftButtonUp;
            // добавить в контейнер
            Grid.SetRow(_descrTextBorder, 2); dGrid.Children.Add(_descrTextBorder);

            _descrText = new TextBlock()
            {
                Width = dGrid.Width,
                Opacity = 0,
                Padding = new Thickness(dishPanelDescriptionFontSize),
                VerticalAlignment = VerticalAlignment.Center,
                TextAlignment = TextAlignment.Center,
                TextWrapping = TextWrapping.Wrap,
                FontSize = dishPanelDescriptionFontSize,
                Foreground = Brushes.White,
                Text = AppLib.GetLangText(_dishItem.langDescriptions),
                Visibility = Visibility.Hidden
            };
            _descrText.Effect = new BlurEffect() { Radius = 20 };
            _descrText.PreviewMouseLeftButtonUp += CanvDescr_PreviewMouseLeftButtonUp;

            // добавить в контейнер
            Grid.SetRow(_descrText, 2); dGrid.Children.Add(_descrText);
        }

        private void createGarnisheButtons()
        {
            double grnColWidth = Math.Ceiling(contentPanelWidth / 3d);
            double grnH = dGrid.RowDefinitions[4].Height.Value, grnW = 1.3 * grnH; // пропорции кнопки

            _grdGarnishes = new Grid();
            _grdGarnishes.ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(grnColWidth, GridUnitType.Pixel) });
            _grdGarnishes.ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(grnColWidth, GridUnitType.Pixel) });
            _grdGarnishes.ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(grnColWidth, GridUnitType.Pixel) });

            MainMenuGarnish grdGarn = new MainMenuGarnish(_dishItem, 0, grnH, grnW, dGrid);
            grdGarn.HorizontalAlignment = HorizontalAlignment.Left;
            grdGarn.SetValue(Grid.ColumnProperty, 0);
            grdGarn.SelectGarnish += GrdGarn_SelectGarnish;
            _grdGarnishes.Children.Add(grdGarn);

            if (_dishItem.Garnishes.Count >= 2)
            {
                grdGarn = new MainMenuGarnish(_dishItem, 1, grnH, grnW, dGrid);
                grdGarn.HorizontalAlignment = HorizontalAlignment.Center;
                grdGarn.SetValue(Grid.ColumnProperty, 1);
                grdGarn.SelectGarnish += GrdGarn_SelectGarnish;
                _grdGarnishes.Children.Add(grdGarn);
            }
            if (_dishItem.Garnishes.Count >= 3)
            {
                grdGarn = new MainMenuGarnish(_dishItem, 2, grnH, grnW, dGrid);
                grdGarn.HorizontalAlignment = HorizontalAlignment.Right;
                grdGarn.SetValue(Grid.ColumnProperty, 2);
                grdGarn.SelectGarnish += GrdGarn_SelectGarnish;
                _grdGarnishes.Children.Add(grdGarn);
            }

            Grid.SetRow(_grdGarnishes, 4); dGrid.Children.Add(_grdGarnishes);

        }  // method


        // здесь возможно три состояния:
        // - установить выделение гарнира в этом же или другом блюде
        // - снять выделение в этом же блюде
        private void GrdGarn_SelectGarnish(object sender, SelectGarnishEventArgs e)
        {
            // выбран гарнир
            if (e.Selected == true)
            {
                // снять выделение ранее выбранного гарнира
                // в том же блюде
                if (this._selectedGarnIndex > -1)
                {
                    getSelectedGarnish().IsSelected = false;
                }
                //  в другом блюде
                else
                {
                    (this.Parent as MainMenuDishesCanvas).ClearSelectedGarnish();
                }
                // установить выделение
                MainMenuGarnish currentGarnItem = (MainMenuGarnish)sender;
                currentGarnItem.IsSelected = true;
                if (this._selectedGarnIndex == -1)
                {
                    setAddButtonState(true);
                }
                this._selectedGarnIndex = e.GarnishIndex;
            }

            //
            else
            {
                setAddButtonState(false);
            }
        }  // method

        // кнопка добавления для блюд с гарнирами
        private void setAddButtonState(bool isActive)
        {
            if (_hasGarnishes == false) return;
            if (isActive == true)
            {
                _btnAddDish.Visibility = Visibility.Visible;
                _btnInvitation.Visibility = Visibility.Hidden;
            }
            else
            {
                _btnAddDish.Visibility = Visibility.Hidden;
                _btnInvitation.Visibility = Visibility.Visible;
            }
        }  // method

        public void ClearSelectedGarnish()
        {
            if (this._selectedGarnIndex > -1)
            {
                getSelectedGarnish().IsSelected = false;
            }
            setAddButtonState(false);
        }

        public void HideDescription()
        {
            if (_showDescription == true)
            {
                _showDescription = false;
                _dishDescrWithoutAnimation();
            }
        }

        private MainMenuGarnish getSelectedGarnish()
        {
            return (MainMenuGarnish)_grdGarnishes.Children[this._selectedGarnIndex];
        }

        private void CanvDescr_PreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            _showDescription = !_showDescription;

            if (_sbDescrShow == null)
                _dishDescrWithoutAnimation();  // без анимации
            else
                _dishDescrWithAnimation();   // с анимацией
        }


        private void setDishAddButton()
        {
            // высота строки кнопки добавления
            double dishPanelAddButtonRowHeight = (double)AppLib.GetAppGlobalValue("dishPanelAddButtonRowHeight");
            double cornerRadiusButton = (double)AppLib.GetAppGlobalValue("cornerRadiusButton");
            // размер шрифтов
            double dishPanelAddButtoFontSize = Convert.ToDouble(AppLib.GetAppGlobalValue("dishPanelAddButtoFontSize"));

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
            _btnAddDish = new Border()
            {
                Name = "btnAddDish",
                Tag = _dishItem.RowGUID.ToString(),
                VerticalAlignment = VerticalAlignment.Top,
                Width = dGrid.Width,
                Height = Math.Floor(0.7d * dishPanelAddButtonRowHeight),
                CornerRadius = new CornerRadius(cornerRadiusButton),
                Background = (Brush)AppLib.GetAppGlobalValue("addButtonBackgroundTextColor"),
                SnapsToDevicePixels = true,
                Effect = _shadowEffect
            };
            _btnAddDish.PreviewMouseLeftButtonUp += BtnAddDish_PreviewMouseLeftButtonUp;

            TextBlock tbText = new TextBlock()
            {
                Name = "tbAdd",
                Text = (string)AppLib.GetLangText((Dictionary<string, string>)AppLib.GetAppGlobalValue("btnSelectDishText")),
                VerticalAlignment = VerticalAlignment.Center,
                HorizontalAlignment = HorizontalAlignment.Center,
                FontSize = dishPanelAddButtoFontSize,
                Foreground = Brushes.White
            };

            if (_hasGarnishes == false)   // не Воки
            {
                // грид с кнопками цены и строки "Добавить", две колонки: с ценой и текстом
                Grid grdPrice = new Grid();
                grdPrice.ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(dGrid.Width / 3d, GridUnitType.Pixel) });
                grdPrice.ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(dGrid.Width * 2d / 3d, GridUnitType.Pixel) });

                Border brdPrice = new Border()
                {
                    VerticalAlignment = VerticalAlignment.Top,
                    Width = grdPrice.ColumnDefinitions[0].Width.Value,
                    Height = _btnAddDish.Height,
                    CornerRadius = new CornerRadius(cornerRadiusButton, 0, 0, cornerRadiusButton),
                    Background = (Brush)AppLib.GetAppGlobalValue("addButtonBackgroundPriceColor"),
                };
                TextBlock tbPrice = new TextBlock()
                {
                    Text = string.Format((string)AppLib.GetAppResource("priceFormatString"), _dishItem.Price),
                    VerticalAlignment = VerticalAlignment.Center,
                    HorizontalAlignment = HorizontalAlignment.Center,
                    FontSize = dishPanelAddButtoFontSize,
                    Foreground = Brushes.White
                };
                brdPrice.Child = tbPrice;
                Grid.SetColumn(brdPrice, 0); grdPrice.Children.Add(brdPrice);

                Grid.SetColumn(tbText, 1); grdPrice.Children.Add(tbText);
                _btnAddDish.Child = grdPrice;

                Grid.SetRow(_btnAddDish, 4); dGrid.Children.Add(_btnAddDish);
            }

            else   // Воки
            {
                // кнопка-приглашение
                _btnInvitation = new Border()
                {
                    Name = "btnInvitation",
                    VerticalAlignment = VerticalAlignment.Top,
                    Width = Math.Floor(dGrid.Width),
                    Height = Math.Floor(0.7d * dishPanelAddButtonRowHeight),
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
                _btnInvitation.Child = tbInvitation;

                _btnAddDish.Child = tbText; _btnAddDish.SetValue(Grid.RowProperty, 6);
                _btnAddDish.Visibility = Visibility.Hidden;
                dGrid.Children.Add(_btnAddDish);

                // добавить в контейнер
                _btnInvitation.Child = tbInvitation; _btnInvitation.SetValue(Grid.RowProperty, 6);
                dGrid.Children.Add(_btnInvitation);
            }

        }  // method

        #region добавить блюдо к заказу

        private void BtnAddDish_PreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            OrderItem _currentOrder = (OrderItem)AppLib.GetAppGlobalValue("currentOrder");

            // если нет ИНГРЕДИЕНТОВ, то сразу в корзину
            // т.к. блюда без гарниров тоже могут быть с ингредиентами (и рекомендациями)
            if ((_dishItem.Ingredients == null) || (_dishItem.Ingredients.Count == 0))
            {
                DishItem orderDish = _dishItem.GetCopyForOrder();
                _currentOrder.Dishes.Add(orderDish);
                // анимировать перемещение блюда в корзину
                animateDishToCart();
            }

            // иначе через "всплывашку"
            else
            {
                DishPopup popupWin = new DishPopup(_dishItem);    // текущее блюдо передать в конструкторе
                // размеры
                FrameworkElement pnlClient = (App.Current.MainWindow as WpfClient.MainWindow).scrollDishes;
                popupWin.Height = pnlClient.ActualHeight;
                popupWin.Width = pnlClient.ActualWidth;
                // и положение
                Point p = this.PointToScreen(new Point(0, 0));
                popupWin.Left = p.X;
                popupWin.Top = p.Y;

                popupWin.ShowDialog();

                // очистить выбранный гарнир
                getSelectedGarnish().IsSelected = false;
            } // if else

        }

        private void animateDishToCart()
        {
            WpfClient.MainWindow mainWin = (App.Current.MainWindow as WpfClient.MainWindow);
            if (_pathImage == null)
            {
                mainWin.updatePrice(); // обновить стоимость заказа
            }
            else
            {
                mainWin.animateSelectDish(_pathImage);  // анимированное перемещение изображения
            }
        }
        #endregion

        #region description animations

        private double getDuration()
        {
            double retVal = 0;
            string s = AppLib.GetAppSetting("ShowDishDescrAnimationSpeed");
            if (s == null) return retVal;
            if (double.TryParse(s, out retVal) == false) return 0;
            return retVal;
        }

        private Storyboard getDescrStoryboard(bool isShow, double duration)
        {
            PropertyPath propPathOpacity = new PropertyPath(UIElement.OpacityProperty);

            Storyboard sb = new Storyboard() { Duration = TimeSpan.FromMilliseconds(duration) };

            // прозрачность рамки текста
            DoubleAnimation daBorderOpacity = new DoubleAnimation()
            {
                From = (isShow)?0:0.6, To = (isShow)?0.6:0,
                EasingFunction = new CubicEase() { EasingMode = EasingMode.EaseOut }
            };
            Storyboard.SetTarget(daBorderOpacity, _descrTextBorder);
            Storyboard.SetTargetProperty(daBorderOpacity, propPathOpacity);
            sb.Children.Add(daBorderOpacity);

            // прозрачность текста
            DoubleAnimation daTextOpacity = new DoubleAnimation()
            { From = (isShow)?0:1, To = (isShow)?1:0 };
            Storyboard.SetTarget(daTextOpacity, _descrText);
            Storyboard.SetTargetProperty(daTextOpacity, propPathOpacity);
            sb.Children.Add(daTextOpacity);

            // прозрачность и расплывчатость текста
            DoubleAnimation daTextBlur = new DoubleAnimation()
            { From = (isShow)?20:0, To = (isShow)?0:20 };
            Storyboard.SetTarget(daTextBlur, _descrText);
            Storyboard.SetTargetProperty(daTextBlur, new PropertyPath("(TextBlock.Effect).(BlurEffect.Radius)"));
            sb.Children.Add(daTextBlur);

            return sb;
        }

        private void _dishDescrWithAnimation()
        {
            // цвет фона кнопки описания блюда
            _btnDescr.Background = (_showDescription == true) ? _brushSelectedItem : Brushes.White;

            // видимость описания
            if (_showDescription == true) 
            {
                // сделать видимым ДО анимации
                _descrTextBorder.Visibility = Visibility.Visible;
                _descrText.Visibility = Visibility.Visible;

                _sbDescrShow.Begin();
            }
            else
            {
                _sbDescrHide.Begin();
            }

        }  // method

        private void _dishDescrWithoutAnimation()
        {
            if (_showDescription == true)
            {
                _btnDescr.Background = _brushSelectedItem;
                _descrTextBorder.Visibility = Visibility.Visible;
                _descrTextBorder.Opacity = 0.6;
                _descrText.Visibility = Visibility.Visible;
                _descrText.Opacity = 1;

                if ((_descrText.Effect != null) && (_descrText.Effect is BlurEffect))
                {
                    BlurEffect be = (_descrText.Effect as BlurEffect);
                    if (be.Radius != 0) be.Radius = 0;
                }
            }
            else
            {
                _btnDescr.Background = Brushes.White;
                _descrTextBorder.Visibility = Visibility.Hidden;
                _descrTextBorder.Opacity = 0;
                _descrText.Visibility = Visibility.Hidden;
                _descrText.Opacity = 0;
            }
        }

        private void _sbDescrHide_Completed(object sender, EventArgs e)
        {
            _descrTextBorder.Visibility = Visibility.Hidden;
            _descrText.Visibility = Visibility.Hidden;
        }

        #endregion

        public void ResetLang()
        {
            List<TextBlock> tbList = AppLib.FindLogicalChildren<TextBlock>(dGrid).ToList();

            // заголовок (состоит из элементов Run)
            var hdRuns = tbList[0].Inlines.Where(t => (t is Run)).ToList();
            if (hdRuns.Count >= 0)
            {
                ((Run)hdRuns[0]).Text = AppLib.GetLangText(_dishItem.langNames);
            }
            if (hdRuns.Count >= 3)
            {
                ((Run)hdRuns[2]).Text = " " + AppLib.GetLangText(_dishItem.langUnitNames);
            }

            // tbList[1] - буковка i на кнопке отображения описания
            
            // описание блюда
            tbList[2].Text = AppLib.GetLangText(_dishItem.langDescriptions);

            // кнопка Добавить с тенью
            TextBlock tbAdd = tbList.First(t => t.Name == "tbAdd");
            if (tbAdd != null) tbAdd.Text = (string)AppLib.GetLangText((Dictionary<string, string>)AppLib.GetAppGlobalValue("btnSelectDishText"));

            if (_hasGarnishes == true)
            {
                TextBlock tbInv = tbList.First(t => t.Name == "tbInvitation");
                if (tbInv != null) tbInv.Text = (string)AppLib.GetLangText((Dictionary<string, string>)AppLib.GetAppGlobalValue("btnSelectGarnishText"));

                foreach (MainMenuGarnish garn in _grdGarnishes.Children)
                {
                    garn.ResetLangName();
                }
            } 
        }  //  method


    }  // class
}
