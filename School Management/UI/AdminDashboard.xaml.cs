using System.Windows;
using System.Windows.Controls;
using School_Management.UI.EmpPages;

namespace School_Management.UI
{
    public partial class AdminDashboard : Window
    {
        public AdminDashboard()
        {
            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            // تحميل الواجهة الرئيسية عند البدء
            LoadWelcomePage();
        }

        private void LoadWelcomePage()
        {
            var welcomePage = new Pages.WelcomePage();
            MainFrame.Content = welcomePage;
            PageTitle.Text = "لوحة تحكم المدير";
        }

        private void SidebarButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.Tag is string pageTag)
            {
                LoadPage(pageTag);
            }
        }

        private void LoadPage(string pageTag)
        {
            UserControl page = null;
            string title = "";

            switch (pageTag)
            {
                case "AddEmployee":
                    page = new Pages.AddEmployeePage();
                    title = "إضافة موظف جديد";
                    break;
                case "EditEmployee":
                    page = new Pages.EditEmployeePage();
                    title = "تعديل بيانات موظف";
                    break;
                case "DeleteEmployee":
                    page = new Pages.DeleteEmployeePage();
                    title = "حذف موظف";
                    break;
                case "ViewAllEmployees":
                    page = new Pages.ViewAllEmployeesPage();
                    title = "عرض جميع الموظفين";
                    break;
                case "AddTeacher":
                    page = new Pages.AddTeacherPage();
                    title = "إضافة مدرس جديد";
                    break;
                case "EditTeacher":
                    page = new Pages.EditTeacherPage();
                    title = "تعديل بيانات مدرس";
                    break;
                case "DeleteTeacher":
                    page = new Pages.DeleteTeacherPage();
                    title = "حذف مدرس";
                    break;
                case "ViewAllTeachers":
                    page = new Pages.ViewAllTeachersPage();
                    title = "عرض جميع المدرسين";
                    break;
                case "AddClass":
                    page = new Pages.AddClassPage();
                    title = "إضافة صف جديد";
                    break;
                case "EditClass":
                    page = new Pages.EditClassPage();
                    title = "تعديل بيانات صف";
                    break;
                case "DeleteClass":
                    page = new Pages.DeleteClassPage();
                    title = "حذف صف";
                    break;
                case "ViewAllClasses":
                    page = new Pages.ViewAllClassesPage();
                    title = "عرض جميع الصفوف";
                    break;
                case "AddGroup":
                    page = new Pages.AddGroupPage();
                    title = "إضافة شعبة جديدة";
                    break;
                case "EditGroup":
                    page = new Pages.EditGroupPage();
                    title = "تعديل بيانات شعبة";
                    break;
                case "DeleteGroup":
                    page = new Pages.DeleteGroupPage();
                    title = "حذف شعبة";
                    break;
                case "ViewAllGroups":
                    page = new Pages.ViewAllGroupsPage();
                    title = "عرض جميع الشعب";
                    break;
                case "ViewAllStudents":
                    page = new ViewAllStudentsPage();
                    title = "عرض جميع الطلاب";
                    break;
            }

            if (page != null)
            {
                MainFrame.Content = page;
                PageTitle.Text = title;
            }
        }

        private void HomeButton_Click(object sender, RoutedEventArgs e)
        {
            LoadWelcomePage();
        }

        private void MinimizeButton_Click(object sender, RoutedEventArgs e)
        {
            this.WindowState = WindowState.Minimized;
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void LogoutButton_Click(object sender, RoutedEventArgs e)
        {
            var result = MessageBox.Show("هل تريد تسجيل الخروج؟", "تأكيد الخروج",
                MessageBoxButton.YesNo, MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                // هنا يمكنك إضافة كود تسجيل الخروج
                this.Close();
            }
        }
    }
}