using System.ComponentModel.DataAnnotations;

namespace Auth.Api.Models.Dto
{
    public class RegisterRequestDto
    {
        [Required]
        public string UserName { get; set; }
        public string Email { get; set; }
        [Required]
        public string Name { get; set; }
        [Required]
        public string Password { get; set; }
    }
}