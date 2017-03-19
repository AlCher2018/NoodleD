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

using UserActionLog;

namespace UserActionLogTester
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        List<SimpleData> _dataList;
        UserActionsLog _actionLogger;

        int _lineNum = 0;

        public MainWindow()
        {
            InitializeComponent();

            createData();

            createLogger();
        }

        protected override void OnClosed(EventArgs e)
        {
            _actionLogger.Close();
            base.OnClosed(e);
        }

        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }


        private void createLogger()
        {
            FrameworkElement[] fElements = new FrameworkElement[] { this.borderTest, this.buttonTest, lbData};
            EventsMouseEnum mouseEvents = EventsMouseEnum.Tunnel | EventsMouseEnum.Bubble | EventsMouseEnum.Buttons;
            EventsKeyboardEnum keybrdEvents = EventsKeyboardEnum.All;
            EventsTouchEnum touchEvents = EventsTouchEnum.All;
            
            _actionLogger = new UserActionsLog(fElements, mouseEvents, keybrdEvents, touchEvents, LogFilesPathLocationEnum.App);
            _actionLogger.ActionEventHandler += _actionLogger_ActionEventHandler;
            _actionLogger.ParentName = "MainWindow";
            _actionLogger.Enabled = true;
        }

        private void _actionLogger_ActionEventHandler(object sender, UserActionEventArgs e)
        {
            string msg = e.ControlName + " " + e.EventName;
            outActionText(msg);
        }

        private void outActionText(string msg)
        {
            this.txtEvents.Text += string.Format("\n{0}. {1}", ++_lineNum, msg); ;
            txtEvents.PageDown();
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

        private void btnClearEventsList_Click(object sender, RoutedEventArgs e)
        {
            _lineNum = 0;
            txtEvents.Clear();
        }

        private void lbData_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            outActionText("lblData select item " + lbData.SelectedIndex.ToString());
        }

        private void lbDataBorder_MouseDown(object sender, MouseButtonEventArgs e)
        {
            outActionText("lbDataBorder: MouseDown");
        }

        private void lbDataBorder_MouseEnter(object sender, MouseEventArgs e)
        {
            outActionText("lbDataBorder: MouseEnter");
        }

        private void lbDataBorder_MouseLeave(object sender, MouseEventArgs e)
        {
            outActionText("lbDataBorder: MouseLeave");
        }

        private void lbDataBorder_MouseMove(object sender, MouseEventArgs e)
        {
            //outActionText("lbDataBorder: MouseMove");
        }

        private void lbDataBorder_MouseUp(object sender, MouseButtonEventArgs e)
        {
            outActionText("lbDataBorder: MouseUp");
        }

        private void lbDataBorder_TouchDown(object sender, TouchEventArgs e)
        {
            outActionText("lbDataBorder: TouchDown");
        }

        private void lbDataBorder_TouchEnter(object sender, TouchEventArgs e)
        {
            outActionText("lbDataBorder: TouchEnter");
        }

        private void lbDataBorder_TouchLeave(object sender, TouchEventArgs e)
        {
            outActionText("lbDataBorder: TouchLeave");
        }

        private void lbDataBorder_TouchMove(object sender, TouchEventArgs e)
        {
            //outActionText("lbDataBorder: TouchMove");
        }

        private void lbDataBorder_TouchUp(object sender, TouchEventArgs e)
        {
            outActionText("lbDataBorder: TouchUp");
        }

        private void lbSubBrd_MouseDown(object sender, MouseButtonEventArgs e)
        {
            outActionText("lbSubBrd: MouseDown");
        }

        private void lbSubBrd_MouseEnter(object sender, MouseEventArgs e)
        {
            outActionText("lbSubBrd: MouseEnter");
        }

        private void lbSubBrd_MouseLeave(object sender, MouseEventArgs e)
        {
            outActionText("lbSubBrd: MouseLeave");
        }

        private void lbSubBrd_MouseUp(object sender, MouseButtonEventArgs e)
        {
            outActionText("lbSubBrd: MouseUp");
        }

        private void lbSubBrd_TouchDown(object sender, TouchEventArgs e)
        {
            outActionText("lbSubBrd: TouchDown");
        }

        private void lbSubBrd_TouchEnter(object sender, TouchEventArgs e)
        {
            outActionText("lbSubBrd: TouchEnter");
        }

        private void lbSubBrd_TouchLeave(object sender, TouchEventArgs e)
        {
            outActionText("lbSubBrd: TouchLeave");
        }

        private void lbSubBrd_TouchUp(object sender, TouchEventArgs e)
        {
            outActionText("lbSubBrd: TouchUp");
        }

        private void lbDataSubList_MouseDown(object sender, MouseButtonEventArgs e)
        {
            outActionText("lbDataSubList: MouseDown");
        }

        private void lbDataSubList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            outActionText("lbDataSubList select item " + (sender as ListBox).SelectedIndex.ToString());
        }

        private void lbDataSubList_MouseEnter(object sender, MouseEventArgs e)
        {
            outActionText("lbDataSubList: MouseEnter");
        }

        private void lbDataSubList_MouseLeave(object sender, MouseEventArgs e)
        {
            outActionText("lbDataSubList: MouseLeave");
        }

        private void lbDataSubList_MouseUp(object sender, MouseButtonEventArgs e)
        {
            outActionText("lbDataSubList: MouseUp");
        }

        private void lbDataSubList_TouchDown(object sender, TouchEventArgs e)
        {
            outActionText("lbDataSubList: TouchDown");
        }

        private void lbDataSubList_TouchEnter(object sender, TouchEventArgs e)
        {
            outActionText("lbDataSubList: TouchEnter");
        }

        private void lbDataSubList_TouchLeave(object sender, TouchEventArgs e)
        {
            outActionText("lbDataSubList: TouchLeave");
        }

        private void lbDataSubList_TouchUp(object sender, TouchEventArgs e)
        {
            outActionText("lbDataSubList: TouchUp");
        }
    }  // class



}
