using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace School_Management.UI.Pages
{
    public partial class WelcomePage : UserControl
    {
        public WelcomePage()
        {
            InitializeComponent();
        }

        private void EmployeeCard_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            // الحصول على النافذة الرئيسية
            var mainWindow = Window.GetWindow(this) as AdminDashboard;
            if (mainWindow != null)
            {
               // mainWindow.LoadPage("ViewAllEmployees");
            }
        }

        private void TeacherCard_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            var mainWindow = Window.GetWindow(this) as AdminDashboard;
            if (mainWindow != null)
            {
               // mainWindow.LoadPage("ViewAllTeachers");
            }
        }

        private void ClassCard_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            var mainWindow = Window.GetWindow(this) as AdminDashboard;
            if (mainWindow != null)
            {
             //   mainWindow.LoadPage("ViewAllClasses");
            }
        }

        private void StudentCard_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            var mainWindow = Window.GetWindow(this) as AdminDashboard;
            if (mainWindow != null)
            {
               // mainWindow.LoadPage("ViewAllStudents");
            }
        }
    }
}