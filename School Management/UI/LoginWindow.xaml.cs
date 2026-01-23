using School_Management.Control;
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
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace School_Management.UI
{
    /// <summary>
    /// Interaction logic for LoginWindow.xaml
    /// </summary>
    public partial class LoginWindow : Window
    {
        public LoginWindow()
        {
            InitializeComponent();
        }

        private void LoginButton_Click(object sender, RoutedEventArgs e)
        {
            if (UsernameTextBox.Text == string.Empty || PasswordBox.Password == string.Empty)
            {
                MessageBox.Show("Name or Password Is Empty" , "fill all fild" ,MessageBoxButton.OK , MessageBoxImage.Warning);
                return;
            }
            if (UsernameTextBox.Text == "admin" && PasswordBox.Password == "admin")
            {
                AdminDashboard admin_ui = new AdminDashboard();
                admin_ui.Show();
            }
            if (UsernameTextBox.Text == "emp" && PasswordBox.Password == "emp")
            {
                EmployeeDashboard emp_ui = new EmployeeDashboard();
                emp_ui.Show();
            }
        }

        private void CreateDatabaseButton_Click(object sender, RoutedEventArgs e)
        {
             CreateDataBaseUI createDataBaseUI = new CreateDataBaseUI();
            createDataBaseUI.Show();
        }
        private void SelectDatabaseButton_Click(Object sender, RoutedEventArgs e)
        {
            SelectDatabaseUI selectDatabaseUI = new SelectDatabaseUI();
            selectDatabaseUI.Show();
        }
    }
}
