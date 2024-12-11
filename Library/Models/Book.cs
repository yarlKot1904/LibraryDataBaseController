using System.ComponentModel.DataAnnotations.Schema;

namespace Library.Models
{
    public class Book
    {
        public int Id { get; set; }
        public required string Name { get; set; }
        public int Amount { get; set; }
        public int TypeId { get; set; }
        public BookType? Type { get; set; }
    }
}
