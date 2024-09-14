using System.ComponentModel.DataAnnotations;

namespace Groove.Models
{
    public class UserInformation
    {
        [Key]
        public int Id { get; set; }

        public string? Street { get; set; }
        public string? City { get; set; }
        public string? PostalCode { get; set; }
        public string? Country { get; set; }

        public string UserId { get; set; }
        public AppUser User { get; set; }
    }
}
