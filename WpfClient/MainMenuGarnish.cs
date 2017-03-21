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
        
        // кисти фона
        private Brush _selectBackgroundBrush;
        private Brush _notSelectBackgroundBrush;
        // кисти текста
        private Brush _selectTextBrush;
        private Brush _notSelectTextBrush;


        double _fontSize, _fontSizeUp;

        private double _height, _width;
        private DishItem _dishItem;
        private int _garnIndex;
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
                setSelectionMode();
            }
        }

        public ImageBrush DishWithGarnishImageBrush;
        public Brush SelectedBackgroundBrush { get { return _selectBackgroundBrush; } set { _selectBackgroundBrush = value; } }
        public Brush NotSelectedBackgroundBrush { get { return _notSelectBackgroundBrush; } set { _notSelectBackgroundBrush = value; } }
        public Brush SelectedTextBrush { get { return _selectTextBrush; } set { _selectTextBrush = value; } }
        public Brush NotSelectedTextBrush { get { return _notSelectTextBrush; } set { _notSelectTextBrush = value; } }

        public event EventHandler<SelectGarnishEventArgs> SelectGarnish;


        public MainMenuGarnish(DishItem dishItem, int garnIndex, double garnHeight, double garnWidth, Grid dishContentPanel)
        {
            _dishItem = dishItem;
            _garnIndex = garnIndex;
            _garnItem = _dishItem.Garnishes[garnIndex];
            _height = garnHeight;
            _width = garnWidth;
            _contentPanel = dishContentPanel;

            _fontSize = (double)AppLib.GetAppGlobalValue("appFontSize5");
            _fontSizeUp = (double)AppLib.GetAppGlobalValue("appFontSize4");

            _selectBackgroundBrush = (Brush)AppLib.GetAppGlobalValue("appSelectedItemColor");
            _notSelectBackgroundBrush = (Brush)AppLib.GetAppGlobalValue("garnishBackgroundColor");
            _selectTextBrush = (Brush)AppLib.GetAppGlobalValue("addButtonBackgroundPriceColor");
            _notSelectTextBrush = Brushes.Black;
            //_shadow = new DropShadowEffect() { BlurRadius = 5, Opacity = 0.5, ShadowDepth = 1 };

            _isSelected = false;

            this.DishWithGarnishImageBrush = new ImageBrush() { ImageSource = _garnItem.ImageDish };
            createGarnishButton();

            base.MouseUp += MainMenuGarnish_PreviewMouseDown;
        }

        public void ResetLangName()
        {
            _tbGarnishName.Text = (string)AppLib.GetLangText((Dictionary<string, string>)_garnItem.langNames);
        }

        private void MainMenuGarnish_PreviewMouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (AppLib.IsDrag) return;

            _isSelected = !_isSelected;
            setSelectionMode();
            // передать уведомление в панель блюда
            if (SelectGarnish != null)
            {
                SelectGarnish(this, new SelectGarnishEventArgs(_isSelected, _garnIndex, AppLib.GetLangText(_garnItem.langDishDescr)));
            }
        }

        private void setSelectionMode()
        {
            _pathSelected.Visibility = (_isSelected) ? Visibility.Visible : Visibility.Collapsed;

            _tbGarnishName.Foreground = (_isSelected) ? _selectTextBrush : _notSelectTextBrush;
            _tbGarnishName.FontWeight = (_isSelected) ? FontWeights.Bold : FontWeights.Normal;

            _tbGarnishPrice.FontSize = (_isSelected) ? 1.1 * _fontSize : _fontSize;
            _tbGarnishPrice.Foreground = (_isSelected) ? _selectTextBrush : Brushes.Black;
            //_tbGarnishPrice.Effect = (_isSelected) ? _shadow: null;

            // логический уровень
            if (((_dishItem.SelectedGarnishes.Count == 1) && (_dishItem.SelectedGarnishes[0].Equals(_garnItem))))
            {
                // если текущий гарнир уже есть, то ничего не делаем
            }
            else
            {
                // иначе очищаем
                if (_dishItem.SelectedGarnishes.Count > 0) _dishItem.SelectedGarnishes.Clear();
                // и добавляем текущий
                if (_isSelected == true) _dishItem.SelectedGarnishes.Add(_garnItem);
            }
        }

        private void createGarnishButton()
        {
            base.Height = _height; base.Width = _width;

            // подложка кнопки
            Path _pathBase = getGarnPath(_width, _height);
            //if (_garnItem.Image == null)
            //{
            //    _pathBase.Fill = _notSelectBrush;
            //}
            //else
            //{
            _pathBase.Fill = new ImageBrush() { ImageSource = _garnItem.Image };
            _pathBase.Stretch = Stretch.UniformToFill;
            string sVal = (string)AppLib.GetAppGlobalValue("dishPanelGarnishBrightness");
            _pathBase.Opacity = (sVal == null) ? 1.0 : sVal.ToDouble();
            //}
            base.Children.Add(_pathBase);

            // выделение кнопки
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
                Foreground = _notSelectTextBrush
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

    public class SelectGarnishEventArgs : EventArgs
    {
        public bool Selected { get; set; }
        public int GarnishIndex { get; set; }
        public string DishWithGarnishDescription;

        public SelectGarnishEventArgs(bool selected, int garnIndex, string dishWithGarnDescr)
        {
            this.Selected = selected;
            this.GarnishIndex = garnIndex;
            this.DishWithGarnishDescription = dishWithGarnDescr;
        }
    }

}
