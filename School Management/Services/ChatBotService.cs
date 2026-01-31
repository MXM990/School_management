using Newtonsoft.Json;
using System;
using System.Data.SqlClient;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace School_Management.Services
{
    public class SmartChatBotService
    {
        private readonly string _connectionString;
        private readonly HttpClient _httpClient;

        // استخدام نموذج عربي ممتاز
        private const string HuggingFaceApiUrl = "https://api-inference.huggingface.co/models/arbml/aragpt2-mega";
        private const string ApiToken = "test";

        public SmartChatBotService(string connectionString)
        {
            _connectionString = connectionString;
            _httpClient = new HttpClient();
            _httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {ApiToken}");
            _httpClient.Timeout = TimeSpan.FromSeconds(30);
        }

        public async Task<string> GetResponseAsync(string question, string username = null)
        {
            try
            {
                Console.WriteLine($"السؤال: {question}");

                // 1. تحليل السؤال بشكل عميق
                var questionInfo = AnalyzeQuestionDeeply(question);
                Console.WriteLine($"تحليل السؤال: نوع={questionInfo.Type}, كيانات={string.Join(",", questionInfo.Entities)}");

                // 2. استخراج البيانات من قاعدة البيانات
                var databaseData = await ExtractDataFromDatabase(questionInfo, question);

                // 3. بناء رد ذكي
                var response = await BuildIntelligentResponse(question, questionInfo, databaseData, username);

                return response;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"خطأ في GetResponseAsync: {ex.Message}");
                return $"عذراً، حدث خطأ في معالجة سؤالك: {ex.Message}";
            }
        }

        private QuestionInfo AnalyzeQuestionDeeply(string question)
        {
            var info = new QuestionInfo();
            var lowerQuestion = question.ToLower().Trim();

            // تحليل نوع السؤال
            if (Regex.IsMatch(lowerQuestion, @"(اسم|أسماء|من هو|من هي|معلومات عن|تفاصيل عن)"))
            {
                info.Type = QuestionType.Details;
            }
            else if (Regex.IsMatch(lowerQuestion, @"(كم|عدد|كَم|كَمْ|إحصاء|إحصائيات)"))
            {
                info.Type = QuestionType.Count;
            }
            else if (Regex.IsMatch(lowerQuestion, @"(أين|مكان|عنوان|موقع)"))
            {
                info.Type = QuestionType.Location;
            }
            else if (Regex.IsMatch(lowerQuestion, @"(متى|تاريخ|متى تم|متى كان)"))
            {
                info.Type = QuestionType.Date;
            }
            else if (Regex.IsMatch(lowerQuestion, @"(كيف|طريقة|كيفية|كَيْفَ)"))
            {
                info.Type = QuestionType.Method;
            }
            else if (Regex.IsMatch(lowerQuestion, @"(لماذا|سبب|لِماذا|لِمَ)"))
            {
                info.Type = QuestionType.Reason;
            }
            else if (Regex.IsMatch(lowerQuestion, @"(مرحبا|اهلاً|السلام|مرحباً)"))
            {
                info.Type = QuestionType.Greeting;
            }
            else if (Regex.IsMatch(lowerQuestion, @"(مساعدة|مساعد|ماذا تستطيع|ماذا يمكنك)"))
            {
                info.Type = QuestionType.Help;
            }
            else
            {
                info.Type = QuestionType.General;
            }

            // تحليل الكيانات المذكورة
            var entityPatterns = new Dictionary<string, string[]>
            {
                ["طالب"] = new[] { "طالب", "طلاب", "تلميذ", "تلاميذ", "الطالب", "الطلاب" },
                ["مدرس"] = new[] { "مدرس", "معلم", "أستاذ", "أساتذة", "مدرسين", "معلمين" },
                ["صف"] = new[] { "صف", "صفوف", "فصل", "فصول", "الصف", "الصفوف" },
                ["شعبة"] = new[] { "شعبة", "شعب", "مجموعة", "مجموعات", "الشعبة", "الشعب" },
                ["مادة"] = new[] { "مادة", "مواد", "درس", "دروس", "المادة", "المواد" },
                ["موظف"] = new[] { "موظف", "موظفين", "إداري", "إداريين", "الموظف", "الموظفين" },
                ["موضوع"] = new[] { "موضوع", "مواضيع" },
                ["مجموعة"] = new[] { "مجموعة", "مجموعات" },
                ["معلومة"] = new[] { "معلومة", "معلومات", "بيانات", "تفاصيل" }
            };

            foreach (var pattern in entityPatterns)
            {
                if (pattern.Value.Any(p => lowerQuestion.Contains(p)))
                {
                    info.Entities.Add(pattern.Key);
                }
            }

            // استخراج الأسماء المحددة
            var nameMatch = Regex.Match(question, @"(أحمد|محمد|علي|حسن|حسين|محمود|خالد|سعيد|عمر|عثمان)");
            if (nameMatch.Success)
            {
                info.SpecificName = nameMatch.Value;
            }

            // استخراج الأرقام
            var numberMatch = Regex.Match(question, @"\d+");
            if (numberMatch.Success)
            {
                info.SpecificNumber = int.Parse(numberMatch.Value);
            }

            // تحليل السياق
            if (lowerQuestion.Contains("مواد") && lowerQuestion.Contains("طالب"))
            {
                info.Context = "student_subjects";
            }
            else if (lowerQuestion.Contains("مدرس") && lowerQuestion.Contains("صف"))
            {
                info.Context = "teacher_classes";
            }
            else if (lowerQuestion.Contains("طالب") && lowerQuestion.Contains("صف"))
            {
                info.Context = "student_classes";
            }

            return info;
        }

        private async Task<DatabaseData> ExtractDataFromDatabase(QuestionInfo questionInfo, string question)
        {
            var data = new DatabaseData();

            if (string.IsNullOrWhiteSpace(_connectionString))
                return data;

            try
            {
                using (var connection = new SqlConnection(_connectionString))
                {
                    await connection.OpenAsync();
                    Console.WriteLine("تم الاتصال بقاعدة البيانات");

                    // استخراج البيانات بناءً على نوع السؤال والكيانات
                    if (questionInfo.Type == QuestionType.Details || questionInfo.Entities.Contains("معلومات"))
                    {
                        await ExtractDetailedData(connection, data, questionInfo, question);
                    }
                    else if (questionInfo.Type == QuestionType.Count)
                    {
                        await ExtractCountData(connection, data, questionInfo, question);
                    }
                    else if (!string.IsNullOrEmpty(questionInfo.Context))
                    {
                        await ExtractContextualData(connection, data, questionInfo, question);
                    }
                    else
                    {
                        // استخراج جميع البيانات المتاحة
                        await ExtractAllData(connection, data, questionInfo, question);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"خطأ في استخراج البيانات: {ex.Message}");
            }

            return data;
        }

        private async Task ExtractDetailedData(SqlConnection connection, DatabaseData data, QuestionInfo questionInfo, string question)
        {
            Console.WriteLine("استخراج البيانات التفصيلية");

            // إذا كان هناك اسم محدد، نبحث عنه
            if (!string.IsNullOrEmpty(questionInfo.SpecificName))
            {
                await SearchForSpecificName(connection, data, questionInfo.SpecificName, question);
                return;
            }

            // استخراج جميع البيانات التفصيلية بناءً على الكيانات
            if (questionInfo.Entities.Contains("طالب") || questionInfo.Entities.Count == 0)
            {
                await ExtractStudentDetails(connection, data);
            }

            if (questionInfo.Entities.Contains("مدرس"))
            {
                await ExtractTeacherDetails(connection, data);
            }

            if (questionInfo.Entities.Contains("صف"))
            {
                await ExtractClassDetails(connection, data);
            }

            if (questionInfo.Entities.Contains("شعبة"))
            {
                await ExtractGroupDetails(connection, data);
            }

            if (questionInfo.Entities.Contains("مادة"))
            {
                await ExtractSubjectDetails(connection, data);
            }

            if (questionInfo.Entities.Contains("موظف"))
            {
                await ExtractEmployeeDetails(connection, data);
            }
        }

        private async Task SearchForSpecificName(SqlConnection connection, DatabaseData data, string name, string question)
        {
            Console.WriteLine($"البحث عن الاسم: {name}");

            var lowerQuestion = question.ToLower();

            // البحث في الطلاب
            var studentQuery = @"
                SELECT StudentID, StudentName, NationalNumber, Age, 
                       RegistrationStatus, PhoneNumber, Email, BirthDate,
                       FatherName, Address
                FROM Students 
                WHERE StudentName LIKE @name
                ORDER BY StudentName";

            using (var cmd = new SqlCommand(studentQuery, connection))
            {
                cmd.Parameters.AddWithValue("@name", "%" + name + "%");

                using (var reader = await cmd.ExecuteReaderAsync())
                {
                    while (await reader.ReadAsync())
                    {
                        data.Students.Add(new Student
                        {
                            Id = reader.GetGuid(0),
                            Name = reader.GetString(1),
                            NationalNumber = reader.GetString(2),
                            Age = reader.GetInt32(3),
                            Status = reader.GetString(4),
                            Phone = reader.IsDBNull(5) ? "" : reader.GetString(5),
                            Email = reader.IsDBNull(6) ? "" : reader.GetString(6),
                            BirthDate = reader.GetDateTime(7),
                            FatherName = reader.IsDBNull(8) ? "" : reader.GetString(8),
                            Address = reader.IsDBNull(9) ? "" : reader.GetString(9)
                        });
                    }
                }
            }

            // إذا لم نجد في الطلاب، نبحث في المدرسين
            if (data.Students.Count == 0)
            {
                var teacherQuery = @"
                    SELECT TeacherID, TeacherName, NationalNumber, Age, PhoneNumber, 
                           YearsOfExperience, Specialization, HireDate, Salary
                    FROM Teachers 
                    WHERE TeacherName LIKE @name
                    ORDER BY TeacherName";

                using (var cmd = new SqlCommand(teacherQuery, connection))
                {
                    cmd.Parameters.AddWithValue("@name", "%" + name + "%");

                    using (var reader = await cmd.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            data.Teachers.Add(new Teacher
                            {
                                Id = reader.GetGuid(0),
                                Name = reader.GetString(1),
                                NationalNumber = reader.GetString(2),
                                Age = reader.GetInt32(3),
                                Phone = reader.IsDBNull(4) ? "" : reader.GetString(4),
                                ExperienceYears = reader.GetInt32(5),
                                Specialization = reader.GetString(6),
                                HireDate = reader.GetDateTime(7),
                                Salary = reader.GetInt32(8)
                            });
                        }
                    }
                }
            }

            // إذا لم نجد في المدرسين، نبحث في الموظفين
            if (data.Students.Count == 0 && data.Teachers.Count == 0)
            {
                var employeeQuery = @"
                    SELECT EmployeeID, EmployeeName, Username, NationalNumber, 
                           JobTitle, Age, PhoneNumber, Salary, HireDate
                    FROM Employees 
                    WHERE EmployeeName LIKE @name AND IsActive = 1
                    ORDER BY EmployeeName";

                using (var cmd = new SqlCommand(employeeQuery, connection))
                {
                    cmd.Parameters.AddWithValue("@name", "%" + name + "%");

                    using (var reader = await cmd.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            data.Employees.Add(new Employee
                            {
                                Id = reader.GetGuid(0),
                                Name = reader.GetString(1),
                                Username = reader.GetString(2),
                                NationalNumber = reader.GetString(3),
                                JobTitle = reader.GetString(4),
                                Age = reader.GetInt32(5),
                                Phone = reader.IsDBNull(6) ? "" : reader.GetString(6),
                                Salary = reader.GetInt32(7),
                                HireDate = reader.GetDateTime(8)
                            });
                        }
                    }
                }
            }
        }

        private async Task ExtractStudentDetails(SqlConnection connection, DatabaseData data)
        {
            Console.WriteLine("استخراج تفاصيل الطلاب");

            var query = @"
                SELECT StudentID, StudentName, NationalNumber, Age, 
                       RegistrationStatus, PhoneNumber, Email, BirthDate
                FROM Students 
                ORDER BY StudentName";

            using (var cmd = new SqlCommand(query, connection))
            {
                using (var reader = await cmd.ExecuteReaderAsync())
                {
                    while (await reader.ReadAsync())
                    {
                        data.Students.Add(new Student
                        {
                            Id = reader.GetGuid(0),
                            Name = reader.GetString(1),
                            NationalNumber = reader.GetString(2),
                            Age = reader.GetInt32(3),
                            Status = reader.GetString(4),
                            Phone = reader.IsDBNull(5) ? "" : reader.GetString(5),
                            Email = reader.IsDBNull(6) ? "" : reader.GetString(6),
                            BirthDate = reader.GetDateTime(7)
                        });
                    }
                }
            }
            Console.WriteLine($"تم استخراج {data.Students.Count} طالب");
        }

        private async Task ExtractTeacherDetails(SqlConnection connection, DatabaseData data)
        {
            Console.WriteLine("استخراج تفاصيل المدرسين");

            var query = @"
                SELECT TeacherID, TeacherName, NationalNumber, Age, PhoneNumber, 
                       YearsOfExperience, Specialization, HireDate, Salary
                FROM Teachers 
                ORDER BY TeacherName";

            using (var cmd = new SqlCommand(query, connection))
            {
                using (var reader = await cmd.ExecuteReaderAsync())
                {
                    while (await reader.ReadAsync())
                    {
                        data.Teachers.Add(new Teacher
                        {
                            Id = reader.GetGuid(0),
                            Name = reader.GetString(1),
                            NationalNumber = reader.GetString(2),
                            Age = reader.GetInt32(3),
                            Phone = reader.IsDBNull(4) ? "" : reader.GetString(4),
                            ExperienceYears = reader.GetInt32(5),
                            Specialization = reader.GetString(6),
                            HireDate = reader.GetDateTime(7),
                            Salary = reader.GetInt32(8)
                        });
                    }
                }
            }
            Console.WriteLine($"تم استخراج {data.Teachers.Count} مدرس");
        }

        private async Task ExtractClassDetails(SqlConnection connection, DatabaseData data)
        {
            Console.WriteLine("استخراج تفاصيل الصفوف");

            // الصفوف الأساسية
            var classesQuery = @"
                SELECT ClassID, EducationLevel, AdditionalInfo, CreatedDate
                FROM Classes 
                ORDER BY CreatedDate DESC";

            using (var cmd = new SqlCommand(classesQuery, connection))
            {
                using (var reader = await cmd.ExecuteReaderAsync())
                {
                    while (await reader.ReadAsync())
                    {
                        data.Classes.Add(new Class
                        {
                            Id = reader.GetGuid(0),
                            EducationLevel = reader.GetString(1),
                            AdditionalInfo = reader.IsDBNull(2) ? "" : reader.GetString(2),
                            CreatedDate = reader.GetDateTime(3)
                        });
                    }
                }
            }

            // الصفوف مع الشعب
            var classGroupsQuery = @"
                SELECT fullClassName, MaxStudents, ClassID, GroupID
                FROM Class_Group 
                ORDER BY fullClassName";

            using (var cmd = new SqlCommand(classGroupsQuery, connection))
            {
                using (var reader = await cmd.ExecuteReaderAsync())
                {
                    while (await reader.ReadAsync())
                    {
                        data.ClassGroups.Add(new ClassGroup
                        {
                            FullClassName = reader.GetString(0),
                            MaxStudents = reader.GetInt32(1),
                            ClassId = reader.GetGuid(2),
                            GroupId = reader.GetGuid(3)
                        });
                    }
                }
            }
        }

        private async Task ExtractGroupDetails(SqlConnection connection, DatabaseData data)
        {
            Console.WriteLine("استخراج تفاصيل الشعب");

            var query = @"
                SELECT GroupName, AdditionalInfo
                FROM Groups 
                ORDER BY GroupName";

            using (var cmd = new SqlCommand(query, connection))
            {
                using (var reader = await cmd.ExecuteReaderAsync())
                {
                    while (await reader.ReadAsync())
                    {
                        data.Groups.Add(new Group
                        {
                            Name = reader.GetString(0),
                            AdditionalInfo = reader.IsDBNull(1) ? "" : reader.GetString(1)
                        });
                    }
                }
            }
        }

        private async Task ExtractSubjectDetails(SqlConnection connection, DatabaseData data)
        {
            Console.WriteLine("استخراج تفاصيل المواد");

            var query = @"
                SELECT SubjectID, SubjectName, ClassID
                FROM Subjects 
                ORDER BY SubjectName";

            using (var cmd = new SqlCommand(query, connection))
            {
                using (var reader = await cmd.ExecuteReaderAsync())
                {
                    while (await reader.ReadAsync())
                    {
                        data.Subjects.Add(new Subject
                        {
                            Id = reader.GetGuid(0),
                            Name = reader.GetString(1),
                            ClassId = reader.GetGuid(2)
                        });
                    }
                }
            }
        }

        private async Task ExtractEmployeeDetails(SqlConnection connection, DatabaseData data)
        {
            Console.WriteLine("استخراج تفاصيل الموظفين");

            var query = @"
                SELECT EmployeeID, EmployeeName, Username, NationalNumber, 
                       JobTitle, Age, PhoneNumber, Salary, HireDate
                FROM Employees 
                WHERE IsActive = 1
                ORDER BY EmployeeName";

            using (var cmd = new SqlCommand(query, connection))
            {
                using (var reader = await cmd.ExecuteReaderAsync())
                {
                    while (await reader.ReadAsync())
                    {
                        data.Employees.Add(new Employee
                        {
                            Id = reader.GetGuid(0),
                            Name = reader.GetString(1),
                            Username = reader.GetString(2),
                            NationalNumber = reader.GetString(3),
                            JobTitle = reader.GetString(4),
                            Age = reader.GetInt32(5),
                            Phone = reader.IsDBNull(6) ? "" : reader.GetString(6),
                            Salary = reader.GetInt32(7),
                            HireDate = reader.GetDateTime(8)
                        });
                    }
                }
            }
        }

        private async Task ExtractCountData(SqlConnection connection, DatabaseData data, QuestionInfo questionInfo, string question)
        {
            Console.WriteLine("استخراج بيانات العد");

            var counts = new Dictionary<string, int>();

            // عد جميع الكيانات
            var countQueries = new Dictionary<string, string>
            {
                ["الطلاب"] = "SELECT COUNT(*) FROM Students",
                ["المدرسين"] = "SELECT COUNT(*) FROM Teachers",
                ["الصفوف"] = "SELECT COUNT(*) FROM Classes",
                ["الشعب"] = "SELECT COUNT(*) FROM Groups",
                ["المواد"] = "SELECT COUNT(*) FROM Subjects",
                ["الموظفين"] = "SELECT COUNT(*) FROM Employees WHERE IsActive = 1",
                ["الصفوف المشكلة"] = "SELECT COUNT(*) FROM Class_Group",
                ["تسجيلات الطلاب في الصفوف"] = "SELECT COUNT(*) FROM Student_Class_Group",
                ["تسجيلات المدرسين في الصفوف"] = "SELECT COUNT(*) FROM Teacher_Class_Group",
                ["تسجيلات المواد للطلاب"] = "SELECT COUNT(*) FROM StudentSubjectsRegster"
            };

            foreach (var query in countQueries)
            {
                using (var cmd = new SqlCommand(query.Value, connection))
                {
                    var count = (int)await cmd.ExecuteScalarAsync();
                    counts[query.Key] = count;
                    Console.WriteLine($"{query.Key}: {count}");
                }
            }

            data.Counts = counts;
        }

        private async Task ExtractContextualData(SqlConnection connection, DatabaseData data, QuestionInfo questionInfo, string question)
        {
            Console.WriteLine($"استخراج البيانات السياقية: {questionInfo.Context}");

            switch (questionInfo.Context)
            {
                case "student_subjects":
                    await ExtractStudentSubjects(connection, data, questionInfo.SpecificName);
                    break;

                case "teacher_classes":
                    await ExtractTeacherClasses(connection, data, questionInfo.SpecificName);
                    break;

                case "student_classes":
                    await ExtractStudentClasses(connection, data, questionInfo.SpecificName);
                    break;
            }
        }

        private async Task ExtractStudentSubjects(SqlConnection connection, DatabaseData data, string studentName = null)
        {
            Console.WriteLine("استخراج مواد الطالب");

            var query = @"
                SELECT ST.StudentName, SUB.SubjectName, C.EducationLevel
                FROM StudentSubjectsRegster AS SSR
                INNER JOIN Students AS ST ON SSR.StudentID = ST.StudentID
                INNER JOIN Subjects AS SUB ON SUB.SubjectID = SSR.SubjectID
                INNER JOIN Classes AS C ON SSR.ClassID = C.ClassID";

            if (!string.IsNullOrEmpty(studentName))
            {
                query += " WHERE ST.StudentName LIKE @name";
            }

            query += " ORDER BY ST.StudentName, SUB.SubjectName";

            using (var cmd = new SqlCommand(query, connection))
            {
                if (!string.IsNullOrEmpty(studentName))
                {
                    cmd.Parameters.AddWithValue("@name", "%" + studentName + "%");
                }

                using (var reader = await cmd.ExecuteReaderAsync())
                {
                    while (await reader.ReadAsync())
                    {
                        data.StudentSubjects.Add(new StudentSubject
                        {
                            StudentName = reader.GetString(0),
                            SubjectName = reader.GetString(1),
                            ClassLevel = reader.GetString(2)
                        });
                    }
                }
            }
        }

        private async Task ExtractTeacherClasses(SqlConnection connection, DatabaseData data, string teacherName = null)
        {
            Console.WriteLine("استخراج صفوف المدرس");

            var query = @"
                SELECT TA.TeacherName, CG.fullClassName, C.EducationLevel
                FROM Teacher_Class_Group AS TCG
                INNER JOIN Teachers AS TA ON TCG.TeacherID = TA.TeacherID
                INNER JOIN Class_Group AS CG ON TCG.ClassGroupID = CG.ClassGroupID
                INNER JOIN Classes AS C ON CG.ClassID = C.ClassID";

            if (!string.IsNullOrEmpty(teacherName))
            {
                query += " WHERE TA.TeacherName LIKE @name";
            }

            query += " ORDER BY TA.TeacherName, CG.fullClassName";

            using (var cmd = new SqlCommand(query, connection))
            {
                if (!string.IsNullOrEmpty(teacherName))
                {
                    cmd.Parameters.AddWithValue("@name", "%" + teacherName + "%");
                }

                using (var reader = await cmd.ExecuteReaderAsync())
                {
                    while (await reader.ReadAsync())
                    {
                        data.TeacherClasses.Add(new TeacherClass
                        {
                            TeacherName = reader.GetString(0),
                            ClassGroupName = reader.GetString(1),
                            EducationLevel = reader.GetString(2)
                        });
                    }
                }
            }
        }

        private async Task ExtractStudentClasses(SqlConnection connection, DatabaseData data, string studentName = null)
        {
            Console.WriteLine("استخراج صفوف الطالب");

            var query = @"
                SELECT SU.StudentName, SU.NationalNumber, SU.RegistrationStatus, 
                       CG.fullClassName, C.EducationLevel
                FROM Student_Class_Group AS SCG
                INNER JOIN Students AS SU ON SU.StudentID = SCG.StudentID
                INNER JOIN Class_Group AS CG ON SCG.ClassGroupID = CG.ClassGroupID
                INNER JOIN Classes AS C ON CG.ClassID = C.ClassID";

            if (!string.IsNullOrEmpty(studentName))
            {
                query += " WHERE SU.StudentName LIKE @name";
            }

            query += " ORDER BY CG.fullClassName, SU.StudentName";

            using (var cmd = new SqlCommand(query, connection))
            {
                if (!string.IsNullOrEmpty(studentName))
                {
                    cmd.Parameters.AddWithValue("@name", "%" + studentName + "%");
                }

                using (var reader = await cmd.ExecuteReaderAsync())
                {
                    while (await reader.ReadAsync())
                    {
                        data.StudentClasses.Add(new StudentClass
                        {
                            StudentName = reader.GetString(0),
                            NationalNumber = reader.GetString(1),
                            Status = reader.GetString(2),
                            ClassGroupName = reader.GetString(3),
                            EducationLevel = reader.GetString(4)
                        });
                    }
                }
            }
        }

        private async Task ExtractAllData(SqlConnection connection, DatabaseData data, QuestionInfo questionInfo, string question)
        {
            Console.WriteLine("استخراج جميع البيانات");

            // استخراج عينات من كل شيء
            await ExtractStudentDetails(connection, data);
            await ExtractTeacherDetails(connection, data);
            await ExtractClassDetails(connection, data);
            await ExtractGroupDetails(connection, data);
            await ExtractSubjectDetails(connection, data);
            await ExtractEmployeeDetails(connection, data);
            await ExtractStudentSubjects(connection, data);
            await ExtractTeacherClasses(connection, data);
            await ExtractStudentClasses(connection, data);
        }

        private async Task<string> BuildIntelligentResponse(string question, QuestionInfo questionInfo,
                                                          DatabaseData data, string username)
        {
            try
            {
                // أولاً: محاولة استخدام الذكاء الاصطناعي
                var aiResponse = await GetAIResponse(question, questionInfo, data, username);
                if (!string.IsNullOrWhiteSpace(aiResponse) && aiResponse.Length > 30)
                {
                    Console.WriteLine("تم استخدام رد الذكاء الاصطناعي");
                    return aiResponse;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"خطأ في الذكاء الاصطناعي: {ex.Message}");
            }

            // ثانياً: الرد الذكي المحلي
            Console.WriteLine("استخدام الرد المحلي الذكي");
            return GenerateLocalResponse(question, questionInfo, data);
        }

        private async Task<string> GetAIResponse(string question, QuestionInfo questionInfo,
                                               DatabaseData data, string username)
        {
            try
            {
                var prompt = BuildAIPrompt(question, questionInfo, data, username);

                var requestData = new
                {
                    inputs = prompt,
                    parameters = new
                    {
                        max_new_tokens = 300,
                        temperature = 0.8,
                        top_p = 0.9,
                        do_sample = true,
                        repetition_penalty = 1.2
                    }
                };

                var json = JsonConvert.SerializeObject(requestData);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await _httpClient.PostAsync(HuggingFaceApiUrl, content);
                var result = await response.Content.ReadAsStringAsync();

                if (response.IsSuccessStatusCode)
                {
                    var responses = JsonConvert.DeserializeObject<List<ChatResponse>>(result);
                    if (responses != null && responses.Count > 0)
                    {
                        var generatedText = responses[0].GeneratedText;

                        // تنظيف النص
                        generatedText = CleanResponseText(generatedText, prompt);

                        return generatedText.Length > 20 ? generatedText : "";
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"خطأ في GetAIResponse: {ex.Message}");
            }

            return string.Empty;
        }

        private string BuildAIPrompt(string question, QuestionInfo questionInfo,
                                   DatabaseData data, string username)
        {
            var prompt = new StringBuilder();

            prompt.AppendLine("أنت مساعد ذكي متخصص في نظام إدارة المدارس. لديك المعلومات التالية:");
            prompt.AppendLine();

            // إضافة البيانات المستخرجة
            if (data.Students.Any())
            {
                prompt.AppendLine("**الطلاب:**");
                foreach (var student in data.Students.Take(5))
                {
                    prompt.AppendLine($"- {student.Name} (العمر: {student.Age}، الحالة: {student.Status})");
                }
                if (data.Students.Count > 5) prompt.AppendLine($"- وهناك {data.Students.Count - 5} طالب آخرين");
                prompt.AppendLine();
            }

            if (data.Teachers.Any())
            {
                prompt.AppendLine("**المدرسون:**");
                foreach (var teacher in data.Teachers.Take(5))
                {
                    prompt.AppendLine($"- {teacher.Name} (التخصص: {teacher.Specialization}، الخبرة: {teacher.ExperienceYears} سنة)");
                }
                if (data.Teachers.Count > 5) prompt.AppendLine($"- وهناك {data.Teachers.Count - 5} مدرس آخرين");
                prompt.AppendLine();
            }

            if (data.Employees.Any())
            {
                prompt.AppendLine("**الموظفون:**");
                foreach (var employee in data.Employees.Take(5))
                {
                    prompt.AppendLine($"- {employee.Name} (الوظيفة: {employee.JobTitle})");
                }
                if (data.Employees.Count > 5) prompt.AppendLine($"- وهناك {data.Employees.Count - 5} موظف آخرين");
                prompt.AppendLine();
            }

            if (data.Classes.Any())
            {
                prompt.AppendLine("**الصفوف:**");
                foreach (var cls in data.Classes.Take(5))
                {
                    prompt.AppendLine($"- {cls.EducationLevel}" +
                                     (string.IsNullOrEmpty(cls.AdditionalInfo) ? "" : $" ({cls.AdditionalInfo})"));
                }
                prompt.AppendLine();
            }

            if (data.ClassGroups.Any())
            {
                prompt.AppendLine("**الصفوف المشكلة:**");
                foreach (var cg in data.ClassGroups.Take(5))
                {
                    prompt.AppendLine($"- {cg.FullClassName} (السعة: {cg.MaxStudents} طالب)");
                }
                prompt.AppendLine();
            }

            if (data.Groups.Any())
            {
                prompt.AppendLine("**الشعب:**");
                foreach (var group in data.Groups.Take(5))
                {
                    prompt.AppendLine($"- {group.Name}" +
                                     (string.IsNullOrEmpty(group.AdditionalInfo) ? "" : $" ({group.AdditionalInfo})"));
                }
                prompt.AppendLine();
            }

            if (data.Subjects.Any())
            {
                prompt.AppendLine("**المواد الدراسية:**");
                foreach (var subject in data.Subjects.Take(5))
                {
                    prompt.AppendLine($"- {subject.Name}");
                }
                if (data.Subjects.Count > 5) prompt.AppendLine($"- وهناك {data.Subjects.Count - 5} مادة أخرى");
                prompt.AppendLine();
            }

            if (data.StudentSubjects.Any())
            {
                prompt.AppendLine("**مواد الطلاب:**");
                var grouped = data.StudentSubjects.GroupBy(s => s.StudentName).Take(3);
                foreach (var group in grouped)
                {
                    prompt.AppendLine($"- {group.Key}: {string.Join(", ", group.Take(3).Select(s => s.SubjectName))}");
                }
                prompt.AppendLine();
            }

            if (data.TeacherClasses.Any())
            {
                prompt.AppendLine("**صفوف المدرسين:**");
                var grouped = data.TeacherClasses.GroupBy(t => t.TeacherName).Take(3);
                foreach (var group in grouped)
                {
                    prompt.AppendLine($"- {group.Key}: {string.Join(", ", group.Take(3).Select(t => t.ClassGroupName))}");
                }
                prompt.AppendLine();
            }

            if (data.StudentClasses.Any())
            {
                prompt.AppendLine("**صفوف الطلاب:**");
                var grouped = data.StudentClasses.GroupBy(s => s.StudentName).Take(3);
                foreach (var group in grouped)
                {
                    prompt.AppendLine($"- {group.Key}: في {group.First().ClassGroupName}");
                }
                prompt.AppendLine();
            }

            if (data.Counts.Any())
            {
                prompt.AppendLine("**الإحصائيات:**");
                foreach (var count in data.Counts)
                {
                    prompt.AppendLine($"- {count.Key}: {count.Value}");
                }
                prompt.AppendLine();
            }

            prompt.AppendLine($"**المستخدم:** {username ?? "موظف المدرسة"}");
            prompt.AppendLine($"**السؤال:** {question}");
            prompt.AppendLine($"**نوع السؤال:** {questionInfo.Type}");
            if (!string.IsNullOrEmpty(questionInfo.Context))
                prompt.AppendLine($"**السياق:** {questionInfo.Context}");

            prompt.AppendLine();
            prompt.AppendLine("**التعليمات:**");
            prompt.AppendLine("1. أجب باللغة العربية الفصحى");
            prompt.AppendLine("2. كن مفيداً ودقيقاً في الإجابة");
            prompt.AppendLine("3. استخدم المعلومات المقدمة أعلاه");
            prompt.AppendLine("4. إذا كانت المعلومات غير كافية، قل ذلك بأدب");
            prompt.AppendLine("5. قدم إجابة شاملة وواضحة");
            prompt.AppendLine("6. لا تكرر المعلومات بشكل ممل");
            prompt.AppendLine();
            prompt.AppendLine("**الرد الذكي:**");

            return prompt.ToString();
        }

        private string CleanResponseText(string text, string prompt)
        {
            if (text.Contains(prompt))
            {
                text = text.Replace(prompt, "").Trim();
            }

            if (text.Contains("**الرد الذكي:**"))
            {
                text = text.Split(new[] { "**الرد الذكي:**" }, StringSplitOptions.None).Last().Trim();
            }

            // إزالة أي نصوص زائدة
            var lines = text.Split('\n').Where(line =>
                !line.Contains("**التعليمات:**") &&
                !line.Contains("أجب باللغة العربية") &&
                !line.Contains("كن مفيداً") &&
                !line.Contains("استخدم المعلومات") &&
                !line.Contains("إذا كانت المعلومات") &&
                !line.Contains("قدم إجابة") &&
                !line.Contains("لا تكرر")
            ).ToArray();

            return string.Join("\n", lines).Trim();
        }

        private string GenerateLocalResponse(string question, QuestionInfo questionInfo, DatabaseData data)
        {
            var response = new StringBuilder();
            var lowerQuestion = question.ToLower();

            response.AppendLine("**إجابة المساعد الذكي:**");
            response.AppendLine();

            // بناء الرد بناءً على نوع السؤال والبيانات
            switch (questionInfo.Type)
            {
                case QuestionType.Greeting:
                    response.AppendLine($"مرحباً بك! 👋 أنا المساعد الذكي لنظام إدارة المدرسة.");
                    response.AppendLine("يمكنني مساعدتك في معرفة معلومات عن:");
                    response.AppendLine("- الطلاب والمدرسين والموظفين");
                    response.AppendLine("- الصفوف والشعب والمواد الدراسية");
                    response.AppendLine("- الإحصائيات والتقارير");
                    response.AppendLine("- أي استفسار آخر عن النظام");
                    response.AppendLine("كيف يمكنني مساعدتك اليوم؟");
                    break;

                case QuestionType.Help:
                    response.AppendLine("**ما يمكنني مساعدتك فيه:**");
                    response.AppendLine("📊 **الإحصائيات والأعداد:**");
                    response.AppendLine("   - كم عدد الطلاب؟");
                    response.AppendLine("   - كم عدد المدرسين؟");
                    response.AppendLine("   - ما هي إحصائيات المدرسة؟");
                    response.AppendLine();
                    response.AppendLine("👥 **الأسماء والمعلومات:**");
                    response.AppendLine("   - من هم الطلاب؟");
                    response.AppendLine("   - من هم المدرسون؟");
                    response.AppendLine("   - معلومات عن الموظفين");
                    response.AppendLine();
                    response.AppendLine("🏫 **الصفوف والشعب:**");
                    response.AppendLine("   - ما هي الصفوف المتاحة؟");
                    response.AppendLine("   - ما هي الشعب؟");
                    response.AppendLine("   - الصفوف المشكلة");
                    response.AppendLine();
                    response.AppendLine("📚 **المواد والعلاقات:**");
                    response.AppendLine("   - ما هي المواد الدراسية؟");
                    response.AppendLine("   - ما هي مواد الطالب [اسم]؟");
                    response.AppendLine("   - في أي صف الطالب [اسم]؟");
                    response.AppendLine("   - من مدرسو الصف [اسم الصف]؟");
                    break;

                case QuestionType.Details:
                    response.AppendLine("**المعلومات التفصيلية:**");
                    response.AppendLine();

                    if (data.Students.Any())
                    {
                        response.AppendLine($"👨‍🎓 **الطلاب ({data.Students.Count}):**");
                        foreach (var student in data.Students.Take(3))
                        {
                            response.AppendLine($"   - {student.Name} (العمر: {student.Age}، الحالة: {student.Status})");
                        }
                        if (data.Students.Count > 3)
                            response.AppendLine($"   - وهناك {data.Students.Count - 3} طالب آخر");
                        response.AppendLine();
                    }

                    if (data.Teachers.Any())
                    {
                        response.AppendLine($"👨‍🏫 **المدرسون ({data.Teachers.Count}):**");
                        foreach (var teacher in data.Teachers.Take(3))
                        {
                            response.AppendLine($"   - {teacher.Name} (التخصص: {teacher.Specialization})");
                        }
                        if (data.Teachers.Count > 3)
                            response.AppendLine($"   - وهناك {data.Teachers.Count - 3} مدرس آخر");
                        response.AppendLine();
                    }

                    if (data.Employees.Any())
                    {
                        response.AppendLine($"👨‍💼 **الموظفون ({data.Employees.Count}):**");
                        foreach (var employee in data.Employees.Take(3))
                        {
                            response.AppendLine($"   - {employee.Name} ({employee.JobTitle})");
                        }
                        if (data.Employees.Count > 3)
                            response.AppendLine($"   - وهناك {data.Employees.Count - 3} موظف آخر");
                        response.AppendLine();
                    }
                    break;

                case QuestionType.Count:
                    response.AppendLine("**الإحصائيات:**");
                    response.AppendLine();

                    if (data.Counts.Any())
                    {
                        foreach (var count in data.Counts)
                        {
                            response.AppendLine($"📊 {count.Key}: **{count.Value}**");
                        }
                    }
                    else
                    {
                        response.AppendLine("📊 **إحصائيات المدرسة:**");
                        if (data.Students.Any()) response.AppendLine($"   - عدد الطلاب: {data.Students.Count}");
                        if (data.Teachers.Any()) response.AppendLine($"   - عدد المدرسين: {data.Teachers.Count}");
                        if (data.Employees.Any()) response.AppendLine($"   - عدد الموظفين: {data.Employees.Count}");
                        if (data.Classes.Any()) response.AppendLine($"   - عدد الصفوف: {data.Classes.Count}");
                        if (data.Groups.Any()) response.AppendLine($"   - عدد الشعب: {data.Groups.Count}");
                        if (data.Subjects.Any()) response.AppendLine($"   - عدد المواد: {data.Subjects.Count}");
                    }
                    break;

                case QuestionType.General:
                default:
                    if (!string.IsNullOrEmpty(questionInfo.Context))
                    {
                        switch (questionInfo.Context)
                        {
                            case "student_subjects":
                                if (data.StudentSubjects.Any())
                                {
                                    response.AppendLine("**مواد الطلاب:**");
                                    var grouped = data.StudentSubjects.GroupBy(s => s.StudentName);
                                    foreach (var group in grouped.Take(5))
                                    {
                                        response.AppendLine($"📚 **{group.Key}:**");
                                        response.AppendLine($"   {string.Join("، ", group.Select(s => s.SubjectName))}");
                                    }
                                }
                                else
                                {
                                    response.AppendLine("لم أجد معلومات عن مواد الطلاب.");
                                }
                                break;

                            case "teacher_classes":
                                if (data.TeacherClasses.Any())
                                {
                                    response.AppendLine("**صفوف المدرسين:**");
                                    var grouped = data.TeacherClasses.GroupBy(t => t.TeacherName);
                                    foreach (var group in grouped.Take(5))
                                    {
                                        response.AppendLine($"🏫 **{group.Key}:**");
                                        response.AppendLine($"   يدرّس في: {string.Join("، ", group.Select(t => t.ClassGroupName))}");
                                    }
                                }
                                else
                                {
                                    response.AppendLine("لم أجد معلومات عن صفوف المدرسين.");
                                }
                                break;

                            case "student_classes":
                                if (data.StudentClasses.Any())
                                {
                                    response.AppendLine("**صفوف الطلاب:**");
                                    var grouped = data.StudentClasses.GroupBy(s => s.StudentName);
                                    foreach (var group in grouped.Take(5))
                                    {
                                        var first = group.First();
                                        response.AppendLine($"👨‍🎓 **{first.StudentName}:**");
                                        response.AppendLine($"   مسجل في: {first.ClassGroupName} ({first.EducationLevel})");
                                        response.AppendLine($"   الحالة: {first.Status}");
                                    }
                                }
                                else
                                {
                                    response.AppendLine("لم أجد معلومات عن صفوف الطلاب.");
                                }
                                break;
                        }
                    }
                    else
                    {
                        response.AppendLine($"**رد على سؤالك: \"{question}\"**");
                        response.AppendLine();

                        if (data.Students.Any() || data.Teachers.Any() || data.Employees.Any())
                        {
                            response.AppendLine("**لدي هذه المعلومات من قاعدة البيانات:**");

                            if (data.Students.Any())
                                response.AppendLine($"👨‍🎓 يوجد {data.Students.Count} طالب في النظام");

                            if (data.Teachers.Any())
                                response.AppendLine($"👨‍🏫 يوجد {data.Teachers.Count} مدرس في النظام");

                            if (data.Employees.Any())
                                response.AppendLine($"👨‍💼 يوجد {data.Employees.Count} موظف في النظام");

                            if (data.Classes.Any())
                                response.AppendLine($"🏫 يوجد {data.Classes.Count} صف دراسي");

                            if (data.Subjects.Any())
                                response.AppendLine($"📚 يوجد {data.Subjects.Count} مادة دراسية");
                        }
                        else
                        {
                            response.AppendLine("لم أتمكن من العثور على معلومات محددة في قاعدة البيانات.");
                            response.AppendLine("يمكنني مساعدتك في الحصول على معلومات عن:");
                            response.AppendLine("- الطلاب والمدرسين والموظفين");
                            response.AppendLine("- الصفوف والشعب والمواد");
                            response.AppendLine("- الإحصائيات والتقارير");
                        }
                    }
                    break;
            }

            // إضافة ملاحظة في النهاية
            response.AppendLine();
            response.AppendLine("---");
            response.AppendLine("💡 **ملاحظة:** يمكنك طرح أسئلة أكثر تحديداً للحصول على معلومات أدق.");
            response.AppendLine("   مثال: \"ما هي مواد الطالب أحمد؟\" أو \"كم عدد المدرسين؟\"");

            return response.ToString();
        }
    }

    // فئات المساعدة
    public class QuestionInfo
    {
        public QuestionType Type { get; set; }
        public List<string> Entities { get; set; } = new List<string>();
        public string SpecificName { get; set; }
        public int? SpecificNumber { get; set; }
        public string Context { get; set; }
    }

    public enum QuestionType
    {
        Details,
        Count,
        Location,
        Date,
        Method,
        Reason,
        Greeting,
        Help,
        General
    }

    public class DatabaseData
    {
        public List<Student> Students { get; set; } = new List<Student>();
        public List<Teacher> Teachers { get; set; } = new List<Teacher>();
        public List<Employee> Employees { get; set; } = new List<Employee>();
        public List<Class> Classes { get; set; } = new List<Class>();
        public List<Group> Groups { get; set; } = new List<Group>();
        public List<Subject> Subjects { get; set; } = new List<Subject>();
        public List<ClassGroup> ClassGroups { get; set; } = new List<ClassGroup>();
        public List<StudentSubject> StudentSubjects { get; set; } = new List<StudentSubject>();
        public List<TeacherClass> TeacherClasses { get; set; } = new List<TeacherClass>();
        public List<StudentClass> StudentClasses { get; set; } = new List<StudentClass>();
        public Dictionary<string, int> Counts { get; set; } = new Dictionary<string, int>();
    }

    public class Student
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string NationalNumber { get; set; }
        public int Age { get; set; }
        public string Status { get; set; }
        public string Phone { get; set; }
        public string Email { get; set; }
        public DateTime BirthDate { get; set; }
        public string FatherName { get; set; }
        public string Address { get; set; }
    }

    public class Teacher
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string NationalNumber { get; set; }
        public int Age { get; set; }
        public string Phone { get; set; }
        public int ExperienceYears { get; set; }
        public string Specialization { get; set; }
        public DateTime HireDate { get; set; }
        public int Salary { get; set; }
    }

    public class Employee
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string Username { get; set; }
        public string NationalNumber { get; set; }
        public string JobTitle { get; set; }
        public int Age { get; set; }
        public string Phone { get; set; }
        public int Salary { get; set; }
        public DateTime HireDate { get; set; }
    }

    public class Class
    {
        public Guid Id { get; set; }
        public string EducationLevel { get; set; }
        public string AdditionalInfo { get; set; }
        public DateTime CreatedDate { get; set; }
    }

    public class Group
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string AdditionalInfo { get; set; }
    }

    public class Subject
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public Guid ClassId { get; set; }
    }

    public class ClassGroup
    {
        public string FullClassName { get; set; }
        public int MaxStudents { get; set; }
        public Guid ClassId { get; set; }
        public Guid GroupId { get; set; }
    }

    public class StudentSubject
    {
        public string StudentName { get; set; }
        public string SubjectName { get; set; }
        public string ClassLevel { get; set; }
    }

    public class TeacherClass
    {
        public string TeacherName { get; set; }
        public string ClassGroupName { get; set; }
        public string EducationLevel { get; set; }
    }

    public class StudentClass
    {
        public string StudentName { get; set; }
        public string NationalNumber { get; set; }
        public string Status { get; set; }
        public string ClassGroupName { get; set; }
        public string EducationLevel { get; set; }
    }

    public class ChatResponse
    {
        [JsonProperty("generated_text")]
        public string GeneratedText { get; set; }
    }
}