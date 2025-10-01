// ========================================
// Models/EVOwner.cs
// ========================================
// Models/BaseEntity.cs
// ========================================
/*
 * BaseEntity.cs
 * Base entity model for all database entities
 * Date: September 2025
 * Description: Contains common properties for all entities
 */

using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace EVChargingSystem.Api.Models
{
    public abstract class BaseEntity
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; } = ObjectId.GenerateNewId().ToString();

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    }
}
