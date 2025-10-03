// ========================================
// Services/AuthService.cs
// ========================================
/*
 * AuthService.cs
 * Authentication service implementation
 * Date: September 2025
 * Description: Handles user authentication and JWT token generation
 */

using EVChargingSystem.Api.Models;
using EVChargingSystem.Api.Models.DTOs;
using Microsoft.IdentityModel.Tokens;
using MongoDB.Driver;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace EVChargingSystem.Api.Services
{
    public class AuthService : IAuthService
    {
        private readonly IMongoCollection<EVOwner> _evOwners;
        private readonly IMongoCollection<Session> _sessions;
        private readonly IConfiguration _configuration;

        /// <summary>
        /// Constructor to initialize authentication service with database collections and configuration
        /// </summary>
        public AuthService(IMongoClient mongoClient, IConfiguration configuration)
        {
            var database = mongoClient.GetDatabase("EVChargingStationDB");
            _sessions = database.GetCollection<Session>("Sessions");
            _evOwners = database.GetCollection<EVOwner>("EVOwners");
            _configuration = configuration;
        }

        /// <summary>
        /// Authenticates EV owners using NIC and password
        /// </summary>
        public async Task<ApiResponseDTO<AuthResponseDTO>> LoginEVOwnerAsync(EVOwnerLoginRequestDTO loginRequest)
        {
            try
            {
                var evOwner = await _evOwners.Find(e => e.NIC == loginRequest.NIC && e.IsActive).FirstOrDefaultAsync();

                if (evOwner == null || !ValidatePassword(loginRequest.Password, evOwner.PasswordHash))
                {
                    return new ApiResponseDTO<AuthResponseDTO>
                    {
                        Success = false,
                        Message = "Invalid credentials"
                    };
                }

                // Update last login
                var update = Builders<EVOwner>.Update.Set(e => e.LastLogin, DateTime.UtcNow);
                await _evOwners.UpdateOneAsync(e => e.Id == evOwner.Id, update);

                var accessToken = GenerateJwtToken(evOwner.Id, "EVOwner", "EVOwner", evOwner.NIC);
                var refreshToken = Guid.NewGuid().ToString();
                var accessTokenExpiry = DateTime.UtcNow.AddMinutes(int.Parse(_configuration["JWT:ExpirationMinutes"]));
                var refreshTokenExpiry = DateTime.UtcNow.AddDays(int.Parse(_configuration["JWT:ExpirationDays"]));

                // Store session
                var session = new Session
                {
                    UserId = evOwner.Id,
                    UserType = "EVOwner",
                    RefreshToken = refreshToken,
                    RefreshTokenExpiry = refreshTokenExpiry,
                    IsActive = true
                };
                await _sessions.InsertOneAsync(session);

                return new ApiResponseDTO<AuthResponseDTO>
                {
                    Success = true,
                    Message = "Login successful",
                    Data = new AuthResponseDTO
                    {
                        AccessToken = accessToken,
                        RefreshToken = refreshToken,
                        UserType = "EVOwner",
                        UserId = evOwner.Id,
                        AccessTokenExpiresAt = accessTokenExpiry,
                        RefreshTokenExpiresAt = refreshTokenExpiry
                    }
                };
            }
            catch (Exception ex)
            {
                return new ApiResponseDTO<AuthResponseDTO>
                {
                    Success = false,
                    Message = "An error occurred during authentication"
                };
            }
        }

        /// <summary>
        /// Refreshes JWT token using a valid refresh token
        /// </summary>
        public async Task<ApiResponseDTO<AuthResponseDTO>> RefreshTokenAsync(RefreshTokenRequestDTO refreshTokenRequest)
        {
            try
            {
                var session = await _sessions.Find(s => s.RefreshToken == refreshTokenRequest.RefreshToken &&
                                                      s.IsActive &&
                                                      s.RefreshTokenExpiry > DateTime.UtcNow).FirstOrDefaultAsync();

                if (session == null)
                {
                    return new ApiResponseDTO<AuthResponseDTO>
                    {
                        Success = false,
                        Message = "Invalid or expired refresh token"
                    };
                }

                string role;
                string newAccessToken = string.Empty;
                if (session.UserType == "EVOwner")
                {
                    var evOwner = await _evOwners.Find(e => e.Id == session.UserId).FirstOrDefaultAsync();
                    if (evOwner == null || !evOwner.IsActive)
                    {
                        await _sessions.UpdateOneAsync(s => s.Id == session.Id,
                            Builders<Session>.Update.Set(s => s.IsActive, false));
                        return new ApiResponseDTO<AuthResponseDTO>
                        {
                            Success = false,
                            Message = "EV Owner not found or inactive"
                        };
                    }
                    role = "EVOwner";
                    newAccessToken = GenerateJwtToken(session.UserId, session.UserType, role, evOwner.NIC);
                }

                var newRefreshToken = Guid.NewGuid().ToString();
                var accessTokenExpiry = DateTime.UtcNow.AddMinutes(int.Parse(_configuration["JWT:ExpirationMinutes"]));
                var refreshTokenExpiry = DateTime.UtcNow.AddDays(int.Parse(_configuration["JWT:ExpirationDays"]));

                // Update session with new refresh token
                await _sessions.UpdateOneAsync(s => s.Id == session.Id,
                    Builders<Session>.Update
                        .Set(s => s.RefreshToken, newRefreshToken)
                        .Set(s => s.RefreshTokenExpiry, refreshTokenExpiry)
                        .Set(s => s.UpdatedAt, DateTime.UtcNow));

                return new ApiResponseDTO<AuthResponseDTO>
                {
                    Success = true,
                    Message = "Token refreshed successfully",
                    Data = new AuthResponseDTO
                    {
                        AccessToken = newAccessToken,
                        RefreshToken = newRefreshToken,
                        UserType = session.UserType,
                        UserId = session.UserId,
                        AccessTokenExpiresAt = accessTokenExpiry,
                        RefreshTokenExpiresAt = refreshTokenExpiry
                    }
                };
            }
            catch (Exception ex)
            {
                return new ApiResponseDTO<AuthResponseDTO>
                {
                    Success = false,
                    Message = "An error occurred during token refresh"
                };
            }
        }

        /// <summary>
        /// Logs out user by invalidating the refresh token
        /// </summary>
        public async Task<ApiResponseDTO<object>> LogoutAsync(LogoutRequestDTO logoutRequest)
        {
            try
            {
                var result = await _sessions.UpdateOneAsync(
                    s => s.RefreshToken == logoutRequest.RefreshToken && s.IsActive,
                    Builders<Session>.Update.Set(s => s.IsActive, false));

                if (result.MatchedCount == 0)
                {
                    return new ApiResponseDTO<object>
                    {
                        Success = false,
                        Message = "Invalid refresh token"
                    };
                }

                return new ApiResponseDTO<object>
                {
                    Success = true,
                    Message = "Logged out successfully"
                };
            }
            catch (Exception ex)
            {
                return new ApiResponseDTO<object>
                {
                    Success = false,
                    Message = "An error occurred during logout"
                };
            }
        }

        /// <summary>
        /// Generates JWT token for authenticated users
        /// </summary>
        public string GenerateJwtToken(string userId, string userType, string role, string? nic = null)
        {
            var jwtSettings = _configuration.GetSection("JWT");
            var secretKey = Encoding.UTF8.GetBytes(jwtSettings["SecretKey"]);

            var claims = new List<Claim>
            {
                new(ClaimTypes.NameIdentifier, userId),
                new(ClaimTypes.Role, role),
                new("UserType", userType),
                new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new(JwtRegisteredClaimNames.Iat, DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString(), ClaimValueTypes.Integer64)
            };

            // Add NIC claim only if provided
            if (!string.IsNullOrEmpty(nic))
            {
                claims.Add(new Claim("NIC", nic));
            }

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Expires = DateTime.UtcNow.AddMinutes(int.Parse(jwtSettings["ExpirationMinutes"])),
                Issuer = jwtSettings["Issuer"],
                Audience = jwtSettings["Audience"],
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(secretKey), SecurityAlgorithms.HmacSha256Signature)
            };

            var tokenHandler = new JwtSecurityTokenHandler();
            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }

        /// <summary>
        /// Validates password against hashed password
        /// </summary>
        public bool ValidatePassword(string password, string hashedPassword)
        {
            return BCrypt.Net.BCrypt.Verify(password, hashedPassword);
        }

        /// <summary>
        /// Hashes password using BCrypt
        /// </summary>
        public string HashPassword(string password)
        {
            return BCrypt.Net.BCrypt.HashPassword(password);
        }
    }
}