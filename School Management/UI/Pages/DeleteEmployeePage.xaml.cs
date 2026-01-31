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
    public partial class DeleteEmployeePage : UserControl
    {
        private List<EmployeeData> searchResults = new List<EmployeeData>();
        private EmployeeData selectedEmployee = null;
        private List<EmployeeData> allEmployees = new List<EmployeeData>();

        public DeleteEmployeePage()
        {
            InitializeComponent();
            LoadStatistics();
        }

        private void LoadStatistics()
        {
            try
            {
                allEmployees.Clear();
                string query = @"
                    SELECT EmployeeID, EmployeeName, Username, IsActive, 
                           NationalNumber, JobTitle, Age, PhoneNumber, Salary, HireDate
                    FROM Employees 
                    ORDER BY EmployeeName";

                if (CurrentConnection.OpenConntion())
                {
                    using (SqlCommand cmd = new SqlCommand(query, CurrentConnection.CuCon))
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            var employee = new EmployeeData
                            {
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
                        }
                    }
                }

                UpdateStatistics();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"حدث خطأ أثناء تحميل الإحصائيات: {ex.Message}",
                    "خطأ", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                CurrentConnection.CloseConntion();
            }
        }

        private void UpdateStatistics()
        {
            TotalEmployeesText.Text = allEmployees.Count.ToString();

            int activeEmployees = 0;
            int inactiveEmployees = 0;
            double totalSalary = 0;

            foreach (var employee in allEmployees)
            {
                if (employee.IsActive)
                    activeEmployees++;
                else
                    inactiveEmployees++;

                totalSalary += employee.Salary;
            }

            ActiveEmployeesText.Text = activeEmployees.ToString();
            InactiveEmployeesText.Text = inactiveEmployees.ToString();

            if (allEmployees.Count > 0)
            {
                double avgSalary = totalSalary / allEmployees.Count;
                AvgSalaryText.Text = avgSalary.ToString("N0");
            }
            else
            {
                AvgSalaryText.Text = "0";
            }
        }

        private void SearchTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            // يمكن إضافة بحث تلقائي هنا إذا رغبت
        }

        private void SearchButton_Click(object sender, RoutedEventArgs e)
        {
            string searchText = SearchTextBox.Text.Trim();
            if (string.IsNullOrEmpty(searchText))
            {
                MessageBox.Show("يرجى إدخال نص للبحث", "تنبيه",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            SearchEmployees(searchText);
        }

        private void SearchEmployees(string searchText)
        {
            try
            {
                searchResults.Clear();
                SearchResultsPanel.Children.Clear();

                string query = @"
                    SELECT EmployeeID, EmployeeName, Username, IsActive, 
                           NationalNumber, JobTitle, Age, PhoneNumber, Salary, HireDate
                    FROM Employees 
                    WHERE EmployeeName LIKE @SearchText 
                       OR NationalNumber LIKE @SearchText 
                       OR Username LIKE @SearchText
                    ORDER BY EmployeeName";

                if (CurrentConnection.OpenConntion())
                {
                    using (SqlCommand cmd = new SqlCommand(query, CurrentConnection.CuCon))
                    {
                        cmd.Parameters.AddWithValue("@SearchText", "%" + searchText + "%");

                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            int index = 1;
                            while (reader.Read())
                            {
                                var employee = new EmployeeData
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

                                searchResults.Add(employee);
                                AddSearchResultToPanel(employee);
                                index++;
                            }
                        }
                    }
                }

                if (searchResults.Count > 0)
                {
                    SearchResultsBorder.Visibility = Visibility.Visible;
                    ResultsCountText.Text = $"({searchResults.Count} نتيجة)";
                }
                else
                {
                    SearchResultsBorder.Visibility = Visibility.Collapsed;
                    MessageBox.Show("لم يتم العثور على موظفين بهذا الاسم", "لا توجد نتائج",
                        MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"حدث خطأ أثناء البحث: {ex.Message}",
                    "خطأ", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                CurrentConnection.CloseConntion();
            }
        }

        private void AddSearchResultToPanel(EmployeeData employee)
        {
            var border = new Border
            {
                Background = employee.Index % 2 == 0 ? Brushes.White : Brushes.WhiteSmoke,
                BorderBrush = new SolidColorBrush(Color.FromRgb(229, 229, 229)),
                BorderThickness = new Thickness(0, 0, 0, 1),
                Padding = new Thickness(15, 10, 15, 10),
                Cursor = System.Windows.Input.Cursors.Hand
            };

            border.MouseLeftButtonDown += (s, e) => SelectEmployee(employee);

            var grid = new Grid();
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(50) });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(150) });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(120) });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(100) });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(80) });

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
                Foreground = new SolidColorBrush(Color.FromRgb(33, 33, 33))
            };
            Grid.SetColumn(nameText, 1);
            grid.Children.Add(nameText);

            // الرقم الوطني
            var nationalText = new TextBlock
            {
                Text = employee.NationalNumber,
                VerticalAlignment = VerticalAlignment.Center,
                Foreground = new SolidColorBrush(Color.FromRgb(117, 117, 117))
            };
            Grid.SetColumn(nationalText, 2);
            grid.Children.Add(nationalText);

            // اسم المستخدم
            var usernameText = new TextBlock
            {
                Text = employee.Username,
                VerticalAlignment = VerticalAlignment.Center,
                Foreground = new SolidColorBrush(Color.FromRgb(117, 117, 117))
            };
            Grid.SetColumn(usernameText, 3);
            grid.Children.Add(usernameText);

            // المسمى الوظيفي
            var jobText = new TextBlock
            {
                Text = employee.JobTitle,
                VerticalAlignment = VerticalAlignment.Center,
                Foreground = new SolidColorBrush(Color.FromRgb(33, 150, 243))
            };
            Grid.SetColumn(jobText, 4);
            grid.Children.Add(jobText);

            // زر التحديد
            var selectButton = new Button
            {
                Content = "تحديد",
                Width = 70,
                Height = 30,
                Background = new SolidColorBrush(Color.FromRgb(33, 150, 243)),
                Foreground = Brushes.White,
                FontSize = 12,
                FontWeight = FontWeights.Medium,
                Tag = employee
            };

            selectButton.Click += (s, e) => SelectEmployee(employee);
            Grid.SetColumn(selectButton, 5);
            grid.Children.Add(selectButton);

            border.Child = grid;
            SearchResultsPanel.Children.Add(border);
        }

        private void SelectEmployee(EmployeeData employee)
        {
            selectedEmployee = employee;
            ShowEmployeeDetails(employee);
        }

        private void ShowEmployeeDetails(EmployeeData employee)
        {
            EmployeeDetailsBorder.Visibility = Visibility.Visible;

            // تعبئة البيانات
            EmployeeNameText.Text = employee.EmployeeName;
            NationalNumberText.Text = employee.NationalNumber;
            UsernameText.Text = employee.Username;
            AgeText.Text = $"{employee.Age} سنة";
            PhoneNumberText.Text = employee.PhoneNumber;
            JobTitleText.Text = employee.JobTitle;
            SalaryText.Text = $"{employee.Salary:N0} ريال";
            HireDateText.Text = employee.HireDate.ToString("yyyy-MM-dd");
            EmployeeIDText.Text = employee.EmployeeID.ToString();

            // حالة الموظف
            if (employee.IsActive)
            {
                EmployeeStatusText.Text = "نشط";
                EmployeeStatusText.Background = new SolidColorBrush(Color.FromRgb(76, 175, 80));
            }
            else
            {
                EmployeeStatusText.Text = "غير نشط";
                EmployeeStatusText.Background = new SolidColorBrush(Color.FromRgb(244, 67, 54));
            }

            // حساب مدة الخدمة
            TimeSpan serviceDuration = DateTime.Now - employee.HireDate;
            int years = serviceDuration.Days / 365;
            int months = (serviceDuration.Days % 365) / 30;

            if (years > 0)
                ServiceDurationText.Text = $"{years} سنة و {months} شهر";
            else
                ServiceDurationText.Text = $"{months} شهر";
        }

        private void DeleteButton_Click(object sender, RoutedEventArgs e)
        {
            if (selectedEmployee == null)
            {
                MessageBox.Show("لم يتم تحديد موظف للحذف", "تنبيه",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var result = MessageBox.Show(
                $"هل أنت متأكد من حذف الموظف التالي:\n\n" +
                $"اسم الموظف: {selectedEmployee.EmployeeName}\n" +
                $"الرقم الوطني: {selectedEmployee.NationalNumber}\n" +
                $"المسمى الوظيفي: {selectedEmployee.JobTitle}\n\n" +
                $"هذا الإجراء لا يمكن التراجع عنه وسيتم حذف جميع بيانات الموظف!",
                "تأكيد حذف الموظف",
                MessageBoxButton.YesNo,
                MessageBoxImage.Warning);

            if (result == MessageBoxResult.Yes)
            {
                DeleteEmployee();
            }
        }

        private void DeleteEmployee()
        {
            try
            {
                string query = "DELETE FROM Employees WHERE EmployeeID = @EmployeeID";

                if (CurrentConnection.OpenConntion())
                {
                    using (SqlCommand cmd = new SqlCommand(query, CurrentConnection.CuCon))
                    {
                        cmd.Parameters.AddWithValue("@EmployeeID", selectedEmployee.EmployeeID);
                        int rowsAffected = cmd.ExecuteNonQuery();

                        if (rowsAffected > 0)
                        {
                            MessageBox.Show($"تم حذف الموظف {selectedEmployee.EmployeeName} بنجاح",
                                "نجاح", MessageBoxButton.OK, MessageBoxImage.Information);

                            // إعادة تعيين الواجهة
                            ResetPage();
                            LoadStatistics();
                        }
                        else
                        {
                            MessageBox.Show("فشل حذف الموظف", "خطأ",
                                MessageBoxButton.OK, MessageBoxImage.Error);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"حدث خطأ أثناء حذف الموظف: {ex.Message}",
                    "خطأ", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                CurrentConnection.CloseConntion();
            }
        }

        private void CancelDeleteButton_Click(object sender, RoutedEventArgs e)
        {
            ResetPage();
        }

        private void ResetPage()
        {
            // إعادة تعيين جميع العناصر
            selectedEmployee = null;
            searchResults.Clear();
            SearchResultsPanel.Children.Clear();
            SearchTextBox.Text = "";
            SearchResultsBorder.Visibility = Visibility.Collapsed;
            EmployeeDetailsBorder.Visibility = Visibility.Collapsed;
        }
    }

    public class EmployeeData
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