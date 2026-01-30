using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace School_Management.Control
{
    internal class CreateTableString
    {

        public static readonly string CreateTableEmployees = @"CREATE TABLE Employees (
                                                                EmployeeID UNIQUEIDENTIFIER ,
                                                                EmployeeName NVARCHAR(100) ,
                                                                Username NVARCHAR(50) UNIQUE,
                                                                Password NVARCHAR(100),
                                                                IsActive BIT DEFAULT 1 ,
                                                                NationalNumber NVARCHAR(20) UNIQUE ,
                                                                JobTitle NVARCHAR(50) ,
                                                                Age INT ,
                                                                PhoneNumber NVARCHAR(15) ,
                                                                Salary INT ,
                                                                HireDate DATE )";

        public static readonly string CreateTableTeachers = @"CREATE TABLE Teachers (
                                                                TeacherID UNIQUEIDENTIFIER ,
                                                                TeacherName NVARCHAR(100) ,
                                                                NationalNumber NVARCHAR(20)  ,
                                                                Specialization NVARCHAR(100) ,
                                                                Age INT ,
                                                                PhoneNumber NVARCHAR(15) ,
                                                                YearsOfExperience INT ,
                                                                Salary INT,
                                                                HireDate DATE)";

        public static readonly string CreateTableClasses = @"CREATE TABLE Classes (
                                                                ClassID UNIQUEIDENTIFIER ,
                                                                EducationLevel NVARCHAR(50) ,
                                                                AdditionalInfo NVARCHAR(500),
                                                                CreatedDate DATETIME)";

        public static readonly string CreateTableGroups = @"CREATE TABLE Groups (
                                                                GroupID UNIQUEIDENTIFIER ,
                                                                GroupName NVARCHAR(50) ,
                                                                AdditionalInfo NVARCHAR(500))";

        public static readonly string CreateTableSubjects = @"CREATE TABLE Subjects (
                                                                SubjectID UNIQUEIDENTIFIER ,
                                                                SubjectName NVARCHAR(100) ,
                                                                ClassID UNIQUEIDENTIFIER )";

        public static readonly string CreateTableStudents = @"CREATE TABLE Students (
                                                                StudentID UNIQUEIDENTIFIER ,
                                                                StudentName NVARCHAR(100) ,
                                                                NationalNumber NVARCHAR(20) UNIQUE ,
                                                                Age INT ,
                                                                FatherName NVARCHAR(100) ,
                                                                PhoneNumber NVARCHAR(15) ,
                                                                RegistrationStatus NVARCHAR(20)  ,
                                                                Address NVARCHAR(200),
                                                                Email NVARCHAR(100),
                                                                BirthDate DATE)";

        public static readonly string CreateTableClass_Group = @"CREATE TABLE Class_Group (
                                                                ClassGroupID UNIQUEIDENTIFIER ,
	                                                            fullClassName NVARCHAR(100),
                                                                ClassID UNIQUEIDENTIFIER ,
                                                                GroupID UNIQUEIDENTIFIER ,
                                                                MaxStudents INT )";

        public static readonly string CreateTableTeacher_Class_Group = @"CREATE TABLE Teacher_Class_Group (
                                                                RegterTeacherClassID UNIQUEIDENTIFIER,
                                                                TeacherID UNIQUEIDENTIFIER ,
                                                                ClassGroupID UNIQUEIDENTIFIER )";

        public static readonly string CreateTableStudent_Class_Group = @"CREATE TABLE Student_Class_Group (
                                                                RegterStudentClassID UNIQUEIDENTIFIER,
                                                                StudentID UNIQUEIDENTIFIER ,
                                                                ClassGroupID UNIQUEIDENTIFIER)";


        public static readonly string CreateTableStudentSubjectsRegster = @"CREATE TABLE StudentSubjectsRegster (
                                                                StudentSubjectsID UNIQUEIDENTIFIER ,
                                                                StudentID UNIQUEIDENTIFIER  ,
                                                                SubjectID UNIQUEIDENTIFIER ,
																ClassID UNIQUEIDENTIFIER)";



        public static List<string> CreateTables = new List<string> { CreateTableEmployees, CreateTableTeachers, CreateTableClasses, CreateTableGroups, CreateTableSubjects, CreateTableStudents, CreateTableClass_Group, CreateTableTeacher_Class_Group , CreateTableStudent_Class_Group };

    }
    public class ProcInsertString
    {
        public static readonly string InsertNewEmployee = @"CREATE PROC InsertNewEmployee 
                                                        @EmployeeName NVARCHAR(100),
                                                        @Username NVARCHAR(50) ,
                                                        @Password NVARCHAR(100) ,
                                                        @NationalNumber NVARCHAR(20),
                                                        @JobTitle NVARCHAR(50),
                                                        @Age INT,
                                                        @PhoneNumber NVARCHAR(15),
                                                        @Salary INT,
                                                        @HireDate DATE 
                                                         AS      
                                                        BEGIN
                                                        INSERT INTO Employees (EmployeeID, EmployeeName,Username, Password, NationalNumber, JobTitle, Age, PhoneNumber, Salary, HireDate)
                                                        VALUES (NEWID(), @EmployeeName,@Username,@Password, @NationalNumber, @JobTitle, @Age, @PhoneNumber, @Salary, @HireDate)
                                                        END";

        public static readonly string InsertNewTeacher = @"CREATE PROC InsertNewTeacher
                                                        @TeacherName NVARCHAR(100),
                                                        @NationalNumber NVARCHAR(20),
                                                        @Specialization NVARCHAR(100),
                                                        @Age INT,
                                                        @PhoneNumber NVARCHAR(15),
                                                        @YearsOfExperience INT,
                                                        @Salary INT,
                                                        @HireDate DATE
                                                         AS
                                                        BEGIN
                                                        INSERT INTO Teachers (TeacherID, TeacherName, NationalNumber, Specialization, Age, PhoneNumber, YearsOfExperience, Salary, HireDate)
                                                        VALUES (NEWID(), @TeacherName, @NationalNumber, @Specialization, @Age, @PhoneNumber, @YearsOfExperience, @Salary, @HireDate)
                                                        END";

        public static readonly string InsertNewClass = @"CREATE PROC InsertNewClass 
                                                          @EducationLevel NVARCHAR(50),
                                                          @AdditionalInfo NVARCHAR(500) = NULL,
                                                          @CreatedDate DATETIME
                                                           AS      
                                                           BEGIN
                                                               INSERT INTO Classes (ClassID, EducationLevel, AdditionalInfo, CreatedDate)
                                                               VALUES (NEWID(), @EducationLevel, @AdditionalInfo, @CreatedDate)
                                                           END";

        public static readonly string InsertNewGroup = @"CREATE PROC InsertNewGroup
                                                        @GroupName NVARCHAR(50),
                                                        @AdditionalInfo NVARCHAR(500)
                                                         AS
                                                        BEGIN
                                                        INSERT INTO Groups (GroupID, GroupName, AdditionalInfo)
                                                        VALUES (NEWID(), @GroupName, @AdditionalInfo)
                                                        END";

        public static readonly string InsertNewSubject = @"CREATE PROC InsertNewSubject
                                                        @SubjectName NVARCHAR(100),
                                                        @ClassID UNIQUEIDENTIFIER
                                                         AS
                                                        BEGIN
                                                        INSERT INTO Subjects (SubjectID, SubjectName, ClassID)
                                                        VALUES (NEWID(), @SubjectName, @ClassID)
                                                        END";

        public static readonly string InsertNewStudent = @"CREATE PROC InsertNewStudent
                                                        @StudentName NVARCHAR(100),
                                                        @NationalNumber NVARCHAR(20),
                                                        @Age INT,
                                                        @FatherName NVARCHAR(100),
                                                        @PhoneNumber NVARCHAR(15),
                                                        @RegistrationStatus NVARCHAR(20),
                                                        @Address NVARCHAR(200),
                                                        @Email NVARCHAR(100),
                                                        @BirthDate DATE
                                                         AS
                                                        BEGIN
                                                        INSERT INTO Students (StudentID, StudentName, NationalNumber, Age, FatherName, PhoneNumber, RegistrationStatus, Address, Email, BirthDate)
                                                        VALUES (NEWID(), @StudentName, @NationalNumber, @Age, @FatherName, @PhoneNumber, @RegistrationStatus, @Address, @Email, @BirthDate)
                                                        END";

        public static readonly string InsertNewClassGroup = @"CREATE PROC InsertClassGroup
                                                             @fullClassName NVARCHAR(100),
                                                             @ClassID UNIQUEIDENTIFIER,
                                                             @GroupID UNIQUEIDENTIFIER,
                                                             @MaxStudents INT
                                                         AS      
                                                         BEGIN
                                                           
                                                             IF EXISTS (SELECT 1 FROM Class_Group WHERE ClassID = @ClassID AND GroupID = @GroupID)
                                                             BEGIN
                                                                 RAISERROR ('هذا الصف والشعبة مجتمعان مسبقاً', 16, 1)
                                                                 RETURN
                                                             END
                                                             
                                                       
                                                             INSERT INTO Class_Group (ClassGroupID, fullClassName, ClassID, GroupID, MaxStudents)
                                                             VALUES (NEWID(), @fullClassName, @ClassID, @GroupID, @MaxStudents)
                                                         END";

        public static readonly string InsertNewTeacherClassGroup = @"CREATE PROC InsertNewTeacherClassGroup
                                                        @TeacherID UNIQUEIDENTIFIER,
                                                        @ClassGroupID UNIQUEIDENTIFIER
                                                         AS
                                                        BEGIN
                                                        INSERT INTO Teacher_Class_Group (RegterTeacherClassID, TeacherID, ClassGroupID)
                                                        VALUES (NEWID(), @TeacherID, @ClassGroupID)
                                                        END";

         public static readonly string InsertNewStudentClassGroup = @"CREATE PROC AssignStudentToClassGroup
                                                                     @StudentID UNIQUEIDENTIFIER,
                                                                     @ClassGroupID UNIQUEIDENTIFIER
                                                                 AS      
                                                                 BEGIN
                                                                    
                                                                     IF EXISTS (SELECT 1 FROM Student_Class_Group WHERE StudentID = @StudentID AND ClassGroupID = @ClassGroupID)
                                                                     BEGIN
                                                                         RAISERROR ('الطالب مسجل مسبقاً في هذا الصف/الشعبة', 16, 1)
                                                                         RETURN
                                                                     END
                                                                     
                                                                     DECLARE @CurrentCount INT, @MaxStudents INT
                                                                     
                                                                     SELECT @CurrentCount = COUNT(*), @MaxStudents = cg.MaxStudents
                                                                     FROM Student_Class_Group scg
                                                                     INNER JOIN Class_Group cg ON scg.ClassGroupID = cg.ClassGroupID
                                                                     WHERE scg.ClassGroupID = @ClassGroupID
                                                                     GROUP BY cg.MaxStudents
                                                                     
                                                                     IF @CurrentCount >= @MaxStudents
                                                                     BEGIN
                                                                         RAISERROR ('الصف/الشعبة قد وصل للعدد الأقصى من الطلاب', 16, 1)
                                                                         RETURN
                                                                     END
                                                                     
                                                                     INSERT INTO Student_Class_Group (RegterStudentClassID, StudentID, ClassGroupID)
                                                                     VALUES (NEWID(), @StudentID, @ClassGroupID)
                                                                 END";


        public static readonly string InsertStudentSubjectsRegster = @"CREATE PROC InsertStudentSubjectsRegster
                                                             @StudentID UNIQUEIDENTIFIER,
                                                             @SubjectID UNIQUEIDENTIFIER,
                                                             @ClassID UNIQUEIDENTIFIER
                                                         AS      
                                                        
                                                       
                                                             INSERT INTO StudentSubjectsRegster (StudentSubjectsID, StudentID, SubjectID, ClassID)
                                                             VALUES (NEWID(), @StudentID, @SubjectID, @ClassID )";


        public static List<string> CreateProceduresCommands = new List<string> { InsertNewEmployee, InsertNewTeacher  , InsertNewClass , InsertNewGroup , InsertNewSubject  , InsertNewStudent , InsertNewClassGroup , InsertNewTeacherClassGroup , InsertNewStudentClassGroup };

    }
    public enum AllPrc
    {
        InsertNewEmployee , InsertNewTeacher , InsertNewClass , InsertNewGroup , InsertNewSubject , InsertNewStudent ,InsertNewClassGroup ,InsertNewTeacherClassGroup , InsertNewStudentClassGroup
    }

}
