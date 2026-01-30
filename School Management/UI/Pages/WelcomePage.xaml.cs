using School_Management.Control;
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
            LoadStatistics();
        }
        private void LoadStatistics()
        {
            try
            {
                // جلب إحصائيات الطلاب
                var studentCount = SqlExec.GetRecordCount("SELECT COUNT(*) FROM Students");
                StudentsCountText.Text = studentCount.ToString();

                // جلب إحصائيات المدرسين
                var teacherCount = SqlExec.GetRecordCount("SELECT COUNT(*) FROM Teachers");
                TeachersCountText.Text = teacherCount.ToString();

                // جلب إحصائيات الصفوف
                var classCount = SqlExec.GetRecordCount("SELECT COUNT(*) FROM Classes");
                ClassesCountText.Text = classCount.ToString();

                // جلب إحصائيات الشعب
                var groupCount = SqlExec.GetRecordCount("SELECT COUNT(*) FROM Groups");
                GroupsCountText.Text = groupCount.ToString();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"حدث خطأ أثناء تحميل الإحصائيات: {ex.Message}",
                    "خطأ", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
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