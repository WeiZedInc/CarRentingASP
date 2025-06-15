using CarRentalSystem.Data;
using CarRentalSystem.DTOs;
using CarRentalSystem.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace CarRentalSystem.Services
{
    public interface IAuthService
    {
        Task<(bool Success, string Token, UserResponseDto User, string ErrorMessage)> Login(UserLoginDto loginDto);
        Task<(bool Success, string ErrorMessage)> Register(UserRegistrationDto registrationDto);
        Task<(bool Success, string Token, UserResponseDto User, string ErrorMessage)> GoogleLogin(string googleId, string email, string name = null);
        string GenerateJwtToken(User user);
        bool VerifyPassword(string password, string passwordHash);
        string HashPassword(string password);
    }

    public class AuthService : IAuthService
    {
        private readonly CarRentalDbContext _context;
        private readonly IConfiguration _configuration;
        private readonly IEmailService _emailService;

        public AuthService(
            CarRentalDbContext context,
            IConfiguration configuration,
            IEmailService emailService)
        {
            _context = context;
            _configuration = configuration;
            _emailService = emailService;
        }

        public async Task<(bool Success, string Token, UserResponseDto User, string ErrorMessage)> Login(UserLoginDto loginDto)
        {
            var user = await _context.Users
                .Include(u => u.LoyaltyProgram)
                .FirstOrDefaultAsync(u => u.Email == loginDto.Email);

            if (user == null)
            {
                return (false, null, null, "User not found");
            }

            // Check if user is a Google user (doesn't have a password)
            if (string.IsNullOrEmpty(user.PasswordHash))
            {
                return (false, null, null, "This account uses Google sign-in. Please use the 'Continue with Google' button.");
            }

            if (!VerifyPassword(loginDto.Password, user.PasswordHash))
            {
                return (false, null, null, "Invalid password");
            }

            var token = GenerateJwtToken(user);

            var userDto = new UserResponseDto
            {
                Id = user.Id,
                Email = user.Email,
                FirstName = user.FirstName,
                LastName = user.LastName,
                PhoneNumber = user.PhoneNumber,
                Address = user.Address,
                Role = user.Role.ToString(),
                RegistrationDate = user.RegistrationDate,
                LoyaltyPoints = user.LoyaltyProgram?.Points ?? 0,
                LoyaltyTier = user.LoyaltyProgram?.Tier ?? "None"
            };

            return (true, token, userDto, null);
        }

        public async Task<(bool Success, string ErrorMessage)> Register(UserRegistrationDto registrationDto)
        {
            if (await _context.Users.AnyAsync(u => u.Email == registrationDto.Email))
            {
                return (false, "Email already exists");
            }

            var passwordHash = HashPassword(registrationDto.Password);

            var user = new User
            {
                Email = registrationDto.Email,
                PasswordHash = passwordHash,
                FirstName = registrationDto.FirstName,
                LastName = registrationDto.LastName,
                PhoneNumber = registrationDto.PhoneNumber,
                Address = registrationDto.Address,
                Role = UserRole.Customer,
                RegistrationDate = DateTime.UtcNow
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            // Create loyalty program for the user
            var loyaltyProgram = new LoyaltyProgram
            {
                UserId = user.Id,
                Points = 0,
                Tier = "Bronze",
                LastUpdated = DateTime.UtcNow
            };

            _context.LoyaltyPrograms.Add(loyaltyProgram);
            await _context.SaveChangesAsync();

            // Send welcome email
            try
            {
                await _emailService.SendWelcomeEmailAsync(user);
            }
            catch (Exception ex)
            {
                // Log the error but don't fail registration if email fails
                Console.WriteLine($"Failed to send welcome email: {ex.Message}");
            }

            return (true, null);
        }

        public async Task<(bool Success, string Token, UserResponseDto User, string ErrorMessage)> GoogleLogin(string googleId, string email, string name = null)
        {
            try
            {
                Console.WriteLine($"GoogleLogin called with: GoogleId={googleId}, Email={email}, Name={name}");

                // Validate input parameters
                if (string.IsNullOrEmpty(googleId) || string.IsNullOrEmpty(email))
                {
                    return (false, null, null, "Invalid Google authentication data.");
                }

                // First, try to find user by GoogleId
                var user = await _context.Users
                    .Include(u => u.LoyaltyProgram)
                    .FirstOrDefaultAsync(u => u.GoogleId == googleId);

                Console.WriteLine($"User found by GoogleId: {user != null}");

                // If not found by GoogleId, try by email
                if (user == null)
                {
                    user = await _context.Users
                        .Include(u => u.LoyaltyProgram)
                        .FirstOrDefaultAsync(u => u.Email == email);

                    Console.WriteLine($"User found by Email: {user != null}");
                }

                // If user exists but doesn't have GoogleId, link the accounts
                if (user != null && string.IsNullOrEmpty(user.GoogleId))
                {
                    Console.WriteLine("Linking existing user with Google account");
                    user.GoogleId = googleId;
                    user.IsEmailVerified = true; // Mark as verified since it's from Google
                    await _context.SaveChangesAsync();
                }

                // If user doesn't exist, create a new one
                if (user == null)
                {
                    Console.WriteLine("Creating new Google user");

                    // Parse name if provided
                    string firstName = "Google";
                    string lastName = "User";

                    if (!string.IsNullOrEmpty(name))
                    {
                        var nameParts = name.Split(' ', StringSplitOptions.RemoveEmptyEntries);
                        if (nameParts.Length >= 1)
                        {
                            firstName = nameParts[0];
                        }
                        if (nameParts.Length >= 2)
                        {
                            lastName = string.Join(" ", nameParts.Skip(1));
                        }
                    }

                    Console.WriteLine($"Parsed name: FirstName={firstName}, LastName={lastName}");

                    // Create new user for Google authentication
                    user = new User
                    {
                        Email = email,
                        GoogleId = googleId,
                        FirstName = firstName,
                        LastName = lastName,
                        Role = UserRole.Customer,
                        RegistrationDate = DateTime.UtcNow,
                        PasswordHash = null, // Google users don't need a password
                        IsEmailVerified = true, // Google emails are considered verified
                        PhoneNumber = null,
                        Address = null
                    };

                    try
                    {
                        _context.Users.Add(user);
                        await _context.SaveChangesAsync();
                        Console.WriteLine($"User created with ID: {user.Id}");
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Error creating user: {ex.Message}");
                        Console.WriteLine($"Inner exception: {ex.InnerException?.Message}");
                        return (false, null, null, "Failed to create user account. Please try again.");
                    }

                    // Create loyalty program for the new user
                    try
                    {
                        var loyaltyProgram = new LoyaltyProgram
                        {
                            UserId = user.Id,
                            Points = 0,
                            Tier = "Bronze",
                            LastUpdated = DateTime.UtcNow
                        };

                        _context.LoyaltyPrograms.Add(loyaltyProgram);
                        await _context.SaveChangesAsync();
                        Console.WriteLine("Loyalty program created");

                        // Load the loyalty program for the response
                        user.LoyaltyProgram = loyaltyProgram;
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Error creating loyalty program: {ex.Message}");
                        // Continue without loyalty program for now
                    }

                    // Send welcome email for new Google users
                    try
                    {
                        await _emailService.SendWelcomeEmailAsync(user);
                    }
                    catch (Exception ex)
                    {
                        // Log the error but don't fail login if email fails
                        Console.WriteLine($"Failed to send welcome email to Google user: {ex.Message}");
                    }
                }

                Console.WriteLine("Generating JWT token");
                var token = GenerateJwtToken(user);

                var userDto = new UserResponseDto
                {
                    Id = user.Id,
                    Email = user.Email,
                    FirstName = user.FirstName,
                    LastName = user.LastName,
                    PhoneNumber = user.PhoneNumber,
                    Address = user.Address,
                    Role = user.Role.ToString(),
                    RegistrationDate = user.RegistrationDate,
                    LoyaltyPoints = user.LoyaltyProgram?.Points ?? 0,
                    LoyaltyTier = user.LoyaltyProgram?.Tier ?? "None"
                };

                Console.WriteLine("Google login successful");
                return (true, token, userDto, null);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Google login error: {ex.Message}");
                Console.WriteLine($"Stack trace: {ex.StackTrace}");
                if (ex.InnerException != null)
                {
                    Console.WriteLine($"Inner exception: {ex.InnerException.Message}");
                }
                return (false, null, null, $"Authentication error: {ex.Message}");
            }
        }

        public string GenerateJwtToken(User user)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(_configuration["Jwt:Secret"]);

            var claims = new ClaimsIdentity(new[]
            {
                new Claim(ClaimTypes.Name, user.Id.ToString()),
                new Claim(ClaimTypes.Email, user.Email),
                new Claim(ClaimTypes.Role, user.Role.ToString())
            });

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = claims,
                Expires = DateTime.UtcNow.AddDays(7),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature),
                Issuer = _configuration["Jwt:Issuer"],
                Audience = _configuration["Jwt:Audience"]
            };

            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }

        public bool VerifyPassword(string password, string passwordHash)
        {
            // Extract parameters from hashed password
            var parts = passwordHash.Split(':', 3);
            if (parts.Length != 3)
            {
                return false;
            }

            var iterations = int.Parse(parts[0]);
            var salt = Convert.FromBase64String(parts[1]);
            var hash = Convert.FromBase64String(parts[2]);

            using (var pbkdf2 = new Rfc2898DeriveBytes(password, salt, iterations, HashAlgorithmName.SHA256))
            {
                var testHash = pbkdf2.GetBytes(hash.Length);
                return CryptographicOperations.FixedTimeEquals(hash, testHash);
            }
        }

        public string HashPassword(string password)
        {
            // Generate a random salt
            byte[] salt = new byte[16];
            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(salt);
            }

            // Hash the password
            const int iterations = 10000;
            using (var pbkdf2 = new Rfc2898DeriveBytes(password, salt, iterations, HashAlgorithmName.SHA256))
            {
                var hashBytes = pbkdf2.GetBytes(32);
                return $"{iterations}:{Convert.ToBase64String(salt)}:{Convert.ToBase64String(hashBytes)}";
            }
        }

        private string GenerateRandomPassword()
        {
            // Generate a random password for Google users (they won't use it anyway)
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789!@#$%^&*";
            var random = new Random();
            var password = new string(Enumerable.Repeat(chars, 12)
                .Select(s => s[random.Next(s.Length)]).ToArray());

            return HashPassword(password);
        }
    }
}