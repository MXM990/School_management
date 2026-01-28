using School_Management.Control;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace School_Management.UI.Pages
{
    public partial class AddClassPage : UserControl
    {
        private bool isEducationLevelValid = false;

        public AddClassPage()
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
            SaveButton.IsEnabled = isEducationLevelValid;
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

        private void EducationLevelText_TextChanged(object sender, TextChangedEventArgs e)
        {
            ValidateEducationLevel();
            UpdateSaveButtonState();
        }

        private void ValidateEducationLevel()
        {
            string educationLevel = EducationLevelText.Text.Trim();

            if (string.IsNullOrWhiteSpace(educationLevel))
            {
                ShowError(EducationLevelErrorText, "الرجاء إدخال المرحلة التعليمية");
                isEducationLevelValid = false;
            }
            else if (educationLevel.Length < 2)
            {
                ShowError(EducationLevelErrorText, "المرحلة التعليمية يجب أن تكون حرفين على الأقل");
                isEducationLevelValid = false;
            }
            else
            {
                HideError(EducationLevelErrorText);
                isEducationLevelValid = true;
            }
        }

        

        private bool ValidateForm()
        {
            bool isValid = true;

            // التحقق من المرحلة التعليمية
            if (string.IsNullOrWhiteSpace(EducationLevelText.Text))
            {
                EducationLevelErrorText.Text = "المرحلة التعليمية مطلوبة";
                EducationLevelErrorText.Visibility = Visibility.Visible;
                isValid = false;
            }
            else
            {
                EducationLevelErrorText.Visibility = Visibility.Collapsed;
            }

            return isValid;
        }

        private void AddClassClick(object sender, RoutedEventArgs e)
        {
            if (!ValidateForm())
                return;

            try
            {
                List<SqlParameter> parameters = new List<SqlParameter>()
                {
                    new SqlParameter("@EducationLevel", EducationLevelText.Text),
                    new SqlParameter("@AdditionalInfo",
                        string.IsNullOrWhiteSpace(AdditionalInfoTextBox.Text) ?
                        (object)DBNull.Value : AdditionalInfoTextBox.Text),
                    new SqlParameter("@CreatedDate", DateTime.Now)
                };

                // استدعاء الإجراء المخزن
                bool success = SqlExec.Exec_proc("InsertNewClass", parameters);

                if (success)
                {
                    MessageBox.Show($"تم إضافة الصف بنجاح!\n" +
                                   $"المرحلة التعليمية: {EducationLevelText.Text.Trim()}",
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
            EducationLevelText.Text = "";
            AdditionalInfoTextBox.Text = "";

            // إخفاء رسائل الخطأ
            EducationLevelErrorText.Visibility = Visibility.Collapsed;

            // إعادة تعيين متغيرات التحقق
            isEducationLevelValid = false;

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
                "هل أنت متأكد من إلغاء إضافة الصف؟\nسيتم فقدان جميع البيانات المدخلة.",
                "تأكيد الإلغاء",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                var mainWindow = Window.GetWindow(this) as AdminDashboard;
                if (mainWindow != null)
                {
                    // mainWindow.LoadPage("ViewAllClasses");
                }
            }
        }

    }
}