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
    public partial class AddGroupPage : UserControl
    {
        private bool isGroupNameValid = false;

        public AddGroupPage()
        {
            InitializeComponent();
            InitializeForm();
        }

        private void InitializeForm()
        {
            UpdateSaveButtonState();
        }

        private void UpdateSaveButtonState()
        {
            SaveButton.IsEnabled = isGroupNameValid;
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

        private void GroupNameTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            ValidateGroupName();
            UpdateSaveButtonState();
        }

        private void ValidateGroupName()
        {
            string groupName = GroupNameTextBox.Text.Trim();

            if (string.IsNullOrWhiteSpace(groupName))
            {
                ShowError(GroupNameErrorText, "الرجاء إدخال اسم الشعبة");
                isGroupNameValid = false;
            }
            else if (groupName.Length < 2)
            {
                ShowError(GroupNameErrorText, "اسم الشعبة يجب أن يكون حرفين على الأقل");
                isGroupNameValid = false;
            }
            else
            {
                HideError(GroupNameErrorText);
                isGroupNameValid = true;
            }
        }

        private bool ValidateForm()
        {
            bool isValid = true;

            // التحقق من اسم الشعبة
            if (string.IsNullOrWhiteSpace(GroupNameTextBox.Text))
            {
                GroupNameErrorText.Text = "اسم الشعبة مطلوب";
                GroupNameErrorText.Visibility = Visibility.Visible;
                isValid = false;
            }
            else
            {
                GroupNameErrorText.Visibility = Visibility.Collapsed;
            }

            return isValid;
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            if (!ValidateForm())
                return;

            try
            {
                List<SqlParameter> parameters = new List<SqlParameter>()
                {
                    new SqlParameter("@GroupName", GroupNameTextBox.Text),
                    new SqlParameter("@AdditionalInfo",
                        string.IsNullOrWhiteSpace(AdditionalInfoTextBox.Text) ?
                        (object)DBNull.Value : AdditionalInfoTextBox.Text)
                };

                // استدعاء الإجراء المخزن
                bool success = SqlExec.Exec_proc("InsertNewGroup", parameters);

                if (success)
                {
                    MessageBox.Show($"تم إضافة الشعبة بنجاح!\n" +
                                   $"اسم الشعبة: {GroupNameTextBox.Text.Trim()}",
                                   "نجاح", MessageBoxButton.OK, MessageBoxImage.Information);

                    ClearForm();
                }
                else
                {
                    MessageBox.Show("حدث خطأ أثناء حفظ البيانات", "خطأ", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"حدث خطأ: {ex.Message}", "خطأ", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ClearForm()
        {
            GroupNameTextBox.Text = "";
            AdditionalInfoTextBox.Text = "";

            // إخفاء رسائل الخطأ
            GroupNameErrorText.Visibility = Visibility.Collapsed;

            // إعادة تعيين متغيرات التحقق
            isGroupNameValid = false;

            // تحديث حالة زر الحفظ
            UpdateSaveButtonState();

            FormStatusBorder.Visibility = Visibility.Collapsed;
        }

        private void ClearButton_Click(object sender, RoutedEventArgs e)
        {
            ClearForm();
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            var result = MessageBox.Show(
                "هل أنت متأكد من إلغاء إضافة الشعبة؟\nسيتم فقدان جميع البيانات المدخلة.",
                "تأكيد الإلغاء",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                var mainWindow = Window.GetWindow(this) as AdminDashboard;
                if (mainWindow != null)
                {
                    // mainWindow.LoadPage("ViewAllGroups");
                }
            }
        }

        private void ShowFormStatus(string message, string icon = "⚠️", string color = "#FF9800")
        {
            StatusIcon.Text = icon;
            StatusMessage.Text = message;
            StatusMessage.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString(color));
            FormStatusBorder.Visibility = Visibility.Visible;
        }

        private void HideFormStatus()
        {
            FormStatusBorder.Visibility = Visibility.Collapsed;
        }
    }
}