using System;
using System.Drawing;
using System.Windows.Forms;

namespace WinClient
{
    public partial class MainForm
    {
        private void InitCustomUI()
        {
            this.Text = "HỆ THỐNG QUẢN LÝ SINH VIÊN - PRO EDITION";
            this.Size = new Size(1300, 780);
            this.FormBorderStyle = FormBorderStyle.FixedDialog; // Chế độ FixedDialog
            this.MaximizeBox = false; // Vô hiệu hóa phóng to
            this.StartPosition = FormStartPosition.CenterScreen;
            this.BackColor = Color.FromArgb(240, 245, 250); // Nền xám xanh nhẹ hiện đại
            this.Font = new Font("Segoe UI", 10);

            SetupHeader();
            InitSidebar();
            SetupMainLayout();
            SetupChatUI();
        }

        private void SetupHeader()
        {
            Panel pnlHeader = new Panel { 
                Dock = DockStyle.Top, 
                Height = 75, 
                BackColor = Color.White,
                Padding = new Padding(20, 0, 20, 0)
            };
            this.Controls.Add(pnlHeader);

            // Shadow effect for header
            pnlHeader.Paint += (s, e) => {
                e.Graphics.DrawLine(new Pen(Color.FromArgb(230, 230, 230), 2), 0, 74, pnlHeader.Width, 74);
            };

            Button btnMenu = new Button {
                Text = "☰",
                Font = new Font("Segoe UI", 18, FontStyle.Bold),
                FlatStyle = FlatStyle.Flat,
                Size = new Size(50, 50),
                Location = new Point(10, 12),
                Cursor = Cursors.Hand,
                ForeColor = Color.FromArgb(0, 120, 215)
            };
            btnMenu.FlatAppearance.BorderSize = 0;
            btnMenu.Click += (s, e) => ToggleSidebar();
            pnlHeader.Controls.Add(btnMenu);

            Label lblTitle = new Label {
                Text = "Hệ thống Quản lý Sinh viên v2.0",
                Font = new Font("Segoe UI", 18, FontStyle.Bold),
                ForeColor = Color.FromArgb(30, 41, 59),
                Location = new Point(70, 20),
                AutoSize = true
            };
            pnlHeader.Controls.Add(lblTitle);

            lblWelcome = new Label {
                Text = $"Chào bạn, {MyFullName}",
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                ForeColor = Color.FromArgb(100, 116, 139),
                Dock = DockStyle.Right,
                Width = 400,
                Padding = new Padding(0, 28, 20, 0),
                TextAlign = ContentAlignment.TopRight
            };
            pnlHeader.Controls.Add(lblWelcome);
            lblWelcome.BringToFront();
        }

        private void SetupMainLayout()
        {
            // Center area - Card style
            Panel pnlMain = new Panel { 
                Location = new Point(20, 95), 
                Size = new Size(920, 230), 
                BackColor = Color.White
            };
            pnlMain.Paint += (s, e) => ControlPaint.DrawBorder(e.Graphics, pnlMain.ClientRectangle, Color.FromArgb(230, 230, 230), ButtonBorderStyle.Solid);
            this.Controls.Add(pnlMain);

            Label lTitle = new Label { Text = "THÔNG TIN CHI TIẾT SINH VIÊN", Font = new Font("Segoe UI", 10, FontStyle.Bold), Location = new Point(20, 15), AutoSize = true, ForeColor = Color.FromArgb(71, 85, 105) };
            pnlMain.Controls.Add(lTitle);

            txtID = new TextBox { PlaceholderText = "Ví dụ: SV001" };
            txtName = new TextBox { PlaceholderText = "Nhập họ và tên đầy đủ" };
            txtClass = new TextBox { PlaceholderText = "Ví dụ: CNTT1" };

            AddInputPair(pnlMain, "Mã Định Danh (MSSV):", txtID, 20, 45, 70, 260);
            AddInputPair(pnlMain, "Họ và Tên:", txtName, 310, 45, 70, 300);
            AddInputPair(pnlMain, "Lớp Học:", txtClass, 640, 45, 70, 250);

            // Action Buttons
            int bx = 20, by = 135, bw = 120, bh = 45;
            btnAdd = CreateActionButton("THÊM MỚI", Color.FromArgb(16, 185, 129), bx, by, bw, bh, btnAdd_Click);
            btnUpdate = CreateActionButton("CẬP NHẬT", Color.FromArgb(37, 99, 235), bx += 135, by, bw, bh, btnUpdate_Click);
            btnDelete = CreateActionButton("XOÁ BỎ", Color.FromArgb(239, 68, 68), bx += 135, by, bw, bh, btnDelete_Click);
            btnRefresh = CreateActionButton("LÀM MỚI", Color.FromArgb(245, 158, 11), bx += 135, by, bw, bh, btnRefresh_Click);

            if (UserRole == "ADMIN")
            {
                Button btnSwitch = CreateActionButton("XEM TÀI KHOẢN", Color.FromArgb(75, 85, 99), bx += 135, by, 180, bh, BtnViewUsers_Click);
                pnlMain.Controls.Add(btnSwitch);
            }

            pnlMain.Controls.AddRange(new Control[] { btnAdd, btnUpdate, btnDelete, btnRefresh });

            // Data Table Card
            Panel pnlTable = new Panel {
                Location = new Point(20, 345),
                Size = new Size(920, 360),
                BackColor = Color.White
            };
            pnlTable.Paint += (s, e) => ControlPaint.DrawBorder(e.Graphics, pnlTable.ClientRectangle, Color.FromArgb(230, 230, 230), ButtonBorderStyle.Solid);
            this.Controls.Add(pnlTable);

            dgvStudents = new DataGridView { 
                Dock = DockStyle.Fill, 
                BackgroundColor = Color.White, 
                BorderStyle = BorderStyle.None,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                RowHeadersVisible = false,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                AllowUserToAddRows = false,
                RowTemplate = { Height = 40 }
            };
            dgvStudents.DefaultCellStyle.Font = new Font("Segoe UI", 9);
            dgvStudents.ColumnHeadersHeight = 45;
            dgvStudents.EnableHeadersVisualStyles = false;
            dgvStudents.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(248, 250, 252);
            dgvStudents.ColumnHeadersDefaultCellStyle.ForeColor = Color.FromArgb(71, 85, 105);
            dgvStudents.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 9, FontStyle.Bold);
            dgvStudents.CellClick += dgvStudents_CellClick;
            pnlTable.Controls.Add(dgvStudents);
        }

