using School_Management.Control;
using School_Management.UI.EmpPages;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;


// وصلت لهون أخر شي 
// بدي ساوي بروسجير للأضافة
namespace School_Management.UI.Pages
{
    public partial class RegisterStudentSubjects : UserControl
    {
        private bool isStudentValid = false;
        private bool isClassValid = false;
        private bool isSubjectsValid = false;

        private List<StudentItemSub> studentList = new List<StudentItemSub>();
        private List<ClassItemSub> classList = new List<ClassItemSub>();
        private List<SubjectItem> subjectList = new List<SubjectItem>();

        private Guid? selectedStudentId = null;
        private Guid? selectedClassId = null;


        public RegisterStudentSubjects()
        {
            InitializeComponent();
            InitializeForm();
        }

        private void InitializeForm()
        {
            LoadStudents();
            LoadClasses();
            UpdateSaveButtonState();
        }

        private void LoadStudents()
        {
            try
            {
                studentList.Clear();
                StudentComboBox.ItemsSource = null;

                string query = "SELECT StudentID, StudentName FROM Students ORDER BY StudentName";

                if (CurrentConnection.OpenConntion())
                {
                    using (SqlCommand cmd = new SqlCommand(query, CurrentConnection.CuCon))
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            studentList.Add(new StudentItemSub
                            {
                                StudentID = reader.GetGuid(0),
                                FullName = reader.GetString(1)
                            });
                        }
                    }
                }

                StudentComboBox.ItemsSource = studentList;

                if (studentList.Count == 0)
                {
                    ShowError(StudentErrorText, "لا يوجد طلاب في النظام");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"حدث خطأ أثناء تحميل الطلاب: {ex.Message}",
                    "خطأ", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                CurrentConnection.CloseConntion();
            }
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
                            classList.Add(new ClassItemSub
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
                    ShowError(ClassErrorText, "لا يوجد صفوف في النظام");
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

        private void LoadSubjects(Guid classId)
        {
            try
            {
                subjectList.Clear();
                SubjectsListView.ItemsSource = null;

                string query = "SELECT SubjectID, SubjectName FROM Subjects WHERE ClassID = @ClassID";

                if (CurrentConnection.OpenConntion())
                {
                    using (SqlCommand cmd = new SqlCommand(query, CurrentConnection.CuCon))
                    {
                        cmd.Parameters.AddWithValue("@ClassID", classId);

                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                subjectList.Add(new SubjectItem
                                {
                                    SubjectID = reader.GetGuid(0),
                                    SubjectName = reader.GetString(1),
                                    IsSelected = false
                                });
                            }
                        }
                    }
                }

                SubjectsListView.ItemsSource = subjectList;

                if (subjectList.Count == 0)
                {
                    ShowError(SubjectsErrorText, "لا توجد مواد مسجلة لهذا الصف");
                    isSubjectsValid = false;
                }
                else
                {
                    HideError(SubjectsErrorText);
                    isSubjectsValid = true;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"حدث خطأ أثناء تحميل المواد: {ex.Message}",
                    "خطأ", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                CurrentConnection.CloseConntion();
            }
        }

        private void UpdateSaveButtonState()
        {
            SaveButton.IsEnabled = isStudentValid && isClassValid && isSubjectsValid;
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
        private void StudentComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (StudentComboBox.SelectedItem is StudentItemSub selectedStudent)
            {
                selectedStudentId = selectedStudent.StudentID;
            }
            ValidateStudent();
            UpdateSaveButtonState();
        }

        private void ClassComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (ClassComboBox.SelectedItem is ClassItemSub selectedClass)
            {
                selectedClassId = selectedClass.ClassID;
            }
                ValidateClass();
            UpdateSaveButtonState();

            // إذا كان الصف صحيحًا، قم بتحميل المواد
            if (isClassValid)
            {
                var selectedClass_view = ClassComboBox.SelectedItem as ClassItemSub;
                LoadSubjects(selectedClass_view.ClassID);
            }
        }
        #endregion

