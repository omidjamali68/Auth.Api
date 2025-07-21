using Auth.Api.Common;
using Auth.Api.Data;
using Auth.Api.Models;
using Auth.Api.Models.Dto;
using Auth.Api.Services.Contracts;
using Auth.Api.Settings;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Auth.Api.Services
{
    public class AuthService : IAuthService
    {
        private const int MAX_VERIFICATION_CODE_SEND_PER_DAY = 10;
        private readonly AppDbContext _db;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly IJwtTokenGenerator _jwtTokenGenerator;
        private readonly ISmsService _smsService;

        public AuthService(RoleManager<IdentityRole> roleManager,
            AppDbContext db,
            UserManager<ApplicationUser> userManager,
            IJwtTokenGenerator jwtTokenGenerator,
            ISmsService smsService)
        {
            _roleManager = roleManager;
            _db = db;
            _userManager = userManager;
            _jwtTokenGenerator = jwtTokenGenerator;
            _smsService = smsService;
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
            if (string.IsNullOrEmpty(dto.UserName))
                return "لطفا شماره همراه را وارد کنید";

            if (!dto.UserName.IsValidMobile())
                return "شماره همراه باید به صورت 09120000000 وارد شود";

            var userExist = _db.ApplicationUsers.Any(x => x.UserName.ToLower() == dto.UserName.ToLower());
            if (userExist)
                return "این کاربر قبلا ثبت نام کرده است";

            if (await _db.VerificationCodes.CountAsync(x => x.PhoneNumber == dto.PhoneNumber) > MAX_VERIFICATION_CODE_SEND_PER_DAY)
                return $"حداکثر تعداد ارسال کد در روز {MAX_VERIFICATION_CODE_SEND_PER_DAY} میباشد";            

            var user = new ApplicationUser
            {
                UserName = dto.UserName,
                Email = dto.Email,
                PhoneNumber = dto.PhoneNumber,
                NormalizedEmail = dto.Email?.ToUpper(),
                Name = dto.Name,
                CreatedAt = DateTime.Now
            };

            try
            {
                var result = await _userManager.CreateAsync(user, dto.Password);
                if (result.Succeeded)
                {
                    await SendVerificationCode(dto);

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

        private async Task SendVerificationCode(RegisterRequestDto dto)
        {
            var code = string.Empty.RandomInt(6);
            var smsResult = await _smsService.VerifySendAsync(dto.UserName, new List<SmsParams>() { new("cde", code) });
            await SaveApplicationUserVerificationCode(dto.UserName, uint.Parse(code), smsResult.Message);
        }

        private async Task SaveApplicationUserVerificationCode(string phoneNumber,
            uint verificationCode, string result)
        {
            _db.VerificationCodes.Add(new IdentityVerificationCode
            {
                SMSResultDesc = result,
                VerificationCode = verificationCode,
                VerificationDate = DateTime.Now,
                PhoneNumber = phoneNumber,
            });

            await _db.SaveChangesAsync();
        }
    }
}
