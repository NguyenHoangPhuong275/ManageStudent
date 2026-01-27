using System;
<<<<<<< HEAD
using System.Collections.Generic;
=======
>>>>>>> a9f7c149d44583bd431f7952ef8661382757e01c
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
<<<<<<< HEAD
using System.IO;
using Server.Core;
using Server.Models;

namespace Server
{
    class Program
    {
        private static readonly DatabaseManager db = new DatabaseManager();
        private static readonly List<TcpClient> clients = new List<TcpClient>();
        private const int Port = 8888;

        public enum LogType { INFO, ERROR, SUCCESS, CMD, DATA }

        static void Main(string[] args)
        {
            Console.OutputEncoding = Encoding.UTF8;
            db.Initialize();

            TcpListener listener = new TcpListener(IPAddress.Any, Port);
            listener.Start();

            Log($"SERVER started on port {Port}", LogType.SUCCESS);
            Log($"Database: {Path.GetFullPath("StudentManager.db")}");

            while (true)
            {
                try
                {
                    TcpClient client = listener.AcceptTcpClient();
                    lock (clients) clients.Add(client);
                    
                    Thread clientThread = new Thread(HandleClient);
                    clientThread.IsBackground = true;
                    clientThread.Start(client);
                }
                catch (Exception ex)
                {
                    Log($"Accept Error: {ex.Message}", LogType.ERROR);
                }
            }
        }

        private static void HandleClient(object obj)
        {
            TcpClient client = (TcpClient)obj;
            NetworkStream stream = client.GetStream();
            string clientEndPoint = client.Client.RemoteEndPoint.ToString();

            Log($"Client connected: {clientEndPoint}", LogType.INFO);

            try
            {
                while (true)
                {
                    string request = Receive(stream);
                    if (string.IsNullOrEmpty(request)) break;

                    Log($"Request from {clientEndPoint}: {request}", LogType.CMD);
                    ProcessCommand(request, stream);
                }
            }
            catch (Exception ex)
            {
                Log($"Session Error ({clientEndPoint}): {ex.Message}", LogType.ERROR);
            }
            finally
            {
                lock (clients) clients.Remove(client);
                client.Close();
                Log($"Client disconnected: {clientEndPoint}", LogType.INFO);
            }
        }

        private static void ProcessCommand(string request, NetworkStream stream)
        {
            string[] p = request.Split('|');
            if (p.Length == 0) return;

            string cmd = p[0].Trim();

            switch (cmd)
            {
                case "LOGIN":
                    var user = db.Authenticate(p[1], p[2]);
                    if (user != null) Send(stream, $"LOGIN_SUCCESS|{user.Role}|{user.FullName}");
                    else Send(stream, "LOGIN_FAIL");
                    break;

                case "LIST":
                    var students = db.GetAllStudents();
                    Send(stream, "LIST_RES|" + string.Join(";", students) + ";");
                    break;

                case "ADD":
                    if (db.AddStudent(p[1], p[2], p[3]))
                    {
                        Send(stream, "ADD_SUCCESS");
                        Broadcast("REFRESH");
                    }
                    else Send(stream, "EXISTS");
                    break;

                case "UPDATE":
                    if (db.UpdateStudent(p[1], p[2], p[3]))
                    {
                        Send(stream, "UPDATE_SUCCESS");
                        Broadcast("REFRESH");
                    }
                    else Send(stream, "STUDENT_NOT_FOUND");
                    break;

                case "DELETE":
                    if (db.DeleteStudent(p[1]))
                    {
                        Send(stream, "DELETE_SUCCESS");
                        Broadcast("REFRESH");
                    }
                    else Send(stream, "STUDENT_NOT_FOUND");
                    break;

                case "SEARCH":
                    var results = db.SearchStudents(p[1], p[2]);
                    if (results.Count > 0) Send(stream, "LIST_RES|" + string.Join(";", results) + ";");
                    else Send(stream, "STUDENT_NOT_FOUND");
                    break;

                case "CHAT":
                    Broadcast($"CHAT|{p[1]}|{p[2]}");
                    break;

                case "LIST_USERS":
                    var users = db.GetAllUsers();
                    Send(stream, "LIST_USERS_RES|" + string.Join(";", users) + ";");
                    break;

                case "LIST_CLASSES":
                    var classes = db.GetAllClasses();
                    Send(stream, "LIST_CLASSES_RES|" + string.Join(";", classes) + ";");
                    break;

                case "CREATE_USER":
                    string fn = p.Length > 4 ? p[4] : "Giáo Viên";
                    if (db.CreateUser(p[1], p[2], p[3], fn)) 
                    {
                        Send(stream, "CREATE_USER_SUCCESS");
                        Broadcast("REFRESH");
                    }
                    else Send(stream, "CREATE_USER_FAIL");
                    break;

                case "UPDATE_USER":
                    string ufn = p.Length > 4 ? p[4] : "";
                    if (db.UpdateUser(p[1], p[2], p[3], ufn)) 
                    {
                        Send(stream, "UPDATE_USER_SUCCESS");
                        Broadcast("REFRESH");
                    }
                    else Send(stream, "UPDATE_USER_FAIL");
                    break;

                case "DELETE_USER":
                    if (db.DeleteUser(p[1])) 
                    {
                        Send(stream, "DELETE_USER_SUCCESS");
                        Broadcast("REFRESH");
                    }
                    else Send(stream, "DELETE_USER_FAIL");
                    break;

                default:
                    Send(stream, "INVALID_COMMAND");
                    break;
            }
        }

