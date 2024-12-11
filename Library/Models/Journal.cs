using System.ComponentModel.DataAnnotations;

namespace Library.Models
{
    public class Journal
    {
        public int Id { get; set; }
        public int BookId { get; set; }
        public int ClientId { get; set; }
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true)]
        public DateTime DateBeg { get; set; }
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true)]
        public DateTime DateEnd { get; set; }
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true)]
        public DateTime? DateRet { get; set; }

        public string? BookName { get; set; }
        public string? ClientLastName { get; set; }
        public string? ClientFirstName { get; set; }
        public string? ClientMiddleName { get; set; }

        public string ClientFullName
        {
            get
            {
                var parts = new List<string>();
                parts.Add(ClientLastName);
                if (!string.IsNullOrEmpty(ClientFirstName)) parts.Add(ClientFirstName);
                parts.Add(ClientMiddleName);
                return string.Join(" ", parts);
            }
        }

        public override string ToString()
        {
            return $"Id: {Id}, BookId: {BookId}, ClientId: {ClientId}, DateBeg: {DateBeg}, DateEnd: {DateEnd}, DateRet: {DateRet}, BookName: {BookName}, ClientFullName: {ClientFullName}";
        }
    }
}
