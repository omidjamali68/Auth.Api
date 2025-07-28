using Auth.Api.Common;
using Auth.Api.Data;
using Auth.Api.Models;
using Auth.Api.Models.Dto;
using Auth.Api.Settings;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Auth.Api.Services
{
    public class RegisterUser
    {
        private readonly AppDbContext _db;
        private readonly UserManager<ApplicationUser> _userManager;

        public RegisterUser(AppDbContext db, UserManager<ApplicationUser> userManager)
        {
            _db = db;
            _userManager = userManager;
        }

        internal async Task<ResponseDto> Register(RegisterRequestDto dto)
        {
            var result = new ResponseDto();

            if (string.IsNullOrEmpty(dto.UserName))
            {
                return result.CreateError("لطفا شماره همراه را وارد کنید");
            }

            if (!dto.UserName.IsValidMobile())
            {
                return result.CreateError("شماره همراه باید به صورت 09120000000 وارد شود");
            }

            var userExist = _db.ApplicationUsers.Any(x => x.UserName.ToLower() == dto.UserName.ToLower());
            if (userExist)
            {
                return result.CreateError("این کاربر قبلا ثبت نام کرده است");
            }

            var confirmCodeResult = await ConfirmRegisteringVerificationCode(new ConfirmVerificationCodeDto
            {
                PhoneNumber = dto.UserName,
                UserIp = dto.UserIp,
                VerificationCode = dto.VerificationCode,
            });

            if (confirmCodeResult.IsSuccess == false)
            {
                return confirmCodeResult;
            }

            var user = new ApplicationUser
            {
                UserName = dto.UserName,
                Email = dto.Email,
                PhoneNumber = dto.UserName,
                NormalizedEmail = dto.Email?.ToUpper(),
                FullName = dto.FullName,
                NationalCode = dto.NationalCode,
                CreatedAt = DateTime.Now,
                PhoneNumberConfirmed = true
            };

            try
            {
                var createUserResult = await _userManager.CreateAsync(user, dto.Password);
                if (createUserResult.Succeeded)
                {
                    return result.Successful();
                }
                else
                {
                    return result.CreateError(
                        createUserResult.Errors.FirstOrDefault()?.Description);
                }
            }
            catch (Exception ex)
            {
                return result.CreateError(ex.Message);
            }
        }

        private async Task<ResponseDto> ConfirmRegisteringVerificationCode(ConfirmVerificationCodeDto dto)
        {
            var result = new ResponseDto();

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

            userVerification.IsUsed = true;
            userVerification.UsedAt = DateTime.Now;

            await _db.SaveChangesAsync();

            return result.Successful();
        }
    }
}
