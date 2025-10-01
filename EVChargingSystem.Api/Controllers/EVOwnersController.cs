// ========================================
// Controllers/EVOwnersController.cs
// ========================================
/*
 * EVOwnersController.cs
 * EV Owner management controller
 * Date: September 2025
 * Description: Handles EV owner management operations
 */

using EVChargingSystem.Api.Models;
using EVChargingSystem.Api.Models.DTOs;
using EVChargingSystem.Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
namespace EVChargingSystem.Api.Controllers
{

    [ApiController]
    [Route("api/[controller]")]
    public class EVOwnersController : ControllerBase
    {
        private readonly IEVOwnerService _evOwnerService;

        /// <summary>
        /// Constructor to initialize EV owners controller
        /// </summary>
        public EVOwnersController(IEVOwnerService evOwnerService)
        {
            _evOwnerService = evOwnerService;
        }

        /// <summary>
        /// Creates a new EV owner account (Public endpoint for self-registration)
        /// </summary>
        [HttpPost("register")]
        public async Task<IActionResult> RegisterEVOwner([FromBody] CreateEVOwnerDTO createEVOwnerDto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(new ApiResponseDTO<object>
                    {
                        Success = false,
                        Message = "Invalid request data",
                        Errors = ModelState.Values.SelectMany(v => v.Errors.Select(e => e.ErrorMessage)).ToList()
                    });
                }

                var result = await _evOwnerService.CreateEVOwnerAsync(createEVOwnerDto);

                if (result.Success)
                {
                    return Ok(result);
                }

