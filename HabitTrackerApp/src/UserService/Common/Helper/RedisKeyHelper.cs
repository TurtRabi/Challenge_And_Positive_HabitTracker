namespace UserService.Common.Helper
{
    public static class RedisKeyHelper
    {
        public static string GetOtpEmailKey(string email) => $"userservice:otp:email:{email}";
        public static string GetOtpPhoneKey(string phone) => $"userservice:otp:phone:{phone}";
    }
}
