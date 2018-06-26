using IntegraLib;
using IntegraWPFLib;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;


namespace WpfClient.Views
{
    /// <summary>
    /// Interaction logic for SplashScreen.xaml
    /// </summary>
    public partial class SplashScreen : Window
    {
        public SplashScreen()
        {
            InitializeComponent();

            this.lblMessage.SetBinding(Label.ContentProperty,
                new Binding()
                {
                    Source = SplashScreenLib.MessageListener.Instance,
                    Path = new PropertyPath("Message")
                });

            // background image
            string fileFullName = getSplashBackImageFile();
            if (fileFullName != null)
            {
                splashBackImage.Source = ImageHelper.GetBitmapImage(fileFullName);
                IntegraWPFLib.DispatcherHelper.DoEvents();
            }
        }

        private string getSplashBackImageFile()
        {
            string hor = CfgFileHelper.GetAppSetting("BackgroundImageHorizontal");
            string ver = CfgFileHelper.GetAppSetting("BackgroundImageVertical");

            string fileName = (WpfHelper.IsAppVerticalLayout ? ver : hor);
            if (fileName == null) return null;

            string cfgValue = CfgFileHelper.GetAppSetting("ImagesPath");
            fileName = AppEnvironment.GetFullFileName(cfgValue, fileName);
            if (System.IO.File.Exists(fileName) == false) return null;

            return fileName;
        }

    }  // class
}
