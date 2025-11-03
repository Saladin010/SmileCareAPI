namespace SmileCareAPI.Repositories.Interface
{

    public interface IEmailService
    {
        Task<bool> SendOtpEmailAsync(string toEmail, string userName, string otpCode);
        Task<bool> SendWelcomeEmailAsync(string toEmail, string userName);
        Task<bool> SendPasswordResetConfirmationAsync(string toEmail, string userName);
    }
}
