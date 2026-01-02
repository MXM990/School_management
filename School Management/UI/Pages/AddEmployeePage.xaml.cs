using System;
using System.Collections.Generic;
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
        // كائن لتمثيل الوظائف المتاحة
        public class JobItem
        {
            public string Icon { get; set; }
            public string Name { get; set; }
            public string Value { get; set; }
        }

        // متغيرات التحقق من الصحة
        private bool isNameValid = false;
        private bool isNationalIdValid = false;
        private bool isJobValid = false;
        private bool isMobilePhoneValid = false;

        public AddEmployeePage()
        {
            InitializeComponent();
            InitializeJobs();
            InitializeForm();
        }

        private void InitializeJobs()
        {
            // قائمة الوظائف المتاحة
            var jobs = new List<JobItem>
            {
                new JobItem { Icon = "👨‍💼", Name = "مدير", Value = "Manager" },
                new JobItem { Icon = "👨‍💻", Name = "مبرمج", Value = "Programmer" },
                new JobItem { Icon = "👨‍🏫", Name = "مدرس", Value = "Teacher" },
                new JobItem { Icon = "👨‍🔧", Name = "فني", Value = "Technician" },
                new JobItem { Icon = "👨‍⚕️", Name = "طبيب", Value = "Doctor" },
                new JobItem { Icon = "👨‍🔬", Name = "مهندس", Value = "Engineer" },
                new JobItem { Icon = "👨‍✈️", Name = "طيار", Value = "Pilot" },
                new JobItem { Icon = "👨‍🚀", Name = "موظف إداري", Value = "Administrative" },
                new JobItem { Icon = "👨‍🍳", Name = "طباخ", Value = "Cook" },
                new JobItem { Icon = "👨‍🌾", Name = "مزارع", Value = "Farmer" },
                new JobItem { Icon = "👨‍🚒", Name = "رجل إطفاء", Value = "Firefighter" },
                new JobItem { Icon = "👨‍🚀", Name = "عامل", Value = "Worker" },
                new JobItem { Icon = "💼", Name = "موظف", Value = "Employee" }
            };

            JobComboBox.ItemsSource = jobs;
            JobComboBox.SelectedIndex = 0;
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
            bool allValid = isNameValid && isNationalIdValid && isJobValid && isMobilePhoneValid;
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
            else if (!Regex.IsMatch(name, @"^[\p{IsArabic}\s]+$"))
            {
                ShowError(NameErrorText, "يرجى إدخال اسم عربي صحيح");
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

        private void JobComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (JobComboBox.SelectedItem != null)
            {
                isJobValid = true;
            }
            else
            {
                isJobValid = false;
                ShowError(JobErrorText, "الرجاء اختيار المهنة");
            }
            UpdateSaveButtonState();
        }

        private void AgeSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
          //  AgeValueText.Text = $"{(int)AgeSlider.Value} سنة";
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

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            // إخفاء أي رسائل حالة سابقة
            HideFormStatus();

            try
            {
                // إنشاء معرف فريد GUID (غير معروض في الواجهة)
                Guid employeeId = Guid.NewGuid();

                // جمع بيانات الموظف
                var employeeData = new
                {
                    Id = employeeId,
                    Name = EmployeeNameTextBox.Text.Trim(),
                    NationalId = NationalIdTextBox.Text.Trim(),
                    Job = (JobComboBox.SelectedItem as JobItem)?.Name,
                    JobValue = (JobComboBox.SelectedItem as JobItem)?.Value,
                    Age = (int)AgeSlider.Value,
                    MobilePhone = MobilePhoneTextBox.Text.Trim(),
                    LandlinePhone = LandlinePhoneTextBox.Text.Trim(),
                    AdditionalInfo = AdditionalInfoTextBox.Text.Trim(),
                    CreatedDate = DateTime.Now
                };

                // هنا يمكنك إضافة كود لحفظ البيانات في قاعدة البيانات
                // SaveToDatabase(employeeData);

                // عرض رسالة نجاح
                ShowFormStatus($"✅ تم إضافة الموظف '{employeeData.Name}' بنجاح!\nتم إنشاء المعرف الفريد: {employeeId}", "✅", "#4CAF50");

                // تعطيل زر الحفظ مؤقتاً
                SaveButton.IsEnabled = false;

                // إمكانية عرض بيانات الموظف في MessageBox
                var result = MessageBox.Show(
                    $"تم إضافة الموظف بنجاح!\n\n" +
                    $"المعرف: {employeeId}\n" +
                    $"الاسم: {employeeData.Name}\n" +
                    $"الرقم الوطني: {employeeData.NationalId}\n" +
                    $"المهنة: {employeeData.Job}\n" +
                    $"العمر: {employeeData.Age} سنة\n" +
                    $"رقم النقال: {employeeData.MobilePhone}\n\n" +
                    "هل تريد إضافة موظف آخر؟",
                    "نجاح الإضافة",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Information);

                if (result == MessageBoxResult.Yes)
                {
                    ClearButton_Click(sender, e);
                }
            }
            catch (Exception ex)
            {
                ShowFormStatus($"❌ حدث خطأ أثناء حفظ البيانات: {ex.Message}", "❌", "#F44336");
                MessageBox.Show($"حدث خطأ: {ex.Message}", "خطأ", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ClearButton_Click(object sender, RoutedEventArgs e)
        {
            // مسح جميع الحقول
            EmployeeNameTextBox.Text = "";
            NationalIdTextBox.Text = "";
            JobComboBox.SelectedIndex = 0;
            AgeSlider.Value = 25;
            MobilePhoneTextBox.Text = "";
            LandlinePhoneTextBox.Text = "";
            AdditionalInfoTextBox.Text = "";

            // إخفاء رسائل الخطأ
            HideError(NameErrorText);
            HideError(NationalIdErrorText);
            HideError(MobilePhoneErrorText);
            HideFormStatus();

            // إعادة تعيين متغيرات التحقق
            isNameValid = false;
            isNationalIdValid = false;
            isMobilePhoneValid = false;
            isJobValid = true; // لأن لدينا قيمة افتراضية

            // تحديث حالة زر الحفظ
            UpdateSaveButtonState();

            // إعادة التركيز على أول حقل
            EmployeeNameTextBox.Focus();
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
    }
}