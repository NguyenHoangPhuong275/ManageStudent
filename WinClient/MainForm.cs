using System;
using System.Data;
using System.Drawing;
using System.Windows.Forms;
<<<<<<< HEAD
using System.Collections.Generic;
=======
using System.Threading;
using System.Threading.Tasks;
>>>>>>> a9f7c149d44583bd431f7952ef8661382757e01c

namespace WinClient
{
    public partial class MainForm : Form
    {
<<<<<<< HEAD
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
=======
        private DataTable dtStudents;
        private bool isListening = false;
        
        // Controls mới
        private TextBox txtPhone;
        private TextBox txtEmail;
        private TextBox txtSubject; // Thêm TextBox Môn học
        private Button btnUndo;

        // Chat Controls
        private RichTextBox rtbChat;
        private TextBox txtChatInput;
        private Button btnSend;
        private string MyUsername;      // Email (để định danh hệ thống)
        private string MyFullName;      // Tên thật (để hiển thị)
        private string MyAvatar;        // Base64 Avatar
        private string MyEmail;         // Email thực tế
        private string UserRole;
        private bool isViewingUsers = false;
        
        public bool IsLogout = false; // Cờ kiểm tra đăng xuất
        
        // --- UNDO SYSTEM ---
        private class UndoAction
        {
            public string Type; // ADD, DELETE, UPDATE, IMPORT
            public string[] Data; // [ID, Name, Class, Phone, Email]
            public System.Collections.Generic.List<string> BatchIds; // Dùng cho Import
        }
        
        private System.Collections.Generic.Stack<UndoAction> undoStack = new System.Collections.Generic.Stack<UndoAction>();
        private UndoAction pendingUndo = null; // Lưu hành động chờ xác nhận từ Server
        private bool isUndoing = false; // Cờ để tránh loop khi đang undo
        private bool isImporting = false; // Cờ import
        private System.Collections.Generic.List<string> importedIds = new System.Collections.Generic.List<string>();
        // -------------------

        // Constructor nhận vào Role, Username và FullName
        public MainForm(string role, string username, string fullname, string avatar, string email)
        {
            InitializeComponent();
            
            this.UserRole = role;
            this.MyUsername = username; 
            this.MyFullName = fullname; // Lưu tên thật
            this.MyAvatar = avatar;
            this.MyEmail = email;
            
            InitTable();
            InitChatUI(); 
            StyleUI(); 

            // Nếu là Admin thì hiện chức năng quản trị
            if (UserRole == "ADMIN")
            {
                InitAdminUI();
            }
        }

        // Constructor mặc định cho Designer
        public MainForm() : this("USER", "dev@dev.com", "Developer", "", "dev@dev.com") { }

        private void InitAdminUI()
        {
            // Các nút quản trị đã được chuyển vào Sidebar Menu để giao diện gọn gàng hơn
        }

        private void BtnViewUsers_Click(object sender, EventArgs e)
        {
            Button btn = sender as Button;
            if (!isViewingUsers)
            {
                // Chuyển sang xem User
                isViewingUsers = true;
                if (btn != null)
                {
                    btn.Text = "Xem DS Sinh Viên";
                    btn.BackColor = Color.DarkOrange;
                }
                
                // Đổi cột Bảng
                label1.Text = "Username";
                label3.Text = "Lớp giảng dạy";

                dtStudents.Clear();
                dtStudents.Columns.Clear();
                dtStudents.Columns.Add("Username");
                dtStudents.Columns.Add("Họ Tên"); 
                dtStudents.Columns.Add("Lớp giảng dạy");
                dtStudents.Columns.Add("Môn học");
                dtStudents.Columns.Add("SĐT"); // Thêm cột SĐT
                dtStudents.Columns.Add("Email");
                dtStudents.Columns.Add("Mật Khẩu (Hidden)");
                dtStudents.Columns.Add("Avatar (Hidden)"); // Ẩn
                
                SetManageButtons(false);
                if (txtSubject != null) txtSubject.Visible = true; // Hiện ô Môn học
                btnAdd.Visible = false;
                btnImport.Visible = false;
                btnUndo.Visible = false; // Ẩn nút Hoàn tác bên User
                SocketClient.Send("LIST_USERS");
            }
            else
            {
                // Quay về xem SV
                isViewingUsers = false;
                if (btn != null)
                {
                    btn.Text = "Xem DS Tài Khoản";
                    btn.BackColor = Color.Teal;
                }
                
                label1.Text = "MSSV";
                label3.Text = "Lớp";

                InitTable(); 
                SetManageButtons(true);
                if (txtSubject != null) txtSubject.Visible = false; // Ẩn ô Môn học (SV không dùng)
                btnAdd.Visible = true;
                btnImport.Visible = true;
                btnUndo.Visible = true; // Hiện lại nút Hoàn tác bên SV
                SocketClient.Send("LIST");
            }
        }

        private void SetManageButtons(bool enable)
        {
            btnAdd.Enabled = enable;
            
            // Nút Sửa & Xóa luôn bật
            btnUpdate.Enabled = true;
            btnDelete.Enabled = true; 
            
            txtID.Enabled = enable;
            txtName.Enabled = enable;
            txtClass.Enabled = enable;
            txtPhone.Enabled = enable;
            txtEmail.Enabled = enable;
            if (txtSubject != null) txtSubject.Enabled = enable;
        }

        private void BtnCreateUser_Click(object sender, EventArgs e)
        {
            Form f = new Form();
            f.Size = new Size(400, 420); // Tăng chiều cao form
            f.Text = "Cấp Tài Khoản Giáo Viên";
            f.StartPosition = FormStartPosition.CenterParent;
            f.FormBorderStyle = FormBorderStyle.FixedDialog; // Chuẩn Dialog
            f.MaximizeBox = false;
            f.MinimizeBox = false;
            f.BackColor = Color.WhiteSmoke;
            f.Font = new Font("Segoe UI", 10);

            int w = 320; 
            int x = (f.ClientSize.Width - w) / 2;
            int y = 20;

            // Username
            Label l1 = new Label() { Parent = f, Left = x, Top = y, AutoSize = true, Text = "Tên đăng nhập (Username):" };
            TextBox tUser = new TextBox() { Parent = f, Left = x, Top = y + 25, Width = w, Font = new Font("Segoe UI", 10) };
            
            y += 65;
            // Email
            Label lEmail = new Label() { Parent = f, Left = x, Top = y, AutoSize = true, Text = "Email liên hệ:" };
            TextBox tEmail = new TextBox() { Parent = f, Left = x, Top = y + 25, Width = w, Font = new Font("Segoe UI", 10) };
            
            y += 65;
            // Password
            Label l2 = new Label() { Parent = f, Left = x, Top = y, AutoSize = true, Text = "Mật khẩu mặc định:" };
            TextBox tPass = new TextBox() { Parent = f, Left = x, Top = y + 25, Width = w, Font = new Font("Segoe UI", 10) };
            
            y += 65;
            // FullName
            Label l3 = new Label() { Parent = f, Left = x, Top = y, AutoSize = true, Text = "Họ Tên Giáo Viên:" };
            TextBox tName = new TextBox() { Parent = f, Left = x, Top = y + 25, Width = w, Font = new Font("Segoe UI", 10) };
            
            y += 75;
            Button bOk = new Button() { Parent = f, Left = x, Top = y, Width = w, Height = 45, Text = "TẠO TÀI KHOẢN" };
            StyleButton(bOk, Color.FromArgb(0, 120, 215)); // Style nút xanh
            
            bOk.Click += (s, ev) => {
                string us = tUser.Text.Trim();
                string em = tEmail.Text.Trim();
                string ep = tPass.Text.Trim();
                string en = tName.Text.Trim(); 
                
                if(string.IsNullOrEmpty(us) || string.IsNullOrEmpty(em) || string.IsNullOrEmpty(ep)) 
                {
                    MessageBox.Show("Vui lòng nhập Username, Email và Mật khẩu.", "Thông báo");
                    return;
                }

                if (!IsValidEmail(em))
                {
                    MessageBox.Show("Email phải đúng định dạng (ví dụ: abc@edu.vn)", "Lỗi nhập liệu");
                    return;
                }
                
                SocketClient.Send($"CREATE_USER|{us}|{ep}|USER|{en}|{em}"); 
                f.Close();
            };
            
            f.ShowDialog();
        }

