using School_Management.Control;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Windows;
using System.Windows.Controls;

namespace School_Management.UI.EmpPages
{
    public partial class AddClassGroupPage : UserControl
    {
        private List<ClassItem> allClasses = new List<ClassItem>();
        private List<GroupItem> allGroups = new List<GroupItem>();

        private bool isClassValid = false;
        private bool isGroupValid = false;
        private bool isFullClassNameValid = false;
        private bool isMaxStudentsValid = true;

        public AddClassGroupPage()
        {
            InitializeComponent();
            LoadData();
        }

        private void LoadData()
        {
            LoadClasses();
            LoadGroups();
            UpdateSaveButtonState();
        }

        private void LoadClasses()
        {
            try
            {
                allClasses.Clear();
                ClassComboBox.ItemsSource = null;

                string query = "SELECT ClassID, EducationLevel FROM Classes ORDER BY EducationLevel";

                if (CurrentConnection.OpenConntion())
                {
                    using (SqlCommand cmd = new SqlCommand(query, CurrentConnection.CuCon))
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            allClasses.Add(new ClassItem
                            {
                                ClassID = reader.GetGuid(0),
                                EducationLevel = reader.GetString(1)
                            });
                        }
                    }
                }

                ClassComboBox.ItemsSource = allClasses;

                if (allClasses.Count == 0)
                {
                    ShowError(ClassErrorText, "لا يوجد صفوف في النظام، يرجى إضافة صف أولاً");
                }
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

        private void LoadGroups()
        {
            try
            {
                allGroups.Clear();
                GroupComboBox.ItemsSource = null;

                // جلب جميع الشعب من جدول Groups
                string query = "SELECT GroupID, GroupName FROM Groups ORDER BY GroupName";

                if (CurrentConnection.OpenConntion())
                {
                    using (SqlCommand cmd = new SqlCommand(query, CurrentConnection.CuCon))
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            allGroups.Add(new GroupItem
                            {
                                GroupID = reader.GetGuid(0),
                                GroupName = reader.GetString(1)
                            });
                        }
                    }
                }

                GroupComboBox.ItemsSource = allGroups;

                if (allGroups.Count == 0)
                {
                    ShowError(GroupErrorText, "لا يوجد شعب في النظام، يرجى إضافة شعبة أولاً");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"حدث خطأ أثناء تحميل الشعب: {ex.Message}",
                    "خطأ", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                CurrentConnection.CloseConntion();
            }
        }

        private void ShowError(TextBlock errorTextBlock, string message)
        {
            errorTextBlock.Text = message;
            errorTextBlock.Visibility = Visibility.Visible;
        }

        private void HideError(TextBlock errorTextBlock)
        {
            errorTextBlock.Visibility = Visibility.Collapsed;
        }

        private void ShowFormStatus(string message, string icon = "⚠️", string color = "#FF9800")
        {
            StatusIcon.Text = icon;
            StatusMessage.Text = message;
            StatusMessage.Foreground = new System.Windows.Media.SolidColorBrush(
                (System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString(color));
            FormStatusBorder.Visibility = Visibility.Visible;
        }

        private void HideFormStatus()
        {
            FormStatusBorder.Visibility = Visibility.Collapsed;
        }

        private void UpdateSaveButtonState()
        {
            SaveButton.IsEnabled = isClassValid && isGroupValid && isFullClassNameValid && isMaxStudentsValid;
        }

        private void UpdateSummary()
        {
            if (ClassComboBox.SelectedItem != null && GroupComboBox.SelectedItem != null)
            {
                var selectedClass = (ClassItem)ClassComboBox.SelectedItem;
                var selectedGroup = (GroupItem)GroupComboBox.SelectedItem;

                SummaryClassName.Text = selectedClass.EducationLevel;
                SummaryGroupName.Text = selectedGroup.GroupName;
                SummaryMaxStudents.Text = $"{MaxStudentsSlider.Value} طالب";
                SummaryFullName.Text = FullClassNameTextBox.Text;

                SummaryBorder.Visibility = Visibility.Visible;

                FullClassNameTextBox.Text = $"الصف : {selectedClass.EducationLevel} - الشعبة : {selectedGroup.GroupName}";
                ValidateFullClassName();
            }
            else
            {
                SummaryBorder.Visibility = Visibility.Collapsed;
            }
        }

        #region أحداث التحكم

        private void ClassComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (ClassComboBox.SelectedItem != null)
            {
                isClassValid = true;
                HideError(ClassErrorText);
                UpdateSummary();
            }
            else
            {
                isClassValid = false;
                ShowError(ClassErrorText, "الرجاء اختيار الصف");
            }

            UpdateSaveButtonState();
        }

