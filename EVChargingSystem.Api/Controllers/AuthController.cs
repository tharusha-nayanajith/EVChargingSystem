// ========================================
// Controllers/AuthController.cs
// ========================================
/*
 * AuthController.cs
 * Authentication controller for user login
 * Date: September 2025
 * Description: Handles authentication endpoints for web users and EV owners
 */

using EVChargingSystem.Api.Models.DTOs;
using EVChargingSystem.Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
namespace EVChargingSystem.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;

        /// <summary>
        /// Constructor to initialize authentication controller
        /// </summary>
        public AuthController(IAuthService authService)
        {
            _authService = authService;
        }

        
        /// <summary>
        /// Authenticates EV owners using NIC and password
        /// </summary>
        [HttpPost("login/evowner")]
        public async Task<IActionResult> LoginEVOwner([FromBody] EVOwnerLoginRequestDTO loginRequest)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(new ApiResponseDTO<object>
                    {
                        Success = false,
                        Message = "Invalid request data",
                        Errors = ModelState.Values.SelectMany(v => v.Errors.Select(e => e.ErrorMessage)).ToList()
                    });
                }

                var result = await _authService.LoginEVOwnerAsync(loginRequest);

                if (result.Success)
                {
                    // Set new token as HTTP-only cookie
                    Response.Cookies.Append("accessToken", result.Data.AccessToken, new CookieOptions
                    {
                        HttpOnly = true,
                        Secure = true,
                        SameSite = SameSiteMode.None, // required for cross-site
                        Expires = result.Data.AccessTokenExpiresAt,
                        Path = "/api"
                    });

                    Response.Cookies.Append("refreshToken", result.Data.RefreshToken, new CookieOptions
                    {
                        HttpOnly = true,
                        Secure = true,
                        SameSite = SameSiteMode.None,
                        Expires = result.Data.RefreshTokenExpiresAt,
                        Path = "/api/auth/refresh"
                    });

                    // Remove token from response body
                    result.Data.RefreshToken = string.Empty;

                    return Ok(result);
                }

                return Unauthorized(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ApiResponseDTO<object>
                {
                    Success = false,
                    Message = "An internal error occurred"
                });
            }
        }

        /// <summary>
        /// Refreshes JWT access token using a valid refresh token
        /// </summary>
        [HttpPost("refresh")]
        public async Task<IActionResult> RefreshToken()
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(new ApiResponseDTO<object>
                    {
                        Success = false,
                        Message = "Invalid request data",
                        Errors = ModelState.Values.SelectMany(v => v.Errors.Select(e => e.ErrorMessage)).ToList()
                    });
                }

                // Get refresh token from cookie
                var refreshToken = Request.Cookies["refreshToken"];

                if (string.IsNullOrEmpty(refreshToken))
                {
                    return Unauthorized(new ApiResponseDTO<object>
                    {
                        Success = false,
                        Message = "No refresh token found"
                    });
                }

                var refreshTokenRequest = new RefreshTokenRequestDTO
                {
                    RefreshToken = refreshToken
                };

                var result = await _authService.RefreshTokenAsync(refreshTokenRequest);
                if (result.Success)
                {
                    // Set new token as HTTP-only cookie
                    Response.Cookies.Append("accessToken", result.Data.AccessToken, new CookieOptions
                    {
                        HttpOnly = true,
                        Secure = true,
                        SameSite = SameSiteMode.None, // required for cross-site
                        Expires = result.Data.AccessTokenExpiresAt,
                        Path = "/api"
                    });

                    Response.Cookies.Append("refreshToken", result.Data.RefreshToken, new CookieOptions
                    {
                        HttpOnly = true,
                        Secure = true,
                        SameSite = SameSiteMode.None,
                        Expires = result.Data.RefreshTokenExpiresAt,
                        Path = "/api/auth/refresh"
                    });

                    // Remove token from response body
                    result.Data.RefreshToken = string.Empty;

                    return Ok(result);
                }
                return Unauthorized(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ApiResponseDTO<object>
                {
                    Success = false,
                    Message = "An internal error occurred"
                });
            }
        }

        /// <summary>
        /// Logs out a user by invalidating their refresh token
        /// </summary>
        [HttpPost("logout")]
        [Authorize]
        public async Task<IActionResult> Logout()
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(new ApiResponseDTO<object>
                    {
                        Success = false,
                        Message = "Invalid request data",
                        Errors = ModelState.Values.SelectMany(v => v.Errors.Select(e => e.ErrorMessage)).ToList()
                    });
                }

                var accessToken = Request.Cookies["accessToken"];
                var refreshToken = Request.Cookies["refreshToken"];

                if (!string.IsNullOrEmpty(refreshToken))
                {
                    var logoutRequest = new LogoutRequestDTO
                    {
                        RefreshToken = refreshToken
                    };

                    await _authService.LogoutAsync(logoutRequest);
                }

                // Clear the token cookie
                Response.Cookies.Delete("accessToken", new CookieOptions
                {
                    Path = "/api",  // must match the Path of the cookie
                    HttpOnly = true,
                    Secure = true
                });
                Response.Cookies.Delete("refreshToken");

                return Ok(new ApiResponseDTO<object>
                {
                    Success = true,
                    Message = "Logged out successfully"
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ApiResponseDTO<object>
                {
                    Success = false,
                    Message = "An internal error occurred"
                });
            }
        }
    }
}