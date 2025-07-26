using Auth.Api.Models.Dto;

namespace Auth.Api.Services.Contracts
{
    public interface IAuthService
    {
        Task<ResponseDto> Register(RegisterRequestDto dto);
        Task<LoginResponseDto> LoginByPassword(LoginRequestDto dto);
        Task<ResponseDto> AssignRole(string userName,  string roleName);
        Task<ResponseDto> ConfirmVerificationCode(ConfirmVerificationCodeDto dto);
        Task<ResponseDto> SendVerificationCode(SendVerificationCodeRequestDto dto);
        Task<ResponseDto> LoginBySms(LoginBySmsRequestDto dto);
    }
}
