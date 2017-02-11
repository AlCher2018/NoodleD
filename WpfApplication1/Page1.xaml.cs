using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
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

namespace WpfApplication1
{
    /// <summary>
    /// Interaction logic for Page1.xaml
    /// </summary>
    public partial class Page1 : Page
    {
        UserActionLog.UserActionIdle _userIdle;
        UserActionLog.UserActionsLog _userActionLog;

        public Page1()
        {
            InitializeComponent();
            
            //FrameworkElement[] felArr = new FrameworkElement[] 
            //{
            //    page1, listBox1, txtBlock01, txtBlock02, txtBox01, img, btnAddHandlers, btnDelHandlers
            //};
            FrameworkElement[] felArr = new FrameworkElement[]
            {
                listBox1, btnAddHandlers, btnDelHandlers
            };
            _userActionLog = new UserActionsLog(felArr, EventsMouseEnum.Bubble | EventsMouseEnum.Buttons, EventsKeyboardEnum.None, EventsTouchEnum.None);
            _userActionLog.ActionEventHandler += _userActionLog_ActionEventHandler;
            _userActionLog.ParentName = this.Name;

            //_userIdle = new UserActionLog.UserActionIdle(3);
            //_userIdle.AnyActionWindow = this;
            ////_userIdle.ActionEventHandler += _userAction_ActionEventHandler;
            //_userIdle.IdleElapseEvent += _userAction_IdleElapseEvent;

            Zen.Barcode.CodeEan13BarcodeDraw bc = Zen.Barcode.BarcodeDrawFactory.CodeEan13WithChecksum;
            System.Drawing.Image imageBC = bc.Draw("012701110123", 50);

            MemoryStream ms = new MemoryStream();
            imageBC.Save(ms, System.Drawing.Imaging.ImageFormat.Png);
            ms.Position = 0;
            BitmapImage bi = new BitmapImage();
            bi.BeginInit();
            bi.StreamSource = ms;
            bi.EndInit();

            img.Source = bi;
            //bmp.Dispose(); //if bmp is not used further. Thanks @Peter

        }   // Page

        private void _userActionLog_ActionEventHandler(object sender, UserActionEventArgs e)
        {
            Debug.Print(string.Format("{0}: {1} ({2}), event {3}, value \"{4}\"",this.Name,e.ControlName,e.ControlType.Name, e.EventName, e.Value));
        }

        private void _userAction_IdleElapseEvent(System.Timers.ElapsedEventArgs obj)
        {
            Debug.Print(string.Format("idleEvent time - {0}", obj.SignalTime.ToString()));
        }


        private void btnAddHandlers_Click(object sender, RoutedEventArgs e)
        {
            //_userIdle.AddHandler(img, "MouseLeftButtonDown");
            //_userIdle.AddHandler(this, "PreviewMouseUp");
        }

        private void btnDelHandlers_Click(object sender, RoutedEventArgs e)
        {
            //_userIdle.ReleaseEvents();
        }

    }  // class

}
