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

        public AuthApiController(IAuthService authService)
        {
            _authService = authService;
            _response = new();
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

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequestDto dto)
        {
            var loginResponse = await _authService.Login(dto);
            if(loginResponse.User == null)
            {
                _response.IsSuccess = false;
                _response.Message = "Username or password is incorrect";
                return BadRequest(_response);
            }
            _response.Data = loginResponse;
            return Ok(_response);
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
