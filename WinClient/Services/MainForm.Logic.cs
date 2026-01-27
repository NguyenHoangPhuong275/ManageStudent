using System;
using System.Data;
using System.Drawing;
using System.Windows.Forms;
using System.Threading.Tasks;
using System.IO;

namespace WinClient
{
    public partial class MainForm
    {
        private void InitTable()
        {
            dtStudents = new DataTable();
            dtStudents.Columns.Add("MSSV");
            dtStudents.Columns.Add("Họ Tên");
            dtStudents.Columns.Add("Lớp");
            dgvStudents.DataSource = dtStudents;
        }

        private void MainForm_Load(object sender, EventArgs e)
        {
            if (!SocketClient.Connect())
            {
                MessageBox.Show("Mất kết nối với máy chủ!", "Lỗi");
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
                    if (!string.IsNullOrEmpty(msg)) this.Invoke(new Action(() => ProcessMessage(msg)));
                }
                catch { break; }
            }
        }

        private void ProcessMessage(string msg)
        {
            string[] parts = msg.Split('|');
            string cmd = parts[0];

            switch (cmd)
            {
                case "REFRESH":
                    LogSystem("Cập nhật: Dữ liệu hệ thống đã thay đổi.");
                    RefreshData();
                    if (pnlSearchPage?.Visible == true) SocketClient.Send("LIST_CLASSES");
                    break;

                case "CHAT":
                    if (parts.Length >= 3) LogChat(parts[1], parts[2]);
                    break;

                case "LIST_RES":
                    UpdateGrid(msg.Substring(9));
                    break;

                case "LIST_USERS_RES":
                    UpdateUserGrid(msg.Substring(15));
                    break;
                
                case "LIST_CLASSES_RES":
                    UpdateClassFilter(msg.Substring(17));
                    break;

                case "ADD_SUCCESS": LogSystem("Thành công: Đã thêm sinh viên mới."); ClearInputs(); break;
                case "UPDATE_SUCCESS": LogSystem("Thành công: Đã cập nhật thông tin."); ClearInputs(); break;
                case "DELETE_SUCCESS": LogSystem("Thành công: Đã xóa sinh viên."); ClearInputs(); break;
                
                case "CREATE_USER_SUCCESS": 
                    MessageBox.Show("Đã cấp tài khoản thành công!"); 
                    RefreshData(); 
                    break;
                case "UPDATE_USER_SUCCESS": 
                    MessageBox.Show("Cập nhật tài khoản thành công!"); 
                    RefreshData(); 
                    break;
                case "DELETE_USER_SUCCESS": 
                    MessageBox.Show("Đã xóa tài khoản."); 
                    RefreshData(); 
                    break;

                case "EXISTS": MessageBox.Show("Lỗi: Mã này đã tồn tại!"); break;
                case "STUDENT_NOT_FOUND": 
                    LogSystem("Thông báo: Không tìm thấy kết quả."); 
                    if (pnlSearchPage?.Visible == true) ((DataTable)dgvSearchResults.DataSource).Clear();
                    break;
            }
        }

        private void UpdateGrid(string data)
        {
            if (isViewingUsers) return;
            var targetDT = (pnlSearchPage?.Visible == true) ? (DataTable)dgvSearchResults.DataSource : dtStudents;
            targetDT.Clear();
            foreach (var row in data.Split(';'))
            {
                if (string.IsNullOrWhiteSpace(row)) continue;
                string[] p = row.Split('#');
                if (p.Length >= 3) targetDT.Rows.Add(p[0], p[1], p[2]);
            }
        }

        private void UpdateUserGrid(string data)
        {
            if (!isViewingUsers) return;
            dtStudents.Clear();
            foreach (var row in data.Split(';'))
            {
                if (string.IsNullOrWhiteSpace(row)) continue;
                string[] p = row.Split('#');
                if (p.Length >= 3) 
                {
                    dtStudents.Rows.Add(p[0], p[1], p[2], "******");
                }
            }
            dgvStudents.Refresh();
        }

        private void UpdateClassFilter(string data)
        {
            if (cbClasses == null) return;
            string current = cbClasses.SelectedItem?.ToString() ?? "TẤT CẢ";
            cbClasses.Items.Clear();
            cbClasses.Items.Add("TẤT CẢ");
            foreach (var cls in data.Split(';'))
            {
                if (!string.IsNullOrWhiteSpace(cls)) cbClasses.Items.Add(cls.Trim());
            }
            
            // Khôi phục lại lựa chọn cũ nếu còn tồn tại
            int idx = cbClasses.Items.IndexOf(current);
            cbClasses.SelectedIndex = idx >= 0 ? idx : 0;
        }

        private void RefreshData()
        {
            SocketClient.Send(isViewingUsers ? "LIST_USERS" : "LIST");
        }

