using Microsoft.Extensions.Options;
using StudentProgressTracker.Configuration;
using StudentProgressTracker.Services.Interfaces;
using System.Net;
using System.Text.Json;

namespace StudentProgressTracker.Middleware
{
    /// <summary>
    /// Middleware for API rate limiting
    /// </summary>
    public class RateLimitingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly IRateLimitService _rateLimitService;
        private readonly RateLimitOptions _options;
        private readonly ILogger<RateLimitingMiddleware> _logger;

        public RateLimitingMiddleware(
            RequestDelegate next,
            IRateLimitService rateLimitService,
            IOptions<RateLimitOptions> options,
            ILogger<RateLimitingMiddleware> logger)
        {
            _next = next;
            _rateLimitService = rateLimitService;
            _options = options.Value;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                // Skip rate limiting for certain paths (health checks, swagger, etc.)
                if (ShouldSkipRateLimit(context.Request.Path))
                {
                    await _next(context);
                    return;
                }

                // Extract client information
                var clientId = ExtractClientId(context);
                var ipAddress = ExtractIpAddress(context);
                var endpoint = ExtractEndpoint(context);

                _logger.LogDebug("Processing rate limit check for client {ClientId} from IP {IPAddress} accessing {Endpoint}",
                    clientId, ipAddress, endpoint);

                // Check rate limits
                var rateLimitResult = await _rateLimitService.CheckRateLimitAsync(clientId, endpoint, ipAddress);

                // Add rate limit headers to response
                foreach (var header in rateLimitResult.Headers)
                {
                    context.Response.Headers[header.Key] = header.Value;
                }

                if (!rateLimitResult.IsAllowed)
                {
                    // Rate limit exceeded - return 429 Too Many Requests
                    await HandleRateLimitExceeded(context, rateLimitResult);
                    return;
                }

                // Record the request
                await _rateLimitService.RecordRequestAsync(clientId, endpoint, ipAddress);

                // Continue to next middleware
                await _next(context);

                _logger.LogDebug("Request processed successfully for client {ClientId}", clientId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in rate limiting middleware");
                // Continue processing even if rate limiting fails (fail-open approach)
                await _next(context);
            }
        }

        private bool ShouldSkipRateLimit(PathString path)
        {
            var pathString = path.Value?.ToLowerInvariant() ?? "";

            // Skip rate limiting for these paths
            var skipPaths = new[]
            {
                "/health",
                "/healthz",
                "/swagger",
                "/favicon.ico",
                "/.well-known",
                "/metrics"
            };

            return skipPaths.Any(skipPath => pathString.StartsWith(skipPath));
        }

        private string ExtractClientId(HttpContext context)
        {
            // Try to extract client ID from various sources
            
            // 1. Custom header
            if (context.Request.Headers.TryGetValue("X-Client-Id", out var clientIdHeader))
            {
                return clientIdHeader.FirstOrDefault() ?? "";
            }

            // 2. API Key
            if (context.Request.Headers.TryGetValue("X-API-Key", out var apiKeyHeader))
            {
                return apiKeyHeader.FirstOrDefault() ?? "";
            }

            // 3. JWT token subject claim
            if (context.User.Identity?.IsAuthenticated == true)
            {
                var userIdClaim = context.User.FindFirst("UserId")?.Value;
                var emailClaim = context.User.FindFirst("email")?.Value;
                
                if (!string.IsNullOrEmpty(userIdClaim))
                {
                    return $"user_{userIdClaim}";
                }
                
                if (!string.IsNullOrEmpty(emailClaim))
                {
                    return $"email_{emailClaim}";
                }
            }

            // 4. Fall back to IP address
            return $"ip_{ExtractIpAddress(context)}";
        }

        private string ExtractIpAddress(HttpContext context)
        {
            // Try to get real IP address from various headers (reverse proxy scenarios)
            var headers = new[]
            {
                "X-Forwarded-For",
                "X-Real-IP",
                "CF-Connecting-IP", // Cloudflare
                "X-Cluster-Client-IP"
            };

            foreach (var header in headers)
            {
                if (context.Request.Headers.TryGetValue(header, out var headerValue))
                {
                    var ip = headerValue.FirstOrDefault()?.Split(',').FirstOrDefault()?.Trim();
                    if (!string.IsNullOrEmpty(ip) && IPAddress.TryParse(ip, out _))
                    {
                        return ip;
                    }
                }
            }

            // Fall back to connection remote IP
            return context.Connection.RemoteIpAddress?.ToString() ?? "unknown";
        }

