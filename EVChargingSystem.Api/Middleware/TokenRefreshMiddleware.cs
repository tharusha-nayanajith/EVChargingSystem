// ========================================
// Middleware/TokenRefreshMiddleware.cs
// ========================================
/*
 * TokenRefreshMiddleware.cs
 * Automatic token refresh middleware
 * Date: September 2025
 * Description: Automatically refreshes JWT access tokens when they are about to expire
 *              using refresh tokens stored in HTTP-only cookies
 */

using EVChargingSystem.Api.Models.DTOs;
using EVChargingSystem.Api.Services;
using Microsoft.AspNetCore.Http;
using Sprache;
using System.IdentityModel.Tokens.Jwt;

namespace EVChargingSystem.Api.Middleware
{
    /// <summary>
    /// Middleware that automatically refreshes JWT access tokens when they are about to expire
    /// </summary>
    public class TokenRefreshMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly IServiceProvider _serviceProvider;

        /// <summary>
        /// Constructor to initialize the middleware with request delegate and service provider
        /// </summary>
        /// <param name="next">The next middleware in the pipeline</param>
        /// <param name="serviceProvider">Service provider for dependency injection</param>
        public TokenRefreshMiddleware(RequestDelegate next, IServiceProvider serviceProvider)
        {
            _next = next;
            _serviceProvider = serviceProvider;
        }

        /// <summary>
        /// Main middleware execution method that checks for expiring tokens and refreshes them automatically
        /// </summary>
        /// <param name="context">The HTTP context for the current request</param>
        public async Task InvokeAsync(HttpContext context)
        {
            // Extract the JWT token from the Authorization header
            var token = ExtractTokenFromHeader(context);

            if (!string.IsNullOrEmpty(token))
            {
                try
                {
                    // Validate and parse the JWT token
                    if (IsValidToken(token, out var jsonToken))
                    {
                        // Check if the token expires within the next 5 minutes
                        if (IsTokenNearExpiry(jsonToken))
                        {
                            // Attempt to refresh the token using the refresh token from cookies
                            await AttemptTokenRefresh(context);
                        }
                    }
                }
                catch (Exception ex)
                {
                    // Log the exception if needed (add logging here if you have a logger)
                    // For now, just continue with the request even if token refresh fails
                }
            }

            // Continue to the next middleware in the pipeline
            await _next(context);
        }

        /// <summary>
        /// Extracts the JWT token from the Authorization header
        /// </summary>
        /// <param name="context">The HTTP context</param>
        /// <returns>The JWT token string or null if not found</returns>
        private string? ExtractTokenFromHeader(HttpContext context)
        {
            return context.Request.Cookies["accessToken"];
        }

        /// <summary>
        /// Validates if the token is a valid JWT token and can be read
        /// </summary>
        /// <param name="token">The JWT token string</param>
        /// <param name="jsonToken">Output parameter for the parsed JWT token</param>
        /// <returns>True if the token is valid, false otherwise</returns>
        private bool IsValidToken(string token, out JwtSecurityToken? jsonToken)
        {
            jsonToken = null;
            var tokenHandler = new JwtSecurityTokenHandler();

            if (!tokenHandler.CanReadToken(token))
                return false;

            jsonToken = tokenHandler.ReadJwtToken(token);
            return true;
        }

        /// <summary>
        /// Checks if the JWT token expires within the next 5 minutes
        /// </summary>
        /// <param name="jsonToken">The parsed JWT token</param>
        /// <returns>True if the token expires within 5 minutes, false otherwise</returns>
        private bool IsTokenNearExpiry(JwtSecurityToken jsonToken)
        {
            return jsonToken.ValidTo <= DateTime.UtcNow.AddMinutes(5);
        }

        /// <summary>
        /// Attempts to refresh the access token using the refresh token from cookies
        /// </summary>
        /// <param name="context">The HTTP context</param>
        private async Task AttemptTokenRefresh(HttpContext context)
        {
            // Get the refresh token from the HTTP-only cookie
            var refreshToken = context.Request.Cookies["refreshToken"];

            if (string.IsNullOrEmpty(refreshToken))
                return;

            // Create a scope to resolve the AuthService dependency
            using var scope = _serviceProvider.CreateScope();
            var authService = scope.ServiceProvider.GetRequiredService<IAuthService>();

            // Attempt to refresh the token
            var refreshResult = await authService.RefreshTokenAsync(new RefreshTokenRequestDTO
            {
                RefreshToken = refreshToken
            });

            if (refreshResult.Success && refreshResult.Data != null)
            {
                // Set the new refresh token as an HTTP-only cookie
                SetTokenCookie(context, refreshResult.Data.RefreshToken, refreshResult.Data.AccessToken, refreshResult.Data.RefreshTokenExpiresAt, refreshResult.Data.AccessTokenExpiresAt);
            }
        }

        /// <summary>
        /// Sets the refresh token as an HTTP-only cookie with security settings
        /// </summary>
        /// <param name="context">The HTTP context</param>
        /// <param name="refreshToken">The refresh token to store</param>
        /// <param name="accessToken">The access token to store</param>
        /// <param name="refreshExpiresAt">When the refresh token expires</param>
        /// <param name="accessExpiresAt">When the access token expires</param>
        private void SetTokenCookie(HttpContext context, string refreshToken, string accessToken, DateTime refreshExpiresAt, DateTime accessExpiresAt)
        {
            context.Response.Cookies.Append("accessToken", accessToken, new CookieOptions
            {
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.None, // required for cross-site
                Expires = accessExpiresAt,
                Path = "/api"
            });

            context.Response.Cookies.Append("refreshToken", refreshToken, new CookieOptions
            {
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.None,
                Expires = refreshExpiresAt,
                Path = "/api/auth/refresh"
            });
        }
    }
}