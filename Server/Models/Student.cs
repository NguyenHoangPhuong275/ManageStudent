namespace Server.Models
{
    public class Student
    {
        public string StudentID { get; set; }
        public string FullName { get; set; }
        public string Class { get; set; }

        public override string ToString()
        {
            return $"{StudentID}#{FullName}#{Class}";
        }
    }
}
