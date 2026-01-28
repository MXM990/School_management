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

namespace School_Management.UI.EmpPages
{
    public partial class AddStudentPage : UserControl
    {
        private bool isStudentNameValid = false;
        private bool isNationalNumberValid = false;
        private bool isAgeValid = false;
        private bool isFatherNameValid = false;
        private bool isPhoneNumberValid = false;
        private bool isRegistrationStatusValid = false;
        private bool isBirthDateValid = false;
        private bool isEmailValid = true; // اختياري، لذا افتراضي صحيح

        public AddStudentPage()
        {
            InitializeComponent();
            InitializeForm();
        }

        private void InitializeForm()
        {
            BirthDatePicker.SelectedDate = DateTime.Now.AddYears(-10);

            RegistrationStatusComboBox.SelectedIndex = 0;

            UpdateSaveButtonState();
        }

        private void UpdateSaveButtonState()
        {
            bool allValid = isStudentNameValid && isNationalNumberValid && isAgeValid &&
                          isFatherNameValid && isPhoneNumberValid && isRegistrationStatusValid &&
                          isBirthDateValid && isEmailValid;
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

        #region أحداث التحقق من الصحة

        private void TextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (sender is TextBox textBox)
            {
                switch (textBox.Name)
                {
                    case "StudentNameTextBox":
                        ValidateStudentName();
                        break;
                    case "NationalNumberTextBox":
                        ValidateNationalNumber();
                        break;
                    case "FatherNameTextBox":
                        ValidateFatherName();
                        break;
                    case "PhoneNumberTextBox":
                        ValidatePhoneNumber();
                        break;
                }
            }
            UpdateSaveButtonState();
        }

        private void AgeTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            ValidateAge();
            UpdateSaveButtonState();
        }