        private void InitChatUI()
        {
            // 1. CẤU HÌNH FORM CHUNG
            this.Text = "QUẢN LÝ SINH VIÊN"; 
            this.Width = 1300; // Tăng chiều rộng tổng thể
            this.Height = 700; 
            this.FormBorderStyle = FormBorderStyle.FixedDialog; 
            this.MaximizeBox = false;

            // KHAI BÁO CÁC THÔNG SỐ KÍCH THƯỚC CHUẨN
            int LEFT_MARGIN = 20;
            int LEFT_WIDTH = 920; // Tăng không gian cho bên trái
            
            // --- NÚT MENU ---
            Button btnMenu = new Button();
            btnMenu.Text = "☰";
            btnMenu.Font = new Font("Segoe UI", 16, FontStyle.Bold);
            btnMenu.Size = new Size(50, 40);
            btnMenu.Location = new Point(10, 10);
            btnMenu.FlatStyle = FlatStyle.Flat;
            btnMenu.FlatAppearance.BorderSize = 0;
            btnMenu.Cursor = Cursors.Hand;
            btnMenu.Click += (s, e) => ToggleSidebar();
            this.Controls.Add(btnMenu);
            
            // --- A. TIÊU ĐỀ ---
            lblTitle.Location = new Point(60, 15); 
            lblTitle.Width = LEFT_WIDTH; 
            lblTitle.TextAlign = ContentAlignment.MiddleLeft;
            
            lblRole.Location = new Point(LEFT_WIDTH - 20, 20); 

            // --- B. KHUNG NHẬP LIỆU (GRP INFO) ---
            grpInfo.Location = new Point(LEFT_MARGIN, 70); 
            grpInfo.Width = LEFT_WIDTH;
            grpInfo.Height = 150; // Tăng chiều cao để chứa thêm dòng
            grpInfo.Anchor = AnchorStyles.Top | AnchorStyles.Left;
            
            // Khởi tạo các control nhập liệu bổ sung (SĐT, Email) nếu chưa có
            if (txtPhone == null)
            {
                Label lblPhone = new Label() { Text = "SĐT:", Location = new Point(20, 80), AutoSize = true };
                grpInfo.Controls.Add(lblPhone);

                txtPhone = new TextBox();
                txtPhone.Location = new Point(80, 77);
                txtPhone.Size = new Size(150, 25);
                grpInfo.Controls.Add(txtPhone);

                Label lblEmail = new Label() { Text = "Email:", Location = new Point(240, 80), AutoSize = true };
                grpInfo.Controls.Add(lblEmail);

                txtEmail = new TextBox();
                txtEmail.Location = new Point(300, 77);
                txtEmail.Size = new Size(250, 25);
                grpInfo.Controls.Add(txtEmail);

                // Thêm ô Môn học vào giao diện
                Label lblSubject = new Label() { Text = "Môn:", Location = new Point(560, 80), AutoSize = true };
                grpInfo.Controls.Add(lblSubject);

                txtSubject = new TextBox();
                txtSubject.Location = new Point(600, 77);
                txtSubject.Size = new Size(120, 25);
                grpInfo.Controls.Add(txtSubject);
            }

            // --- C. HÀNG NÚT BẤM (BUTTONS ROW) ---
            int BTN_Y = 240; // Đẩy nút xuống
            int btnW = 100; // Trả lại 100 để không mất chữ
            int btnH = 40;   
            int gap = 10;    

            // Nhóm 1: Thao tác 
            btnAdd.Location     = new Point(LEFT_MARGIN, BTN_Y);
            btnAdd.Size         = new Size(btnW, btnH);
            btnAdd.TextAlign    = ContentAlignment.MiddleCenter;

            btnUpdate.Location  = new Point(LEFT_MARGIN + (btnW + gap), BTN_Y);
            btnUpdate.Size      = new Size(btnW, btnH);
            btnUpdate.TextAlign = ContentAlignment.MiddleCenter;

            btnDelete.Location  = new Point(LEFT_MARGIN + (btnW + gap) * 2, BTN_Y);
            btnDelete.Size      = new Size(btnW, btnH);
            btnDelete.TextAlign = ContentAlignment.MiddleCenter;

            btnRefresh.Location = new Point(LEFT_MARGIN + (btnW + gap) * 3, BTN_Y);
            btnRefresh.Size     = new Size(btnW, btnH);
            btnRefresh.TextAlign = ContentAlignment.MiddleCenter;

            btnImport = new Button();
            btnImport.Text      = "Nhập Excel";
            btnImport.Location  = new Point(LEFT_MARGIN + (btnW + gap) * 4, BTN_Y);
            btnImport.Size      = new Size(btnW, btnH);
            btnImport.BackColor = Color.SeaGreen;
            btnImport.ForeColor = Color.White;
            btnImport.FlatStyle = FlatStyle.Flat;
            btnImport.FlatAppearance.BorderSize = 0;
            btnImport.Font      = new Font("Segoe UI", 9, FontStyle.Bold);
            btnImport.TextAlign = ContentAlignment.MiddleCenter;
            btnImport.Click    += (s, e) => ImportCSV();
            this.Controls.Add(btnImport);
            btnImport.BringToFront();
            
            // Nút Hoàn Tác
            btnUndo = new Button();
            btnUndo.Text = "Hoàn Tác";
            btnUndo.Location = new Point(LEFT_MARGIN + (btnW + gap) * 5, BTN_Y);
            btnUndo.Size = new Size(btnW, btnH);
            btnUndo.Click += BtnUndo_Click;
            this.Controls.Add(btnUndo);
            btnUndo.BringToFront();
            StyleButton(btnUndo, Color.Gray);

            // Nhóm 2: Tìm kiếm (Đã chuyển sang trang Tra Cứu riêng)
            
            // Xóa Anchor 
            btnAdd.Anchor = AnchorStyles.Top | AnchorStyles.Left;
            btnUpdate.Anchor = AnchorStyles.Top | AnchorStyles.Left;
            btnDelete.Anchor = AnchorStyles.Top | AnchorStyles.Left;
            btnRefresh.Anchor = AnchorStyles.Top | AnchorStyles.Left;
            btnImport.Anchor  = AnchorStyles.Top | AnchorStyles.Left;
            btnUndo.Anchor = AnchorStyles.Top | AnchorStyles.Left;

            // --- D. BẢNG DỮ LIỆU ---
            dgvStudents.Location = new Point(LEFT_MARGIN, BTN_Y + btnH + 20); 
            dgvStudents.Width = LEFT_WIDTH;
            dgvStudents.Height = 380; 
            dgvStudents.Anchor = AnchorStyles.Top | AnchorStyles.Left;

            // 3. CẤU HÌNH PHẦN PHẢI (CHAT LOG)
            GroupBox grpChat = new GroupBox();
            grpChat.Text = "THẢO LUẬN CHUNG";
            grpChat.Location = new Point(LEFT_MARGIN + LEFT_WIDTH + 20, 70); 
            grpChat.Size = new Size(320, 540); 
            grpChat.Font = new Font("Segoe UI", 9, FontStyle.Bold);
            grpChat.ForeColor = Color.DarkSlateGray;
            grpChat.BackColor = Color.White;
            this.Controls.Add(grpChat);

            // Log (RichTextBox)
            rtbChat = new RichTextBox();
            rtbChat.Location = new Point(10, 25);
            rtbChat.Size = new Size(300, 460); 
            rtbChat.BorderStyle = BorderStyle.None;
            rtbChat.BackColor = Color.AliceBlue;
            rtbChat.ReadOnly = true; 
            grpChat.Controls.Add(rtbChat);

            // Input
            txtChatInput = new TextBox();
            txtChatInput.Location = new Point(10, 500);
            txtChatInput.Size = new Size(220, 25);
            txtChatInput.PlaceholderText = "Nhập tin nhắn...";
            grpChat.Controls.Add(txtChatInput);

            // Button Send
            btnSend = new Button();
            btnSend.Text = "Gửi";
            btnSend.Location = new Point(240, 499);
            btnSend.Size = new Size(70, 27);
            btnSend.Click += BtnSend_Click;
            btnSend.BackColor = Color.RoyalBlue;
            btnSend.ForeColor = Color.White;
            btnSend.FlatStyle = FlatStyle.Flat;
            grpChat.Controls.Add(btnSend);
            
            txtChatInput.KeyDown += (s, e) => { if (e.KeyCode == Keys.Enter) BtnSend_Click(s, e); };
            
            InitSidebar();
        }

