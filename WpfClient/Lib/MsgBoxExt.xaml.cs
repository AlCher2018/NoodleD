using System;
using System.Diagnostics;
using System.Threading;
using System.Timers;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Effects;
using System.Windows.Threading;

namespace WpfClient.Lib
{
    /// <summary>
    /// Interaction logic for MsgBoxExt.xaml
    /// </summary>
    public partial class MsgBoxExt : Window
    {
        private MessageBoxResult _retValue = MessageBoxResult.None;

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


        public string MessageText
        {
            get { return mbMessageText.Text; }
            set { mbMessageText.Text = value; }
        }

        private bool _closeByButtonPress = true;
        public bool CloseByButtonPress { get { return _closeByButtonPress; } set { _closeByButtonPress = value; } }

        #endregion

        #region timer
        private System.Timers.Timer _timer;
        private double _autoCloseInterval;   // in msec
        public double AutoCloseInterval
        {
            get { return _autoCloseInterval; }
            set
            {
                if (_autoCloseInterval == value) return;

                _autoCloseInterval = value;
                if (_autoCloseInterval == 0)
                {
                    if (_timer != null)
                    {
                        _timer.Elapsed -= _timer_Elapsed;
                        _timer.Close(); _timer = null;
                    }
                }
                else
                {
                    if (_timer == null)
                    {
                        _timer = new System.Timers.Timer(_autoCloseInterval);
                        _timer.Elapsed += _timer_Elapsed;
                    }
                    _timer.Interval = _autoCloseInterval;
                    _timer.Enabled = true;
                }
            }
        }

        private void _timer_Elapsed(object sender, ElapsedEventArgs e)
        {
            this.Dispatcher.Invoke(() =>
            {
                this.closeWin();
            });
        }

        private System.Timers.Timer _pressTimer;
        private FrameworkElement _buttonPressed;

        #endregion

        public MsgBoxExt()
        {
            InitializeComponent();

            this._closeByButtonPress = true;
            this.TitleFontSize = 12d;
            this.MessageFontSize = 20d;
            this.ButtonFontSize = 20d;
            _buttonsTextArr = _buttonsText.Split(';');
            this.ButtonBackground = System.Windows.SystemColors.ControlDarkBrush;

            _pressTimer = new System.Timers.Timer(500);
            _pressTimer.Elapsed += _pressTimer_Elapsed;
        }

        private void _pressTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            Dispatcher.Invoke(() => doUnpress(), DispatcherPriority.Input);
        }

        private void mbWindow_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            double w = this.ActualWidth;
            double h = this.ActualHeight;
            
            mainGrid.Width = ((w > h)?0.4:0.7) * w;

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
            mbTitleText.FontStyle = FontStyles.Italic;
            mbTitleText.Margin = new Thickness(TitleFontSize, 0.3 * TitleFontSize, TitleFontSize, 0.3 * TitleFontSize);
            //   message
            mbMessageText.FontSize = MessageFontSize;
            mbMessageText.Margin = new Thickness(2 * MessageFontSize, MessageFontSize, 2 * MessageFontSize, 0);
            //   button text
            btnPanel.Margin = new Thickness(2 * ButtonFontSize);
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

        private void btn_TouchDown(object sender, System.Windows.Input.TouchEventArgs e)
        {
            doPress((FrameworkElement)sender);
        }

        private void btn_MouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            //if (e.StylusDevice != null) return;
            doPress((FrameworkElement)sender);

            //closeWin(e);
        }

        private void btn_TouchLeave(object sender, System.Windows.Input.TouchEventArgs e)
        {
            doUnpress();
        }
        private void btn_MouseLeave(object sender, System.Windows.Input.MouseEventArgs e)
        {
            //if (e.StylusDevice != null) return;
            doUnpress();
        }

        private void btn_TouchUp(object sender, System.Windows.Input.TouchEventArgs e)
        {
            if ((sender as FrameworkElement).Equals(_buttonPressed))
            {
                closeWin(e);
            }
        }
        private void btn_MouseUp(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            //if (e.StylusDevice != null) return;

            if ((sender as FrameworkElement).Equals(_buttonPressed))
            {
                closeWin(e);
            }
        }

        private void doPress(FrameworkElement btn)
        {
            // сохранить нажатую кнопку
            _buttonPressed = btn;

            TranslateTransform tt = (_buttonPressed.RenderTransform as TranslateTransform);
            DropShadowEffect de = (_buttonPressed.Effect as DropShadowEffect);

            tt.X = 3; tt.Y = 3;
            de.ShadowDepth = 0; de.BlurRadius = 0;

            _pressTimer.Enabled = true;
        }

        private void doUnpress()
        {
            if (_buttonPressed == null) return;

            TranslateTransform tt = (_buttonPressed.RenderTransform as TranslateTransform);
            tt.X = 0; tt.Y = 0;
            DropShadowEffect de = (_buttonPressed.Effect as DropShadowEffect);
            de.ShadowDepth = 6; de.BlurRadius = 10;

            _buttonPressed = null;
            if (_pressTimer.Enabled) _pressTimer.Enabled = false;
        }

        private void MsgBoxExt_PreviewKeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == System.Windows.Input.Key.Escape) closeWin(e);
        }

        private void closeWin(RoutedEventArgs e = null)
        {
            if ((e != null) && (e is RoutedEventArgs)) e.Handled = true;

            // установить возвращаемое значение
            if (_buttonPressed == null)
            {
                _retValue = MessageBoxResult.None;
            }
            else
            {
                MessageBoxResult res;
                if (Enum.TryParse<MessageBoxResult>(_buttonPressed.Tag.ToString(), out res) == true) _retValue = res;
            }
            doUnpress();

            //if (this._closeByButtonPress)
            //{
            //    this.Close();
            //}
            //else
            //{
                this.Hide();
            //}
        }

    }  // class

}
