namespace Server.Models
{
    public class User
    {
        public string Username { get; set; }
        public string Password { get; set; }
        public string Role { get; set; }
        public string FullName { get; set; }

        public override string ToString()
        {
            return $"{Username}#{FullName}#{Role}";
        }
    }
}