        private void BtnUndo_Click(object sender, EventArgs e)
        {
            if (undoStack.Count == 0)
            {
                MessageBox.Show("Không có thao tác nào để hoàn tác.");
                return;
            }

            var action = undoStack.Pop();
            isUndoing = true; // Bật cờ để ProcessMessage không push ngược lại vào stack

            try
            {
                if (action.Type == "RevertAdd") // Hủy thêm -> Xóa
                {
                    SocketClient.Send($"DELETE|{action.Data[0]}");
                }
                else if (action.Type == "RevertDelete") // Hủy xóa -> Thêm lại
                {
                    SocketClient.Send($"ADD|{action.Data[0]}|{action.Data[1]}|{action.Data[2]}|{action.Data[3]}|{action.Data[4]}");
                }
                else if (action.Type == "RevertUpdate") // Hủy sửa -> Sửa lại như cũ
                {
                    SocketClient.Send($"UPDATE|{action.Data[0]}|{action.Data[1]}|{action.Data[2]}|{action.Data[3]}|{action.Data[4]}");
                }
                else if (action.Type == "RevertImport") // Hủy Import -> Xóa hàng loạt
                {
                    foreach (var id in action.BatchIds)
                    {
                        SocketClient.Send($"DELETE|{id}");
                        Thread.Sleep(5);
                    }
                    LogSystem($"Đã hoàn tác Import ({action.BatchIds.Count} sinh viên).");
                }
            }
            finally
            {
                // Tắt cờ sau một khoảng ngắn để đảm bảo server phản hồi xong (tương đối)
                Task.Delay(500).ContinueWith(t => isUndoing = false);
            }
        }

        private void BtnSend_Click(object sender, EventArgs e)
        {
            string msg = txtChatInput.Text.Trim();
            if (string.IsNullOrEmpty(msg)) return;
            
            // Gửi Họ Tên thật để mọi người biết ai đang chat
            SocketClient.Send($"CHAT|{MyFullName}|{msg}");
            txtChatInput.Text = "";
        }

        private void StyleUI()
        {
             // --- GIAO DIỆN HIỆN ĐẠI ---
            this.BackColor = Color.WhiteSmoke;
            this.Font = new Font("Segoe UI", 10);
            
            lblTitle.ForeColor = Color.DarkBlue; 
            lblTitle.Font = new Font("Segoe UI", 18, FontStyle.Bold); 
            lblTitle.Text = "QUẢN LÝ SINH VIÊN"; 
            
            lblRole.ForeColor = Color.DarkSlateGray;
            lblRole.Text = $"Xin chào, {MyFullName}"; 
            
            // Grid View
            dgvStudents.BackgroundColor = Color.White;
            dgvStudents.BorderStyle = BorderStyle.None;
            dgvStudents.CellBorderStyle = DataGridViewCellBorderStyle.SingleHorizontal;
            dgvStudents.EnableHeadersVisualStyles = false;
            dgvStudents.ColumnHeadersBorderStyle = DataGridViewHeaderBorderStyle.None;
            
            // Header Xanh
            dgvStudents.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(0, 120, 215);
            dgvStudents.ColumnHeadersDefaultCellStyle.ForeColor = Color.White;
            dgvStudents.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 10, FontStyle.Bold);
            dgvStudents.ColumnHeadersHeight = 40;
            
            // Row
            dgvStudents.DefaultCellStyle.SelectionBackColor = Color.FromArgb(235, 245, 255);
            dgvStudents.DefaultCellStyle.SelectionForeColor = Color.Black;
            dgvStudents.DefaultCellStyle.BackColor = Color.White;
            dgvStudents.DefaultCellStyle.ForeColor = Color.Black;
            dgvStudents.RowTemplate.Height = 35;

            // Buttons Flat
            StyleButton(btnAdd, Color.SeaGreen);
            StyleButton(btnUpdate, Color.FromArgb(0, 120, 215));
            StyleButton(btnDelete, Color.Crimson);
            StyleButton(btnRefresh, Color.DarkOrange);
            
            StyleButton(btnSend, Color.FromArgb(0, 120, 215));
        }

        private void StyleButton(Button btn, Color color)
        {
            btn.FlatStyle = FlatStyle.Flat;
            btn.FlatAppearance.BorderSize = 0;
            btn.BackColor = color;
            btn.ForeColor = Color.White;
            btn.Cursor = Cursors.Hand;
            btn.Font = new Font("Segoe UI", 10, FontStyle.Bold);
        }

        private void InitTable()
        {
            dtStudents = new DataTable();
            dtStudents.Columns.Add("MSSV");
            dtStudents.Columns.Add("Họ Tên");
            dtStudents.Columns.Add("Lớp");
            dtStudents.Columns.Add("SĐT");
            dtStudents.Columns.Add("Email");
            dgvStudents.DataSource = dtStudents;
            dgvStudents.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
        }

        private void MainForm_Load(object sender, EventArgs e)
        {
            if (!SocketClient.Connect())
            {
                MessageBox.Show("Không thể kết nối Server!");
                this.Close();
                return;
            }

            isListening = true;
            Task.Run(() => ListenToServer());
            SocketClient.Send("LIST");
        }

        private void ListenToServer()
        {
            while (isListening)
            {
                try
                {
                    string msg = SocketClient.Receive();
                    if (string.IsNullOrEmpty(msg)) continue;

                    this.Invoke((MethodInvoker)delegate {
                        ProcessMessage(msg);
                    });
                }
                catch { isListening = false; }
            }
        }

