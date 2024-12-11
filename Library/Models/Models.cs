namespace Library.Models
{
    public class User
    {
        public string Login { get; set; }
        public string PasswordHash { get; set; }
        public string Role { get; set; }
    }
}
