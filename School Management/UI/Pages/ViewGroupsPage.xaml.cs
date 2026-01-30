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
    public partial class ViewGroupsPage : UserControl
    {
        private List<GroupData> allGroups = new List<GroupData>();
        private List<GroupData> filteredGroups = new List<GroupData>();

        public ViewGroupsPage()
        {
            InitializeComponent();
            LoadGroups();
        }

        private void LoadGroups()
        {
            try
            {
                allGroups.Clear();
                GroupsListPanel.Children.Clear();

                // الاستعلام المطلوب
                string query = @"
                    SELECT GroupName, AdditionalInfo 
                    FROM Groups 
                    ORDER BY GroupName";

                if (CurrentConnection.OpenConntion())
                {
                    using (SqlCommand cmd = new SqlCommand(query, CurrentConnection.CuCon))
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        int index = 1;
                        while (reader.Read())
                        {
                            var group = new GroupData
                            {
                                Index = index,
                                GroupName = reader.GetString(0),
                                AdditionalInfo = reader.IsDBNull(1) ? "" : reader.GetString(1)
                            };

                            allGroups.Add(group);
                            AddGroupToPanel(group);
                            index++;
                        }
                    }
                }

                UpdateStatistics();
                filteredGroups = new List<GroupData>(allGroups);
                InfoFilterComboBox.SelectedIndex = 0;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"حدث خطأ أثناء تحميل المجموعات: {ex.Message}",
                    "خطأ", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                CurrentConnection.CloseConntion();
            }
        }

        private void AddGroupToPanel(GroupData group)
        {
            var border = new Border
            {
                Background = group.Index % 2 == 0 ? Brushes.White : Brushes.WhiteSmoke,
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
                Text = group.Index.ToString(),
                VerticalAlignment = VerticalAlignment.Center,
                Foreground = new SolidColorBrush(Color.FromRgb(117, 117, 117))
            };
            Grid.SetColumn(indexText, 0);
            grid.Children.Add(indexText);

            // اسم المجموعة
            var groupNameText = new TextBlock
            {
                Text = group.GroupName,
                VerticalAlignment = VerticalAlignment.Center,
                FontWeight = FontWeights.Medium,
                Foreground = new SolidColorBrush(Color.FromRgb(0, 150, 136))
            };
            Grid.SetColumn(groupNameText, 1);
            grid.Children.Add(groupNameText);

            // المعلومات الإضافية
            var infoBorder = new Border
            {
                CornerRadius = new CornerRadius(5),
                Padding = new Thickness(10, 5, 10, 5),
                VerticalAlignment = VerticalAlignment.Center,
                Background = string.IsNullOrEmpty(group.AdditionalInfo) ?
                    new SolidColorBrush(Color.FromRgb(245, 245, 245)) :
                    new SolidColorBrush(Color.FromRgb(224, 242, 241)),
                BorderBrush = string.IsNullOrEmpty(group.AdditionalInfo) ?
                    new SolidColorBrush(Color.FromRgb(189, 189, 189)) :
                    new SolidColorBrush(Color.FromRgb(0, 150, 136)),
                BorderThickness = new Thickness(1),
                Child = new TextBlock
                {
                    Text = string.IsNullOrEmpty(group.AdditionalInfo) ?
                        "لا توجد معلومات إضافية" :
                        group.AdditionalInfo,
                    HorizontalAlignment = HorizontalAlignment.Center,
                    VerticalAlignment = VerticalAlignment.Center,
                    Foreground = string.IsNullOrEmpty(group.AdditionalInfo) ?
                        new SolidColorBrush(Color.FromRgb(117, 117, 117)) :
                        new SolidColorBrush(Color.FromRgb(0, 96, 100)),
                    TextWrapping = TextWrapping.Wrap,
                    MaxHeight = 40,
                    TextTrimming = TextTrimming.CharacterEllipsis
                }
            };
            Grid.SetColumn(infoBorder, 2);
            grid.Children.Add(infoBorder);

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
                Tag = group.GroupName // استخدام اسم المجموعة كمعرف
            };

            var deleteButton = new Button
            {
                Content = "🗑️",
                ToolTip = "حذف المجموعة",
                Width = 30,
                Height = 30,
                Background = new SolidColorBrush(Color.FromRgb(255, 235, 238)),
                BorderBrush = new SolidColorBrush(Color.FromRgb(239, 154, 154)),
                Tag = group.GroupName // استخدام اسم المجموعة كمعرف
            };

            actionsPanel.Children.Add(viewButton);
            actionsPanel.Children.Add(deleteButton);

            Grid.SetColumn(actionsPanel, 3);
            grid.Children.Add(actionsPanel);

            border.Child = grid;
            GroupsListPanel.Children.Add(border);

            // ربط الأحداث
            viewButton.Click += ViewGroupButton_Click;
            deleteButton.Click += DeleteGroupButton_Click;
        }

        private void UpdateStatistics()
        {
            TotalGroupsText.Text = allGroups.Count.ToString();

            if (allGroups.Count > 0)
            {
                int groupsWithInfo = 0;
                string longestGroupName = "";
                int maxLength = 0;

                foreach (var group in allGroups)
                {
                    // حساب المجموعات التي لديها معلومات إضافية
                    if (!string.IsNullOrEmpty(group.AdditionalInfo))
                        groupsWithInfo++;

                    // البحث عن أطول اسم مجموعة
                    if (group.GroupName.Length > maxLength)
                    {
                        maxLength = group.GroupName.Length;
                        longestGroupName = group.GroupName;
                    }
                }

                GroupsWithInfoText.Text = groupsWithInfo.ToString();
                GroupsWithoutInfoText.Text = (allGroups.Count - groupsWithInfo).ToString();

                // عرض أطول اسم مجموعة (مع تقصير إذا كان طويلاً جداً)
                if (longestGroupName.Length > 15)
                    LongestGroupNameText.Text = longestGroupName.Substring(0, 12) + "...";
                else
                    LongestGroupNameText.Text = longestGroupName;
            }
            else
            {
                GroupsWithInfoText.Text = "0";
                GroupsWithoutInfoText.Text = "0";
                LongestGroupNameText.Text = "-";
            }
        }

        private void SearchTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            FilterGroups();
        }

        private void InfoFilterComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            FilterGroups();
        }

        private void FilterGroups()
        {
            string searchText = SearchTextBox.Text.ToLower();
            string selectedFilter = (InfoFilterComboBox.SelectedItem as ComboBoxItem)?.Content.ToString();

            filteredGroups.Clear();
            GroupsListPanel.Children.Clear();

            foreach (var group in allGroups)
            {
                bool matchesSearch = string.IsNullOrEmpty(searchText) ||
                                   group.GroupName.ToLower().Contains(searchText) ||
                                   group.AdditionalInfo.ToLower().Contains(searchText);

                bool matchesFilter = selectedFilter == "جميع المجموعات" ||
                    (selectedFilter == "يوجد معلومات إضافية" && !string.IsNullOrEmpty(group.AdditionalInfo)) ||
                    (selectedFilter == "لا يوجد معلومات إضافية" && string.IsNullOrEmpty(group.AdditionalInfo));

                if (matchesSearch && matchesFilter)
                {
                    filteredGroups.Add(group);
                    AddGroupToPanel(group);
                }
            }
        }

        private void RefreshButton_Click(object sender, RoutedEventArgs e)
        {
            LoadGroups();
            SearchTextBox.Text = "";
            InfoFilterComboBox.SelectedIndex = 0;
        }

        private void ViewGroupButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.Tag is string groupName)
            {
                var group = allGroups.Find(g => g.GroupName == groupName);
                if (group != null)
                {
                    string infoText = string.IsNullOrEmpty(group.AdditionalInfo) ?
                        "لا توجد معلومات إضافية" :
                        group.AdditionalInfo;

                    string message = $"اسم المجموعة: {group.GroupName}\n" +
                                   $"المعلومات الإضافية: {infoText}\n\n" +
                                   "تفاصيل إضافية:\n" +
                                   $"• تاريخ التحميل: {DateTime.Now:yyyy-MM-dd HH:mm}\n" +
                                   $"• عدد المجموعات الكلي: {allGroups.Count}";

                    MessageBox.Show(message, "تفاصيل المجموعة",
                        MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
        }

        private void DeleteGroupButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.Tag is string groupName)
            {
                var group = allGroups.Find(g => g.GroupName == groupName);
                if (group != null)
                {
                    var result = MessageBox.Show(
                        $"هل أنت متأكد من حذف المجموعة:\n{group.GroupName}؟\n" +
                        $"المعلومات الإضافية: {group.AdditionalInfo}\n\n" +
                        $"هذا الإجراء لا يمكن التراجع عنه.",
                        "تأكيد الحذف",
                        MessageBoxButton.YesNo,
                        MessageBoxImage.Warning);

                    if (result == MessageBoxResult.Yes)
                    {
                        try
                        {
                            string query = "DELETE FROM Groups WHERE GroupName = @GroupName";

                            if (CurrentConnection.OpenConntion())
                            {
                                using (SqlCommand cmd = new SqlCommand(query, CurrentConnection.CuCon))
                                {
                                    cmd.Parameters.AddWithValue("@GroupName", groupName);
                                    int rowsAffected = cmd.ExecuteNonQuery();

                                    if (rowsAffected > 0)
                                    {
                                        MessageBox.Show("تم حذف المجموعة بنجاح", "نجاح",
                                            MessageBoxButton.OK, MessageBoxImage.Information);
                                    }
                                    else
                                    {
                                        MessageBox.Show("لم يتم العثور على المجموعة", "تنبيه",
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
                            LoadGroups();
                        }
                    }
                }
            }
        }
    }

    public class GroupData
    {
        public int Index { get; set; }
        public string GroupName { get; set; }
        public string AdditionalInfo { get; set; }
    }
}