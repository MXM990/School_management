using System;
using System.Collections.Generic;
using Microsoft.Win32;
using System.Windows;
using System.Windows.Controls;
using System.Data.SqlClient;
using System.Data;

namespace School_Management.Control
{
    public partial class SelectDatabaseUI : Window
    {
        // خصائص لإرجاع البيانات
        public string ConnectionString { get; private set; }
        public string SelectedDatabase { get; private set; }
        public string SelectedServer { get; private set; }
        public string Username { get; private set; }
        public bool UseWindowsAuth { get; private set; }

        public SelectDatabaseUI()
        {
            InitializeComponent();
            Loaded += (s, e) => FindSqlServers();
        }

        private void btnFindServers_Click(object sender, RoutedEventArgs e)
        {
            FindSqlServers();
        }


        private void FindSqlServers()
        {
            try
            {
                lstServers.Items.Clear();
                lstDatabases.Items.Clear();
                txtServerCount.Text = "(0)";
                txtDatabaseCount.Text = "(0)";

                var servers = new List<string>();

                // البحث في Registry عن سيرفرات SQL
                using (var key = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Microsoft\Microsoft SQL Server"))
                {
                    if (key != null)
                    {
                        var instances = key.GetValue("InstalledInstances") as string[];
                        if (instances != null && instances.Length > 0)
                        {
                            foreach (var instance in instances)
                            {
                                    servers.Add($".\\{instance}");
                            }
                        }
                        else
                        {
                            // إذا لم توجد أي مثيلات، نضيف الخيارات الأساسية
                            servers.Add(".");
                            servers.Add(".\\SQLEXPRESS");
                        }
                    }
                }

                servers.Sort();

                if (servers.Count > 0)
                {
                    foreach (var server in servers)
                    {
                        lstServers.Items.Add(server);
                    }
                    txtServerCount.Text = $"({servers.Count})";
                }
                else
                {
                    lstServers.Items.Add("لا توجد سيرفرات SQL مثبتة");
                    txtServerCount.Text = "(0)";
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"خطأ في البحث عن السيرفرات: {ex.Message}",
                              "خطأ", MessageBoxButton.OK, MessageBoxImage.Error);
                // إضافة الخيارات الأساسية في حالة الخطأ
                lstServers.Items.Add(".");
                lstServers.Items.Add(".\\SQLEXPRESS");
                txtServerCount.Text = "(2)";
            }
        }


        private void lstServers_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (lstServers.SelectedItem != null && !lstServers.SelectedItem.ToString().StartsWith("لا توجد"))
            {
                LoadDatabases();
            }
        }