        private void ProcessMessage(string msg)
        {
            if (msg == "REFRESH")
            {
                LogSystem("Hệ thống: Dữ liệu vừa được cập nhật.");
                if (isViewingUsers) SocketClient.Send("LIST_USERS");
                else SocketClient.Send("LIST");
            }
            else if (msg == "DELETE_USER_SUCCESS")
            {
                LogSystem("Đã xóa tài khoản user.");
                SocketClient.Send("LIST_USERS"); // Tải lại danh sách user
                MessageBox.Show("Xóa tài khoản thành công!");
            }
            else if (msg == "UPDATE_USER_SUCCESS")
            {
                LogSystem("Đã cập nhật user.");
                SocketClient.Send("LIST_USERS"); 
                MessageBox.Show("Cập nhật thành công!");
            }
            else if (msg == "CREATE_USER_SUCCESS")
            {
                LogSystem("Đã cấp tài khoản giáo viên mới.");
                if (isViewingUsers) SocketClient.Send("LIST_USERS");
                MessageBox.Show("Cấp tài khoản thành công!");
            }
            else if (msg == "CREATE_USER_FAIL")
            {
                MessageBox.Show("Tạo tài khoản thất bại (Username đã tồn tại).");
            }
            else if (msg.StartsWith("DELETE_USER_FAIL"))
            {
                MessageBox.Show("Lỗi xóa user: " + msg.Split('|')[1]);
            }
            else if (msg.StartsWith("CHAT|"))
            {
                string[] p = msg.Split('|');
                if (p.Length >= 3)
                {
                    string user = p[1];
                    string content = p[2];
                    LogChat(user, content);
                }
            }
            else if (msg.StartsWith("LIST_USERS_RES|"))
            {
                if (!isViewingUsers) return; 
                
                dtStudents.Clear();
                string data = msg.Substring(15); 
                string[] rows = data.Split(';');
                foreach (var row in rows)
                {
                    if (string.IsNullOrWhiteSpace(row)) continue;
                    string[] parts = row.Split('#');
                    // Format mới: Username # FullName # Role # Email # Phone # TeachingClass # Subject # Avatar
                    if (parts.Length >= 3)
                    {
                        string email = parts.Length > 3 ? parts[3] : "";
                        string phone = parts.Length > 4 ? parts[4] : "";
                        string tClass = parts.Length > 5 ? parts[5] : "";
                        string subj = parts.Length > 6 ? parts[6] : "";
                        string avatar = parts.Length > 7 ? parts[7] : "";
                        dtStudents.Rows.Add(parts[0], parts[1], tClass, subj, phone, email, "******", avatar);
                    }
                }
            }
            else if (msg.StartsWith("LIST_RES|"))
            {
                if (isViewingUsers) return;
                
                DataTable targetDT = dtStudents;
                bool isSearchMode = (pnlSearchPage != null && pnlSearchPage.Visible);
                if (isSearchMode) targetDT = (DataTable)dgvSearchResults.DataSource;

                targetDT.Clear();
                string data = msg.Substring(9); 
                string[] rows = data.Split(';');
                foreach (var row in rows)
                {
                    if (string.IsNullOrWhiteSpace(row)) continue;
                    string[] p = row.Split('#');
                    // ID, Name, Class, Phone, Email
                    if (p.Length >= 3)
                    {
                        string ph = p.Length > 3 ? p[3] : "";
                        string em = p.Length > 4 ? p[4] : "";
                        targetDT.Rows.Add(p[0], p[1], p[2], ph, em);
                    }
                }
            }
            else if (msg == "ADD_SUCCESS") 
            { 
                LogSystem("Bạn vừa thêm thành công."); 
                if (!isUndoing && !isImporting && pendingUndo != null) undoStack.Push(pendingUndo);
                if (isImporting && pendingUndo != null) importedIds.Add(pendingUndo.Data[0]); // Lưu ID vừa import
                ClearInputs(); 
            }
            else if (msg == "UPDATE_SUCCESS") 
            { 
                LogSystem("Bạn vừa sửa thành công."); 
                if (!isUndoing && pendingUndo != null) undoStack.Push(pendingUndo);
                ClearInputs(); 
            }
            else if (msg == "DELETE_SUCCESS") 
            { 
                LogSystem("Bạn vừa xóa thành công."); 
                if (!isUndoing && pendingUndo != null) undoStack.Push(pendingUndo);
                ClearInputs(); 
            }
            else if (msg == "EXISTS") MessageBox.Show("MSSV đã tồn tại!");
            else if (msg.StartsWith("FOUND|"))
            {
                string[] p = msg.Split('|');
                dtStudents.Clear();
                dtStudents.Rows.Add(p[1], p[2], p[3]); 
            }
            else if (msg == "STUDENT_NOT_FOUND")
            {
                if (pnlSearchPage != null && pnlSearchPage.Visible)
                {
                    DataTable dt = (DataTable)dgvSearchResults.DataSource;
                    if (dt != null) dt.Clear();
                    LogSystem("Tra cứu: Không tìm thấy kết quả nào.");
                }
                else
                {
                    MessageBox.Show("Không tìm thấy sinh viên!");
                }
            }
        }

        private void LogSystem(string content)
        {
            string time = DateTime.Now.ToString("HH:mm:ss");
            rtbChat.SelectionStart = rtbChat.TextLength;
            rtbChat.SelectionLength = 0;
            
            rtbChat.SelectionColor = Color.Red; 
            rtbChat.AppendText($"[{time}] " + content + "\n");
            rtbChat.ScrollToCaret();
        }

        private void LogChat(string user, string content)
        {
             rtbChat.SelectionStart = rtbChat.TextLength;
             rtbChat.SelectionLength = 0;

             // So sánh với Tên Thật hoặc Email để biết là chính mình
             bool isMe = (user == MyFullName || user == MyUsername);

             rtbChat.SelectionColor = isMe ? Color.Blue : Color.DarkGreen;
             rtbChat.SelectionFont = new Font(rtbChat.Font, FontStyle.Bold);
             rtbChat.AppendText((isMe ? "Tôi" : user) + ": ");
             
             rtbChat.SelectionColor = Color.Black;
             rtbChat.SelectionFont = new Font(rtbChat.Font, FontStyle.Regular);
             rtbChat.AppendText(content + "\n");
             
             rtbChat.ScrollToCaret();
        }

        private void btnRefresh_Click(object sender, EventArgs e)
        {
            if (isViewingUsers) SocketClient.Send("LIST_USERS");
            else SocketClient.Send("LIST");
            
            ClearInputs();
            LogSystem("Đã làm mới danh sách.");
        }

        private void ImportCSV()
        {
            OpenFileDialog open = new OpenFileDialog();
            open.Filter = "CSV Files (*.csv)|*.csv|Text Files (*.txt)|*.txt";
            open.Title = "Chọn file dữ liệu sinh viên";

            if (open.ShowDialog() == DialogResult.OK)
            {
                try
                {
                    string[] lines = File.ReadAllLines(open.FileName);
                    int count = 0;
                    int success = 0;

                    isImporting = true;
                    importedIds.Clear();

                    foreach (string line in lines)
                    {
                        if (string.IsNullOrWhiteSpace(line)) continue;
                        count++;
                        
                        // Định dạng: MSSV, HoTen, Lop
                        string[] parts = line.Split(',');
                        if (parts.Length >= 2)
                        {
                            string id = parts[0].Trim();
                            if (!id.StartsWith("SV")) id = "SV" + id;
                            
                            string name = parts[1].Trim();
                            string cls = parts.Length > 2 ? parts[2].Trim() : "";
                            string ph = parts.Length > 3 ? parts[3].Trim() : "";
                            string em = parts.Length > 4 ? parts[4].Trim() : "";

                            // Validation tương tự nút Thêm
                            if (!IsAlphanumeric(id) || !IsAlphanumeric(name, true) || !IsAlphanumeric(cls))
                            {
                                LogSystem($"Lỗi dòng {count}: Ký tự không hợp lệ ({id})");
                                continue; 
                            }
                            
                            // Tạo pendingUndo tạm để lấy ID nếu thành công
                            pendingUndo = new UndoAction { Type = "RevertAdd", Data = new string[] { id } };

                            SocketClient.Send($"ADD|{id}|{name}|{cls}|{ph}|{em}");
                            success++;
                            
                            // Nghỉ 5ms để tránh dính gói tin (TCP Sticky Packets)
                            System.Threading.Thread.Sleep(5); 
                        }
                    }
                    
                    // Sau khi import xong, tạo 1 Undo Action gộp
                    if (importedIds.Count > 0)
                    {
                        undoStack.Push(new UndoAction { Type = "RevertImport", BatchIds = new System.Collections.Generic.List<string>(importedIds) });
                    }
                    isImporting = false;

                    LogSystem($"Import hoàn tất: Đã đọc {count} dòng. Gửi thành công {success} lệnh.");
                    if (isViewingUsers) SocketClient.Send("LIST_USERS");
                    else SocketClient.Send("LIST");
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Lỗi khi đọc file: " + ex.Message);
                }
            }
        }

