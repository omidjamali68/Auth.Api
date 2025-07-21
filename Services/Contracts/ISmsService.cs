using Auth.Api.Models.Dto;
using Auth.Api.Settings;

namespace Auth.Api.Services.Contracts
{
    public interface ISmsService
    {
        Task<ResponseDto> VerifySendAsync(string mobile, List<SmsParams> parameters);
    }
}
