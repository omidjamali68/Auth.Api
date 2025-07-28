using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace Auth.Api.Models.Dto
{
    public class RegisterRequestDto
    {
        [Required]
        public string UserName { get; set; }
        public string Email { get; set; }
        [Required]
        public string FullName { get; set; }
        [MaxLength(10)]
        public string? NationalCode { get; set; }
        [Required]
        public string Password { get; set; }
        [Required]
        public uint VerificationCode { get; set; }
        [JsonIgnore]
        internal string? UserIp { get; set; }
    }
}