                return BadRequest(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ApiResponseDTO<object>
                {
                    Success = false,
                    Message = "An internal error occurred"
                });
            }
        }

        /// <summary>
        /// Gets all EV owners (BackOffice only)
        /// </summary>
        [HttpGet]
        [Authorize(Roles = "BackOffice")]
        public async Task<IActionResult> GetAllEVOwners()
        {
            try
            {
                var result = await _evOwnerService.GetAllEVOwnersAsync();
                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ApiResponseDTO<object>
                {
                    Success = false,
                    Message = "An internal error occurred"
                });
            }
        }

        /// <summary>
        /// Gets an EV owner by NIC
        /// </summary>
        [HttpGet("nic/{nic}")]
        [Authorize]
        public async Task<IActionResult> GetEVOwnerByNIC(string nic)
        {
            try
            {
                var result = await _evOwnerService.GetEVOwnerByNICAsync(nic);

                if (result.Success)
                {
                    return Ok(result);
                }

                return NotFound(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ApiResponseDTO<object>
                {
                    Success = false,
                    Message = "An internal error occurred"
                });
            }
        }

        /// <summary>
        /// Gets an EV owner by ID
        /// </summary>
        [HttpGet("{id}")]
        [Authorize]
        public async Task<IActionResult> GetEVOwnerById(string id)
        {
            try
            {
                // Check if user is asking for their own profile or is BackOffice
                var currentUserId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                var userRole = User.FindFirst(ClaimTypes.Role)?.Value;

                if (currentUserId != id && userRole != "BackOffice")
                {
                    return Forbid();
                }

                var result = await _evOwnerService.GetEVOwnerByIdAsync(id);

                if (result.Success)
                {
                    return Ok(result);
                }

                return NotFound(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ApiResponseDTO<object>
                {
                    Success = false,
                    Message = "An internal error occurred"
                });
            }
        }

        /// <summary>
        /// Updates an EV owner profile
        /// </summary>
        [HttpPut("{id}")]
        [Authorize]
        public async Task<IActionResult> UpdateEVOwner(string id, [FromBody] EVOwner evOwner)
        {
            try
            {
                // Check if user is updating their own profile or is BackOffice
                var currentUserId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                var userRole = User.FindFirst(ClaimTypes.Role)?.Value;

                if (currentUserId != id && userRole != "BackOffice")
                {
                    return Forbid();
                }

                if (!ModelState.IsValid)
                {
                    return BadRequest(new ApiResponseDTO<object>
                    {
                        Success = false,
                        Message = "Invalid request data",
                        Errors = ModelState.Values.SelectMany(v => v.Errors.Select(e => e.ErrorMessage)).ToList()
                    });
                }

                var result = await _evOwnerService.UpdateEVOwnerAsync(id, evOwner);

                if (result.Success)
                {
                    return Ok(result);
                }

                return BadRequest(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ApiResponseDTO<object>
                {
                    Success = false,
                    Message = "An internal error occurred"
                });
            }
        }

        /// <summary>
        /// Deactivates an EV owner account (Self-deactivation allowed)
        /// </summary>
        [HttpPatch("{id}/deactivate")]
        [Authorize]
        public async Task<IActionResult> DeactivateEVOwner(string id)
        {
            try
            {
                // Check if user is deactivating their own account or is BackOffice
                var currentUserId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                var userRole = User.FindFirst(ClaimTypes.Role)?.Value;

                if (currentUserId != id && userRole != "BackOffice")
                {
                    return Forbid();
                }

                var result = await _evOwnerService.ActivateDeactivateEVOwnerAsync(id, false);

                if (result.Success)
                {
                    return Ok(result);
                }

                return BadRequest(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ApiResponseDTO<object>
                {
                    Success = false,
                    Message = "An internal error occurred"
                });
            }
        }

        /// <summary>
        /// Gets all deactivated EV owners for reactivation (BackOffice only)
        /// </summary>
        [HttpGet("deactivated")]
        [Authorize(Roles = "BackOffice")]
        public async Task<IActionResult> GetDeactivatedEVOwners()
        {
            try
            {
                var result = await _evOwnerService.GetDeactivatedEVOwnersAsync();
                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ApiResponseDTO<object>
                {
                    Success = false,
                    Message = "An internal error occurred"
                });
            }
        }

        /// <summary>
        /// Reactivates a deactivated EV owner account (BackOffice only)
        /// </summary>
        [HttpPatch("{id}/reactivate")]
        [Authorize(Roles = "BackOffice")]
        public async Task<IActionResult> ReactivateEVOwner(string id)
        {
            try
            {
                var result = await _evOwnerService.ReactivateEVOwnerAsync(id);

                if (result.Success)
                {
                    return Ok(result);
                }

                return BadRequest(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ApiResponseDTO<object>
                {
                    Success = false,
                    Message = "An internal error occurred"
                });
            }
        }

        /// <summary>
        /// Gets dashboard statistics for an EV owner
        /// </summary>
        //[HttpGet("dashboard/{nic}")]
        //[Authorize(Roles = "EVOwner")]
        //public async Task<IActionResult> GetDashboardStats(string nic)
        //{
        //    try
        //    {
        //        var result = await _evOwnerService.GetEVOwnerDashboardStatsAsync(nic);
        //        return Ok(result);
        //    }
        //    catch (Exception ex)
        //    {
        //        return StatusCode(500, new ApiResponseDTO<object>
        //        {
        //            Success = false,
        //            Message = "An internal error occurred"
        //        });
        //    }
        //}

        /// <summary>
        /// Deletes an EV owner (BackOffice only)
        /// </summary>
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteEVOwner(string id)
        {
            try
            {
                // Check if user is deleting their own account or is BackOffice
                var currentUserId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                var userRole = User.FindFirst(ClaimTypes.Role)?.Value;

                if (currentUserId != id && userRole != "BackOffice")
                {
                    return Forbid();
                }

                var result = await _evOwnerService.DeleteEVOwnerAsync(id);

                if (result.Success)
                {
                    return Ok(result);
                }

                return BadRequest(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ApiResponseDTO<object>
                {
                    Success = false,
                    Message = "An internal error occurred"
                });
            }
        }
    }
}