        private void RegistrationStatusComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ValidateRegistrationStatus();
            UpdateSaveButtonState();
        }

        private void BirthDatePicker_SelectedDateChanged(object sender, SelectionChangedEventArgs e)
        {
            ValidateBirthDate();
            UpdateSaveButtonState();
        }

        private void EmailTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            ValidateEmail();
            UpdateSaveButtonState();
        }

        #endregion

        #region دوال التحقق من الصحة

        private void ValidateStudentName()
        {
            string name = StudentNameTextBox.Text.Trim();

            if (string.IsNullOrWhiteSpace(name))
            {
                ShowError(StudentNameErrorText, "الرجاء إدخال اسم الطالب");
                isStudentNameValid = false;
            }
            else if (name.Length < 3)
            {
                ShowError(StudentNameErrorText, "اسم الطالب يجب أن يكون 3 أحرف على الأقل");
                isStudentNameValid = false;
            }
            else
            {
                HideError(StudentNameErrorText);
                isStudentNameValid = true;
            }
        }

        private void ValidateNationalNumber()
        {
            string nationalNumber = NationalNumberTextBox.Text.Trim();

            if (string.IsNullOrWhiteSpace(nationalNumber))
            {
                ShowError(NationalNumberErrorText, "الرجاء إدخال الرقم الوطني");
                isNationalNumberValid = false;
            }
            else if (nationalNumber.Length != 14)
            {
                ShowError(NationalNumberErrorText, "الرقم الوطني يجب أن يكون 14 رقماً");
                isNationalNumberValid = false;
            }
            else if (!Regex.IsMatch(nationalNumber, @"^\d+$"))
            {
                ShowError(NationalNumberErrorText, "الرقم الوطني يجب أن يحتوي على أرقام فقط");
                isNationalNumberValid = false;
            }
            else
            {
                HideError(NationalNumberErrorText);
                isNationalNumberValid = true;
            }
        }

        private void ValidateAge()
        {
            string ageText = AgeTextBox.Text.Trim();

            if (string.IsNullOrWhiteSpace(ageText))
            {
                ShowError(AgeErrorText, "الرجاء إدخال العمر");
                isAgeValid = false;
            }
            else if (!int.TryParse(ageText, out int age))
            {
                ShowError(AgeErrorText, "الرجاء إدخال عمر صحيح");
                isAgeValid = false;
            }
            else if (age < 6 || age > 18)
            {
                ShowError(AgeErrorText, "العمر يجب أن يكون بين 6 و 18 سنة");
                isAgeValid = false;
            }
            else
            {
                HideError(AgeErrorText);
                isAgeValid = true;
            }
        }

        private void ValidateFatherName()
        {
            string fatherName = FatherNameTextBox.Text.Trim();

            if (string.IsNullOrWhiteSpace(fatherName))
            {
                ShowError(FatherNameErrorText, "الرجاء إدخال اسم الأب");
                isFatherNameValid = false;
            }
            else if (fatherName.Length < 3)
            {
                ShowError(FatherNameErrorText, "اسم الأب يجب أن يكون 3 أحرف على الأقل");
                isFatherNameValid = false;
            }
            else
            {
                HideError(FatherNameErrorText);
                isFatherNameValid = true;
            }
        }

        private void ValidatePhoneNumber()
        {
            string phoneNumber = PhoneNumberTextBox.Text.Trim();

            if (string.IsNullOrWhiteSpace(phoneNumber))
            {
                ShowError(PhoneNumberErrorText, "الرجاء إدخال رقم الهاتف");
                isPhoneNumberValid = false;
            }
            else if (phoneNumber.Length != 10)
            {
                ShowError(PhoneNumberErrorText, "رقم الهاتف يجب أن يكون 10 أرقام");
                isPhoneNumberValid = false;
            }
            else if (!Regex.IsMatch(phoneNumber, @"^\d+$"))
            {
                ShowError(PhoneNumberErrorText, "رقم الهاتف يجب أن يحتوي على أرقام فقط");
                isPhoneNumberValid = false;
            }
            else
            {
                HideError(PhoneNumberErrorText);
                isPhoneNumberValid = true;
            }
        }

        private void ValidateRegistrationStatus()
        {
            if (RegistrationStatusComboBox.SelectedItem == null)
            {
                ShowError(RegistrationStatusErrorText, "الرجاء اختيار حالة التسجيل");
                isRegistrationStatusValid = false;
            }
            else
            {
                HideError(RegistrationStatusErrorText);
                isRegistrationStatusValid = true;
            }
        }

        private void ValidateBirthDate()
        {
            if (BirthDatePicker.SelectedDate == null)
            {
                ShowError(BirthDateErrorText, "الرجاء اختيار تاريخ الميلاد");
                isBirthDateValid = false;
            }
            else
            {
                DateTime birthDate = BirthDatePicker.SelectedDate.Value;
                DateTime today = DateTime.Today;
                int age = today.Year - birthDate.Year;

                if (birthDate.Date > today.AddYears(-age)) age--;

                if (age < 6 || age > 18)
                {
                    ShowError(BirthDateErrorText, "العمر من تاريخ الميلاد يجب أن يكون بين 6 و 18 سنة");
                    isBirthDateValid = false;
                }
                else
                {
                    HideError(BirthDateErrorText);
                    isBirthDateValid = true;
                }
            }
        }

        private void ValidateEmail()
        {
            string email = EmailTextBox.Text.Trim();

            if (!string.IsNullOrWhiteSpace(email))
            {
                string emailPattern = @"^[^@\s]+@[^@\s]+\.[^@\s]+$";
                if (!Regex.IsMatch(email, emailPattern))
                {
                    ShowError(EmailErrorText, "البريد الإلكتروني غير صحيح");
                    isEmailValid = false;
                }
                else
                {
                    HideError(EmailErrorText);
                    isEmailValid = true;
                }
            }
            else
            {
                HideError(EmailErrorText);
                isEmailValid = true; // اختياري
            }
        }

        #endregion

        #region أحداث PreviewTextInput للتحكم بالإدخال

        private void NationalNumberTextBox_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            Regex regex = new Regex(@"^[0-9]+$");
            e.Handled = !regex.IsMatch(e.Text);
        }

        private void AgeTextBox_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            Regex regex = new Regex(@"^[0-9]+$");
            e.Handled = !regex.IsMatch(e.Text);
        }

        private void PhoneNumberTextBox_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            Regex regex = new Regex(@"^[0-9]+$");
            e.Handled = !regex.IsMatch(e.Text);
        }

        #endregion

        #region التحقق النهائي من النموذج

        private bool ValidateForm()
        {
            bool isValid = true;

            ValidateStudentName();
            ValidateNationalNumber();
            ValidateAge();
            ValidateFatherName();
            ValidatePhoneNumber();
            ValidateRegistrationStatus();
            ValidateBirthDate();
            ValidateEmail();

            if (!isStudentNameValid || !isNationalNumberValid || !isAgeValid ||
                !isFatherNameValid || !isPhoneNumberValid || !isRegistrationStatusValid ||
                !isBirthDateValid || !isEmailValid)
            {
                isValid = false;
                ShowFormStatus("❌ يرجى التحقق من صحة جميع البيانات المدخلة", "❌", "#F44336");
            }

            return isValid;
        }

        #endregion

        #region أحداث الأزرار

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            if (!ValidateForm())
                return;

            try
            {
                // التحقق من صحة العمر من تاريخ الميلاد
                DateTime birthDate = BirthDatePicker.SelectedDate.Value;
                DateTime today = DateTime.Today;
                int ageFromBirthDate = today.Year - birthDate.Year;
                if (birthDate.Date > today.AddYears(-ageFromBirthDate)) ageFromBirthDate--;

                // إذا كان العمر المدخل يختلف عن العمر المحسوب من تاريخ الميلاد
                if (int.TryParse(AgeTextBox.Text, out int enteredAge) && enteredAge != ageFromBirthDate)
                {
                    var result = MessageBox.Show($"العمر المدخل ({enteredAge}) يختلف عن العمر المحسوب من تاريخ الميلاد ({ageFromBirthDate}). هل تريد الاستمرار؟",
                        "تنبيه", MessageBoxButton.YesNo, MessageBoxImage.Warning);

                    if (result == MessageBoxResult.No)
                        return;
                }

                // تجميع معلمات الإجراء المخزن
                List<SqlParameter> parameters = new List<SqlParameter>()
                {
                    new SqlParameter("@StudentName", StudentNameTextBox.Text),
                    new SqlParameter("@NationalNumber", NationalNumberTextBox.Text),
                    new SqlParameter("@Age", int.Parse(AgeTextBox.Text)),
                    new SqlParameter("@FatherName", FatherNameTextBox.Text),
                    new SqlParameter("@PhoneNumber", PhoneNumberTextBox.Text),
                    new SqlParameter("@RegistrationStatus",
                        (RegistrationStatusComboBox.SelectedItem as ComboBoxItem)?.Content.ToString()),
                    new SqlParameter("@Address",
                        string.IsNullOrWhiteSpace(AddressTextBox.Text) ?
                        (object)DBNull.Value : AddressTextBox.Text),
                    new SqlParameter("@Email",
                        string.IsNullOrWhiteSpace(EmailTextBox.Text) ?
                        (object)DBNull.Value : EmailTextBox.Text),
                    new SqlParameter("@BirthDate", BirthDatePicker.SelectedDate)
                };

                // استدعاء الإجراء المخزن
                bool success = SqlExec.Exec_proc("InsertNewStudent", parameters);

                if (success)
                {
                    MessageBox.Show($"تم إضافة الطالب بنجاح!\n" +
                                   $"اسم الطالب: {StudentNameTextBox.Text.Trim()}\n" +
                                   $"الرقم الوطني: {NationalNumberTextBox.Text}\n" +
                                   $"حالة التسجيل: {(RegistrationStatusComboBox.SelectedItem as ComboBoxItem)?.Content}",
                                   "نجاح", MessageBoxButton.OK, MessageBoxImage.Information);

                    ClearForm();
                }
                else
                {
                    MessageBox.Show("حدث خطأ أثناء حفظ البيانات", "خطأ",
                        MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            catch (SqlException ex)
            {
                if (ex.Message.Contains("UNIQUE"))
                {
                    MessageBox.Show("الرقم الوطني مسجل مسبقاً في النظام", "خطأ",
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
        }

        private void ClearButton_Click(object sender, RoutedEventArgs e)
        {
            ClearForm();
        }

        private void ClearForm()
        {
            // مسح جميع الحقول
            StudentNameTextBox.Text = "";
            NationalNumberTextBox.Text = "";
            AgeTextBox.Text = "";
            FatherNameTextBox.Text = "";
            PhoneNumberTextBox.Text = "";
            RegistrationStatusComboBox.SelectedIndex = 0;
            AddressTextBox.Text = "";
            EmailTextBox.Text = "";
            BirthDatePicker.SelectedDate = DateTime.Now.AddYears(-10);

            // إخفاء رسائل الخطأ
            HideError(StudentNameErrorText);
            HideError(NationalNumberErrorText);
            HideError(AgeErrorText);
            HideError(FatherNameErrorText);
            HideError(PhoneNumberErrorText);
            HideError(RegistrationStatusErrorText);
            HideError(BirthDateErrorText);
            HideError(EmailErrorText);

            // إعادة تعيين متغيرات التحقق
            isStudentNameValid = false;
            isNationalNumberValid = false;
            isAgeValid = false;
            isFatherNameValid = false;
            isPhoneNumberValid = false;
            isRegistrationStatusValid = true; // لأن لدينا قيمة افتراضية
            isBirthDateValid = true; // لأن لدينا قيمة افتراضية
            isEmailValid = true;

            // إخفاء حالة النموذج
            HideFormStatus();

            // تحديث حالة زر الحفظ
            UpdateSaveButtonState();
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            var result = MessageBox.Show(
                "هل أنت متأكد من إلغاء إضافة الطالب؟\nسيتم فقدان جميع البيانات المدخلة.",
                "تأكيد الإلغاء",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                // العودة إلى الصفحة الرئيسية للموظف
                var mainWindow = Window.GetWindow(this) as EmployeeDashboard;
                if (mainWindow != null)
                {
                  //  mainWindow.HomeButton_Click(sender, e);
                }
            }
        }



        #endregion
    }
}