        private void GroupComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (GroupComboBox.SelectedItem != null)
            {
                isGroupValid = true;
                HideError(GroupErrorText);
                UpdateSummary();
            }
            else
            {
                isGroupValid = false;
                ShowError(GroupErrorText, "الرجاء اختيار الشعبة");
            }

            UpdateSaveButtonState();
        }

        private void FullClassNameTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            ValidateFullClassName();
            UpdateSummary();
            UpdateSaveButtonState();
        }

        private void ValidateFullClassName()
        {
            string fullName = FullClassNameTextBox.Text.Trim();

            if (string.IsNullOrWhiteSpace(fullName))
            {
                ShowError(FullClassNameErrorText, "الرجاء إدخال اسم الصف الكامل");
                isFullClassNameValid = false;
            }
            else if (fullName.Length < 3)
            {
                ShowError(FullClassNameErrorText, "اسم الصف الكامل يجب أن يكون 3 أحرف على الأقل");
                isFullClassNameValid = false;
            }
            else
            {
                // التحقق من أن هذا الاسم غير موجود مسبقاً
                if (CheckDuplicateFullClassName(fullName))
                {
                    ShowError(FullClassNameErrorText, "اسم الصف الكامل موجود مسبقاً");
                    isFullClassNameValid = false;
                }
                else
                {
                    HideError(FullClassNameErrorText);
                    isFullClassNameValid = true;
                }
            }
        }

        private bool CheckDuplicateFullClassName(string fullClassName)
        {
            try
            {
                string query = "SELECT COUNT(*) FROM Class_Group WHERE fullClassName = @FullClassName";

                if (CurrentConnection.OpenConntion())
                {
                    using (SqlCommand cmd = new SqlCommand(query, CurrentConnection.CuCon))
                    {
                        cmd.Parameters.AddWithValue("@FullClassName", fullClassName);
                        int count = Convert.ToInt32(cmd.ExecuteScalar());
                        return count > 0;
                    }
                }
                return false;
            }
            catch
            {
                return false;
            }
            finally
            {
                CurrentConnection.CloseConntion();
            }
        }

        private void MaxStudentsSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (MaxStudentsValueText != null)
            {
                MaxStudentsValueText.Text = ((int)MaxStudentsSlider.Value).ToString();
                isMaxStudentsValid = true;
                UpdateSummary();
                UpdateSaveButtonState();
            }
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            if (!ValidateForm())
                return;

            try
            {
                var selectedClass = (ClassItem)ClassComboBox.SelectedItem;
                var selectedGroup = (GroupItem)GroupComboBox.SelectedItem;

                // التحقق من أن هذا التجميع غير موجود مسبقاً
                if (CheckExistingClassGroup(selectedClass.ClassID, selectedGroup.GroupID))
                {
                    MessageBox.Show("هذا الصف والشعبة مجتمعان مسبقاً في نظام الصفوف الكاملة",
                        "تنبيه", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                // استخدام الإجراء المخزن
                List<SqlParameter> parameters = new List<SqlParameter>()
                {
                    new SqlParameter("@fullClassName", FullClassNameTextBox.Text),
                    new SqlParameter("@ClassID", selectedClass.ClassID),
                    new SqlParameter("@GroupID", selectedGroup.GroupID),
                    new SqlParameter("@MaxStudents", (int)MaxStudentsSlider.Value)
                };

                bool success = SqlExec.Exec_proc("InsertClassGroup", parameters);

                if (success)
                {
                    MessageBox.Show($"تم إنشاء الصف الكامل بنجاح!\n" +
                                   $"اسم الصف الكامل: {FullClassNameTextBox.Text}\n" +
                                   $"الصف: {selectedClass.EducationLevel}\n" +
                                   $"الشعبة: {selectedGroup.GroupName}\n" +
                                   $"العدد الأقصى للطلاب: {MaxStudentsSlider.Value}",
                                   "نجاح", MessageBoxButton.OK, MessageBoxImage.Information);

                    ClearForm();
                    LoadData(); // تحديث البيانات
                }
                else
                {
                    MessageBox.Show("حدث خطأ أثناء حفظ البيانات", "خطأ",
                        MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            catch (SqlException ex)
            {
                if (ex.Message.Contains("هذا الصف والشعبة مجتمعان مسبقاً"))
                {
                    MessageBox.Show("هذا الصف والشعبة مجتمعان مسبقاً في النظام",
                        "تنبيه", MessageBoxButton.OK, MessageBoxImage.Warning);
                }
                else if (ex.Number == 2627) // انتهاك فريدة
                {
                    MessageBox.Show("اسم الصف الكامل موجود مسبقاً في النظام", "خطأ",
                        MessageBoxButton.OK, MessageBoxImage.Error);
                }
                else
                {
                    MessageBox.Show($"حدث خطأ في قاعدة البيانات: {ex.Message}", "خطأ",
                        MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"حدث خطأ: {ex.Message}", "خطأ",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                CurrentConnection.CloseConntion();
            }
        }

        private bool CheckExistingClassGroup(Guid classId, Guid groupId)
        {
            try
            {
                string query = "SELECT COUNT(*) FROM Class_Group WHERE ClassID = @ClassID AND GroupID = @GroupID";

                if (CurrentConnection.OpenConntion())
                {
                    using (SqlCommand cmd = new SqlCommand(query, CurrentConnection.CuCon))
                    {
                        cmd.Parameters.AddWithValue("@ClassID", classId);
                        cmd.Parameters.AddWithValue("@GroupID", groupId);
                        int count = Convert.ToInt32(cmd.ExecuteScalar());
                        return count > 0;
                    }
                }
                return false;
            }
            catch
            {
                return false;
            }
            finally
            {
                CurrentConnection.CloseConntion();
            }
        }

        private bool ValidateForm()
        {
            bool isValid = true;

            ValidateFullClassName();

            if (!isClassValid)
            {
                ShowError(ClassErrorText, "الرجاء اختيار الصف");
                isValid = false;
            }

            if (!isGroupValid)
            {
                ShowError(GroupErrorText, "الرجاء اختيار الشعبة");
                isValid = false;
            }

            if (!isFullClassNameValid)
            {
                isValid = false;
            }

            if (!isMaxStudentsValid)
            {
                ShowFormStatus("الرجاء تحديد عدد طلاب صحيح", "⚠️", "#FF9800");
                isValid = false;
            }

            return isValid;
        }

        private void ClearButton_Click(object sender, RoutedEventArgs e)
        {
            ClearForm();
        }

        private void ClearForm()
        {
            ClassComboBox.SelectedIndex = -1;
            GroupComboBox.SelectedIndex = -1;
            FullClassNameTextBox.Text = "";
            MaxStudentsSlider.Value = 25;

            isClassValid = false;
            isGroupValid = false;
            isFullClassNameValid = false;
            isMaxStudentsValid = true;

            SummaryBorder.Visibility = Visibility.Collapsed;
            HideFormStatus();

            HideError(ClassErrorText);
            HideError(GroupErrorText);
            HideError(FullClassNameErrorText);

            UpdateSaveButtonState();
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            var result = MessageBox.Show(
                "هل أنت متأكد من إلغاء العملية؟\nسيتم فقدان جميع البيانات المدخلة.",
                "تأكيد الإلغاء",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                var mainWindow = Window.GetWindow(this) as EmployeeDashboard;
                if (mainWindow != null)
                {
                    //  mainWindow.HomeButton_Click(sender, e);
                }
            }
        }

        #endregion
    }

    #region فئات البيانات

    public class ClassItem
    {
        public Guid ClassID { get; set; }
        public string EducationLevel { get; set; }

        public override string ToString()
        {
            return EducationLevel;
        }
    }

    public class GroupItem
    {
        public Guid GroupID { get; set; }
        public string GroupName { get; set; }

        public override string ToString()
        {
            return GroupName;
        }
    }

    #endregion
}