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
    public partial class AddSubjectPage : UserControl
    {
        private bool isSubjectNameValid = false;
        private bool isClassValid = false;
        private List<ClassItem> classList = new List<ClassItem>();

        public AddSubjectPage()
        {
            InitializeComponent();
            InitializeForm();
        }

        private void InitializeForm()
        {
            LoadClasses();
            UpdateSaveButtonState();
        }

        private void LoadClasses()
        {
            try
            {
                classList.Clear();
                ClassComboBox.ItemsSource = null;

                string query = "SELECT ClassID, EducationLevel FROM Classes ORDER BY EducationLevel";

                if (CurrentConnection.OpenConntion())
                {
                    using (SqlCommand cmd = new SqlCommand(query, CurrentConnection.CuCon))
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            classList.Add(new ClassItem
                            {
                                ClassID = reader.GetGuid(0),
                                EducationLevel = reader.GetString(1)
                            });
                        }
                    }
                }

                ClassComboBox.ItemsSource = classList;

                if (classList.Count == 0)
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

        private void UpdateSaveButtonState()
        {
            SaveButton.IsEnabled = isSubjectNameValid && isClassValid;
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
        private void SubjectNameTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            ValidateSubjectName();
            UpdateSaveButtonState();
        }

        private void ClassComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ValidateClass();
            UpdateSaveButtonState();
        }
        #endregion

        #region دوال التحقق من الصحة
        private void ValidateSubjectName()
        {
            string subjectName = SubjectNameTextBox.Text.Trim();

            if (string.IsNullOrWhiteSpace(subjectName))
            {
                ShowError(SubjectNameErrorText, "الرجاء إدخال اسم المادة");
                isSubjectNameValid = false;
            }
            else if (subjectName.Length < 2)
            {
                ShowError(SubjectNameErrorText, "اسم المادة يجب أن يكون حرفين على الأقل");
                isSubjectNameValid = false;
            }
            else if (subjectName.Length > 100)
            {
                ShowError(SubjectNameErrorText, "اسم المادة يجب ألا يتجاوز 100 حرف");
                isSubjectNameValid = false;
            }
            else
            {
                // التحقق من عدم تكرار المادة في نفس الصف
                if (CheckSubjectExists(subjectName))
                {
                    ShowError(SubjectNameErrorText, "هذه المادة مسجلة مسبقاً في هذا الصف");
                    isSubjectNameValid = false;
                }
                else
                {
                    HideError(SubjectNameErrorText);
                    isSubjectNameValid = true;
                }
            }
        }

        private bool CheckSubjectExists(string subjectName)
        {
            try
            {
                if (ClassComboBox.SelectedValue == null)
                    return false;

                string classId = ClassComboBox.SelectedValue.ToString();

                string query = "SELECT COUNT(*) FROM Subjects WHERE SubjectName = @SubjectName AND ClassID = @ClassID";

                if (CurrentConnection.OpenConntion())
                {
                    using (SqlCommand cmd = new SqlCommand(query, CurrentConnection.CuCon))
                    {
                        cmd.Parameters.AddWithValue("@SubjectName", subjectName);
                        cmd.Parameters.AddWithValue("@ClassID", new Guid(classId));

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

        private void ValidateClass()
        {
            if (ClassComboBox.SelectedValue == null)
            {
                ShowError(ClassErrorText, "الرجاء اختيار الصف الدراسي");
                isClassValid = false;
            }
            else
            {
                HideError(ClassErrorText);
                isClassValid = true;
            }
        }
        #endregion

        #region التحقق النهائي من النموذج
        private bool ValidateForm()
        {
            bool isValid = true;

            ValidateSubjectName();
            ValidateClass();

            if (!isSubjectNameValid || !isClassValid)
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
                // تجميع معلمات الإجراء المخزن
                List<SqlParameter> parameters = new List<SqlParameter>()
                {
                    new SqlParameter("@SubjectName", SubjectNameTextBox.Text.Trim()),
                    new SqlParameter("@ClassID", new Guid(ClassComboBox.SelectedValue.ToString()))
                };

                // استدعاء الإجراء المخزن
                bool success = SqlExec.Exec_proc("InsertNewSubject", parameters);

                if (success)
                {
                    var selectedClass = ClassComboBox.SelectedItem as ClassItem;
                    string className = selectedClass?.EducationLevel ?? "غير معروف";

                    MessageBox.Show($"تم إضافة المادة بنجاح!\n" +
                                   $"اسم المادة: {SubjectNameTextBox.Text.Trim()}\n" +
                                   $"الصف: {className}",
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
                    MessageBox.Show("المادة مسجلة مسبقاً في هذا الصف", "خطأ",
                        MessageBoxButton.OK, MessageBoxImage.Error);
                }
                else if (ex.Message.Contains("FOREIGN KEY"))
                {
                    MessageBox.Show("الصف المحدد غير موجود", "خطأ",
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
            SubjectNameTextBox.Text = "";

            LoadClasses();

            HideError(SubjectNameErrorText);
            HideError(ClassErrorText);

            isSubjectNameValid = false;
            isClassValid = true; 

            HideFormStatus();

            UpdateSaveButtonState();
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            var result = MessageBox.Show(
                "هل أنت متأكد من إلغاء إضافة المادة؟\nسيتم فقدان جميع البيانات المدخلة.",
                "تأكيد الإلغاء",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                // العودة إلى الصفحة الرئيسية للموظف
                var mainWindow = Window.GetWindow(this) as EmployeeDashboard;
                if (mainWindow != null)
                {
                    // mainWindow.HomeButton_Click(sender, e);
                }
            }
        }
        #endregion
    }

    #region فئة مساعدة للصفوف
    public class ClassItem
    {
        public Guid ClassID { get; set; }
        public string EducationLevel { get; set; }

        public override string ToString()
        {
            return EducationLevel;
        }
    }
    #endregion
}