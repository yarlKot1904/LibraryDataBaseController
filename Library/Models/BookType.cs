using System.ComponentModel.DataAnnotations.Schema;

namespace Library.Models
{
    public class BookType
    {
        public int Id { get; set; }
        public required string Type { get; set; }
        public decimal Fine { get; set; }
        public int MaxDays { get; set; }
    }
}
