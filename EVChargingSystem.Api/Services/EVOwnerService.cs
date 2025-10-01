// ========================================
// Services/EVOwnerService.cs
// ========================================
/*
 * EVOwnerService.cs
 * EV Owner service implementation
 * Date: September 2025
 * Description: Handles EV owner management and related operations
 */

using EVChargingSystem.Api.Models;
using EVChargingSystem.Api.Models.DTOs;
using MongoDB.Driver;
namespace EVChargingSystem.Api.Services
{
    public class EVOwnerService : IEVOwnerService
    {
        private readonly IMongoCollection<EVOwner> _evOwners;
        //private readonly IMongoCollection<Booking> _bookings;
        //private readonly IMongoCollection<ChargingStation> _chargingStations;
        private readonly IAuthService _authService;

        /// <summary>
        /// Constructor to initialize EV owner service with database collections
        /// </summary>
        public EVOwnerService(IMongoClient mongoClient, IAuthService authService)
        {
            var database = mongoClient.GetDatabase("EVChargingStationDB");
            _evOwners = database.GetCollection<EVOwner>("EVOwners");
            //_bookings = database.GetCollection<Booking>("Bookings");
            //_chargingStations = database.GetCollection<ChargingStation>("ChargingStations");
            _authService = authService;
        }

        /// <summary>
        /// Creates a new EV owner account
        /// </summary>
        public async Task<ApiResponseDTO<EVOwner>> CreateEVOwnerAsync(CreateEVOwnerDTO createEVOwnerDto)
        {
            try
            {
                // Validate NIC format (basic validation)
                if (string.IsNullOrWhiteSpace(createEVOwnerDto.NIC) || createEVOwnerDto.NIC.Length < 9)
                {
                    return new ApiResponseDTO<EVOwner>
                    {
                        Success = false,
                        Message = "Invalid NIC format"
                    };
                }

                // Check if NIC already exists
                var existingEVOwner = await _evOwners.Find(e => e.NIC == createEVOwnerDto.NIC).FirstOrDefaultAsync();
                if (existingEVOwner != null)
                {
                    return new ApiResponseDTO<EVOwner>
                    {
                        Success = false,
                        Message = "NIC already exists"
                    };
                }

                // Check if email already exists
                var existingEmail = await _evOwners.Find(e => e.Email == createEVOwnerDto.Email).FirstOrDefaultAsync();
                if (existingEmail != null)
                {
                    return new ApiResponseDTO<EVOwner>
                    {
                        Success = false,
                        Message = "Email already exists"
                    };
                }

                var (isValid, message) = PasswordValidator.Validate(createEVOwnerDto.Password);
                if (!isValid)
                {
                    return new ApiResponseDTO<EVOwner>
                    {
                        Success = false,
                        Message = message
                    };
                }

                var evOwner = new EVOwner
                {
                    NIC = createEVOwnerDto.NIC.ToUpper(),
                    FirstName = createEVOwnerDto.FirstName,
                    LastName = createEVOwnerDto.LastName,
                    Email = createEVOwnerDto.Email,
                    PhoneNumber = createEVOwnerDto.PhoneNumber,
                    PasswordHash = _authService.HashPassword(createEVOwnerDto.Password),
                    VehicleDetails = createEVOwnerDto.VehicleDetails,
                    IsActive = true
                };

                await _evOwners.InsertOneAsync(evOwner);

                // Remove password hash from response
                evOwner.PasswordHash = string.Empty;

                return new ApiResponseDTO<EVOwner>
                {
                    Success = true,
                    Message = "EV Owner created successfully",
                    Data = evOwner
                };
            }
            catch (Exception ex)
            {
                return new ApiResponseDTO<EVOwner>
                {
                    Success = false,
                    Message = "An error occurred while creating the EV owner"
                };
            }
        }

        /// <summary>
        /// Retrieves all EV owners from the system
        /// </summary>
        public async Task<ApiResponseDTO<List<EVOwner>>> GetAllEVOwnersAsync()
        {
            try
            {
                var evOwners = await _evOwners.Find(_ => true).ToListAsync();

                // Remove password hashes from response
                foreach (var evOwner in evOwners)
                {
                    evOwner.PasswordHash = string.Empty;
                }

                return new ApiResponseDTO<List<EVOwner>>
                {
                    Success = true,
                    Message = "EV Owners retrieved successfully",
                    Data = evOwners
                };
            }
            catch (Exception ex)
            {
                return new ApiResponseDTO<List<EVOwner>>
                {
                    Success = false,
                    Message = "An error occurred while retrieving EV owners"
                };
            }
        }

