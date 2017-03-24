using System;
using System.Collections.Generic;
using System.Diagnostics;
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


namespace EventsExplorer
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        List<SimpleData> _dataList;

        int _lineNum = 0;
        // флажки событий
        bool _onlyNamed, _preview, _move, _touch, _enter, _mouseButton, _mouseWheel;


        public MainWindow()
        {
            InitializeComponent();

            createData();
        }


        private void createData()
        {
            _dataList = new List<SimpleData>();
            _dataList.Add(new SimpleData() { Id=1, Name="Item 1", SubList = {"subItem 1","subItem 2","subItem 3","subItem 4" } });
            _dataList.Add(new SimpleData() { Id=2, Name="Item 2", SubList = {"color11","color22","color33" } });
            _dataList.Add(new SimpleData() { Id=3, Name="Item 3" });
            _dataList.Add(new SimpleData() { Id=4, Name="Item 4", SubList = {"ingredient 1", "ingredient 2", "ingredient 3" } });
            _dataList.Add(new SimpleData() { Id=5, Name="Item 5" });

            lbData.ItemsSource = _dataList;
        }


        #region event handlers
        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
        private void btnClearEventsList_Click(object sender, RoutedEventArgs e)
        {
            _lineNum = 0;
            txtEvents.Clear();
        }
        private void button_ClickHandler(object sender, RoutedEventArgs e)
        {
            outActionText(sender, e);
        }

        private void border_MouseHandler(object sender, MouseEventArgs e)
        {
            outActionText(sender, e);
        }
        private void border_MouseButtonHandler(object sender, MouseButtonEventArgs e)
        {
            outActionText(sender, e);
        }
        private void border_MouseWheelHandler(object sender, MouseWheelEventArgs e)
        {
            if (_mouseWheel) outActionText(sender, e);
        }

        private void border_TouchHandler(object sender, TouchEventArgs e)
        {
            if (_touch) outActionText(sender, e);
        }

        private void listBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ListBox lb = (sender as ListBox);
            outActionText(string.Format("{0}, select item {1}", lb.Name, lb.SelectedIndex.ToString()));
        }

        #endregion

        #region check boxes
        private void chkOnlyNamed_Checked(object sender, RoutedEventArgs e)
        {
            _onlyNamed = chkOnlyNamed.IsChecked ?? false;
        }
        private void chkOnlyNamed_Unchecked(object sender, RoutedEventArgs e)
        {
            _onlyNamed = chkOnlyNamed.IsChecked ?? false;
        }

        private void chkEnter_Checked(object sender, RoutedEventArgs e)
        {
            _enter = chkEnter.IsChecked ?? false;
        }

        private void chkMouseButton_Checked(object sender, RoutedEventArgs e)
        {
            _mouseButton = chkMouseBotton.IsChecked ?? false;
        }

        private void chkPreview_Unchecked(object sender, RoutedEventArgs e)
        {
            _preview = chkPreview.IsChecked ?? false;
        }

        private void chkMouseMove_Unchecked(object sender, RoutedEventArgs e)
        {
            _move = chkMouseMove.IsChecked ?? false;
        }

        private void chkTouch_Unchecked(object sender, RoutedEventArgs e)
        {
            _touch = chkTouch.IsChecked ?? false;
        }

        private void chkEnter_Unchecked(object sender, RoutedEventArgs e)
        {
            _enter = chkEnter.IsChecked ?? false;
        }

        private void chkMouseBotton_Unchecked(object sender, RoutedEventArgs e)
        {
            _mouseButton = chkMouseBotton.IsChecked ?? false;
        }

        private void chkMouseWheel_Unchecked(object sender, RoutedEventArgs e)
        {
            _mouseWheel = chkMouseWheel.IsChecked ?? false;
        }

        private void chkPreview_Checked(object sender, RoutedEventArgs e)
        {
            _preview = chkPreview.IsChecked ?? false;
        }

        private void chkMouseMove_Checked(object sender, RoutedEventArgs e)
        {
            _move = chkMouseMove.IsChecked ?? false;
        }

        private void chkMouseWheel_Checked(object sender, RoutedEventArgs e)
        {
            _mouseWheel = chkMouseWheel.IsChecked ?? false;
        }

        private void chkTouch_Checked(object sender, RoutedEventArgs e)
        {
            _touch = chkTouch.IsChecked ?? false;
        }


        #endregion

        #region out to log
        private void outActionText(object sender, RoutedEventArgs e)
        {
            FrameworkElement fe = (sender as FrameworkElement);
            string elName = fe.Name;

            if (_onlyNamed && string.IsNullOrEmpty(fe.Name)) return;
            if (e.RoutedEvent.Name.StartsWith("Preview") && (_preview == false)) return;
            if (e.RoutedEvent.Name.Contains("Move") && (_move == false)) return;

            if (e.RoutedEvent.Name.Contains("Enter") && (_enter == false)) return;
            if (e.RoutedEvent.Name.Contains("Leave") && (_enter == false)) return;

            if (e.RoutedEvent.Name.Contains("Button") && (_mouseButton == false)) return;

            if (string.IsNullOrEmpty(elName)) elName = fe.GetType().Name;
            string msg = string.Format("{0}: {1}", elName, e.RoutedEvent.Name);

            this.txtEvents.Text += string.Format("\n{0}. {1}", ++_lineNum, msg); ;
            txtEvents.PageDown();
        }
        private void outActionText(string msg)
        {
            this.txtEvents.Text += string.Format("\n{0}. {1}", ++_lineNum, msg); ;
            txtEvents.PageDown();
        }

        #endregion

    }  // class

}
