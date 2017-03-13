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
using System.Windows.Media.Effects;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace WpfClient
{
    /// <summary>
    /// Interaction logic for Promocode.xaml
    /// </summary>
    public partial class Promocode : Window
    {
        public string InputValue;

        private string _preValue;

        public Promocode(string editCode = null)
        {
            InitializeComponent();

            setLayout();

            if (string.IsNullOrEmpty(editCode) == false) txtInput.Text = editCode;
            _preValue = editCode;
        }

        private void setLayout()
        {
            // размеры
            double scrWidth = (double)AppLib.GetAppGlobalValue("screenWidth");
            double scrHeight = (double)AppLib.GetAppGlobalValue("screenHeight");

            // vertical
            if (AppLib.IsAppVerticalLayout)
            {
                this.Width = scrWidth; this.Height = scrHeight;
                panelMain.Height = 0.55 * scrHeight;
                panelMain.Width = 0.7 * panelMain.Height;
            }
            // horizontal
            else
            {
                panelMain.Height = 0.7 * scrHeight;
                panelMain.Width = 0.7 * panelMain.Height;
            }

            // радиусы закругления
            if (panelMain is Border)
            {
                double radius = 0.07 * panelMain.Width;
                double radius1 = 0.9 * radius;
                (panelMain as Border).CornerRadius = new CornerRadius(radius);
                (brdTitle as Border).CornerRadius = new CornerRadius(radius1, radius1, 0, 0);
                (brdFooterCancel as Border).CornerRadius = new CornerRadius(0, 0, 0, radius1);
                (brdFooterOk as Border).CornerRadius = new CornerRadius(0, 0, radius1, 0);
            }

            double d1 = 0.5 * (double)AppLib.GetAppGlobalValue("appFontSize1");
            txtInput.Margin = new Thickness(d1, 0, d1, 0);
        }

        private void digBtn_MouseEnter(object sender, System.Windows.Input.MouseEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed) doPress((FrameworkElement)sender);
        }

        private void digBtn_MouseLeave(object sender, System.Windows.Input.MouseEventArgs e)
        {
            doUnpress((FrameworkElement)sender);
        }

        private void digBtn_MouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            doPress((FrameworkElement)sender);
        }

        private void digBtn_MouseUp(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            FrameworkElement fe = (FrameworkElement)sender;
            doUnpress(fe);

            TextBlock tb = ((fe as Border).Child as TextBlock);
            if (fe.Name == "brdBackspace")
            {
                if (txtInput.Text.Length > 0) txtInput.Text = txtInput.Text.Remove(txtInput.Text.Length - 1);
            }
            else
            {
                if (txtInput.Text.Length < 18) txtInput.Text += tb.Text;
            }
        }

        private void doPress(FrameworkElement fe)
        {
            TranslateTransform tt = (fe.RenderTransform as TranslateTransform);
            tt.X = 3; tt.Y = 3;

            DropShadowEffect de = (fe.Effect as DropShadowEffect);
            de.BlurRadius = 0; de.ShadowDepth = 0;
        }

        private void doUnpress(FrameworkElement fe)
        {
            TranslateTransform tt = (fe.RenderTransform as TranslateTransform);
            tt.X = 0; tt.Y = 0;
            
            DropShadowEffect de = (fe.Effect as DropShadowEffect);
            de.BlurRadius = 10; de.ShadowDepth = 6;
        }


        private void Window_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            anyAction("Window_PreviewKeyDown");

            if (e.Key == Key.Escape) closeWin(false);
        }

        private void brdFooterCancel_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.StylusDevice != null) return;
            e.Handled = true;
            closeWin(false);
        }
        private void brdFooterCancel_PreviewTouchDown(object sender, TouchEventArgs e)
        {
            e.Handled = true;
            closeWin(false);
        }


        private void brdFooterOk_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.StylusDevice != null) return;
            e.Handled = true;
            closeWin(true);
        }
        private void brdFooterOk_PreviewTouchDown(object sender, TouchEventArgs e)
        {
            e.Handled = true;
            closeWin(true);
        }



        private void closeWin(bool isSetRetValue)
        {
            if (isSetRetValue)
            {
                this.InputValue = txtInput.Text;
            }
            else
            {
                this.InputValue = null;
            }
            this.Close();
        }

        private void Window_PreviewKeyUp(object sender, KeyEventArgs e)
        {
            anyAction("Window_PreviewKeyUp");
        }

        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            anyAction("Window_KeyDown");
        }

        private void Window_KeyUp(object sender, KeyEventArgs e)
        {
            anyAction("Window_KeyUp");
        }

        private void Window_PreviewTouchDown(object sender, TouchEventArgs e)
        {
            anyAction("Window_PreviewTouchDown");
        }

        private void Window_PreviewTouchUp(object sender, TouchEventArgs e)
        {
            anyAction("Window_PreviewTouchUp");
        }

        private void Window_TouchDown(object sender, TouchEventArgs e)
        {
            anyAction("Window_TouchDown");
        }

        private void Window_TouchUp(object sender, TouchEventArgs e)
        {
            anyAction("Window_TouchUp");
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            anyAction("Window_Loaded");
        }

        private void Window_Activated(object sender, EventArgs e)
        {
            anyAction("Window_Activated");
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            anyAction("Window_Closing");
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            anyAction("Window_Closed");
        }

        private void anyAction(string msg)
        {
            AppLib.WriteLogTraceMessage(msg + "\n\t"+ Environment.StackTrace);
        }
    }  // class
}