        private static void Broadcast(string msg)
        {
            byte[] data = Encoding.UTF8.GetBytes(msg);
            lock (clients)
            {
                foreach (var c in clients.ToArray())
                {
                    try { c.GetStream().Write(data, 0, data.Length); }
                    catch { /* Connection closed */ }
                }
            }
        }

        private static void Send(NetworkStream s, string msg)
        {
            try {
                byte[] data = Encoding.UTF8.GetBytes(msg);
                s.Write(data, 0, data.Length);
            } catch { }
        }

        private static string Receive(NetworkStream s)
        {
            byte[] b = new byte[8192];
            try
            {
                int n = s.Read(b, 0, b.Length);
                return n > 0 ? Encoding.UTF8.GetString(b, 0, n) : null;
            }
            catch { return null; }
        }

        public static void Log(string msg, LogType type = LogType.INFO)
        {
            string ts = DateTime.Now.ToString("HH:mm:ss");
            Console.Write($"[{ts}] ");
            switch (type)
            {
                case LogType.INFO: Console.ForegroundColor = ConsoleColor.Gray; break;
                case LogType.ERROR: Console.ForegroundColor = ConsoleColor.Red; break;
                case LogType.SUCCESS: Console.ForegroundColor = ConsoleColor.Green; break;
                case LogType.CMD: Console.ForegroundColor = ConsoleColor.Cyan; break;
                case LogType.DATA: Console.ForegroundColor = ConsoleColor.Yellow; break;
            }
            Console.Write($"[{type}] ");
            Console.ResetColor();
            Console.WriteLine(msg);
        }
    }
=======
using Microsoft.Data.Sqlite;
using System.IO;
using System.Collections.Generic;

class Server
{
    static string dbFile = "StudentManager.db";
    static string connStr = $"Data Source={dbFile}";

    enum LogType { INFO, ERROR, SUCCESS, CMD, DATA }

    static void Log(string msg, LogType type = LogType.INFO)
    {
        string ts = DateTime.Now.ToString("HH:mm:ss");
        Console.Write($"[{ts}] ");

        switch (type)
        {
            case LogType.INFO: Console.ForegroundColor = ConsoleColor.Gray; break;
            case LogType.ERROR: Console.ForegroundColor = ConsoleColor.Red; break;
            case LogType.SUCCESS: Console.ForegroundColor = ConsoleColor.Green; break;
            case LogType.CMD: Console.ForegroundColor = ConsoleColor.Cyan; break;
            case LogType.DATA: Console.ForegroundColor = ConsoleColor.Yellow; break;
        }
        Console.Write($"[{type}] ");
        Console.ResetColor();
        Console.WriteLine(msg);
    }
    
    // Danh sách Client để Broadcast
    static List<TcpClient> clients = new List<TcpClient>();

