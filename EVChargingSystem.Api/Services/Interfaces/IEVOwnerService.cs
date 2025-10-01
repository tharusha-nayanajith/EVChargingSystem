// ========================================
// Services/Interfaces/IEVOwnerService.cs
// ========================================
/*
 * IEVOwnerService.cs
 * EV Owner service interface
 * Date: September 2025
 * Description: Defines EV owner management service contract
 */

using EVChargingSystem.Api.Models;
using EVChargingSystem.Api.Models.DTOs;
namespace EVChargingSystem.Api.Services
{
    public interface IEVOwnerService
    {
        Task<ApiResponseDTO<EVOwner>> CreateEVOwnerAsync(CreateEVOwnerDTO createEVOwnerDto);
        Task<ApiResponseDTO<List<EVOwner>>> GetAllEVOwnersAsync();
        Task<ApiResponseDTO<EVOwner>> GetEVOwnerByNICAsync(string nic);
        Task<ApiResponseDTO<EVOwner>> GetEVOwnerByIdAsync(string id);
        Task<ApiResponseDTO<EVOwner>> UpdateEVOwnerAsync(string id, EVOwner evOwner);
        Task<ApiResponseDTO<bool>> DeleteEVOwnerAsync(string id);
        Task<ApiResponseDTO<bool>> ActivateDeactivateEVOwnerAsync(string id, bool isActive);
        Task<ApiResponseDTO<List<EVOwner>>> GetDeactivatedEVOwnersAsync();
        Task<ApiResponseDTO<bool>> ReactivateEVOwnerAsync(string id);
        //Task<ApiResponseDTO<DashboardStatsDTO>> GetEVOwnerDashboardStatsAsync(string evOwnerNIC);
    }
}
