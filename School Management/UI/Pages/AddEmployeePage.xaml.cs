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
    /// <summary>
    /// Interaction logic for AddEmployeePage.xaml
    /// </summary>
    public partial class AddEmployeePage : UserControl
    {


        private bool isNameValid = false;
        private bool isNationalIdValid = false;
        private bool isMobilePhoneValid = false;

        public AddEmployeePage()
        {
            InitializeComponent();
            InitializeForm();
            LoadJobItems();
        }


        public class JobItem
        {
            public string Name { get; set; }
        }


        private void LoadJobItems()
        {
            List<JobItem> jobItems = new List<JobItem>
    {
        new JobItem { Name = "مدرس" },
        new JobItem { Name = "مدير مدرسة"},
        new JobItem { Name = "ناظر" },
        new JobItem { Name = "مرشد تربوي" },
        new JobItem { Name = "سكرتير" },
        new JobItem { Name = "محاسب" },
        new JobItem { Name = "أمين مكتبة" },
        new JobItem { Name = "حارس" },
        new JobItem { Name = "عامل نظافة" },
        new JobItem { Name = "سائق" },
        new JobItem { Name = "معلم مساعد" },
        new JobItem { Name = "مشرف" },
        new JobItem { Name = "طبيب مدرسي" },
        new JobItem { Name = "ممرض/ممرضة" },
        new JobItem { Name = "أخصائي اجتماعي" },
        new JobItem { Name = "فني مختبر" },
        new JobItem { Name = "مدرب رياضة" },
        new JobItem { Name = "مدرس حاسوب" },
        new JobItem { Name = "مدرس لغة" },
        new JobItem { Name = "مدرس علوم" },
        new JobItem { Name = "مدرس رياضيات" },
        new JobItem { Name = "مساعد إداري" },
        new JobItem { Name = "منسق" },
        new JobItem { Name = "مراقب" },
        new JobItem { Name = "مشرف صيانة" }
    };


            foreach (var item in jobItems)
            {
                JobComboBox.Items.Add(item.Name);
            }

        }


        private void InitializeForm()
        {
            // تعيين القيم الافتراضية
            AgeValueText.Text = $"{AgeSlider.Value} سنة";

            // تحديث حالة زر الحفظ
            UpdateSaveButtonState();
        }

        private void UpdateSaveButtonState()
        {
            // التحقق من صحة جميع الحقول الإلزامية
            bool allValid = isNameValid && isNationalIdValid  && isMobilePhoneValid;
            if (allValid)
            {
                SaveButton.IsEnabled = true;
            }
            else
            {
                SaveButton.IsEnabled = false;
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

        private void TextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (sender is TextBox textBox)
            {
                switch (textBox.Name)
                {
                    case "EmployeeNameTextBox":
                        ValidateName();
                        break;
                    case "NationalIdTextBox":
                        ValidateNationalId();
                        break;
                    case "MobilePhoneTextBox":
                        ValidateMobilePhone();
                        break;
                }
            }
            UpdateSaveButtonState();
        }

        private void ValidateName()
        {
            string name = EmployeeNameTextBox.Text.Trim();

            if (string.IsNullOrWhiteSpace(name))
            {
                ShowError(NameErrorText, "الرجاء إدخال اسم الموظف");
                isNameValid = false;
            }
            else if (name.Length < 3)
            {
                ShowError(NameErrorText, "اسم الموظف يجب أن يكون 3 أحرف على الأقل");
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
            else if (mobilePhone.Length != 10 && mobilePhone.Length != 9)
            {
                ShowError(MobilePhoneErrorText, "رقم الهاتف يجب أن يكون 9 أو 10 أرقام");
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

        private void AgeSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (AgeValueText is not null)
            {
                AgeValueText.Text = AgeSlider.Value.ToString();
            }
        }

        private void NationalIdTextBox_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            // السماح فقط بالأرقام
            Regex regex = new Regex(@"^[0-9]+$");
            e.Handled = !regex.IsMatch(e.Text);
        }

        private void PhoneTextBox_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            // السماح فقط بالأرقام
            Regex regex = new Regex(@"^[0-9]+$");
            e.Handled = !regex.IsMatch(e.Text);
        }
        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            var result = MessageBox.Show(
                "هل أنت متأكد من إلغاء إضافة الموظف؟\nسيتم فقدان جميع البيانات المدخلة.",
                "تأكيد الإلغاء",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                // العودة إلى الصفحة الرئيسية
                var mainWindow = Window.GetWindow(this) as AdminDashboard;
                if (mainWindow != null)
                {
                    //  mainWindow.LoadPage("ViewAllEmployees");
                }
            }
        }

        // دالة المساعدة للتحقق من صحة رقم الهاتف
        private bool IsValidPhoneNumber(string phone)
        {
            if (string.IsNullOrWhiteSpace(phone))
                return false;

            // التحقق من أن الرقم يحتوي على أرقام فقط
            return Regex.IsMatch(phone, @"^\d+$");
        }

        // دالة المساعدة للتحقق من صحة الاسم
        private bool IsValidName(string name)
        {
            if (string.IsNullOrWhiteSpace(name) || name.Length < 3)
                return false;

            // التحقق من أن الاسم يحتوي على أحرف عربية فقط
            return Regex.IsMatch(name, @"^[\p{IsArabic}\s]+$");
        }

        // دالة المساعدة للتحقق من صحة الرقم الوطني
        private bool IsValidNationalId(string nationalId)
        {
            if (string.IsNullOrWhiteSpace(nationalId) || nationalId.Length != 14)
                return false;

            // التحقق من أن الرقم الوطني يحتوي على أرقام فقط
            return Regex.IsMatch(nationalId, @"^\d+$");
        }
        // إضافة حدث التحقق من إدخال الراتب
        private void SalaryTextBox_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            // السماح فقط بالأرقام
            foreach (char c in e.Text)
            {
                if (!char.IsDigit(c))
                {
                    e.Handled = true;
                    return;
                }
            }
        }

        // تحديث دالة ValidateForm لإضافة التحقق من الراتب
        private bool ValidateForm()
        {
            bool isValid = true;

            // التحقق من اسم الموظف
            if (string.IsNullOrWhiteSpace(EmployeeNameTextBox.Text))
            {
                NameErrorText.Text = "اسم الموظف مطلوب";
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

            // التحقق من المهنة
            if (JobComboBox.SelectedItem == null)
            {
                JobErrorText.Text = "المهنة مطلوبة";
                JobErrorText.Visibility = Visibility.Visible;
                isValid = false;
            }
            else
            {
                JobErrorText.Visibility = Visibility.Collapsed;
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

            // التحقق من اسم المستخدم
            if (string.IsNullOrWhiteSpace(UsernameTextBox.Text))
            {
                UsernameErrorText.Text = "اسم المستخدم مطلوب";
                UsernameErrorText.Visibility = Visibility.Visible;
                isValid = false;
            }
            else if (UsernameTextBox.Text.Length < 3)
            {
                UsernameErrorText.Text = "اسم المستخدم يجب أن يكون 3 أحرف على الأقل";
                UsernameErrorText.Visibility = Visibility.Visible;
                isValid = false;
            }
            else
            {
                UsernameErrorText.Visibility = Visibility.Collapsed;
            }

            // التحقق من كلمة المرور
            if (string.IsNullOrEmpty(PasswordBox.Password))
            {
                PasswordErrorText.Text = "كلمة المرور مطلوبة";
                PasswordErrorText.Visibility = Visibility.Visible;
                isValid = false;
            }
            else if (PasswordBox.Password.Length < 6)
            {
                PasswordErrorText.Text = "كلمة المرور يجب أن تكون 6 أحرف على الأقل";
                PasswordErrorText.Visibility = Visibility.Visible;
                isValid = false;
            }
            else
            {
                PasswordErrorText.Visibility = Visibility.Collapsed;
            }

            // التحقق من الراتب
            if (string.IsNullOrWhiteSpace(SalaryTextBox.Text))
            {
                SalaryErrorText.Text = "الراتب مطلوب";
                SalaryErrorText.Visibility = Visibility.Visible;
                isValid = false;
            }
            else if (!int.TryParse(SalaryTextBox.Text, out int salary) || salary <= 0)
            {
                SalaryErrorText.Text = "الراتب يجب أن يكون رقمًا موجبًا";
                SalaryErrorText.Visibility = Visibility.Visible;
                isValid = false;
            }
            else
            {
                SalaryErrorText.Visibility = Visibility.Collapsed;
            }

            return isValid;
        }

        // تحديث دالة ClearButton_Click لمسح حقل الراتب
        private void ClearButton_Click(object sender, RoutedEventArgs e)
        {
            EmployeeNameTextBox.Text = "";
            NationalIdTextBox.Text = "";
            JobComboBox.SelectedIndex = -1;
            AgeSlider.Value = 25;
            MobilePhoneTextBox.Text = "";
            UsernameTextBox.Text = "";
            PasswordBox.Password = "";
            SalaryTextBox.Text = "";
            AdditionalInfoTextBox.Text = "";

            // إخفاء رسائل الخطأ
            NameErrorText.Visibility = Visibility.Collapsed;
            NationalIdErrorText.Visibility = Visibility.Collapsed;
            JobErrorText.Visibility = Visibility.Collapsed;
            MobilePhoneErrorText.Visibility = Visibility.Collapsed;
            UsernameErrorText.Visibility = Visibility.Collapsed;
            PasswordErrorText.Visibility = Visibility.Collapsed;
            SalaryErrorText.Visibility = Visibility.Collapsed;

            FormStatusBorder.Visibility = Visibility.Collapsed;
        }

        // تحديث دالة SaveButton_Click
        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // التحقق من صحة الحقول الإلزامية
                if (!ValidateForm())
                    return;

                // استخدام سلسلة الاتصال الحالية
                string connectionString = "";

                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();

                    // إعداد المعاملات للإجراء المخزن
                    using (SqlCommand command = new SqlCommand("InsertNewEmployee", connection))
                    {
                        command.CommandType = CommandType.StoredProcedure;

                        command.Parameters.AddWithValue("@EmployeeName", EmployeeNameTextBox.Text.Trim());
                        command.Parameters.AddWithValue("@Username", UsernameTextBox.Text.Trim());
                        command.Parameters.AddWithValue("@Password", PasswordBox.Password);
                        command.Parameters.AddWithValue("@NationalNumber", NationalIdTextBox.Text.Trim());
                        command.Parameters.AddWithValue("@JobTitle", ((JobItem)JobComboBox.SelectedItem).Name);
                        command.Parameters.AddWithValue("@Age", int.Parse(AgeValueText.Text.Replace(" سنة", "")));
                        command.Parameters.AddWithValue("@PhoneNumber", "+963" + MobilePhoneTextBox.Text.Trim());
                        command.Parameters.AddWithValue("@Salary", int.Parse(SalaryTextBox.Text));
                        command.Parameters.AddWithValue("@HireDate", DateTime.Today);

                        // تنفيذ الإجراء المخزن
                        var result = command.ExecuteScalar();

                        MessageBox.Show($"تم إضافة الموظف بنجاح!\n" +
                                      $"اسم الموظف: {EmployeeNameTextBox.Text.Trim()}\n" +
                                      $"اسم المستخدم: {UsernameTextBox.Text.Trim()}\n" +
                                      $"الراتب: {SalaryTextBox.Text} ل.س",
                                      "نجاح", MessageBoxButton.OK, MessageBoxImage.Information);

                        ClearButton_Click(sender, e);
                    }
                }
            }
            catch (SqlException sqlEx)
            {
                // إذا كان الخطأ بسبب اسم مستخدم مكرر
                if (sqlEx.Message.Contains("UNIQUE") && sqlEx.Message.Contains("Username"))
                {
                    UsernameErrorText.Text = "اسم المستخدم هذا مستخدم بالفعل";
                    UsernameErrorText.Visibility = Visibility.Visible;
                }
                else if (sqlEx.Message.Contains("UNIQUE") && sqlEx.Message.Contains("NationalNumber"))
                {
                    NationalIdErrorText.Text = "الرقم الوطني هذا مسجل بالفعل";
                    NationalIdErrorText.Visibility = Visibility.Visible;
                }
                else
                {
                    MessageBox.Show($"خطأ في قاعدة البيانات: {sqlEx.Message}",
                                  "خطأ", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"حدث خطأ: {ex.Message}",
                              "خطأ", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}