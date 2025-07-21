using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;

namespace Auth.Api.Common
{
    public static class ExpressionHelper
    {
        public static bool IsValidMobile(this string Phone)
        {
            try
            {
                const string pattern = @"^09[0-9]{9}$";
                Regex reg = new Regex(pattern);
                return reg.IsMatch(Phone);
            }
            catch
            {
                return false;
            }
        }

        public static string RandomInt(this string i, int maxSize)
        {
            char[] chars;
            { chars = "1234567890".ToCharArray(); }
            {
                byte[] data = new byte[1];
                using (RNGCryptoServiceProvider crypto = new RNGCryptoServiceProvider())
                {
                    crypto.GetNonZeroBytes(data);
                    data = new byte[maxSize];
                    crypto.GetNonZeroBytes(data);
                }
                StringBuilder result = new StringBuilder(maxSize);
                foreach (byte b in data)
                {
                    result.Append(chars[b % chars.Length]);
                }
                return result.ToString();
            }
        }
    }
}

    
    
