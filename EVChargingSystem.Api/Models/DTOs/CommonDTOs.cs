// ========================================
// Models/DTOs/CommonDTOs.cs
// ========================================
/*
 * CommonDTOs.cs
 * Common Data Transfer Objects
 * Date: September 2025
 * Description: Contains common DTOs used across the application
 */

using System.ComponentModel.DataAnnotations;

namespace EVChargingSystem.Api.Models.DTOs
{
   
    public class ApiResponseDTO<T>
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public T? Data { get; set; }
        public List<string> Errors { get; set; } = new();
    }

    public class NearbyStationsRequestDTO
    {
        [Required]
        [Range(-90, 90)]
        public double Latitude { get; set; }

        [Required]
        [Range(-180, 180)]
        public double Longitude { get; set; }

        [Range(1, 50)]
        public double RadiusKm { get; set; } = 10;
    }

    public class DashboardStatsDTO
    {
        public int PendingReservations { get; set; }
        public int ApprovedFutureReservations { get; set; }
        public int TotalActiveStations { get; set; }
        //public List<ChargingStation> NearbyStations { get; set; } = new();
    }
}
