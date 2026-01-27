using System;
using System.Data;
using System.Drawing;
using System.Windows.Forms;
using System.Collections.Generic;

namespace WinClient
{
    public partial class MainForm : Form
    {
        // State & Data
        private DataTable dtStudents;
        private bool isListening = false;
        private string MyUsername;      // Email
        private string MyFullName;      // Actual Name
        private string UserRole;
        private bool isViewingUsers = false;
        public bool IsLogout = false;

        // UI Components
        private RichTextBox rtbChat;
        private TextBox txtChatInput;
        private Button btnSend;
        private Panel pnlSidebar;
        private bool isSidebarOpen = false;
        private System.Windows.Forms.Timer sidebarTimer;
        private System.Windows.Forms.Timer chatClearTimer;

        // Dynamic Panels
        private Panel pnlUserInfo;
        private Panel pnlSearchPage;
        private DataGridView dgvSearchResults;
        private ComboBox cbClasses;

        // Sidebar Speed
        private const int sidebarStep = 50;

        public MainForm(string role, string username, string fullname)
        {
            this.UserRole = role;
            this.MyUsername = username; 
            this.MyFullName = fullname;

            InitializeComponent(); // Designer (empty)
            InitCustomUI();        // Manual Refactored UI
            InitTable();
            
            if (UserRole == "ADMIN")
            {
                // Extra setup for admin if needed
            }

            // Chat Auto-clear timer (5 minutes = 300,000 ms)
            chatClearTimer = new System.Windows.Forms.Timer { Interval = 300000 };
            chatClearTimer.Tick += (s, e) => {
                rtbChat.Invoke(new Action(() => {
                    rtbChat.Clear();
                    LogSystem("Lịch sử chat đã được tự động dọn dẹp để tối ưu hệ thống.");
                }));
            };
            chatClearTimer.Start();
        }

        public MainForm() : this("USER", "dev@dev.com", "Developer") { }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            isListening = false;
            chatClearTimer?.Stop();
            base.OnFormClosing(e);
        }
    }
}
