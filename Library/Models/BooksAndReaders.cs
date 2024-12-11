namespace Library.Models
{
    public class BooksAndReadersRecord
    {
        public string BookName { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string MiddleName { get; set; }
        public DateTime DataBeg { get; set; }
        public DateTime DateEnd { get; set; }
        public DateTime? DateRet { get; set; }
    }
}
