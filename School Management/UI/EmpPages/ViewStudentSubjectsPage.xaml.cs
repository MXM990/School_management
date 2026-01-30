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
    public partial class ViewStudentSubjectsPage : UserControl
    {
        private List<StudentSubjectData> allRecords = new List<StudentSubjectData>();
        private List<StudentSubjectData> filteredRecords = new List<StudentSubjectData>();
        private string currentSearchStudent = "";

        public ViewStudentSubjectsPage()
        {
            InitializeComponent();
            LoadAllData();
            LoadStudentsList();
        }

        private void LoadAllData()
        {
            try
            {
                allRecords.Clear();
                SubjectsListPanel.Children.Clear();

                // الاستعلام الأساسي لجميع البيانات
                string query = @"
                    SELECT ST.StudentName, SUB.SubjectName 
                    FROM StudentSubjectsRegster AS SSR
                    INNER JOIN Students AS ST ON SSR.StudentID = ST.StudentID
                    INNER JOIN Subjects AS SUB ON SUB.SubjectID = SSR.SubjectID
                    ORDER BY ST.StudentName, SUB.SubjectName";

                if (CurrentConnection.OpenConntion())
                {
                    using (SqlCommand cmd = new SqlCommand(query, CurrentConnection.CuCon))
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        int index = 1;
                        while (reader.Read())
                        {
                            var record = new StudentSubjectData
                            {
                                Index = index,
                                StudentName = reader.GetString(0),
                                SubjectName = reader.GetString(1)
                            };

                            allRecords.Add(record);
                            index++;
                        }
                    }
                }

                // عرض جميع البيانات
                DisplayRecords(allRecords);
                UpdateStatistics();
                filteredRecords = new List<StudentSubjectData>(allRecords);
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

        private void LoadStudentsList()
        {
            try
            {
                StudentsComboBox.Items.Clear();
                StudentsComboBox.Items.Add(new ComboBoxItem { Content = "اختر طالب..." });

                string query = "SELECT DISTINCT StudentName FROM Students ORDER BY StudentName";

                if (CurrentConnection.OpenConntion())
                {
                    using (SqlCommand cmd = new SqlCommand(query, CurrentConnection.CuCon))
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            string studentName = reader.GetString(0);
                            StudentsComboBox.Items.Add(new ComboBoxItem { Content = studentName });
                        }
                    }
                }

                StudentsComboBox.SelectedIndex = 0;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"حدث خطأ أثناء تحميل قائمة الطلاب: {ex.Message}",
                    "خطأ", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                CurrentConnection.CloseConntion();
            }
        }

        private void SearchStudentSubjects(string studentName)
        {
            try
            {
                filteredRecords.Clear();
                SubjectsListPanel.Children.Clear();

                if (string.IsNullOrWhiteSpace(studentName))
                {
                    DisplayRecords(allRecords);
                    UpdateStatistics();
                    StudentInfoBorder.Visibility = Visibility.Collapsed;
                    return;
                }

                // البحث عن مواد الطالب المحدد
                string query = @"
                    SELECT ST.StudentName, SUB.SubjectName 
                    FROM StudentSubjectsRegster AS SSR
                    INNER JOIN Students AS ST ON SSR.StudentID = ST.StudentID
                    INNER JOIN Subjects AS SUB ON SUB.SubjectID = SSR.SubjectID
                    WHERE ST.StudentName LIKE @StudentName
                    ORDER BY SUB.SubjectName";

                if (CurrentConnection.OpenConntion())
                {
                    using (SqlCommand cmd = new SqlCommand(query, CurrentConnection.CuCon))
                    {
                        cmd.Parameters.AddWithValue("@StudentName", "%" + studentName + "%");

                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            int index = 1;
                            int subjectCount = 0;
                            string actualStudentName = "";

                            while (reader.Read())
                            {
                                var record = new StudentSubjectData
                                {
                                    Index = index,
                                    StudentName = reader.GetString(0),
                                    SubjectName = reader.GetString(1)
                                };

                                filteredRecords.Add(record);
                                index++;
                                subjectCount++;

                                if (string.IsNullOrEmpty(actualStudentName))
                                    actualStudentName = record.StudentName;
                            }

                            // عرض النتائج
                            DisplayRecords(filteredRecords);

                            // عرض معلومات الطالب
                            if (subjectCount > 0)
                            {
                                StudentInfoBorder.Visibility = Visibility.Visible;
                                SelectedStudentNameText.Text = actualStudentName;
                                SubjectsCountText.Text = $"{subjectCount} مادة مسجلة";
                            }
                            else
                            {
                                StudentInfoBorder.Visibility = Visibility.Collapsed;
                                MessageBox.Show($"لم يتم العثور على مواد للطالب: {studentName}",
                                    "لا توجد بيانات", MessageBoxButton.OK, MessageBoxImage.Information);
                            }
                        }
                    }
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

        private void DisplayRecords(List<StudentSubjectData> records)
        {
            SubjectsListPanel.Children.Clear();

            foreach (var record in records)
            {
                AddRecordToPanel(record);
            }
        }

        private void AddRecordToPanel(StudentSubjectData record)
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

            // اسم المادة
            var subjectBorder = new Border
            {
                CornerRadius = new CornerRadius(5),
                Padding = new Thickness(10, 5, 10, 5),
                VerticalAlignment = VerticalAlignment.Center,
                Background = new SolidColorBrush(Color.FromRgb(237, 231, 246)),
                BorderBrush = new SolidColorBrush(Color.FromRgb(103, 58, 183)),
                BorderThickness = new Thickness(1),
                Child = new TextBlock
                {
                    Text = record.SubjectName,
                    HorizontalAlignment = HorizontalAlignment.Center,
                    VerticalAlignment = VerticalAlignment.Center,
                    Foreground = new SolidColorBrush(Color.FromRgb(103, 58, 183)),
                    FontWeight = FontWeights.Medium
                }
            };
            Grid.SetColumn(subjectBorder, 2);
            grid.Children.Add(subjectBorder);

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
                ToolTip = "إلغاء تسجيل المادة",
                Width = 30,
                Height = 30,
                Background = new SolidColorBrush(Color.FromRgb(255, 235, 238)),
                BorderBrush = new SolidColorBrush(Color.FromRgb(239, 154, 154)),
                Tag = record // تخزين كامل السجل
            };

            actionsPanel.Children.Add(viewButton);
            actionsPanel.Children.Add(deleteButton);

            Grid.SetColumn(actionsPanel, 3);
            grid.Children.Add(actionsPanel);

            border.Child = grid;
            SubjectsListPanel.Children.Add(border);

            // ربط الأحداث
            viewButton.Click += ViewRecordButton_Click;
            deleteButton.Click += DeleteRecordButton_Click;
        }

        private void UpdateStatistics()
        {
            TotalRegistrationsText.Text = allRecords.Count.ToString();

            if (allRecords.Count > 0)
            {
                // حساب إحصائيات
                HashSet<string> uniqueStudents = new HashSet<string>();
                HashSet<string> uniqueSubjects = new HashSet<string>();

                // تجميع البيانات لحساب متوسط المواد لكل طالب
                Dictionary<string, int> subjectsPerStudent = new Dictionary<string, int>();

                foreach (var record in allRecords)
                {
                    uniqueStudents.Add(record.StudentName);
                    uniqueSubjects.Add(record.SubjectName);

                    // حساب عدد المواد لكل طالب
                    if (subjectsPerStudent.ContainsKey(record.StudentName))
                        subjectsPerStudent[record.StudentName]++;
                    else
                        subjectsPerStudent[record.StudentName] = 1;
                }

                StudentsCountText.Text = uniqueStudents.Count.ToString();
                SubjectsCountText2.Text = uniqueSubjects.Count.ToString();

                // حساب متوسط المواد لكل طالب
                if (uniqueStudents.Count > 0)
                {
                    double totalSubjects = 0;
                    foreach (var count in subjectsPerStudent.Values)
                    {
                        totalSubjects += count;
                    }
                    double avgSubjectsPerStudent = totalSubjects / uniqueStudents.Count;
                    AvgSubjectsPerStudentText.Text = avgSubjectsPerStudent.ToString("F1");
                }
                else
                {
                    AvgSubjectsPerStudentText.Text = "0";
                }
            }
            else
            {
                StudentsCountText.Text = "0";
                SubjectsCountText2.Text = "0";
                AvgSubjectsPerStudentText.Text = "0";
            }
        }

        private void StudentNameTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            // يمكن إضافة بحث تلقائي أثناء الكتابة
            // SearchStudentSubjects(StudentNameTextBox.Text);
        }

        private void StudentsComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (StudentsComboBox.SelectedIndex > 0)
            {
                string selectedStudent = (StudentsComboBox.SelectedItem as ComboBoxItem)?.Content.ToString();
                if (!string.IsNullOrEmpty(selectedStudent))
                {
                    StudentNameTextBox.Text = selectedStudent;
                    SearchStudentSubjects(selectedStudent);
                }
            }
        }

        private void SearchButton_Click(object sender, RoutedEventArgs e)
        {
            string studentName = StudentNameTextBox.Text.Trim();
            if (!string.IsNullOrEmpty(studentName))
            {
                currentSearchStudent = studentName;
                SearchStudentSubjects(studentName);
            }
            else
            {
                MessageBox.Show("يرجى إدخال اسم الطالب للبحث", "تنبيه",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void RefreshButton_Click(object sender, RoutedEventArgs e)
        {
            LoadAllData();
            LoadStudentsList();
            StudentNameTextBox.Text = "";
            StudentInfoBorder.Visibility = Visibility.Collapsed;
            currentSearchStudent = "";
        }

        private void ShowAllButton_Click(object sender, RoutedEventArgs e)
        {
            DisplayRecords(allRecords);
            UpdateStatistics();
            StudentInfoBorder.Visibility = Visibility.Collapsed;
            currentSearchStudent = "";
            StudentNameTextBox.Text = "";
            StudentsComboBox.SelectedIndex = 0;
        }

        private void ClearSelectionButton_Click(object sender, RoutedEventArgs e)
        {
            StudentInfoBorder.Visibility = Visibility.Collapsed;
            DisplayRecords(allRecords);
            UpdateStatistics();
            currentSearchStudent = "";
            StudentNameTextBox.Text = "";
            StudentsComboBox.SelectedIndex = 0;
        }

        private void ViewRecordButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.Tag is StudentSubjectData record)
            {
                string message = $"اسم الطالب: {record.StudentName}\n" +
                               $"المادة المسجلة: {record.SubjectName}\n\n" +
                               "ملاحظات:\n" +
                               "• يمكن للطالب التسجيل في أكثر من مادة\n" +
                               "• يمكن للمادة أن يكون لها أكثر من طالب مسجل";

                MessageBox.Show(message, "تفاصيل تسجيل المادة",
                    MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private void DeleteRecordButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.Tag is StudentSubjectData record)
            {
                var result = MessageBox.Show(
                    $"هل أنت متأكد من إلغاء تسجيل الطالب:\n{record.StudentName}\n" +
                    $"من المادة:\n{record.SubjectName}؟\n\n" +
                    $"هذا الإجراء سيلغي تسجيل الطالب في هذه المادة فقط.",
                    "تأكيد الإلغاء",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Warning);

                if (result == MessageBoxResult.Yes)
                {
                    try
                    {
                        // حذف التسجيل من جدول StudentSubjectsRegster
                        // نستخدم subquery للحصول على StudentID و SubjectID
                        string query = @"
                            DELETE FROM StudentSubjectsRegster 
                            WHERE StudentID IN (SELECT StudentID FROM Students WHERE StudentName = @StudentName)
                            AND SubjectID IN (SELECT SubjectID FROM Subjects WHERE SubjectName = @SubjectName)";

                        if (CurrentConnection.OpenConntion())
                        {
                            using (SqlCommand cmd = new SqlCommand(query, CurrentConnection.CuCon))
                            {
                                cmd.Parameters.AddWithValue("@StudentName", record.StudentName);
                                cmd.Parameters.AddWithValue("@SubjectName", record.SubjectName);
                                int rowsAffected = cmd.ExecuteNonQuery();

                                if (rowsAffected > 0)
                                {
                                    MessageBox.Show("تم إلغاء تسجيل المادة بنجاح", "نجاح",
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
                        MessageBox.Show($"حدث خطأ أثناء الإلغاء: {ex.Message}", "خطأ",
                            MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                    finally
                    {
                        CurrentConnection.CloseConntion();

                        // إعادة تحميل البيانات بناءً على حالة البحث الحالية
                        if (!string.IsNullOrEmpty(currentSearchStudent))
                            SearchStudentSubjects(currentSearchStudent);
                        else
                            LoadAllData();
                    }
                }
            }
        }
    }

    public class StudentSubjectData
    {
        public int Index { get; set; }
        public string StudentName { get; set; }
        public string SubjectName { get; set; }
    }
}