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

namespace MsgBoxTester
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            rbtOk.IsChecked = true;
            txtTitle.Text = "My any-any-any Title";
            txtTitleFontSize.Text = "20";
            txtMessage.Text = "Вы еще здесь? sdfsdhfkhewr wkerhwrhje wekrhwerjh wekrjhewr kjhwer werkhj  werwekrhjwe  kwjerhewr khwejrhwer  wkerhwe rkh ewrkhewrkj kjwehr werkjhwer";
            txtMsgFontSize.Text = "30";
            txtBtnFontSize.Text = "20";
        }

        private void btnShowMsgBox_Click(object sender, RoutedEventArgs e)
        {
            LinearGradientBrush lb = new LinearGradientBrush() { StartPoint = new Point(0.5, 0), EndPoint = new Point(0.5, 1) };
            lb.GradientStops.Add(new GradientStop(Colors.Black, 0));
            lb.GradientStops.Add(new GradientStop(Colors.White, 0.5));
            lb.GradientStops.Add(new GradientStop(Colors.Black, 1));

            MsgBoxExt mbe = new MsgBoxExt()
            {
                Title = txtTitle.Text,
                TitleFontSize = int.Parse(txtTitleFontSize.Text),
                MessageText = txtMessage.Text,
                MessageFontSize = int.Parse(txtMsgFontSize.Text),
                ButtonFontSize = int.Parse(txtBtnFontSize.Text),

                MsgBoxButton = getButtons(),

                ButtonBackground = lb,
                ButtonForeground = Brushes.Black,
                ButtonBackgroundOver = lb,
                ButtonForegroundOver = Brushes.Red
            };

            MessageBoxResult result = mbe.Show();
            mbe = null;

            //MessageBox.Show("Result - " + result.ToString());
        }

        private MessageBoxButton getButtons()
        {
            MessageBoxButton retVal = MessageBoxButton.OK;
            List<RadioButton> radioButtons = (groupBox as Panel).Children.OfType<RadioButton>().ToList();
            RadioButton rbTarget = radioButtons
                  .Where(r => r.GroupName == "b1" && (r.IsChecked ?? false)).FirstOrDefault();
            if (rbTarget == null) return retVal;

            switch (rbTarget.Content.ToString())
            {
                case "Ok":
                    retVal = MessageBoxButton.OK;
                    break;
                case "OkCancel":
                    retVal = MessageBoxButton.OKCancel;
                    break;
                case "YesNoCancel":
                    retVal = MessageBoxButton.YesNoCancel;
                    break;
                case "YesNo":
                    retVal = MessageBoxButton.YesNo;
                    break;
                default:
                    break;
            }
            return retVal;
        }

        private void OnClick(object sender, RoutedEventArgs e)
        {
            myCheckBox.IsChecked = true;
        }

    }  // class
}