        #region دوال التحقق من الصحة
        private void ValidateStudent()
        {
            if (StudentComboBox.SelectedValue == null)
            {
                ShowError(StudentErrorText, "الرجاء اختيار الطالب");
                isStudentValid = false;
            }
            else
            {
                HideError(StudentErrorText);
                isStudentValid = true;
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

        private void ValidateSubjects()
        {
            // تحقق من أنه تم تحديد مادة واحدة على الأقل
            bool atLeastOneSelected = false;
            foreach (SubjectItem subject in subjectList)
            {
                if (subject.IsSelected)
                {
                    atLeastOneSelected = true;
                    break;
                }
            }

            if (!atLeastOneSelected)
            {
                ShowError(SubjectsErrorText, "الرجاء اختيار مادة واحدة على الأقل");
                isSubjectsValid = false;
            }
            else
            {
                HideError(SubjectsErrorText);
                isSubjectsValid = true;
            }
        }
        #endregion

        #region التحقق النهائي من النموذج
        private bool ValidateForm()
        {
            bool isValid = true;

            ValidateStudent();
            ValidateClass();
            ValidateSubjects();

            if (!isStudentValid || !isClassValid || !isSubjectsValid)
            {
                isValid = false;
                ShowFormStatus("❌ يرجى التحقق من صحة جميع البيانات المدخلة", "❌", "#F44336");
            }

            return isValid;
        }
        #endregion

        #region أحداث الأزرار
        private void SelectAllButton_Click(object sender, RoutedEventArgs e)
        {
            foreach (SubjectItem subject in subjectList)
            {
                subject.IsSelected = true;
            }
            SubjectsListView.Items.Refresh();
            ValidateSubjects();
            UpdateSaveButtonState();
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            if (!ValidateForm())
                return;

            try
            {
                // الحصول على الطالب المحدد
               
                bool allSuccess = true;
                int successCount = 0;
                int totalSelected = 0;

                // إدراج سجل لكل مادة محددة
                foreach (SubjectItem subject in subjectList)
                {
                    if (subject.IsSelected)
                    {
                        totalSelected++;

                        List<SqlParameter> parameters = new List<SqlParameter>()
                        {
                            new SqlParameter("@StudentID", selectedStudentId.Value),
                            new SqlParameter("@SubjectID", subject.SubjectID),
                            new SqlParameter("@ClassID", selectedClassId.Value)
                        };

                        bool success = SqlExec.Exec_proc("InsertStudentSubjectsRegster", parameters);

                        if (success)
                        {
                            successCount++;
                        }
                        else
                        {
                            allSuccess = false;
                            // يمكنك تسجيل المواد التي فشل تسجيلها
                        }
                    }
                }

                if (successCount > 0)
                {
                    string message;

                    if (successCount == totalSelected)
                    {
                        message = $"✅ تم تسجيل جميع المواد ({successCount}) بنجاح!";
                        ShowFormStatus(message, "✅", "#4CAF50");
                    }
                    else
                    {
                        message = $"⚠️ تم تسجيل {successCount} من أصل {totalSelected} مادة\n" +
                                 "بعض المواد كانت مسجلة مسبقاً";
                        ShowFormStatus(message, "⚠️", "#FF9800");
                    }

                    // إظهار رسالة نجاح
                    MessageBox.Show(message, "نتيجة التسجيل",
                        MessageBoxButton.OK, MessageBoxImage.Information);
                }
                else
                {
                    ShowFormStatus("❌ لم يتم تسجيل أي مادة. جميع المواد مسجلة مسبقاً", "❌", "#F44336");
                    MessageBox.Show("جميع المواد المحددة مسجلة مسبقاً للطالب", "تحذير",
                        MessageBoxButton.OK, MessageBoxImage.Warning);
                }

                // مسح النموذج بعد الحفظ الناجح
                if (successCount > 0)
                {
                    ClearForm();
                }

            }
            catch (SqlException ex)
            {
                if (ex.Message.Contains("المادة المحددة لا تنتمي إلى الصف المختار"))
                {
                    MessageBox.Show("المادة المحددة لا تنتمي إلى الصف المختار", "خطأ",
                        MessageBoxButton.OK, MessageBoxImage.Error);
                }
                else if (ex.Message.Contains("الطالب مسجل مسبقاً في هذه المادة"))
                {
                    MessageBox.Show("الطالب مسجل مسبقاً في بعض المواد المحددة", "تحذير",
                        MessageBoxButton.OK, MessageBoxImage.Warning);
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
            StudentComboBox.SelectedIndex = -1;
            ClassComboBox.SelectedIndex = -1;
            SubjectsListView.ItemsSource = null;
            subjectList.Clear();

            HideError(StudentErrorText);
            HideError(ClassErrorText);
            HideError(SubjectsErrorText);

            isStudentValid = false;
            isClassValid = false;
            isSubjectsValid = false;

            HideFormStatus();

            UpdateSaveButtonState();
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            var result = MessageBox.Show(
                "هل أنت متأكد من إلغاء التسجيل؟\nسيتم فقدان جميع البيانات المدخلة.",
                "تأكيد الإلغاء",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                ClearForm();
            }
        }
        #endregion
    }

    #region فئات مساعدة
    public class StudentItemSub
    {
        public Guid StudentID { get; set; }
        public string FullName { get; set; }

        public override string ToString()
        {
            return FullName;
        }
    }

    public class ClassItemSub
    {
        public Guid ClassID { get; set; }
        public string EducationLevel { get; set; }

        public override string ToString()
        {
            return EducationLevel;
        }
    }

    public class SubjectItem
    {
        public Guid SubjectID { get; set; }
        public string SubjectName { get; set; }
        public bool IsSelected { get; set; }
    }
    #endregion
}