    static void Main()
    {
        InitDatabase();

        TcpListener server = new TcpListener(IPAddress.Any, 8888);
        server.Start();
        Log("SERVER running on port 8888...", LogType.SUCCESS);
        Log($"Database file: {Path.GetFullPath(dbFile)}");

        while (true)
        {
            TcpClient client = server.AcceptTcpClient();
            Log("New client connected", LogType.INFO);
            
            lock(clients) clients.Add(client);
            
            new Thread(HandleClient).Start(client);
        }
    }

    static void InitDatabase()
    {
        if (!File.Exists(dbFile))
        {
            Log("Tao moi Database...", LogType.INFO);
        }

        using (var conn = new SqliteConnection(connStr))
        {
            conn.Open();

            // 1. Tạo bảng Users
            string tblUsers = @"
                CREATE TABLE IF NOT EXISTS Users (
                    Username TEXT PRIMARY KEY,
                    Password TEXT,
                    Role TEXT,
                    FullName TEXT
                );
            ";
            new SqliteCommand(tblUsers, conn).ExecuteNonQuery();

            // Sửa lỗi thiếu cột cho DB cũ (Migration)
            try { new SqliteCommand("ALTER TABLE Users ADD COLUMN FullName TEXT", conn).ExecuteNonQuery(); } catch { }
            try { new SqliteCommand("ALTER TABLE Users ADD COLUMN Email TEXT", conn).ExecuteNonQuery(); } catch { }
            try { new SqliteCommand("ALTER TABLE Users ADD COLUMN Phone TEXT", conn).ExecuteNonQuery(); } catch { }
            try { new SqliteCommand("ALTER TABLE Users ADD COLUMN Avatar TEXT", conn).ExecuteNonQuery(); } catch { }
            try { new SqliteCommand("ALTER TABLE Users ADD COLUMN TeachingClass TEXT", conn).ExecuteNonQuery(); } catch { }
            try { new SqliteCommand("ALTER TABLE Users ADD COLUMN Subject TEXT", conn).ExecuteNonQuery(); } catch { }

            // Migration: Cập nhật email mặc định cho admin và gv01 nếu dữ liệu cũ chưa có
            try { new SqliteCommand("UPDATE Users SET Email='admin@admin.edu.vn' WHERE Username='admin' AND (Email IS NULL OR Email='')", conn).ExecuteNonQuery(); } catch { }
            try { new SqliteCommand("UPDATE Users SET Email='gv01@school.edu.vn' WHERE Username='gv01' AND (Email IS NULL OR Email='')", conn).ExecuteNonQuery(); } catch { }

            // Chỉ nạp Admin và Giáo viên mẫu nếu bảng trống
            var checkUserCmd = new SqliteCommand("SELECT COUNT(*) FROM Users", conn);
            if ((long)checkUserCmd.ExecuteScalar() == 0)
            {
                Log("Bảng Users trống, nạp tài khoản mặc định...", LogType.INFO);
                string initUsers = @"
                    INSERT INTO Users VALUES ('admin', 'admin123', 'ADMIN', 'Quản Trị Viên', 'admin@admin.edu.vn');
                    INSERT INTO Users VALUES ('gv01', '123', 'USER', 'Nguyễn Văn A', 'gv01@school.edu.vn');
                ";
                new SqliteCommand(initUsers, conn).ExecuteNonQuery();
            }

            // 2. Tạo bảng Students
            string tblStudents = @"
                CREATE TABLE IF NOT EXISTS Students (
                    StudentID TEXT PRIMARY KEY,
                    FullName TEXT,
                    Class TEXT,
                    Phone TEXT,
                    Email TEXT
                );
            ";
            new SqliteCommand(tblStudents, conn).ExecuteNonQuery();
            
            // Migration: Thêm cột cho DB cũ nếu chưa có
            try { new SqliteCommand("ALTER TABLE Students ADD COLUMN Phone TEXT", conn).ExecuteNonQuery(); } catch { }
            try { new SqliteCommand("ALTER TABLE Students ADD COLUMN Email TEXT", conn).ExecuteNonQuery(); } catch { }

            // Chỉ nạp SV mẫu nếu bảng trống hoặc dữ liệu quá ít (data cũ)
            var checkStudentCmd = new SqliteCommand("SELECT COUNT(*) FROM Students", conn);
            long count = (long)checkStudentCmd.ExecuteScalar();

            if (count < 10) // Nếu số lượng SV < 10 (tức là đang dùng data cũ), thì nạp lại
            {
                Log("Phát hiện dữ liệu cũ, cập nhật danh sách đầy đủ...", LogType.INFO);
                new SqliteCommand("DELETE FROM Students", conn).ExecuteNonQuery(); // Xóa data cũ để tránh trùng ID

                string initStudents = @"
                    INSERT INTO Students VALUES ('SV001', 'Le Ngoc Chau Anh', 'CNTT', '0901234567', 'sv001@school.edu.vn');
                    INSERT INTO Students VALUES ('SV002', 'Nguyen Van An', 'CNTT', '0901234568', 'sv002@school.edu.vn');
                    INSERT INTO Students VALUES ('SV003', 'Dao Gia Bao', 'CNTT', '0901234569', 'sv003@school.edu.vn');
                    INSERT INTO Students VALUES ('SV004', 'Ngo Thu Cuc', 'CNTT', '0901234570', 'sv004@school.edu.vn');
                    INSERT INTO Students VALUES ('SV005', 'Hoang Van Tuan Dat', 'CNTT', '0901234571', 'sv005@school.edu.vn');
                    INSERT INTO Students VALUES ('SV006', 'Le Trung Dung', 'CNTT', '0901234572', 'sv006@school.edu.vn');
                    INSERT INTO Students VALUES ('SV007', 'Vu Thi Thuy Duong', 'CNTT', '0901234573', 'sv007@school.edu.vn');
                    INSERT INTO Students VALUES ('SV008', 'Pham Thi My Duyen', 'CNTT', '0901234574', 'sv008@school.edu.vn');
                    INSERT INTO Students VALUES ('SV009', 'Do Hoang Giang', 'CNTT', '0901234575', 'sv009@school.edu.vn');
                    INSERT INTO Students VALUES ('SV010', 'Bui Thi Thu Hien', 'CNTT', '0901234576', 'sv010@school.edu.vn');
                    INSERT INTO Students VALUES ('SV011', 'Dang Nhat Huy', 'CNTT', '0901234577', 'sv011@school.edu.vn');
                    INSERT INTO Students VALUES ('SV012', 'Ngo Thi Kim Huyen', 'CNTT', '0901234578', 'sv012@school.edu.vn');
                    INSERT INTO Students VALUES ('SV013', 'Tran Lam', 'CNTT', '0901234579', 'sv013@school.edu.vn');
                    INSERT INTO Students VALUES ('SV014', 'Le Ha My Linh', 'CNTT', '0901234580', 'sv014@school.edu.vn');
                    INSERT INTO Students VALUES ('SV015', 'Tran Ngo My Linh', 'CNTT', '0901234581', 'sv015@school.edu.vn');
                    INSERT INTO Students VALUES ('SV016', 'Tran Hoai Nam', 'CNTT', '0901234582', 'sv016@school.edu.vn');
                    INSERT INTO Students VALUES ('SV017', 'Nguyen Tran Trung Nam', 'CNTT', '0901234583', 'sv017@school.edu.vn');
                    INSERT INTO Students VALUES ('SV018', 'Le Hoang Phi Ngan', 'CNTT', '0901234584', 'sv018@school.edu.vn');
                    INSERT INTO Students VALUES ('SV019', 'Chau Ngoc Tuyet Ngan', 'CNTT', '0901234585', 'sv019@school.edu.vn');
                    INSERT INTO Students VALUES ('SV020', 'Vu Van Nghinh', 'CNTT', '0901234586', 'sv020@school.edu.vn');
                    INSERT INTO Students VALUES ('SV021', 'Do Tan Phat', 'CNTT', '0901234587', 'sv021@school.edu.vn');
                    INSERT INTO Students VALUES ('SV022', 'Ngo Duy Phuc', 'CNTT', '0901234588', 'sv022@school.edu.vn');
                    INSERT INTO Students VALUES ('SV023', 'Le Anh Quynh', 'CNTT', '0901234589', 'sv023@school.edu.vn');
                    INSERT INTO Students VALUES ('SV024', 'Nguyen Thi Truc Quynh', 'CNTT', '0901234590', 'sv024@school.edu.vn');
                    INSERT INTO Students VALUES ('SV025', 'Ha Van Sa', 'CNTT', '0901234591', 'sv025@school.edu.vn');
                    INSERT INTO Students VALUES ('SV026', 'Bui Ngoc Sang', 'CNTT', '0901234592', 'sv026@school.edu.vn');
                    INSERT INTO Students VALUES ('SV027', 'Le Tan Tai', 'CNTT', '0901234593', 'sv027@school.edu.vn');
                    INSERT INTO Students VALUES ('SV028', 'Do Tan Tu', 'CNTT', '0901234594', 'sv028@school.edu.vn');
                    INSERT INTO Students VALUES ('SV029', 'Truong Thanh Tung', 'CNTT', '0901234595', 'sv029@school.edu.vn');
                    INSERT INTO Students VALUES ('SV030', 'Tran Thu Uyen', 'CNTT', '0901234596', 'sv030@school.edu.vn');
                    INSERT INTO Students VALUES ('SV031', 'Ngo Thuy Uyen', 'CNTT', '0901234597', 'sv031@school.edu.vn');
                    INSERT INTO Students VALUES ('SV032', 'Ngo Huu Vinh', 'CNTT', '0901234598', 'sv032@school.edu.vn');
                    INSERT INTO Students VALUES ('SV033', 'Tran Huu Vinh', 'CNTT', '0901234599', 'sv033@school.edu.vn');
                    INSERT INTO Students VALUES ('SV034', 'Pham Xuyen', 'CNTT', '0901234600', 'sv034@school.edu.vn');
                    INSERT INTO Students VALUES ('SV035', 'Ho Ngoc Nhu Y', 'CNTT', '0901234601', 'sv035@school.edu.vn');
                ";
                new SqliteCommand(initStudents, conn).ExecuteNonQuery();
            }
            
            Log("Kiem tra Database: OK", LogType.SUCCESS);
        }
    }

