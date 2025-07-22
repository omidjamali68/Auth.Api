using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace Auth.Api.Models.Dto
{
    public class ConfirmVerificationCodeDto
    {
        [Required] 
        public string PhoneNumber { get; set; }
        [Required] 
        public uint VerificationCode { get; set; }
        [JsonIgnore]        
        internal string? UserIp { get; set; }
    }
}
