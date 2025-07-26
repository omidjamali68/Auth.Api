using System.ComponentModel.DataAnnotations;

namespace Auth.Api.Models.Dto
{
    public class SendVerificationCodeRequestDto
    {
        [Required]
        public string PhoneNumber { get; set; }
    }
}
