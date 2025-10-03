// ========================================
// Models/Session.cs
// ========================================
/*
 * Session.cs
 * Session model for web session management
 * Date: September 2025
 * Description: Used for managing user sessions and refresh tokens
 */

using MongoDB.Bson.Serialization.Attributes;
namespace EVChargingSystem.Api.Models
{

    public class Session : BaseEntity
    {
        [BsonElement("userId")]
        public string UserId { get; set; } = string.Empty;

        [BsonElement("userType")]
        public string UserType { get; set; } = string.Empty;

        [BsonElement("refreshToken")]
        public string RefreshToken { get; set; } = string.Empty;

        [BsonElement("refreshTokenExpiry")]
        public DateTime RefreshTokenExpiry { get; set; }

        [BsonElement("isActive")]
        public bool IsActive { get; set; } = true;
    }
}
