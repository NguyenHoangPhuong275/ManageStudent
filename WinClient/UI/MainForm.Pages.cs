using System;
using System.Drawing;
using System.Windows.Forms;
using System.Data;

namespace WinClient
{
    public partial class MainForm
    {
        private void ShowDashboard()
        {
            if (pnlUserInfo != null) pnlUserInfo.Visible = false;
            if (pnlSearchPage != null) pnlSearchPage.Visible = false;
        }

        private void ShowUserInfo()
        {
            if (pnlUserInfo == null)
            {
                pnlUserInfo = new Panel { Size = this.ClientSize, BackColor = Color.FromArgb(245, 247, 251), Location = new Point(0, 0), Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right };
                this.Controls.Add(pnlUserInfo);
                
                // Card container
                Panel card = new Panel { 
                    Size = new Size(500, 480), 
                    BackColor = Color.White, 
                    Location = new Point((pnlUserInfo.Width - 500) / 2, 80),
                    Anchor = AnchorStyles.None
                };
                card.Paint += (s, e) => {
                    ControlPaint.DrawBorder(e.Graphics, card.ClientRectangle, Color.FromArgb(230, 230, 230), ButtonBorderStyle.Solid);
                };
                pnlUserInfo.Controls.Add(card);

                // Avatar Circle
                Panel avatar = new Panel {
                    Size = new Size(100, 100),
                    Location = new Point((card.Width - 100) / 2, 40),
                    BackColor = (UserRole == "ADMIN") ? Color.FromArgb(0, 120, 215) : Color.FromArgb(0, 150, 136)
                };
                avatar.Paint += (s, e) => {
                    e.Graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
                    string initial = MyFullName.Substring(0, 1).ToUpper();
                    using (Font f = new Font("Segoe UI", 32, FontStyle.Bold))
                    {
                        var size = e.Graphics.MeasureString(initial, f);
                        e.Graphics.DrawString(initial, f, Brushes.White, (avatar.Width - size.Width) / 2, (avatar.Height - size.Height) / 2);
                    }
                };
                card.Controls.Add(avatar);

                Label lblName = new Label { 
                    Text = MyFullName, 
                    Font = new Font("Segoe UI", 16, FontStyle.Bold), 
                    TextAlign = ContentAlignment.MiddleCenter,
                    Location = new Point(0, 150),
                    Size = new Size(500, 40),
                    ForeColor = Color.FromArgb(44, 62, 80)
                };
                card.Controls.Add(lblName);

                Label lblRole = new Label { 
                    Text = (UserRole == "ADMIN") ? "QUẢN TRỊ VIÊN HỆ THỐNG" : "GIÁO VIÊN", 
                    Font = new Font("Segoe UI", 10, FontStyle.Bold), 
                    TextAlign = ContentAlignment.MiddleCenter,
                    Location = new Point(0, 190),
                    Size = new Size(500, 30),
                    ForeColor = (UserRole == "ADMIN") ? Color.OrangeRed : Color.SeaGreen
                };
                card.Controls.Add(lblRole);

                // Divider
                Panel div = new Panel { Height = 1, BackColor = Color.FromArgb(240, 240, 240), Width = 400, Location = new Point(50, 230) };
                card.Controls.Add(div);

                // Information details
                AddProfileItem(card, "Hộp thư điện tử:", MyUsername, 260);
                AddProfileItem(card, "Trạng thái:", "Đang hoạt động", 310);
                AddProfileItem(card, "Mã số định danh:", (UserRole == "ADMIN") ? "AD-001" : "GV-102", 360);
                
                Button btnBack = new Button { 
                    Text = "QUAY LẠI BẢNG ĐIỀU KHIỂN", 
                    Location = new Point(125, 410), 
                    Size = new Size(250, 45), 
                    BackColor = Color.FromArgb(0, 120, 215), 
                    ForeColor = Color.White, 
                    FlatStyle = FlatStyle.Flat,
                    Font = new Font("Segoe UI", 9, FontStyle.Bold),
                    Cursor = Cursors.Hand
                };
                btnBack.FlatAppearance.BorderSize = 0;
                btnBack.Click += (s, e) => ShowDashboard();
                card.Controls.Add(btnBack);
            }
            pnlUserInfo.Visible = true;
            pnlUserInfo.BringToFront();
        }

        private void AddProfileItem(Panel card, string label, string value, int y)
        {
            Label lb = new Label { 
                Text = label, 
                Location = new Point(50, y), 
                Font = new Font("Segoe UI", 9, FontStyle.Bold), 
                ForeColor = Color.FromArgb(127, 140, 141),
                AutoSize = true 
            };
            Label vl = new Label { 
                Text = value, 
                Location = new Point(50, y + 20), 
                Font = new Font("Segoe UI", 10), 
                ForeColor = Color.FromArgb(44, 62, 80),
                AutoSize = true 
            };
            card.Controls.Add(lb);
            card.Controls.Add(vl);
        }

