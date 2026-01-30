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
    public partial class ViewClassGroupsPage : UserControl
    {
        private List<ClassGroupData> allClassGroups = new List<ClassGroupData>();
        private List<ClassGroupData> filteredClassGroups = new List<ClassGroupData>();

        public ViewClassGroupsPage()
        {
            InitializeComponent();
            LoadClassGroups();
        }

        private void LoadClassGroups()
        {
            try
            {
                allClassGroups.Clear();
                ClassGroupsListPanel.Children.Clear();

                // الاستعلام المطلوب
                string query = @"
                    SELECT fullClassName, MaxStudents 
                    FROM Class_Group 
                    ORDER BY fullClassName";

                if (CurrentConnection.OpenConntion())
                {
                    using (SqlCommand cmd = new SqlCommand(query, CurrentConnection.CuCon))
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        int index = 1;
                        while (reader.Read())
                        {
                            var classGroup = new ClassGroupData
                            {
                                Index = index,
                                FullClassName = reader.GetString(0),
                                MaxStudents = reader.GetInt32(1)
                            };

                            allClassGroups.Add(classGroup);
                            AddClassGroupToPanel(classGroup);
                            index++;
                        }
                    }
                }

                UpdateStatistics();
                filteredClassGroups = new List<ClassGroupData>(allClassGroups);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"حدث خطأ أثناء تحميل الصفوف والشعب: {ex.Message}",
                    "خطأ", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                CurrentConnection.CloseConntion();
            }
        }

        private void AddClassGroupToPanel(ClassGroupData classGroup)
        {
            var border = new Border
            {
                Background = classGroup.Index % 2 == 0 ? Brushes.White : Brushes.WhiteSmoke,
                BorderBrush = new SolidColorBrush(Color.FromRgb(229, 229, 229)),
                BorderThickness = new Thickness(0, 0, 0, 1),
                Padding = new Thickness(15, 10, 15, 10)
            };

            var grid = new Grid();
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(50) });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(150) });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(120) });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(120) });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(100) });

            // الرقم التسلسلي
            var indexText = new TextBlock
            {
                Text = classGroup.Index.ToString(),
                VerticalAlignment = VerticalAlignment.Center,
                Foreground = new SolidColorBrush(Color.FromRgb(117, 117, 117))
            };
            Grid.SetColumn(indexText, 0);
            grid.Children.Add(indexText);

            // اسم الصف الكامل
            var nameText = new TextBlock
            {
                Text = classGroup.FullClassName,
                VerticalAlignment = VerticalAlignment.Center,
                FontWeight = FontWeights.Medium,
                Foreground = new SolidColorBrush(Color.FromRgb(33, 33, 33))
            };
            Grid.SetColumn(nameText, 1);
            grid.Children.Add(nameText);

            // السعة القصوى
            var capacityText = new TextBlock
            {
                Text = classGroup.MaxStudents.ToString(),
                VerticalAlignment = VerticalAlignment.Center,
                FontWeight = FontWeights.Bold,
                Foreground = new SolidColorBrush(Color.FromRgb(33, 150, 243)),
                HorizontalAlignment = HorizontalAlignment.Center
            };
            Grid.SetColumn(capacityText, 2);
            grid.Children.Add(capacityText);

            // حالة السعة
            string status = GetCapacityStatus(classGroup.MaxStudents);
            var statusBorder = new Border
            {
                CornerRadius = new CornerRadius(10),
                Padding = new Thickness(10, 5, 10, 5),
                Width = 100,
                VerticalAlignment = VerticalAlignment.Center,
                Background = GetStatusColor(status),
                Child = new TextBlock
                {
                    Text = status,
                    HorizontalAlignment = HorizontalAlignment.Center,
                    VerticalAlignment = VerticalAlignment.Center,
                    Foreground = Brushes.White,
                    FontWeight = FontWeights.Medium,
                    FontSize = 12
                }
            };
            Grid.SetColumn(statusBorder, 3);
            grid.Children.Add(statusBorder);

            // شريط التقدم (النسبة)
            var progressBar = new ProgressBar
            {
                Value = classGroup.MaxStudents,
                Maximum = 50, // أقصى سعة افتراضية للعرض
                Height = 20,
                VerticalAlignment = VerticalAlignment.Center,
                Foreground = GetCapacityColor(classGroup.MaxStudents)
            };
            Grid.SetColumn(progressBar, 4);
            grid.Children.Add(progressBar);

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
                Tag = classGroup.FullClassName
            };

            var deleteButton = new Button
            {
                Content = "🗑️",
                ToolTip = "حذف",
                Width = 30,
                Height = 30,
                Background = new SolidColorBrush(Color.FromRgb(255, 235, 238)),
                BorderBrush = new SolidColorBrush(Color.FromRgb(239, 154, 154)),
                Tag = classGroup.FullClassName
            };

            actionsPanel.Children.Add(viewButton);
            actionsPanel.Children.Add(deleteButton);

            Grid.SetColumn(actionsPanel, 5);
            grid.Children.Add(actionsPanel);

            border.Child = grid;
            ClassGroupsListPanel.Children.Add(border);

            // ربط الأحداث
            viewButton.Click += ViewClassGroupButton_Click;
            deleteButton.Click += DeleteClassGroupButton_Click;
        }

        private string GetCapacityStatus(int maxStudents)
        {
            if (maxStudents < 20)
                return "صغيرة";
            else if (maxStudents <= 30)
                return "متوسطة";
            else
                return "كبيرة";
        }

        private Brush GetStatusColor(string status)
        {
            switch (status)
            {
                case "صغيرة":
                    return new SolidColorBrush(Color.FromRgb(76, 175, 80)); // أخضر
                case "متوسطة":
                    return new SolidColorBrush(Color.FromRgb(33, 150, 243)); // أزرق
                case "كبيرة":
                    return new SolidColorBrush(Color.FromRgb(255, 152, 0)); // برتقالي
                default:
                    return new SolidColorBrush(Color.FromRgb(158, 158, 158)); // رمادي
            }
        }

        private Brush GetCapacityColor(int maxStudents)
        {
            if (maxStudents < 20)
                return new SolidColorBrush(Color.FromRgb(76, 175, 80)); // أخضر
            else if (maxStudents <= 30)
                return new SolidColorBrush(Color.FromRgb(33, 150, 243)); // أزرق
            else
                return new SolidColorBrush(Color.FromRgb(255, 152, 0)); // برتقالي
        }

        private void UpdateStatistics()
        {
            TotalClassGroupsText.Text = allClassGroups.Count.ToString();

            int totalCapacity = 0;
            int maxCapacity = 0;

            foreach (var group in allClassGroups)
            {
                totalCapacity += group.MaxStudents;
                if (group.MaxStudents > maxCapacity)
                    maxCapacity = group.MaxStudents;
            }

            TotalCapacityText.Text = totalCapacity.ToString();
            MaxCapacityText.Text = maxCapacity.ToString();

            if (allClassGroups.Count > 0)
            {
                double average = (double)totalCapacity / allClassGroups.Count;
                AverageCapacityText.Text = average.ToString("F1");
            }
            else
            {
                AverageCapacityText.Text = "0";
            }
        }

        private void SearchTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            FilterClassGroups();
        }

        private void CapacityFilterComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            FilterClassGroups();
        }

        private void FilterClassGroups()
        {
            string searchText = SearchTextBox.Text.ToLower();
            string selectedFilter = (CapacityFilterComboBox.SelectedItem as ComboBoxItem)?.Content.ToString();

            filteredClassGroups.Clear();
            ClassGroupsListPanel.Children.Clear();

            foreach (var classGroup in allClassGroups)
            {
                bool matchesSearch = string.IsNullOrEmpty(searchText) ||
                                   classGroup.FullClassName.ToLower().Contains(searchText);

                bool matchesCapacity = selectedFilter == "جميع السعات" ||
                    (selectedFilter == "صغيرة (أقل من 20)" && classGroup.MaxStudents < 20) ||
                    (selectedFilter == "متوسطة (20-30)" && classGroup.MaxStudents >= 20 && classGroup.MaxStudents <= 30) ||
                    (selectedFilter == "كبيرة (أكثر من 30)" && classGroup.MaxStudents > 30);

                if (matchesSearch && matchesCapacity)
                {
                    filteredClassGroups.Add(classGroup);
                    AddClassGroupToPanel(classGroup);
                }
            }
        }

        private void RefreshButton_Click(object sender, RoutedEventArgs e)
        {
            LoadClassGroups();
            SearchTextBox.Text = "";
            CapacityFilterComboBox.SelectedIndex = 0;
        }

        private void ViewClassGroupButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.Tag is string className)
            {
                var classGroup = allClassGroups.Find(c => c.FullClassName == className);
                if (classGroup != null)
                {
                    string message = $"اسم الصف الكامل: {classGroup.FullClassName}\n" +
                                   $"السعة القصوى: {classGroup.MaxStudents} طالب\n" +
                                   $"الحالة: {GetCapacityStatus(classGroup.MaxStudents)}";

                    MessageBox.Show(message, "تفاصيل الصف والشعبة",
                        MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
        }


        private void DeleteClassGroupButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.Tag is string className)
            {
                var result = MessageBox.Show(
                    $"هل أنت متأكد من حذف الصف والشعبة:\n{className}؟\nهذا الإجراء لا يمكن التراجع عنه.",
                    "تأكيد الحذف",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Warning);

                if (result == MessageBoxResult.Yes)
                {
                    try
                    {
                        string query = "DELETE FROM Class_Group WHERE fullClassName = @FullClassName";

                        if (CurrentConnection.OpenConntion())
                        {
                            using (SqlCommand cmd = new SqlCommand(query, CurrentConnection.CuCon))
                            {
                                cmd.Parameters.AddWithValue("@FullClassName", className);
                                int rowsAffected = cmd.ExecuteNonQuery();

                                if (rowsAffected > 0)
                                {
                                    MessageBox.Show("تم حذف الصف والشعبة بنجاح", "نجاح",
                                        MessageBoxButton.OK, MessageBoxImage.Information);
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
                        LoadClassGroups();
                    }
                }
            }
        }
    }

    public class ClassGroupData
    {
        public int Index { get; set; }
        public string FullClassName { get; set; }
        public int MaxStudents { get; set; }
    }
}