using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;

namespace MsgBoxTester
{
    /// <summary>
    /// Interaction logic for MsgBoxExt.xaml
    /// </summary>
    public partial class MsgBoxExt : Window
    {
        MessageBoxResult _retValue = MessageBoxResult.None;
        Storyboard _sbPress, _sbUnpress;

        #region properties

        private MessageBoxButton _msgBoxButton = MessageBoxButton.OK;

        public MessageBoxButton MsgBoxButton
        {
            get { return _msgBoxButton; }
            set { _msgBoxButton = value; }
        }


        // string format: Ok;Cancel;Yes;No
        private string _buttonsText = "Ок;Отмена;Да;Нет";
        private string[] _buttonsTextArr;
        public string ButtonsText
        {
            get { return _buttonsText; }
            set {
                if (_buttonsText == value) return;
                _buttonsText = value;
                _buttonsTextArr = _buttonsText.Split(';');
            }
        }

        public double TitleFontSize { get; set; } 
        public double MessageFontSize { get; set; }
        public double ButtonFontSize { get; set; }



        public Brush ButtonBackground
        {
            get { return (Brush)GetValue(ButtonBackgroundProperty); }
            set { SetValue(ButtonBackgroundProperty, value); }
        }

        // Using a DependencyProperty as the backing store for ButtonBackground.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ButtonBackgroundProperty =
            DependencyProperty.Register("ButtonBackground", typeof(Brush), typeof(MsgBoxExt));



        public Brush ButtonBackgroundOver
        {
            get { return (Brush)GetValue(ButtonBackgroundOverProperty); }
            set { SetValue(ButtonBackgroundOverProperty, value); }
        }
        public static readonly DependencyProperty ButtonBackgroundOverProperty =
            DependencyProperty.Register("ButtonBackgroundOver", typeof(Brush), typeof(MsgBoxExt), new PropertyMetadata(Brushes.Gray));


        public Brush ButtonForeground
        {
            get { return (Brush)GetValue(ButtonForegroundProperty); }
            set { SetValue(ButtonForegroundProperty, value); }
        }
        public static readonly DependencyProperty ButtonForegroundProperty =
            DependencyProperty.Register("ButtonForeground", typeof(Brush), typeof(MsgBoxExt), new PropertyMetadata(Brushes.Black));

        public Brush ButtonForegroundOver
        {
            get { return (Brush)GetValue(ButtonForegroundOverProperty); }
            set { SetValue(ButtonForegroundOverProperty, value); }
        }
        public static readonly DependencyProperty ButtonForegroundOverProperty =
            DependencyProperty.Register("ButtonForegroundOver", typeof(Brush), typeof(MsgBoxExt), new PropertyMetadata(Brushes.Yellow));


        public string MessageText {
            get { return mbMessageText.Text; }
            set { mbMessageText.Text = value; }
        }
        #endregion

        public MsgBoxExt()
        {
            InitializeComponent();
            
            this.TitleFontSize = 12d;
            this.MessageFontSize = 20d;
            this.ButtonFontSize = 20d;
            _buttonsTextArr = _buttonsText.Split(';');
            this.ButtonBackground = System.Windows.SystemColors.ControlDarkBrush;

            _sbPress = (Storyboard)this.Resources["sbPress"];
            _sbUnpress = (Storyboard)this.Resources["sbUnpress"];
        }


        private static object CorrectDoubleValue(DependencyObject d, object baseValue)
        {
            double currentValue = Convert.ToDouble(baseValue);
            if (currentValue < 0) currentValue = 12d;

            return currentValue;
        }

        private void mbWindow_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            double w = this.ActualWidth;
            double h = this.ActualHeight;
            
            mainGrid.Width = 0.4 * w;
            CornerRadius cr = new CornerRadius(0.008 * Math.Min(w, h));
            btn1.CornerRadius = cr; btn2.CornerRadius = cr; btn3.CornerRadius = cr;
            //mainGrid.Height = 0.3 * h;
        }


