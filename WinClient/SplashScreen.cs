using System;
using System.Drawing;
using System.Windows.Forms;

namespace WinClient
{
    public class SplashScreen : Form
    {
        private System.Windows.Forms.Timer tmr;

        public SplashScreen()
        {
            // Thiết lập Style UWP: Không viền, chính giữa, TopMost
            this.FormBorderStyle = FormBorderStyle.None;
            this.StartPosition = FormStartPosition.CenterScreen;
            this.Size = new Size(600, 350);
            this.BackColor = Color.FromArgb(0, 120, 215); // Màu xanh UWP/Windows 10/11
            this.ShowInTaskbar = false;

            // Label Logo
            Label lblLogo = new Label();
            lblLogo.Text = "Windows System";
            lblLogo.Font = new Font("Segoe UI Light", 32, FontStyle.Regular); // Font mỏng chuẩn Metro/UWP
            lblLogo.ForeColor = Color.White;
            lblLogo.AutoSize = true;
            
            // Căn giữa Form
            // Lưu ý: Cần thêm vào Controls trước khi tính toán vị trí để đảm bảo chính xác hoặc tính thủ công
            this.Controls.Add(lblLogo);
            
            // Tính toán vị trí căn giữa
            int x = (this.Width - lblLogo.PreferredWidth) / 2;
            int y = (this.Height - lblLogo.PreferredHeight) / 2;
            lblLogo.Location = new Point(x, y);

            // Label Loading (Spinner giả) - Đơn giản là dòng chữ nhỏ hoặc dấu chấm
            Label lblLoading = new Label();
            lblLoading.Text = "Loading...";
            lblLoading.Font = new Font("Segoe UI", 10, FontStyle.Regular);
            lblLoading.ForeColor = Color.White;
            lblLoading.AutoSize = true;
            lblLoading.Location = new Point((this.Width - 60) / 2, y + 80);
            this.Controls.Add(lblLoading);

            // Timer để tắt Splash
            tmr = new System.Windows.Forms.Timer();
            tmr.Interval = 3000; // 3 giây
            tmr.Tick += (s, e) => {
                tmr.Stop();
                this.Close(); // Đóng Splash để trả quyền điều khiển về Program.cs
            };
            tmr.Start();
        }
    }
}
