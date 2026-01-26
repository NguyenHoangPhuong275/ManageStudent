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
                dtStudents.Clear();
                dtStudents.Columns.Clear();
                dtStudents.Columns.Add("Email / Username");
                dtStudents.Columns.Add("Họ Tên"); 
                dtStudents.Columns.Add("Vai Trò (Role)");
                dtStudents.Columns.Add("Mật Khẩu (Hidden)");
                
                SetManageButtons(false);
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
                
                InitTable(); 
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

        // --- USER INFO PAGE ---
        private Panel pnlUserInfo;

        private void ShowDashboard()
        {
            if (pnlUserInfo != null) pnlUserInfo.Visible = false;
            ToggleSidebar(); // Đóng menu
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

                // Avatar 
                Label lblAvatar = new Label();
                lblAvatar.Size = new Size(120, 120);
                lblAvatar.Location = new Point((card.Width - 120) / 2, 30);
                lblAvatar.BackColor = Color.FromArgb(0, 120, 215); 
                lblAvatar.ForeColor = Color.White;
                lblAvatar.Text = MyFullName.Length > 0 ? MyFullName.Substring(0, 1).ToUpper() : "U";
                lblAvatar.Font = new Font("Segoe UI", 52, FontStyle.Bold);
                lblAvatar.TextAlign = ContentAlignment.MiddleCenter;
                card.Controls.Add(lblAvatar);

                int x = 60;
                int y = 170;

                AddProfileField(card, "HỌ VÀ TÊN", MyFullName, x, y); y += 70;
                AddProfileField(card, "ĐỊA CHỈ EMAIL", MyUsername, x, y); y += 70;
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
            AddSidebarItem("Trang Chủ (Sinh Viên)", top, (s, e) => { 
                ShowDashboard();
                if (isViewingUsers) BtnViewUsers_Click(null, null); // Quay về xem SV nêú đang xem User
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
    }
}
