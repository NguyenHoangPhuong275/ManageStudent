using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
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

            // Chỉ nạp Admin và Giáo viên mẫu nếu bảng trống
            var checkUserCmd = new SqliteCommand("SELECT COUNT(*) FROM Users", conn);
            if ((long)checkUserCmd.ExecuteScalar() == 0)
            {
                Log("Bảng Users trống, nạp tài khoản mặc định...", LogType.INFO);
                string initUsers = @"
                    INSERT INTO Users VALUES ('admin@admin.edu.vn', 'admin123', 'ADMIN', 'Quản Trị Viên');
                    INSERT INTO Users VALUES ('gv01@school.edu.vn', '123', 'USER', 'Nguyễn Văn A');
                ";
                new SqliteCommand(initUsers, conn).ExecuteNonQuery();
            }

            // 2. Tạo bảng Students
            string tblStudents = @"
                CREATE TABLE IF NOT EXISTS Students (
                    StudentID TEXT PRIMARY KEY,
                    FullName TEXT,
                    Class TEXT
                );
            ";
            new SqliteCommand(tblStudents, conn).ExecuteNonQuery();

            // Chỉ nạp SV mẫu nếu bảng trống
            var checkStudentCmd = new SqliteCommand("SELECT COUNT(*) FROM Students", conn);
            if ((long)checkStudentCmd.ExecuteScalar() == 0)
            {
                Log("Bảng Students trống, nạp sinh viên mẫu...", LogType.INFO);
                string initStudents = "INSERT INTO Students VALUES ('SV001', 'Nguyen Van A', 'CNTT')";
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
        var cmd = new SqliteCommand("SELECT Role, FullName FROM Users WHERE Username=@u AND Password=@p", conn);
        cmd.Parameters.AddWithValue("@u", p[1]);
        cmd.Parameters.AddWithValue("@p", p[2]);
        using var r = cmd.ExecuteReader();
        if (r.Read()) 
        {
            string role = r["Role"].ToString();
            string fn = r["FullName"].ToString();
            if (string.IsNullOrEmpty(fn)) fn = p[1]; // Nếu chưa có tên thì lấy tạm Username
            
            // Trả về: LOGIN_SUCCESS | Role | FullName
            Send(s, $"LOGIN_SUCCESS|{role}|{fn}");
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
            var cmd = new SqliteCommand("INSERT INTO Students VALUES (@id,@name,@class)", conn);
            cmd.Parameters.AddWithValue("@id", p[1]);
            cmd.Parameters.AddWithValue("@name", p[2]);
            cmd.Parameters.AddWithValue("@class", p[3]);
            cmd.ExecuteNonQuery();
            Send(s, "ADD_SUCCESS");
        } catch { Send(s, "EXISTS"); }
    }

    static void UpdateStudent(string[] p, NetworkStream s)
    {
        using var conn = new SqliteConnection(connStr);
        conn.Open();
        var cmd = new SqliteCommand("UPDATE Students SET FullName=@name, Class=@class WHERE StudentID=@id", conn);
        cmd.Parameters.AddWithValue("@id", p[1]);
        cmd.Parameters.AddWithValue("@name", p[2]);
        cmd.Parameters.AddWithValue("@class", p[3]);
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
        if (type == "ID") query = "SELECT StudentID, FullName, Class FROM Students WHERE StudentID=@v";
        else if (type == "CLASS") query = "SELECT StudentID, FullName, Class FROM Students WHERE Class LIKE @v";
        else if (type == "ALL") query = "SELECT StudentID, FullName, Class FROM Students WHERE StudentID LIKE @v OR FullName LIKE @v OR Class LIKE @v";
            
        var cmd = new SqliteCommand(query, conn);
        cmd.Parameters.AddWithValue("@v", (type == "ID") ? val : $"%{val}%");
        
        using var r = cmd.ExecuteReader();
        
        StringBuilder sb = new StringBuilder("LIST_RES|");
        int count = 0;
        while (r.Read())
        {
            count++;
            sb.Append($"{r["StudentID"]}#{r["FullName"]}#{r["Class"]};");
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
        var cmd = new SqliteCommand("SELECT StudentID, FullName, Class FROM Students ORDER BY FullName", conn);
        using var r = cmd.ExecuteReader();
        
        StringBuilder sb = new StringBuilder("LIST_RES|");
        while (r.Read())
        {
            sb.Append($"{r["StudentID"]}#{r["FullName"]}#{r["Class"]};");
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
        byte[] b = new byte[8192];
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
            var cmd = new SqliteCommand("INSERT INTO Users VALUES (@u, @p, @r, @fn)", conn);
            cmd.Parameters.AddWithValue("@u", p[1]);
            cmd.Parameters.AddWithValue("@p", p[2]);
            cmd.Parameters.AddWithValue("@r", p[3]);
            cmd.Parameters.AddWithValue("@fn", p.Length > 4 ? p[4] : "Giáo Viên"); // Nhận thêm Họ Tên
            cmd.ExecuteNonQuery();
            Send(s, "CREATE_USER_SUCCESS");
        } catch { Send(s, "CREATE_USER_FAIL"); }
    }

    static void ListUsers(NetworkStream s)
    {
        using var conn = new SqliteConnection(connStr);
        conn.Open();
        var cmd = new SqliteCommand("SELECT Username, FullName, Role FROM Users", conn);
        using var r = cmd.ExecuteReader();
        
        StringBuilder sb = new StringBuilder("LIST_USERS_RES|");
        while (r.Read())
        {
            // Gửi về: Username # FullName # Role
            sb.Append($"{r["Username"]}#{r["FullName"]}#{r["Role"]}#***;");
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
        // FORMAT: UPDATE_USER | Username | NewPass | NewRole | [NewFullName]
        try {
            using var conn = new SqliteConnection(connStr);
            conn.Open();
            
            string sql = "UPDATE Users SET Role=@r";
            if (!string.IsNullOrEmpty(p[2])) sql += ", Password=@p"; // Chỉ update Pass nếu không rỗng
            if (p.Length > 4) sql += ", FullName=@fn";
            
            sql += " WHERE Username=@u";

            var cmd = new SqliteCommand(sql, conn);
            cmd.Parameters.AddWithValue("@u", p[1]);
            cmd.Parameters.AddWithValue("@r", p[3]);
            if (!string.IsNullOrEmpty(p[2])) cmd.Parameters.AddWithValue("@p", p[2]);
            if (p.Length > 4) cmd.Parameters.AddWithValue("@fn", p[4]);

            int rows = cmd.ExecuteNonQuery();
            Send(s, rows > 0 ? "UPDATE_USER_SUCCESS" : "UPDATE_USER_FAIL");
        } catch { Send(s, "UPDATE_USER_FAIL"); }
    }
}
