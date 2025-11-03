namespace SmileCareAPI.Helpers
{
    public class OtpHelper
    {
        public static string GenerateOtp(int length = 6)
        {
            var random = new Random();
            var otp = string.Empty;

            for (int i = 0; i < length; i++)
            {
                otp += random.Next(0, 10).ToString();
            }

            return otp;
        }

        public static DateTime GetOtpExpirationTime(int expirationMinutes = 10)
        {
            return DateTime.UtcNow.AddMinutes(expirationMinutes);
        }

        public static bool IsOtpExpired(DateTime expiresAt)
        {
            return DateTime.UtcNow > expiresAt;
        }

        public static bool IsOtpValid(string inputOtp, string storedOtp, DateTime expiresAt, bool isUsed)
        {
            return !isUsed &&
                   !IsOtpExpired(expiresAt) &&
                   inputOtp.Equals(storedOtp, StringComparison.Ordinal);
        }
    }
}