        private void ClearInputs()
        {
            txtID.Text = ""; txtName.Text = ""; txtClass.Text = "";
            txtID.Enabled = true;
        }

        // --- Event Handlers ---

        private void btnAdd_Click(object sender, EventArgs e)
        {
            if (isViewingUsers) return;
            string id = txtID.Text.Trim();
            if (!id.StartsWith("SV")) id = "SV" + id;
            SocketClient.Send($"ADD|{id}|{txtName.Text.Trim()}|{txtClass.Text.Trim()}");
        }

        private void btnUpdate_Click(object sender, EventArgs e)
        {
            if (isViewingUsers)
            {
                string target = txtID.Text.Trim();
                if (string.IsNullOrEmpty(target)) { MessageBox.Show("Vui lòng chọn user."); return; }

                Form f = new Form { Size = new Size(400, 350), Text = "Cập nhật User", StartPosition = FormStartPosition.CenterParent, FormBorderStyle = FormBorderStyle.FixedDialog, MaximizeBox = false, MinimizeBox = false, BackColor = Color.WhiteSmoke, Font = new Font("Segoe UI", 10) };
                int w = 320, x = (f.ClientSize.Width - w) / 2, y = 20;

                f.Controls.Add(new Label { Left = x, Top = y, AutoSize = true, Text = "Mật khẩu mới:" });
                TextBox tPass = new TextBox { Left = x, Top = y + 25, Width = w, PlaceholderText = "Để trống nếu không đổi" }; f.Controls.Add(tPass);
                
                y += 65;
                f.Controls.Add(new Label { Left = x, Top = y, AutoSize = true, Text = "Họ Tên:" });
                TextBox tName = new TextBox { Left = x, Top = y + 25, Width = w, Text = txtName.Text }; f.Controls.Add(tName);
                
                y += 65;
                f.Controls.Add(new Label { Left = x, Top = y, AutoSize = true, Text = "Vai trò (ADMIN/USER):" });
                TextBox tRole = new TextBox { Left = x, Top = y + 25, Width = w, Text = "USER" }; f.Controls.Add(tRole);
                
                y += 75;
                Button bOk = new Button { Left = x, Top = y, Width = w, Height = 45, Text = "CẬP NHẬT", BackColor = Color.FromArgb(0, 120, 215), ForeColor = Color.White, FlatStyle = FlatStyle.Flat, Font = new Font("Segoe UI", 10, FontStyle.Bold) };
                bOk.Click += (s, ev) => { SocketClient.Send($"UPDATE_USER|{target}|{tPass.Text}|{tRole.Text}|{tName.Text}"); f.Close(); };
                f.Controls.Add(bOk);
                f.ShowDialog();
                return;
            }
            SocketClient.Send($"UPDATE|{txtID.Text.Trim()}|{txtName.Text.Trim()}|{txtClass.Text.Trim()}");
        }

        private void btnDelete_Click(object sender, EventArgs e)
        {
            string id = txtID.Text.Trim();
            if (string.IsNullOrEmpty(id)) return;
            
            string cmd = isViewingUsers ? "DELETE_USER" : "DELETE";
            if (MessageBox.Show($"Bạn chắc chắn muốn xóa {id}?", "Xác nhận", MessageBoxButtons.YesNo) == DialogResult.Yes)
                SocketClient.Send($"{cmd}|{id}");
        }

        private void btnRefresh_Click(object sender, EventArgs e) => RefreshData();

