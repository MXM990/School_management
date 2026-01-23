using System;
using System.Collections.Generic;
using Microsoft.Win32;
using System.Windows;
using System.Windows.Controls;
using System.Data.SqlClient;
using System.Text;

namespace School_Management.Control
{
    public partial class CreateDataBaseUI : Window
    {
        public CreateDataBaseUI()
        {
            InitializeComponent();
            Loaded += (s, e) => FindSqlServers();
        }

        private void btnFind_Click(object sender, RoutedEventArgs e)
        {
            FindSqlServers();
        }

        private void FindSqlServers()
        {
            try
            {
                lstServers.Items.Clear();
                txtStatus.Text = "جاري البحث...";
                resultPanel.Visibility = Visibility.Collapsed;
                btnTest.IsEnabled = false;
                btnCreate.IsEnabled = false;
                btnCopy.IsEnabled = false;

                var servers = new List<string>();

                // البحث في Registry عن سيرفرات SQL
                using (var key = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Microsoft\Microsoft SQL Server"))
                {
                    if (key != null)
                    {
                        var instances = key.GetValue("InstalledInstances") as string[];
                        if (instances != null)
                        {
                            foreach (var instance in instances)
                            {
                                servers.Add($".\\{instance}");
                            }
                        }
                    }
                }


                // إزالة التكرارات
                servers.Sort();

                if (servers.Count > 0)
                {
                    foreach (var server in servers)
                    {
                        lstServers.Items.Add(server);
                    }
                    txtStatus.Text = $"تم العثور على {servers.Count} سيرفر";
                }
                else
                {
                    lstServers.Items.Add("لا توجد سيرفرات SQL مثبتة");
                    txtStatus.Text = "لم يتم العثور على سيرفرات";
                }
            }
            catch (Exception ex)
            {
                ShowResult("❌", $"خطأ: {ex.Message}", "حدث خطأ أثناء البحث عن السيرفرات", "#FFCDD2");
                lstServers.Items.Add(".");
                lstServers.Items.Add(".\\SQLEXPRESS");
            }
        }

        private void lstServers_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (lstServers.SelectedItem != null)
            {
                string selectedServer = lstServers.SelectedItem.ToString();
                btnTest.IsEnabled = true;

                // تمكين زر الإنشاء إذا كانت جميع الحقول مملوءة
                CheckCreateButtonStatus();
            }
            else
            {
                btnTest.IsEnabled = false;
                btnCreate.IsEnabled = false;
            }
        }

        private void CheckCreateButtonStatus()
        {
            bool canCreate = lstServers.SelectedItem != null
                && !string.IsNullOrWhiteSpace(txtDatabaseName.Text)
                && !string.IsNullOrWhiteSpace(txtUsername.Text);

            btnCreate.IsEnabled = canCreate;
            btnCopy.IsEnabled = canCreate;
        }

        private void txtDatabaseName_TextChanged(object sender, TextChangedEventArgs e)
        {
            CheckCreateButtonStatus();
        }

        private void txtUsername_TextChanged(object sender, TextChangedEventArgs e)
        {
            CheckCreateButtonStatus();
        }

        private void txtPassword_PasswordChanged(object sender, RoutedEventArgs e)
        {
            CheckCreateButtonStatus();
        }

        private void btnTest_Click(object sender, RoutedEventArgs e)
        {
            if (lstServers.SelectedItem != null)
            {
                string serverName = lstServers.SelectedItem.ToString();
                TestConnection(serverName);
            }
        }

        private void btnCreate_Click(object sender, RoutedEventArgs e)
        {
            if (lstServers.SelectedItem != null)
            {
                string serverName = lstServers.SelectedItem.ToString();
                string dbName = txtDatabaseName.Text.Trim();

                // التحقق من صحة اسم قاعدة البيانات
                if (string.IsNullOrWhiteSpace(dbName))
                {
                    ShowResult("⚠️", "اسم قاعدة البيانات مطلوب", "الرجاء إدخال اسم قاعدة البيانات", "#FFF3CD");
                    return;
                }

                // إنشاء قاعدة البيانات
                CreateDatabase(serverName, dbName);
            }
        }

        private void TestConnection(string serverName)
        {
            try
            {
                string username = txtUsername.Text.Trim();
                string password = txtPassword.Password;
                string connectionString = "";

                if (string.IsNullOrEmpty(username))
                {
                    connectionString = $"Server={serverName};Integrated Security=True;Connection Timeout=5;";
                }
                else
                {
                    connectionString = $"Server={serverName};User Id={username};Password={password};Connection Timeout=5;";
                }

                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();


                    ShowResult("✅", "الاتصال ناجح", $"تم الاتصال بنجاح بالسيرفر: {serverName}", "#D4EDDA");

                }
            }
            catch (SqlException sqlEx)
            {
                ShowResult("❌", "فشل الاتصال", $"خطأ: {sqlEx.Message}\nتأكد من صحة اسم المستخدم وكلمة المرور", "#FFCDD2");
            }
            catch (Exception ex)
            {
                ShowResult("❌", "حدث خطأ", $"خطأ: {ex.Message}", "#FFCDD2");
            }
        }

