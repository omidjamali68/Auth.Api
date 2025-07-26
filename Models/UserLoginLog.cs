using System.ComponentModel.DataAnnotations;

namespace Auth.Api.Models
{
    public class UserLoginLog
    {
        public int LogId { get; set; }

        public string UserId { get; set; }

        public DateTime LoginTime { get; set; } = DateTime.UtcNow;

        public DateTime? LogoutTime { get; set; }

        [MaxLength(45)]
        public string? IpAddress { get; set; }

        [MaxLength(512)]
        public string? UserAgent { get; set; }

        [MaxLength(255)]
        public string? DeviceInfo { get; set; }

        public LoginStatus LoginStatus { get; set; }  

        public LoginSource? LoginSource { get; set; }  

        [MaxLength(255)]
        public string? Note { get; set; }
    }

    public enum LoginStatus : short
    {
        Success = 1,
        Failed = 2,
        LockedOut = 3
    }

    public enum LoginSource : short
    {
        Web = 1,
        Api = 2,
        AdminPanel = 3
    }

}
