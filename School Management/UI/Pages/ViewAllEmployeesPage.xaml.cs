using School_Management.Control;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace School_Management.UI.Pages
{
    public partial class ViewEmployeesPage : UserControl
    {
        private List<EmployeeData_view> allEmployees = new List<EmployeeData_view>();
        private List<EmployeeData_view> filteredEmployees = new List<EmployeeData_view>();
        private List<string> jobTitles = new List<string>();

        public ViewEmployeesPage()
        {
            InitializeComponent();
            LoadEmployees();
        }

        private void LoadEmployees()
        {
            try
            {
                allEmployees.Clear();
                EmployeesListPanel.Children.Clear();
                jobTitles.Clear();

                // تنظيف ComboBox
                JobFilterComboBox.Items.Clear();
                JobFilterComboBox.Items.Add(new ComboBoxItem { Content = "جميع الوظائف" });

                // الاستعلام المطلوب
                string query = @"
                    SELECT 
                        EmployeeID,
                        EmployeeName,
                        Username,
                        IsActive,
                        NationalNumber,
                        JobTitle,
                        Age,
                        PhoneNumber,
                        Salary,
                        HireDate
                    FROM Employees 
                    ORDER BY EmployeeName";

                if (CurrentConnection.OpenConntion())
                {
                    using (SqlCommand cmd = new SqlCommand(query, CurrentConnection.CuCon))
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        int index = 1;
                        while (reader.Read())
                        {
                            var employee = new EmployeeData_view
                            {
                                Index = index,
                                EmployeeID = reader.GetGuid(0),
                                EmployeeName = reader.GetString(1),
                                Username = reader.GetString(2),
                                IsActive = reader.GetBoolean(3),
                                NationalNumber = reader.GetString(4),
                                JobTitle = reader.GetString(5),
                                Age = reader.GetInt32(6),
                                PhoneNumber = reader.GetString(7),
                                Salary = reader.GetInt32(8),
                                HireDate = reader.GetDateTime(9)
                            };

                            allEmployees.Add(employee);
                            AddEmployeeToPanel(employee);

                            // تجميع الوظائف المختلفة
                            if (!jobTitles.Contains(employee.JobTitle))
                            {
                                jobTitles.Add(employee.JobTitle);
                            }

                            index++;
                        }
                    }
                }

                // إضافة الوظائف إلى ComboBox
                foreach (var job in jobTitles)
                {
                    JobFilterComboBox.Items.Add(new ComboBoxItem { Content = job });
                }

                UpdateStatistics();
                filteredEmployees = new List<EmployeeData_view>(allEmployees);
                StatusFilterComboBox.SelectedIndex = 0;
                JobFilterComboBox.SelectedIndex = 0;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"حدث خطأ أثناء تحميل الموظفين: {ex.Message}",
                    "خطأ", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                CurrentConnection.CloseConntion();
            }
        }

        private void AddEmployeeToPanel(EmployeeData_view employee)
        {
            var border = new Border
            {
                Background = employee.Index % 2 == 0 ? Brushes.White : Brushes.WhiteSmoke,
                BorderBrush = new SolidColorBrush(Color.FromRgb(229, 229, 229)),
                BorderThickness = new Thickness(0, 0, 0, 1),
                Padding = new Thickness(15, 10, 15, 10)
            };

            var grid = new Grid();
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(50) });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(100) });

            // الرقم التسلسلي
            var indexText = new TextBlock
            {
                Text = employee.Index.ToString(),
                VerticalAlignment = VerticalAlignment.Center,
                Foreground = new SolidColorBrush(Color.FromRgb(117, 117, 117))
            };
            Grid.SetColumn(indexText, 0);
            grid.Children.Add(indexText);

            // اسم الموظف
            var nameText = new TextBlock
            {
                Text = employee.EmployeeName,
                VerticalAlignment = VerticalAlignment.Center,
                FontWeight = FontWeights.Medium,
                Foreground = new SolidColorBrush(Color.FromRgb(44, 62, 80))
            };
            Grid.SetColumn(nameText, 1);
            grid.Children.Add(nameText);

            // المسمى الوظيفي
            var jobText = new TextBlock
            {
                Text = employee.JobTitle,
                VerticalAlignment = VerticalAlignment.Center,
                FontWeight = FontWeights.Medium,
                Foreground = new SolidColorBrush(Color.FromRgb(52, 152, 219))
            };
            Grid.SetColumn(jobText, 2);
            grid.Children.Add(jobText);

            // الراتب
            var salaryText = new TextBlock
            {
                Text = $"{employee.Salary:N0}",
                VerticalAlignment = VerticalAlignment.Center,
                FontWeight = FontWeights.Medium,
                Foreground = new SolidColorBrush(Color.FromRgb(243, 156, 18))
            };
            Grid.SetColumn(salaryText, 3);
            grid.Children.Add(salaryText);

            // تاريخ التعيين
            var hireDateText = new TextBlock
            {
                Text = employee.HireDate.ToString("yyyy-MM-dd"),
                VerticalAlignment = VerticalAlignment.Center,
                Foreground = new SolidColorBrush(Color.FromRgb(127, 140, 141))
            };
            Grid.SetColumn(hireDateText, 4);
            grid.Children.Add(hireDateText);

            // الحالة
            var statusBorder = new Border
            {
                CornerRadius = new CornerRadius(5),
                Padding = new Thickness(10, 5, 10, 5),
                VerticalAlignment = VerticalAlignment.Center,
                Width = 70,
                Background = employee.IsActive ?
                    new SolidColorBrush(Color.FromRgb(39, 174, 96)) :
                    new SolidColorBrush(Color.FromRgb(231, 76, 60)),
                BorderBrush = employee.IsActive ?
                    new SolidColorBrush(Color.FromRgb(46, 204, 113)) :
                    new SolidColorBrush(Color.FromRgb(192, 57, 43)),
                BorderThickness = new Thickness(1),
                Child = new TextBlock
                {
                    Text = employee.IsActive ? "نشط" : "غير نشط",
                    HorizontalAlignment = HorizontalAlignment.Center,
                    VerticalAlignment = VerticalAlignment.Center,
                    Foreground = Brushes.White,
                    FontWeight = FontWeights.Bold
                }
            };
            Grid.SetColumn(statusBorder, 5);
            grid.Children.Add(statusBorder);

            // أزرار الإجراءات
            var actionsPanel = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center,
            };

            var viewButton = new Button
            {
                Content = "👁️",
                ToolTip = "عرض التفاصيل",
                Width = 30,
                Height = 30,
                Background = new SolidColorBrush(Color.FromRgb(225, 245, 254)),
                BorderBrush = new SolidColorBrush(Color.FromRgb(144, 202, 249)),
                Tag = employee.EmployeeID
            };

            
            actionsPanel.Children.Add(viewButton);
            

            Grid.SetColumn(actionsPanel, 6);
            grid.Children.Add(actionsPanel);

            border.Child = grid;
            EmployeesListPanel.Children.Add(border);

            viewButton.Click += ViewEmployeeButton_Click;
        }

        private void UpdateStatistics()
        {
            TotalEmployeesText.Text = allEmployees.Count.ToString();

            if (allEmployees.Count > 0)
            {
                int activeEmployees = 0;
                int totalSalary = 0;
                HashSet<string> uniqueJobs = new HashSet<string>();

                foreach (var employee in allEmployees)
                {
                    // حساب الموظفين النشطين
                    if (employee.IsActive)
                        activeEmployees++;

                    // حساب مجموع الرواتب
                    totalSalary += employee.Salary;

                    // جمع الوظائف المختلفة
                    uniqueJobs.Add(employee.JobTitle);
                }

                ActiveEmployeesText.Text = activeEmployees.ToString();
                InactiveEmployeesText.Text = (allEmployees.Count - activeEmployees).ToString();

                // حساب متوسط الراتب
                int averageSalary = allEmployees.Count > 0 ? totalSalary / allEmployees.Count : 0;
                AverageSalaryText.Text = $"{averageSalary:N0}";

                DifferentJobsText.Text = uniqueJobs.Count.ToString();
            }
            else
            {
                ActiveEmployeesText.Text = "0";
                InactiveEmployeesText.Text = "0";
                AverageSalaryText.Text = "0";
                DifferentJobsText.Text = "0";
            }
        }

        private void SearchTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            FilterEmployees();
        }

        private void StatusFilterComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            FilterEmployees();
        }

        private void JobFilterComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            FilterEmployees();
        }

        private void FilterEmployees()
        {
            string searchText = SearchTextBox.Text.ToLower();
            string selectedStatusFilter = (StatusFilterComboBox.SelectedItem as ComboBoxItem)?.Content.ToString();
            string selectedJobFilter = (JobFilterComboBox.SelectedItem as ComboBoxItem)?.Content.ToString();

            filteredEmployees.Clear();
            EmployeesListPanel.Children.Clear();

            foreach (var employee in allEmployees)
            {
                bool matchesSearch = string.IsNullOrEmpty(searchText) ||
                                   employee.EmployeeName.ToLower().Contains(searchText) ||
                                   employee.JobTitle.ToLower().Contains(searchText) ||
                                   employee.NationalNumber.ToLower().Contains(searchText) ||
                                   employee.Username.ToLower().Contains(searchText) ||
                                   employee.PhoneNumber.ToLower().Contains(searchText);

                bool matchesStatusFilter = selectedStatusFilter == "جميع الموظفين" ||
                    (selectedStatusFilter == "نشط فقط" && employee.IsActive) ||
                    (selectedStatusFilter == "غير نشط" && !employee.IsActive);

                bool matchesJobFilter = selectedJobFilter == "جميع الوظائف" ||
                    (selectedJobFilter == employee.JobTitle);

                if (matchesSearch && matchesStatusFilter && matchesJobFilter)
                {
                    filteredEmployees.Add(employee);
                    AddEmployeeToPanel(employee);
                }
            }
        }

        private void RefreshButton_Click(object sender, RoutedEventArgs e)
        {
            LoadEmployees();
            SearchTextBox.Text = "";
            StatusFilterComboBox.SelectedIndex = 0;
            JobFilterComboBox.SelectedIndex = 0;
        }

        private void ViewEmployeeButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.Tag is Guid employeeID)
            {
                var employee = allEmployees.Find(emp => emp.EmployeeID == employeeID);
                if (employee != null)
                {
                    string statusText = employee.IsActive ? "نشط ✅" : "غير نشط ❌";

                    string message = $"تفاصيل الموظف:\n\n" +
                                   $"👤 الاسم الكامل: {employee.EmployeeName}\n" +
                                   $"👤 اسم المستخدم: {employee.Username}\n" +
                                   $"🆔 الرقم الوطني: {employee.NationalNumber}\n" +
                                   $"👔 المسمى الوظيفي: {employee.JobTitle}\n" +
                                   $"📅 العمر: {employee.Age} سنة\n" +
                                   $"📞 رقم الهاتف: {employee.PhoneNumber}\n" +
                                   $"💰 الراتب: {employee.Salary:N0}\n" +
                                   $"📅 تاريخ التعيين: {employee.HireDate:yyyy-MM-dd}\n" +
                                   $"✅ الحالة: {statusText}\n\n" +
                                   $"تفاصيل إضافية:\n" +
                                   $"• تاريخ التحميل: {DateTime.Now:yyyy-MM-dd HH:mm}\n" +
                                   $"• عدد الموظفين الكلي: {allEmployees.Count}";

                    MessageBox.Show(message, "تفاصيل الموظف",
                        MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
        }

    }

    public class EmployeeData_view
    {
        public int Index { get; set; }
        public Guid EmployeeID { get; set; }
        public string EmployeeName { get; set; }
        public string Username { get; set; }
        public bool IsActive { get; set; }
        public string NationalNumber { get; set; }
        public string JobTitle { get; set; }
        public int Age { get; set; }
        public string PhoneNumber { get; set; }
        public int Salary { get; set; }
        public DateTime HireDate { get; set; }
    }
}