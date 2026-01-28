using School_Management.Control;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace School_Management.UI.Pages
{
    public partial class AddTeacherPage : UserControl
    {
        private bool isNameValid = false;
        private bool isNationalIdValid = false;
        private bool isSpecializationValid = false;
        private bool isMobilePhoneValid = false;

        public AddTeacherPage()
        {
            InitializeComponent();
            InitializeForm();
        }

        private void InitializeForm()
        {
            AgeValueText.Text = $"{(int)AgeSlider.Value}";
            ExperienceValueText.Text = $"{(int)ExperienceSlider.Value}";
            UpdateSaveButtonState();
        }

        private void UpdateSaveButtonState()
        {
            bool allValid = isNameValid && isNationalIdValid && isSpecializationValid && isMobilePhoneValid;
            SaveButton.IsEnabled = allValid;
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

        private void TextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (sender is TextBox textBox)
            {
                switch (textBox.Name)
                {
                    case "TeacherNameTextBox":
                        ValidateName();
                        break;
                    case "NationalIdTextBox":
                        ValidateNationalId();
                        break;
                    case "MobilePhoneTextBox":
                        ValidateMobilePhone();
                        break;
                    case "SalaryTextBox":
                        ValidateSalary();
                        break;
                }
            }
            UpdateSaveButtonState();
        }

        private void ValidateName()
        {
            string name = TeacherNameTextBox.Text.Trim();

            if (string.IsNullOrWhiteSpace(name))
            {
                ShowError(NameErrorText, "الرجاء إدخال اسم المدرس");
                isNameValid = false;
            }
            else if (name.Length < 3)
            {
                ShowError(NameErrorText, "اسم المدرس يجب أن يكون 3 أحرف على الأقل");
                isNameValid = false;
            }
            else
            {
                HideError(NameErrorText);
                isNameValid = true;
            }
        }

        private void ValidateNationalId()
        {
            string nationalId = NationalIdTextBox.Text.Trim();

            if (string.IsNullOrWhiteSpace(nationalId))
            {
                ShowError(NationalIdErrorText, "الرجاء إدخال الرقم الوطني");
                isNationalIdValid = false;
            }
            else if (nationalId.Length != 14)
            {
                ShowError(NationalIdErrorText, "الرقم الوطني يجب أن يكون 14 رقماً");
                isNationalIdValid = false;
            }
            else if (!Regex.IsMatch(nationalId, @"^\d+$"))
            {
                ShowError(NationalIdErrorText, "الرقم الوطني يجب أن يحتوي على أرقام فقط");
                isNationalIdValid = false;
            }
            else
            {
                HideError(NationalIdErrorText);
                isNationalIdValid = true;
            }
        }

        private void ValidateMobilePhone()
        {
            string mobilePhone = MobilePhoneTextBox.Text.Trim();

            if (string.IsNullOrWhiteSpace(mobilePhone))
            {
                ShowError(MobilePhoneErrorText, "الرجاء إدخال رقم الهاتف النقال");
                isMobilePhoneValid = false;
            }
            else if (mobilePhone.Length != 10)
            {
                ShowError(MobilePhoneErrorText, "رقم الهاتف يجب أن يكون 10 أرقام");
                isMobilePhoneValid = false;
            }
            else if (!Regex.IsMatch(mobilePhone, @"^\d+$"))
            {
                ShowError(MobilePhoneErrorText, "رقم الهاتف يجب أن يحتوي على أرقام فقط");
                isMobilePhoneValid = false;
            }
            else
            {
                HideError(MobilePhoneErrorText);
                isMobilePhoneValid = true;
            }
        }

        private void ValidateSalary()
        {
            string salary = SalaryTextBox.Text.Trim();

            if (!string.IsNullOrWhiteSpace(salary))
            {
                if (!int.TryParse(salary, out int salaryValue) || salaryValue < 0)
                {
                    ShowError(SalaryErrorText, "الراتب يجب أن يكون رقمًا صحيحًا موجبًا");
                }
                else
                {
                    HideError(SalaryErrorText);
                }
            }
            else
            {
                HideError(SalaryErrorText);
            }
        }

        private void AgeSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if(AgeValueText is not null)
            {
                AgeValueText.Text = $"{(int)AgeSlider.Value}";
            }
        }

        private void ExperienceSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (ExperienceValueText is not null)
            {
                ExperienceValueText.Text = $"{(int)ExperienceSlider.Value}";
            }
        }

        private void NationalIdTextBox_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            Regex regex = new Regex(@"^[0-9]+$");
            e.Handled = !regex.IsMatch(e.Text);
        }

        private void PhoneTextBox_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            Regex regex = new Regex(@"^[0-9]+$");
            e.Handled = !regex.IsMatch(e.Text);
        }

        private void SalaryTextBox_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            foreach (char c in e.Text)
            {
                if (!char.IsDigit(c))
                {
                    e.Handled = true;
                    return;
                }
            }
        }

        private void Specializationtext_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (!string.IsNullOrWhiteSpace(Specializationtext.Text))
            {
                isSpecializationValid = true;
                HideError(SpecializationErrorText);
            }
            else
            {
                isSpecializationValid = false;
                ShowError(SpecializationErrorText, "الرجاء إدخال الاختصاص");
            }
            UpdateSaveButtonState();
        }

        private bool ValidateForm()
        {
            bool isValid = true;

            // التحقق من اسم المدرس
            if (string.IsNullOrWhiteSpace(TeacherNameTextBox.Text))
            {
                NameErrorText.Text = "اسم المدرس مطلوب";
                NameErrorText.Visibility = Visibility.Visible;
                isValid = false;
            }
            else
            {
                NameErrorText.Visibility = Visibility.Collapsed;
            }

            // التحقق من الرقم الوطني
            if (string.IsNullOrWhiteSpace(NationalIdTextBox.Text))
            {
                NationalIdErrorText.Text = "الرقم الوطني مطلوب";
                NationalIdErrorText.Visibility = Visibility.Visible;
                isValid = false;
            }
            else if (NationalIdTextBox.Text.Length != 14)
            {
                NationalIdErrorText.Text = "الرقم الوطني يجب أن يكون 14 رقمًا";
                NationalIdErrorText.Visibility = Visibility.Visible;
                isValid = false;
            }
            else
            {
                NationalIdErrorText.Visibility = Visibility.Collapsed;
            }

            // التحقق من الاختصاص
            if (string.IsNullOrWhiteSpace(Specializationtext.Text))
            {
                SpecializationErrorText.Text = "الاختصاص مطلوب";
                SpecializationErrorText.Visibility = Visibility.Visible;
                isValid = false;
            }
            else
            {
                SpecializationErrorText.Visibility = Visibility.Collapsed;
            }

            // التحقق من الهاتف النقال
            if (string.IsNullOrWhiteSpace(MobilePhoneTextBox.Text))
            {
                MobilePhoneErrorText.Text = "رقم الهاتف النقال مطلوب";
                MobilePhoneErrorText.Visibility = Visibility.Visible;
                isValid = false;
            }
            else if (MobilePhoneTextBox.Text.Length != 10)
            {
                MobilePhoneErrorText.Text = "رقم الهاتف يجب أن يكون 10 أرقام";
                MobilePhoneErrorText.Visibility = Visibility.Visible;
                isValid = false;
            }
            else
            {
                MobilePhoneErrorText.Visibility = Visibility.Collapsed;
            }

            // التحقق من الراتب (اختياري)
            if (!string.IsNullOrWhiteSpace(SalaryTextBox.Text))
            {
                if (!int.TryParse(SalaryTextBox.Text, out int salary) || salary <= 0)
                {
                    SalaryErrorText.Text = "الراتب يجب أن يكون رقمًا صحيحًا موجبًا";
                    SalaryErrorText.Visibility = Visibility.Visible;
                    isValid = false;
                }
                else
                {
                    SalaryErrorText.Visibility = Visibility.Collapsed;
                }
            }

            return isValid;
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            if (!ValidateForm())
                return;

            // التحقق من صحة الراتب إذا تم إدخاله
            int? salaryValue = null;
            if (!string.IsNullOrWhiteSpace(SalaryTextBox.Text))
            {
                if (int.TryParse(SalaryTextBox.Text, out int salary))
                {
                    salaryValue = salary;
                }
            }

            List<SqlParameter> parameters = new List<SqlParameter>()
            {
                new SqlParameter("@TeacherName", TeacherNameTextBox.Text),
                new SqlParameter("@NationalNumber", NationalIdTextBox.Text),
                new SqlParameter("@Specialization", Specializationtext.Text),
                new SqlParameter("@Age", (int)AgeSlider.Value),
                new SqlParameter("@PhoneNumber", MobilePhoneTextBox.Text),
                new SqlParameter("@YearsOfExperience", (int)ExperienceSlider.Value),
                new SqlParameter("@Salary", salaryValue.HasValue ? (object)salaryValue.Value : DBNull.Value),
                new SqlParameter("@HireDate", DateTime.Today)
            };

            try
            {
                // استدعاء الإجراء المخزن مع اسم الإجراء الصحيح
                bool success = SqlExec.Exec_proc("InsertNewTeacher", parameters);

                if (success)
                {
                    MessageBox.Show($"تم إضافة المدرس بنجاح!\n" +
                                   $"اسم المدرس: {TeacherNameTextBox.Text.Trim()}\n" +
                                   $"الاختصاص: {Specializationtext.Text}\n" +
                                   $"سنوات الخبرة: {ExperienceSlider.Value}\n" +
                                   (salaryValue.HasValue ? $"الراتب: {salaryValue.Value:N0} ل.س" : "الراتب: غير محدد"),
                                   "نجاح", MessageBoxButton.OK, MessageBoxImage.Information);

                    ClearButton_Click(sender, e);
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

        private void ClearButton_Click(object sender, RoutedEventArgs e)
        {
            TeacherNameTextBox.Text = "";
            NationalIdTextBox.Text = "";
            Specializationtext.Text = "";
            AgeSlider.Value = 30;
            ExperienceSlider.Value = 0;
            MobilePhoneTextBox.Text = "";
            SalaryTextBox.Text = "";
            AdditionalInfoTextBox.Text = "";

            // إخفاء رسائل الخطأ
            NameErrorText.Visibility = Visibility.Collapsed;
            NationalIdErrorText.Visibility = Visibility.Collapsed;
            SpecializationErrorText.Visibility = Visibility.Collapsed;
            MobilePhoneErrorText.Visibility = Visibility.Collapsed;
            SalaryErrorText.Visibility = Visibility.Collapsed;

            // إعادة تعيين متغيرات التحقق
            isNameValid = false;
            isNationalIdValid = false;
            isSpecializationValid = false;
            isMobilePhoneValid = false;

            // تحديث حالة زر الحفظ
            UpdateSaveButtonState();

            FormStatusBorder.Visibility = Visibility.Collapsed;
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            var result = MessageBox.Show(
                "هل أنت متأكد من إلغاء إضافة المدرس؟\nسيتم فقدان جميع البيانات المدخلة.",
                "تأكيد الإلغاء",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                var mainWindow = Window.GetWindow(this) as AdminDashboard;
                if (mainWindow != null)
                {
                    // mainWindow.LoadPage("ViewAllTeachers");
                }
            }
        }

        // إضافة دالة SalaryTextBox_TextChanged للتأكد من تحديث الرسالة
        private void SalaryTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            ValidateSalary();
            UpdateSaveButtonState();
        }
    }
}   