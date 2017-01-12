using System;
using System.Collections.Generic;
using System.IO;
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

namespace WpfApplication1
{
    /// <summary>
    /// Interaction logic for Page1.xaml
    /// </summary>
    public partial class Page1 : Page
    {
        public Page1()
        {
            InitializeComponent();

            Zen.Barcode.CodeEan13BarcodeDraw bc = Zen.Barcode.BarcodeDrawFactory.CodeEan13WithChecksum;
            System.Drawing.Image imageBC = bc.Draw("012701110123", 50);

            MemoryStream ms = new MemoryStream();
            imageBC.Save(ms, System.Drawing.Imaging.ImageFormat.Png);
            ms.Position = 0;
            BitmapImage bi = new BitmapImage();
            bi.BeginInit();
            bi.StreamSource = ms;
            bi.EndInit();

            img.Source = bi;
            //bmp.Dispose(); //if bmp is not used further. Thanks @Peter
        }
    }
}
