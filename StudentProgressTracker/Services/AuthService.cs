using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using StudentProgressTracker.Data;
using StudentProgressTracker.DTOs;
using StudentProgressTracker.Models;
using StudentProgressTracker.Services.Interfaces;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace StudentProgressTracker.Services
{
    public class AuthService : IAuthService
    {
        private readonly ApplicationDbContext _context;
        private readonly IConfiguration _configuration;
        private readonly UserManager<User> userManager;
        private readonly ILogger<AuthService> _logger;

        public AuthService(
            ApplicationDbContext context,
            IConfiguration configuration,
            UserManager<User> userManager,
            ILogger<AuthService> logger)
        {
            _context = context;
            _configuration = configuration;
            this.userManager = userManager;
            _logger = logger;
        }

        public async Task<AuthDto.LoginResponseDto?> LoginAsync(AuthDto.LoginRequestDto loginRequest)
        {
            try
            {
                var user = await _context.Users
                    .FirstOrDefaultAsync(u => u.Email == loginRequest.Email && u.IsActive);

                if (user == null || !BCrypt.Net.BCrypt.Verify(loginRequest.Password, user.PasswordHash))
                {
                    _logger.LogWarning("Failed login attempt for email {Email}", loginRequest.Email);
                    return null;
                }

                // Update last login date
                user.LastLoginDate = DateTime.UtcNow;
                await _context.SaveChangesAsync();

                var token = GenerateJwtToken(user);
                var refreshToken = Guid.NewGuid().ToString(); // In production, store this securely

                var response = new AuthDto.LoginResponseDto
                {
                    Token = token,
                    RefreshToken = refreshToken,
                    ExpiresAt = DateTime.UtcNow.AddHours(24),
                    User = new AuthDto.UserProfileDto
                    {
                        Id = user.Id,
                        FirstName = user.FirstName,
                        LastName = user.LastName,
                        Email = user.Email,
                        Role = user.Role,
                        IsActive = user.IsActive,
                        LastLoginDate = user.LastLoginDate
                    }
                };

                _logger.LogInformation("Successful login for user {UserId}", user.Id);
                return response;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during login for email {Email}", loginRequest.Email);
                throw;
            }
        }

        public async Task<AuthDto.UserProfileDto?> GetUserProfileAsync(int userId)
        {
            try
            {
                var user = await _context.Users
                    .FirstOrDefaultAsync(u => u.Id == userId && u.IsActive);

                if (user == null) return null;

                return new AuthDto.UserProfileDto
                {
                    Id = user.Id,
                    FirstName = user.FirstName,
                    LastName = user.LastName,
                    Email = user.Email,
                    Role = user.Role,
                    IsActive = user.IsActive,
                    LastLoginDate = user.LastLoginDate
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving user profile for user {UserId}", userId);
                throw;
            }
        }

        public string GenerateJwtToken(User user)
        {
            var jwtKey = _configuration["JwtSettings:SecretKey"] ?? "StudentProgressTracker_JWT_Secret_Key_2024_Must_Be_At_Least_32_Characters_Long";
            var jwtIssuer = _configuration["JwtSettings:Issuer"] ?? "StudentProgressTracker";
            var jwtAudience = _configuration["JwtSettings:Audience"] ?? "StudentProgressTracker.Users";

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey));
            var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var claims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Name, $"{user.FirstName} {user.LastName}"),
                new Claim(ClaimTypes.Email, user.Email),
                new Claim(ClaimTypes.Role, user.Role),
                new Claim("UserId", user.Id.ToString())
            };

            var token = new JwtSecurityToken(
                issuer: jwtIssuer,
                audience: jwtAudience,
                claims: claims,
                expires: DateTime.UtcNow.AddHours(24),
                signingCredentials: credentials
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        public int? ValidateJwtToken(string token)
        {
            try
            {
                var jwtKey = _configuration["JwtSettings:SecretKey"] ?? "your-super-secret-jwt-key-that-is-at-least-32-characters-long";
                var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey));

                var tokenHandler = new JwtSecurityTokenHandler();
                var validationParameters = new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = key,
                    ValidateIssuer = true,
                    ValidIssuer = _configuration["JwtSettings:Issuer"] ?? "StudentProgressTracker",
                    ValidateAudience = true,
                    ValidAudience = _configuration["JwtSettings:Audience"] ?? "StudentProgressTracker",
                    ValidateLifetime = true,
                    ClockSkew = TimeSpan.Zero
                };

                tokenHandler.ValidateToken(token, validationParameters, out SecurityToken validatedToken);

                var jwtToken = (JwtSecurityToken)validatedToken;
                var userIdClaim = jwtToken.Claims.FirstOrDefault(c => c.Type == "UserId");

                if (userIdClaim != null && int.TryParse(userIdClaim.Value, out int userId))
                {
                    return userId;
                }

                return null;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Token validation failed");
                return null;
            }
        }
    }
}
