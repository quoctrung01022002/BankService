using System.ComponentModel.DataAnnotations;

namespace Demo.Dtos.Requests
{
    public class LoginRequest
    {
        [Required]
        [MaxLength(225)]
        public string Username { get; set; }
        [Required]
        [MaxLength(225)]
        public string Password { get; set; }
    }
}
