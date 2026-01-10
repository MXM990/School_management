using System;
using System.Collections.Generic;
using Microsoft.Win32;
using System.Windows;
using System.Windows.Controls;
using System.Data.SqlClient;

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
                testResultPanel.Visibility = Visibility.Collapsed;
                btnTest.IsEnabled = false;
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
                                if (instance == "MSSQLSERVER")
                                {
                                    servers.Add(".");
                                    servers.Add("(local)");
                                    servers.Add("localhost");
                                }
                                else
                                {
                                    servers.Add($".\\{instance}");
                                    servers.Add($"localhost\\{instance}");
                                }
                            }
                        }
                    }
                }

                // إضافة SQLEXPRESS إذا لم تكن موجودة
                if (!servers.Contains(".\\SQLEXPRESS"))
                {
                    servers.Add(".\\SQLEXPRESS");
                    servers.Add("localhost\\SQLEXPRESS");
                }

                // إزالة التكرارات
                var uniqueServers = new HashSet<string>(servers);
                var sortedServers = new List<string>(uniqueServers);
                sortedServers.Sort();

                if (sortedServers.Count > 0)
                {
                    foreach (var server in sortedServers)
                    {
                        lstServers.Items.Add(server);
                    }
                    txtStatus.Text = $"تم العثور على {sortedServers.Count} سيرفر";
                }
                else
                {
                    lstServers.Items.Add("لا توجد سيرفرات SQL مثبتة");
                    txtStatus.Text = "لم يتم العثور على سيرفرات";
                }
            }
            catch (Exception ex)
            {
                lstServers.Items.Add($"خطأ: {ex.Message}");
                txtStatus.Text = "حدث خطأ";
            }
        }

        private void lstServers_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (lstServers.SelectedItem != null)
            {
                string selectedServer = lstServers.SelectedItem.ToString();
                txtSelectedServer.Text = selectedServer;
                btnTest.IsEnabled = true;
                btnCopy.IsEnabled = true;
            }
            else
            {
                txtSelectedServer.Text = "لم يتم اختيار أي سيرفر";
                btnTest.IsEnabled = false;
                btnCopy.IsEnabled = false;
            }
        }

        private void btnTest_Click(object sender, RoutedEventArgs e)
        {
            if (lstServers.SelectedItem != null)
            {
                string serverName = lstServers.SelectedItem.ToString();
                TestConnection(serverName);
            }
        }

        private void TestConnection(string serverName)
        {
            try
            {
                txtTestResult.Text = "جاري اختبار الاتصال...";
                txtTestDetails.Text = serverName;
                testResultPanel.Visibility = Visibility.Visible;
                testResultPanel.Background = new System.Windows.Media.SolidColorBrush(
                    System.Windows.Media.Color.FromArgb(255, 232, 245, 232));

                // استخدام connection string للاتصال بالسيرفر
                string connectionString = $"Server={serverName};Integrated Security=True;Connection Timeout=3;";

                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();

                    // جلب معلومات السيرفر
                    using (SqlCommand command = new SqlCommand("SELECT @@VERSION", connection))
                    {
                        string version = command.ExecuteScalar().ToString();

                        // عرض الجزء الأول من الإصدار فقط
                        int firstLine = version.IndexOf('\n');
                        if (firstLine > 0)
                        {
                            version = version.Substring(0, firstLine);
                        }

                        txtTestResult.Text = "✅ الاتصال ناجح";
                        txtTestDetails.Text = $"السيرفر: {serverName}\nالإصدار: {version}";
                        testResultPanel.Background = new System.Windows.Media.SolidColorBrush(
                            System.Windows.Media.Color.FromArgb(255, 232, 245, 232));
                    }
                }
            }
            catch (SqlException sqlEx)
            {
                txtTestResult.Text = "❌ فشل الاتصال";
                txtTestDetails.Text = $"خطأ: {sqlEx.Message}\nالسيرفر: {serverName}";
                testResultPanel.Background = new System.Windows.Media.SolidColorBrush(
                    System.Windows.Media.Color.FromArgb(255, 255, 235, 238));
            }
            catch (Exception ex)
            {
                txtTestResult.Text = "⚠️ حدث خطأ";
                txtTestDetails.Text = $"خطأ: {ex.Message}\nالسيرفر: {serverName}";
                testResultPanel.Background = new System.Windows.Media.SolidColorBrush(
                    System.Windows.Media.Color.FromArgb(255, 255, 243, 224));
            }
        }

        private void btnCopy_Click(object sender, RoutedEventArgs e)
        {
            if (lstServers.SelectedItem != null)
            {
                string serverName = lstServers.SelectedItem.ToString();
                Clipboard.SetText(serverName);

                // عرض رسالة تأكيد
                txtTestResult.Text = "📋 تم النسخ";
                txtTestDetails.Text = $"تم نسخ اسم السيرفر: {serverName}";
                testResultPanel.Background = new System.Windows.Media.SolidColorBrush(
                    System.Windows.Media.Color.FromArgb(255, 224, 247, 250));
                testResultPanel.Visibility = Visibility.Visible;
            }
        }

        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}