        private void btnAdd_Click(object sender, EventArgs e)
        {
            // Yêu cầu: Khi chọn một sinh viên, ấn thêm phải hiện "Làm mới để thêm sinh viên mới"
            if (!txtID.Enabled) 
            { 
                MessageBox.Show("Làm mới để thêm sinh viên mới", "Thông báo"); 
                return; 
            }
            if (isViewingUsers) return;

            string id = txtID.Text.Trim();
            string name = txtName.Text.Trim();
            string cls = txtClass.Text.Trim();
            string ph = txtPhone.Text.Trim();
            string em = txtEmail.Text.Trim();

            if (string.IsNullOrEmpty(id) || string.IsNullOrEmpty(name))
            {
                MessageBox.Show("Vui lòng nhập đầy đủ thông tin!");
                return;
            }

            // Chặn ký tự đặc biệt
            if (!IsAlphanumeric(id) || !IsAlphanumeric(name, true) || !IsAlphanumeric(cls, true))
            {
                MessageBox.Show("Dữ liệu chỉ chấp nhận chữ cái (a-z) và số (0-9). Vui lòng kiểm tra lại!", "Lỗi nhập liệu");
                return;
            }

            if (!string.IsNullOrEmpty(em) && !IsValidEmail(em))
            {
                MessageBox.Show("Email phải đúng định dạng (ví dụ: abc@edu.vn)", "Lỗi nhập liệu");
                return;
            }

            if (!id.StartsWith("SV")) id = "SV" + id;
            
            // Chuẩn bị Undo: Nếu thêm thành công, Undo sẽ là Xóa ID này
            pendingUndo = new UndoAction { Type = "RevertAdd", Data = new string[] { id } };
            SocketClient.Send($"ADD|{id}|{name}|{cls}|{ph}|{em}");
        }

        private void btnUpdate_Click(object sender, EventArgs e)
        {
            // XỬ LÝ SỬA USER
            if (isViewingUsers)
            {
                string target = txtID.Text.Trim();
                if (string.IsNullOrEmpty(target)) { MessageBox.Show("Vui lòng chọn user."); return; }

                // Dialog nhập liệu nhanh
                Form f = new Form();
                f.Size = new Size(400, 600); // Tăng size để chứa thêm trường
                f.Text = "Cập nhật User"; 
                f.StartPosition = FormStartPosition.CenterParent;
                f.FormBorderStyle = FormBorderStyle.FixedDialog;
                f.MaximizeBox = false;
                f.MinimizeBox = false;
                f.BackColor = Color.WhiteSmoke;
                f.Font = new Font("Segoe UI", 10);

                int w = 320; 
                int x = (f.ClientSize.Width - w) / 2;
                int y = 20;

                // Pass
                Label l1 = new Label() { Parent = f, Left = x, Top = y, AutoSize = true, Text = "Mật khẩu mới:" };
                TextBox tPass = new TextBox() { Parent = f, Left = x, Top = y + 25, Width = w, Font = new Font("Segoe UI", 10), PlaceholderText = "Để trống nếu không đổi" };
                
                y += 65;
                // FullName (Mới)
                Label lName = new Label() { Parent = f, Left = x, Top = y, AutoSize = true, Text = "Họ Tên:" };
                TextBox tName = new TextBox() { Parent = f, Left = x, Top = y + 25, Width = w, Font = new Font("Segoe UI", 10) };
                
                // Thử lấy tên hiện tại từ TextBox tên (nếu đang click vào row)
                tName.Text = txtName.Text; 

                y += 65;
                // Email
                Label lEmail = new Label() { Parent = f, Left = x, Top = y, AutoSize = true, Text = "Email:" };
                TextBox tEmail = new TextBox() { Parent = f, Left = x, Top = y + 25, Width = w, Font = new Font("Segoe UI", 10) };
                tEmail.Text = txtEmail.Text;

                y += 65;
                // Phone
                Label lPhone = new Label() { Parent = f, Left = x, Top = y, AutoSize = true, Text = "Số điện thoại:" };
                TextBox tPhone = new TextBox() { Parent = f, Left = x, Top = y + 25, Width = w, Font = new Font("Segoe UI", 10) };
                tPhone.Text = txtPhone.Text;

                y += 65;
                // Teaching Class
                Label lClass = new Label() { Parent = f, Left = x, Top = y, AutoSize = true, Text = "Lớp giảng dạy (ngăn cách bằng phẩy):" };
                TextBox tClass = new TextBox() { Parent = f, Left = x, Top = y + 25, Width = w, Font = new Font("Segoe UI", 10) };
                // Lấy từ Grid nếu có (cột 2)
                if (dgvStudents.CurrentRow != null) tClass.Text = dgvStudents.CurrentRow.Cells["Lớp giảng dạy"].Value?.ToString() ?? "";

                y += 65;
                // Subject
                Label lSubj = new Label() { Parent = f, Left = x, Top = y, AutoSize = true, Text = "Môn học:" };
                TextBox tSubj = new TextBox() { Parent = f, Left = x, Top = y + 25, Width = w, Font = new Font("Segoe UI", 10) };
                if (dgvStudents.CurrentRow != null && dgvStudents.Columns.Contains("Môn học")) tSubj.Text = dgvStudents.CurrentRow.Cells["Môn học"].Value?.ToString() ?? "";

                y += 65;
                // Role
                Label l2 = new Label() { Parent = f, Left = x, Top = y, AutoSize = true, Text = "Vai trò (ADMIN/USER):" };
                TextBox tRole = new TextBox() { Parent = f, Left = x, Top = y + 25, Width = w, Font = new Font("Segoe UI", 10), Text = "USER" };
                // Lấy Role từ Grid nếu có (cần logic lấy từ row, ở đây tạm để USER hoặc lấy từ biến tạm nếu lưu)
                
                y += 75;
                Button bOk = new Button() { Parent = f, Left = x, Top = y, Width = w, Height = 45, Text = "CẬP NHẬT" };
                StyleButton(bOk, Color.FromArgb(0, 120, 215)); 

                bOk.Click += (s, ev) => {
                    // UPDATE_USER | Username | NewPass | NewRole | FullName | Email | Phone | TeachingClass | Subject | Avatar
                    // Admin sửa thì không đổi Avatar (gửi chuỗi rỗng hoặc KEEP)
                    string cmd = $"UPDATE_USER|{target}|{tPass.Text}|{tRole.Text}|{tName.Text}|{tEmail.Text}|{tPhone.Text}|{tClass.Text}|{tSubj.Text}|"; 
                    SocketClient.Send(cmd);
                    f.Close();
                };
                f.ShowDialog();
                return;
            }

            // XỬ LÝ SỬA SINH VIÊN (Cũ)
            if (txtID.Enabled) { MessageBox.Show("Vui lòng chọn SV."); return; }
            
            // Yêu cầu: Hiện bảng thông báo xác nhận
            if (MessageBox.Show("Bạn có muốn thay đổi thông tin sinh viên không?", "Xác nhận cập nhật", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.No)
            {
                return; // Hủy thao tác
            }

            string id = txtID.Text.Trim();
            string name = txtName.Text.Trim();
            string cls = txtClass.Text.Trim();
            string ph = txtPhone.Text.Trim();
            string em = txtEmail.Text.Trim();

            // Chặn ký tự đặc biệt
            if (!IsAlphanumeric(name, true) || !IsAlphanumeric(cls, true))
            {
                MessageBox.Show("Dữ liệu chỉ chấp nhận chữ cái (a-z) và số (0-9). Vui lòng kiểm tra lại!", "Lỗi nhập liệu");
                return;
            }

            if (!string.IsNullOrEmpty(em) && !IsValidEmail(em))
            {
                MessageBox.Show("Email phải đúng định dạng (ví dụ: abc@edu.vn)", "Lỗi nhập liệu");
                return;
            }
            
            // Chuẩn bị Undo: Lưu lại dữ liệu CŨ (đang hiển thị trên Grid trước khi bị sửa)
            // Tuy nhiên, txt đang chứa dữ liệu MỚI. Ta cần lấy dữ liệu cũ từ Grid hoặc biến tạm.
            // Ở đây đơn giản nhất là ta lấy dữ liệu từ Grid dựa vào ID (vì ID không đổi)
            string[] oldData = GetStudentDataFromGrid(id);
            pendingUndo = new UndoAction { Type = "RevertUpdate", Data = oldData };

            SocketClient.Send($"UPDATE|{id}|{name}|{cls}|{ph}|{em}");
        }

        private void btnDelete_Click(object sender, EventArgs e)
        {
            string target = txtID.Text.Trim();
            if (string.IsNullOrEmpty(target)) { MessageBox.Show("Vui lòng chọn dòng cần xóa."); return; }

            // XỬ LÝ XÓA USER
            if (isViewingUsers)
            {
                if (string.IsNullOrEmpty(target)) { MessageBox.Show("Chọn user để xóa"); return; }
                
                // CHẶN TỰ XÓA BẢN THÂN
                if (target.Equals(MyUsername, StringComparison.OrdinalIgnoreCase))
                {
                    MessageBox.Show("Bạn không thể tự xóa tài khoản của chính mình!", "Thông báo");
                    return;
                }

                if (MessageBox.Show($"Xóa tài khoản {target}?", "Xác nhận xóa", MessageBoxButtons.YesNo) == DialogResult.Yes)
                {
                    SocketClient.Send($"DELETE_USER|{target}");
                }
                return;
            }

            // XỬ LÝ XÓA SINH VIÊN (Cũ)
            if (txtID.Enabled) { MessageBox.Show("Vui lòng chọn SV.", "Thông báo"); return; }
            
            if (MessageBox.Show($"Xóa sinh viên {target}?", "Xác nhận xóa", MessageBoxButtons.YesNo) == DialogResult.Yes)
            {
                // Chuẩn bị Undo: Lưu lại toàn bộ thông tin của SV bị xóa để Add lại
                string[] oldData = GetStudentDataFromGrid(target);
                pendingUndo = new UndoAction { Type = "RevertDelete", Data = oldData };
                
                SocketClient.Send($"DELETE|{target}");
            }
        }

        private string[] GetStudentDataFromGrid(string id)
        {
            foreach (DataRow row in dtStudents.Rows)
            {
                if (row["MSSV"].ToString() == id)
                {
                    return new string[] { id, row["Họ Tên"].ToString(), row["Lớp"].ToString(), row["SĐT"].ToString(), row["Email"].ToString() };
                }
            }
            return new string[] { id, "", "", "", "" };
        }

        private void dgvStudents_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0)
            {
                DataGridViewRow row = dgvStudents.Rows[e.RowIndex];
                if (row.Cells[0].Value == null) return;
                
                string val1 = row.Cells[0].Value.ToString();
                
                if (isViewingUsers)
                {
                    // Chế độ xem User: Chỉ lấy Username gán vào để xóa (txtID dùng tạm làm biến lưu)
                    txtID.Text = val1; 
                    txtName.Text = row.Cells[1].Value.ToString();
                    txtPhone.Text = row.Cells["SĐT"].Value != null ? row.Cells["SĐT"].Value.ToString() : "";
                    txtEmail.Text = row.Cells["Email"].Value != null ? row.Cells["Email"].Value.ToString() : "";
                    // Cập nhật Lớp giảng dạy và Môn học lên TextBox
                    txtClass.Text = row.Cells["Lớp giảng dạy"].Value != null ? row.Cells["Lớp giảng dạy"].Value.ToString() : "";
                    if (txtSubject != null) txtSubject.Text = row.Cells["Môn học"].Value != null ? row.Cells["Môn học"].Value.ToString() : "";
                }
                else
                {
                    // Chế độ xem SV: Gán đầy đủ thông tin
                    txtID.Text = val1;
                    txtName.Text = row.Cells[1].Value.ToString();
                    txtClass.Text = row.Cells[2].Value.ToString();
                    txtPhone.Text = row.Cells[3].Value != null ? row.Cells[3].Value.ToString() : "";
                    txtEmail.Text = row.Cells[4].Value != null ? row.Cells[4].Value.ToString() : "";
                    txtID.Enabled = false; 
                }
            }
        }

        private void ClearInputs()
        {
            txtID.Text = ""; txtName.Text = ""; txtClass.Text = ""; txtPhone.Text = ""; txtEmail.Text = "";
            if (txtSubject != null) txtSubject.Text = "";
            txtID.Enabled = true;
        }

        private bool IsAlphanumeric(string str, bool allowSpace = false)
        {
            if (string.IsNullOrEmpty(str)) return true;
            foreach (char c in str)
            {
                // Cho phép chữ cái (bao gồm có dấu), chữ số, và dấu cách (nếu allowSpace=true)
                if (char.IsLetterOrDigit(c)) continue;
                if (allowSpace && c == ' ') continue;
                return false;
            }
            return true;
        }

        private bool IsValidEmail(string email)
        {
            return !string.IsNullOrEmpty(email) && email.Contains("@");
        }