        private void CreateDatabase(string serverName, string dbName)
        {
            try
            {
                string username = txtUsername.Text.Trim();
                string password = txtPassword.Password;
                string masterConnectionString = "";

                if (string.IsNullOrEmpty(username))
                {
                    masterConnectionString = $"Server={serverName};Integrated Security=True;Connection Timeout=5;";
                }
                else
                {
                    masterConnectionString = $"Server={serverName};User Id={username};Password={password};Connection Timeout=5;";
                }

                // أولاً: اختبار الاتصال وإنشاء قاعدة البيانات
                using (SqlConnection masterConnection = new SqlConnection(masterConnectionString))
                {
                    masterConnection.Open();

                    // التحقق من عدم وجود قاعدة بيانات بنفس الاسم
                    string checkDbQuery = $"SELECT COUNT(*) FROM sys.databases WHERE name = '{dbName}'";
                    using (SqlCommand checkCmd = new SqlCommand(checkDbQuery, masterConnection))
                    {
                        int dbExists = (int)checkCmd.ExecuteScalar();

                        if (dbExists > 0)
                        {
                            ShowResult("⚠️", "القاعدة موجودة", $"قاعدة البيانات '{dbName}' موجودة بالفعل على السيرفر", "#FFF3CD");
                            return;
                        }
                    }

                    // إنشاء قاعدة البيانات
                    string createDbQuery = $"CREATE DATABASE [{dbName}]";
                    using (SqlCommand createCmd = new SqlCommand(createDbQuery, masterConnection))
                    {
                        createCmd.ExecuteNonQuery();
                    }
                }

                // إنشاء اتصال جديد بقاعدة البيانات الجديدة
                string dbConnectionString = "";
                if (string.IsNullOrEmpty(username))
                {
                    dbConnectionString = $"Server={serverName};Database={dbName};Integrated Security=True;Connection Timeout=5;";
                }
                else
                {
                    dbConnectionString = $"Server={serverName};Database={dbName};User Id={username};Password={password};Connection Timeout=5;";
                }

                using (SqlConnection dbConnection = new SqlConnection(dbConnectionString))
                {
                    dbConnection.Open();

                    // إنشاء الجداول والإجراءات المخزنة
                    if (CreatetableProc.CreateAllTable(dbConnection) && CreatetableProc.CreateAllProcedures(dbConnection))
                    {
                        ShowResult("✅", "تم الإنشاء",
                            $"تم إنشاء قاعدة البيانات '{dbName}' بنجاح على السيرفر: {serverName}\n" +
                            $"✓ تم إنشاء جميع الجداول\n" +
                            $"✓ تم إنشاء جميع الإجراءات المخزنة\n" +
                            $"اسم المستخدم: {username}\n" +
                            $"سلسلة الاتصال:\n{dbConnectionString.Replace(password, "*****")}",
                            "#D4EDDA");
                    }
                    else
                    {
                        throw new Exception("لم يتمكن من إنشاء الجداول أو الإجراءات المخزنة");
                    }
                }
            }
            catch (SqlException sqlEx)
            {
                ShowResult("❌", "فشل الإنشاء", $"خطأ SQL: {sqlEx.Message}", "#FFCDD2");
            }
            catch (Exception ex)
            {
                ShowResult("❌", "حدث خطأ", $"خطأ: {ex.Message}", "#FFCDD2");
            }
        }

        private void ShowResult(string icon, string title, string details, string colorCode)
        {
            resultIcon.Text = icon;
            txtResult.Text = title;
            txtDetails.Text = details;
            resultPanel.Background = (System.Windows.Media.Brush)new System.Windows.Media.BrushConverter()
                .ConvertFromString(colorCode);
            resultPanel.Visibility = Visibility.Visible;

            // تمرير إلى الأعلى
            resultPanel.BringIntoView();
        }

        private void btnCopy_Click(object sender, RoutedEventArgs e)
        {
            if (lstServers.SelectedItem != null)
            {
                string serverName = lstServers.SelectedItem.ToString();
                string dbName = txtDatabaseName.Text.Trim();
                string username = txtUsername.Text.Trim();

                StringBuilder sb = new StringBuilder();
                sb.AppendLine($"سيرفر SQL: {serverName}");
                sb.AppendLine($"قاعدة البيانات: {dbName}");
                sb.AppendLine($"اسم المستخدم: {username}");
                sb.AppendLine($"كلمة المرور: [محمية]");

                string connectionInfo = sb.ToString();
                Clipboard.SetText(connectionInfo);

                ShowResult("📋", "تم النسخ", "تم نسخ معلومات الاتصال إلى الحافظة", "#D1ECF1");
            }
        }

        private void btnClear_Click(object sender, RoutedEventArgs e)
        {
            txtDatabaseName.Text = "SchoolDB";
            txtUsername.Text = "sa";
            txtPassword.Password = "";
            lstServers.SelectedItem = null;
            resultPanel.Visibility = Visibility.Collapsed;
        }

        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}