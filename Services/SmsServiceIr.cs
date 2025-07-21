using Auth.Api.Models.Dto;
using Auth.Api.Services.Contracts;
using Auth.Api.Settings;
using IPE.SmsIrClient;
using IPE.SmsIrClient.Models.Requests;
using Microsoft.Extensions.Options;

namespace Auth.Api.Services
{
    public class SmsServiceIr : ISmsService
    {
        private readonly IrSmsSetting _smsSetting;
        private readonly ILogger<SmsServiceIr> _logger;

        public SmsServiceIr(IOptions<IrSmsSetting> smsSetting, ILogger<SmsServiceIr> logger)
        {
            _smsSetting = smsSetting.Value;
            _logger = logger;
        }

        public async Task<ResponseDto> VerifySendAsync(string mobile, List<SmsParams> parameters)
        {
            var result = new ResponseDto();
            try
            {
                string apiKey = _smsSetting.ApiKey;
                string number = _smsSetting.Number;
                int templateId = int.Parse(_smsSetting.LoginPaternCode);
                SmsIr smsIr = new SmsIr(apiKey);
                List<VerifySendParameter> pms = new List<VerifySendParameter>();
                parameters.ForEach(x => pms.Add(new(x.Key, x.Value)));
                var verificationSendResult = await smsIr.VerifySendAsync(mobile, templateId, pms.ToArray());
                if (verificationSendResult.Status == 1)
                {
                    result.IsSuccess = true;
                    result.Message = "پیامک ارسال شد .";
                }
                else
                {
                    result.IsSuccess = false;
                    result.Message = $"خطا در ارسال پیامک : {verificationSendResult.Message}";
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($" خطا در سرویس ارسال پیامک ایران sms : {ex.Message}" );
                result.IsSuccess = false;
                result.Message = ex.Message;
            }

            return result;
        }
    }
}
