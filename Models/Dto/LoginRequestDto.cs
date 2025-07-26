using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace Auth.Api.Models.Dto
{
    public class LoginRequestDto
    {
        [Required]
        public string UserName { get; set; }
        [Required]
        public string Password { get; set; }
        [JsonIgnore]
        internal string? UserIp { get; set; }
        [JsonIgnore]
        internal string? UserAgent { get; set; }
    }
}