        private void dgvStudents_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0) return;
            var row = dgvStudents.Rows[e.RowIndex];
            txtID.Text = row.Cells[0].Value?.ToString();
            if (!isViewingUsers)
            {
                txtName.Text = row.Cells[1].Value?.ToString();
                txtClass.Text = row.Cells[2].Value?.ToString();
                txtID.Enabled = false;
            }
        }
        
        private void BtnSend_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(txtChatInput.Text)) return;
            SocketClient.Send($"CHAT|{MyFullName}|{txtChatInput.Text}");
            txtChatInput.Text = "";
        }

        private void LogSystem(string content)
        {
            if (rtbChat.IsDisposed) return;
            string time = DateTime.Now.ToString("HH:mm:ss");
            rtbChat.Invoke(new Action(() => {
                rtbChat.SelectionStart = rtbChat.TextLength;
                rtbChat.SelectionLength = 0;
                rtbChat.SelectionColor = Color.Red;
                rtbChat.AppendText($"[{time}] Hệ thống: {content}\n");
                rtbChat.ScrollToCaret();
            }));
        }

        private void LogChat(string user, string content)
        {
            if (rtbChat.IsDisposed) return;
            bool isMe = (user == MyFullName || user == MyUsername);
            rtbChat.Invoke(new Action(() => {
                rtbChat.SelectionStart = rtbChat.TextLength;
                rtbChat.SelectionLength = 0;
                rtbChat.SelectionColor = isMe ? Color.Blue : Color.DarkGreen;
                rtbChat.SelectionFont = new Font(rtbChat.Font, FontStyle.Bold);
                rtbChat.AppendText($"{(isMe ? "Tôi" : user)}: ");
                rtbChat.SelectionColor = Color.Black;
                rtbChat.SelectionFont = new Font(rtbChat.Font, FontStyle.Regular);
                rtbChat.AppendText($"{content}\n");
                rtbChat.ScrollToCaret();
            }));
        }

        private void ImportCSV()
        {
            OpenFileDialog open = new OpenFileDialog { Filter = "CSV Files (*.csv)|*.csv" };
            if (open.ShowDialog() == DialogResult.OK)
            {
                var lines = File.ReadAllLines(open.FileName);
                foreach (var line in lines)
                {
                    var p = line.Split(',');
                    if (p.Length >= 2) SocketClient.Send($"ADD|{p[0].Trim()}|{p[1].Trim()}|{(p.Length > 2 ? p[2].Trim() : "")}");
                }
                MessageBox.Show("Đã gửi lệnh nhập dữ liệu!");
            }
        }

        private void BtnViewUsers_Click(object sender, EventArgs e)
        {
            Button btn = sender as Button;
            isViewingUsers = !isViewingUsers;
            
            // Tạm ngắt binding để sửa cột
            dgvStudents.DataSource = null; 
            dtStudents.Rows.Clear();
            dtStudents.Columns.Clear();

            if (isViewingUsers)
            {
                if (btn != null) { btn.Text = "XEM SINH VIÊN"; btn.BackColor = Color.DarkOrange; }
                dtStudents.Columns.Add("Email"); 
                dtStudents.Columns.Add("Họ Tên"); 
                dtStudents.Columns.Add("Quyền");
                dtStudents.Columns.Add("Mật khẩu");
                SocketClient.Send("LIST_USERS");
            }
            else
            {
                if (btn != null) { btn.Text = "XEM TÀI KHOẢN"; btn.BackColor = Color.Teal; }
                dtStudents.Columns.Add("MSSV"); 
                dtStudents.Columns.Add("Họ Tên"); 
                dtStudents.Columns.Add("Lớp");
                SocketClient.Send("LIST");
            }
            
            dgvStudents.DataSource = dtStudents;
            txtID.Enabled = true;
        }

        private void BtnCreateUser_Click(object sender, EventArgs e)
        {
            Form f = new Form { Size = new Size(400, 340), Text = "Cấp Tài Khoản Giáo Viên", StartPosition = FormStartPosition.CenterParent, FormBorderStyle = FormBorderStyle.FixedDialog, MaximizeBox = false, MinimizeBox = false, BackColor = Color.WhiteSmoke, Font = new Font("Segoe UI", 10) };
            int w = 320, x = (f.ClientSize.Width - w) / 2, y = 20;

            f.Controls.Add(new Label { Left = x, Top = y, AutoSize = true, Text = "Email / Tên đăng nhập:" });
            TextBox tEmail = new TextBox { Left = x, Top = y + 25, Width = w }; f.Controls.Add(tEmail);
            
            y += 65;
            f.Controls.Add(new Label { Left = x, Top = y, AutoSize = true, Text = "Mật khẩu mặc định:" });
            TextBox tPass = new TextBox { Left = x, Top = y + 25, Width = w }; f.Controls.Add(tPass);
            
            y += 65;
            f.Controls.Add(new Label { Left = x, Top = y, AutoSize = true, Text = "Họ Tên Giáo Viên:" });
            TextBox tName = new TextBox { Left = x, Top = y + 25, Width = w }; f.Controls.Add(tName);
            
            y += 75;
            Button bOk = new Button { Left = x, Top = y, Width = w, Height = 45, Text = "TẠO TÀI KHOẢN", BackColor = Color.FromArgb(0, 120, 215), ForeColor = Color.White, FlatStyle = FlatStyle.Flat, Font = new Font("Segoe UI", 10, FontStyle.Bold) };
            bOk.Click += (s, ev) => {
                if(string.IsNullOrEmpty(tEmail.Text) || string.IsNullOrEmpty(tPass.Text)) { MessageBox.Show("Vui lòng nhập Email và Mật khẩu."); return; }
                SocketClient.Send($"CREATE_USER|{tEmail.Text.Trim()}|{tPass.Text.Trim()}|USER|{tName.Text.Trim()}");
                f.Close();
            };
            f.Controls.Add(bOk);
            f.ShowDialog();
        }
    }
}
