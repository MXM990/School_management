using School_Management.Control;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace School_Management.UI.EmpPages
{
    public partial class ViewAllStudentsPage : UserControl
    {
        private List<StudentData> allStudents = new List<StudentData>();
        private List<StudentData> filteredStudents = new List<StudentData>();

        public ViewAllStudentsPage()
        {
            InitializeComponent();
            LoadStudents();
        }

        private void LoadStudents()
        {
            try
            {
                allStudents.Clear();
                StudentsListPanel.Children.Clear();

                string query = @"
                    SELECT StudentID, StudentName, NationalNumber, Age, 
                           RegistrationStatus, PhoneNumber, Email, BirthDate
                    FROM Students 
                    ORDER BY StudentName";

                if (CurrentConnection.OpenConntion())
                {
                    using (SqlCommand cmd = new SqlCommand(query, CurrentConnection.CuCon))
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        int index = 1;
                        while (reader.Read())
                        {
                            var student = new StudentData
                            {
                                Index = index,
                                StudentID = reader.GetGuid(0),
                                StudentName = reader.GetString(1),
                                NationalNumber = reader.GetString(2),
                                Age = reader.GetInt32(3),
                                RegistrationStatus = reader.GetString(4),
                                PhoneNumber = reader.GetString(5),
                                Email = reader.IsDBNull(6) ? "" : reader.GetString(6),
                                BirthDate = reader.GetDateTime(7)
                            };

                            allStudents.Add(student);
                            AddStudentToPanel(student);
                            index++;
                        }
                    }
                }

                UpdateStatistics();
                filteredStudents = new List<StudentData>(allStudents);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"حدث خطأ أثناء تحميل الطلاب: {ex.Message}",
                    "خطأ", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                CurrentConnection.CloseConntion();
            }
        }

        private void AddStudentToPanel(StudentData student)
        {
            var border = new Border
            {
                Background = student.Index % 2 == 0 ? Brushes.White : Brushes.WhiteSmoke,
                BorderBrush = new SolidColorBrush(Color.FromRgb(229, 229, 229)),
                BorderThickness = new Thickness(0, 0, 0, 1),
                Padding = new Thickness(15, 10, 15, 10)
            };

            var grid = new Grid();
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(50) });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(120) });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(120) });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(150) });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(120) });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(100) });

            // الرقم التسلسلي
            var indexText = new TextBlock
            {
                Text = student.Index.ToString(),
                VerticalAlignment = VerticalAlignment.Center,
                Foreground = new SolidColorBrush(Color.FromRgb(117, 117, 117))
            };
            Grid.SetColumn(indexText, 0);
            grid.Children.Add(indexText);

            // اسم الطالب
            var nameText = new TextBlock
            {
                Text = student.StudentName,
                VerticalAlignment = VerticalAlignment.Center,
                FontWeight = FontWeights.Medium,
                Foreground = new SolidColorBrush(Color.FromRgb(33, 33, 33))
            };
            Grid.SetColumn(nameText, 1);
            grid.Children.Add(nameText);

            // الرقم الوطني
            var nationalText = new TextBlock
            {
                Text = student.NationalNumber,
                VerticalAlignment = VerticalAlignment.Center,
                Foreground = new SolidColorBrush(Color.FromRgb(117, 117, 117)),
                FontFamily = new FontFamily("Consolas")
            };
            Grid.SetColumn(nationalText, 2);
            grid.Children.Add(nationalText);

            // العمر
            var ageText = new TextBlock
            {
                Text = $"{student.Age} سنة",
                VerticalAlignment = VerticalAlignment.Center,
                Foreground = new SolidColorBrush(Color.FromRgb(117, 117, 117))
            };
            Grid.SetColumn(ageText, 3);
            grid.Children.Add(ageText);

            // حالة التسجيل
            var statusBorder = new Border
            {
                CornerRadius = new CornerRadius(10),
                Padding = new Thickness(10, 5, 10, 5),
                Width = 120,
                VerticalAlignment = VerticalAlignment.Center,
                Background = GetStatusColor(student.RegistrationStatus),
                Child = new TextBlock
                {
                    Text = student.RegistrationStatus,
                    HorizontalAlignment = HorizontalAlignment.Center,
                    VerticalAlignment = VerticalAlignment.Center,
                    Foreground = Brushes.White,
                    FontWeight = FontWeights.Medium
                }
            };
            Grid.SetColumn(statusBorder, 4);
            grid.Children.Add(statusBorder);

            // تاريخ الميلاد
            var birthText = new TextBlock
            {
                Text = student.BirthDate.ToString("yyyy-MM-dd"),
                VerticalAlignment = VerticalAlignment.Center,
                Foreground = new SolidColorBrush(Color.FromRgb(117, 117, 117))
            };
            Grid.SetColumn(birthText, 5);
            grid.Children.Add(birthText);

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
            //    Click += ViewStudentButton_Click
            };

            var editButton = new Button
            {
                Content = "✏️",
                ToolTip = "تعديل",
                Width = 30,
                Height = 30,
                Background = new SolidColorBrush(Color.FromRgb(232, 245, 233)),
                BorderBrush = new SolidColorBrush(Color.FromRgb(165, 214, 167)),
                Margin = new Thickness(0, 0, 5, 0),
                Tag = student.StudentID,
             //   Click += EditStudentButton_Click
            };

            var deleteButton = new Button
            {
                Content = "🗑️",
                ToolTip = "حذف",
                Width = 30,
                Height = 30,
                Background = new SolidColorBrush(Color.FromRgb(255, 235, 238)),
                BorderBrush = new SolidColorBrush(Color.FromRgb(239, 154, 154)),
                Tag = student.StudentID,
              //  Click += DeleteStudentButton_Click
            };

            actionsPanel.Children.Add(viewButton);
            actionsPanel.Children.Add(editButton);
            actionsPanel.Children.Add(deleteButton);

            Grid.SetColumn(actionsPanel, 6);
            grid.Children.Add(actionsPanel);

            border.Child = grid;
            StudentsListPanel.Children.Add(border);
        }

        private Brush GetStatusColor(string status)
        {
            switch (status)
            {
                case "مسجل":
                    return new SolidColorBrush(Color.FromRgb(76, 175, 80)); // أخضر
                case "منتظم":
                    return new SolidColorBrush(Color.FromRgb(33, 150, 243)); // أزرق
                case "منقطع":
                    return new SolidColorBrush(Color.FromRgb(255, 152, 0)); // برتقالي
                case "ملغى":
                    return new SolidColorBrush(Color.FromRgb(244, 67, 54)); // أحمر
                default:
                    return new SolidColorBrush(Color.FromRgb(158, 158, 158)); // رمادي
            }
        }

        private void UpdateStatistics()
        {
            TotalStudentsText.Text = allStudents.Count.ToString();

            int registered = allStudents.FindAll(s => s.RegistrationStatus == "مسجل").Count;
            int active = allStudents.FindAll(s => s.RegistrationStatus == "منتظم").Count;
            int inactive = allStudents.FindAll(s => s.RegistrationStatus == "منقطع").Count;

            RegisteredStudentsText.Text = registered.ToString();
            ActiveStudentsText.Text = active.ToString();
            InactiveStudentsText.Text = inactive.ToString();
        }

        private void SearchTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            FilterStudents();
        }

        private void StatusFilterComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            FilterStudents();
        }

        private void FilterStudents()
        {
            string searchText = SearchTextBox.Text.ToLower();
            string selectedStatus = (StatusFilterComboBox.SelectedItem as ComboBoxItem)?.Content.ToString();

            filteredStudents.Clear();
            StudentsListPanel.Children.Clear();

            foreach (var student in allStudents)
            {
                bool matchesSearch = string.IsNullOrEmpty(searchText) ||
                                   student.StudentName.ToLower().Contains(searchText) ||
                                   student.NationalNumber.Contains(searchText);

                bool matchesStatus = selectedStatus == "جميع الحالات" ||
                                   student.RegistrationStatus == selectedStatus;

                if (matchesSearch && matchesStatus)
                {
                    filteredStudents.Add(student);
                    AddStudentToPanel(student);
                }
            }
        }

        private void RefreshButton_Click(object sender, RoutedEventArgs e)
        {
            LoadStudents();
            SearchTextBox.Text = "";
            StatusFilterComboBox.SelectedIndex = 0;
        }

        private void ExportButton_Click(object sender, RoutedEventArgs e)
        {
            // كود التصدير إلى Excel سيتم إضافته لاحقاً
            MessageBox.Show("ميزة التصدير إلى Excel قيد التطوير", "قيد التطوير",
                MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void ViewStudentButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.Tag is Guid studentId)
            {
                // عرض تفاصيل الطالب
                var student = allStudents.Find(s => s.StudentID == studentId);
                if (student != null)
                {
                    string message = $"اسم الطالب: {student.StudentName}\n" +
                                   $"الرقم الوطني: {student.NationalNumber}\n" +
                                   $"العمر: {student.Age} سنة\n" +
                                   $"حالة التسجيل: {student.RegistrationStatus}\n" +
                                   $"رقم الهاتف: {student.PhoneNumber}\n" +
                                   $"البريد الإلكتروني: {student.Email}\n" +
                                   $"تاريخ الميلاد: {student.BirthDate:yyyy-MM-dd}";

                    MessageBox.Show(message, "تفاصيل الطالب",
                        MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
        }

        private void EditStudentButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.Tag is Guid studentId)
            {
                MessageBox.Show("ميزة التعديل قيد التطوير", "قيد التطوير",
                    MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private void DeleteStudentButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.Tag is Guid studentId)
            {
                var result = MessageBox.Show(
                    "هل أنت متأكد من حذف هذا الطالب؟\nهذا الإجراء لا يمكن التراجع عنه.",
                    "تأكيد الحذف",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Warning);

                if (result == MessageBoxResult.Yes)
                {
                    try
                    {
                        string query = "DELETE FROM Students WHERE StudentID = @StudentID";

                        if (CurrentConnection.OpenConntion())
                        {
                            using (SqlCommand cmd = new SqlCommand(query, CurrentConnection.CuCon))
                            {
                                cmd.Parameters.AddWithValue("@StudentID", studentId);
                                int rowsAffected = cmd.ExecuteNonQuery();

                                if (rowsAffected > 0)
                                {
                                    MessageBox.Show("تم حذف الطالب بنجاح", "نجاح",
                                        MessageBoxButton.OK, MessageBoxImage.Information);
                                    LoadStudents();
                                }
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"حدث خطأ أثناء الحذف: {ex.Message}", "خطأ",
                            MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                    finally
                    {
                        CurrentConnection.CloseConntion();
                    }
                }
            }
        }
    }

    public class StudentData
    {
        public int Index { get; set; }
        public Guid StudentID { get; set; }
        public string StudentName { get; set; }
        public string NationalNumber { get; set; }
        public int Age { get; set; }
        public string RegistrationStatus { get; set; }
        public string PhoneNumber { get; set; }
        public string Email { get; set; }
        public DateTime BirthDate { get; set; }
    }
}