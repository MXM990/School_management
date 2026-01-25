using School_Management.Control;
using System;
using System.Collections.Generic;
using System.Data.SqlTypes;
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

namespace School_Management.UI.Pages
{
    /// <summary>
    /// Interaction logic for AddClassPage.xaml
    /// </summary>
    public partial class AddClassPage : UserControl
    {
        public AddClassPage()
        {
            InitializeComponent();
        }

        private void MaxStudentsSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (MaxStudentsValueText is not null)
                //   MaxStudentsValueText.Text = (sender as Slider).Value.ToString();
                  MaxStudentsValueText.Text = MaxStudentsSlider.Value.ToString();
                //  MaxStudentsValueText.Text = e.NewValue.ToString();

        }
        private void AddClassClick(Object sender, RoutedEventArgs e)
        {
            //test
            //string name = "hi";
            //string number = "3";
          
            //List<string> a = new List<string>();
            //a.Add(name);
            //a.Add(number);
            //SqlExec.AddAndExecPrc("InsertNewClass" , a);
        }
    }
}