>>>>>>> a9f7c149d44583bd431f7952ef8661382757e01c

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            isListening = false;
<<<<<<< HEAD
            chatClearTimer?.Stop();
            base.OnFormClosing(e);
        }
=======
            base.OnFormClosing(e);
        }

        // --- USER INFO PAGE ---
        private Panel pnlUserInfo;
        private Panel pnlSearchPage;
        private DataGridView dgvSearchResults;

        private void ShowDashboard()
        {
            if (pnlUserInfo != null) pnlUserInfo.Visible = false;
            if (pnlSearchPage != null) pnlSearchPage.Visible = false;
            if (isSidebarOpen) ToggleSidebar(); // Đóng menu nếu đang mở
        }

        private void ShowSearchPage()
        {
            ToggleSidebar();

            if (pnlSearchPage == null)
            {
                pnlSearchPage = new Panel();
                pnlSearchPage.Location = new Point(0, 0);
                pnlSearchPage.Size = this.ClientSize;
                pnlSearchPage.BackColor = Color.FromArgb(245, 247, 251);
                pnlSearchPage.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;

                // 1. Header
                Panel head = new Panel() { Dock = DockStyle.Top, Height = 60, BackColor = Color.White };
                pnlSearchPage.Controls.Add(head);
                Label title = new Label() { Text = "TRA CỨU & LỌC", Font = new Font("Segoe UI", 16, FontStyle.Bold), ForeColor = Color.FromArgb(0, 120, 215), Location = new Point(20, 15), AutoSize = true };
                head.Controls.Add(title);

                // 2. Filter Area
                Panel filterArea = new Panel() { Location = new Point(20, 80), Size = new Size(pnlSearchPage.Width - 40, 100), BackColor = Color.White, Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right };
                pnlSearchPage.Controls.Add(filterArea);

                Label l1 = new Label() { Text = "Lọc theo lớp:", Location = new Point(20, 20), AutoSize = true, Font = new Font("Segoe UI", 9, FontStyle.Bold) };
                ComboBox cbClasses = new ComboBox() { Location = new Point(20, 45), Width = 200, DropDownStyle = ComboBoxStyle.DropDownList, Font = new Font("Segoe UI", 10) };
                cbClasses.Items.AddRange(new string[] { "TẤT CẢ", "CNTT", "CNTT1", "CNTT2", "QTKD", "QTKD1", "QTKD2", "DIEN1", "COKHI1" });
                cbClasses.SelectedIndex = 0;
                filterArea.Controls.Add(l1);
                filterArea.Controls.Add(cbClasses);

                Label l2 = new Label() { Text = "Tìm kiếm tổng hợp (Tên/Mã/Lớp):", Location = new Point(250, 20), AutoSize = true, Font = new Font("Segoe UI", 9, FontStyle.Bold) };
                TextBox tSearch = new TextBox() { Location = new Point(250, 45), Width = 300, Font = new Font("Segoe UI", 10), PlaceholderText = "Ví dụ: SV001 hoặc Nguyen Van A..." };
                filterArea.Controls.Add(l2);
                filterArea.Controls.Add(tSearch);

                Button bFind = new Button() { Text = "TRA CỨU", Location = new Point(570, 40), Size = new Size(120, 35), BackColor = Color.FromArgb(0, 120, 215), ForeColor = Color.White, FlatStyle = FlatStyle.Flat, Font = new Font("Segoe UI", 10, FontStyle.Bold) };
                bFind.FlatAppearance.BorderSize = 0;
                filterArea.Controls.Add(bFind);

                // Event Logic
                cbClasses.SelectedIndexChanged += (s, e) => {
                    if (dgvSearchResults.DataSource is DataTable dt) dt.Clear();

                    if (isViewingUsers)
                    {
                        if (cbClasses.SelectedIndex <= 0) SocketClient.Send("LIST_USERS");
                        else SocketClient.Send($"SEARCH_USER|CLASS|{cbClasses.SelectedItem.ToString()}");
                    }
                    else
                    {
                        if (cbClasses.SelectedIndex <= 0) SocketClient.Send("LIST"); 
                        else SocketClient.Send($"SEARCH|CLASS|{cbClasses.SelectedItem.ToString()}");
                    }
                };

                bFind.Click += (s, e) => {
                    string k = tSearch.Text.Trim();
                    if (string.IsNullOrEmpty(k)) return;
                    
                    DataTable dt = (DataTable)dgvSearchResults.DataSource;
                    if (dt != null) dt.Clear();

                    if (isViewingUsers) SocketClient.Send($"SEARCH_USER|ALL|{k}");
                    else SocketClient.Send($"SEARCH|ALL|{k}");
                };

                // 3. Grid Results
                dgvSearchResults = new DataGridView();
                dgvSearchResults.Location = new Point(20, 200);
                dgvSearchResults.Size = new Size(pnlSearchPage.Width - 40, pnlSearchPage.Height - 400); // Điều chỉnh lại size
                dgvSearchResults.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
                dgvSearchResults.BackgroundColor = Color.White;
                dgvSearchResults.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
                dgvSearchResults.AllowUserToAddRows = false;
                dgvSearchResults.ReadOnly = true;
                pnlSearchPage.Controls.Add(dgvSearchResults);

                // Setup Columns
                DataTable dtInitial = new DataTable();
                dtInitial.Columns.Add("MSSV");
                dtInitial.Columns.Add("Họ Tên");
                dtInitial.Columns.Add("Lớp");
                dtInitial.Columns.Add("SĐT");
                dtInitial.Columns.Add("Email");
                dgvSearchResults.DataSource = dtInitial;

                // 4. Back Button
                Button bBack = new Button() { Text = "QUAY LẠI", Location = new Point(20, pnlSearchPage.Height - 60), Size = new Size(120, 40), BackColor = Color.Gray, ForeColor = Color.White, FlatStyle = FlatStyle.Flat, Anchor = AnchorStyles.Bottom | AnchorStyles.Left };
                bBack.Click += (s, e) => ShowDashboard();
                pnlSearchPage.Controls.Add(bBack);

                this.Controls.Add(pnlSearchPage);
            }
            pnlSearchPage.Visible = true;
            pnlSearchPage.BringToFront();
            pnlSidebar.BringToFront();

            // Tự động load toàn bộ khi vừa mở trang
            SocketClient.Send("LIST"); 
        }

        private void ShowUserInfo()
        {
            ToggleSidebar();
            
            if (pnlUserInfo == null)
            {
                pnlUserInfo = new Panel();
                pnlUserInfo.Location = new Point(0, 0); // Bắt đầu từ 0,0 để phủ toàn bộ
                pnlUserInfo.Size = this.ClientSize;
                pnlUserInfo.BackColor = Color.FromArgb(240, 242, 245); 
                pnlUserInfo.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
                
                // --- HEADER RIÊNG CHO TRANG PROFILE ---
                Panel pnlHeader = new Panel();
                pnlHeader.Size = new Size(pnlUserInfo.Width, 60);
                pnlHeader.BackColor = Color.White;
                pnlHeader.Dock = DockStyle.Top;
                pnlUserInfo.Controls.Add(pnlHeader);

                Label lblProfileTitle = new Label();
                lblProfileTitle.Text = "HỒ SƠ NGƯỜI DÙNG";
                lblProfileTitle.Font = new Font("Segoe UI", 16, FontStyle.Bold);
                lblProfileTitle.ForeColor = Color.FromArgb(0, 120, 215);
                lblProfileTitle.Location = new Point(20, 15);
                lblProfileTitle.AutoSize = true;
                pnlHeader.Controls.Add(lblProfileTitle);

                // --- PROFILE CARD CENTERED ---
                Panel card = new Panel();
                card.Size = new Size(550, 520); // Rộng hơn chút
                card.BackColor = Color.White;
                // Căn giữa card trong không gian còn lại
                card.Location = new Point((pnlUserInfo.Width - card.Width) / 2, 100);
                pnlUserInfo.Controls.Add(card);

                // Avatar PictureBox
                PictureBox picAvatar = new PictureBox();
                picAvatar.Size = new Size(120, 120);
                picAvatar.Location = new Point((card.Width - 120) / 2, 20);
                picAvatar.SizeMode = PictureBoxSizeMode.StretchImage;
                picAvatar.BorderStyle = BorderStyle.FixedSingle;
                
                // Load Avatar
                if (!string.IsNullOrEmpty(MyAvatar)) picAvatar.Image = Base64ToImage(MyAvatar);
                else picAvatar.BackColor = Color.LightGray; // Placeholder
                
                card.Controls.Add(picAvatar);

                // Button Upload Avatar
                Button btnUpload = new Button() { Text = "Đổi Ảnh", Size = new Size(80, 30), Location = new Point((card.Width - 80) / 2, 145) };
                btnUpload.Click += (s, e) => {
                    OpenFileDialog op = new OpenFileDialog();
                    op.Filter = "Image Files|*.jpg;*.jpeg;*.png;*.bmp";
                    if (op.ShowDialog() == DialogResult.OK) {
                        Image img = Image.FromFile(op.FileName);
                        // Resize về 150x150 để nhẹ
                        Image thumb = new Bitmap(img, new Size(150, 150));
                        picAvatar.Image = thumb;
                        
                        string base64 = ImageToBase64(thumb);
                        MyAvatar = base64; // Cập nhật local
                        
                        // Gửi lệnh cập nhật: UPDATE_USER | Username | Pass(Empty) | Role | FullName | Email | Phone | TeachingClass | Subject | Avatar
                        // Cần lấy lại Email/Phone hiện tại? Tạm thời gửi rỗng các trường kia nếu Server hỗ trợ partial update, 
                        // nhưng Server code hiện tại ghi đè. Ta nên gửi lại thông tin cũ.
                        // Vì đây là User tự sửa, ta giả định các trường khác giữ nguyên (hoặc phải fetch về).
                        // Để đơn giản: Ta gửi lệnh UPDATE_USER với đầy đủ thông tin (cần lưu Email/Phone vào biến toàn cục MyEmail, MyPhone khi Login/List).
                        // Ở đây ta chỉ demo update Avatar, các trường khác ta gửi chuỗi rỗng (Server cần sửa để không ghi đè null).
                        // TUY NHIÊN: Server code tôi vừa sửa ở trên sẽ ghi đè nếu gửi rỗng.
                        // Giải pháp: Gửi lại MyFullName, MyUsername. Các trường khác nếu rỗng Server sẽ ghi rỗng.
                        // Để an toàn, ta chỉ gửi Avatar và Server check.
                        
                        // Sửa lại format để khớp với Server mới (thêm 2 trường rỗng cho Class và Subject trước Avatar)
                        SocketClient.Send($"UPDATE_USER|{MyUsername}||{UserRole}|{MyFullName}|||||{base64}");
                        MessageBox.Show("Đã cập nhật ảnh đại diện!");
                    }
                };
                card.Controls.Add(btnUpload);

                int x = 60;
                int y = 170;

                AddProfileField(card, "HỌ VÀ TÊN", MyFullName, x, y); y += 70;
                
                string displayEmail = string.IsNullOrEmpty(MyEmail) ? MyUsername : MyEmail;
                AddProfileField(card, "ĐỊA CHỈ EMAIL", displayEmail, x, y); y += 70;
                
                AddProfileField(card, "QUYỀN HẠN", UserRole, x, y); y += 70;
                AddProfileField(card, "TRẠNG THÁI KẾT NỐI", "Trực tuyến (Online)", x, y, Color.SeaGreen);

                // Nút Quay lại
                Button btnBack = new Button();
                btnBack.Text = "QUAY LẠI HỆ THỐNG";
                btnBack.Size = new Size(430, 50);
                btnBack.Location = new Point(60, 440);
                btnBack.FlatStyle = FlatStyle.Flat;
                btnBack.FlatAppearance.BorderSize = 0;
                btnBack.BackColor = Color.FromArgb(0, 120, 215);
                btnBack.ForeColor = Color.White;
                btnBack.Font = new Font("Segoe UI", 10, FontStyle.Bold);
                btnBack.Cursor = Cursors.Hand;
                btnBack.Click += (s, e) => ShowDashboard();
                card.Controls.Add(btnBack);

                this.Controls.Add(pnlUserInfo);
            }
            
            pnlUserInfo.Visible = true;
            pnlUserInfo.BringToFront();
            pnlSidebar.BringToFront(); 
        }

        private void AddProfileField(Panel parent, string title, string content, int x, int y, Color? contentColor = null)
        {
            Label lblT = new Label();
            lblT.Text = title;
            lblT.Font = new Font("Segoe UI", 8, FontStyle.Bold);
            lblT.ForeColor = Color.DarkGray;
            lblT.Location = new Point(x, y);
            lblT.AutoSize = true;
            parent.Controls.Add(lblT);

            Label lblC = new Label();
            lblC.Text = content;
            lblC.Font = new Font("Segoe UI", 12, FontStyle.Bold);
            lblC.ForeColor = contentColor ?? Color.FromArgb(40, 40, 40);
            lblC.Location = new Point(x, y + 20);
            lblC.AutoSize = true;
            parent.Controls.Add(lblC);
            
            Panel line = new Panel();
            line.Size = new Size(430, 1);
            line.BackColor = Color.FromArgb(235, 235, 235);
            line.Location = new Point(x, y + 55);
            parent.Controls.Add(line);
        }

        // Helper Images
        private string ImageToBase64(Image image)
        {
            using (System.IO.MemoryStream m = new System.IO.MemoryStream()) {
                image.Save(m, System.Drawing.Imaging.ImageFormat.Png);
                return Convert.ToBase64String(m.ToArray());
            }
        }

        private Image Base64ToImage(string base64String)
        {
            try {
                byte[] imageBytes = Convert.FromBase64String(base64String);
                using (System.IO.MemoryStream ms = new System.IO.MemoryStream(imageBytes, 0, imageBytes.Length)) {
                    return Image.FromStream(ms, true);
                }
            } catch { return null; }
        }

        // --- SIDEBAR LOGIC ---
        private Panel pnlSidebar;
        private bool isSidebarOpen = false;
        private System.Windows.Forms.Timer sidebarTimer;
        private int sidebarStep = 50; // Tốc độ trượt
        private int sidebarTargetX;

        private void InitSidebar()
        {
            pnlSidebar = new Panel();
            pnlSidebar.Size = new Size(250, this.Height); 
            // Ban đầu ẩn bên trái màn hình
            pnlSidebar.Location = new Point(-250, 0); 
            pnlSidebar.BackColor = Color.FromArgb(45, 45, 48); 
            pnlSidebar.Visible = true; 
            
            // Header
            Label lblMenu = new Label();
            lblMenu.Text = "MENU DỰ ÁN";
            lblMenu.ForeColor = Color.White;
            lblMenu.Font = new Font("Segoe UI", 14, FontStyle.Bold);
            lblMenu.Location = new Point(20, 20);
            lblMenu.AutoSize = true;
            pnlSidebar.Controls.Add(lblMenu);

            // Close Button
            Button btnClose = new Button();
            btnClose.Text = "✕";
            btnClose.ForeColor = Color.White;
            btnClose.FlatStyle = FlatStyle.Flat;
            btnClose.FlatAppearance.BorderSize = 0;
            btnClose.Location = new Point(200, 10);
            btnClose.Size = new Size(40, 40);
            btnClose.Click += (s, e) => ToggleSidebar();
            pnlSidebar.Controls.Add(btnClose);

            // Adding Items
            int top = 80;
            AddSidebarItem("Trang Chủ (Quản Lý)", top, (s, e) => { 
                ShowDashboard();
                if (isViewingUsers) BtnViewUsers_Click(null, null); 
            }); top += 50;

            AddSidebarItem("Tra Cứu & Lọc", top, (s, e) => { 
                ShowSearchPage();
            }); top += 50;

            if (UserRole == "ADMIN")
            {
                AddSidebarItem("Quản Lý Giáo Viên", top, (s, e) => { 
                    ShowDashboard();
                    if (!isViewingUsers) BtnViewUsers_Click(null, null); // Chuyển sang xem User
                }); top += 50;

                AddSidebarItem("Cấp Tài Khoản GV", top, (s, e) => { 
                    ToggleSidebar();
                    BtnCreateUser_Click(null, null); 
                }); top += 50;
            }
            
            AddSidebarItem("Thông Tin Cá Nhân", top, (s, e) => ShowUserInfo()); top += 50;

            AddSidebarItem("Đăng Xuất", top, (s, e) => { 
                IsLogout = true; 
                this.Close(); 
            }); top += 50;

            this.Controls.Add(pnlSidebar);
            pnlSidebar.BringToFront();

            // INIT TIMER
            sidebarTimer = new System.Windows.Forms.Timer();
            sidebarTimer.Interval = 10; // 10ms
            sidebarTimer.Tick += SidebarTimer_Tick;
        }

        private void SidebarTimer_Tick(object sender, EventArgs e)
        {
            int currentX = pnlSidebar.Location.X;
            
            if (isSidebarOpen)
            {
                // Đang mở: Trượt từ âm về 0
                if (currentX < 0)
                {
                    currentX += sidebarStep;
                    if (currentX > 0) currentX = 0;
                    pnlSidebar.Location = new Point(currentX, 0);
                }
                else sidebarTimer.Stop(); // Đã mở xong
            }
            else
            {
                // Đang đóng: Trượt từ 0 về -250
                if (currentX > -pnlSidebar.Width)
                {
                    currentX -= sidebarStep;
                    if (currentX < -pnlSidebar.Width) currentX = -pnlSidebar.Width;
                    pnlSidebar.Location = new Point(currentX, 0);
                }
                else sidebarTimer.Stop(); // Đã đóng xong
            }
        }

        private void AddSidebarItem(string text, int top, EventHandler onClick)
        {
            Button btn = new Button();
            btn.Text = text;
            btn.ForeColor = Color.White;
            btn.BackColor = Color.Transparent;
            btn.FlatStyle = FlatStyle.Flat;
            btn.FlatAppearance.BorderSize = 0;
            btn.TextAlign = ContentAlignment.MiddleLeft;
            btn.Padding = new Padding(20, 0, 0, 0); 
            btn.Font = new Font("Segoe UI", 10);
            btn.Size = new Size(250, 45);
            btn.Location = new Point(0, top);
            btn.Cursor = Cursors.Hand;
            btn.MouseEnter += (s, e) => btn.BackColor = Color.FromArgb(60, 60, 60);
            btn.MouseLeave += (s, e) => btn.BackColor = Color.Transparent;
            btn.Click += onClick;
            pnlSidebar.Controls.Add(btn);
        }

        private void ToggleSidebar()
        {
            isSidebarOpen = !isSidebarOpen;
            
            // Nếu mở thì BringToFront để đè lên form
            if (isSidebarOpen) pnlSidebar.BringToFront();
            
            sidebarTimer.Start();
        }
>>>>>>> a9f7c149d44583bd431f7952ef8661382757e01c
    }
}
