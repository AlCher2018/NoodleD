using System;
using System.Collections.Generic;
using System.Diagnostics;
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
using System.Windows.Shapes;

namespace WpfClient
{
    /// <summary>
    /// Interaction logic for MessageBoxDialog.xaml
    /// </summary>
    public partial class MessageBoxDialog : Window
    {
        public new string Title
        {
            get { return txtTitle.Text; }
            set { txtTitle.Text = value; }
        }

        public string MessageText
        {
            get { return this.txtMessage.Text; }
            set { this.txtMessage.Text = value; }
        }

        public MessageBoxDialog()
        {
            InitializeComponent();
        }

        private void btnOk_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = true;
        }

        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
        }

        public new bool ShowDialog()
        {
            if (string.IsNullOrEmpty(txtTitle.Text) == true) brdTitle.Visibility = Visibility.Collapsed;
            bool? res = base.ShowDialog();
            return res ?? false;
        }

        private void Window_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape) this.DialogResult = false;
            else if (e.Key == Key.Return) this.DialogResult = true;
            e.Handled = true;

        }
    }
}