    static void HandleClient(object obj)
    {
        TcpClient client = (TcpClient)obj;
        NetworkStream stream = client.GetStream();

        try
        {
            while (true)
            {
                string request = Receive(stream);
                if (request == null) break;

                Log("Request: " + request, LogType.CMD);

                string[] p = request.Split('|');
                string cmd = p[0].Trim();

                switch (cmd)
                {
                    case "LOGIN": HandleLogin(p, stream); break;
                    case "CHECK": CheckStudent(p, stream); break;
                    
                    case "ADD":
                        AddStudent(p, stream);
                        Broadcast("REFRESH");
                        break;
                    
                    case "UPDATE":
                        UpdateStudent(p, stream);
                        Broadcast("REFRESH");
                        break;
                    
                    case "DELETE":
                        DeleteStudent(p, stream);
                        Broadcast("REFRESH");
                        break;
                    
                    case "SEARCH": SearchStudent(p, stream); break;
                    case "LIST": ListStudents(stream); break;
                    
                    case "SEARCH_USER": SearchUser(p, stream); break;
                    case "CHAT":
                        string chatMsg = $"CHAT|{p[1]}|{p[2]}"; 
                        Broadcast(chatMsg);
                        break;
                    
                    case "CREATE_USER": 
                        CreateUser(p, stream);
                        break;

                    case "LIST_USERS": 
                        ListUsers(stream);
                        break;
                    
                    case "DELETE_USER": 
                        DeleteUser(p, stream);
                        break;

                    case "UPDATE_USER": // Admin sửa user
                        UpdateUser(p, stream);
                        break;

                    default: Send(stream, "INVALID_COMMAND"); break;
                }
            }
        }
        catch (Exception ex)
        {
            Log("SERVER ERROR: " + ex.Message, LogType.ERROR);
        }
        finally
        {
            lock(clients) clients.Remove(client);
            client.Close();
            Log("Client disconnected", LogType.INFO);
        }
    }

