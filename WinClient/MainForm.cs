using System;
using System.Data;
using System.Drawing;
using System.Windows.Forms;
using System.Threading;
using System.Threading.Tasks;

namespace WinClient
{
    public partial class MainForm : Form
    {
        private DataTable dtStudents;
        private bool isListening = false;
        
        // Chat Controls
        private RichTextBox rtbChat;
        private TextBox txtChatInput;
        private Button btnSend;
        private string MyUsername;      // Email (để định danh hệ thống)
        private string MyFullName;      // Tên thật (để hiển thị)
        private string UserRole;
        private bool isViewingUsers = false;
        
        public bool IsLogout = false; // Cờ kiểm tra đăng xuất
        
        // Constructor nhận vào Role, Username và FullName
        public MainForm(string role, string username, string fullname)
        {
            InitializeComponent();
            
            this.UserRole = role;
            this.MyUsername = username; 
            this.MyFullName = fullname; // Lưu tên thật
            
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
        public MainForm() : this("USER", "dev@dev.com", "Developer") { }

        private void InitAdminUI()
        {
            Button btnCreateUser = new Button();
            btnCreateUser.Text = "Cấp Quyền GV";
            btnCreateUser.Size = new Size(130, 30);
            btnCreateUser.Location = new Point(680, 15); 
            // Style đẹp
            btnCreateUser.BackColor = Color.Firebrick;
            btnCreateUser.ForeColor = Color.White;
            btnCreateUser.FlatStyle = FlatStyle.Flat;
            btnCreateUser.FlatAppearance.BorderSize = 0;
            btnCreateUser.Font = new Font("Segoe UI", 9, FontStyle.Bold);
            
            btnCreateUser.Click += BtnCreateUser_Click;
            btnCreateUser.Anchor = AnchorStyles.Top | AnchorStyles.Right; 
            this.Controls.Add(btnCreateUser);
            btnCreateUser.BringToFront();

            Button btnViewUsers = new Button();
            btnViewUsers.Text = "Xem DS Tài Khoản";
            btnViewUsers.Size = new Size(130, 30);
            btnViewUsers.Location = new Point(540, 15); 
            
            btnViewUsers.BackColor = Color.Teal;
            btnViewUsers.ForeColor = Color.White;
            btnViewUsers.FlatStyle = FlatStyle.Flat;
            btnViewUsers.FlatAppearance.BorderSize = 0;
            btnViewUsers.Font = new Font("Segoe UI", 9, FontStyle.Bold);

            btnViewUsers.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            btnViewUsers.Click += BtnViewUsers_Click;
            this.Controls.Add(btnViewUsers);
            btnViewUsers.BringToFront();
        }

        private void BtnViewUsers_Click(object sender, EventArgs e)
        {
            Button btn = (Button)sender;
            if (!isViewingUsers)
            {
                // Chuyển sang xem User
                isViewingUsers = true;
                btn.Text = "Xem DS Sinh Viên";
                btn.BackColor = Color.DarkOrange;
                
                // Đổi cột Bảng
                dtStudents.Clear();
                dtStudents.Columns.Clear();
                dtStudents.Columns.Add("Email / Username");
                dtStudents.Columns.Add("Họ Tên"); // Thêm cột Họ Tên
                dtStudents.Columns.Add("Vai Trò (Role)");
                dtStudents.Columns.Add("Mật Khẩu (Hidden)");
                
                // Vô hiệu hóa các nút quản lý SV, NHƯNG giữ nút Xóa
                SetManageButtons(false);

                SocketClient.Send("LIST_USERS");
            }
            else
            {
                // Quay về xem SV
                isViewingUsers = false;
                btn.Text = "Xem DS Tài Khoản";
                btn.BackColor = Color.Teal;
                
                InitTable(); // Reset cột về SV
                SetManageButtons(true);
                
                SocketClient.Send("LIST");
            }
        }

        private void SetManageButtons(bool enable)
        {
            btnAdd.Enabled = enable;
            
            // Nút Sửa & Xóa luôn bật
            btnUpdate.Enabled = true;
            btnDelete.Enabled = true; 
            
            btnSearch.Enabled = enable;
            txtID.Enabled = enable;
            txtName.Enabled = enable;
            txtClass.Enabled = enable;
        }

        private void BtnCreateUser_Click(object sender, EventArgs e)
        {
            Form f = new Form();
            f.Size = new Size(400, 340); // Tăng chiều cao
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

            // Email
            Label l1 = new Label() { Parent = f, Left = x, Top = y, AutoSize = true, Text = "Email / Tên đăng nhập:" };
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
                string eu = tEmail.Text.Trim();
                string ep = tPass.Text.Trim();
                string en = tName.Text.Trim(); 
                
                if(string.IsNullOrEmpty(eu) || string.IsNullOrEmpty(ep)) 
                {
                    MessageBox.Show("Vui lòng nhập Email và Mật khẩu.", "Thông báo");
                    return;
                }
                
                SocketClient.Send($"CREATE_USER|{eu}|{ep}|USER|{en}"); 
                MessageBox.Show("Đã gửi lệnh tạo tài khoản.", "Thông báo");
                f.Close();
            };
            
            f.ShowDialog();
        }