        private void SetupChatUI()
        {
            // Di chuyển vị trí xuống Y=110 và tăng chiều rộng để dễ nhìn hơn
            Panel pnlChat = new Panel { 
                Location = new Point(960, 110), 
                Size = new Size(315, 580), 
                BackColor = Color.White,
                BorderStyle = BorderStyle.FixedSingle 
            };
            this.Controls.Add(pnlChat);

            // 1. Tiêu đề (Vị trí cố định trên cùng)
            Label lblChat = new Label { 
                Text = "CỘNG ĐỒNG THẢO LUẬN", 
                Font = new Font("Segoe UI", 9, FontStyle.Bold), 
                Location = new Point(0, 0),
                Size = new Size(315, 35),
                TextAlign = ContentAlignment.MiddleCenter, 
                BackColor = Color.FromArgb(235, 240, 250),
                ForeColor = Color.FromArgb(0, 120, 215)
            };
            pnlChat.Controls.Add(lblChat);

            // 2. Khung nội dung (Bắt đầu từ Y=35, chiều cao cố định)
            rtbChat = new RichTextBox { 
                Location = new Point(5, 40),
                Size = new Size(305, 480),
                BorderStyle = BorderStyle.None, 
                BackColor = Color.FromArgb(252, 252, 252), 
                ReadOnly = true, 
                Font = new Font("Segoe UI", 10)
            };
            pnlChat.Controls.Add(rtbChat);

            // 3. Khu vực nhập liệu (Nằm ở đáy)
            Panel pnlInput = new Panel { 
                Location = new Point(0, 525),
                Size = new Size(315, 55),
                BackColor = Color.FromArgb(245, 245, 245)
            };
            pnlChat.Controls.Add(pnlInput);

            btnSend = new Button { 
                Text = "GỬI", 
                Location = new Point(255, 10),
                Size = new Size(55, 35),
                BackColor = Color.FromArgb(0, 120, 215), 
                ForeColor = Color.White, 
                FlatStyle = FlatStyle.Flat, 
                Font = new Font("Segoe UI", 9, FontStyle.Bold),
                Cursor = Cursors.Hand
            };
            btnSend.Click += BtnSend_Click;
            pnlInput.Controls.Add(btnSend);

            txtChatInput = new TextBox { 
                Location = new Point(5, 12),
                Width = 245,
                Font = new Font("Segoe UI", 11), 
                PlaceholderText = "Nhập tin nhắn..." 
            };
            txtChatInput.KeyDown += (s, e) => { if (e.KeyCode == Keys.Enter) { e.SuppressKeyPress = true; BtnSend_Click(s, e); } };
            pnlInput.Controls.Add(txtChatInput);

            pnlChat.BringToFront();
        }

        private void AddInputPair(Control parent, string text, TextBox tb, int x, int ly, int ty, int w)
        {
            parent.Controls.Add(new Label { Text = text, Location = new Point(x, ly), AutoSize = true });
            tb.Location = new Point(x, ty); tb.Width = w; tb.Font = new Font("Segoe UI", 11);
            parent.Controls.Add(tb);
        }

        private Button CreateActionButton(string text, Color backColor, int x, int y, int w, int h, EventHandler click)
        {
            Button btn = new Button {
                Text = text,
                Location = new Point(x, y),
                Size = new Size(w, h),
                BackColor = backColor,
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 9, FontStyle.Bold),
                Cursor = Cursors.Hand
            };
            btn.FlatAppearance.BorderSize = 0;
            btn.Click += click;
            return btn;
        }


        // --- Controls Definitions (to be accessible globally) ---
        private GroupBox grpInfo;
        private TextBox txtID, txtName, txtClass;
        private Button btnAdd, btnUpdate, btnDelete, btnRefresh;
        private DataGridView dgvStudents;
        private Label lblWelcome;
    }
}
