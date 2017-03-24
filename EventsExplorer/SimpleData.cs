using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace EventsExplorer
{
    public class SimpleData : DependencyObject
    {
        // Using a DependencyProperty as the backing store for Id.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty IdProperty =
            DependencyProperty.Register("Id", typeof(int), typeof(SimpleData), new PropertyMetadata(0));

        public int Id
        {
            get { return (int)GetValue(IdProperty); }
            set { SetValue(IdProperty, value); }
        }



        public string Name
        {
            get { return (string)GetValue(NameProperty); }
            set { SetValue(NameProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Name.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty NameProperty =
            DependencyProperty.Register("Name", typeof(string), typeof(SimpleData), new PropertyMetadata(""));




        public List<string> SubList
        {
            get { return (List<string>)GetValue(SubListProperty); }
            set { SetValue(SubListProperty, value); }
        }

        // Using a DependencyProperty as the backing store for SubList.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty SubListProperty =
            DependencyProperty.Register("SubList", typeof(List<string>), typeof(SimpleData));

        public SimpleData()
        {
            SubList = new List<string>();
        }

    }
}
