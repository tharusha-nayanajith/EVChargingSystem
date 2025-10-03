// ========================================
// Services/Interfaces/IAuthService.cs
// ========================================
/*
 * IAuthService.cs
 * Authentication service interface
 * Date: September 2025
 * Description: Defines authentication service contract
 */

using EVChargingSystem.Api.Models.DTOs;
namespace EVChargingSystem.Api.Services
{
    public interface IAuthService
    {
        //Task<ApiResponseDTO<AuthResponseDTO>> LoginUserAsync(LoginRequestDTO loginRequest);
        Task<ApiResponseDTO<AuthResponseDTO>> LoginEVOwnerAsync(EVOwnerLoginRequestDTO loginRequest);
        Task<ApiResponseDTO<AuthResponseDTO>> RefreshTokenAsync(RefreshTokenRequestDTO refreshTokenRequest);
        Task<ApiResponseDTO<object>> LogoutAsync(LogoutRequestDTO logoutRequest);
        string GenerateJwtToken(string userId, string userType, string role, string? nic = null);
        bool ValidatePassword(string password, string hashedPassword);
        string HashPassword(string password);
    }
}
