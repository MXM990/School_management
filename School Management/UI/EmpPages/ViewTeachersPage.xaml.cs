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
    public partial class ViewTeachersPage : UserControl
    {
        private List<TeacherData> allTeachers = new List<TeacherData>();
        private List<TeacherData> filteredTeachers = new List<TeacherData>();

        public ViewTeachersPage()
        {
            InitializeComponent();
            LoadTeachers();
        }

        private void LoadTeachers()
        {
            try
            {
                allTeachers.Clear();
                TeachersListPanel.Children.Clear();
                SpecializationFilterComboBox.Items.Clear();
                SpecializationFilterComboBox.Items.Add(new ComboBoxItem { Content = "جميع التخصصات" });

                // الاستعلام المطلوب
                string query = @"
                    SELECT TeacherName, NationalNumber, Age, PhoneNumber, 
                           YearsOfExperience, Specialization, HireDate 
                    FROM Teachers 
                    ORDER BY TeacherName";

                if (CurrentConnection.OpenConntion())
                {
                    using (SqlCommand cmd = new SqlCommand(query, CurrentConnection.CuCon))
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        int index = 1;
                        HashSet<string> specializations = new HashSet<string>();

                        while (reader.Read())
                        {
                            var teacher = new TeacherData
                            {
                                Index = index,
                                TeacherName = reader.GetString(0),
                                NationalNumber = reader.GetString(1),
                                Age = reader.GetInt32(2),
                                PhoneNumber = reader.GetString(3),
                                YearsOfExperience = reader.GetInt32(4),
                                Specialization = reader.GetString(5),
                                HireDate = reader.GetDateTime(6)
                            };

                            allTeachers.Add(teacher);
                            AddTeacherToPanel(teacher);

                            // جمع التخصصات الفريدة
                            if (!string.IsNullOrEmpty(teacher.Specialization) && !specializations.Contains(teacher.Specialization))
                            {
                                specializations.Add(teacher.Specialization);
                            }

                            index++;
                        }

                        // إضافة التخصصات إلى ComboBox
                        foreach (var spec in specializations)
                        {
                            SpecializationFilterComboBox.Items.Add(new ComboBoxItem { Content = spec });
                        }
                    }
                }

                UpdateStatistics();
                filteredTeachers = new List<TeacherData>(allTeachers);
                SpecializationFilterComboBox.SelectedIndex = 0;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"حدث خطأ أثناء تحميل المعلمين: {ex.Message}",
                    "خطأ", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                CurrentConnection.CloseConntion();
            }
        }

        private void AddTeacherToPanel(TeacherData teacher)
        {
            var border = new Border
            {
                Background = teacher.Index % 2 == 0 ? Brushes.White : Brushes.WhiteSmoke,
                BorderBrush = new SolidColorBrush(Color.FromRgb(229, 229, 229)),
                BorderThickness = new Thickness(0, 0, 0, 1),
                Padding = new Thickness(15, 10, 15, 10)
            };

            var grid = new Grid();
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(50) });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(100) });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(90) });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(130) });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(150) });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(140) });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(150) });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(120) });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(100) });

        

            // الرقم التسلسلي
            var indexText = new TextBlock
            {
                Text = teacher.Index.ToString(),
                VerticalAlignment = VerticalAlignment.Center,
                Foreground = new SolidColorBrush(Color.FromRgb(117, 117, 117))
            };
            Grid.SetColumn(indexText, 0);
            grid.Children.Add(indexText);

            // اسم المعلم
            var nameText = new TextBlock
            {
                Text = teacher.TeacherName,
                VerticalAlignment = VerticalAlignment.Center,
                FontWeight = FontWeights.Medium,
                Foreground = new SolidColorBrush(Color.FromRgb(33, 33, 33))
            };
            Grid.SetColumn(nameText, 1);
            grid.Children.Add(nameText);

            // الرقم الوطني
            var nationalText = new TextBlock
            {
                Text = teacher.NationalNumber,
                VerticalAlignment = VerticalAlignment.Center,
                Foreground = new SolidColorBrush(Color.FromRgb(117, 117, 117)),
                FontFamily = new FontFamily("Consolas")
            };
            Grid.SetColumn(nationalText, 2);
            grid.Children.Add(nationalText);

            // العمر
            var ageText = new TextBlock
            {
                Text = $"{teacher.Age} سنة",
                VerticalAlignment = VerticalAlignment.Center,
                Foreground = new SolidColorBrush(Color.FromRgb(117, 117, 117)),
                HorizontalAlignment = HorizontalAlignment.Center
            };
            Grid.SetColumn(ageText, 3);
            grid.Children.Add(ageText);

            // رقم الهاتف
            var phoneText = new TextBlock
            {
                Text = teacher.PhoneNumber,
                VerticalAlignment = VerticalAlignment.Center,
                Foreground = new SolidColorBrush(Color.FromRgb(117, 117, 117))
            };
            Grid.SetColumn(phoneText, 4);
            grid.Children.Add(phoneText);

            // سنوات الخبرة
            var experienceBorder = new Border
            {
                Child = new TextBlock
                {
                    Text = $"{teacher.YearsOfExperience} سنة",
                    VerticalAlignment = VerticalAlignment.Center,
                    Foreground = Brushes.Aqua,
                }
            };
            Grid.SetColumn(experienceBorder, 5);
            grid.Children.Add(experienceBorder);

            // التخصص
            var specializationText = new TextBlock
            {
                Text = teacher.Specialization,
                VerticalAlignment = VerticalAlignment.Center,
                FontWeight = FontWeights.Medium,
                Foreground = new SolidColorBrush(Color.FromRgb(33, 150, 243))
            };
            Grid.SetColumn(specializationText, 6);
            grid.Children.Add(specializationText);

            // تاريخ التعيين
            var hireDateText = new TextBlock
            {
                Text = teacher.HireDate.ToString("yyyy-MM-dd"),
                VerticalAlignment = VerticalAlignment.Center,
                Foreground = new SolidColorBrush(Color.FromRgb(117, 117, 117))
            };
            Grid.SetColumn(hireDateText, 7);
            grid.Children.Add(hireDateText);

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
                Tag = teacher.NationalNumber // استخدام الرقم الوطني كمعرف
            };

            actionsPanel.Children.Add(viewButton);

            Grid.SetColumn(actionsPanel, 8);
            grid.Children.Add(actionsPanel);

            border.Child = grid;
            TeachersListPanel.Children.Add(border);

            // ربط الأحداث
            viewButton.Click += ViewTeacherButton_Click;
        }

        private Brush GetExperienceColor(int yearsOfExperience)
        {
            if (yearsOfExperience < 5)
                return new SolidColorBrush(Color.FromRgb(76, 175, 80)); // أخضر (مبتدئ)
            else if (yearsOfExperience < 15)
                return new SolidColorBrush(Color.FromRgb(33, 150, 243)); // أزرق (متوسط)
            else
                return new SolidColorBrush(Color.FromRgb(255, 152, 0)); // برتقالي (خبير)
        }

        private void UpdateStatistics()
        {
            TotalTeachersText.Text = allTeachers.Count.ToString();

            if (allTeachers.Count > 0)
            {
                // حساب متوسط الخبرة
                double totalExperience = 0;
                int maxExperience = 0;
                double totalAge = 0;
                HashSet<string> specializations = new HashSet<string>();

                foreach (var teacher in allTeachers)
                {
                    totalExperience += teacher.YearsOfExperience;
                    totalAge += teacher.Age;

                    if (teacher.YearsOfExperience > maxExperience)
                        maxExperience = teacher.YearsOfExperience;

                    if (!string.IsNullOrEmpty(teacher.Specialization))
                        specializations.Add(teacher.Specialization);
                }

                double avgExperience = totalExperience / allTeachers.Count;
                double avgAge = totalAge / allTeachers.Count;

                AvgExperienceText.Text = $"{avgExperience:F1} سنة";
                MaxExperienceText.Text = $"{maxExperience} سنة";
                SpecializationsCountText.Text = specializations.Count.ToString();
                AvgAgeText.Text = $"{avgAge:F1} سنة";
            }
            else
            {
                AvgExperienceText.Text = "0 سنة";
                MaxExperienceText.Text = "0 سنة";
                SpecializationsCountText.Text = "0";
                AvgAgeText.Text = "0 سنة";
            }
        }

        private void SearchTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            FilterTeachers();
        }

        private void SpecializationFilterComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            FilterTeachers();
        }

        private void FilterTeachers()
        {
            string searchText = SearchTextBox.Text.ToLower();
            string selectedSpecialization = (SpecializationFilterComboBox.SelectedItem as ComboBoxItem)?.Content.ToString();

            filteredTeachers.Clear();
            TeachersListPanel.Children.Clear();

            foreach (var teacher in allTeachers)
            {
                bool matchesSearch = string.IsNullOrEmpty(searchText) ||
                                   teacher.TeacherName.ToLower().Contains(searchText) ||
                                   teacher.NationalNumber.Contains(searchText) ||
                                   teacher.PhoneNumber.Contains(searchText) ||
                                   teacher.Specialization.ToLower().Contains(searchText);

                bool matchesSpecialization = selectedSpecialization == "جميع التخصصات" ||
                                           teacher.Specialization == selectedSpecialization;

                if (matchesSearch && matchesSpecialization)
                {
                    filteredTeachers.Add(teacher);
                    AddTeacherToPanel(teacher);
                }
            }
        }

        private void RefreshButton_Click(object sender, RoutedEventArgs e)
        {
            LoadTeachers();
            SearchTextBox.Text = "";
            SpecializationFilterComboBox.SelectedIndex = 0;
        }

        private void ViewTeacherButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.Tag is string nationalNumber)
            {
                var teacher = allTeachers.Find(t => t.NationalNumber == nationalNumber);
                if (teacher != null)
                {
                    string message = $"اسم المعلم: {teacher.TeacherName}\n" +
                                   $"الرقم الوطني: {teacher.NationalNumber}\n" +
                                   $"العمر: {teacher.Age} سنة\n" +
                                   $"رقم الهاتف: {teacher.PhoneNumber}\n" +
                                   $"سنوات الخبرة: {teacher.YearsOfExperience} سنة\n" +
                                   $"التخصص: {teacher.Specialization}\n" +
                                   $"تاريخ التعيين: {teacher.HireDate:yyyy-MM-dd}\n" +
                                   $"مدة العمل حتى الآن: {(DateTime.Now - teacher.HireDate).Days / 365} سنة";

                    MessageBox.Show(message, "تفاصيل المعلم",
                        MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
        }

       
    }

    public class TeacherData
    {
        public int Index { get; set; }
        public string TeacherName { get; set; }
        public string NationalNumber { get; set; }
        public int Age { get; set; }
        public string PhoneNumber { get; set; }
        public int YearsOfExperience { get; set; }
        public string Specialization { get; set; }
        public DateTime HireDate { get; set; }
    }
}