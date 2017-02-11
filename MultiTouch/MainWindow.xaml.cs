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

namespace MultiTouch
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void langBorder_TouchDown(object sender, TouchEventArgs e)
        {
            string ctrlName = (sender as FrameworkElement).Name;
            txtBox.AppendText(string.Format("TouchDown: {0}\n", ctrlName));
            txtBox.CaretIndex = txtBox.Text.Length;
        }

        private void langBorder_MouseDown(object sender, MouseButtonEventArgs e)
        {
            string ctrlName = (sender as FrameworkElement).Name;

            Point p = e.GetPosition(mainWin);

            txtBox.AppendText(string.Format("MouseDown: {0}, point {1}\n", ctrlName, p.ToString()));
            txtBox.CaretIndex = txtBox.Text.Length;
            //MessageBox.Show(string.Format("You pressed: {0}", ctrlName));
        }
    }
}
