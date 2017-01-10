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

namespace Geometry
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private bool btnDishDescrPressed = false;

        private SolidColorBrush brushWhite = new SolidColorBrush(Colors.White);
        private SolidColorBrush brushYellow = new SolidColorBrush(Colors.Yellow);
        private SolidColorBrush brushMagenda = new SolidColorBrush(Colors.DarkMagenta);

        private List<Viewbox> _garnList;

        private bool isSignDown = true;

        public MainWindow()
        {
            InitializeComponent();

            var v1 = System.Configuration.ConfigurationManager.AppSettings;
            System.Configuration.ConfigurationManager.AppSettings.Set("newAppSettong", "nnnnnn");


            _garnList = new List<Viewbox>();
            foreach (Viewbox item in FindLogicalChildren<Viewbox>(this))
            {
                if (item.Name.Contains("btnGarn")) _garnList.Add(item);
            }

            setScrollSignStatus();
        }

        private void setScrollSignStatus()
        {
            signDown.Visibility = (isSignDown) ? Visibility.Visible : Visibility.Hidden;
            signUp.Visibility = (isSignDown) ? Visibility.Hidden: Visibility.Visible;
        }

        private void btnDishDescr_PreviewMouseUp(object sender, MouseButtonEventArgs e)
        {
            btnDishDescrPressed = !btnDishDescrPressed;

            bool btnDishDescrCurrentPressed = ((SolidColorBrush)btnDishDescr.Fill).Color != Colors.White;
            if (btnDishDescrCurrentPressed != btnDishDescrPressed)
            {
                btnDishDescr.Fill = (btnDishDescrPressed) ? brushYellow : brushWhite;
            }
        }

        private void btnGarnish_PreviewMouseUp(object sender, MouseButtonEventArgs e)
        {
            setGarnishState((FrameworkElement)sender);
        }

        private void setGarnishState(FrameworkElement btnGarnish)
        {
            int btnPreIndex = getPressedGarnishIndex();
            int btnCurIndex = int.Parse(btnGarnish.Uid);

            if (btnPreIndex != -1) switchGarnState(btnPreIndex);
            if (btnPreIndex != btnCurIndex) switchGarnState(btnCurIndex);

        }
        private void switchGarnState(int gIndex)
        {
            FrameworkElement fe = _garnList[gIndex];
            bool b = fe.Tag.ToString() == "0";
            fe.Tag = (b) ? "1" : "0"; b = !b;

            Path p = FindLogicalChildren<Path>(fe).FirstOrDefault();
            if (p != null) p.Fill = (b) ? brushMagenda : brushYellow;
        }

        private int getPressedGarnishIndex()
        {
            int retVal = -1;
            foreach (FrameworkElement item in _garnList)
            {
                retVal++;
                if (item.Tag.ToString() == "1") return retVal;
            }
            return -1;
        }

        public static IEnumerable<T> FindLogicalChildren<T>(DependencyObject depObj) where T : DependencyObject
        {
            if (depObj != null)
            {
                foreach (object rawChild in LogicalTreeHelper.GetChildren(depObj))
                {
                    if (rawChild is DependencyObject)
                    {
                        DependencyObject child = (DependencyObject)rawChild;
                        if (child is T)
                        {
                            yield return (T)child;
                        }

                        foreach (T childOfChild in FindLogicalChildren<T>(child))
                        {
                            yield return childOfChild;
                        }
                    }
                }
            }
        }

        private void Canvas_MouseDown(object sender, MouseButtonEventArgs e)
        {
            isSignDown = !isSignDown;
            setScrollSignStatus();
        }
    }
}
