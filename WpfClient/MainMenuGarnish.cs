using AppModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Effects;
using System.Windows.Shapes;

namespace WpfClient
{
    // создает Grid с готовыми элементами: Path и два TextBox-а
    public class MainMenuGarnish: Grid
    {
        private bool _isSelected;
        //private Brush _selectBrush;
        //private Brush _notSelectBrush;
        private Brush _selectTextBrush;
        double _fontSize, _fontSizeUp;

        private double _height, _width;
        private DishItem _dishItem;
        private DishAdding _garnItem;
        private Path _pathSelected;
        private TextBlock _tbGarnishName;
        private TextBlock _tbGarnishPrice;
        private Grid _contentPanel;

        //private DropShadowEffect _shadow;

        public bool IsSelected {
            get { return _isSelected; }
            set
            {
                _isSelected = value;
                switchSelectMode();
            }
        }

        public MainMenuGarnish(DishItem dishItem, int garnIndex, double garnHeight, double garnWidth, Grid dishContentPanel)
        {
            _dishItem = dishItem;
            _garnItem = _dishItem.Garnishes[garnIndex];
            _height = garnHeight;
            _width = garnWidth;
            _contentPanel = dishContentPanel;

            _fontSize = (double)AppLib.GetAppGlobalValue("appFontSize5");
            _fontSizeUp = (double)AppLib.GetAppGlobalValue("appFontSize4");

            //_notSelectBrush = (Brush)AppLib.GetAppGlobalValue("selectGarnishBackgroundColor");
            _selectTextBrush = (Brush)AppLib.GetAppGlobalValue("addButtonBackgroundPriceColor");
            //_shadow = new DropShadowEffect() { BlurRadius = 5, Opacity = 0.5, ShadowDepth = 1 };

            _isSelected = false;

            createGarnishButton();

            base.MouseDown += MainMenuGarnish_PreviewMouseDown;
        }

        public void ResetLangName()
        {
            _tbGarnishName.Text = (string)AppLib.GetLangText((Dictionary<string, string>)_garnItem.langNames);
        }

        private void MainMenuGarnish_PreviewMouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            switchSelectModeByClicked();
        }

        private void switchSelectModeByClicked()
        {
            // если текущий не выбран, то очистить имеющиеся
            if (_isSelected == false)
            {
                //if (Selecting != null) Selecting(_canvas);
                clearSelectedGarnish();
            }

            _isSelected = !_isSelected;
            switchSelectMode();
        }

        private void clearSelectedGarnish()
        {
            Canvas currentCanvas = ((Grid)_contentPanel.Parent).Parent as Canvas;
            List<MainMenuGarnish> v = AppLib.FindLogicalChildren<MainMenuGarnish>(currentCanvas).ToList();
            if (v != null)
            {
                foreach (MainMenuGarnish item in v)
                {
                    if (item.IsSelected == true) item.IsSelected = false;
                }
            }

        }

        private void switchSelectMode()
        {
            _pathSelected.Visibility = (_isSelected) ? Visibility.Visible : Visibility.Collapsed;

            _tbGarnishName.Foreground = (_isSelected) ? Brushes.Black : Brushes.White;
            _tbGarnishName.FontWeight = (_isSelected) ? FontWeights.Bold : FontWeights.Normal;

            _tbGarnishPrice.FontSize = (_isSelected) ? 1.1 * _fontSize : _fontSize;
            _tbGarnishPrice.Foreground = (_isSelected) ? _selectTextBrush : Brushes.Black;
            //_tbGarnishPrice.Effect = (_isSelected) ? _shadow: null;

            // кнопка добавления
            List<MainMenuGarnish> mmGarn = AppLib.FindLogicalChildren<MainMenuGarnish>(_contentPanel).ToList();
            bool isAdd = mmGarn.Any(g => g.IsSelected == true);
            List<Border> btnList = AppLib.FindLogicalChildren<Border>(_contentPanel).ToList();
            Border btnAddDish = btnList.FirstOrDefault<Border>(b => b.Name == "btnAddDish");
            Border btnInvitation = btnList.FirstOrDefault<Border>(b => b.Name == "btnInvitation");
            if ((btnAddDish != null) && (btnInvitation != null))
            {
                if (isAdd == true)
                {
                    btnAddDish.Visibility = Visibility.Visible;
                    btnInvitation.Visibility = Visibility.Hidden;
                }
                else
                {
                    btnAddDish.Visibility = Visibility.Hidden;
                    btnInvitation.Visibility = Visibility.Visible;
                }
            }

            // логический уровень
            if (_isSelected == true)
            {
                _dishItem.SelectedGarnishes.Add(_garnItem);
            }
            else
            {
                _dishItem.SelectedGarnishes.Remove(_garnItem);
            }
        }

