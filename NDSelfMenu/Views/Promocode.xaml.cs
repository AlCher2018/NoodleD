using AppActionNS;
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
using UserActionLog;

namespace WpfClient.Views
{
    /// <summary>
    /// Interaction logic for Promocode.xaml
    /// </summary>
    public partial class Promocode : Window
    {
        private string _preValue;
        private UserActionsLog _eventsLog;

        public Promocode()
        {
            InitializeComponent();
            this.Activated += Promocode_Activated;

            if (AppLib.GetAppSetting("IsWriteWindowEvents").ToBool())
            {
                _eventsLog = new UserActionsLog(new FrameworkElement[] { this, brdFooterCancel, brdFooterOk }, EventsMouseEnum.Bubble, EventsKeyboardEnum.None, EventsTouchEnum.Bubble, UserActionLog.LogFilesPathLocationEnum.App_Logs, true, false);
            }

            setLayout();
        }

        private void Promocode_Activated(object sender, EventArgs e)
        {
            BindingExpression be;
            // установка текстов на выбранном языке
            //    заголовок
            be = txtTitle.GetBindingExpression(TextBlock.TextProperty);
            if (be != null) be.UpdateTarget();
        }

        public new void ShowDialog()
        {
            //string stack = Environment.StackTrace;
            // вызывающее окно
            System.Diagnostics.StackFrame aFrame = (new System.Diagnostics.StackTrace()).GetFrame(1);
            string callingWinName = aFrame.GetMethod().DeclaringType.Name;

            AppLib.WriteLogTraceMessage("Открывается окно ввода промокода");
            AppLib.WriteAppAction(this.Name, AppActionsEnum.PromocodeWinOpen, callingWinName);

            this.ReOpen();
        }


        #region активация ожидашки
        protected override void OnActivated(EventArgs e)
        {
            App.IdleTimerStart(this);
            base.OnActivated(e);
        }
        protected override void OnDeactivated(EventArgs e)
        {
            App.IdleTimerStop();
            base.OnDeactivated(e);
        }
        #endregion

        public void ReOpen()
        {
            txtInput.Text = App.PromocodeNumber;
            _preValue = txtInput.Text;

            base.ShowDialog();
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
            if (e.Key == Key.Escape) closeWin(false);
        }

        private void brdFooterCancel_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            //if (e.StylusDevice != null) return;
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
            //if (e.StylusDevice != null) return;
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
                App.PromocodeNumber = txtInput.Text;
                AppLib.WriteAppAction(this.Name, AppActionsEnum.PromocodeInputValue, txtInput.Text);
            }
            else
            {
                txtInput.Text = App.PromocodeNumber;
            }

            AppLib.WriteLogTraceMessage("Закрывается окно ввода промокода");
            AppLib.WriteAppAction(this.Name, AppActionsEnum.PromocodeWinClose, (isSetRetValue ? "Ok" : "Cancel"));

            this.Hide();
        }

    }  // class
}
