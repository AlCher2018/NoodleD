using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace WpfClient
{
    /// <summary>
    /// Interaction logic for MessageBox.xaml
    /// </summary>
    public partial class AppMsgBox : Window
    {
        private Timer _timer;
        private int _closeInterval;   // in msec
        public int CloseInterval
        {
            get { return _closeInterval; }
            set
            {
                if (_closeInterval == value) return;
                _closeInterval = value;
                if (_timer == null)
                {
                    _timer = new Timer(_closeInterval);
                    _timer.Elapsed += _timer_Elapsed;
                }
                _timer.Interval = _closeInterval;
                _timer.Enabled = true;
            }
        }

        private void _timer_Elapsed(object sender, ElapsedEventArgs e)
        {

            this.Close();
        }

        public AppMsgBox(string messageText)
        {
            InitializeComponent();

            txtMessage.Text = messageText;
        }

        private void Window_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape) closeWin(e);
            e.Handled = true;
        }

        private void btnClose_MouseDown(object sender, MouseButtonEventArgs e)
        {
            closeWin(e);
        }

        private void btnOk_MouseDown(object sender, MouseButtonEventArgs e)
        {
            closeWin(e);
        }

        private void closeWin(RoutedEventArgs e)
        {
            e.Handled = true;
            this.Close();
        }

    }  // class
}
