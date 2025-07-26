using Auth.Api.Models.Dto;
using Auth.Api.Services.Contracts;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Auth.Api.Controllers
{
    [Route("api/auth")]
    [ApiController]
    public class AuthApiController : ControllerBase
    {
        private readonly IAuthService _authService;
        private ResponseDto _response;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public AuthApiController(IAuthService authService, IHttpContextAccessor httpContextAccessor)
        {
            _authService = authService;
            _response = new();
            _httpContextAccessor = httpContextAccessor;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterRequestDto dto)
        {
            var errorMessage = await _authService.Register(dto);

            if (!string.IsNullOrEmpty(errorMessage)) 
            {
                _response.IsSuccess = false;
                _response.Message = errorMessage;
                return BadRequest(_response);
            }

            return Ok(_response);
        }

        [HttpPut("verify-code")]
        public async Task<ResponseDto> ConfirmVerificationCode(ConfirmVerificationCodeDto dto)
        {
            dto.UserIp = _httpContextAccessor.HttpContext?.Connection?.RemoteIpAddress?.MapToIPv4().ToString();
            return await _authService.ConfirmVerificationCode(dto);
        }

        [HttpPost("send-verification-code")]
        public async Task<ResponseDto> SendVerificationCode(SendVerificationCodeRequestDto dto)
        {
            return await _authService.SendVerificationCode(dto);
        }

        [HttpPut("login-by-sms")]
        public async Task<ResponseDto> LoginBySms(LoginBySmsRequestDto dto)
        {
            dto.UserIp = _httpContextAccessor.HttpContext?.Connection?.RemoteIpAddress?.MapToIPv4().ToString();
            dto.UserAgent = Request.Headers["User-Agent"].ToString();
            return await _authService.LoginBySms(dto);
        }

        [HttpPost("login-by-password")]
        public async Task<IActionResult> LoginByPassword([FromBody] LoginRequestDto dto)
        {
            dto.UserIp = _httpContextAccessor.HttpContext?.Connection?.RemoteIpAddress?.MapToIPv4().ToString();
            dto.UserAgent = Request.Headers["User-Agent"].ToString();
            var loginResponse = await _authService.LoginByPassword(dto);            
            
            return Ok(loginResponse);
        }

        [HttpPost("assign-role")]
        public async Task<IActionResult> AssignRole([FromBody] AssignRoleDto dto)
        {
            var assignRoleResponse = await _authService.AssignRole(dto.UserName, dto.RoleName);
            if (!assignRoleResponse)
            {
                _response.IsSuccess = false;
                _response.Message = "Error encountered";
                return BadRequest(_response);
            }
            
            return Ok(_response);
        }
    }
}