        /// <summary>
        /// Retrieves an EV owner by their NIC
        /// </summary>
        public async Task<ApiResponseDTO<EVOwner>> GetEVOwnerByNICAsync(string nic)
        {
            try
            {
                var evOwner = await _evOwners.Find(e => e.NIC == nic.ToUpper()).FirstOrDefaultAsync();

                if (evOwner == null)
                {
                    return new ApiResponseDTO<EVOwner>
                    {
                        Success = false,
                        Message = "EV Owner not found"
                    };
                }

                evOwner.PasswordHash = string.Empty;

                return new ApiResponseDTO<EVOwner>
                {
                    Success = true,
                    Message = "EV Owner retrieved successfully",
                    Data = evOwner
                };
            }
            catch (Exception ex)
            {
                return new ApiResponseDTO<EVOwner>
                {
                    Success = false,
                    Message = "An error occurred while retrieving the EV owner"
                };
            }
        }

        /// <summary>
        /// Retrieves an EV owner by their ID
        /// </summary>
        public async Task<ApiResponseDTO<EVOwner>> GetEVOwnerByIdAsync(string id)
        {
            try
            {
                var evOwner = await _evOwners.Find(e => e.Id == id).FirstOrDefaultAsync();

                if (evOwner == null)
                {
                    return new ApiResponseDTO<EVOwner>
                    {
                        Success = false,
                        Message = "EV Owner not found"
                    };
                }

                evOwner.PasswordHash = string.Empty;

                return new ApiResponseDTO<EVOwner>
                {
                    Success = true,
                    Message = "EV Owner retrieved successfully",
                    Data = evOwner
                };
            }
            catch (Exception ex)
            {
                return new ApiResponseDTO<EVOwner>
                {
                    Success = false,
                    Message = "An error occurred while retrieving the EV owner"
                };
            }
        }

        /// <summary>
        /// Updates an existing EV owner
        /// </summary>
        public async Task<ApiResponseDTO<EVOwner>> UpdateEVOwnerAsync(string id, EVOwner evOwner)
        {
            try
            {
                var existingEVOwner = await _evOwners.Find(e => e.Id == id).FirstOrDefaultAsync();
                if (existingEVOwner == null)
                {
                    return new ApiResponseDTO<EVOwner>
                    {
                        Success = false,
                        Message = "EV Owner not found"
                    };
                }

                evOwner.Id = id;
                evOwner.UpdatedAt = DateTime.UtcNow;
                evOwner.CreatedAt = existingEVOwner.CreatedAt;
                evOwner.NIC = existingEVOwner.NIC; // NIC cannot be changed

                var result = await _evOwners.ReplaceOneAsync(e => e.Id == id, evOwner);

                if (result.ModifiedCount > 0)
                {
                    evOwner.PasswordHash = string.Empty;
                    return new ApiResponseDTO<EVOwner>
                    {
                        Success = true,
                        Message = "EV Owner updated successfully",
                        Data = evOwner
                    };
                }

                return new ApiResponseDTO<EVOwner>
                {
                    Success = false,
                    Message = "Failed to update EV Owner"
                };
            }
            catch (Exception ex)
            {
                return new ApiResponseDTO<EVOwner>
                {
                    Success = false,
                    Message = "An error occurred while updating the EV owner"
                };
            }
        }

        /// <summary>
        /// Deletes an EV owner from the system
        /// </summary>
        public async Task<ApiResponseDTO<bool>> DeleteEVOwnerAsync(string id)
        {
            try
            {
                // Check if EV owner has active bookings
                //var activeBookings = await _bookings.Find(b =>
                //    b.EVOwnerNIC == id &&
                //    (b.Status == BookingStatus.Pending || b.Status == BookingStatus.Approved || b.Status == BookingStatus.InProgress)
                //).CountDocumentsAsync();

                //if (activeBookings > 0)
                //{
                //    return new ApiResponseDTO<bool>
                //    {
                //        Success = false,
                //        Message = "Cannot delete EV Owner with active bookings"
                //    };
                //}

                var result = await _evOwners.DeleteOneAsync(e => e.Id == id);

                if (result.DeletedCount > 0)
                {
                    return new ApiResponseDTO<bool>
                    {
                        Success = true,
                        Message = "EV Owner deleted successfully",
                        Data = true
                    };
                }

                return new ApiResponseDTO<bool>
                {
                    Success = false,
                    Message = "EV Owner not found"
                };
            }
            catch (Exception ex)
            {
                return new ApiResponseDTO<bool>
                {
                    Success = false,
                    Message = "An error occurred while deleting the EV owner"
                };
            }
        }

