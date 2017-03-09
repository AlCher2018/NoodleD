using System;
using System.Collections.Generic;
using System.Globalization;
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

namespace WpfClient.Lib
{
    /// <summary>
    /// Interaction logic for PromoPanel.xaml
    /// </summary>
    public partial class PromoPanel : UserControl
    {



        public string InviteText
        {
            get { return (string)GetValue(InviteTextProperty); }
            set {
                SetValue(InviteTextProperty, value);
                setTextStyle(true);
            }
        }

        // Using a DependencyProperty as the backing store for InviteText.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty InviteTextProperty =
            DependencyProperty.Register("InviteText", typeof(string), typeof(PromoPanel), new PropertyMetadata(0));



        public string PromoCode
        {
            get { return (string)GetValue(PromoCodeProperty); }
            set {
                SetValue(PromoCodeProperty, value);
                setTextStyle(false);
            }
        }

        // Using a DependencyProperty as the backing store for PromoCode.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty PromoCodeProperty =
            DependencyProperty.Register("PromoCode", typeof(string), typeof(PromoPanel), new PropertyMetadata(0));



        public PromoPanel()
        {
            InitializeComponent();

            LayoutRoot.DataContext = this;
        }

        private void setTextStyle(bool isInvite)
        {
            if (isInvite)
            {
                txtPromoCode.Style = (Style)this.Resources["inviteTextStyle"];
            }
            else 
            {
                txtPromoCode.Style = (Style)this.Resources["codeTextStyle"];
            }
        }  // method

    }  // class
}
