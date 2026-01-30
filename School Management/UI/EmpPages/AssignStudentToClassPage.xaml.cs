using School_Management.Control;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Windows;
using System.Windows.Controls;

namespace School_Management.UI.EmpPages
{
    public partial class AssignStudentToClassPage : UserControl
    {
        private List<StudentItem> allStudents = new List<StudentItem>();
        private List<ClassGroupItem> allClassGroups = new List<ClassGroupItem>();

        private Guid? selectedStudentId = null;
        private Guid? selectedClassGroupId = null;

        public AssignStudentToClassPage()
        {
            InitializeComponent();
            LoadData();
        }

        private void LoadData()
        {
            LoadStudents();
            LoadClassGroups();
            UpdateSaveButtonState();
        }

        private void LoadStudents()
        {
            try
            {
                allStudents.Clear();
                StudentsComboBox.ItemsSource = null;

                // جلب الطلاب غير المسجلين في أي صف (أو جميع الطلاب للاختيار)
                string query = @"
                    SELECT s.StudentID, s.StudentName, s.NationalNumber, s.Age, s.RegistrationStatus
                     FROM Students s
                     WHERE s.RegistrationStatus IN (N'مسجل', N'منتظم')
                     ORDER BY s.StudentName";

                if (CurrentConnection.OpenConntion())
                {
                    using (SqlCommand cmd = new SqlCommand(query, CurrentConnection.CuCon))
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            var student = new StudentItem
                            {
                                StudentID = reader.GetGuid(0),
                                StudentName = reader.GetString(1),
                                NationalNumber = reader.GetString(2),
                                Age = reader.GetInt32(3),
                                RegistrationStatus = reader.GetString(4)
                            };

                            allStudents.Add(student);
                        }
                    }
                }

                StudentsComboBox.ItemsSource = allStudents;
                if (allStudents.Count > 0)
                {
                    StudentsComboBox.DisplayMemberPath = "DisplayText";
                    StudentInfoText.Text = $"عدد الطلاب المتاحين: {allStudents.Count}";
                    StudentInfoText.Visibility = Visibility.Visible;
                }
                else
                {
                    ShowError(StudentErrorText, "لا يوجد طلاب متاحين للتسجيل");
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

        private void LoadClassGroups()
        {
            try
            {
                allClassGroups.Clear();
                ClassGroupComboBox.ItemsSource = null;

                // جلب جميع الصفوف المدموجة مع الشعب
                string query = @"
                    SELECT cg.ClassGroupID, cg.fullClassName, c.EducationLevel, 
                           g.GroupName, cg.MaxStudents,
                           (SELECT COUNT(*) FROM Student_Class_Group scg 
                            WHERE scg.ClassGroupID = cg.ClassGroupID) as CurrentStudents
                    FROM Class_Group cg
                    INNER JOIN Classes c ON cg.ClassID = c.ClassID
                    INNER JOIN Groups g ON cg.GroupID = g.GroupID
                    ORDER BY c.EducationLevel, g.GroupName";

                if (CurrentConnection.OpenConntion())
                {
                    using (SqlCommand cmd = new SqlCommand(query, CurrentConnection.CuCon))
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            var classGroup = new ClassGroupItem
                            {
                                ClassGroupID = reader.GetGuid(0),
                                FullClassName = reader.GetString(1),
                                EducationLevel = reader.GetString(2),
                                GroupName = reader.GetString(3),
                                MaxStudents = reader.GetInt32(4),
                                CurrentStudents = reader.GetInt32(5)
                            };

                            allClassGroups.Add(classGroup);
                        }
                    }
                }

                ClassGroupComboBox.ItemsSource = allClassGroups;
                if (allClassGroups.Count > 0)
                {
                    ClassGroupComboBox.DisplayMemberPath = "DisplayText";
                    ClassGroupInfoText.Text = $"عدد الصفوف/الشعب المتاحة: {allClassGroups.Count}";
                    ClassGroupInfoText.Visibility = Visibility.Visible;
                }
                else
                {
                    ShowError(ClassGroupErrorText, "لا يوجد صفوف/شعب متاحة");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"حدث خطأ أثناء تحميل الصفوف والشعب: {ex.Message}",
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
            SaveButton.IsEnabled = (selectedStudentId.HasValue && selectedClassGroupId.HasValue);
        }

        #region أحداث التحكم

        private void StudentsComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (StudentsComboBox.SelectedItem is StudentItem selectedStudent)
            {
                selectedStudentId = selectedStudent.StudentID;
                HideError(StudentErrorText);

                // عرض تفاصيل الطالب المحدد
                SelectedStudentNameText.Text = selectedStudent.StudentName;
                SelectedNationalNumberText.Text = selectedStudent.NationalNumber;
                SelectedAgeText.Text = $"{selectedStudent.Age} سنة";
                SelectedStatusText.Text = selectedStudent.RegistrationStatus;

                StudentDetailsBorder.Visibility = Visibility.Visible;
            }
            else
            {
                selectedStudentId = null;
                StudentDetailsBorder.Visibility = Visibility.Collapsed;
            }

            UpdateSaveButtonState();
        }

