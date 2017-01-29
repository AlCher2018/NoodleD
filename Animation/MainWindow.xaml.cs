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
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Animation
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

        private void btnTranslate_Click(object sender, RoutedEventArgs e)
        {
            Storyboard sb = (Storyboard)Application.Current.MainWindow.Resources["sbTranslate"];
            Canvas.SetLeft(imgMove, -imgMove.ActualWidth / 2d);
            Canvas.SetTop(imgMove, -imgMove.ActualHeight / 2d);
            sb.Begin();
        }
        private void sbTranslate_Completed(object sender, EventArgs e)
        {
            Canvas.SetLeft(imgMove, 0);
            Canvas.SetTop(imgMove, 0);
        }


        private void btnRotate_Click(object sender, RoutedEventArgs e)
        {
            Storyboard sb = (Storyboard)Application.Current.MainWindow.Resources["sbRotate"];

            Canvas.SetLeft(canvasRotate, -imgRotate.ActualWidth / 2d);
            Canvas.SetTop(canvasRotate, -imgRotate.ActualHeight / 2d);

            sb.Begin();
        }

        private void sbRotate_Completed(object sender, EventArgs e)
        {
            Canvas.SetLeft(canvasRotate, 0);
            Canvas.SetTop(canvasRotate, 0);
        }

        private void btnScale_Click(object sender, RoutedEventArgs e)
        {
            Storyboard sb = (Storyboard)Application.Current.MainWindow.Resources["sbScale"];
            Canvas.SetLeft(canvasScale, -imgScale.ActualWidth / 2d);
            Canvas.SetTop(canvasScale, -imgScale.ActualHeight / 2d);
            sb.Begin();
        }
        private void sbScale_Completed(object sender, EventArgs e)
        {
            Canvas.SetLeft(canvasScale, 0);
            Canvas.SetTop(canvasScale, 0);
        }

        private void btnAnim_Click(object sender, RoutedEventArgs e)
        {
            Storyboard sb = (Storyboard)Application.Current.MainWindow.Resources["sbAnim"];
            Canvas.SetLeft(canvasAnim, -imgAnim.ActualWidth / 2d);
            Canvas.SetTop(canvasAnim, -imgAnim.ActualHeight / 2d);
            sb.Begin();
        }
        private void sbAnim_Completed(object sender, EventArgs e)
        {
            Canvas.SetLeft(canvasAnim, 0);
            Canvas.SetTop(canvasAnim, 0);
        }

    }
}