    static void Broadcast(string msg)
    {
        lock (clients)
        {
            foreach (var c in clients)
            {
                try {
                    byte[] data = Encoding.UTF8.GetBytes(msg);
                    c.GetStream().Write(data, 0, data.Length);
                }
                catch { }
            }
        }
    }

    // SQL FUNCTIONS

    static void HandleLogin(string[] p, NetworkStream s)
    {
        using var conn = new SqliteConnection(connStr);
        conn.Open();
        var cmd = new SqliteCommand("SELECT Username, Role, FullName, Avatar, Email FROM Users WHERE (Username=@u OR Email=@u) AND Password=@p", conn);
        cmd.Parameters.AddWithValue("@u", p[1]);
        cmd.Parameters.AddWithValue("@p", p[2]);
        using var r = cmd.ExecuteReader();
        if (r.Read()) 
        {
            string realUser = r["Username"].ToString(); // Lấy ID gốc
            string role = r["Role"].ToString();
            string fn = r["FullName"].ToString();
            string avt = r["Avatar"] != null ? r["Avatar"].ToString() : "";
            string email = r["Email"] != null ? r["Email"].ToString() : "";
            if (string.IsNullOrEmpty(fn)) fn = p[1]; // Nếu chưa có tên thì lấy tạm Username
            
            // Trả về: LOGIN_SUCCESS | Role | FullName | Avatar | RealUsername | Email
            Send(s, $"LOGIN_SUCCESS|{role}|{fn}|{avt}|{realUser}|{email}");
        }
        else Send(s, "LOGIN_FAIL");
    }

