using Auth.Api.Models.Dto;

namespace Auth.Api.Services.Contracts
{
    public interface IAuthService
    {
        Task<string> Register(RegisterRequestDto dto);
        Task<LoginResponseDto> Login(LoginRequestDto dto);
        Task<bool> AssignRole(string userName,  string roleName);
        Task<ResponseDto> ConfirmVerificationCode(ConfirmVerificationCodeDto dto);
    }
}
