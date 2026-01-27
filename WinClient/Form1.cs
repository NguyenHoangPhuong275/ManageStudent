using System;
using System.Drawing;
using System.Windows.Forms;
using System.Net.Sockets;

namespace WinClient
{
    public partial class Form1 : Form
    {
        private TextBox txtUser;
        private TextBox txtPass;
        private Button btnLogin;
        private Label lblMsg;

        public Form1()
        {
            InitializeComponent();
            SetupUI();
        }

        private void SetupUI()
        {
<<<<<<< HEAD
            this.Text = "ĐĂNG NHẬP HỆ THỐNG";
            this.Size = new Size(400, 350);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.BackColor = Color.FromArgb(245, 245, 245);
            this.Font = new Font("Segoe UI", 9);

            int x = 40, w = 300;

            Label lblTitle = new Label {
                Text = "QUẢN LÝ SINH VIÊN",
                Font = new Font("Segoe UI", 16, FontStyle.Bold),
                Location = new Point(x, 30),
                Size = new Size(w, 40),
                TextAlign = ContentAlignment.MiddleCenter
            };
            this.Controls.Add(lblTitle);

            Label l1 = new Label { Text = "Tài khoản:", Location = new Point(x, 90), AutoSize = true };
            this.Controls.Add(l1);

            txtUser = new TextBox { 
                Location = new Point(x, 115), 
                Width = w, 
                Text = "admin@admin.edu.vn",
                Font = new Font("Segoe UI", 10)
            };
            this.Controls.Add(txtUser);

            Label l2 = new Label { Text = "Mật khẩu:", Location = new Point(x, 155), AutoSize = true };
            this.Controls.Add(l2);

            txtPass = new TextBox { 
                Location = new Point(x, 180), 
                Width = w, 
                PasswordChar = '●',
                Font = new Font("Segoe UI", 10)
            };
            this.Controls.Add(txtPass);

            btnLogin = new Button {
                Text = "ĐĂNG NHẬP",
                Location = new Point(x, 230),
                Size = new Size(w, 40),
                BackColor = Color.FromArgb(0, 120, 215),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 10, FontStyle.Bold)
            };
            btnLogin.Click += BtnLogin_Click;
            this.Controls.Add(btnLogin);

            lblMsg = new Label {
                Location = new Point(x, 280),
                Size = new Size(w, 20),
                TextAlign = ContentAlignment.MiddleCenter,
                ForeColor = Color.Red
            };
            this.Controls.Add(lblMsg);

=======
            this.Text = "QUẢN LÝ SINH VIÊN";
            this.Size = new Size(450, 400); // Tăng chiều rộng lên 450
            this.StartPosition = FormStartPosition.CenterScreen;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.BackColor = Color.WhiteSmoke;

            // Tính toán căn giữa
            int centerX  = this.ClientSize.Width / 2;
            int inputW   = 320; // Rộng hơn vì Form rộng hơn
            int inputX   = (this.ClientSize.Width - inputW) / 2;
            
            // TITLE
            Label lblTitle = new Label();
            lblTitle.Text = "ĐĂNG NHẬP HỆ THỐNG";
            lblTitle.Font = new Font("Segoe UI", 16, FontStyle.Bold); // Giảm size chữ xuống 16
            lblTitle.ForeColor = Color.DarkBlue;
            lblTitle.AutoSize = true;
            this.Controls.Add(lblTitle);
            
            // Căn giữa Title sau khi Add
            lblTitle.Location = new Point((this.ClientSize.Width - lblTitle.PreferredWidth) / 2, 30);

            // USERNAME
            Label l1 = new Label(); 
            l1.Text = "Tài khoản (Email):"; 
            l1.Font = new Font("Segoe UI", 10, FontStyle.Regular);
            l1.AutoSize = true;
            l1.Location = new Point(inputX, 90);
            this.Controls.Add(l1);
            
            txtUser = new TextBox();
            txtUser.Location = new Point(inputX, 115);
            txtUser.Size = new Size(inputW, 30); // Cao hơn chút nhưng TextBox thường fixed height theo font
            txtUser.Font = new Font("Segoe UI", 11); // Font to hơn
            txtUser.Text = "admin@admin.edu.vn"; 
            this.Controls.Add(txtUser);

            // PASSWORD
            Label l2 = new Label(); 
            l2.Text = "Mật khẩu:"; 
            l2.Font = new Font("Segoe UI", 10, FontStyle.Regular);
            l2.AutoSize = true;
            l2.Location = new Point(inputX, 160); // Cách xa User ra chút
            this.Controls.Add(l2);
            
            txtPass = new TextBox();
            txtPass.Location = new Point(inputX, 185);
            txtPass.Size = new Size(inputW, 30);
            txtPass.PasswordChar = '•';
            txtPass.Font = new Font("Segoe UI", 11);
            this.Controls.Add(txtPass);