        private void LoadDatabases()
        {
            try
            {
                lstDatabases.Items.Clear();
                txtDatabaseCount.Text = "(0)";
                btnTest.IsEnabled = false;
                btnConnect.IsEnabled = false;

                string serverName = lstServers.SelectedItem.ToString();
                string connectionString = "";

                if (chkWindowsAuth.IsChecked == true)
                {
                    connectionString = $"Server={serverName};Integrated Security=True;Connection Timeout=3;";
                }
                else
                {
                    string username = txtUsername.Text.Trim();
                    string password = txtPassword.Password;

                    if (string.IsNullOrEmpty(username))
                    {
                        connectionString = $"Server={serverName};Integrated Security=True;Connection Timeout=3;";
                    }
                    else
                    {
                        connectionString = $"Server={serverName};User Id={username};Password={password};Connection Timeout=3;";
                    }
                }

                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();

                    // جلب قواعد البيانات المتاحة
                    string query = @"
                        SELECT name, create_date, state_desc 
                        FROM sys.databases 
                        WHERE database_id > 4  
                        AND state = 0  
                        ORDER BY name";

                    using (SqlCommand command = new SqlCommand(query, connection))
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        int count = 0;
                        while (reader.Read())
                        {
                            string dbName = reader["name"].ToString();
                            lstDatabases.Items.Add(dbName);
                            count++;
                        }
                        txtDatabaseCount.Text = $"({count})";
                    }
                }
            }
            catch (SqlException sqlEx)
            {
                MessageBox.Show($"فشل في تحميل قواعد البيانات: {sqlEx.Message}\n" +
                              "تأكد من صحة بيانات المصادقة",
                              "خطأ", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"خطأ: {ex.Message}",
                              "خطأ", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void lstDatabases_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            UpdateButtons();
        }

        private void chkWindowsAuth_Checked(object sender, RoutedEventArgs e)
        {
            txtUsername.IsEnabled = false;
            txtPassword.IsEnabled = false;
            UpdateButtons();
        }

        private void chkWindowsAuth_Unchecked(object sender, RoutedEventArgs e)
        {
            txtUsername.IsEnabled = true;
            txtPassword.IsEnabled = true;
            UpdateButtons();
        }

        private void UpdateButtons()
        {
            bool canTest = lstServers.SelectedItem != null &&
                          lstDatabases.SelectedItem != null &&
                          !lstServers.SelectedItem.ToString().StartsWith("لا توجد");

            btnTest.IsEnabled = canTest;
            btnConnect.IsEnabled = canTest;
        }

        private void btnTest_Click(object sender, RoutedEventArgs e)
        {
            if (lstServers.SelectedItem != null && lstDatabases.SelectedItem != null)
            {
                string serverName = lstServers.SelectedItem.ToString();
                string databaseName = lstDatabases.SelectedItem.ToString();
                TestConnection(serverName, databaseName);
            }
        }

        private void TestConnection(string serverName, string databaseName)
        {
            try
            {
                string connectionString = BuildConnectionString(serverName, databaseName);

                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();

                    // الحصول على معلومات عن قاعدة البيانات
                    string query = @"
                        SELECT 
                            name,
                            create_date,
                            compatibility_level,
                            recovery_model_desc,
                            collation_name
                        FROM sys.databases 
                        WHERE name = @dbName";

                    using (SqlCommand cmd = new SqlCommand(query, connection))
                    {
                        cmd.Parameters.AddWithValue("@dbName", databaseName);
                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                string createDate = Convert.ToDateTime(reader["create_date"]).ToString("yyyy-MM-dd HH:mm");
                                string recoveryModel = reader["recovery_model_desc"].ToString();
                                string collation = reader["collation_name"].ToString();

                                MessageBox.Show(
                                    $"✅ الاتصال ناجح!\n\n" +
                                    $"السيرفر: {serverName}\n" +
                                    $"قاعدة البيانات: {databaseName}\n" +
                                    $"تاريخ الإنشاء: {createDate}\n" +
                                    $"نموذج الاستعادة: {recoveryModel}\n" +
                                    $"التجميع: {collation}",
                                    "نجاح الاتصال",
                                    MessageBoxButton.OK,
                                    MessageBoxImage.Information);
                            }
                        }
                    }
                }
            }
            catch (SqlException sqlEx)
            {
                MessageBox.Show($"❌ فشل الاتصال: {sqlEx.Message}",
                              "خطأ", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"❌ حدث خطأ: {ex.Message}",
                              "خطأ", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void btnConnect_Click(object sender, RoutedEventArgs e)
        {
            if (lstServers.SelectedItem != null && lstDatabases.SelectedItem != null)
            {
                SelectedServer = lstServers.SelectedItem.ToString();
                SelectedDatabase = lstDatabases.SelectedItem.ToString();
                UseWindowsAuth = chkWindowsAuth.IsChecked == true;
                Username = UseWindowsAuth ? "Windows Authentication" : txtUsername.Text.Trim();

                ConnectionString = BuildConnectionString(SelectedServer, SelectedDatabase);

                // اختبار الاتصال مرة أخرى قبل الإغلاق
                try
                {
                    using (SqlConnection connection = new SqlConnection(ConnectionString))
                    {
                        connection.Open();

                        // نجاح - إغلاق النافذة مع نتيجة إيجابية
                        this.DialogResult = true;
                        this.Close();
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"فشل الاتصال: {ex.Message}",
                                  "خطأ", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private string BuildConnectionString(string serverName, string databaseName)
        {
            if (chkWindowsAuth.IsChecked == true)
            {
                return $"Server={serverName};Database={databaseName};Integrated Security=True;Connection Timeout=5;";
            }
            else
            {
                string username = txtUsername.Text.Trim();
                string password = txtPassword.Password;
                return $"Server={serverName};Database={databaseName};User Id={username};Password={password};Connection Timeout=5;";
            }
        }

        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
            this.Close();
        }
    }
}