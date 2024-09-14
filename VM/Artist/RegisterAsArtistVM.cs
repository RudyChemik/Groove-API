using System.ComponentModel.DataAnnotations;

namespace Groove.VM.Artist
{
    public class RegisterAsArtistVM
    {
        [Required]
        public string Name { get; set; }

        [Required]
        [EmailAddress]
        public string Email { get; set; }

        [Required]
        [DataType(DataType.Password)]
        public string Password { get; set; }

        [DataType(DataType.Password)]
        [Compare("Password", ErrorMessage = "Hasła nie są identyczne")]
        public string ComfirmedPassword { get; set; }

    }
}
