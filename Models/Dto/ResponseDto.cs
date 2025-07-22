

namespace Auth.Api.Models.Dto
{
    public class ResponseDto
    {
        public object? Data { get; set; }
        public bool IsSuccess { get; set; } = true;
        public string Message { get; set; } = "";

        internal void CreateError(string error)
        {
            IsSuccess = false;
            Message = error;
        }

        internal ResponseDto Successful()
        {
            IsSuccess = true;
            Message = "عملیات با موفقیت انجام شد";
            return this;
        }

        internal ResponseDto Successful(string message, object data = null)
        {
            IsSuccess = true;
            Message = message;
            Data = data;

            return this;
        }
    }
}
