using SmileCareAPI.Repositories.Interface;
using System.Net.Mail;
using System.Net;

namespace SmileCareAPI.Repositories.Implementation
{
    public class EmailService : IEmailService
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<EmailService> _logger;

        public EmailService(IConfiguration configuration, ILogger<EmailService> logger)
        {
            _configuration = configuration;
            _logger = logger;
        }

        public async Task<bool> SendOtpEmailAsync(string toEmail, string userName, string otpCode)
        {
            try
            {
                var subject = "SmileCare - Verify Your Email";
                var body = $@"
                    <html>
                    <body style='font-family: Arial, sans-serif; background-color: #f4f4f4; padding: 20px;'>
                        <div style='max-width: 600px; margin: 0 auto; background-color: white; padding: 30px; border-radius: 10px; box-shadow: 0 2px 10px rgba(0,0,0,0.1);'>
                            <h2 style='color: #2c3e50; text-align: center;'>SmileCare Email Verification</h2>
                            <p style='color: #34495e; font-size: 16px;'>Hello <strong>{userName}</strong>,</p>
                            <p style='color: #34495e; font-size: 16px;'>Your OTP code for password reset is:</p>
                            <div style='background-color: #3498db; color: white; font-size: 32px; font-weight: bold; text-align: center; padding: 20px; border-radius: 5px; letter-spacing: 5px; margin: 20px 0;'>
                                {otpCode}
                            </div>
                            <p style='color: #e74c3c; font-size: 14px;'><strong>⚠️ This code will expire in 10 minutes.</strong></p>
                            <p style='color: #7f8c8d; font-size: 14px;'>If you didn't request this code, please ignore this email.</p>
                            <hr style='border: none; border-top: 1px solid #ecf0f1; margin: 20px 0;'>
                            <p style='color: #95a5a6; font-size: 12px; text-align: center;'>© 2025 SmileCare. All rights reserved.</p>
                        </div>
                    </body>
                    </html>";

                return await SendEmailAsync(toEmail, subject, body);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending OTP email to {Email}", toEmail);
                return false;
            }
        }

        public async Task<bool> SendWelcomeEmailAsync(string toEmail, string userName)
        {
            try
            {
                var subject = "Welcome to SmileCare!";
                var body = $@"
                    <html>
                    <body style='font-family: Arial, sans-serif; background-color: #f4f4f4; padding: 20px;'>
                        <div style='max-width: 600px; margin: 0 auto; background-color: white; padding: 30px; border-radius: 10px; box-shadow: 0 2px 10px rgba(0,0,0,0.1);'>
                            <h2 style='color: #27ae60; text-align: center;'>Welcome to SmileCare! 🎉</h2>
                            <p style='color: #34495e; font-size: 16px;'>Hello <strong>{userName}</strong>,</p>
                            <p style='color: #34495e; font-size: 16px;'>Thank you for registering with SmileCare. We're excited to have you on board!</p>
                            <p style='color: #34495e; font-size: 16px;'>You can now access all our services and book appointments with ease.</p>
                            <hr style='border: none; border-top: 1px solid #ecf0f1; margin: 20px 0;'>
                            <p style='color: #95a5a6; font-size: 12px; text-align: center;'>© 2025 SmileCare. All rights reserved.</p>
                        </div>
                    </body>
                    </html>";

                return await SendEmailAsync(toEmail, subject, body);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending welcome email to {Email}", toEmail);
                return false;
            }
        }

        public async Task<bool> SendPasswordResetConfirmationAsync(string toEmail, string userName)
        {
            try
            {
                var subject = "SmileCare - Password Reset Successful";
                var body = $@"
                    <html>
                    <body style='font-family: Arial, sans-serif; background-color: #f4f4f4; padding: 20px;'>
                        <div style='max-width: 600px; margin: 0 auto; background-color: white; padding: 30px; border-radius: 10px; box-shadow: 0 2px 10px rgba(0,0,0,0.1);'>
                            <h2 style='color: #27ae60; text-align: center;'>Password Reset Successful ✅</h2>
                            <p style='color: #34495e; font-size: 16px;'>Hello <strong>{userName}</strong>,</p>
                            <p style='color: #34495e; font-size: 16px;'>Your password has been successfully reset.</p>
                            <p style='color: #e74c3c; font-size: 14px;'><strong>⚠️ If you didn't make this change, please contact support immediately.</strong></p>
                            <hr style='border: none; border-top: 1px solid #ecf0f1; margin: 20px 0;'>
                            <p style='color: #95a5a6; font-size: 12px; text-align: center;'>© 2025 SmileCare. All rights reserved.</p>
                        </div>
                    </body>
                    </html>";

                return await SendEmailAsync(toEmail, subject, body);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending password reset confirmation to {Email}", toEmail);
                return false;
            }
        }

        private async Task<bool> SendEmailAsync(string toEmail, string subject, string body)
        {
            try
            {
                var smtpHost = _configuration["Email:SmtpHost"];
                var smtpPort = int.Parse(_configuration["Email:SmtpPort"]);
                var smtpUsername = _configuration["Email:SmtpUsername"];
                var smtpPassword = _configuration["Email:SmtpPassword"];
                var fromEmail = _configuration["Email:FromEmail"];
                var fromName = _configuration["Email:FromName"];

                using var client = new SmtpClient(smtpHost, smtpPort)
                {
                    EnableSsl = true,
                    Credentials = new NetworkCredential(smtpUsername, smtpPassword)
                };

                var message = new MailMessage
                {
                    From = new MailAddress(fromEmail, fromName),
                    Subject = subject,
                    Body = body,
                    IsBodyHtml = true
                };

                message.To.Add(toEmail);

                await client.SendMailAsync(message);
                _logger.LogInformation("Email sent successfully to {Email}", toEmail);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send email to {Email}", toEmail);
                return false;
            }
        }
    }
}
