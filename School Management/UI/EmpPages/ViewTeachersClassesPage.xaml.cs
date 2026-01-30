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
    public partial class ViewTeachersClassesPage : UserControl
    {
        private List<TeacherClassData> allRecords = new List<TeacherClassData>();
        private List<TeacherClassData> filteredRecords = new List<TeacherClassData>();

        public ViewTeachersClassesPage()
        {
            InitializeComponent();
            LoadTeachersClasses();
        }

        private void LoadTeachersClasses()
        {
            try
            {
                allRecords.Clear();
                TeachersClassesListPanel.Children.Clear();
                TeacherFilterComboBox.Items.Clear();
                TeacherFilterComboBox.Items.Add(new ComboBoxItem { Content = "جميع المدرسين" });

                // الاستعلام المطلوب مع JOIN
                string query = @"
                    SELECT TA.TeacherName, CG.fullClassName 
                    FROM Teacher_Class_Group AS TCG
                    INNER JOIN Teachers AS TA ON TCG.TeacherID = TA.TeacherID
                    INNER JOIN Class_Group AS CG ON TCG.ClassGroupID = CG.ClassGroupID
                    ORDER BY TA.TeacherName, CG.fullClassName";

                if (CurrentConnection.OpenConntion())
                {
                    using (SqlCommand cmd = new SqlCommand(query, CurrentConnection.CuCon))
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        int index = 1;
                        HashSet<string> uniqueTeachers = new HashSet<string>();

                        while (reader.Read())
                        {
                            var record = new TeacherClassData
                            {
                                Index = index,
                                TeacherName = reader.GetString(0),
                                FullClassName = reader.GetString(1)
                            };

                            allRecords.Add(record);
                            AddRecordToPanel(record);

                            // جمع أسماء المدرسين الفريدة
                            if (!uniqueTeachers.Contains(record.TeacherName))
                            {
                                uniqueTeachers.Add(record.TeacherName);
                            }

                            index++;
                        }

                        // إضافة أسماء المدرسين إلى ComboBox
                        foreach (var teacher in uniqueTeachers)
                        {
                            TeacherFilterComboBox.Items.Add(new ComboBoxItem { Content = teacher });
                        }
                    }
                }

                UpdateStatistics();
                filteredRecords = new List<TeacherClassData>(allRecords);
                TeacherFilterComboBox.SelectedIndex = 0;
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

        private void AddRecordToPanel(TeacherClassData record)
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

            // اسم المعلم
            var teacherText = new TextBlock
            {
                Text = record.TeacherName,
                VerticalAlignment = VerticalAlignment.Center,
                FontWeight = FontWeights.Medium,
                Foreground = new SolidColorBrush(Color.FromRgb(33, 33, 33))
            };
            Grid.SetColumn(teacherText, 1);
            grid.Children.Add(teacherText);

            // اسم الصف الكامل
            var classText = new TextBlock
            {
                Text = record.FullClassName,
                VerticalAlignment = VerticalAlignment.Center,
                Foreground = new SolidColorBrush(Color.FromRgb(117, 117, 117))
            };
            Grid.SetColumn(classText, 2);
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
                ToolTip = "حذف الربط",
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
            TeachersClassesListPanel.Children.Add(border);

            // ربط الأحداث
            viewButton.Click += ViewRecordButton_Click;
            deleteButton.Click += DeleteRecordButton_Click;
        }

        private void UpdateStatistics()
        {
            TotalRecordsText.Text = allRecords.Count.ToString();

            if (allRecords.Count > 0)
            {
                // حساب عدد المدرسين الفريدين
                HashSet<string> uniqueTeachers = new HashSet<string>();
                HashSet<string> uniqueClasses = new HashSet<string>();

                foreach (var record in allRecords)
                {
                    uniqueTeachers.Add(record.TeacherName);
                    uniqueClasses.Add(record.FullClassName);
                }

                TeachersCountText.Text = uniqueTeachers.Count.ToString();
                ClassesCountText.Text = uniqueClasses.Count.ToString();

                // حساب متوسط الصفوف لكل معلم
                double avgClassesPerTeacher = (double)allRecords.Count / uniqueTeachers.Count;
                AvgClassesPerTeacherText.Text = avgClassesPerTeacher.ToString("F1");
            }
            else
            {
                TeachersCountText.Text = "0";
                ClassesCountText.Text = "0";
                AvgClassesPerTeacherText.Text = "0";
            }
        }

        private void SearchTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            FilterRecords();
        }

        private void TeacherFilterComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            FilterRecords();
        }

        private void FilterRecords()
        {
            string searchText = SearchTextBox.Text.ToLower();
            string selectedTeacher = (TeacherFilterComboBox.SelectedItem as ComboBoxItem)?.Content.ToString();

            filteredRecords.Clear();
            TeachersClassesListPanel.Children.Clear();

            foreach (var record in allRecords)
            {
                bool matchesSearch = string.IsNullOrEmpty(searchText) ||
                                   record.TeacherName.ToLower().Contains(searchText) ||
                                   record.FullClassName.ToLower().Contains(searchText);

                bool matchesTeacher = selectedTeacher == "جميع المدرسين" ||
                                    record.TeacherName == selectedTeacher;

                if (matchesSearch && matchesTeacher)
                {
                    filteredRecords.Add(record);
                    AddRecordToPanel(record);
                }
            }
        }

        private void RefreshButton_Click(object sender, RoutedEventArgs e)
        {
            LoadTeachersClasses();
            SearchTextBox.Text = "";
            TeacherFilterComboBox.SelectedIndex = 0;
        }

        private void ViewRecordButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.Tag is TeacherClassData record)
            {
                string message = $"اسم المعلم: {record.TeacherName}\n" +
                               $"اسم الصف: {record.FullClassName}\n\n" +
                               "معلومات إضافية:\n" +
                               $"• يمكن للمعلم تدريس أكثر من صف\n" +
                               $"• يمكن للصف أن يكون له أكثر من معلم";

                MessageBox.Show(message, "تفاصيل الربط بين المعلم والصف",
                    MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private void DeleteRecordButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.Tag is TeacherClassData record)
            {
                var result = MessageBox.Show(
                    $"هل أنت متأكد من حذف الربط بين:\n" +
                    $"المعلم: {record.TeacherName}\n" +
                    $"والصف: {record.FullClassName}؟\n\n" +
                    $"هذا الإجراء سيمنع هذا المعلم من تدريس هذا الصف.",
                    "تأكيد الحذف",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Warning);

                if (result == MessageBoxResult.Yes)
                {
                    try
                    {
                        // حذف الربط من جدول Teacher_Class_Group
                        // نستخدم subquery للحصول على TeacherID و ClassGroupID
                        string query = @"
                            DELETE FROM Teacher_Class_Group 
                            WHERE TeacherID IN (SELECT TeacherID FROM Teachers WHERE TeacherName = @TeacherName)
                            AND ClassGroupID IN (SELECT ClassGroupID FROM Class_Group WHERE fullClassName = @FullClassName)";

                        if (CurrentConnection.OpenConntion())
                        {
                            using (SqlCommand cmd = new SqlCommand(query, CurrentConnection.CuCon))
                            {
                                cmd.Parameters.AddWithValue("@TeacherName", record.TeacherName);
                                cmd.Parameters.AddWithValue("@FullClassName", record.FullClassName);
                                int rowsAffected = cmd.ExecuteNonQuery();

                                if (rowsAffected > 0)
                                {
                                    MessageBox.Show("تم حذف الربط بنجاح", "نجاح",
                                        MessageBoxButton.OK, MessageBoxImage.Information);
                                }
                                else
                                {
                                    MessageBox.Show("لم يتم العثور على الربط المطلوب", "تنبيه",
                                        MessageBoxButton.OK, MessageBoxImage.Warning);
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
                        LoadTeachersClasses();
                    }
                }
            }
        }
    }

    public class TeacherClassData
    {
        public int Index { get; set; }
        public string TeacherName { get; set; }
        public string FullClassName { get; set; }
    }
}