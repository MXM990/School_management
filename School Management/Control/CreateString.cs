using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace School_Management.Control
{
    internal class CreateString
    {
        public static readonly string DBName = "";

        public static readonly string CreateTableEmployees = @"CREATE TABLE Employees (
                                                                EmployeeID UNIQUEIDENTIFIER ,
                                                                EmployeeName NVARCHAR(100) ,
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




    }
}
