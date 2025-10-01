// ========================================
// Models/DTOs/AuthDTOs.cs
// ========================================
/*
 * AuthDTOs.cs
 * Data Transfer Objects for authentication
 * Date: September 2025
 * Description: Contains DTOs for login, registration, and authentication responses
 */

using System.ComponentModel.DataAnnotations;
namespace EVChargingSystem.Api.Models.DTOs
{
    public class EVOwnerLoginRequestDTO
    {
        [Required]
        public string NIC { get; set; } = string.Empty;

        [Required]
        public string Password { get; set; } = string.Empty;
    }
    public class AuthResponseDTO
    {
        public string AccessToken { get; set; } = string.Empty;
        public string RefreshToken { get; set; } = string.Empty;
        public string UserType { get; set; } = string.Empty;
        public string UserId { get; set; } = string.Empty;
        public DateTime AccessTokenExpiresAt { get; set; }
        public DateTime RefreshTokenExpiresAt { get; set; }
    }

    public class RefreshTokenRequestDTO
    {
        [Required]
        public string RefreshToken { get; set; } = string.Empty;
    }

    public class LogoutRequestDTO
    {
        [Required]
        public string RefreshToken { get; set; } = string.Empty;
    }
    public class CreateEVOwnerDTO
    {
        [Required]
        public string NIC { get; set; } = string.Empty;

        [Required]
        public string FirstName { get; set; } = string.Empty;

        [Required]
        public string LastName { get; set; } = string.Empty;

        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Required]
        public string PhoneNumber { get; set; } = string.Empty;

        [Required]
        [MinLength(6)]
        public string Password { get; set; } = string.Empty;

        public List<VehicleDetail> VehicleDetails { get; set; } = new();
    }
}
