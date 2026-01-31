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
    public partial class DeleteClassPage : UserControl
    {
        private List<ClassData> searchResults = new List<ClassData>();
        private ClassData selectedClass = null;
        private List<ClassData> allClasses = new List<ClassData>();

        public DeleteClassPage()
        {
            InitializeComponent();
            LoadStatistics();
        }

        private void LoadStatistics()
        {
            try
            {
                allClasses.Clear();
                string query = @"
                    SELECT ClassID, EducationLevel, AdditionalInfo, CreatedDate
                    FROM Classes 
                    ORDER BY CreatedDate DESC";

                if (CurrentConnection.OpenConntion())
                {
                    using (SqlCommand cmd = new SqlCommand(query, CurrentConnection.CuCon))
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            var classItem = new ClassData
                            {
                                ClassID = reader.GetGuid(0),
                                EducationLevel = reader.GetString(1),
                                AdditionalInfo = reader.IsDBNull(2) ? "" : reader.GetString(2),
                                CreatedDate = reader.GetDateTime(3)
                            };

                            allClasses.Add(classItem);
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
            TotalClassesText.Text = allClasses.Count.ToString();

            if (allClasses.Count > 0)
            {
                // حساب الصفوف التي أنشئت هذا الشهر
                int thisMonthClasses = 0;
                DateTime oldestDate = DateTime.MaxValue;
                DateTime newestDate = DateTime.MinValue;
                string oldestClass = "";
                string newestClass = "";

                foreach (var classItem in allClasses)
                {
                    if (classItem.CreatedDate.Month == DateTime.Now.Month &&
                        classItem.CreatedDate.Year == DateTime.Now.Year)
                    {
                        thisMonthClasses++;
                    }

                    if (classItem.CreatedDate < oldestDate)
                    {
                        oldestDate = classItem.CreatedDate;
                        oldestClass = classItem.EducationLevel;
                    }

                    if (classItem.CreatedDate > newestDate)
                    {
                        newestDate = classItem.CreatedDate;
                        newestClass = classItem.EducationLevel;
                    }
                }

                ThisMonthClassesText.Text = thisMonthClasses.ToString();

                if (!string.IsNullOrEmpty(oldestClass))
                {
                    OldestClassText.Text = oldestClass.Length > 10 ?
                        oldestClass.Substring(0, 8) + "..." :
                        oldestClass;
                }

                if (!string.IsNullOrEmpty(newestClass))
                {
                    NewestClassText.Text = newestClass.Length > 10 ?
                        newestClass.Substring(0, 8) + "..." :
                        newestClass;
                }
            }
            else
            {
                ThisMonthClassesText.Text = "0";
                OldestClassText.Text = "-";
                NewestClassText.Text = "-";
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

            SearchClasses(searchText);
        }

        private void SearchClasses(string searchText)
        {
            try
            {
                searchResults.Clear();
                SearchResultsPanel.Children.Clear();

                string query = @"
                    SELECT ClassID, EducationLevel, AdditionalInfo, CreatedDate
                    FROM Classes 
                    WHERE EducationLevel LIKE @SearchText 
                       OR AdditionalInfo LIKE @SearchText
                    ORDER BY EducationLevel";

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
                                var classItem = new ClassData
                                {
                                    Index = index,
                                    ClassID = reader.GetGuid(0),
                                    EducationLevel = reader.GetString(1),
                                    AdditionalInfo = reader.IsDBNull(2) ? "" : reader.GetString(2),
                                    CreatedDate = reader.GetDateTime(3)
                                };

                                searchResults.Add(classItem);
                                AddSearchResultToPanel(classItem);
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
                    MessageBox.Show("لم يتم العثور على صفوف بهذه المعايير", "لا توجد نتائج",
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

        private void AddSearchResultToPanel(ClassData classItem)
        {
            var border = new Border
            {
                Background = classItem.Index % 2 == 0 ? Brushes.White : Brushes.WhiteSmoke,
                BorderBrush = new SolidColorBrush(Color.FromRgb(229, 229, 229)),
                BorderThickness = new Thickness(0, 0, 0, 1),
                Padding = new Thickness(15, 10, 15, 10),
                Cursor = System.Windows.Input.Cursors.Hand
            };

            border.MouseLeftButtonDown += (s, e) => SelectClass(classItem);

            var grid = new Grid();
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(50) });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(150) });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(80) });

            // الرقم التسلسلي
            var indexText = new TextBlock
            {
                Text = classItem.Index.ToString(),
                VerticalAlignment = VerticalAlignment.Center,
                Foreground = new SolidColorBrush(Color.FromRgb(117, 117, 117))
            };
            Grid.SetColumn(indexText, 0);
            grid.Children.Add(indexText);

            // مستوى التعليم
            var educationText = new TextBlock
            {
                Text = classItem.EducationLevel,
                VerticalAlignment = VerticalAlignment.Center,
                FontWeight = FontWeights.Medium,
                Foreground = new SolidColorBrush(Color.FromRgb(33, 33, 33))
            };
            Grid.SetColumn(educationText, 1);
            grid.Children.Add(educationText);

            // المعلومات الإضافية (مختصرة)
            string shortInfo = classItem.AdditionalInfo;
            if (shortInfo.Length > 30)
                shortInfo = shortInfo.Substring(0, 27) + "...";

            var infoText = new TextBlock
            {
                Text = string.IsNullOrEmpty(shortInfo) ? "لا توجد معلومات إضافية" : shortInfo,
                VerticalAlignment = VerticalAlignment.Center,
                Foreground = new SolidColorBrush(Color.FromRgb(117, 117, 117))
            };
            Grid.SetColumn(infoText, 2);
            grid.Children.Add(infoText);

            // تاريخ الإنشاء
            var dateText = new TextBlock
            {
                Text = classItem.CreatedDate.ToString("yyyy-MM-dd"),
                VerticalAlignment = VerticalAlignment.Center,
                Foreground = new SolidColorBrush(Color.FromRgb(117, 117, 117))
            };
            Grid.SetColumn(dateText, 3);
            grid.Children.Add(dateText);

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
                Tag = classItem
            };

            selectButton.Click += (s, e) => SelectClass(classItem);
            Grid.SetColumn(selectButton, 4);
            grid.Children.Add(selectButton);

            border.Child = grid;
            SearchResultsPanel.Children.Add(border);
        }

        private void SelectClass(ClassData classItem)
        {
            selectedClass = classItem;
            ShowClassDetails(classItem);
        }

        private void ShowClassDetails(ClassData classItem)
        {
            ClassDetailsBorder.Visibility = Visibility.Visible;

            // تعبئة البيانات
            EducationLevelText.Text = classItem.EducationLevel;
            CreatedDateText.Text = classItem.CreatedDate.ToString("yyyy-MM-dd HH:mm");
            AdditionalInfoText.Text = string.IsNullOrEmpty(classItem.AdditionalInfo) ?
                "لا توجد معلومات إضافية" : classItem.AdditionalInfo;
            ClassIDText.Text = classItem.ClassID.ToString();

            // حساب عمر الصف
            TimeSpan classAge = DateTime.Now - classItem.CreatedDate;
            int days = classAge.Days;

            if (days > 365)
            {
                int years = days / 365;
                int remainingDays = days % 365;
                ClassAgeText.Text = $"{years} سنة و {remainingDays} يوم";
            }
            else if (days > 30)
            {
                int months = days / 30;
                ClassAgeText.Text = $"{months} شهر";
            }
            else
            {
                ClassAgeText.Text = $"{days} يوم";
            }

            // مراحل العملية
            ProcessStagesText.Text = "1. بحث → 2. تحديد → 3. مراجعة → 4. حذف";
        }

        private void DeleteButton_Click(object sender, RoutedEventArgs e)
        {
            if (selectedClass == null)
            {
                MessageBox.Show("لم يتم تحديد صف للحذف", "تنبيه",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // التحقق من وجود شعب مرتبطة بالصف
            if (CheckIfClassHasGroups(selectedClass.ClassID))
            {
                MessageBox.Show($"لا يمكن حذف الصف '{selectedClass.EducationLevel}' لأنه يحتوي على شعب مرتبطة.\n" +
                              "يجب حذف الشعب المرتبطة أولاً.", "خطأ في الحذف",
                    MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            var result = MessageBox.Show(
                $"هل أنت متأكد من حذف الصف الدراسي التالي:\n\n" +
                $"مستوى التعليم: {selectedClass.EducationLevel}\n" +
                $"تاريخ الإنشاء: {selectedClass.CreatedDate:yyyy-MM-dd}\n" +
                $"المعلومات الإضافية: {selectedClass.AdditionalInfo}\n\n" +
                $"هذا الإجراء لا يمكن التراجع عنه وسيتم حذف جميع بيانات الصف!",
                "تأكيد حذف الصف الدراسي",
                MessageBoxButton.YesNo,
                MessageBoxImage.Warning);

            if (result == MessageBoxResult.Yes)
            {
                DeleteClass();
            }
        }

        private bool CheckIfClassHasGroups(Guid classID)
        {
            try
            {
                string query = "SELECT COUNT(*) FROM Class_Group WHERE ClassID = @ClassID";

                if (CurrentConnection.OpenConntion())
                {
                    using (SqlCommand cmd = new SqlCommand(query, CurrentConnection.CuCon))
                    {
                        cmd.Parameters.AddWithValue("@ClassID", classID);
                        int count = (int)cmd.ExecuteScalar();
                        return count > 0;
                    }
                }
                return false;
            }
            catch (Exception)
            {
                // في حالة حدوث خطأ، نفترض أن هناك شعب مرتبطة لمنع الحذف العرضي
                return true;
            }
            finally
            {
                CurrentConnection.CloseConntion();
            }
        }

        private void DeleteClass()
        {
            try
            {
                string query = "DELETE FROM Classes WHERE ClassID = @ClassID";

                if (CurrentConnection.OpenConntion())
                {
                    using (SqlCommand cmd = new SqlCommand(query, CurrentConnection.CuCon))
                    {
                        cmd.Parameters.AddWithValue("@ClassID", selectedClass.ClassID);
                        int rowsAffected = cmd.ExecuteNonQuery();

                        if (rowsAffected > 0)
                        {
                            MessageBox.Show($"تم حذف الصف '{selectedClass.EducationLevel}' بنجاح",
                                "نجاح", MessageBoxButton.OK, MessageBoxImage.Information);

                            // إعادة تعيين الواجهة
                            ResetPage();
                            LoadStatistics();
                        }
                        else
                        {
                            MessageBox.Show("فشل حذف الصف", "خطأ",
                                MessageBoxButton.OK, MessageBoxImage.Error);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"حدث خطأ أثناء حذف الصف: {ex.Message}",
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
            selectedClass = null;
            searchResults.Clear();
            SearchResultsPanel.Children.Clear();
            SearchTextBox.Text = "";
            SearchResultsBorder.Visibility = Visibility.Collapsed;
            ClassDetailsBorder.Visibility = Visibility.Collapsed;
        }
    }

    public class ClassData
    {
        public int Index { get; set; }
        public Guid ClassID { get; set; }
        public string EducationLevel { get; set; }
        public string AdditionalInfo { get; set; }
        public DateTime CreatedDate { get; set; }
    }
}