        private void InitChatUI()
        {
            // 1. CẤU HÌNH FORM CHUNG
            this.Text = "QUẢN LÝ SINH VIÊN"; 
            this.Width = 1250; 
            this.Height = 700; 
            // Đổi theo yêu cầu cho giống Login
            this.FormBorderStyle = FormBorderStyle.FixedDialog; 
            this.MaximizeBox = false;

            // KHAI BÁO CÁC THÔNG SỐ KÍCH THƯỚC CHUẨN
            int LEFT_MARGIN = 20;
            int LEFT_WIDTH = 840; 
            
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
            grpInfo.Height = 100;
            grpInfo.Anchor = AnchorStyles.Top | AnchorStyles.Left;

            // --- C. HÀNG NÚT BẤM (BUTTONS ROW) ---
            int BTN_Y = 190;
            int btnW = 100; 
            int btnH = 40;   
            int gap = 10;    

            // Nhóm 1: Thao tác 
            btnAdd.Location     = new Point(LEFT_MARGIN, BTN_Y);
            btnAdd.Size         = new Size(btnW, btnH);

            btnUpdate.Location  = new Point(LEFT_MARGIN + (btnW + gap), BTN_Y);
            btnUpdate.Size      = new Size(btnW, btnH);

            btnDelete.Location  = new Point(LEFT_MARGIN + (btnW + gap) * 2, BTN_Y);
            btnDelete.Size      = new Size(btnW, btnH);

            btnRefresh.Location = new Point(LEFT_MARGIN + (btnW + gap) * 3, BTN_Y);
            btnRefresh.Size     = new Size(btnW, btnH);

            // Nhóm 2: Tìm kiếm 
            btnSearch.Location  = new Point(LEFT_MARGIN + LEFT_WIDTH - btnW, BTN_Y);
            btnSearch.Size      = new Size(btnW, btnH);
            
            txtSearch.Location  = new Point(btnSearch.Left - 210, BTN_Y + 8); 
            txtSearch.Size      = new Size(200, 25);

            // Xóa Anchor 
            btnAdd.Anchor = AnchorStyles.Top | AnchorStyles.Left;
            btnUpdate.Anchor = AnchorStyles.Top | AnchorStyles.Left;
            btnDelete.Anchor = AnchorStyles.Top | AnchorStyles.Left;
            btnRefresh.Anchor = AnchorStyles.Top | AnchorStyles.Left;
            btnSearch.Anchor = AnchorStyles.Top | AnchorStyles.Left;
            txtSearch.Anchor = AnchorStyles.Top | AnchorStyles.Left;

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
            StyleButton(btnSearch, Color.Teal);
            
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
                    // Format mới: Username # FullName # Role
                    if (parts.Length >= 3)
                        dtStudents.Rows.Add(parts[0], parts[1], parts[2], "******");
                }
            }
            else if (msg.StartsWith("LIST_RES|"))
            {
                if (isViewingUsers) return;
                dtStudents.Clear();
                string data = msg.Substring(9); 
                string[] rows = data.Split(';');
                foreach (var row in rows)
                {
                    if (string.IsNullOrWhiteSpace(row)) continue;
                    string[] parts = row.Split('#');
                    if (parts.Length == 3)
                        dtStudents.Rows.Add(parts[0], parts[1], parts[2]);
                }
            }
            else if (msg == "ADD_SUCCESS") { LogSystem("Bạn vừa thêm thành công."); ClearInputs(); }
            else if (msg == "UPDATE_SUCCESS") { LogSystem("Bạn vừa sửa thành công."); ClearInputs(); }
            else if (msg == "DELETE_SUCCESS") { LogSystem("Bạn vừa xóa thành công."); ClearInputs(); }
            else if (msg == "EXISTS") MessageBox.Show("MSSV đã tồn tại!");
            else if (msg.StartsWith("FOUND|"))
            {
                string[] p = msg.Split('|');
                dtStudents.Clear();
                dtStudents.Rows.Add(p[1], p[2], p[3]); 
            }
            else if (msg == "STUDENT_NOT_FOUND") MessageBox.Show("Không tìm thấy sinh viên!");
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