        private void ClassGroupComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (ClassGroupComboBox.SelectedItem is ClassGroupItem selectedClassGroup)
            {
                selectedClassGroupId = selectedClassGroup.ClassGroupID;
                HideError(ClassGroupErrorText);

                // عرض تفاصيل الصف/الشعبة المحددة
                ClassNameText.Text = selectedClassGroup.EducationLevel;
                GroupNameText.Text = selectedClassGroup.GroupName;
                CurrentStudentsText.Text = selectedClassGroup.CurrentStudents.ToString();
                MaxStudentsText.Text = selectedClassGroup.MaxStudents.ToString();

                ClassGroupDetailsBorder.Visibility = Visibility.Visible;

                // التحقق من السعة
                if (selectedClassGroup.CurrentStudents >= selectedClassGroup.MaxStudents)
                {
                    ShowFormStatus("⚠️ تحذير: هذا الصف/الشعبة قد وصل للعدد الأقصى من الطلاب", "⚠️", "#FF9800");
                    SaveButton.IsEnabled = false;
                }
                else
                {
                    HideFormStatus();
                }
            }
            else
            {
                selectedClassGroupId = null;
                ClassGroupDetailsBorder.Visibility = Visibility.Collapsed;
            }

            UpdateSaveButtonState();
        }

        private void RefreshStudentsButton_Click(object sender, RoutedEventArgs e)
        {
            LoadStudents();
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            if (!selectedStudentId.HasValue || !selectedClassGroupId.HasValue)
            {
                ShowFormStatus("❌ الرجاء اختيار طالب وصف/شعبة", "❌", "#F44336");
                return;
            }

            // التحقق من أن الطالب غير مسجل مسبقاً في نفس الصف/الشعبة
            try
            {
                string checkQuery = @"
                    SELECT COUNT(*) 
                    FROM Student_Class_Group 
                    WHERE StudentID = @StudentID AND ClassGroupID = @ClassGroupID";

                if (CurrentConnection.OpenConntion())
                {
                    using (SqlCommand checkCmd = new SqlCommand(checkQuery, CurrentConnection.CuCon))
                    {
                        checkCmd.Parameters.AddWithValue("@StudentID", selectedStudentId.Value);
                        checkCmd.Parameters.AddWithValue("@ClassGroupID", selectedClassGroupId.Value);

                        int existingCount = Convert.ToInt32(checkCmd.ExecuteScalar());

                        if (existingCount > 0)
                        {
                            MessageBox.Show("الطالب مسجل مسبقاً في هذا الصف/الشعبة",
                                "تنبيه", MessageBoxButton.OK, MessageBoxImage.Warning);
                            return;
                        }
                    }
                }

                // استخدام الإجراء المخزن
                List<SqlParameter> parameters = new List<SqlParameter>()
                {
                    new SqlParameter("@StudentID", selectedStudentId.Value),
                    new SqlParameter("@ClassGroupID", selectedClassGroupId.Value)
                };

                bool success = SqlExec.Exec_proc("AssignStudentToClassGroup", parameters);

                if (success)
                {
                    // الحصول على تفاصيل الصف/الشعبة
                    var classGroup = allClassGroups.Find(cg => cg.ClassGroupID == selectedClassGroupId.Value);
                    var student = allStudents.Find(s => s.StudentID == selectedStudentId.Value);

                    MessageBox.Show($"تم تسجيل الطالب بنجاح!\n" +
                                  $"الطالب: {student.StudentName}\n" +
                                  $"الصف/الشعبة: {classGroup.FullClassName}",
                                  "نجاح", MessageBoxButton.OK, MessageBoxImage.Information);

                    ClearForm();
                    LoadData(); // تحديث البيانات
                }
                else
                {
                    MessageBox.Show("حدث خطأ أثناء تسجيل الطالب", "خطأ",
                        MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            catch (SqlException ex)
            {
                if (ex.Message.Contains("الطالب مسجل مسبقاً"))
                {
                    MessageBox.Show("الطالب مسجل مسبقاً في هذا الصف/الشعبة",
                        "تنبيه", MessageBoxButton.OK, MessageBoxImage.Warning);
                }
                else if (ex.Message.Contains("السعة القصوى"))
                {
                    MessageBox.Show("الصف/الشعبة قد وصل للعدد الأقصى من الطلاب",
                        "تنبيه", MessageBoxButton.OK, MessageBoxImage.Warning);
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

        private void ClearButton_Click(object sender, RoutedEventArgs e)
        {
            ClearForm();
        }

        private void ClearForm()
        {
            StudentsComboBox.SelectedIndex = -1;
            ClassGroupComboBox.SelectedIndex = -1;

            selectedStudentId = null;
            selectedClassGroupId = null;

            StudentDetailsBorder.Visibility = Visibility.Collapsed;
            ClassGroupDetailsBorder.Visibility = Visibility.Collapsed;
            HideFormStatus();

            HideError(StudentErrorText);
            HideError(ClassGroupErrorText);

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
                   // mainWindow.HomeButton_Click(sender, e);
                }
            }
        }

        #endregion
    }

    #region فئات البيانات

    public class StudentItem
    {
        public Guid StudentID { get; set; }
        public string StudentName { get; set; }
        public string NationalNumber { get; set; }
        public int Age { get; set; }
        public string RegistrationStatus { get; set; }

        public string DisplayText
        {
            get { return $"{StudentName} ({NationalNumber})"; }
        }
    }

    public class ClassGroupItem
    {
        public Guid ClassGroupID { get; set; }
        public string FullClassName { get; set; } // مثلاً: "الصف الثالث الشعبة الرابعة"
        public string EducationLevel { get; set; } // اسم الصف فقط
        public string GroupName { get; set; } // اسم الشعبة فقط
        public int MaxStudents { get; set; }
        public int CurrentStudents { get; set; }

        public string DisplayText
        {
            get { return $"{FullClassName} ({CurrentStudents}/{MaxStudents} طالب)"; }
        }

        public bool HasAvailableSeats
        {
            get { return CurrentStudents < MaxStudents; }
        }
    }

    #endregion
}