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
    public partial class ViewClassesReportsPage : UserControl
    {
        private List<ClassDataView> allClasses = new List<ClassDataView>();
        private List<ClassDataView> filteredClasses = new List<ClassDataView>();

        public ViewClassesReportsPage()
        {
            InitializeComponent();
            LoadClasses();
        }

        private void LoadClasses()
        {
            try
            {
                allClasses.Clear();
                ClassesListPanel.Children.Clear();

                string query = @"
                    SELECT ClassID, EducationLevel, AdditionalInfo, CreatedDate
                    FROM Classes 
                    ORDER BY CreatedDate DESC";

                if (CurrentConnection.OpenConntion())
                {
                    using (SqlCommand cmd = new SqlCommand(query, CurrentConnection.CuCon))
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        int index = 1;
                        while (reader.Read())
                        {
                            var classItem = new ClassDataView
                            {
                                Index = index,
                                ClassID = reader.GetGuid(0),
                                EducationLevel = reader.GetString(1),
                                AdditionalInfo = reader.IsDBNull(2) ? "" : reader.GetString(2),
                                CreatedDate = reader.GetDateTime(3)
                            };

                            allClasses.Add(classItem);
                            AddClassToPanel(classItem);
                            index++;
                        }
                    }
                }

                UpdateStatistics();
                filteredClasses = new List<ClassDataView>(allClasses);
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

        private void AddClassToPanel(ClassDataView classItem)
        {
            var border = new Border
            {
                Background = classItem.Index % 2 == 0 ? Brushes.White : Brushes.WhiteSmoke,
                BorderBrush = new SolidColorBrush(Color.FromRgb(229, 229, 229)),
                BorderThickness = new Thickness(0, 0, 0, 1),
                Padding = new Thickness(15, 10, 15, 10)
            };

            var grid = new Grid();
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(50) });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(200) });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(150) });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(150) });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(100) });

            var indexText = new TextBlock
            {
                Text = classItem.Index.ToString(),
                VerticalAlignment = VerticalAlignment.Center,
                Foreground = new SolidColorBrush(Color.FromRgb(117, 117, 117))
            };
            Grid.SetColumn(indexText, 0);
            grid.Children.Add(indexText);

            var levelText = new TextBlock
            {
                Text = classItem.EducationLevel,
                VerticalAlignment = VerticalAlignment.Center,
                FontWeight = FontWeights.Medium,
            };
            Grid.SetColumn(levelText, 1);
            grid.Children.Add(levelText);

            var infoText = new TextBlock
            {
                Text = string.IsNullOrEmpty(classItem.AdditionalInfo) ? "لا توجد معلومات إضافية" : classItem.AdditionalInfo,
                VerticalAlignment = VerticalAlignment.Center,
                Foreground = new SolidColorBrush(Color.FromRgb(117, 117, 117)),
                TextWrapping = TextWrapping.Wrap,
                MaxHeight = 40,
                TextTrimming = TextTrimming.CharacterEllipsis
            };
            Grid.SetColumn(infoText, 2);
            grid.Children.Add(infoText);

            // تاريخ الإنشاء
            var dateText = new TextBlock
            {
                Text = classItem.CreatedDate.ToString("yyyy-MM-dd HH:mm"),
                VerticalAlignment = VerticalAlignment.Center,
                Foreground = new SolidColorBrush(Color.FromRgb(117, 117, 117))
            };
            Grid.SetColumn(dateText, 3);
            grid.Children.Add(dateText);

            // العمر (أيام)
            var ageInDays = (DateTime.Now - classItem.CreatedDate).Days;
            var ageText = new TextBlock
            {
                Text = $"{ageInDays} يوم",
                VerticalAlignment = VerticalAlignment.Center,
                Foreground = new SolidColorBrush(Color.FromRgb(117, 117, 117))
            };
            Grid.SetColumn(ageText, 4);
            grid.Children.Add(ageText);

            // أزرار الإجراءات
            var actionsPanel = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center
            };

          
            var deleteButton = new Button
            {
                Content = "🗑️",
                ToolTip = "حذف",
                Width = 30,
                Height = 30,
                Background = new SolidColorBrush(Color.FromRgb(255, 235, 238)),
                BorderBrush = new SolidColorBrush(Color.FromRgb(239, 154, 154)),
                Tag = classItem.ClassID
            };

        
            actionsPanel.Children.Add(deleteButton);

            Grid.SetColumn(actionsPanel, 5);
            grid.Children.Add(actionsPanel);

            border.Child = grid;
            ClassesListPanel.Children.Add(border);

           
            deleteButton.Click += DeleteClassButton_Click;
        }

        private void UpdateStatistics()
        {
            TotalClassesText.Text = allClasses.Count.ToString();
        }

        private void SearchTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            FilterClasses();
        }

        private void LevelFilterComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            FilterClasses();
        }

        private void FilterClasses()
        {
            string searchText = SearchTextBox.Text.ToLower();

            filteredClasses.Clear();
            ClassesListPanel.Children.Clear();

            foreach (var classItem in allClasses)
            {
                bool matchesSearch = string.IsNullOrEmpty(searchText) ||
                                   classItem.EducationLevel.ToLower().Contains(searchText) ||
                                   classItem.AdditionalInfo.ToLower().Contains(searchText);

                if (matchesSearch )
                {
                    filteredClasses.Add(classItem);
                    AddClassToPanel(classItem);
                }
            }
        }

        private void RefreshButton_Click(object sender, RoutedEventArgs e)
        {
            LoadClasses();
            SearchTextBox.Text = "";
        }

       

        private void ViewClassButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.Tag is Guid classId)
            {
                var classItem = allClasses.Find(c => c.ClassID == classId);
                if (classItem != null)
                {
                    string message = $"مستوى التعليم: {classItem.EducationLevel}\n" +
                                   $"المعلومات الإضافية: {classItem.AdditionalInfo}\n" +
                                   $"تاريخ الإنشاء: {classItem.CreatedDate:yyyy-MM-dd HH:mm}\n" +
                                   $"العمر: {(DateTime.Now - classItem.CreatedDate).Days} يوم";

                    MessageBox.Show(message, "تفاصيل الصف",
                        MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
        }

     

        private void DeleteClassButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.Tag is Guid classId)
            {
                var result = MessageBox.Show(
                    "هل أنت متأكد من حذف هذا الصف؟\nهذا الإجراء سيمسح جميع البيانات المرتبطة به.",
                    "تأكيد الحذف",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Warning);

                if (result == MessageBoxResult.Yes)
                {
                    try
                    {
                        string query = "DELETE FROM Classes WHERE ClassID = @ClassID";

                        if (CurrentConnection.OpenConntion())
                        {
                            using (SqlCommand cmd = new SqlCommand(query, CurrentConnection.CuCon))
                            {
                                cmd.Parameters.AddWithValue("@ClassID", classId);
                                int rowsAffected = cmd.ExecuteNonQuery();

                                if (rowsAffected > 0)
                                {
                                    MessageBox.Show("تم حذف الصف بنجاح", "نجاح",
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
                        LoadClasses();
                    }
                }
            }
        }
    }

    public class ClassDataView
    {
        public int Index { get; set; }
        public Guid ClassID { get; set; }
        public string EducationLevel { get; set; }
        public string AdditionalInfo { get; set; }
        public DateTime CreatedDate { get; set; }
    }
}