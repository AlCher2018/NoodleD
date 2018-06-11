using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Animation;
using System.Windows.Media.Effects;

namespace WpfClient.Views
{
    public class TextAnimation
    {
        //private Storyboard _storyBoard;
        private DoubleAnimation _daSize, _daBlur;

        private int _durationFontSize = 100, _durationTextBlur = 100;
        public int DurationFontSize
        {
            get { return _durationFontSize; }
            set
            {
                if (_durationFontSize != value)
                {
                    _durationFontSize = value;
                    _daSize.Duration = TimeSpan.FromMilliseconds(_durationFontSize);
                }
            }
        }
        public int DurationTextBlur
        {
            get { return _durationTextBlur; }
            set
            {
                if (_durationTextBlur != value)
                {
                    _durationTextBlur = value;
                    _daBlur.Duration = TimeSpan.FromMilliseconds(_durationTextBlur);
                }
            }
        }

        public bool IsAnimFontSize { get; set; }
        public double FontSizeKoef { get; set; }
        public bool IsAnimTextBlur { get; set; }
        public int TextBlurTo { get; set; }

        private int _repeatBehaviorFontSize = 1, _repeatBehaviorTextBlur = 1;
        public int RepeatBehaviorFontSize { get { return _repeatBehaviorFontSize; }
            set
            {
                if (_repeatBehaviorFontSize != value)
                {
                    _repeatBehaviorFontSize = value;
                    _daSize.RepeatBehavior = new RepeatBehavior(_repeatBehaviorFontSize);
                }
            }
        }
        public int RepeatBehaviorTextBlur
        {
            get { return _repeatBehaviorTextBlur; }
            set
            {
                if (_repeatBehaviorTextBlur != value)
                {
                    _repeatBehaviorTextBlur = value;
                    _daBlur.RepeatBehavior = new RepeatBehavior(_repeatBehaviorTextBlur);
                }
            }
        }

        public event EventHandler Completed;

        public TextAnimation()
        {
            IsAnimFontSize = true; IsAnimTextBlur = false;
            FontSizeKoef = 1.8; TextBlurTo = 10;

            _daSize = new DoubleAnimation() { AutoReverse = true };
            _daSize.FillBehavior = FillBehavior.Stop;
            _daSize.Duration = TimeSpan.FromMilliseconds(_durationFontSize);
            _daSize.Completed += _daSize_Completed;

            _daBlur = new DoubleAnimation() { AutoReverse = true };
            _daBlur.FillBehavior = FillBehavior.Stop;
            _daBlur.Duration = TimeSpan.FromMilliseconds(_durationTextBlur);
            _daBlur.Completed += _daBlur_Completed;
        }

        private void _daBlur_Completed(object sender, EventArgs e)
        {
            if (Completed != null) Completed("AnimTextBlur", null);
        }

        private void _daSize_Completed(object sender, EventArgs e)
        {
            if (Completed != null) Completed("AnimFontSize", null);
        }

        public void BeginAnimation(TextBlock textBlock, double initFontSize = 0)
        {
            if (IsAnimFontSize)
            {
                _daSize.From = (initFontSize == 0) ? textBlock.FontSize : initFontSize;
                _daSize.To = FontSizeKoef * _daSize.From;
                textBlock.BeginAnimation(TextBlock.FontSizeProperty, _daSize);
            }

            if (IsAnimTextBlur)
            {
                if (textBlock.Effect == null) textBlock.Effect = new BlurEffect() {Radius = 0 };
                _daBlur.From = 0;
                _daBlur.To = TextBlurTo;
                textBlock.Effect.BeginAnimation(BlurEffect.RadiusProperty, _daBlur);
            }
        }

    } // class
}