            // BUTTON
            btnLogin = new Button();
            btnLogin.Text = "ĐĂNG NHẬP";
            btnLogin.Location = new Point(inputX, 240); // Cách Pass xa hơn
            btnLogin.Size = new Size(inputW, 45); // Cao hơn, bấm cho sướng
            btnLogin.BackColor = Color.FromArgb(0, 120, 215); // Xanh chuẩn Windows
            btnLogin.ForeColor = Color.White;
            btnLogin.FlatStyle = FlatStyle.Flat;
            btnLogin.FlatAppearance.BorderSize = 0;
            btnLogin.Font = new Font("Segoe UI", 11, FontStyle.Bold);
            btnLogin.Cursor = Cursors.Hand;
            btnLogin.Click += BtnLogin_Click;
            this.Controls.Add(btnLogin);

            // MESSAGE LABEL
            lblMsg = new Label();
            lblMsg.Location = new Point(inputX, 300);
            lblMsg.Size = new Size(inputW, 25);
            lblMsg.TextAlign = ContentAlignment.MiddleCenter;
            lblMsg.ForeColor = Color.Crimson;
            lblMsg.Font = new Font("Segoe UI", 9, FontStyle.Italic);
            this.Controls.Add(lblMsg);

            // Enter key
>>>>>>> a9f7c149d44583bd431f7952ef8661382757e01c
            this.AcceptButton = btnLogin;
        }

        private void BtnLogin_Click(object sender, EventArgs e)
        {
            try
            {
<<<<<<< HEAD
=======
                // Reset kết nối cũ nếu có để tránh lỗi khi đổi tài khoản
>>>>>>> a9f7c149d44583bd431f7952ef8661382757e01c
                SocketClient.Close(); 

                if (!SocketClient.Connect())
                {
                    lblMsg.Text = "Không thể kết nối Server!";
                    return;
                }

                string u = txtUser.Text.Trim();
                string p = txtPass.Text.Trim();

                if (string.IsNullOrEmpty(u) || string.IsNullOrEmpty(p))
                {
<<<<<<< HEAD
                    lblMsg.Text = "Nhập đủ thông tin.";
=======
                    lblMsg.Text = "Vui lòng nhập đủ thông tin.";
>>>>>>> a9f7c149d44583bd431f7952ef8661382757e01c
                    return;
                }

                SocketClient.Send($"LOGIN|{u}|{p}");
                string response = SocketClient.Receive();

                if (response != null && response.StartsWith("LOGIN_SUCCESS"))
                {
<<<<<<< HEAD
                    string[] parts = response.Split('|');
                    string role = parts.Length > 1 ? parts[1] : "USER";
                    string fullName = (parts.Length > 2 && !string.IsNullOrWhiteSpace(parts[2])) ? parts[2] : u; 

                    MainForm main = new MainForm(role, u, fullName);
                    this.Hide();
                    main.ShowDialog();
                    
                    if (main.IsLogout) 
                    {
=======
                    // Response format: LOGIN_SUCCESS|ROLE|FULL_NAME|AVATAR|REAL_USERNAME
                    string[] parts = response.Split('|');
                    string role = parts.Length > 1 ? parts[1] : "USER";
                    string fullName = parts.Length > 2 ? parts[2] : u; 
                    string avatar = parts.Length > 3 ? parts[3] : "";
                    string realUsername = parts.Length > 4 ? parts[4] : u; // Lấy Username gốc từ DB
                    string email = parts.Length > 5 ? parts[5] : "";

                    // Mở Form Chính và truyền Role, Username, FullName
                    MainForm main = new MainForm(role, realUsername, fullName, avatar, email);
                    this.Hide();
                    main.ShowDialog();
                    
                    // Khi MainForm đóng
                    if (main.IsLogout) 
                    {
                        // Nếu là Đăng Xuất -> Hiện lại Login
>>>>>>> a9f7c149d44583bd431f7952ef8661382757e01c
                        SocketClient.Close(); 
                        this.Show(); 
                        txtPass.Text = ""; 
                        txtUser.Focus();
                    }
                    else 
                    {
<<<<<<< HEAD
=======
                        // Nếu là tắt Form (Nút X) -> Thoát luôn app
>>>>>>> a9f7c149d44583bd431f7952ef8661382757e01c
                        SocketClient.Close();
                        Application.Exit(); 
                    }
                }
                else
                {
                    lblMsg.Text = "Sai tài khoản hoặc mật khẩu!";
<<<<<<< HEAD
                    SocketClient.Close();
=======
                    SocketClient.Close(); // Đóng socket để thử lại sạch sẽ
>>>>>>> a9f7c149d44583bd431f7952ef8661382757e01c
                }
            }
            catch (Exception ex)
            {
                lblMsg.Text = "Lỗi: " + ex.Message;
            }
        }
    }
}
