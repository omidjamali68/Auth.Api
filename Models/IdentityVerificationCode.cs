namespace Auth.Api.Models
{
    public class IdentityVerificationCode
    {
        public long Id { get; set; }
        public uint VerificationCode { get; set; }
        public DateTime VerificationDate { get; set; }
        public string SMSResultDesc { get; set; }
        public string Email { get; set; }
        public string PhoneNumber { get; set; }
        public bool? IsUsed { get; set; }
        public DateTime? UsedAt { get; set; }
    }
}
