using System.ComponentModel.DataAnnotations;

namespace Groove.VM
{
    public class RegisterArtistVM
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
        public string Img { get; set; }
        public string Desc { get; set; }
    }
}