    static void CheckStudent(string[] p, NetworkStream s)
    {
        using var conn = new SqliteConnection(connStr);
        conn.Open();
        var cmd = new SqliteCommand("SELECT COUNT(*) FROM Students WHERE StudentID=@id", conn);
        cmd.Parameters.AddWithValue("@id", p[1]);
        long count = (long)cmd.ExecuteScalar();
        Send(s, count > 0 ? "EXISTS" : "NOT_EXISTS");
    }

    static void AddStudent(string[] p, NetworkStream s)
    {
        try {
            using var conn = new SqliteConnection(connStr);
            conn.Open();
            var cmd = new SqliteCommand("INSERT INTO Students VALUES (@id,@name,@class,@phone,@email)", conn);
            cmd.Parameters.AddWithValue("@id", p[1]);
            cmd.Parameters.AddWithValue("@name", p[2]);
            cmd.Parameters.AddWithValue("@class", p[3]);
            cmd.Parameters.AddWithValue("@phone", p.Length > 4 ? p[4] : "");
            cmd.Parameters.AddWithValue("@email", p.Length > 5 ? p[5] : "");
            cmd.ExecuteNonQuery();
            Send(s, "ADD_SUCCESS");
        } catch { Send(s, "EXISTS"); }
    }

    static void UpdateStudent(string[] p, NetworkStream s)
    {
        using var conn = new SqliteConnection(connStr);
        conn.Open();
        var cmd = new SqliteCommand("UPDATE Students SET FullName=@name, Class=@class, Phone=@phone, Email=@email WHERE StudentID=@id", conn);
        cmd.Parameters.AddWithValue("@id", p[1]);
        cmd.Parameters.AddWithValue("@name", p[2]);
        cmd.Parameters.AddWithValue("@class", p[3]);
        cmd.Parameters.AddWithValue("@phone", p.Length > 4 ? p[4] : "");
        cmd.Parameters.AddWithValue("@email", p.Length > 5 ? p[5] : "");
        int rows = cmd.ExecuteNonQuery();
        Send(s, rows > 0 ? "UPDATE_SUCCESS" : "STUDENT_NOT_FOUND");
    }

    static void DeleteStudent(string[] p, NetworkStream s)
    {
        using var conn = new SqliteConnection(connStr);
        conn.Open();
        var cmd = new SqliteCommand("DELETE FROM Students WHERE StudentID=@id", conn);
        cmd.Parameters.AddWithValue("@id", p[1]);
        int rows = cmd.ExecuteNonQuery();
        Send(s, rows > 0 ? "DELETE_SUCCESS" : "STUDENT_NOT_FOUND");
    }

