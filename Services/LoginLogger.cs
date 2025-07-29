using Auth.Api.Data;
using Auth.Api.Models;
using UAParser;

namespace Auth.Api.Services
{
    public class LoginLogger
    {
        private readonly AppDbContext _db;

        public LoginLogger(AppDbContext db)
        {
            _db = db;
        }

        public async Task LogLoginAsync(
            string phoneNumber, 
            string ip, 
            string userAgent,
            LoginStatus status, 
            LoginType type, 
            LoginSource source, 
            string note)
        {
            var parser = Parser.GetDefault();
            ClientInfo client = parser.Parse(userAgent);

            string deviceInfo = $"{client.Device.Family} / {client.OS.Family} {client.OS.Major}.{client.OS.Minor} / {client.UA.Family} {client.UA.Major}";

            var log = new UserLoginLog
            {
                UserId = phoneNumber,
                IpAddress = ip,
                UserAgent = userAgent,
                DeviceInfo = deviceInfo,
                LoginStatus = status,
                LoginSource = source,
                LoginType = type,
                Note = note,
                LoginTime = DateTime.Now
            };

            _db.UserLoginLogs.Add(log);
            await _db.SaveChangesAsync();
        }
    }

}