        public new MessageBoxResult Show()
        {
            return ShowDialog();
        }
        public new MessageBoxResult ShowDialog()
        {
            //   title
            mbTitleText.Text = base.Title;
            mbTitleText.FontSize = TitleFontSize;
            mbTitleText.Margin = new Thickness(TitleFontSize, TitleFontSize / 2, TitleFontSize, TitleFontSize / 2);
            //   message
            mbMessageText.FontSize = MessageFontSize;
            mbMessageText.Margin = new Thickness(2 * MessageFontSize);
            //   button text
            btnPanel.Margin = new Thickness(0, 0, 3 * ButtonFontSize, 2 * ButtonFontSize);
            btn1Text.FontSize = ButtonFontSize; btn2Text.FontSize = ButtonFontSize; btn3Text.FontSize = ButtonFontSize;

            Thickness borderMargin = new Thickness(ButtonFontSize, 0, ButtonFontSize, 0);
            btn1.Margin = borderMargin; btn2.Margin = borderMargin; btn3.Margin = borderMargin;
            Thickness btnTextMargin = new Thickness(2*ButtonFontSize, 0.5* ButtonFontSize, 2* ButtonFontSize, 0.7*ButtonFontSize);
            btn1Text.Margin = btnTextMargin; btn2Text.Margin = btnTextMargin; btn3Text.Margin = btnTextMargin;
            
            setButtonVisibility();

            base.ShowDialog();

            return _retValue;
        }

        private void setButtonVisibility()
        {
            switch (_msgBoxButton)
            {
                case MessageBoxButton.OK:
                    btn1.Visibility = Visibility.Visible;
                    btn2.Visibility = Visibility.Collapsed;
                    btn3.Visibility = Visibility.Collapsed;

                    btn1Text.Text = _buttonsTextArr[0];
                    btn1.Tag = MessageBoxResult.OK;
                    break;

                case MessageBoxButton.OKCancel:
                    btn1.Visibility = Visibility.Visible;
                    btn2.Visibility = Visibility.Visible;
                    btn3.Visibility = Visibility.Collapsed;

                    btn1Text.Text = _buttonsTextArr[0];
                    btn1.Tag = MessageBoxResult.OK;
                    btn2Text.Text = _buttonsTextArr[1];
                    btn2.Tag = MessageBoxResult.Cancel;
                    break;

                case MessageBoxButton.YesNoCancel:
                    btn1.Visibility = Visibility.Visible;
                    btn2.Visibility = Visibility.Visible;
                    btn3.Visibility = Visibility.Visible;

                    btn1Text.Text = _buttonsTextArr[2];
                    btn1.Tag = MessageBoxResult.Yes;
                    btn2Text.Text = _buttonsTextArr[3];
                    btn2.Tag = MessageBoxResult.No;
                    btn3Text.Text = _buttonsTextArr[1];
                    btn3.Tag = MessageBoxResult.Cancel;
                    break;

                case MessageBoxButton.YesNo:
                    btn1.Visibility = Visibility.Visible;
                    btn2.Visibility = Visibility.Visible;
                    btn3.Visibility = Visibility.Collapsed;

                    btn1Text.Text = _buttonsTextArr[2];
                    btn1.Tag = MessageBoxResult.Yes;
                    btn2Text.Text = _buttonsTextArr[3];
                    btn2.Tag = MessageBoxResult.No;
                    break;

                default:
                    break;
            }
        }

        private void btn_MouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            //(sender as Border).BeginStoryboard(_sbPress);

            //FrameworkElement fe = (sender as FrameworkElement);
            //TranslateTransform tt = (fe.RenderTransform as TranslateTransform);
            //tt.X = 7; tt.Y = 7;
            //e.Handled = true;

            FrameworkElement fe = (sender as FrameworkElement);
            TranslateTransform tt = new TranslateTransform(7,7);
            fe.RenderTransform = tt;

            System.Diagnostics.Debug.Print("mouse DOWN");
            MessageBoxResult res;
            if (Enum.TryParse<MessageBoxResult>((sender as FrameworkElement).Tag.ToString(), out res) == true)
                _retValue = res;
        }

        private void btn_MouseLeave(object sender, System.Windows.Input.MouseEventArgs e)
        {
            FrameworkElement fe = (sender as FrameworkElement);
            TranslateTransform tt = (fe.RenderTransform as TranslateTransform);
            tt.X = 0; tt.Y = 0;
            //(sender as Border).BeginStoryboard(_sbUnpress);
            e.Handled = true;
            System.Diagnostics.Debug.Print("mouse LEAVE");

            _retValue = MessageBoxResult.None;
        }

        private void btn_MouseUp(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            FrameworkElement fe = (sender as FrameworkElement);
            TranslateTransform tt = (fe.RenderTransform as TranslateTransform);
            tt.X = 0; tt.Y = 0;
            e.Handled = true;
            System.Diagnostics.Debug.Print("mouse UP");

            //(sender as Border).BeginStoryboard(_sbUnpress);
        }

        private void Storyboard_Completed(object sender, EventArgs e)
        {
            //if (_retValue != MessageBoxResult.None)
            //{
            //    this.Close();
            //}
        }

    }  // class
}
