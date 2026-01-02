using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using School_Management.Models;

namespace School_Management.UI.Pages
{
    public partial class AddTeacherPage : UserControl
    {
        // كائن لتمثيل الاختصاصات
        public class SpecializationItem
        {
            public string Icon { get; set; }
            public string Name { get; set; }
            public string Value { get; set; }
        }

        // متغيرات التحقق من الصحة
        private bool isNameValid = false;
        private bool isNationalIdValid = false;
        private bool isSpecializationValid = false;
        private bool isMobilePhoneValid = false;

        public AddTeacherPage()
        {
            InitializeComponent();
            InitializeSpecializations();
            InitializeForm();
        }

        private void InitializeSpecializations()
        {
            // قائمة الاختصاصات المتاحة
            var specializations = new List<SpecializationItem>
            {
                new SpecializationItem { Icon = "📐", Name = "الرياضيات", Value = "Mathematics" },
                new SpecializationItem { Icon = "🔬", Name = "العلوم", Value = "Science" },
                new SpecializationItem { Icon = "🔭", Name = "الفيزياء", Value = "Physics" },
                new SpecializationItem { Icon = "🧪", Name = "الكيمياء", Value = "Chemistry" },
                new SpecializationItem { Icon = "🧬", Name = "الأحياء", Value = "Biology" },
                new SpecializationItem { Icon = "🔄", Name = "اللغة العربية", Value = "Arabic" },
                new SpecializationItem { Icon = "🇬🇧", Name = "اللغة الإنجليزية", Value = "English" },
                new SpecializationItem { Icon = "🇫🇷", Name = "اللغة الفرنسية", Value = "French" },
                new SpecializationItem { Icon = "📜", Name = "التاريخ", Value = "History" },
                new SpecializationItem { Icon = "🌍", Name = "الجغرافيا", Value = "Geography" },
                new SpecializationItem { Icon = "💻", Name = "المعلوماتية", Value = "ComputerScience" },
                new SpecializationItem { Icon = "🎨", Name = "الفنون", Value = "Arts" },
                new SpecializationItem { Icon = "🎵", Name = "الموسيقى", Value = "Music" },
                new SpecializationItem { Icon = "⚽", Name = "التربية الرياضية", Value = "PhysicalEducation" },
                new SpecializationItem { Icon = "📊", Name = "الإحصاء", Value = "Statistics" }
            };

            SpecializationComboBox.ItemsSource = specializations;
            SpecializationComboBox.SelectedIndex = 0;
        }

        private void InitializeForm()
        {
            // تعيين القيم الافتراضية
            AgeValueText.Text = $"{AgeSlider.Value} سنة";
            ExperienceValueText.Text = $"{ExperienceSlider.Value} سنوات";

            // تحديث حالة زر الحفظ
            UpdateSaveButtonState();
        }

        private void UpdateSaveButtonState()
        {
            // التحقق من صحة جميع الحقول الإلزامية
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
                    case "TeacherNameTextBox":
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

        private void SpecializationComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (SpecializationComboBox.SelectedItem != null)
            {
                isSpecializationValid = true;
                HideError(SpecializationErrorText);
            }
            else
            {
                isSpecializationValid = false;
                ShowError(SpecializationErrorText, "الرجاء اختيار الاختصاص");
            }
            UpdateSaveButtonState();
        }

        private void AgeSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
         //   AgeValueText.Text = $"{(int)AgeSlider.Value} سنة";
        }

        private void ExperienceSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
         //   ExperienceValueText.Text = $"{(int)ExperienceSlider.Value} سنوات";
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

