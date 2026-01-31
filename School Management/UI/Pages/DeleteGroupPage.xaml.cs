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
    public partial class DeleteGroupPage : UserControl
    {
        private List<GroupDataDelete> searchResults = new List<GroupDataDelete>();
        private GroupDataDelete selectedGroup = null;
        private List<GroupDataDelete> allGroups = new List<GroupDataDelete>();

        public DeleteGroupPage()
        {
            InitializeComponent();
            LoadStatistics();
        }

        private void LoadStatistics()
        {
            try
            {
                allGroups.Clear();
                string query = @"
                    SELECT GroupID, GroupName, AdditionalInfo
                    FROM Groups 
                    ORDER BY GroupName";

                if (CurrentConnection.OpenConntion())
                {
                    using (SqlCommand cmd = new SqlCommand(query, CurrentConnection.CuCon))
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            var group = new GroupDataDelete
                            {
                                GroupID = reader.GetGuid(0),
                                GroupName = reader.GetString(1),
                                AdditionalInfo = reader.IsDBNull(2) ? "" : reader.GetString(2)
                            };

                            allGroups.Add(group);
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
            TotalGroupsText.Text = allGroups.Count.ToString();

            if (allGroups.Count > 0)
            {
                int groupsWithInfo = 0;
                string longestName = "";
                int maxLength = 0;

                foreach (var group in allGroups)
                {
                    // حساب الشعب التي لديها معلومات إضافية
                    if (!string.IsNullOrEmpty(group.AdditionalInfo))
                        groupsWithInfo++;

                    // البحث عن أطول اسم شعبة
                    if (group.GroupName.Length > maxLength)
                    {
                        maxLength = group.GroupName.Length;
                        longestName = group.GroupName;
                    }
                }

                GroupsWithInfoText.Text = groupsWithInfo.ToString();
                GroupsWithoutInfoText.Text = (allGroups.Count - groupsWithInfo).ToString();

                if (!string.IsNullOrEmpty(longestName))
                {
                    LongestNameText.Text = longestName.Length > 10 ?
                        longestName.Substring(0, 8) + "..." :
                        longestName;
                }
                else
                {
                    LongestNameText.Text = "-";
                }
            }
            else
            {
                GroupsWithInfoText.Text = "0";
                GroupsWithoutInfoText.Text = "0";
                LongestNameText.Text = "-";
            }
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

            SearchGroups(searchText);
        }

        private void SearchGroups(string searchText)
        {
            try
            {
                searchResults.Clear();
                SearchResultsPanel.Children.Clear();

                string query = @"
                    SELECT GroupID, GroupName, AdditionalInfo
                    FROM Groups 
                    WHERE GroupName LIKE @SearchText 
                       OR AdditionalInfo LIKE @SearchText
                    ORDER BY GroupName";

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
                                var group = new GroupDataDelete
                                {
                                    Index = index,
                                    GroupID = reader.GetGuid(0),
                                    GroupName = reader.GetString(1),
                                    AdditionalInfo = reader.IsDBNull(2) ? "" : reader.GetString(2)
                                };

                                searchResults.Add(group);
                                AddSearchResultToPanel(group);
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
                    MessageBox.Show("لم يتم العثور على شعب بهذه المعايير", "لا توجد نتائج",
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

        private void AddSearchResultToPanel(GroupDataDelete group)
        {
            var border = new Border
            {
                Background = group.Index % 2 == 0 ? Brushes.White : Brushes.WhiteSmoke,
                BorderBrush = new SolidColorBrush(Color.FromRgb(229, 229, 229)),
                BorderThickness = new Thickness(0, 0, 0, 1),
                Padding = new Thickness(15, 10, 15, 10),
                Cursor = System.Windows.Input.Cursors.Hand
            };

            border.MouseLeftButtonDown += (s, e) => SelectGroup(group);

            var grid = new Grid();
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(50) });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(80) });

            // الرقم التسلسلي
            var indexText = new TextBlock
            {
                Text = group.Index.ToString(),
                VerticalAlignment = VerticalAlignment.Center,
                Foreground = new SolidColorBrush(Color.FromRgb(117, 117, 117))
            };
            Grid.SetColumn(indexText, 0);
            grid.Children.Add(indexText);

            // اسم الشعبة
            var nameText = new TextBlock
            {
                Text = group.GroupName,
                VerticalAlignment = VerticalAlignment.Center,
                FontWeight = FontWeights.Medium,
                Foreground = new SolidColorBrush(Color.FromRgb(33, 33, 33))
            };
            Grid.SetColumn(nameText, 1);
            grid.Children.Add(nameText);

            // المعلومات الإضافية (مختصرة)
            string shortInfo = group.AdditionalInfo;
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
                Tag = group
            };

            selectButton.Click += (s, e) => SelectGroup(group);
            Grid.SetColumn(selectButton, 3);
            grid.Children.Add(selectButton);

            border.Child = grid;
            SearchResultsPanel.Children.Add(border);
        }

        private void SelectGroup(GroupDataDelete group)
        {
            selectedGroup = group;
            ShowGroupDetails(group);
        }

        private void ShowGroupDetails(GroupDataDelete group)
        {
            GroupDetailsBorder.Visibility = Visibility.Visible;

            // تعبئة البيانات
            GroupNameText.Text = group.GroupName;
            AdditionalInfoText.Text = string.IsNullOrEmpty(group.AdditionalInfo) ?
                "لا توجد معلومات إضافية" : group.AdditionalInfo;
            GroupIDText.Text = group.GroupID.ToString();

            // طول المعلومات
            InfoLengthText.Text = $"{group.AdditionalInfo.Length} حرف";

            // حالة المعلومات
            if (string.IsNullOrEmpty(group.AdditionalInfo))
            {
                InfoStatusText.Text = "بدون معلومات";
                InfoStatusBorder.Background = new SolidColorBrush(Color.FromRgb(255, 193, 7)); // أصفر
                InfoStatusText.Foreground = Brushes.Black;
            }
            else if (group.AdditionalInfo.Length < 50)
            {
                InfoStatusText.Text = "معلومات قصيرة";
                InfoStatusBorder.Background = new SolidColorBrush(Color.FromRgb(76, 175, 80)); // أخضر
                InfoStatusText.Foreground = Brushes.White;
            }
            else if (group.AdditionalInfo.Length < 100)
            {
                InfoStatusText.Text = "معلومات متوسطة";
                InfoStatusBorder.Background = new SolidColorBrush(Color.FromRgb(33, 150, 243)); // أزرق
                InfoStatusText.Foreground = Brushes.White;
            }
            else
            {
                InfoStatusText.Text = "معلومات طويلة";
                InfoStatusBorder.Background = new SolidColorBrush(Color.FromRgb(156, 39, 176)); // بنفسجي
                InfoStatusText.Foreground = Brushes.White;
            }

            // مراحل العملية
            ProcessStagesText.Text = "1. بحث → 2. تحديد → 3. مراجعة → 4. حذف";
        }

        private void DeleteButton_Click(object sender, RoutedEventArgs e)
        {
            if (selectedGroup == null)
            {
                MessageBox.Show("لم يتم تحديد شعبة للحذف", "تنبيه",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // التحقق من وجود طلاب مرتبطين بالشعبة
            if (CheckIfGroupHasStudents(selectedGroup.GroupID))
            {
                MessageBox.Show($"لا يمكن حذف الشعبة '{selectedGroup.GroupName}' لأنها تحتوي على طلاب مسجلين.\n" +
                              "يجب إزالة الطلاب من الشعبة أولاً.", "خطأ في الحذف",
                    MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            var result = MessageBox.Show(
                $"هل أنت متأكد من حذف الشعبة التالية:\n\n" +
                $"اسم الشعبة: {selectedGroup.GroupName}\n" +
                $"المعلومات الإضافية: {selectedGroup.AdditionalInfo}\n\n" +
                $"هذا الإجراء لا يمكن التراجع عنه وسيتم حذف جميع بيانات الشعبة!",
                "تأكيد حذف الشعبة",
                MessageBoxButton.YesNo,
                MessageBoxImage.Warning);

            if (result == MessageBoxResult.Yes)
            {
                DeleteGroup();
            }
        }

        private bool CheckIfGroupHasStudents(Guid groupID)
        {
            try
            {
                // نفترض أن هناك جدول Student_Group يربط الطلاب بالشعب
                // يمكن تعديل الاستعلام حسب هيكل قاعدة البيانات الخاص بك
                string query = "SELECT COUNT(*) FROM Student_Group WHERE GroupID = @GroupID";

                if (CurrentConnection.OpenConntion())
                {
                    using (SqlCommand cmd = new SqlCommand(query, CurrentConnection.CuCon))
                    {
                        cmd.Parameters.AddWithValue("@GroupID", groupID);
                        int count = (int)cmd.ExecuteScalar();
                        return count > 0;
                    }
                }
                return false;
            }
            catch (Exception)
            {
                // في حالة عدم وجود الجدول أو حدوث خطأ، نفترض أنه لا توجد علاقات
                return false;
            }
            finally
            {
                CurrentConnection.CloseConntion();
            }
        }

        private void DeleteGroup()
        {
            try
            {
                string query = "DELETE FROM Groups WHERE GroupID = @GroupID";

                if (CurrentConnection.OpenConntion())
                {
                    using (SqlCommand cmd = new SqlCommand(query, CurrentConnection.CuCon))
                    {
                        cmd.Parameters.AddWithValue("@GroupID", selectedGroup.GroupID);
                        int rowsAffected = cmd.ExecuteNonQuery();

                        if (rowsAffected > 0)
                        {
                            MessageBox.Show($"تم حذف الشعبة '{selectedGroup.GroupName}' بنجاح",
                                "نجاح", MessageBoxButton.OK, MessageBoxImage.Information);

                            // إعادة تعيين الواجهة
                            ResetPage();
                            LoadStatistics();
                        }
                        else
                        {
                            MessageBox.Show("فشل حذف الشعبة", "خطأ",
                                MessageBoxButton.OK, MessageBoxImage.Error);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"حدث خطأ أثناء حذف الشعبة: {ex.Message}",
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
            selectedGroup = null;
            searchResults.Clear();
            SearchResultsPanel.Children.Clear();
            SearchTextBox.Text = "";
            SearchResultsBorder.Visibility = Visibility.Collapsed;
            GroupDetailsBorder.Visibility = Visibility.Collapsed;
        }
    }

    public class GroupDataDelete
    {
        public int Index { get; set; }
        public Guid GroupID { get; set; }
        public string GroupName { get; set; }
        public string AdditionalInfo { get; set; }
    }
}