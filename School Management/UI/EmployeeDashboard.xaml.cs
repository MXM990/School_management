using School_Management.UI.EmpPages;
using School_Management.UI.Pages;
using System;
using System.Data.SqlClient;
using System.Windows;
using System.Windows.Controls;

namespace School_Management.UI
{
    public partial class EmployeeDashboard : Window
    {
        private string employeeName;
        private string employeeUsername;

        public EmployeeDashboard(string username, string name)
        {
            InitializeComponent();
            employeeUsername = username;
            employeeName = name;
            LoadEmployeeData();
        }

        private void LoadEmployeeData()
        {
            EmployeeNameText.Text = employeeName;
            // يمكنك جلب المزيد من بيانات الموظف من قاعدة البيانات إذا لزم الأمر
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            // عرض الصفحة الرئيسية عند التحميل
            HomeButton_Click(sender, e);
        }

        private void MinimizeButton_Click(object sender, RoutedEventArgs e)
        {
            this.WindowState = WindowState.Minimized;
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }

        private void HomeButton_Click(object sender, RoutedEventArgs e)
        {
            PageTitle.Text = "لوحة تحكم الموظف";
            MainFrame.Content = new EmployeeWelcomePage();
        }

        private void SidebarButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.Tag is string pageTag)
            {
                try
                {
                    switch (pageTag)
                    {
                        case "AddStudent":
                            PageTitle.Text = "إضافة طالب جديد";
                            MainFrame.Content = new AddStudentPage();
                            break;

                        case "AddSubject": 
                            PageTitle.Text = "إضافة مادة جديدة";
                            MainFrame.Content = new AddSubjectPage();
                            break;

                        case "AssignGourpToClass":
                            PageTitle.Text = "إضافة شعبة إلى صف";
                            MainFrame.Content = new AddClassGroupPage();
                            break;

                        case "AssignStudentToClass":
                            PageTitle.Text = "إضافة طالب إلى صف";
                            MainFrame.Content = new AssignStudentToClassPage();
                            break;

                        case "AssignTeacherToClass":
                            PageTitle.Text = "إضافة مدرس إلى صف";
                            MainFrame.Content = new AssignTeacherToClassPage();
                            break;

                        case "AssignSubjects":
                            PageTitle.Text = "إعطاء مواد للطالب";
                         //   MainFrame.Content = new StudentSubjectRegistrationPage();
                            break;

                        case "ViewAllClasses":
                            PageTitle.Text = "عرض جميع الصفوف";
                            //MainFrame.Content = new ViewAllClassesPage();
                            break;

                        case "ViewClassesByGroup":
                            PageTitle.Text = "عرض الصفوف حسب الشعبة";
                            //MainFrame.Content = new ViewClassesByGroupPage();
                            break;

                        case "ViewAllTeachers":
                            PageTitle.Text = "عرض جميع المدرسين";
                            //MainFrame.Content = new ViewAllTeachersPage();
                            break;

                        case "ViewAllStudents":
                            PageTitle.Text = "عرض جميع الطلاب";
                            MainFrame.Content = new ViewAllStudentsPage();
                            break;

                        case "SearchStudent":
                            PageTitle.Text = "بحث عن طالب";
                            MainFrame.Content = new SearchStudentPage();
                            break;

                        case "ViewStudentsByClass":
                            PageTitle.Text = "عرض الطلاب حسب الصف";
                            //MainFrame.Content = new ViewStudentsByClassPage();
                            break;

                        case "Reports":
                            PageTitle.Text = "التقارير الإحصائية";
                            //MainFrame.Content = new ReportsPage();
                            break;

                        default:
                            break;
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"حدث خطأ أثناء تحميل الصفحة: {ex.Message}", "خطأ",
                        MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void LogoutButton_Click(object sender, RoutedEventArgs e)
        {
            var result = MessageBox.Show(
                "هل أنت متأكد من تسجيل الخروج؟",
                "تأكيد الخروج",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                // إغلاق نافذة الموظف والعودة لشاشة تسجيل الدخول
                LoginWindow loginWindow = new LoginWindow();
                loginWindow.Show();
                this.Close();
            }
        }
    }
}