        private void SalaryTextBox_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            // السماح فقط بالأرقام والنقطة
            Regex regex = new Regex(@"^[0-9\.]+$");
            e.Handled = !regex.IsMatch(e.Text);
        }

        private void SalaryTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            // تحقق من صحة الراتب إذا تم إدخاله
            string salaryText = SalaryTextBox.Text.Trim();
            if (!string.IsNullOrWhiteSpace(salaryText))
            {
                if (!decimal.TryParse(salaryText, out decimal salary) || salary < 0)
                {
                    ShowFormStatus("الرجاء إدخال قيمة راتب صحيحة", "⚠️", "#FF9800");
                }
                else
                {
                    HideFormStatus();
                }
            }
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            // إخفاء أي رسائل حالة سابقة
            HideFormStatus();

            try
            {
                // إنشاء معرف فريد GUID
                Guid teacherId = Guid.NewGuid();

                // جمع بيانات المدرس
                var teacher = new Teacher
                {
                    Id = teacherId,
                    Name = TeacherNameTextBox.Text.Trim(),
                    NationalId = NationalIdTextBox.Text.Trim(),
                    Specialization = (SpecializationComboBox.SelectedItem as SpecializationItem)?.Name,
                    Age = (int)AgeSlider.Value,
                    MobilePhone = MobilePhoneTextBox.Text.Trim(),
                    LandlinePhone = LandlinePhoneTextBox.Text.Trim(),
                    ExperienceYears = (int)ExperienceSlider.Value,
                    CreatedDate = DateTime.Now
                };

                // معالجة الراتب إذا تم إدخاله
                if (!string.IsNullOrWhiteSpace(SalaryTextBox.Text.Trim()))
                {
                    if (decimal.TryParse(SalaryTextBox.Text.Trim(), out decimal salary))
                    {
                        teacher.Salary = salary;
                    }
                }

                // التحقق من صحة بيانات المدرس
                if (!teacher.IsValid())
                {
                    ShowFormStatus("❌ يرجى التحقق من صحة جميع البيانات المدخلة", "❌", "#F44336");
                    return;
                }

                // هنا يمكنك إضافة كود لحفظ البيانات في قاعدة البيانات
                // SaveToDatabase(teacher);

                // عرض رسالة نجاح
                ShowFormStatus($"✅ تم إضافة المدرس '{teacher.Name}' بنجاح!\nتم إنشاء المعرف الفريد: {teacherId}", "✅", "#4CAF50");

                // تعطيل زر الحفظ مؤقتاً
                SaveButton.IsEnabled = false;

                // إمكانية عرض بيانات المدرس في MessageBox
                var salaryInfo = teacher.Salary.HasValue ? $"{teacher.Salary.Value:N0} ل.س" : "غير محدد";
                var result = MessageBox.Show(
                    $"تم إضافة المدرس بنجاح!\n\n" +
                    $"المعرف: {teacher.Id}\n" +
                    $"الاسم: {teacher.Name}\n" +
                    $"الرقم الوطني: {teacher.NationalId}\n" +
                    $"الاختصاص: {teacher.Specialization}\n" +
                    $"العمر: {teacher.Age} سنة\n" +
                    $"سنوات الخبرة: {teacher.ExperienceYears}\n" +
                    $"رقم النقال: {teacher.MobilePhone}\n" +
                    $"الراتب: {salaryInfo}\n\n" +
                    "هل تريد إضافة مدرس آخر؟",
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
            TeacherNameTextBox.Text = "";
            NationalIdTextBox.Text = "";
            SpecializationComboBox.SelectedIndex = 0;
            AgeSlider.Value = 30;
            ExperienceSlider.Value = 5;
            MobilePhoneTextBox.Text = "";
            LandlinePhoneTextBox.Text = "";
            SalaryTextBox.Text = "";
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
            isSpecializationValid = true; // لأن لدينا قيمة افتراضية

            // تحديث حالة زر الحفظ
            UpdateSaveButtonState();

            // إعادة التركيز على أول حقل
            TeacherNameTextBox.Focus();
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
                // العودة إلى الصفحة الرئيسية أو قائمة المدرسين
                var mainWindow = Window.GetWindow(this) as AdminDashboard;
                if (mainWindow != null)
                {
                   // mainWindow.LoadPage("ViewAllTeachers");
                }
            }
        }

        // كلاس التحقق من صحة الراتب (اختياري)
        public class SalaryValidationRule : ValidationRule
        {
            public override ValidationResult Validate(object value, System.Globalization.CultureInfo cultureInfo)
            {
                string salaryText = value as string;

                if (string.IsNullOrWhiteSpace(salaryText))
                {
                    return ValidationResult.ValidResult; // الراتب اختياري
                }

                if (!decimal.TryParse(salaryText, out decimal salary))
                {
                    return new ValidationResult(false, "الرجاء إدخال قيمة رقمية صحيحة");
                }

                if (salary < 0)
                {
                    return new ValidationResult(false, "الراتب لا يمكن أن يكون سالباً");
                }

                return ValidationResult.ValidResult;
            }
        }
    }
}