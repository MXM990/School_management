using School_Management.Control;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Windows;
using System.Windows.Controls;

namespace School_Management.UI.EmpPages
{
    public partial class AssignTeacherToClassPage : UserControl
    {
        private List<TeacherItem> allTeachers = new List<TeacherItem>();
        private List<ClassGroupItemTech> allClassGroups = new List<ClassGroupItemTech>();

        private Guid? selectedTeacherId = null;
        private Guid? selectedClassGroupId = null;

        public AssignTeacherToClassPage()
        {
            InitializeComponent();
            LoadData();
        }

        private void LoadData()
        {
            LoadTeachers();
            LoadClassGroups();
            UpdateSaveButtonState();
        }

        private void LoadTeachers()
        {
            try
            {
                allTeachers.Clear();
                TeachersComboBox.ItemsSource = null;

                // استعلام معدل ليناسب جدول Teachers الجديد
                string query = @"
                    SELECT TeacherID, TeacherName, NationalNumber, 
                           Specialization, Age, PhoneNumber, 
                           YearsOfExperience, Salary, HireDate
                    FROM Teachers
                    ORDER BY TeacherName";

                if (CurrentConnection.OpenConntion())
                {
                    using (SqlCommand cmd = new SqlCommand(query, CurrentConnection.CuCon))
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            var teacher = new TeacherItem
                            {
                                TeacherID = reader.GetGuid(0),
                                TeacherName = reader.GetString(1),
                                NationalNumber = reader.IsDBNull(2) ? "غير متوفر" : reader.GetString(2),
                                Specialization = reader.IsDBNull(3) ? "غير محدد" : reader.GetString(3),
                                Age = reader.IsDBNull(4) ? 0 : reader.GetInt32(4),
                                PhoneNumber = reader.IsDBNull(5) ? "غير متوفر" : reader.GetString(5),
                                YearsOfExperience = reader.IsDBNull(6) ? 0 : reader.GetInt32(6),
                                Salary = reader.IsDBNull(7) ? 0 : reader.GetInt32(7),
                                HireDate = reader.IsDBNull(8) ? DateTime.MinValue : reader.GetDateTime(8)
                            };

                            allTeachers.Add(teacher);
                        }
                    }
                }

