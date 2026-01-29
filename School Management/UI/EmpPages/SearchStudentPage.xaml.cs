using School_Management.Control;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace School_Management.UI.EmpPages
{
    public partial class SearchStudentPage : UserControl
    {
        private List<StudentResult> searchResults = new List<StudentResult>();
        private List<ClassData> allClasses = new List<ClassData>();

        public SearchStudentPage()
        {
            InitializeComponent();
            LoadClasses();
        }

        private void LoadClasses()
        {
            try
            {
                allClasses.Clear();
                allClasses.Add(new ClassData { ClassID = Guid.Empty, EducationLevel = "جميع الصفوف" });

                string query = "SELECT ClassID, EducationLevel FROM Classes ORDER BY EducationLevel";

                if (CurrentConnection.OpenConntion())
                {
                    using (SqlCommand cmd = new SqlCommand(query, CurrentConnection.CuCon))
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            allClasses.Add(new ClassData
                            {
                                ClassID = reader.GetGuid(0),
                                EducationLevel = reader.GetString(1)
                            });
                        }
                    }
                }

                ClassFilterComboBox.ItemsSource = allClasses;
                ClassFilterComboBox.SelectedIndex = 0;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"حدث خطأ أثناء تحميل الصفوف: {ex.Message}",
                    "خطأ", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                CurrentConnection.CloseConntion();
            }
        }

        private void SearchStudents()
        {
            string searchText = SearchTextBox.Text.Trim();

            if (string.IsNullOrEmpty(searchText))
            {
                MessageBox.Show("الرجاء إدخال نص للبحث", "تنبيه",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                searchResults.Clear();
                ResultsPanel.Children.Clear();

                string baseQuery = @"
                    SELECT s.StudentID, s.StudentName, s.NationalNumber, s.Age, 
                           s.RegistrationStatus, s.PhoneNumber, s.Email,
                           c.EducationLevel, g.GroupName
                    FROM Students s
                    LEFT JOIN Student_Class_Group scg ON s.StudentID = scg.StudentID
                    LEFT JOIN Class_Group cg ON scg.ClassGroupID = cg.ClassGroupID
                    LEFT JOIN Classes c ON cg.ClassID = c.ClassID
                    LEFT JOIN Groups g ON cg.GroupID = g.GroupID
                    WHERE (s.StudentName LIKE @Search OR s.NationalNumber LIKE @Search)";

                // تطبيق الفلاتر
                if (AgeFilterComboBox.SelectedIndex > 0)
                {
                    switch (AgeFilterComboBox.SelectedIndex)
                    {
                        case 1: baseQuery += " AND s.Age BETWEEN 6 AND 10"; break;
                        case 2: baseQuery += " AND s.Age BETWEEN 11 AND 14"; break;
                        case 3: baseQuery += " AND s.Age BETWEEN 15 AND 18"; break;
                    }
                }

                if (StatusFilterComboBox.SelectedIndex > 0)
                {
                    string status = (StatusFilterComboBox.SelectedItem as ComboBoxItem)?.Content.ToString();
                    baseQuery += $" AND s.RegistrationStatus = '{status}'";
                }

                if (ClassFilterComboBox.SelectedIndex > 0)
                {
                    var selectedClass = ClassFilterComboBox.SelectedItem as ClassData;
                    baseQuery += $" AND c.ClassID = '{selectedClass.ClassID}'";
                }

                baseQuery += " ORDER BY s.StudentName";

                if (CurrentConnection.OpenConntion())
                {
                    using (SqlCommand cmd = new SqlCommand(baseQuery, CurrentConnection.CuCon))
                    {
                        cmd.Parameters.AddWithValue("@Search", $"%{searchText}%");

                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                var student = new StudentResult
                                {
                                    StudentID = reader.GetGuid(0),
                                    StudentName = reader.GetString(1),
                                    NationalNumber = reader.GetString(2),
                                    Age = reader.GetInt32(3),
                                    RegistrationStatus = reader.GetString(4),
                                    PhoneNumber = reader.GetString(5),
                                    Email = reader.IsDBNull(6) ? "" : reader.GetString(6),
                                    EducationLevel = reader.IsDBNull(7) ? "غير مسجل" : reader.GetString(7),
                                    GroupName = reader.IsDBNull(8) ? "" : reader.GetString(8)
                                };

                                searchResults.Add(student);
                                AddResultToPanel(student);
                            }
                        }
                    }
                }

                UpdateResultsDisplay();
                UpdateStatistics();
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

        private void AddResultToPanel(StudentResult student)
        {
            var border = new Border
            {
                Background = Brushes.White,
                BorderBrush = new SolidColorBrush(Color.FromRgb(229, 229, 229)),
                BorderThickness = new Thickness(0, 0, 0, 1),
                Padding = new Thickness(15, 10, 15, 10),
                Margin = new Thickness(0, 0, 0, 10)
            };

            var grid = new Grid();
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(150) });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(100) });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(150) });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(100) });

            // معلومات الطالب
            var studentInfoPanel = new StackPanel();

            var nameText = new TextBlock
            {
                Text = student.StudentName,
                FontWeight = FontWeights.Bold,
                FontSize = 14,
                Foreground = new SolidColorBrush(Color.FromRgb(33, 33, 33))
            };

            var nationalText = new TextBlock
            {
                Text = $"الرقم الوطني: {student.NationalNumber}",
                FontSize = 12,
                Foreground = new SolidColorBrush(Color.FromRgb(117, 117, 117))
            };

            studentInfoPanel.Children.Add(nameText);
            studentInfoPanel.Children.Add(nationalText);
            Grid.SetColumn(studentInfoPanel, 0);
            grid.Children.Add(studentInfoPanel);

            // العمر والحالة
            var detailsPanel = new StackPanel();

            var ageText = new TextBlock
            {
                Text = $"{student.Age} سنة",
                FontSize = 12,
                Foreground = new SolidColorBrush(Color.FromRgb(33, 33, 33))
            };

            var statusBorder = new Border
            {
                CornerRadius = new CornerRadius(5),
                Padding = new Thickness(5, 2, 5, 2),
                Background = GetStatusColor(student.RegistrationStatus),
                Child = new TextBlock
                {
                    Text = student.RegistrationStatus,
                    FontSize = 10,
                    Foreground = Brushes.White,
                    HorizontalAlignment = HorizontalAlignment.Center
                },
                Margin = new Thickness(0, 5, 0, 0)
            };

            detailsPanel.Children.Add(ageText);
            detailsPanel.Children.Add(statusBorder);
            Grid.SetColumn(detailsPanel, 1);
            grid.Children.Add(detailsPanel);

            // الصف والشعبة
            var classGroupPanel = new StackPanel();

            var classText = new TextBlock
            {
                Text = student.EducationLevel,
                FontSize = 12,
                Foreground = new SolidColorBrush(Color.FromRgb(33, 33, 33))
            };

            var groupText = new TextBlock
            {
                Text = student.GroupName,
                FontSize = 12,
                Foreground = new SolidColorBrush(Color.FromRgb(117, 117, 117))
            };

            classGroupPanel.Children.Add(classText);
            classGroupPanel.Children.Add(groupText);
            Grid.SetColumn(classGroupPanel, 2);
            grid.Children.Add(classGroupPanel);

            // معلومات الاتصال
            var contactPanel = new StackPanel();

            var phoneText = new TextBlock
            {
                Text = student.PhoneNumber,
                FontSize = 12,
                Foreground = new SolidColorBrush(Color.FromRgb(33, 33, 33))
            };

            var emailText = new TextBlock
            {
                Text = string.IsNullOrEmpty(student.Email) ? "لا يوجد بريد" : student.Email,
                FontSize = 12,
                Foreground = new SolidColorBrush(Color.FromRgb(117, 117, 117))
            };

            contactPanel.Children.Add(phoneText);
            contactPanel.Children.Add(emailText);
            Grid.SetColumn(contactPanel, 3);
            grid.Children.Add(contactPanel);

            // أزرار الإجراءات
            var actionsPanel = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center
            };

            var viewButton = new Button
            {
                Content = "👁️",
                ToolTip = "عرض التفاصيل",
                Width = 30,
                Height = 30,
                Background = new SolidColorBrush(Color.FromRgb(225, 245, 254)),
                BorderBrush = new SolidColorBrush(Color.FromRgb(144, 202, 249)),
                Margin = new Thickness(0, 0, 5, 0),
                Tag = student.StudentID,
               // Click += ViewResultButton_Click
            };

            var assignButton = new Button
            {
                Content = "↪️",
                ToolTip = "إضافة إلى صف",
                Width = 30,
                Height = 30,
                Background = new SolidColorBrush(Color.FromRgb(232, 245, 233)),
                BorderBrush = new SolidColorBrush(Color.FromRgb(165, 214, 167)),
                Tag = student.StudentID,
               // Click += AssignResultButton_Click
            };

            actionsPanel.Children.Add(viewButton);
            actionsPanel.Children.Add(assignButton);
            Grid.SetColumn(actionsPanel, 4);
            grid.Children.Add(actionsPanel);

            border.Child = grid;
            ResultsPanel.Children.Add(border);
        }

        private Brush GetStatusColor(string status)
        {
            switch (status)
            {
                case "مسجل": return new SolidColorBrush(Color.FromRgb(76, 175, 80));
                case "منتظم": return new SolidColorBrush(Color.FromRgb(33, 150, 243));
                case "منقطع": return new SolidColorBrush(Color.FromRgb(255, 152, 0));
                case "ملغى": return new SolidColorBrush(Color.FromRgb(244, 67, 54));
                default: return new SolidColorBrush(Color.FromRgb(158, 158, 158));
            }
        }

        private void UpdateResultsDisplay()
        {
            if (searchResults.Count > 0)
            {
                ResultsBorder.Visibility = Visibility.Visible;
                NoResultsText.Visibility = Visibility.Collapsed;
                StatisticsBorder.Visibility = Visibility.Visible;
                ResultsTitle.Text = $"نتائج البحث ({searchResults.Count})";
            }
            else
            {
                ResultsBorder.Visibility = Visibility.Visible;
                NoResultsText.Visibility = Visibility.Visible;
                StatisticsBorder.Visibility = Visibility.Collapsed;
                ResultsTitle.Text = "نتائج البحث";
            }
        }

        private void UpdateStatistics()
        {
            ResultsCountText.Text = searchResults.Count.ToString();

            if (searchResults.Count > 0)
            {
                double totalAge = 0;
                foreach (var student in searchResults)
                {
                    totalAge += student.Age;
                }
                double averageAge = totalAge / searchResults.Count;
                AverageAgeText.Text = $"{averageAge:F1} سنة";

                // حساب النسبة المئوية من إجمالي الطلاب
                int totalStudents = GetTotalStudentsCount();
                if (totalStudents > 0)
                {
                    double percentage = (searchResults.Count * 100.0) / totalStudents;
                    PercentageText.Text = $"{percentage:F1}%";
                }
                else
                {
                    PercentageText.Text = "0%";
                }
            }
            else
            {
                AverageAgeText.Text = "0";
                PercentageText.Text = "0%";
            }
        }

        private int GetTotalStudentsCount()
        {
            try
            {
                string query = "SELECT COUNT(*) FROM Students";

                if (CurrentConnection.OpenConntion())
                {
                    using (SqlCommand cmd = new SqlCommand(query, CurrentConnection.CuCon))
                    {
                        return Convert.ToInt32(cmd.ExecuteScalar());
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

        #region أحداث التحكم

        private void SearchTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            // يمكن إضافة بحث فوري هنا إذا رغبت
        }

        private void SearchTextBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                SearchStudents();
            }
        }

        private void SearchButton_Click(object sender, RoutedEventArgs e)
        {
            SearchStudents();
        }

        private void ApplyFiltersButton_Click(object sender, RoutedEventArgs e)
        {
            SearchStudents();
        }

        private void ViewResultButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.Tag is Guid studentId)
            {
                var student = searchResults.Find(s => s.StudentID == studentId);
                if (student != null)
                {
                    string message = $"اسم الطالب: {student.StudentName}\n" +
                                   $"الرقم الوطني: {student.NationalNumber}\n" +
                                   $"العمر: {student.Age} سنة\n" +
                                   $"حالة التسجيل: {student.RegistrationStatus}\n" +
                                   $"رقم الهاتف: {student.PhoneNumber}\n" +
                                   $"البريد الإلكتروني: {student.Email}\n" +
                                   $"الصف: {student.EducationLevel}\n" +
                                   $"الشعبة: {student.GroupName}";

                    MessageBox.Show(message, "تفاصيل الطالب",
                        MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
        }

        private void AssignResultButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.Tag is Guid studentId)
            {
                // الانتقال إلى صفحة إضافة طالب إلى صف/شعبة مع الطالب المحدد
                MessageBox.Show("سيتم فتح صفحة إضافة الطالب إلى الصف/الشعبة",
                    "تحويل", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        #endregion
    }

    public class StudentResult
    {
        public Guid StudentID { get; set; }
        public string StudentName { get; set; }
        public string NationalNumber { get; set; }
        public int Age { get; set; }
        public string RegistrationStatus { get; set; }
        public string PhoneNumber { get; set; }
        public string Email { get; set; }
        public string EducationLevel { get; set; }
        public string GroupName { get; set; }
    }

    public class ClassData
    {
        public Guid ClassID { get; set; }
        public string EducationLevel { get; set; }
    }
}