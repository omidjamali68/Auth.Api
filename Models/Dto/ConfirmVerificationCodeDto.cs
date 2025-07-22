using System.ComponentModel.DataAnnotations;

namespace Auth.Api.Models.Dto
{
    public class ConfirmVerificationCodeDto
    {
        [Required] public string PhoneNumber { get; set; }

        [Required] public uint VerificationCode { get; set; }

    }
}