                TeachersComboBox.ItemsSource = allTeachers;
                if (allTeachers.Count > 0)
                {
                    TeacherInfoText.Text = $"عدد المدرسين المتاحين: {allTeachers.Count}";
                    TeacherInfoText.Visibility = Visibility.Visible;
                }
                else
                {
                    ShowError(TeacherErrorText, "لا يوجد مدرسين متاحين للتخصيص");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"حدث خطأ أثناء تحميل المدرسين: {ex.Message}",
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

                string query = @"
                    SELECT cg.ClassGroupID, cg.fullClassName, c.EducationLevel, 
                           g.GroupName,
                           (SELECT COUNT(*) FROM Teacher_Class_Group tcg 
                            WHERE tcg.ClassGroupID = cg.ClassGroupID) as CurrentTeachers
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
                            var classGroup = new ClassGroupItemTech
                            {
                                ClassGroupID = reader.GetGuid(0),
                                FullClassName = reader.GetString(1),
                                EducationLevel = reader.GetString(2),
                                GroupName = reader.GetString(3),
                                CurrentTeachers = reader.GetInt32(4)
                            };

                            allClassGroups.Add(classGroup);
                        }
                    }
                }

                ClassGroupComboBox.ItemsSource = allClassGroups;
                if (allClassGroups.Count > 0)
                {
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
            SaveButton.IsEnabled = (selectedTeacherId.HasValue && selectedClassGroupId.HasValue);
        }

        #region أحداث التحكم

        private void TeachersComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (TeachersComboBox.SelectedItem is TeacherItem selectedTeacher)
            {
                selectedTeacherId = selectedTeacher.TeacherID;
                HideError(TeacherErrorText);

                // عرض تفاصيل المدرس المحدد حسب الحقول الجديدة
                SelectedTeacherNameText.Text = selectedTeacher.TeacherName;
                SelectedNationalNumberText.Text = selectedTeacher.NationalNumber;
                SelectedSpecializationText.Text = selectedTeacher.Specialization;
                SelectedExperienceText.Text = $"{selectedTeacher.YearsOfExperience} سنة";

                TeacherDetailsBorder.Visibility = Visibility.Visible;

                // تحميل الصفوف الحالية للمدرس
                LoadTeacherCurrentClasses(selectedTeacher.TeacherID);
            }
            else
            {
                selectedTeacherId = null;
                TeacherDetailsBorder.Visibility = Visibility.Collapsed;
            }

            UpdateSaveButtonState();
        }

        private void LoadTeacherCurrentClasses(Guid teacherId)
        {
            try
            {
                string query = @"
                    SELECT COUNT(*) as TotalClasses,
                           STRING_AGG(cg.fullClassName, ', ') WITHIN GROUP (ORDER BY cg.fullClassName) as ClassList
                    FROM Teacher_Class_Group tcg
                    INNER JOIN Class_Group cg ON tcg.ClassGroupID = cg.ClassGroupID
                    WHERE tcg.TeacherID = @TeacherID";

                if (CurrentConnection.OpenConntion())
                {
                    using (SqlCommand cmd = new SqlCommand(query, CurrentConnection.CuCon))
                    {
                        cmd.Parameters.AddWithValue("@TeacherID", teacherId);

                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                int totalClasses = reader.GetInt32(0);
                                string classList = reader.IsDBNull(1) ? "لا يوجد" : reader.GetString(1);

                                if (totalClasses > 0)
                                {
                                    ShowFormStatus($"المدرس يدرس حالياً في {totalClasses} صف/شعبة: {classList}",
                                        "ℹ️", "#2196F3");
                                }
                                else
                                {
                                    ShowFormStatus("المدرس غير مخصص لأي صف/شعبة حالياً", "ℹ️", "#2196F3");
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception)
            {
                // تجاهل الخطأ، هذه المعلومة إضافية
            }
            finally
            {
                CurrentConnection.CloseConntion();
            }
        }

        private void ClassGroupComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (ClassGroupComboBox.SelectedItem is ClassGroupItemTech selectedClassGroup)
            {
                selectedClassGroupId = selectedClassGroup.ClassGroupID;
                HideError(ClassGroupErrorText);

                // عرض تفاصيل الصف/الشعبة المحددة
                ClassNameText.Text = selectedClassGroup.EducationLevel;
                GroupNameText.Text = selectedClassGroup.GroupName;
                CurrentTeachersText.Text = selectedClassGroup.CurrentTeachers.ToString();
                MaxTeachersText.Text = "غير محدد";

                ClassGroupDetailsBorder.Visibility = Visibility.Visible;

                // تحميل المدرسين الحاليين في هذا الصف
                LoadCurrentTeachersInClass(selectedClassGroup.ClassGroupID);
            }
            else
            {
                selectedClassGroupId = null;
                ClassGroupDetailsBorder.Visibility = Visibility.Collapsed;
            }

            UpdateSaveButtonState();
        }

        private void LoadCurrentTeachersInClass(Guid classGroupId)
        {
            try
            {
                string query = @"
                    SELECT COUNT(*) as TotalTeachers,
                           STRING_AGG(t.TeacherName, ', ') WITHIN GROUP (ORDER BY t.TeacherName) as TeacherList
                    FROM Teacher_Class_Group tcg
                    INNER JOIN Teachers t ON tcg.TeacherID = t.TeacherID
                    WHERE tcg.ClassGroupID = @ClassGroupID";

                if (CurrentConnection.OpenConntion())
                {
                    using (SqlCommand cmd = new SqlCommand(query, CurrentConnection.CuCon))
                    {
                        cmd.Parameters.AddWithValue("@ClassGroupID", classGroupId);

                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                int totalTeachers = reader.GetInt32(0);
                                string teacherList = reader.IsDBNull(1) ? "لا يوجد" : reader.GetString(1);

                                // تحديث واجهة المستخدم بالمعلومات
                                CurrentTeachersText.Text = totalTeachers.ToString();

                                if (totalTeachers > 0)
                                {
                                    ShowFormStatus($"هذا الصف يحتوي على {totalTeachers} مدرس: {teacherList}",
                                        "ℹ️", "#2196F3");
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception)
            {
                // تجاهل الخطأ، هذه المعلومة إضافية
            }
            finally
            {
                CurrentConnection.CloseConntion();
            }
        }

        private void RefreshTeachersButton_Click(object sender, RoutedEventArgs e)
        {
            LoadTeachers();
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            if (!selectedTeacherId.HasValue || !selectedClassGroupId.HasValue)
            {
                ShowFormStatus("❌ الرجاء اختيار مدرس وصف/شعبة", "❌", "#F44336");
                return;
            }

            // التحقق من أن المدرس غير مخصص مسبقاً في نفس الصف/الشعبة
            try
            {
                string checkQuery = @"
                    SELECT COUNT(*) 
                    FROM Teacher_Class_Group 
                    WHERE TeacherID = @TeacherID AND ClassGroupID = @ClassGroupID";

                if (CurrentConnection.OpenConntion())
                {
                    using (SqlCommand checkCmd = new SqlCommand(checkQuery, CurrentConnection.CuCon))
                    {
                        checkCmd.Parameters.AddWithValue("@TeacherID", selectedTeacherId.Value);
                        checkCmd.Parameters.AddWithValue("@ClassGroupID", selectedClassGroupId.Value);

                        int existingCount = Convert.ToInt32(checkCmd.ExecuteScalar());

                        if (existingCount > 0)
                        {
                            MessageBox.Show("المدرس مخصص مسبقاً في هذا الصف/الشعبة",
                                "تنبيه", MessageBoxButton.OK, MessageBoxImage.Warning);
                            return;
                        }
                    }
                }
                CurrentConnection.CloseConntion();
                // استخدام الإجراء المخزن
                List<SqlParameter> parameters = new List<SqlParameter>()
                {
                    new SqlParameter("@TeacherID", selectedTeacherId.Value),
                    new SqlParameter("@ClassGroupID", selectedClassGroupId.Value)
                };

                bool success = SqlExec.Exec_proc("InsertNewTeacherClassGroup", parameters);

                if (success)
                {
                    // الحصول على تفاصيل الصف/الشعبة والمدرس
                    var classGroup = allClassGroups.Find(cg => cg.ClassGroupID == selectedClassGroupId.Value);
                    var teacher = allTeachers.Find(t => t.TeacherID == selectedTeacherId.Value);

                    MessageBox.Show($"تم تخصيص المدرس بنجاح!\n" +
                                  $"المدرس: {teacher.TeacherName}\n" +
                                  $"الصف/الشعبة: {classGroup.FullClassName}",
                                  "نجاح", MessageBoxButton.OK, MessageBoxImage.Information);

                    ClearForm();
                    LoadData(); // تحديث البيانات
                }
                else
                {
                    MessageBox.Show("حدث خطأ أثناء تخصيص المدرس", "خطأ",
                        MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            catch (SqlException ex)
            {
                if (ex.Message.Contains("المدرس مخصص مسبقاً"))
                {
                    MessageBox.Show("المدرس مخصص مسبقاً في هذا الصف/الشعبة",
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
            TeachersComboBox.SelectedIndex = -1;
            ClassGroupComboBox.SelectedIndex = -1;

            selectedTeacherId = null;
            selectedClassGroupId = null;

            TeacherDetailsBorder.Visibility = Visibility.Collapsed;
            ClassGroupDetailsBorder.Visibility = Visibility.Collapsed;
            HideFormStatus();

            HideError(TeacherErrorText);
            HideError(ClassGroupErrorText);

            // إعادة تعيين النصوص
            SelectedTeacherNameText.Text = "-";
            SelectedNationalNumberText.Text = "-";
            SelectedSpecializationText.Text = "-";
            SelectedExperienceText.Text = "-";
            ClassNameText.Text = "-";
            GroupNameText.Text = "-";
            CurrentTeachersText.Text = "0";

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

    #region فئات البيانات المعدلة

    public class TeacherItem
    {
        public Guid TeacherID { get; set; }
        public string TeacherName { get; set; }
        public string NationalNumber { get; set; }
        public string Specialization { get; set; }
        public int Age { get; set; }
        public string PhoneNumber { get; set; }
        public int YearsOfExperience { get; set; }
        public int Salary { get; set; }
        public DateTime HireDate { get; set; }
        public override string ToString()
        {
            return TeacherName;
        }

    }

    public class ClassGroupItemTech
    {
        public Guid ClassGroupID { get; set; }
        public string FullClassName { get; set; }
        public string EducationLevel { get; set; }
        public string GroupName { get; set; }
        public int CurrentTeachers { get; set; }
        public override string ToString()
        {
            return FullClassName;
        }
    }

    #endregion
}