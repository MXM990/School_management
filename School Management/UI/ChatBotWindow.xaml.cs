using School_Management.Services;
using System;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;

namespace School_Management.UI
{
    public partial class ChatBotWindow : Window
    {
        private SmartChatBotService _chatBotService;
        private string _username;
        private DispatcherTimer _typingTimer;

        public ObservableCollection<ChatMessage> Messages { get; set; }

        public ChatBotWindow(string connectionString, string username = null)
        {
            InitializeComponent();
            _chatBotService = new SmartChatBotService(connectionString);
            _username = username;
            Messages = new ObservableCollection<ChatMessage>();
            ChatMessagesControl.ItemsSource = Messages;

            InitializeTypingIndicator();
            AddWelcomeMessage();
        }

        private void InitializeTypingIndicator()
        {
            _typingTimer = new DispatcherTimer();
            _typingTimer.Interval = TimeSpan.FromMilliseconds(500);
            _typingTimer.Tick += TypingTimer_Tick;
        }

        private void AddWelcomeMessage()
        {
            var welcomeMessage = new ChatMessage
            {
                Sender = "المساعد الذكي",
                Message = "مرحباً! أنا مساعدك الذكي لنظام إدارة المدرسة. يمكنني مساعدتك في:\n" +
                         "• معلومات عن الطلاب\n" +
                         "• بيانات المدرسين\n" +
                         "• إحصائيات الصفوف والشعب\n" +
                         "• معلومات الموظفين\n" +
                         "• أي استفسار آخر عن النظام",
                Time = DateTime.Now.ToString("HH:mm"),
                BackgroundColor = "#E3F2FD"
            };

            Messages.Add(welcomeMessage);
        }

        private async void SendButton_Click(object sender, RoutedEventArgs e)
        {
            await SendMessage();
        }

        private async void MessageTextBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter && !Keyboard.IsKeyDown(Key.LeftShift))
            {
                await SendMessage();
                e.Handled = true;
            }
        }

        private async Task SendMessage()
        {
            var message = MessageTextBox.Text.Trim();
            if (string.IsNullOrWhiteSpace(message))
                return;

            // إضافة رسالة المستخدم
            Messages.Add(new ChatMessage
            {
                Sender = _username ?? "أنت",
                Message = message,
                Time = DateTime.Now.ToString("HH:mm"),
                BackgroundColor = "#E8F5E9"
            });

            MessageTextBox.Clear();
            ScrollToBottom();

            // إظهار مؤشر الكتابة
            ShowTypingIndicator();

            // الحصول على الرد من المساعد
            var response = await _chatBotService.GetResponseAsync(message, _username);

            // إزالة مؤشر الكتابة
            RemoveTypingIndicator();

            // إضافة رد المساعد
            Messages.Add(new ChatMessage
            {
                Sender = "المساعد الذكي",
                Message = response,
                Time = DateTime.Now.ToString("HH:mm"),
                BackgroundColor = "#F3E5F5"
            });

            ScrollToBottom();
        }

        private void ShowTypingIndicator()
        {
            Messages.Add(new ChatMessage
            {
                Sender = "المساعد الذكي",
                Message = "يكتب...",
                Time = DateTime.Now.ToString("HH:mm"),
                BackgroundColor = "#FFF3E0"
            });

            ScrollToBottom();
            _typingTimer.Start();
        }

        private void TypingTimer_Tick(object sender, EventArgs e)
        {
            var lastMessage = Messages[Messages.Count - 1];
            if (lastMessage.Sender == "المساعد الذكي" && lastMessage.Message.StartsWith("يكتب"))
            {
                lastMessage.Message = lastMessage.Message.EndsWith("...") ? "يكتب." : lastMessage.Message + ".";
                ChatMessagesControl.Items.Refresh();
            }
        }

        private void RemoveTypingIndicator()
        {
            _typingTimer.Stop();

            var lastMessage = Messages[Messages.Count - 1];
            if (lastMessage.Sender == "المساعد الذكي" && lastMessage.Message.Contains("يكتب"))
            {
                Messages.RemoveAt(Messages.Count - 1);
            }
        }

        private void ScrollToBottom()
        {
            ChatScrollViewer.ScrollToBottom();
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        protected override void OnMouseLeftButtonDown(MouseButtonEventArgs e)
        {
            base.OnMouseLeftButtonDown(e);
            this.DragMove();
        }
    }

    public class ChatMessage
    {
        public string Sender { get; set; }
        public string Message { get; set; }
        public string Time { get; set; }
        public string BackgroundColor { get; set; }
    }
}