        private void btnAdd_Click(object sender, EventArgs e)
        {
            if (!txtID.Enabled) { MessageBox.Show("Vui lòng bấm 'Làm mới'."); return; }
            if (isViewingUsers) return;

            string id = txtID.Text.Trim();
            if (!id.StartsWith("SV")) id = "SV" + id;
            string name = txtName.Text.Trim();
            string cls = txtClass.Text.Trim();

            if (string.IsNullOrEmpty(id) || string.IsNullOrEmpty(name)) return;

            SocketClient.Send($"ADD|{id}|{name}|{cls}");
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
                f.Size = new Size(400, 350); // Tăng size
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
                // Role
                Label l2 = new Label() { Parent = f, Left = x, Top = y, AutoSize = true, Text = "Vai trò (ADMIN/USER):" };
                TextBox tRole = new TextBox() { Parent = f, Left = x, Top = y + 25, Width = w, Font = new Font("Segoe UI", 10), Text = "USER" };
                
                y += 75;
                Button bOk = new Button() { Parent = f, Left = x, Top = y, Width = w, Height = 45, Text = "CẬP NHẬT" };
                StyleButton(bOk, Color.FromArgb(0, 120, 215)); 

                bOk.Click += (s, ev) => {
                    // Gửi tham số FullName vào cuối
                    SocketClient.Send($"UPDATE_USER|{target}|{tPass.Text}|{tRole.Text}|{tName.Text}");
                    f.Close();
                };
                f.ShowDialog();
                return;
            }

            // XỬ LÝ SỬA SINH VIÊN (Cũ)
            if (txtID.Enabled) { MessageBox.Show("Vui lòng chọn SV."); return; }
            string id = txtID.Text.Trim();
            string name = txtName.Text.Trim();
            string cls = txtClass.Text.Trim();
            SocketClient.Send($"UPDATE|{id}|{name}|{cls}");
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
                SocketClient.Send($"DELETE|{target}");
        }

        private void btnSearch_Click(object sender, EventArgs e)
        {
            if (isViewingUsers) return;
            string k = txtSearch.Text.Trim();
            if (string.IsNullOrEmpty(k)) return;
            if (!k.StartsWith("SV")) k = "SV" + k;
            SocketClient.Send($"SEARCH|{k}");
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
                }
                else
                {
                    // Chế độ xem SV: Gán đầy đủ thông tin
                    txtID.Text = val1;
                    txtName.Text = row.Cells[1].Value.ToString();
                    txtClass.Text = row.Cells[2].Value.ToString();
                    txtID.Enabled = false; 
                }
            }
        }

        private void ClearInputs()
        {
            txtID.Text = ""; txtName.Text = ""; txtClass.Text = "";
            txtID.Enabled = true;
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            isListening = false;
            base.OnFormClosing(e);
        }
        // --- SIDEBAR LOGIC ---
        private Panel pnlSidebar;
        private bool isSidebarOpen = false;

        private void InitSidebar()
        {
            pnlSidebar = new Panel();
            pnlSidebar.Size = new Size(250, this.Height); 
            pnlSidebar.Location = new Point(0, 0); 
            pnlSidebar.BackColor = Color.FromArgb(45, 45, 48); 
            pnlSidebar.Visible = false; 
            pnlSidebar.BringToFront(); 

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
            AddSidebarItem("Trang Chủ", top, (s, e) => { ToggleSidebar(); MessageBox.Show("Đây là trang quản lý chính.", "Thông báo"); }); top += 50;
            AddSidebarItem("Thông Tin Cá Nhân", top, (s, e) => { ToggleSidebar(); MessageBox.Show($"Tài khoản: {MyUsername}\nTên: {MyFullName}\nVai trò: {UserRole}", "Thông tin"); }); top += 50;
            AddSidebarItem("Hướng Dẫn", top, (s, e) => { ToggleSidebar(); MessageBox.Show("Bấm Thêm/Sửa/Xóa để quản lý sinh viên.", "Hướng dẫn"); }); top += 50;
            AddSidebarItem("Đăng Xuất", top, (s, e) => { 
                IsLogout = true; // Đánh dấu là đăng xuất chủ động
                this.Close(); 
            }); top += 50;

            this.Controls.Add(pnlSidebar);
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
            pnlSidebar.Visible = isSidebarOpen;
            if (isSidebarOpen) pnlSidebar.BringToFront(); 
        }
    }
}
