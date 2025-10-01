// ========================================
// Models/DatabaseSettings.cs
// ========================================
/*
 * DatabaseSettings.cs
 * Database configuration model
 * Date: September 2025
 * Description: Contains MongoDB connection settings
 */
namespace EVChargingSystem.Api.Models
{
    public class DatabaseSettings
    {
        public string MongoDB { get; set; } = string.Empty;
    }
}
