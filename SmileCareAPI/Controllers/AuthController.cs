using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using SmileCareAPI.DTOs;
using SmileCareAPI.Helpers;
using SmileCareAPI.Models;
using SmileCareAPI.Repositories.Interface;

namespace SmileCareAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly UserManager<User> _userManager;
        private readonly IAuthRepository _authRepository;
        private readonly JwtHelper _jwtHelper;
        private readonly IEmailService _emailService;
        private readonly ILogger<AuthController> _logger;
        private readonly IConfiguration _configuration;

        public AuthController(
            UserManager<User> userManager,
            IAuthRepository authRepository,
            JwtHelper jwtHelper,
            IEmailService emailService,
            ILogger<AuthController> logger,
            IConfiguration configuration)
        {
            _userManager = userManager;
            _authRepository = authRepository;
            _jwtHelper = jwtHelper;
            _emailService = emailService;
            _logger = logger;
            _configuration = configuration;
        }

        // 1. POST /api/auth/register
        [HttpPost("register")]
        public async Task<ActionResult<RegisterResponseDto>> Register([FromBody] RegisterDto registerDto)
        {
            try
            {
                // Check if username exists
                var existingUsername = await _authRepository.GetUserByUsernameAsync(registerDto.Username);
                if (existingUsername != null)
                {
                    return BadRequest(new { message = "Username already exists" });
                }

                // Check if email exists
                var existingEmail = await _authRepository.GetUserByEmailAsync(registerDto.Email);
                if (existingEmail != null)
                {
                    return BadRequest(new { message = "Email already exists" });
                }

                // Create new user
                var user = new User
                {
                    FirstName = registerDto.FirstName,
                    LastName = registerDto.LastName,
                    UserName = registerDto.Username,
                    Email = registerDto.Email,
                    PhoneNumber = registerDto.PhoneNumber,
                    Gender = registerDto.Gender,
                    DateOfBirth = registerDto.DateOfBirth,
                    City = registerDto.City,
                    DetailedAddress = registerDto.DetailedAddress,
                    Role = UserRole.Patient,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow
                };

                var result = await _userManager.CreateAsync(user, registerDto.Password);

                if (!result.Succeeded)
                {
                    return BadRequest(new { message = "User creation failed", errors = result.Errors });
                }

                await _userManager.AddToRoleAsync(user, "Patient");


                // Generate tokens
                var token = _jwtHelper.GenerateAccessToken(user);
                var refreshToken = _jwtHelper.GenerateRefreshToken();

                // Save refresh token
                var refreshTokenEntity = new RefreshToken
                {
                    UserId = user.Id,
                    Token = refreshToken,
                    CreatedAt = DateTime.UtcNow,
                    ExpiresAt = DateTime.UtcNow.AddDays(Convert.ToDouble(_configuration["Jwt:RefreshTokenExpirationDays"])),
                    DeviceName = Request.Headers["User-Agent"].ToString(),
                    IpAddress = HttpContext.Connection.RemoteIpAddress?.ToString()
                };

                await _authRepository.AddRefreshTokenAsync(refreshTokenEntity);

                // Log activity
                await _authRepository.AddActivityLogAsync(new ActivityLog
                {
                    UserId = user.Id,
                    Action = "User Registration",
                    Description = $"New user registered: {user.UserName}",
                    IpAddress = HttpContext.Connection.RemoteIpAddress?.ToString(),
                    DeviceName = Request.Headers["User-Agent"].ToString(),
                    IsSuccess = true,
                    CreatedAt = DateTime.UtcNow
                });

                await _authRepository.SaveChangesAsync();

                // Send welcome email
                await _emailService.SendWelcomeEmailAsync(user.Email, user.FirstName);

                return Ok(new RegisterResponseDto
                {
                    Message = "User registered successfully",
                    UserId = user.Id,
                    Token = token,
                    RefreshToken = refreshToken
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during user registration");
                return StatusCode(500, new { message = "An error occurred during registration" });
            }
        }

        // 2. POST /api/auth/login
        [HttpPost("login")]
        public async Task<ActionResult<AuthResponseDto>> Login([FromBody] LoginDto loginDto)
        {
            try
            {
                var user = await _authRepository.GetUserByEmailOrUsernameAsync(loginDto.EmailOrUsername);

                if (user == null)
                {
                    await LogFailedLogin(loginDto.EmailOrUsername, "User not found");
                    return Unauthorized(new { message = "Invalid credentials" });
                }

                if (!user.IsActive)
                {
                    return Unauthorized(new { message = "Account is deactivated" });
                }

                var isPasswordValid = await _userManager.CheckPasswordAsync(user, loginDto.Password);

                if (!isPasswordValid)
                {
                    await LogFailedLogin(user.Id, "Invalid password");
                    return Unauthorized(new { message = "Invalid credentials" });
                }

                // Generate tokens
                var token = _jwtHelper.GenerateAccessToken(user);
                var refreshToken = _jwtHelper.GenerateRefreshToken();

                // Save refresh token
                var refreshTokenEntity = new RefreshToken
                {
                    UserId = user.Id,
                    Token = refreshToken,
                    CreatedAt = DateTime.UtcNow,
                    ExpiresAt = DateTime.UtcNow.AddDays(Convert.ToDouble(_configuration["Jwt:RefreshTokenExpirationDays"])),
                    DeviceName = Request.Headers["User-Agent"].ToString(),
                    IpAddress = HttpContext.Connection.RemoteIpAddress?.ToString()
                };

                await _authRepository.AddRefreshTokenAsync(refreshTokenEntity);

                // Update last login
                user.LastLoginAt = DateTime.UtcNow;
                await _userManager.UpdateAsync(user);

                // Log activity
                await _authRepository.AddActivityLogAsync(new ActivityLog
                {
                    UserId = user.Id,
                    Action = "User Login",
                    Description = "User logged in successfully",
                    IpAddress = HttpContext.Connection.RemoteIpAddress?.ToString(),
                    DeviceName = Request.Headers["User-Agent"].ToString(),
                    IsSuccess = true,
                    CreatedAt = DateTime.UtcNow
                });

                await _authRepository.SaveChangesAsync();

                return Ok(new AuthResponseDto
                {
                    Token = token,
                    RefreshToken = refreshToken,
                    User = new UserDto
                    {
                        Id = user.Id,
                        FirstName = user.FirstName,
                        LastName = user.LastName,
                        Email = user.Email,
                        Username = user.UserName,
                        Role = user.Role.ToString(),
                        ProfilePicture = user.ProfilePicture
                    }
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during login");
                return StatusCode(500, new { message = "An error occurred during login" });
            }
        }

        // 3. POST /api/auth/refresh-token
        [HttpPost("refresh-token")]
        public async Task<ActionResult<AuthResponseDto>> RefreshToken([FromBody] RefreshTokenDto refreshTokenDto)
        {
            try
            {
                var refreshToken = await _authRepository.GetRefreshTokenAsync(refreshTokenDto.RefreshToken);

                if (refreshToken == null || refreshToken.IsRevoked || refreshToken.ExpiresAt < DateTime.UtcNow)
                {
                    return Unauthorized(new { message = "Invalid or expired refresh token" });
                }

                var user = await _authRepository.GetUserByIdAsync(refreshToken.UserId);

                if (user == null || !user.IsActive)
                {
                    return Unauthorized(new { message = "User not found or inactive" });
                }

                // Generate new tokens
                var newToken = _jwtHelper.GenerateAccessToken(user);
                var newRefreshToken = _jwtHelper.GenerateRefreshToken();

                // Revoke old refresh token
                await _authRepository.RevokeRefreshTokenAsync(refreshToken);

                // Save new refresh token
                var newRefreshTokenEntity = new RefreshToken
                {
                    UserId = user.Id,
                    Token = newRefreshToken,
                    CreatedAt = DateTime.UtcNow,
                    ExpiresAt = DateTime.UtcNow.AddDays(Convert.ToDouble(_configuration["Jwt:RefreshTokenExpirationDays"])),
                    DeviceName = refreshToken.DeviceName,
                    IpAddress = HttpContext.Connection.RemoteIpAddress?.ToString()
                };

                await _authRepository.AddRefreshTokenAsync(newRefreshTokenEntity);
                await _authRepository.SaveChangesAsync();

                return Ok(new AuthResponseDto
                {
                    Token = newToken,
                    RefreshToken = newRefreshToken,
                    User = new UserDto
                    {
                        Id = user.Id,
                        FirstName = user.FirstName,
                        LastName = user.LastName,
                        Email = user.Email,
                        Username = user.UserName,
                        Role = user.Role.ToString(),
                        ProfilePicture = user.ProfilePicture
                    }
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error refreshing token");
                return StatusCode(500, new { message = "An error occurred while refreshing token" });
            }
        }

        // 4. POST /api/auth/logout
        [Authorize]
        [HttpPost("logout")]
        public async Task<ActionResult<MessageResponseDto>> Logout()
        {
            try
            {
                var userId = User.FindFirstValue("UserId");
                var authHeader = Request.Headers["Authorization"].ToString();
                var token = authHeader.Replace("Bearer ", "");

                // Get refresh token from user's active sessions
                var refreshTokens = await _authRepository.GetUserActiveRefreshTokensAsync(userId);

                if (refreshTokens.Any())
                {
                    // Revoke the current session's refresh token
                    var currentRefreshToken = refreshTokens.FirstOrDefault();
                    if (currentRefreshToken != null)
                    {
                        await _authRepository.RevokeRefreshTokenAsync(currentRefreshToken);
                    }
                }

                // Log activity
                await _authRepository.AddActivityLogAsync(new ActivityLog
                {
                    UserId = userId,
                    Action = "User Logout",
                    Description = "User logged out",
                    IpAddress = HttpContext.Connection.RemoteIpAddress?.ToString(),
                    DeviceName = Request.Headers["User-Agent"].ToString(),
                    IsSuccess = true,
                    CreatedAt = DateTime.UtcNow
                });

                await _authRepository.SaveChangesAsync();

                return Ok(new MessageResponseDto { Message = "Logged out successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during logout");
                return StatusCode(500, new { message = "An error occurred during logout" });
            }
        }

        // 5. POST /api/auth/forgot-password
        [HttpPost("forgot-password")]
        public async Task<ActionResult<MessageResponseDto>> ForgotPassword([FromBody] ForgotPasswordDto forgotPasswordDto)
        {
            try
            {
                var user = await _authRepository.GetUserByEmailAsync(forgotPasswordDto.Email);

                if (user == null)
                {
                    // Don't reveal that user doesn't exist
                    return Ok(new MessageResponseDto { Message = "If the email exists, an OTP has been sent" });
                }

                // Invalidate old OTPs
                await _authRepository.InvalidateOldOtpsAsync(user.Id);

                // Generate new OTP
                var otpCode = OtpHelper.GenerateOtp();
                var otpEntity = new OtpCode
                {
                    UserId = user.Id,
                    Code = otpCode,
                    CreatedAt = DateTime.UtcNow,
                    ExpiresAt = OtpHelper.GetOtpExpirationTime(10)
                };

                await _authRepository.AddOtpAsync(otpEntity);

                // Log activity
                await _authRepository.AddActivityLogAsync(new ActivityLog
                {
                    UserId = user.Id,
                    Action = "Password Reset Request",
                    Description = "OTP requested for password reset",
                    IpAddress = HttpContext.Connection.RemoteIpAddress?.ToString(),
                    DeviceName = Request.Headers["User-Agent"].ToString(),
                    IsSuccess = true,
                    CreatedAt = DateTime.UtcNow
                });

                await _authRepository.SaveChangesAsync();

                // Send OTP email
                await _emailService.SendOtpEmailAsync(user.Email, user.FirstName, otpCode);

                return Ok(new MessageResponseDto { Message = "OTP sent to email" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending OTP");
                return StatusCode(500, new { message = "An error occurred while sending OTP" });
            }
        }

        // 6. POST /api/auth/verify-otp
        [HttpPost("verify-otp")]
        public async Task<ActionResult<OtpResponseDto>> VerifyOtp([FromBody] VerifyOtpDto verifyOtpDto)
        {
            try
            {
                var user = await _authRepository.GetUserByEmailAsync(verifyOtpDto.Email);

                if (user == null)
                {
                    return BadRequest(new { message = "Invalid email" });
                }

                var otp = await _authRepository.GetOtpByCodeAsync(user.Id, verifyOtpDto.OtpCode);

                if (otp == null)
                {
                    return BadRequest(new { message = "Invalid OTP" });
                }

                if (!OtpHelper.IsOtpValid(verifyOtpDto.OtpCode, otp.Code, otp.ExpiresAt, otp.IsUsed))
                {
                    return BadRequest(new { message = "Invalid or expired OTP" });
                }

                // Mark OTP as used
                otp.IsUsed = true;
                otp.UsedAt = DateTime.UtcNow;
                await _authRepository.UpdateOtpAsync(otp);

                // Generate reset token (using JWT)
                var resetToken = _jwtHelper.GenerateAccessToken(user);

                // Log activity
                await _authRepository.AddActivityLogAsync(new ActivityLog
                {
                    UserId = user.Id,
                    Action = "OTP Verification",
                    Description = "OTP verified successfully",
                    IpAddress = HttpContext.Connection.RemoteIpAddress?.ToString(),
                    DeviceName = Request.Headers["User-Agent"].ToString(),
                    IsSuccess = true,
                    CreatedAt = DateTime.UtcNow
                });

                await _authRepository.SaveChangesAsync();

                return Ok(new OtpResponseDto
                {
                    Message = "OTP verified",
                    ResetToken = resetToken
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error verifying OTP");
                return StatusCode(500, new { message = "An error occurred while verifying OTP" });
            }
        }

        // 7. POST /api/auth/reset-password
        [HttpPost("reset-password")]
        public async Task<ActionResult<MessageResponseDto>> ResetPassword([FromBody] ResetPasswordDto resetPasswordDto)
        {
            try
            {
                var userId = _jwtHelper.GetUserIdFromToken(resetPasswordDto.ResetToken);

                if (string.IsNullOrEmpty(userId))
                {
                    return BadRequest(new { message = "Invalid reset token" });
                }

                var user = await _authRepository.GetUserByIdAsync(userId);

                if (user == null)
                {
                    return BadRequest(new { message = "User not found" });
                }

                // Reset password
                var token = await _userManager.GeneratePasswordResetTokenAsync(user);
                var result = await _userManager.ResetPasswordAsync(user, token, resetPasswordDto.NewPassword);

                if (!result.Succeeded)
                {
                    return BadRequest(new { message = "Password reset failed", errors = result.Errors });
                }

                // Revoke all refresh tokens
                await _authRepository.RevokeAllUserRefreshTokensAsync(user.Id);

                // Log activity
                await _authRepository.AddActivityLogAsync(new ActivityLog
                {
                    UserId = user.Id,
                    Action = "Password Reset",
                    Description = "Password reset successfully",
                    IpAddress = HttpContext.Connection.RemoteIpAddress?.ToString(),
                    DeviceName = Request.Headers["User-Agent"].ToString(),
                    IsSuccess = true,
                    CreatedAt = DateTime.UtcNow
                });

                await _authRepository.SaveChangesAsync();

                // Send confirmation email
                await _emailService.SendPasswordResetConfirmationAsync(user.Email, user.FirstName);

                return Ok(new MessageResponseDto { Message = "Password reset successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error resetting password");
                return StatusCode(500, new { message = "An error occurred while resetting password" });
            }
        }

        // 8. POST /api/auth/change-password
        [Authorize]
        [HttpPost("change-password")]
        public async Task<ActionResult<MessageResponseDto>> ChangePassword([FromBody] ChangePasswordDto changePasswordDto)
        {
            try
            {
                var userId = User.FindFirstValue("UserId");
                var user = await _authRepository.GetUserByIdAsync(userId);

                if (user == null)
                {
                    return NotFound(new { message = "User not found" });
                }

                var result = await _userManager.ChangePasswordAsync(user, changePasswordDto.CurrentPassword, changePasswordDto.NewPassword);

                if (!result.Succeeded)
                {
                    return BadRequest(new { message = "Password change failed", errors = result.Errors });
                }

                // Log activity
                await _authRepository.AddActivityLogAsync(new ActivityLog
                {
                    UserId = user.Id,
                    Action = "Password Change",
                    Description = "Password changed successfully",
                    IpAddress = HttpContext.Connection.RemoteIpAddress?.ToString(),
                    DeviceName = Request.Headers["User-Agent"].ToString(),
                    IsSuccess = true,
                    CreatedAt = DateTime.UtcNow
                });

                await _authRepository.SaveChangesAsync();

                return Ok(new MessageResponseDto { Message = "Password changed successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error changing password");
                return StatusCode(500, new { message = "An error occurred while changing password" });
            }
        }

        // 9. GET /api/auth/active-sessions
        [Authorize]
        [HttpGet("active-sessions")]
        public async Task<ActionResult<List<SessionDto>>> GetActiveSessions()
        {
            try
            {
                var userId = User.FindFirstValue("UserId");
                var refreshTokens = await _authRepository.GetUserActiveRefreshTokensAsync(userId);

                var authHeader = Request.Headers["Authorization"].ToString();
                var currentToken = authHeader.Replace("Bearer ", "");

                var sessions = refreshTokens.Select(rt => new SessionDto
                {
                    Id = rt.Id,
                    DeviceName = rt.DeviceName ?? "Unknown Device",
                    IpAddress = rt.IpAddress ?? "Unknown IP",
                    CreatedAt = rt.CreatedAt,
                    IsCurrentSession = false // We can't easily determine this without storing more info
                }).ToList();

                return Ok(sessions);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting active sessions");
                return StatusCode(500, new { message = "An error occurred while getting sessions" });
            }
        }

        // 10. DELETE /api/auth/revoke-session/{id}
        [Authorize]
        [HttpDelete("revoke-session/{id}")]
        public async Task<ActionResult<MessageResponseDto>> RevokeSession(int id)
        {
            try
            {
                var userId = User.FindFirstValue("UserId");
                var refreshToken = await _authRepository.GetRefreshTokenByIdAsync(id);

                if (refreshToken == null || refreshToken.UserId != userId)
                {
                    return NotFound(new { message = "Session not found" });
                }

                await _authRepository.RevokeRefreshTokenAsync(refreshToken);

                // Log activity
                await _authRepository.AddActivityLogAsync(new ActivityLog
                {
                    UserId = userId,
                    Action = "Session Revoked",
                    Description = $"Session {id} revoked",
                    IpAddress = HttpContext.Connection.RemoteIpAddress?.ToString(),
                    DeviceName = Request.Headers["User-Agent"].ToString(),
                    IsSuccess = true,
                    CreatedAt = DateTime.UtcNow
                });

                await _authRepository.SaveChangesAsync();

                return Ok(new MessageResponseDto { Message = "Session revoked" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error revoking session");
                return StatusCode(500, new { message = "An error occurred while revoking session" });
            }
        }

        // 11. DELETE /api/auth/revoke-all-sessions
        [Authorize]
        [HttpDelete("revoke-all-sessions")]
        public async Task<ActionResult<MessageResponseDto>> RevokeAllSessions()
        {
            try
            {
                var userId = User.FindFirstValue("UserId");
                var authHeader = Request.Headers["Authorization"].ToString();

                // Get current refresh token (if exists) to keep it active
                var currentRefreshTokens = await _authRepository.GetUserActiveRefreshTokensAsync(userId);
                var currentToken = currentRefreshTokens.FirstOrDefault()?.Token;

                await _authRepository.RevokeAllUserRefreshTokensAsync(userId, currentToken);

                // Log activity
                await _authRepository.AddActivityLogAsync(new ActivityLog
                {
                    UserId = userId,
                    Action = "All Sessions Revoked",
                    Description = "All other sessions revoked",
                    IpAddress = HttpContext.Connection.RemoteIpAddress?.ToString(),
                    DeviceName = Request.Headers["User-Agent"].ToString(),
                    IsSuccess = true,
                    CreatedAt = DateTime.UtcNow
                });

                await _authRepository.SaveChangesAsync();

                return Ok(new MessageResponseDto { Message = "All sessions revoked" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error revoking all sessions");
                return StatusCode(500, new { message = "An error occurred while revoking all sessions" });
            }
        }

        // Helper Methods
        private async Task LogFailedLogin(string identifier, string reason)
        {
            await _authRepository.AddActivityLogAsync(new ActivityLog
            {
                UserId = identifier,
                Action = "Login Failed",
                Description = reason,
                IpAddress = HttpContext.Connection.RemoteIpAddress?.ToString(),
                DeviceName = Request.Headers["User-Agent"].ToString(),
                IsSuccess = false,
                ErrorMessage = reason,
                CreatedAt = DateTime.UtcNow
            });
            await _authRepository.SaveChangesAsync();
        }
    }
}
