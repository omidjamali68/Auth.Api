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

        public async Task<ResponseDto> AssignRole(string userName, string roleName)
        {
            var result = new ResponseDto();

            var user = _db.ApplicationUsers.FirstOrDefault(x => x.UserName.ToLower() == userName.ToLower());
            if (user == null)
            {
                result.CreateError("کاربری با این نام کاربری یافت نشد");
                return result;
            }

            if (!_roleManager.RoleExistsAsync(roleName).GetAwaiter().GetResult())
            {
                result.CreateError($"نقش {roleName} در سیستم وجود ندارد. لطفا با مدیر سیستم تماس بگیرید");
                return result;
            }
            //await _roleManager.CreateAsync(new IdentityRole(roleName));

            await _userManager.AddToRoleAsync(user, roleName);

            return result.Successful();
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
            result.Successful($"{user.FullName} خوش آمدید", new { Token = token });

            await loginLogger.LogLoginAsync(
                    user.UserName, dto.UserIp, dto.UserAgent, LoginStatus.Success, LoginSource.Web, $"{user.FullName} خوش آمدید");

            return result;
        }

        public async Task<ResponseDto> Register(RegisterRequestDto dto)
        {
            return await new RegisterUser(_db, _userManager).Register(dto);
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
                    user.UserName, dto.UserIp, dto.UserAgent, LoginStatus.Success, LoginSource.Web, $"{user.FullName} خوش آمدید");

            return result.Successful($"{user.FullName} خوش آمدید");
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

            var expireTime = DateTime.Now.Subtract(new TimeSpan(0, Setting.EXPIRE_VERIFICATIONCODE_TIME_MINUTE, 0));

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

            userVerification.SentFromIP = dto.UserIp; 

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

            if (await _db.VerificationCodes.CountAsync(
                x => x.PhoneNumber == dto.PhoneNumber) > Setting.MAX_VERIFICATION_CODE_SEND_PER_DAY)
            {
                return result.CreateError(
                    $"حداکثر تعداد ارسال کد در روز {Setting.MAX_VERIFICATION_CODE_SEND_PER_DAY} میباشد");
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
