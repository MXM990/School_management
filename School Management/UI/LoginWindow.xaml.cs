using School_Management.Control;
using System;
using System.Data;
using System.Data.SqlClient;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

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

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            // تحديث حالة الاتصال عند تحميل النافذة
            UpdateConnectionStatus();
           
        }

        private void UpdateConnectionStatus()
        {
            try
            {
                // محاولة فتح الاتصال
                if (CurrentConnection.OpenConntion())
                {
                    // الاتصال ناجح
                    ConnectionIndicator.Fill = new SolidColorBrush(Colors.Green);
                    ConnectionStatusText.Text = "متصل";
                    ConnectionStatusText.Foreground = new SolidColorBrush(Colors.Green);

                    // إغلاق الاتصال بعد التحقق
                    CurrentConnection.CloseConntion();
                }
                else
                {
                    // الاتصال فاشل
                    ConnectionIndicator.Fill = new SolidColorBrush(Colors.Red);
                    ConnectionStatusText.Text = "غير متصل";
                    ConnectionStatusText.Foreground = new SolidColorBrush(Colors.Red);
                }
            }
            catch (Exception ex)
            {
                // في حالة وجود خطأ
                ConnectionIndicator.Fill = new SolidColorBrush(Colors.Red);
                ConnectionStatusText.Text = "خطأ في الاتصال";
                ConnectionStatusText.Foreground = new SolidColorBrush(Colors.Red);

                Console.WriteLine($"خطأ في التحقق من الاتصال: {ex.Message}");
            }
        }

        private void LoginButton_Click(object sender, RoutedEventArgs e)
        {
            if (UsernameTextBox.Text == string.Empty || PasswordBox.Password == string.Empty)
            {
                MessageBox.Show("يرجى إدخال اسم المستخدم وكلمة المرور", "حقول فارغة", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            UpdateConnectionStatus();

            if (CurrentConnection.CuCon == null || !CurrentConnection.OpenConntion())
            {
                MessageBox.Show("لا يوجد اتصال بقاعدة البيانات. يرجى اختيار قاعدة بيانات أولاً.", "خطأ في الاتصال", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            try
            {
                // التحقق من مدير النظام (admin/admin)
                if (UsernameTextBox.Text == "admin" && PasswordBox.Password == "admin")
                {
                    AdminDashboard admin_ui = new AdminDashboard();
                    admin_ui.Show();
                    return;
                }

                // التحقق من الموظفين في قاعدة البيانات
                string query = @"
                    SELECT EmployeeID, EmployeeName, Username, Password, IsActive, JobTitle 
                    FROM Employees 
                    WHERE Username = @Username AND Password = @Password";

                using (SqlCommand cmd = new SqlCommand(query, CurrentConnection.CuCon))
                {
                    cmd.Parameters.AddWithValue("@Username", UsernameTextBox.Text);
                    cmd.Parameters.AddWithValue("@Password", PasswordBox.Password);

                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            // التحقق من حالة الموظف (نشط/غير نشط)
                            bool isActive = reader.GetBoolean(4);

                            if (!isActive)
                            {
                                MessageBox.Show("حساب المستخدم غير نشط. يرجى الاتصال بالمسؤول.", "حساب غير نشط", MessageBoxButton.OK, MessageBoxImage.Warning);
                                return;
                            }
                            string employeeUserName= reader.GetString(1);
                            string employeeName = reader.GetString(2);

                         

                            // افتح واجهة الموظف
                            EmployeeDashboard emp_ui = new EmployeeDashboard(employeeUserName, employeeName);
                            emp_ui.Show();
                        }
                        else
                        {
                            MessageBox.Show("اسم المستخدم أو كلمة المرور غير صحيحة", "خطأ في الدخول", MessageBoxButton.OK, MessageBoxImage.Error);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"حدث خطأ أثناء تسجيل الدخول: {ex.Message}", "خطأ", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                CurrentConnection.CloseConntion();
            }
        }

        private void CreateDatabaseButton_Click(object sender, RoutedEventArgs e)
        {
            CreateDataBaseUI createDataBaseUI = new CreateDataBaseUI();
            createDataBaseUI.Closed += (s, args) => UpdateConnectionStatus();
            createDataBaseUI.ShowDialog();
        }

        private void SelectDatabaseButton_Click(object sender, RoutedEventArgs e)
        {
            SelectDatabaseUI selectDatabaseUI = new SelectDatabaseUI();
            selectDatabaseUI.Closed += (s, args) => UpdateConnectionStatus();
            selectDatabaseUI.ShowDialog();
        }

        private void ExitButton_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }

        private void ForgotPassword_MouseDown(object sender, MouseButtonEventArgs e)
        {
            MessageBox.Show("في حالة نسيان كلمة المرور، يرجى الاتصال بمسؤول النظام.", "نسيان كلمة المرور", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void MinimizeButton_Click(object sender, RoutedEventArgs e)
        {
            this.WindowState = WindowState.Minimized;
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }

      
    }
}