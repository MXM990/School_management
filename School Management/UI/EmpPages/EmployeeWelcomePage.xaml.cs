using School_Management.Control;
using System;
using System.Data;
using System.Data.SqlClient;
using System.Windows;
using System.Windows.Controls;

namespace School_Management.UI.EmpPages
{
    public partial class EmployeeWelcomePage : UserControl
    {
        public EmployeeWelcomePage()
        {
            InitializeComponent();
            LoadStatistics();
        }

        private void LoadStatistics()
        {
            try
            {
                // جلب إحصائيات الطلاب
                var studentCount = GetRecordCount("SELECT COUNT(*) FROM Students");
                StudentsCountText.Text = studentCount.ToString();

                // جلب إحصائيات المدرسين
                var teacherCount = GetRecordCount("SELECT COUNT(*) FROM Teachers");
                TeachersCountText.Text = teacherCount.ToString();

                // جلب إحصائيات الصفوف
                var classCount = GetRecordCount("SELECT COUNT(*) FROM Classes");
                ClassesCountText.Text = classCount.ToString();

                // جلب إحصائيات الشعب
                var groupCount = GetRecordCount("SELECT COUNT(*) FROM Groups");
                GroupsCountText.Text = groupCount.ToString();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"حدث خطأ أثناء تحميل الإحصائيات: {ex.Message}",
                    "خطأ", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private int GetRecordCount(string query)
        {
            try
            {
                if (CurrentConnection.OpenConntion())
                {
                    using (SqlCommand cmd = new SqlCommand(query, CurrentConnection.CuCon))
                    {
                        var result = cmd.ExecuteScalar();
                        return Convert.ToInt32(result);
                    }
                }
                return 0;
            }
            catch
            {
                return 0;
            }
            finally
            {
                CurrentConnection.CloseConntion();
            }
        }

        private void QuickAddStudentBtn_Click(object sender, RoutedEventArgs e)
        {
            // الانتقال إلى صفحة إضافة طالب
            var parentWindow = Window.GetWindow(this) as EmployeeDashboard;
            if (parentWindow != null)
            {
              //  parentWindow.SidebarButton_Click(FindButtonByName(parentWindow, "AddStudentButton"), e);
            }
        }

        private void QuickViewStudentsBtn_Click(object sender, RoutedEventArgs e)
        {
            // الانتقال إلى صفحة عرض الطلاب
            var parentWindow = Window.GetWindow(this) as EmployeeDashboard;
            if (parentWindow != null)
            {
              //  parentWindow.SidebarButton_Click(FindButtonByName(parentWindow, "ViewAllStudentsButton"), e);
            }
        }

        private void QuickAssignStudentsBtn_Click(object sender, RoutedEventArgs e)
        {
            // الانتقال إلى صفحة توزيع الطلاب
            var parentWindow = Window.GetWindow(this) as EmployeeDashboard;
            if (parentWindow != null)
            {
               // parentWindow.SidebarButton_Click(FindButtonByName(parentWindow, "AssignStudentToClassButton"), e);
            }
        }

        private void QuickReportsBtn_Click(object sender, RoutedEventArgs e)
        {
            // الانتقال إلى صفحة التقارير
            var parentWindow = Window.GetWindow(this) as EmployeeDashboard;
            if (parentWindow != null)
            {
               // parentWindow.SidebarButton_Click(FindButtonByName(parentWindow, "ReportsButton"), e);
            }
        }

        private Button FindButtonByName(EmployeeDashboard window, string buttonName)
        {
            return window.FindName(buttonName) as Button;
        }
    }
}