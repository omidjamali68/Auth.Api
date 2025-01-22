using Auth.Api.Data;
using Auth.Api.Models;
using Auth.Api.Models.Dto;
using Auth.Api.Services.Contracts;
using Microsoft.AspNetCore.Identity;

namespace Auth.Api.Services
{
    public class AuthService : IAuthService
    {
        private readonly AppDbContext _db;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly IJwtTokenGenerator _jwtTokenGenerator;

        public AuthService(RoleManager<IdentityRole> roleManager, 
            AppDbContext db, 
            UserManager<ApplicationUser> userManager, 
            IJwtTokenGenerator jwtTokenGenerator)
        {
            _roleManager = roleManager;
            _db = db;
            _userManager = userManager;
            _jwtTokenGenerator = jwtTokenGenerator;
        }

        public async Task<bool> AssignRole(string userName, string roleName)
        {
            var user = _db.ApplicationUsers.FirstOrDefault(x => x.UserName.ToLower() == userName.ToLower());
            if (user == null)
                return false;

            if (!_roleManager.RoleExistsAsync(roleName).GetAwaiter().GetResult())
                await _roleManager.CreateAsync(new IdentityRole(roleName));

            await _userManager.AddToRoleAsync(user, roleName);

            return true;
        }

        public async Task<LoginResponseDto> Login(LoginRequestDto dto)
        {
            var user = _db.ApplicationUsers.FirstOrDefault(x => x.UserName.ToLower() == dto.UserName.ToLower());

            bool isValid = await _userManager.CheckPasswordAsync(user, dto.Password);
            if (isValid == false || user == null)
            {
                return new LoginResponseDto { User = null, Token = "" };
            }

            var userDto = new UserDto
            {
                Email = user.Email,
                Id = user.Id,
                Name = user.Name,
                PhoneNumber = user.PhoneNumber
            };

            var token = _jwtTokenGenerator.GenerateToken(user);

            return new LoginResponseDto { User = userDto, Token = token };
        }

        public async Task<string> Register(RegisterRequestDto dto)
        {
            var user = new ApplicationUser
            {
                UserName = dto.Email,
                Email = dto.Email,
                PhoneNumber = dto.PhoneNumber,
                NormalizedEmail = dto.Email.ToUpper(),
                Name = dto.Name
            };

            try
            {
                var result = await _userManager.CreateAsync(user, dto.Password);
                if (result.Succeeded)
                {
                    var userToReturn = _db.ApplicationUsers.FirstOrDefault(x => x.UserName == dto.Email);
                    return "";
                }
                else
                {
                    return result.Errors.FirstOrDefault().Description;
                }
            }
            catch(Exception ex) 
            {
                return ex.Message;
            }            
        }
    }
}
