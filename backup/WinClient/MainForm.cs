using System;
using System.Data;
using System.Drawing;
using System.Windows.Forms;
using System.Threading;
using System.Threading.Tasks;
using System.IO;

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
            grpInfo.Height = 100;
            grpInfo.Anchor = AnchorStyles.Top | AnchorStyles.Left;

            // --- C. HÀNG NÚT BẤM (BUTTONS ROW) ---
            int BTN_Y = 190;
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

            // Nhóm 2: Tìm kiếm (Đã chuyển sang trang Tra Cứu riêng)
            
            // Xóa Anchor 
            btnAdd.Anchor = AnchorStyles.Top | AnchorStyles.Left;
            btnUpdate.Anchor = AnchorStyles.Top | AnchorStyles.Left;
            btnDelete.Anchor = AnchorStyles.Top | AnchorStyles.Left;
            btnRefresh.Anchor = AnchorStyles.Top | AnchorStyles.Left;
            btnImport.Anchor  = AnchorStyles.Top | AnchorStyles.Left;

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
                
                DataTable targetDT = dtStudents;
                bool isSearchMode = (pnlSearchPage != null && pnlSearchPage.Visible);
                if (isSearchMode) targetDT = (DataTable)dgvSearchResults.DataSource;

                targetDT.Clear();
                string data = msg.Substring(9); 
                string[] rows = data.Split(';');
                foreach (var row in rows)
                {
                    if (string.IsNullOrWhiteSpace(row)) continue;
                    string[] parts = row.Split('#');
                    if (parts.Length == 3)
                        targetDT.Rows.Add(parts[0], parts[1], parts[2]);
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

                            // Validation tương tự nút Thêm
                            if (!IsAlphanumeric(id) || !IsAlphanumeric(name, true) || !IsAlphanumeric(cls))
                            {
                                LogSystem($"Lỗi dòng {count}: Ký tự không hợp lệ ({id})");
                                continue; 
                            }

                            SocketClient.Send($"ADD|{id}|{name}|{cls}");
                            success++;
                            
                            // Nghỉ 5ms để tránh dính gói tin (TCP Sticky Packets)
                            System.Threading.Thread.Sleep(5); 
                        }
                    }

                    MessageBox.Show($"Đã đọc {count} dòng. Gửi thành công {success} lệnh nhập liệu.", "Kết quả Import");
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
            if (!txtID.Enabled) { MessageBox.Show("Vui lòng bấm 'Làm mới'."); return; }
            if (isViewingUsers) return;

            string id = txtID.Text.Trim();
            string name = txtName.Text.Trim();
            string cls = txtClass.Text.Trim();

            if (string.IsNullOrEmpty(id) || string.IsNullOrEmpty(name))
            {
                MessageBox.Show("Vui lòng nhập đầy đủ thông tin!");
                return;
            }

            // Chặn ký tự đặc biệt
            if (!IsAlphanumeric(id) || !IsAlphanumeric(name, true) || !IsAlphanumeric(cls))
            {
                MessageBox.Show("Dữ liệu chỉ chấp nhận chữ cái (a-z) và số (0-9). Vui lòng kiểm tra lại!", "Lỗi nhập liệu");
                return;
            }

            if (!id.StartsWith("SV")) id = "SV" + id;
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

            // Chặn ký tự đặc biệt
            if (!IsAlphanumeric(name, true) || !IsAlphanumeric(cls))
            {
                MessageBox.Show("Dữ liệu chỉ chấp nhận chữ cái (a-z) và số (0-9). Vui lòng kiểm tra lại!", "Lỗi nhập liệu");
                return;
            }

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

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            isListening = false;
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
            isViewingUsers = false; // QUAN TRỌNG: Chuyển về chế độ SV để nhận dữ liệu LIST_RES

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
                Label title = new Label() { Text = "TRA CỨU & LỌC SINH VIÊN", Font = new Font("Segoe UI", 16, FontStyle.Bold), ForeColor = Color.FromArgb(0, 120, 215), Location = new Point(20, 15), AutoSize = true };
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
                
                // Logic lọc
                cbClasses.SelectedIndexChanged += (s, ev) => {
                    string selected = cbClasses.SelectedItem.ToString();
                    if (selected == "TẤT CẢ") SocketClient.Send("LIST");
                    else SocketClient.Send($"SEARCH|CLASS|{selected}");
                };

                // 3. Search Area
                Label l2 = new Label() { Text = "Tìm tên/mã:", Location = new Point(250, 20), AutoSize = true, Font = new Font("Segoe UI", 9, FontStyle.Bold) };
                TextBox tSearch = new TextBox() { Location = new Point(250, 45), Width = 250, Font = new Font("Segoe UI", 10), PlaceholderText = "Nhập từ khóa..." };
                filterArea.Controls.Add(l2);
                filterArea.Controls.Add(tSearch);
                
                Button bFind = new Button() { Text = "TÌM KIẾM", Location = new Point(510, 43), Size = new Size(100, 32), BackColor = Color.FromArgb(0, 120, 215), ForeColor = Color.White, FlatStyle = FlatStyle.Flat, Font = new Font("Segoe UI", 9, FontStyle.Bold) };
                bFind.Click += (s, ev) => SocketClient.Send($"SEARCH|ALL|{tSearch.Text.Trim()}");
                filterArea.Controls.Add(bFind);

                // 4. Results Grid
                dgvSearchResults = new DataGridView() { Location = new Point(20, 200), Size = new Size(pnlSearchPage.Width - 40, 420), BackgroundColor = Color.White, BorderStyle = BorderStyle.None, Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right };
                dgvSearchResults.EnableHeadersVisualStyles = false;
                dgvSearchResults.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(0, 120, 215);
                dgvSearchResults.ColumnHeadersDefaultCellStyle.ForeColor = Color.White;
                dgvSearchResults.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
                pnlSearchPage.Controls.Add(dgvSearchResults);

                DataTable dtRes = new DataTable();
                dtRes.Columns.Add("MSSV"); dtRes.Columns.Add("Họ Tên"); dtRes.Columns.Add("Lớp");
                dgvSearchResults.DataSource = dtRes;

                // 5. Back Button
                Button bBack = new Button() { Text = "← QUAY LẠI TRANG CHỦ", Location = new Point(20, 630), Size = new Size(200, 40), FlatStyle = FlatStyle.Flat, BackColor = Color.Gray, ForeColor = Color.White, Font = new Font("Segoe UI", 9, FontStyle.Bold), Anchor = AnchorStyles.Bottom | AnchorStyles.Left };
                bBack.Click += (s, ev) => ShowDashboard();
                pnlSearchPage.Controls.Add(bBack);

                this.Controls.Add(pnlSearchPage);
            }
            pnlSearchPage.Visible = true;
            pnlSearchPage.BringToFront();
            SocketClient.Send("LIST"); // Load ban đầu
        }

        private void ShowUserInfo()
        {
            ToggleSidebar();
            if (pnlUserInfo == null)
            {
                pnlUserInfo = new Panel();
                pnlUserInfo.Location = new Point(0, 0);
                pnlUserInfo.Size = this.ClientSize;
                pnlUserInfo.BackColor = Color.White;
                pnlUserInfo.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;

                Label title = new Label() { Text = "THÔNG TIN TÀI KHOẢN", Font = new Font("Segoe UI", 20, FontStyle.Bold), ForeColor = Color.DarkBlue, Location = new Point(50, 50), AutoSize = true };
                pnlUserInfo.Controls.Add(title);

                int y = 130;
                AddInfoRow(pnlUserInfo, "Họ và Tên:", MyFullName, ref y);
                AddInfoRow(pnlUserInfo, "Email/Username:", MyUsername, ref y);
                AddInfoRow(pnlUserInfo, "Quyền Hạn:", UserRole, ref y);
                AddInfoRow(pnlUserInfo, "Trạng Thái:", "Đang hoạt động", ref y);

                Button btnBack = new Button() { Text = "QUAY LẠI TRANG CHỦ", Location = new Point(50, y + 40), Size = new Size(200, 45), BackColor = Color.RoyalBlue, ForeColor = Color.White, FlatStyle = FlatStyle.Flat, Font = new Font("Segoe UI", 10, FontStyle.Bold) };
                btnBack.Click += (s, e) => ShowDashboard();
                pnlUserInfo.Controls.Add(btnBack);

                this.Controls.Add(pnlUserInfo);
            }
            pnlUserInfo.Visible = true;
            pnlUserInfo.BringToFront();
        }

        private void AddInfoRow(Panel p, string label, string value, ref int y)
        {
            Label l = new Label() { Text = label, Font = new Font("Segoe UI", 11, FontStyle.Bold), Location = new Point(50, y), AutoSize = true };
            Label v = new Label() { Text = value, Font = new Font("Segoe UI", 11), Location = new Point(200, y), AutoSize = true, ForeColor = Color.DarkSlateGray };
            p.Controls.Add(l); p.Controls.Add(v);
            y += 40;
        }

        // --- SIDEBAR LOGIC ---
        private Panel pnlSidebar;
        private bool isSidebarOpen = false;
        private System.Windows.Forms.Timer sidebarTimer;
        private const int sidebarWidth = 250;
        private const int sidebarStep = 30;

        private void InitSidebar()
        {
            pnlSidebar = new Panel();
            pnlSidebar.Width = sidebarWidth;
            pnlSidebar.Height = this.Height;
            pnlSidebar.Location = new Point(-sidebarWidth, 0);
            pnlSidebar.BackColor = Color.FromArgb(45, 45, 48); // Màu tối hiện đại
            this.Controls.Add(pnlSidebar);

            // Header Sidebar
            Label lblMenu = new Label() { Text = "DANH MỤC", ForeColor = Color.White, Font = new Font("Segoe UI", 14, FontStyle.Bold), Location = new Point(20, 20), AutoSize = true };
            pnlSidebar.Controls.Add(lblMenu);

            int top = 80;
            AddSidebarItem("Bảng Điều Khiển", top, (s, e) => ShowDashboard()); top += 50;
            AddSidebarItem("Tra Cứu Sinh Viên", top, (s, e) => ShowSearchPage()); top += 50;
            
            if (UserRole == "ADMIN")
            {
                AddSidebarItem("Quản Lý Tài Khoản", top, BtnViewUsers_Click); top += 50;
                AddSidebarItem("Cấp Tài Khoản Mới", top, BtnCreateUser_Click); top += 50;
            }
            
            AddSidebarItem("Thông Tin Cá Nhân", top, (s, e) => ShowUserInfo()); top += 50;
            
            // Logout ở dưới cùng
            Button btnLogout = new Button() { Text = "ĐĂNG XUẤT", Size = new Size(sidebarWidth, 50), Location = new Point(0, this.Height - 100), FlatStyle = FlatStyle.Flat, ForeColor = Color.Salmon, Font = new Font("Segoe UI", 10, FontStyle.Bold), TextAlign = ContentAlignment.MiddleLeft, Padding = new Padding(20, 0, 0, 0) };
            btnLogout.FlatAppearance.BorderSize = 0;
            btnLogout.Click += (s, e) => { IsLogout = true; this.Close(); };
            pnlSidebar.Controls.Add(btnLogout);

            pnlSidebar.BringToFront();

            sidebarTimer = new System.Windows.Forms.Timer();
            sidebarTimer.Interval = 10;
            sidebarTimer.Tick += SidebarTimer_Tick;
        }

        private void AddSidebarItem(string text, int top, EventHandler onClick)
        {
            Button btn = new Button();
            btn.Text = text;
            btn.Size = new Size(sidebarWidth, 45);
            btn.Location = new Point(0, top);
            btn.FlatStyle = FlatStyle.Flat;
            btn.FlatAppearance.BorderSize = 0;
            btn.ForeColor = Color.White;
            btn.Font = new Font("Segoe UI", 10);
            btn.TextAlign = ContentAlignment.MiddleLeft;
            btn.Padding = new Padding(20, 0, 0, 0);
            btn.Cursor = Cursors.Hand;
            btn.MouseEnter += (s, e) => btn.BackColor = Color.FromArgb(60, 60, 60);
            btn.MouseLeave += (s, e) => btn.BackColor = Color.Transparent;
            btn.Click += onClick;
            pnlSidebar.Controls.Add(btn);
        }

        private void ToggleSidebar()
        {
            isSidebarOpen = !isSidebarOpen;
            sidebarTimer.Start();
        }

        private void SidebarTimer_Tick(object sender, EventArgs e)
        {
            if (isSidebarOpen)
            {
                if (pnlSidebar.Left < 0) pnlSidebar.Left += sidebarStep;
                else sidebarTimer.Stop();
            }
            else
            {
                if (pnlSidebar.Left > -sidebarWidth) pnlSidebar.Left -= sidebarStep;
                else sidebarTimer.Stop();
            }
        }
    }
}