    static void SearchStudent(string[] p, NetworkStream s)
    {
        if (p.Length < 3) return;
        
        string type = p[1].Trim();
        string val = p[2].Trim();
        
        using var conn = new SqliteConnection(connStr);
        conn.Open();
        
        string query = "";
        if (type == "ID") query = "SELECT * FROM Students WHERE StudentID=@v";
        else if (type == "CLASS") query = "SELECT * FROM Students WHERE Class LIKE @v";
        else if (type == "ALL") query = "SELECT * FROM Students WHERE StudentID LIKE @v OR FullName LIKE @v OR Class LIKE @v";
            
        var cmd = new SqliteCommand(query, conn);
        cmd.Parameters.AddWithValue("@v", (type == "ID") ? val : $"%{val}%");
        
        using var r = cmd.ExecuteReader();
        
        StringBuilder sb = new StringBuilder("LIST_RES|");
        int count = 0;
        while (r.Read())
        {
            count++;
            sb.Append($"{r["StudentID"]}#{r["FullName"]}#{r["Class"]}#{r["Phone"]}#{r["Email"]};");
        }

        if (count > 0) 
        {
            Send(s, sb.ToString());
            Log($"Search [{type}:{val}] found {count} results.", LogType.SUCCESS);
        }
        else 
        {
            Send(s, "STUDENT_NOT_FOUND");
            Log($"Search [{type}:{val}] NOT FOUND in database.", LogType.INFO);
        }
    }

    static void ListStudents(NetworkStream s)
    {
        using var conn = new SqliteConnection(connStr);
        conn.Open();
        var cmd = new SqliteCommand("SELECT StudentID, FullName, Class, Phone, Email FROM Students ORDER BY FullName", conn);
        using var r = cmd.ExecuteReader();
        
        StringBuilder sb = new StringBuilder("LIST_RES|");
        while (r.Read())
        {
            sb.Append($"{r["StudentID"]}#{r["FullName"]}#{r["Class"]}#{r["Phone"]}#{r["Email"]};");
        }
        Send(s, sb.ToString());
    }

    static void Send(NetworkStream s, string msg)
    {
        byte[] data = Encoding.UTF8.GetBytes(msg);
        s.Write(data, 0, data.Length);
    }

    static string Receive(NetworkStream s)
    {
        byte[] b = new byte[1024 * 1024]; // Tăng buffer lên 1MB để nhận ảnh
        try {
            int n = s.Read(b, 0, b.Length);
            if (n == 0) return null;
            return Encoding.UTF8.GetString(b, 0, n);
        } catch { return null; }
    }

    static void CreateUser(string[] p, NetworkStream s)
    {
        try {
            using var conn = new SqliteConnection(connStr);
            conn.Open();
            var cmd = new SqliteCommand("INSERT INTO Users (Username, Password, Role, FullName, Email) VALUES (@u, @p, @r, @fn, @em)", conn);
            cmd.Parameters.AddWithValue("@u", p[1]);
            cmd.Parameters.AddWithValue("@p", p[2]);
            cmd.Parameters.AddWithValue("@r", p[3]);
            cmd.Parameters.AddWithValue("@fn", p.Length > 4 ? p[4] : "Giáo Viên"); // Nhận thêm Họ Tên
            cmd.Parameters.AddWithValue("@em", p.Length > 5 ? p[5] : ""); // Nhận thêm Email
            cmd.ExecuteNonQuery();
            Send(s, "CREATE_USER_SUCCESS");
        } catch { Send(s, "CREATE_USER_FAIL"); }
    }

    static void ListUsers(NetworkStream s)
    {
        using var conn = new SqliteConnection(connStr);
        conn.Open();
        var cmd = new SqliteCommand("SELECT Username, FullName, Role, Email, Phone, TeachingClass, Subject, Avatar FROM Users", conn);
        using var r = cmd.ExecuteReader();
        
        StringBuilder sb = new StringBuilder("LIST_USERS_RES|");
        while (r.Read())
        {
            // Gửi về: Username # FullName # Role # Email # Phone # TeachingClass # Subject # Avatar
            sb.Append($"{r["Username"]}#{r["FullName"]}#{r["Role"]}#{r["Email"]}#{r["Phone"]}#{r["TeachingClass"]}#{r["Subject"]}#{r["Avatar"]};");
        }
        Send(s, sb.ToString());
    }

    static void DeleteUser(string[] p, NetworkStream s)
    {
        try {
            using var conn = new SqliteConnection(connStr);
            conn.Open();
            // Không cho phép xóa Admin chính
            if (p[1] == "admin@admin.edu.vn") 
            {
                Send(s, "DELETE_USER_FAIL|Cannot delete Super Admin");
                return; 
            }

            var cmd = new SqliteCommand("DELETE FROM Users WHERE Username=@u", conn);
            cmd.Parameters.AddWithValue("@u", p[1]);
            int rows = cmd.ExecuteNonQuery();
            Send(s, rows > 0 ? "DELETE_USER_SUCCESS" : "DELETE_USER_FAIL|User not found");
        } catch { Send(s, "DELETE_USER_FAIL|Error"); }
    }

