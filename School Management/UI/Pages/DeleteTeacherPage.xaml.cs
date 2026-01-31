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
    public partial class DeleteTeacherPage : UserControl
    {
        private List<TeacherData> searchResults = new List<TeacherData>();
        private TeacherData selectedTeacher = null;
        private List<TeacherData> allTeachers = new List<TeacherData>();

        public DeleteTeacherPage()
        {
            InitializeComponent();
            LoadStatistics();
        }

        private void LoadStatistics()
        {
            try
            {
                allTeachers.Clear();
                string query = @"
                    SELECT TeacherID, TeacherName, NationalNumber, Specialization, 
                           Age, PhoneNumber, YearsOfExperience, Salary, HireDate
                    FROM Teachers 
                    ORDER BY TeacherName";

                if (CurrentConnection.OpenConntion())
                {
                    using (SqlCommand cmd = new SqlCommand(query, CurrentConnection.CuCon))
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            var teacher = new TeacherData
                            {
                                TeacherID = reader.GetGuid(0),
                                TeacherName = reader.GetString(1),
                                NationalNumber = reader.GetString(2),
                                Specialization = reader.GetString(3),
                                Age = reader.GetInt32(4),
                                PhoneNumber = reader.GetString(5),
                                YearsOfExperience = reader.GetInt32(6),
                                Salary = reader.GetInt32(7),
                                HireDate = reader.GetDateTime(8)
                            };

                            allTeachers.Add(teacher);
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
            TotalTeachersText.Text = allTeachers.Count.ToString();

            if (allTeachers.Count > 0)
            {
                double totalSalary = 0;
                double totalExperience = 0;
                HashSet<string> uniqueSpecializations = new HashSet<string>();

                foreach (var teacher in allTeachers)
                {
                    totalSalary += teacher.Salary;
                    totalExperience += teacher.YearsOfExperience;
                    uniqueSpecializations.Add(teacher.Specialization);
                }

                double avgSalary = totalSalary / allTeachers.Count;
                double avgExperience = totalExperience / allTeachers.Count;

                AvgSalaryText.Text = avgSalary.ToString("N0");
                AvgExperienceText.Text = avgExperience.ToString("F1") + " سنة";
                SpecializationsCountText.Text = uniqueSpecializations.Count.ToString();
            }
            else
            {
                AvgSalaryText.Text = "0";
                AvgExperienceText.Text = "0 سنة";
                SpecializationsCountText.Text = "0";
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

            SearchTeachers(searchText);
        }

        private void SearchTeachers(string searchText)
        {
            try
            {
                searchResults.Clear();
                SearchResultsPanel.Children.Clear();

                string query = @"
                    SELECT TeacherID, TeacherName, NationalNumber, Specialization, 
                           Age, PhoneNumber, YearsOfExperience, Salary, HireDate
                    FROM Teachers 
                    WHERE TeacherName LIKE @SearchText 
                       OR NationalNumber LIKE @SearchText 
                       OR Specialization LIKE @SearchText
                    ORDER BY TeacherName";

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
                                var teacher = new TeacherData
                                {
                                    Index = index,
                                    TeacherID = reader.GetGuid(0),
                                    TeacherName = reader.GetString(1),
                                    NationalNumber = reader.GetString(2),
                                    Specialization = reader.GetString(3),
                                    Age = reader.GetInt32(4),
                                    PhoneNumber = reader.GetString(5),
                                    YearsOfExperience = reader.GetInt32(6),
                                    Salary = reader.GetInt32(7),
                                    HireDate = reader.GetDateTime(8)
                                };

                                searchResults.Add(teacher);
                                AddSearchResultToPanel(teacher);
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
                    MessageBox.Show("لم يتم العثور على مدرسين بهذه المعايير", "لا توجد نتائج",
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

        private void AddSearchResultToPanel(TeacherData teacher)
        {
            var border = new Border
            {
                Background = teacher.Index % 2 == 0 ? Brushes.White : Brushes.WhiteSmoke,
                BorderBrush = new SolidColorBrush(Color.FromRgb(229, 229, 229)),
                BorderThickness = new Thickness(0, 0, 0, 1),
                Padding = new Thickness(15, 10, 15, 10),
                Cursor = System.Windows.Input.Cursors.Hand
            };

            border.MouseLeftButtonDown += (s, e) => SelectTeacher(teacher);

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
                Text = teacher.Index.ToString(),
                VerticalAlignment = VerticalAlignment.Center,
                Foreground = new SolidColorBrush(Color.FromRgb(117, 117, 117))
            };
            Grid.SetColumn(indexText, 0);
            grid.Children.Add(indexText);

            // اسم المدرس
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
                Foreground = new SolidColorBrush(Color.FromRgb(117, 117, 117))
            };
            Grid.SetColumn(nationalText, 2);
            grid.Children.Add(nationalText);

            // التخصص
            var specializationText = new TextBlock
            {
                Text = teacher.Specialization,
                VerticalAlignment = VerticalAlignment.Center,
                Foreground = new SolidColorBrush(Color.FromRgb(33, 150, 243))
            };
            Grid.SetColumn(specializationText, 3);
            grid.Children.Add(specializationText);

            // سنوات الخبرة
            var experienceText = new TextBlock
            {
                Text = $"{teacher.YearsOfExperience} سنة",
                VerticalAlignment = VerticalAlignment.Center,
                FontWeight = FontWeights.Medium,
                Foreground = new SolidColorBrush(Color.FromRgb(76, 175, 80))
            };
            Grid.SetColumn(experienceText, 4);
            grid.Children.Add(experienceText);

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
                Tag = teacher
            };

            selectButton.Click += (s, e) => SelectTeacher(teacher);
            Grid.SetColumn(selectButton, 5);
            grid.Children.Add(selectButton);

            border.Child = grid;
            SearchResultsPanel.Children.Add(border);
        }

        private void SelectTeacher(TeacherData teacher)
        {
            selectedTeacher = teacher;
            ShowTeacherDetails(teacher);
        }

        private void ShowTeacherDetails(TeacherData teacher)
        {
            TeacherDetailsBorder.Visibility = Visibility.Visible;

            // تعبئة البيانات
            TeacherNameText.Text = teacher.TeacherName;
            NationalNumberText.Text = teacher.NationalNumber;
            SpecializationText.Text = teacher.Specialization;
            AgeText.Text = $"{teacher.Age} سنة";
            PhoneNumberText.Text = teacher.PhoneNumber;
            YearsOfExperienceText.Text = $"{teacher.YearsOfExperience} سنة";
            SalaryText.Text = $"{teacher.Salary:N0} ريال";
            HireDateText.Text = teacher.HireDate.ToString("yyyy-MM-dd");
            TeacherIDText.Text = teacher.TeacherID.ToString();

            // حساب مدة الخدمة
            TimeSpan serviceDuration = DateTime.Now - teacher.HireDate;
            int years = serviceDuration.Days / 365;
            int months = (serviceDuration.Days % 365) / 30;

            if (years > 0)
                ServiceDurationText.Text = $"{years} سنة و {months} شهر";
            else
                ServiceDurationText.Text = $"{months} شهر";

            // تحديد مستوى الخبرة
            if (teacher.YearsOfExperience < 5)
                ExperienceLevelText.Text = "مبتدئ";
            else if (teacher.YearsOfExperience < 15)
                ExperienceLevelText.Text = "متوسط";
            else
                ExperienceLevelText.Text = "خبير";
        }

        private void DeleteButton_Click(object sender, RoutedEventArgs e)
        {
            if (selectedTeacher == null)
            {
                MessageBox.Show("لم يتم تحديد مدرس للحذف", "تنبيه",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // التحقق من وجود صفوف مرتبطة بالمدرس
            if (CheckIfTeacherHasClasses(selectedTeacher.TeacherID))
            {
                MessageBox.Show($"لا يمكن حذف المدرس '{selectedTeacher.TeacherName}' لأنه يدرس في صفوف.\n" +
                              "يجب إزالة المدرس من الصفوف أولاً.", "خطأ في الحذف",
                    MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            var result = MessageBox.Show(
                $"هل أنت متأكد من حذف المدرس التالي:\n\n" +
                $"اسم المدرس: {selectedTeacher.TeacherName}\n" +
                $"الرقم الوطني: {selectedTeacher.NationalNumber}\n" +
                $"التخصص: {selectedTeacher.Specialization}\n" +
                $"سنوات الخبرة: {selectedTeacher.YearsOfExperience} سنة\n\n" +
                $"هذا الإجراء لا يمكن التراجع عنه وسيتم حذف جميع بيانات المدرس!",
                "تأكيد حذف المدرس",
                MessageBoxButton.YesNo,
                MessageBoxImage.Warning);

            if (result == MessageBoxResult.Yes)
            {
                DeleteTeacher();
            }
        }

        private bool CheckIfTeacherHasClasses(Guid teacherID)
        {
            try
            {
                string query = "SELECT COUNT(*) FROM Teacher_Class_Group WHERE TeacherID = @TeacherID";

                if (CurrentConnection.OpenConntion())
                {
                    using (SqlCommand cmd = new SqlCommand(query, CurrentConnection.CuCon))
                    {
                        cmd.Parameters.AddWithValue("@TeacherID", teacherID);
                        int count = (int)cmd.ExecuteScalar();
                        return count > 0;
                    }
                }
                return false;
            }
            catch (Exception)
            {
                // في حالة حدوث خطأ، نفترض أن هناك صفوف مرتبطة لمنع الحذف العرضي
                return true;
            }
            finally
            {
                CurrentConnection.CloseConntion();
            }
        }

        private void DeleteTeacher()
        {
            try
            {
                string query = "DELETE FROM Teachers WHERE TeacherID = @TeacherID";

                if (CurrentConnection.OpenConntion())
                {
                    using (SqlCommand cmd = new SqlCommand(query, CurrentConnection.CuCon))
                    {
                        cmd.Parameters.AddWithValue("@TeacherID", selectedTeacher.TeacherID);
                        int rowsAffected = cmd.ExecuteNonQuery();

                        if (rowsAffected > 0)
                        {
                            MessageBox.Show($"تم حذف المدرس '{selectedTeacher.TeacherName}' بنجاح",
                                "نجاح", MessageBoxButton.OK, MessageBoxImage.Information);

                            // إعادة تعيين الواجهة
                            ResetPage();
                            LoadStatistics();
                        }
                        else
                        {
                            MessageBox.Show("فشل حذف المدرس", "خطأ",
                                MessageBoxButton.OK, MessageBoxImage.Error);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"حدث خطأ أثناء حذف المدرس: {ex.Message}",
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
            selectedTeacher = null;
            searchResults.Clear();
            SearchResultsPanel.Children.Clear();
            SearchTextBox.Text = "";
            SearchResultsBorder.Visibility = Visibility.Collapsed;
            TeacherDetailsBorder.Visibility = Visibility.Collapsed;
        }
    }

    public class TeacherData
    {
        public int Index { get; set; }
        public Guid TeacherID { get; set; }
        public string TeacherName { get; set; }
        public string NationalNumber { get; set; }
        public string Specialization { get; set; }
        public int Age { get; set; }
        public string PhoneNumber { get; set; }
        public int YearsOfExperience { get; set; }
        public int Salary { get; set; }
        public DateTime HireDate { get; set; }
    }
}