        /// <summary>
        /// Activates or deactivates an EV owner account
        /// </summary>
        public async Task<ApiResponseDTO<bool>> ActivateDeactivateEVOwnerAsync(string id, bool isActive)
        {
            try
            {
                var update = Builders<EVOwner>.Update
                    .Set(e => e.IsActive, isActive)
                    .Set(e => e.UpdatedAt, DateTime.UtcNow);

                var result = await _evOwners.UpdateOneAsync(e => e.Id == id, update);

                if (result.ModifiedCount > 0)
                {
                    return new ApiResponseDTO<bool>
                    {
                        Success = true,
                        Message = $"EV Owner {(isActive ? "activated" : "deactivated")} successfully",
                        Data = true
                    };
                }

                return new ApiResponseDTO<bool>
                {
                    Success = false,
                    Message = "EV Owner not found"
                };
            }
            catch (Exception ex)
            {
                return new ApiResponseDTO<bool>
                {
                    Success = false,
                    Message = "An error occurred while updating EV owner status"
                };
            }
        }

        /// <summary>
        /// Retrieves all deactivated EV owners for back office reactivation
        /// </summary>
        public async Task<ApiResponseDTO<List<EVOwner>>> GetDeactivatedEVOwnersAsync()
        {
            try
            {
                var deactivatedEVOwners = await _evOwners.Find(e => !e.IsActive).ToListAsync();

                // Remove password hashes from response
                foreach (var evOwner in deactivatedEVOwners)
                {
                    evOwner.PasswordHash = string.Empty;
                }

                return new ApiResponseDTO<List<EVOwner>>
                {
                    Success = true,
                    Message = "Deactivated EV Owners retrieved successfully",
                    Data = deactivatedEVOwners
                };
            }
            catch (Exception ex)
            {
                return new ApiResponseDTO<List<EVOwner>>
                {
                    Success = false,
                    Message = "An error occurred while retrieving deactivated EV owners"
                };
            }
        }

        /// <summary>
        /// Reactivates a deactivated EV owner account (back office function)
        /// </summary>
        public async Task<ApiResponseDTO<bool>> ReactivateEVOwnerAsync(string id)
        {
            try
            {
                var update = Builders<EVOwner>.Update
                    .Set(e => e.IsActive, true)
                    .Set(e => e.UpdatedAt, DateTime.UtcNow);

                var result = await _evOwners.UpdateOneAsync(e => e.Id == id && !e.IsActive, update);

                if (result.ModifiedCount > 0)
                {
                    return new ApiResponseDTO<bool>
                    {
                        Success = true,
                        Message = "EV Owner reactivated successfully",
                        Data = true
                    };
                }

                return new ApiResponseDTO<bool>
                {
                    Success = false,
                    Message = "EV Owner not found or already active"
                };
            }
            catch (Exception ex)
            {
                return new ApiResponseDTO<bool>
                {
                    Success = false,
                    Message = "An error occurred while reactivating EV owner"
                };
            }
        }

        /// <summary>
        /// Gets dashboard statistics for an EV owner
        /// </summary>
        //public async Task<ApiResponseDTO<DashboardStatsDTO>> GetEVOwnerDashboardStatsAsync(string evOwnerNIC)
        //{
        //    try
        //    {
        //        var pendingReservations = await _bookings.CountDocumentsAsync(b =>
        //            b.EVOwnerNIC == evOwnerNIC && b.Status == BookingStatus.Pending);

        //        var approvedFutureReservations = await _bookings.CountDocumentsAsync(b =>
        //            b.EVOwnerNIC == evOwnerNIC &&
        //            b.Status == BookingStatus.Approved &&
        //            b.ReservationDateTime > DateTime.UtcNow);

        //        var totalActiveStations = await _chargingStations.CountDocumentsAsync(s => s.IsActive);

        //        return new ApiResponseDTO<DashboardStatsDTO>
        //        {
        //            Success = true,
        //            Message = "Dashboard statistics retrieved successfully",
        //            Data = new DashboardStatsDTO
        //            {
        //                PendingReservations = (int)pendingReservations,
        //                ApprovedFutureReservations = (int)approvedFutureReservations,
        //                TotalActiveStations = (int)totalActiveStations
        //            }
        //        };
        //    }
        //    catch (Exception ex)
        //    {
        //        return new ApiResponseDTO<DashboardStatsDTO>
        //        {
        //            Success = false,
        //            Message = "An error occurred while retrieving dashboard statistics"
        //        };
        //    }
        //}
    }
}
