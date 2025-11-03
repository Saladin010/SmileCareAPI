using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SmileCareAPI.DTOs;
using SmileCareAPI.Models;
using SmileCareAPI.Repositories.Interface;

namespace SmileCareAPI.Controllers
{
    [Route("api/users")]
    [ApiController]
    [Authorize]
    public class UsersController : ControllerBase
    {
        private readonly IUserRepository _userRepository;

        public UsersController(IUserRepository userRepository)
        {
            _userRepository = userRepository;
        }
        [HttpGet]
        [Authorize(Roles = "Admin,Receptionist")]
        public async Task<ActionResult<UsersListResponseDto>> GetAllUsers(
         [FromQuery] UserRole? role = null,
         [FromQuery] string? status = null,
         [FromQuery] string? search = null,
         [FromQuery] int page = 1,
         [FromQuery] int pageSize = 20)
        {
            try
            {
                bool? isActive = null;
                if (!string.IsNullOrWhiteSpace(status))
                {
                    if (status.ToLower() == "active")
                        isActive = true;
                    else if (status.ToLower() == "inactive")
                        isActive = false;
                }

                var result = await _userRepository.GetAllUsersAsync(role, isActive, search, page, pageSize);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while retrieving users", error = ex.Message });
            }
        }
        /// <summary>
        /// Get user details by ID
        /// </summary>
        /// <param name="id">User ID</param>
        /// <returns>User details</returns>
        [HttpGet("{id}")]
        [Authorize(Roles = "Admin,Receptionist,Doctor")]
        public async Task<ActionResult<UserDetailDto>> GetUserById(string id)
        {
            try
            {
                var user = await _userRepository.GetUserByIdAsync(id);
                if (user == null)
                {
                    return NotFound(new { message = "User not found" });
                }

                return Ok(user);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while retrieving user details", error = ex.Message });
            }
        }

        /// <summary>
        /// Create a new user
        /// </summary>
        /// <param name="dto">User creation data</param>
        /// <returns>Created user ID</returns>
        [HttpPost]
        [Authorize(Roles = "Admin,Receptionist")]
        public async Task<ActionResult> CreateUser([FromBody] CreateUserDto dto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var (success, userId, errorMessage) = await _userRepository.CreateUserAsync(dto);
                if (!success)
                {
                    return BadRequest(new { message = errorMessage });
                }

                return Ok(new { message = "User created successfully", userId });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while creating user", error = ex.Message });
            }
        }

        /// <summary>
        /// Update user information
        /// </summary>
        /// <param name="id">User ID</param>
        /// <param name="dto">Updated user data</param>
        /// <returns>Success message</returns>
        [HttpPut("{id}")]
        [Authorize(Roles = "Admin,Receptionist")]
        public async Task<ActionResult> UpdateUser(string id, [FromBody] UpdateUserDto dto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var (success, errorMessage) = await _userRepository.UpdateUserAsync(id, dto);
                if (!success)
                {
                    return BadRequest(new { message = errorMessage });
                }

                return Ok(new { message = "User updated successfully" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while updating user", error = ex.Message });
            }
        }

        /// <summary>
        /// Delete a user
        /// </summary>
        /// <param name="id">User ID</param>
        /// <returns>Success message</returns>
        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult> DeleteUser(string id)
        {
            try
            {
                var (success, errorMessage) = await _userRepository.DeleteUserAsync(id);
                if (!success)
                {
                    return BadRequest(new { message = errorMessage });
                }

                return Ok(new { message = "User deleted successfully" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while deleting user", error = ex.Message });
            }
        }

        /// <summary>
        /// Toggle user active status
        /// </summary>
        /// <param name="id">User ID</param>
        /// <returns>Updated status</returns>
        [HttpPatch("{id}/toggle-status")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<ToggleStatusResponseDto>> ToggleUserStatus(string id)
        {
            try
            {
                var (success, isActive, errorMessage) = await _userRepository.ToggleUserStatusAsync(id);
                if (!success)
                {
                    return BadRequest(new { message = errorMessage });
                }

                return Ok(new ToggleStatusResponseDto
                {
                    Message = "User status updated successfully",
                    IsActive = isActive
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while toggling user status", error = ex.Message });
            }
        }

        /// <summary>
        /// Upload profile picture for a user
        /// </summary>
        /// <param name="id">User ID</param>
        /// <param name="file">Image file</param>
        /// <returns>Profile picture URL</returns>
        [HttpPost("{id}/upload-profile-picture")]
        [Authorize(Roles = "Admin,Receptionist")]
        public async Task<ActionResult<UploadProfilePictureResponseDto>> UploadProfilePicture(string id, IFormFile file)
        {
            try
            {
                var (success, profilePictureUrl, errorMessage) = await _userRepository.UploadProfilePictureAsync(id, file);
                if (!success)
                {
                    return BadRequest(new { message = errorMessage });
                }

                return Ok(new UploadProfilePictureResponseDto
                {
                    Message = "Profile picture uploaded successfully",
                    ProfilePictureUrl = profilePictureUrl
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while uploading profile picture", error = ex.Message });
            }
        }

        /// <summary>
        /// Delete user's profile picture
        /// </summary>
        /// <param name="id">User ID</param>
        /// <returns>Success message</returns>
        [HttpDelete("{id}/delete-profile-picture")]
        [Authorize(Roles = "Admin,Receptionist")]
        public async Task<ActionResult> DeleteProfilePicture(string id)
        {
            try
            {
                var (success, errorMessage) = await _userRepository.DeleteProfilePictureAsync(id);
                if (!success)
                {
                    return BadRequest(new { message = errorMessage });
                }

                return Ok(new { message = "Profile picture deleted successfully" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while deleting profile picture", error = ex.Message });
            }
        }

        /// <summary>
        /// Get user statistics (Admin only)
        /// </summary>
        /// <returns>User statistics</returns>
        [HttpGet("statistics")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<UserStatisticsDto>> GetUserStatistics()
        {
            try
            {
                var statistics = await _userRepository.GetUserStatisticsAsync();
                return Ok(statistics);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while retrieving statistics", error = ex.Message });
            }
        }
    }
}
