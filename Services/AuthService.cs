using Auth.Api.Common;
using Auth.Api.Data;
using Auth.Api.Models;
using Auth.Api.Models.Dto;
using Auth.Api.Services.Contracts;
using Auth.Api.Settings;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System;

namespace Auth.Api.Services
{
    public class AuthService : IAuthService
    {
        private const int MAX_VERIFICATION_CODE_SEND_PER_DAY = 10;
        private const int EXPIRE_VERIFICATIONCODE_TIME_MINUTE = 5;

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

        public async Task<LoginResponseDto> LoginByPassword(LoginRequestDto dto)
        {
            var result = new LoginResponseDto();
            var loginLogger = new LoginLogger(_db);

            var user = _db.ApplicationUsers.FirstOrDefault(x => x.UserName.ToLower() == dto.UserName.ToLower());
            if (user == null)
            {
                result.CreateError("کاربری با این نام کاربر یافت نشد");
                await loginLogger.LogLoginAsync(
                    dto.UserName, dto.UserIp, dto.UserAgent, LoginStatus.Failed, LoginSource.Web, "کاربری با این نام کاربر یافت نشد");
                return result;
            }

            if (user.PhoneNumberConfirmed == false)
            {
                result.CreateError("کاربر احراز هویت نشده است");
                await loginLogger.LogLoginAsync(
                    user.UserName, dto.UserIp, dto.UserAgent, LoginStatus.Failed, LoginSource.Web, "کاربر احراز هویت نشده است");
                return result;
            }

            bool isValid = await _userManager.CheckPasswordAsync(user, dto.Password);
            if (isValid == false)
            {
                result.CreateError("کلمه عبور صحیح نمیباشد");
                await loginLogger.LogLoginAsync(
                    user.UserName, dto.UserIp, dto.UserAgent, LoginStatus.Failed, LoginSource.Web, "کلمه عبور صحیح نمیباشد");
                return result;
            }            

            var token = _jwtTokenGenerator.GenerateToken(user);
            result.Successful($"{user.Name} خوش آمدید", new { Token = token });

            await loginLogger.LogLoginAsync(
                    user.UserName, dto.UserIp, dto.UserAgent, LoginStatus.Success, LoginSource.Web, $"{user.Name} خوش آمدید");

            return result;
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
                    await SendVerificationCode(
                        new SendVerificationCodeRequestDto { PhoneNumber = dto.UserName});

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

        public async Task<ResponseDto> LoginBySms(LoginBySmsRequestDto dto)
        {
            var result = new ResponseDto();
            var loginLogger = new LoginLogger(_db);

            var user = _db.ApplicationUsers.FirstOrDefault(x => x.UserName.ToLower() == dto.PhoneNumber.ToLower());
            if (user == null)
            {
                result.CreateError("کاربری با این نام کاربر یافت نشد");
                await loginLogger.LogLoginAsync(
                    dto.PhoneNumber, dto.UserIp, dto.UserAgent, LoginStatus.Failed, LoginSource.Web, "کاربری با این نام کاربر یافت نشد");
                return result;
            }

            var confirmCodeResult = await ConfirmVerificationCode(new ConfirmVerificationCodeDto
            {
                PhoneNumber = dto.PhoneNumber,
                UserIp = dto.UserIp,
                VerificationCode = dto.VerificationCode,
            });

            if (confirmCodeResult.IsSuccess == false)
            {
                await loginLogger.LogLoginAsync(
                    user.UserName, dto.UserIp, dto.UserAgent, LoginStatus.Failed, LoginSource.Web,confirmCodeResult.Message);                
                return confirmCodeResult;
            }

            await loginLogger.LogLoginAsync(
                    user.UserName, dto.UserIp, dto.UserAgent, LoginStatus.Success, LoginSource.Web, $"{user.Name} خوش آمدید");

            return result.Successful($"{user.Name} خوش آمدید");
        }

        public async Task<ResponseDto> ConfirmVerificationCode(ConfirmVerificationCodeDto dto)
        {
            var result = new ResponseDto();

            var applicationUser = await _userManager.FindByNameAsync(dto.PhoneNumber);

            if(applicationUser == null)
            {
                result.CreateError("کاربر قبلا ثبت نام نکرده است");
                return result;
            }

            var userVerification = await _db.VerificationCodes
                .Where(_ => _.PhoneNumber == dto.PhoneNumber && _.IsUsed == false)
                .OrderByDescending(_ => _.VerificationDate)
                .FirstOrDefaultAsync();
            
            if (userVerification == null)
            {
                result.CreateError("کد تایید برای این کاربر یافت نشد");
                return result;
            }

            var expireTime = DateTime.Now.Subtract(new TimeSpan(0, EXPIRE_VERIFICATIONCODE_TIME_MINUTE, 0));

            if (userVerification.VerificationDate < expireTime)
            {
                result.CreateError("کد تایید منقضی شده است");
                return result;
            }

            if (userVerification.TryCount >= 5)
            {
                result.CreateError("تعداد تلاش‌های ناموفق بیش از حد مجاز است. لطفاً کد جدید دریافت کنید.");
                return result;
            }

            userVerification.SentFromIP = dto.UserIp; // یا از HttpContext

            if (userVerification.VerificationCode != dto.VerificationCode)
            {
                userVerification.TryCount += 1;
                await _db.SaveChangesAsync();
                result.CreateError("کد تایید وارد شده صحیح نمی‌باشد");
                return result;
            }

            applicationUser.PhoneNumberConfirmed = true;

            userVerification.IsUsed = true;
            userVerification.UsedAt = DateTime.Now;

            await _db.SaveChangesAsync();

            return result.Successful();
        }

        public async Task<ResponseDto> SendVerificationCode(SendVerificationCodeRequestDto dto)
        {
            var result = new ResponseDto();
            var userExist = _db.ApplicationUsers.Any(x => x.UserName.ToLower() == dto.PhoneNumber.ToLower());
            if (!userExist)
            {
                result.CreateError("کاربری با این شماره تماس یافت نشد");
                return result;
            }

            var code = string.Empty.RandomInt(6);
            var smsResult = await _smsService.VerifySendAsync(dto.PhoneNumber, new List<SmsParams>() { new("cde", code) });
            await SaveApplicationUserVerificationCode(dto.PhoneNumber, uint.Parse(code), smsResult.Message);

            return result.Successful();
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
                IsUsed = false,
            });

            await _db.SaveChangesAsync();
        }
        
    }
}