        private void createGarnishButton()
        {
            base.Height = _height; base.Width = _width;

            Path _path = getGarnPath(_width, _height);
            //_notSelectBrush = new ImageBrush() { ImageSource = ImageHelper.ByteArrayToBitmapImage(_garnItem.Image) };
            //_path.Fill = _notSelectBrush;
            //_path.Fill = new DrawingBrush( new ImageDrawing()
            //    { ImageSource = ImageHelper.ByteArrayToBitmapImage(_garnItem.Image), Rect = new Rect(0,0,_width,_height) });
            _path.Fill = new ImageBrush() { ImageSource = ImageHelper.ByteArrayToBitmapImage(_garnItem.Image) };
            base.Children.Add(_path);

            _pathSelected = getGarnPath(_width, _height);
            Color c = ((SolidColorBrush)AppLib.GetAppGlobalValue("appSelectedItemColor")).Color;
            Color c1 = new Color(); c1.A = 0xAA; c1.R = c.R; c1.G = c.G; c1.B = c.B;
            _pathSelected.Fill = new SolidColorBrush(c1);
            _pathSelected.Visibility = Visibility.Collapsed;
            base.Children.Add(_pathSelected);

            double dMarg = 0.05 * _width;
            string grnText = (string)AppLib.GetLangText((Dictionary<string, string>)_garnItem.langNames);
            _tbGarnishName = new TextBlock()
            {
                Text = grnText,
                Width = _width - (2 * dMarg),
                TextWrapping = TextWrapping.Wrap,
                VerticalAlignment = VerticalAlignment.Top,
                TextAlignment = TextAlignment.Center,
                FontSize = _fontSize,
                Margin = new Thickness(dMarg),
                Foreground = Brushes.White
            };
            base.Children.Add(_tbGarnishName);

            string grnPrice = string.Format((string)AppLib.GetAppResource("priceFormatString"), _garnItem.Price);
            _tbGarnishPrice = new TextBlock()
            {
                Text = grnPrice,
                Width = _width,
                VerticalAlignment = VerticalAlignment.Bottom,
                TextAlignment = TextAlignment.Center,
                FontSize = _fontSize,
                FontWeight = FontWeights.Bold,
                Foreground = Brushes.Black
            };
            base.Children.Add(_tbGarnishPrice);
        }

        private Path getGarnPath(double grnW, double grnH)
        {
            Path grnPath = new Path();
            double grnBorderRadius = Math.Min(grnH, grnW) * 0.1;
            double grnPriceCircle = Math.Min(grnH, grnW) * 0.3d;

            RectangleGeometry rectGeom = new RectangleGeometry(new Rect(0, 0, grnW, grnH), grnBorderRadius, grnBorderRadius);
            EllipseGeometry ellGeom = new EllipseGeometry(new Point(grnW / 2d, grnH), grnPriceCircle, grnPriceCircle);
            CombinedGeometry combGeom = new CombinedGeometry(GeometryCombineMode.Exclude, rectGeom, ellGeom);
            grnPath.Data = combGeom;
            grnPath.Tag = 0;

            //pathImage.Effect = new DropShadowEffect();

            return grnPath;
        }

    }  // end class

}