        private void ShowSearchPage()
        {
            if (isSidebarOpen) ToggleSidebar();
            isViewingUsers = false; 

            if (pnlSearchPage == null)
            {
                pnlSearchPage = new Panel { Size = this.ClientSize, BackColor = Color.FromArgb(240, 245, 250), Location = new Point(0, 0), Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right };
                this.Controls.Add(pnlSearchPage);
                
                // 1. Header Card
                Panel head = new Panel { Dock = DockStyle.Top, Height = 70, BackColor = Color.White };
                pnlSearchPage.Controls.Add(head);
                Label title = new Label { Text = "TRA CỨU & LỌC DỮ LIỆU", Font = new Font("Segoe UI", 18, FontStyle.Bold), ForeColor = Color.FromArgb(15, 23, 42), Location = new Point(20, 18), AutoSize = true };
                head.Controls.Add(title);

                // 2. Filter Card
                Panel filterArea = new Panel { Location = new Point(20, 90), Size = new Size(pnlSearchPage.Width - 360, 120), BackColor = Color.White };
                filterArea.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
                filterArea.Paint += (s, e) => ControlPaint.DrawBorder(e.Graphics, filterArea.ClientRectangle, Color.FromArgb(230, 230, 230), ButtonBorderStyle.Solid);
                pnlSearchPage.Controls.Add(filterArea);

                Label l1 = new Label { Text = "Lọc theo lớp học", Location = new Point(20, 20), AutoSize = true, Font = new Font("Segoe UI", 9, FontStyle.Bold), ForeColor = Color.FromArgb(100, 116, 139) };
                cbClasses = new ComboBox { Location = new Point(20, 45), Width = 180, DropDownStyle = ComboBoxStyle.DropDownList, Font = new Font("Segoe UI", 11) };
                cbClasses.Items.Add("TẤT CẢ");
                cbClasses.SelectedIndex = 0;
                filterArea.Controls.Add(l1);
                filterArea.Controls.Add(cbClasses);
                
                cbClasses.SelectedIndexChanged += (s, ev) => {
                    string selected = cbClasses.SelectedItem.ToString();
                    if (selected == "TẤT CẢ") SocketClient.Send("LIST");
                    else SocketClient.Send($"SEARCH|CLASS|{selected}");
                };

                Label l2 = new Label { Text = "Từ khóa tìm kiếm (Tên/MSSV)", Location = new Point(220, 20), AutoSize = true, Font = new Font("Segoe UI", 9, FontStyle.Bold), ForeColor = Color.FromArgb(100, 116, 139) };
                TextBox tSearch = new TextBox { Location = new Point(220, 45), Width = 300, Font = new Font("Segoe UI", 11), PlaceholderText = "Ví dụ: Nguyễn Văn A..." };
                filterArea.Controls.Add(l2);
                filterArea.Controls.Add(tSearch);
                
                Button bFind = new Button { 
                    Text = "TÌM KIẾM NGAY", 
                    Location = new Point(540, 43), 
                    Size = new Size(150, 40), 
                    BackColor = Color.FromArgb(0, 120, 215), 
                    ForeColor = Color.White, 
                    FlatStyle = FlatStyle.Flat, 
                    Font = new Font("Segoe UI", 9, FontStyle.Bold),
                    Cursor = Cursors.Hand
                };
                bFind.FlatAppearance.BorderSize = 0;
                bFind.Click += (s, ev) => SocketClient.Send($"SEARCH|ALL|{tSearch.Text.Trim()}");
                filterArea.Controls.Add(bFind);

                // 3. Results Card
                Panel gridCard = new Panel {
                    Location = new Point(20, 230),
                    Size = new Size(pnlSearchPage.Width - 360, 410),
                    BackColor = Color.White,
                    Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right
                };
                gridCard.Paint += (s, e) => ControlPaint.DrawBorder(e.Graphics, gridCard.ClientRectangle, Color.FromArgb(230, 230, 230), ButtonBorderStyle.Solid);
                pnlSearchPage.Controls.Add(gridCard);

                dgvSearchResults = new DataGridView { 
                    Dock = DockStyle.Fill, 
                    BackgroundColor = Color.White, 
                    BorderStyle = BorderStyle.None,
                    RowHeadersVisible = false,
                    AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                    AllowUserToAddRows = false,
                    RowTemplate = { Height = 40 }
                };
                dgvSearchResults.ColumnHeadersHeight = 45;
                dgvSearchResults.EnableHeadersVisualStyles = false;
                dgvSearchResults.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(248, 250, 252);
                dgvSearchResults.ColumnHeadersDefaultCellStyle.ForeColor = Color.FromArgb(71, 85, 105);
                dgvSearchResults.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 9, FontStyle.Bold);
                gridCard.Controls.Add(dgvSearchResults);

                DataTable dtRes = new DataTable();
                dtRes.Columns.Add("MSSV"); dtRes.Columns.Add("Họ Tên"); dtRes.Columns.Add("Lớp");
                dgvSearchResults.DataSource = dtRes;

                // 4. Back Button
                Button bBack = new Button { 
                    Text = "← QUAY LẠI TRẠNG CHỦ", 
                    Location = new Point(20, 660), 
                    Size = new Size(220, 45), 
                    FlatStyle = FlatStyle.Flat, 
                    BackColor = Color.FromArgb(71, 85, 105), 
                    ForeColor = Color.White, 
                    Font = new Font("Segoe UI", 9, FontStyle.Bold), 
                    Anchor = AnchorStyles.Bottom | AnchorStyles.Left,
                    Cursor = Cursors.Hand
                };
                bBack.FlatAppearance.BorderSize = 0;
                bBack.Click += (s, ev) => ShowDashboard();
                pnlSearchPage.Controls.Add(bBack);
            }
            pnlSearchPage.Visible = true;
            pnlSearchPage.BringToFront();
            SocketClient.Send("LIST_CLASSES");
            SocketClient.Send("LIST"); 
        }
    }
}
