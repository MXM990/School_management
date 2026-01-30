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
    public partial class ViewStudentsInClassesPage : UserControl
    {
        private List<StudentClassData> allRecords = new List<StudentClassData>();
        private List<StudentClassData> filteredRecords = new List<StudentClassData>();

        public ViewStudentsInClassesPage()
        {
            InitializeComponent();
            LoadStudentsInClasses();
        }

        private void LoadStudentsInClasses()
        {
            try
            {
                allRecords.Clear();
                StudentsInClassesListPanel.Children.Clear();
                ClassFilterComboBox.Items.Clear();
                ClassFilterComboBox.Items.Add(new ComboBoxItem { Content = "جميع الصفوف" });

                // الاستعلام المطلوب مع JOIN
                string query = @"
                    SELECT SU.StudentName, SU.NationalNumber, SU.RegistrationStatus, CG.fullClassName 
                    FROM Student_Class_Group AS SCG
                    INNER JOIN Students AS SU ON SU.StudentID = SCG.StudentID
                    INNER JOIN Class_Group AS CG ON SCG.ClassGroupID = CG.ClassGroupID
                    ORDER BY CG.fullClassName, SU.StudentName";

                if (CurrentConnection.OpenConntion())
                {
                    using (SqlCommand cmd = new SqlCommand(query, CurrentConnection.CuCon))
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        int index = 1;
                        HashSet<string> uniqueClasses = new HashSet<string>();

                        while (reader.Read())
                        {
                            var record = new StudentClassData
                            {
                                Index = index,
                                StudentName = reader.GetString(0),
                                NationalNumber = reader.GetString(1),
                                RegistrationStatus = reader.GetString(2),
                                FullClassName = reader.GetString(3)
                            };

                            allRecords.Add(record);
                            AddRecordToPanel(record);

                            // جمع أسماء الصفوف الفريدة
                            if (!uniqueClasses.Contains(record.FullClassName))
                            {
                                uniqueClasses.Add(record.FullClassName);
                            }

                            index++;
                        }

                        // إضافة أسماء الصفوف إلى ComboBox
                        foreach (var className in uniqueClasses)
                        {
                            ClassFilterComboBox.Items.Add(new ComboBoxItem { Content = className });
                        }
                    }
                }

                UpdateStatistics();
                filteredRecords = new List<StudentClassData>(allRecords);
                ClassFilterComboBox.SelectedIndex = 0;
                StatusFilterComboBox.SelectedIndex = 0;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"حدث خطأ أثناء تحميل البيانات: {ex.Message}",
                    "خطأ", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                CurrentConnection.CloseConntion();
            }
        }

        private void AddRecordToPanel(StudentClassData record)
        {
            var border = new Border
            {
                Background = record.Index % 2 == 0 ? Brushes.White : Brushes.WhiteSmoke,
                BorderBrush = new SolidColorBrush(Color.FromRgb(229, 229, 229)),
                BorderThickness = new Thickness(0, 0, 0, 1),
                Padding = new Thickness(15, 10, 15, 10)
            };

            var grid = new Grid();
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(50) });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(150) });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(120) });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(80) });

            // الرقم التسلسلي
            var indexText = new TextBlock
            {
                Text = record.Index.ToString(),
                VerticalAlignment = VerticalAlignment.Center,
                Foreground = new SolidColorBrush(Color.FromRgb(117, 117, 117))
            };
            Grid.SetColumn(indexText, 0);
            grid.Children.Add(indexText);

            // اسم الطالب
            var studentText = new TextBlock
            {
                Text = record.StudentName,
                VerticalAlignment = VerticalAlignment.Center,
                FontWeight = FontWeights.Medium,
                Foreground = new SolidColorBrush(Color.FromRgb(33, 33, 33))
            };
            Grid.SetColumn(studentText, 1);
            grid.Children.Add(studentText);

            // الرقم الوطني
            var nationalText = new TextBlock
            {
                Text = record.NationalNumber,
                VerticalAlignment = VerticalAlignment.Center,
                Foreground = new SolidColorBrush(Color.FromRgb(117, 117, 117)),
                FontFamily = new FontFamily("Consolas")
            };
            Grid.SetColumn(nationalText, 2);
            grid.Children.Add(nationalText);

            // حالة التسجيل
            var statusBorder = new Border
            {
                CornerRadius = new CornerRadius(10),
                Padding = new Thickness(10, 5, 10, 5),
                Width = 100,
                VerticalAlignment = VerticalAlignment.Center,
                Background = GetStatusColor(record.RegistrationStatus),
                Child = new TextBlock
                {
                    Text = record.RegistrationStatus,
                    HorizontalAlignment = HorizontalAlignment.Center,
                    VerticalAlignment = VerticalAlignment.Center,
                    Foreground = Brushes.White,
                    FontWeight = FontWeights.Medium
                }
            };
            Grid.SetColumn(statusBorder, 3);
            grid.Children.Add(statusBorder);

            // اسم الصف الكامل
            var classText = new TextBlock
            {
                Text = record.FullClassName,
                VerticalAlignment = VerticalAlignment.Center,
                Foreground = new SolidColorBrush(Color.FromRgb(33, 150, 243)),
                FontWeight = FontWeights.Medium
            };
            Grid.SetColumn(classText, 4);
            grid.Children.Add(classText);

            // أزرار الإجراءات (عرض وحذف فقط)
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
                Tag = record // تخزين كامل السجل
            };

            var deleteButton = new Button
            {
                Content = "🗑️",
                ToolTip = "إزالة الطالب من الصف",
                Width = 30,
                Height = 30,
                Background = new SolidColorBrush(Color.FromRgb(255, 235, 238)),
                BorderBrush = new SolidColorBrush(Color.FromRgb(239, 154, 154)),
                Tag = record // تخزين كامل السجل
            };

            actionsPanel.Children.Add(viewButton);
            actionsPanel.Children.Add(deleteButton);

            Grid.SetColumn(actionsPanel, 5);
            grid.Children.Add(actionsPanel);

            border.Child = grid;
            StudentsInClassesListPanel.Children.Add(border);

            // ربط الأحداث
            viewButton.Click += ViewRecordButton_Click;
            deleteButton.Click += DeleteRecordButton_Click;
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
            TotalRegistrationsText.Text = allRecords.Count.ToString();

            if (allRecords.Count > 0)
            {
                // حساب إحصائيات
                HashSet<string> uniqueClasses = new HashSet<string>();
                HashSet<string> uniqueStudents = new HashSet<string>();
                int activeStudents = 0;

                // تجميع البيانات لحساب متوسط الطلاب في الصف
                Dictionary<string, int> studentsPerClass = new Dictionary<string, int>();

                foreach (var record in allRecords)
                {
                    uniqueClasses.Add(record.FullClassName);
                    uniqueStudents.Add(record.NationalNumber);

                    if (record.RegistrationStatus == "منتظم")
                        activeStudents++;

                    // حساب عدد الطلاب في كل صف
                    if (studentsPerClass.ContainsKey(record.FullClassName))
                        studentsPerClass[record.FullClassName]++;
                    else
                        studentsPerClass[record.FullClassName] = 1;
                }

                ClassesCountText.Text = uniqueClasses.Count.ToString();
                StudentsCountText.Text = uniqueStudents.Count.ToString();
                ActiveStudentsText.Text = activeStudents.ToString();

                // حساب متوسط الطلاب في الصف
                if (uniqueClasses.Count > 0)
                {
                    double totalStudentsInClasses = 0;
                    foreach (var count in studentsPerClass.Values)
                    {
                        totalStudentsInClasses += count;
                    }
                    double avgStudentsPerClass = totalStudentsInClasses / uniqueClasses.Count;
                    AvgStudentsPerClassText.Text = avgStudentsPerClass.ToString("F1");
                }
                else
                {
                    AvgStudentsPerClassText.Text = "0";
                }
            }
            else
            {
                ClassesCountText.Text = "0";
                StudentsCountText.Text = "0";
                ActiveStudentsText.Text = "0";
                AvgStudentsPerClassText.Text = "0";
            }
        }

        private void SearchTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            FilterRecords();
        }

        private void ClassFilterComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            FilterRecords();
        }

        private void StatusFilterComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            FilterRecords();
        }

        private void FilterRecords()
        {
            string searchText = SearchTextBox.Text.ToLower();
            string selectedClass = (ClassFilterComboBox.SelectedItem as ComboBoxItem)?.Content.ToString();
            string selectedStatus = (StatusFilterComboBox.SelectedItem as ComboBoxItem)?.Content.ToString();

            filteredRecords.Clear();
            StudentsInClassesListPanel.Children.Clear();

            foreach (var record in allRecords)
            {
                bool matchesSearch = string.IsNullOrEmpty(searchText) ||
                                   record.StudentName.ToLower().Contains(searchText) ||
                                   record.NationalNumber.Contains(searchText) ||
                                   record.FullClassName.ToLower().Contains(searchText);

                bool matchesClass = selectedClass == "جميع الصفوف" ||
                                  record.FullClassName == selectedClass;

                bool matchesStatus = selectedStatus == "جميع الحالات" ||
                                   record.RegistrationStatus == selectedStatus;

                if (matchesSearch && matchesClass && matchesStatus)
                {
                    filteredRecords.Add(record);
                    AddRecordToPanel(record);
                }
            }
        }

        private void RefreshButton_Click(object sender, RoutedEventArgs e)
        {
            LoadStudentsInClasses();
            SearchTextBox.Text = "";
            ClassFilterComboBox.SelectedIndex = 0;
            StatusFilterComboBox.SelectedIndex = 0;
        }

        private void ViewRecordButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.Tag is StudentClassData record)
            {
                string message = $"اسم الطالب: {record.StudentName}\n" +
                               $"الرقم الوطني: {record.NationalNumber}\n" +
                               $"حالة التسجيل: {record.RegistrationStatus}\n" +
                               $"الصف الدراسي: {record.FullClassName}\n\n" +
                               "ملاحظة:\n" +
                               "• يمكن للطالب أن يكون مسجل في أكثر من صف\n" +
                               "• حالة التسجيل تخص الطالب وليست مرتبطة بالصف";

                MessageBox.Show(message, "تفاصيل تسجيل الطالب في الصف",
                    MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private void DeleteRecordButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.Tag is StudentClassData record)
            {
                var result = MessageBox.Show(
                    $"هل أنت متأكد من إزالة الطالب:\n{record.StudentName}\n" +
                    $"من الصف:\n{record.FullClassName}؟\n\n" +
                    $"هذا الإجراء سيزيل تسجيل الطالب من هذا الصف فقط.",
                    "تأكيد الإزالة",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Warning);

                if (result == MessageBoxResult.Yes)
                {
                    try
                    {
                        // حذف الربط من جدول Student_Class_Group
                        // نستخدم subquery للحصول على StudentID و ClassGroupID
                        string query = @"
                            DELETE FROM Student_Class_Group 
                            WHERE StudentID IN (SELECT StudentID FROM Students WHERE NationalNumber = @NationalNumber)
                            AND ClassGroupID IN (SELECT ClassGroupID FROM Class_Group WHERE fullClassName = @FullClassName)";

                        if (CurrentConnection.OpenConntion())
                        {
                            using (SqlCommand cmd = new SqlCommand(query, CurrentConnection.CuCon))
                            {
                                cmd.Parameters.AddWithValue("@NationalNumber", record.NationalNumber);
                                cmd.Parameters.AddWithValue("@FullClassName", record.FullClassName);
                                int rowsAffected = cmd.ExecuteNonQuery();

                                if (rowsAffected > 0)
                                {
                                    MessageBox.Show("تم إزالة الطالب من الصف بنجاح", "نجاح",
                                        MessageBoxButton.OK, MessageBoxImage.Information);
                                }
                                else
                                {
                                    MessageBox.Show("لم يتم العثور على التسجيل المطلوب", "تنبيه",
                                        MessageBoxButton.OK, MessageBoxImage.Warning);
                                }
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"حدث خطأ أثناء الإزالة: {ex.Message}", "خطأ",
                            MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                    finally
                    {
                        CurrentConnection.CloseConntion();
                        LoadStudentsInClasses();
                    }
                }
            }
        }
    }

    public class StudentClassData
    {
        public int Index { get; set; }
        public string StudentName { get; set; }
        public string NationalNumber { get; set; }
        public string RegistrationStatus { get; set; }
        public string FullClassName { get; set; }
    }
}