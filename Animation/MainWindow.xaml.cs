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
            runAnim("sbTranslate");
        }

        private void btnRotate_Click(object sender, RoutedEventArgs e)
        {
            runAnim("sbRotate");
        }


        private void btnScale_Click(object sender, RoutedEventArgs e)
        {
            runAnim("sbScale");
        }

        private void btnAnim_Click(object sender, RoutedEventArgs e)
        {
            runAnim("sbAnim");
        }

        private void runAnim(string nameAnim)
        {
            Storyboard sb = (Storyboard)Application.Current.MainWindow.Resources[nameAnim];
            Canvas.SetLeft(canvasAnim, -imgAnim.ActualWidth / 2d);
            Canvas.SetTop(canvasAnim, -imgAnim.ActualHeight / 2d);
            sb.Begin();
        }

        private void sbAnim_Completed(object sender, EventArgs e)
        {
            Canvas.SetLeft(canvasAnim, 0);
            Canvas.SetTop(canvasAnim, 0);
        }

    }  // class
}