        private string ExtractEndpoint(HttpContext context)
        {
            var method = context.Request.Method;
            var path = context.Request.Path.Value ?? "";
            
            // Normalize path to remove dynamic segments for better grouping
            var normalizedPath = NormalizePath(path);
            
            return $"{method} {normalizedPath}";
        }

        private string NormalizePath(string path)
        {
            // Replace numeric IDs with placeholders for better rate limit grouping
            // Examples:
            // /api/v1.0/students/123 -> /api/v1.0/students/{id}
            // /api/v1.0/students/123/progress -> /api/v1.0/students/{id}/progress
            
            var segments = path.Split('/', StringSplitOptions.RemoveEmptyEntries);
            
            for (int i = 0; i < segments.Length; i++)
            {
                // Replace numeric segments with {id}
                if (int.TryParse(segments[i], out _))
                {
                    segments[i] = "{id}";
                }
                // Replace GUID segments with {guid}
                else if (Guid.TryParse(segments[i], out _))
                {
                    segments[i] = "{guid}";
                }
            }

            return "/" + string.Join("/", segments);
        }

        private async Task HandleRateLimitExceeded(HttpContext context, RateLimitResult rateLimitResult)
        {
            context.Response.StatusCode = (int)_options.ResponseConfig.StatusCode;
            context.Response.ContentType = _options.ResponseConfig.ContentType;

            // Create structured error response
            var errorResponse = new
            {
                success = false,
                message = _options.ResponseConfig.Message,
                error = new
                {
                    code = "RATE_LIMIT_EXCEEDED",
                    reason = rateLimitResult.Reason,
                    retryAfter = rateLimitResult.RetryAfter?.TotalSeconds,
                    limits = new
                    {
                        clientTier = rateLimitResult.LimitInfo.ClientTier,
                        requestsRemaining = new
                        {
                            minute = Math.Max(0, GetLimitFromHeaders(rateLimitResult.Headers, "X-RateLimit-Limit-Minute") - rateLimitResult.LimitInfo.RequestsInCurrentMinute),
                            hour = Math.Max(0, GetLimitFromHeaders(rateLimitResult.Headers, "X-RateLimit-Limit-Hour") - rateLimitResult.LimitInfo.RequestsInCurrentHour),
                            day = Math.Max(0, GetLimitFromHeaders(rateLimitResult.Headers, "X-RateLimit-Limit-Day") - rateLimitResult.LimitInfo.RequestsInCurrentDay)
                        }
                    }
                },
                metadata = new
                {
                    timestamp = DateTime.UtcNow,
                    endpoint = rateLimitResult.LimitInfo.Endpoint,
                    clientId = rateLimitResult.LimitInfo.ClientId
                }
            };

            var jsonResponse = JsonSerializer.Serialize(errorResponse, new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                WriteIndented = true
            });

            _logger.LogWarning("Rate limit exceeded for client {ClientId} accessing {Endpoint}. Reason: {Reason}",
                rateLimitResult.LimitInfo.ClientId, rateLimitResult.LimitInfo.Endpoint, rateLimitResult.Reason);

            await context.Response.WriteAsync(jsonResponse);
        }

        private int GetLimitFromHeaders(Dictionary<string, string> headers, string headerName)
        {
            return headers.TryGetValue(headerName, out var value) && int.TryParse(value, out var limit) ? limit : 0;
        }
    }

    /// <summary>
    /// Extension methods for rate limiting middleware registration
    /// </summary>
    public static class RateLimitingMiddlewareExtensions
    {
        /// <summary>
        /// Adds rate limiting middleware to the application pipeline
        /// </summary>
        public static IApplicationBuilder UseRateLimiting(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<RateLimitingMiddleware>();
        }
    }
}