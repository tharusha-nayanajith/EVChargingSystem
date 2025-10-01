// ========================================
// Models/EVOwner.cs
// ========================================
/*
 * EVOwner.cs
 * EV Owner model for electric vehicle owners
 * Date: September 2025
 * Description: Represents EV owners who can make bookings
 */

using MongoDB.Bson.Serialization.Attributes;
using System.ComponentModel.DataAnnotations;


namespace EVChargingSystem.Api.Models
{
    public class EVOwner : BaseEntity
    {
        [BsonElement("nic")]
        [Required]
        public string NIC { get; set; } = string.Empty;

        [BsonElement("firstName")]
        public string FirstName { get; set; } = string.Empty;

        [BsonElement("lastName")]
        public string LastName { get; set; } = string.Empty;

        [BsonElement("email")]
        public string Email { get; set; } = string.Empty;

        [BsonElement("phoneNumber")]
        public string PhoneNumber { get; set; } = string.Empty;

        [BsonElement("passwordHash")]
        public string PasswordHash { get; set; } = string.Empty;

        [BsonElement("isActive")]
        public bool IsActive { get; set; } = true;

        [BsonElement("vehicleDetails")]
        public List<VehicleDetail> VehicleDetails { get; set; } = new();

        [BsonElement("lastLogin")]
        public DateTime? LastLogin { get; set; }
    }

    public class VehicleDetail
    {
        public string Make { get; set; } = string.Empty;
        public string Model { get; set; } = string.Empty;
        public string LicensePlate { get; set; } = string.Empty;
        public int Year { get; set; }
    }
}