    static void UpdateUser(string[] p, NetworkStream s)
    {
        // FORMAT: UPDATE_USER | Username | NewPass | NewRole | FullName | Email | Phone | TeachingClass | Subject | Avatar
        // Index:      0      |     1    |    2    |    3    |    4     |   5   |   6   |       7       |    8    |   9
        try {
            using var conn = new SqliteConnection(connStr);
            conn.Open();
            
            string sql = "UPDATE Users SET Role=@r";
            if (!string.IsNullOrEmpty(p[2])) sql += ", Password=@p"; // Chỉ update Pass nếu không rỗng
            
            if (p.Length > 4 && !string.IsNullOrEmpty(p[4])) sql += ", FullName=@fn";
            if (p.Length > 5 && !string.IsNullOrEmpty(p[5])) sql += ", Email=@em";
            if (p.Length > 6 && !string.IsNullOrEmpty(p[6])) sql += ", Phone=@ph";
            if (p.Length > 7 && !string.IsNullOrEmpty(p[7])) sql += ", TeachingClass=@tc";
            if (p.Length > 8 && !string.IsNullOrEmpty(p[8])) sql += ", Subject=@sj";
            if (p.Length > 9 && !string.IsNullOrEmpty(p[9])) sql += ", Avatar=@av";
            
            sql += " WHERE Username=@u";

            var cmd = new SqliteCommand(sql, conn);
            cmd.Parameters.AddWithValue("@u", p[1]);
            cmd.Parameters.AddWithValue("@r", p[3]);
            if (!string.IsNullOrEmpty(p[2])) cmd.Parameters.AddWithValue("@p", p[2]);
            
            if (p.Length > 4 && !string.IsNullOrEmpty(p[4])) cmd.Parameters.AddWithValue("@fn", p[4]);
            if (p.Length > 5 && !string.IsNullOrEmpty(p[5])) cmd.Parameters.AddWithValue("@em", p[5]);
            if (p.Length > 6 && !string.IsNullOrEmpty(p[6])) cmd.Parameters.AddWithValue("@ph", p[6]);
            if (p.Length > 7 && !string.IsNullOrEmpty(p[7])) cmd.Parameters.AddWithValue("@tc", p[7]);
            if (p.Length > 8 && !string.IsNullOrEmpty(p[8])) cmd.Parameters.AddWithValue("@sj", p[8]);
            if (p.Length > 9 && !string.IsNullOrEmpty(p[9])) cmd.Parameters.AddWithValue("@av", p[9]);

            int rows = cmd.ExecuteNonQuery();
            Send(s, rows > 0 ? "UPDATE_USER_SUCCESS" : "UPDATE_USER_FAIL");
        } catch { Send(s, "UPDATE_USER_FAIL"); }
    }

    static void SearchUser(string[] p, NetworkStream s)
    {
        // SEARCH_USER | TYPE | VALUE
        string type = p[1];
        string val = p[2];
        
        using var conn = new SqliteConnection(connStr);
        conn.Open();
        
        string query = "SELECT Username, FullName, Role, Email, Phone, TeachingClass, Subject, Avatar FROM Users WHERE 1=1";
        if (type == "CLASS") query += " AND TeachingClass LIKE @v";
        else if (type == "ALL") query += " AND (FullName LIKE @v OR Username LIKE @v OR TeachingClass LIKE @v)";

        var cmd = new SqliteCommand(query, conn);
        cmd.Parameters.AddWithValue("@v", $"%{val}%");
        
        using var r = cmd.ExecuteReader();
        StringBuilder sb = new StringBuilder("LIST_USERS_RES|");
        while (r.Read())
        {
            sb.Append($"{r["Username"]}#{r["FullName"]}#{r["Role"]}#{r["Email"]}#{r["Phone"]}#{r["TeachingClass"]}#{r["Subject"]}#{r["Avatar"]};");
        }
        Send(s, sb.ToString());
    }
>>>>>>> a9f7c149d44583bd431f7952ef8661382757e01c
}
