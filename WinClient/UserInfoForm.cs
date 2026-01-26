using System;
using System.Drawing;
using System.Windows.Forms;

namespace WinClient
{
    public class UserInfoForm : Form
    {
        public UserInfoForm(string username, string fullname, string role)
        {
            this.Text = "HỒ SƠ CÁ NHÂN";
            this.Size = new Size(400, 450);
            this.StartPosition = FormStartPosition.CenterParent;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.BackColor = Color.White;
            this.ShowIcon = false;

            // --- AVATAR ---
            Label lblAvatar = new Label();
            lblAvatar.Size = new Size(100, 100);
            lblAvatar.Location = new Point((this.ClientSize.Width - 100) / 2, 30);
            lblAvatar.BackColor = Color.FromArgb(0, 120, 215); // Màu xanh chủ đạo
            lblAvatar.ForeColor = Color.White;
            lblAvatar.Text = fullname.Length > 0 ? fullname.Substring(0, 1).ToUpper() : "U";
            lblAvatar.Font = new Font("Segoe UI", 40, FontStyle.Bold);
            lblAvatar.TextAlign = ContentAlignment.MiddleCenter;
            // Bo tròn (giả lập bằng cách vẽ hoặc đơn giản là hình vuông bo góc, ở đây dùng hình vuông màu)
            this.Controls.Add(lblAvatar);

            // --- INFO ---
            int y = 150;
            
            AddInfoLabel("HỌ VÀ TÊN", fullname, y); y += 70;
            AddInfoLabel("TÀI KHOẢN (EMAIL)", username, y); y += 70;
            AddInfoLabel("VAI TRÒ HỆ THỐNG", role, y); y += 70;

            // --- BUTTON ---
            Button btnClose = new Button();
            btnClose.Text = "ĐÓNG";
            btnClose.Size = new Size(120, 40);
            btnClose.Location = new Point((this.ClientSize.Width - 120) / 2, y + 20);
            btnClose.FlatStyle = FlatStyle.Flat;
            btnClose.BackColor = Color.Gray;
            btnClose.ForeColor = Color.White;
            btnClose.Cursor = Cursors.Hand;
            btnClose.Click += (s, e) => this.Close();
            this.Controls.Add(btnClose);
        }

        private void AddInfoLabel(string title, string content, int top)
        {
            Label lblTitle = new Label();
            lblTitle.Text = title;
            lblTitle.Font = new Font("Segoe UI", 9, FontStyle.Regular);
            lblTitle.ForeColor = Color.Gray;
            lblTitle.Location = new Point(50, top);
            lblTitle.AutoSize = true;
            this.Controls.Add(lblTitle);

            Label lblContent = new Label();
            lblContent.Text = content;
            lblContent.Font = new Font("Segoe UI", 12, FontStyle.Bold);
            lblContent.ForeColor = Color.Black;
            lblContent.Location = new Point(50, top + 20);
            lblContent.AutoSize = true;
            this.Controls.Add(lblContent);
            
            // Đường kẻ
            Panel line = new Panel();
            line.Size = new Size(300, 1);
            line.BackColor = Color.LightGray;
            line.Location = new Point(50, top + 50);
            this.Controls.Add(line);
        }
    }
}
