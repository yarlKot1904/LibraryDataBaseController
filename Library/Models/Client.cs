using System.ComponentModel.DataAnnotations.Schema;

namespace Library.Models
{
    public class Client
    {
        public int Id { get; set; }
        public required string FirstName { get; set; }
        public required string LastName { get; set; }
        public string? MiddleName { get; set; }
        public required string PassportSeries { get; set; }
        public required string PassportNumber { get; set; }
    }
}
