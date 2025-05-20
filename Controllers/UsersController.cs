using CarRentalSystem.DTOs;
using CarRentalSystem.Models;
using CarRentalSystem.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace CarRentalSystem.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class UsersController : ControllerBase
    {
        private readonly IUserRepository _userRepository;
        private readonly ILoyaltyProgramRepository _loyaltyRepository;
        private readonly IDocumentRepository _documentRepository;

        public UsersController(
            IUserRepository userRepository,
            ILoyaltyProgramRepository loyaltyRepository,
            IDocumentRepository documentRepository)
        {
            _userRepository = userRepository;
            _loyaltyRepository = loyaltyRepository;
            _documentRepository = documentRepository;
        }

        // GET: api/users
        [HttpGet]
        [Authorize(Roles = "Administrator,Manager")]
        public async Task<ActionResult<List<UserResponseDto>>> GetAllUsers()
        {
            var users = await _userRepository.GetAllAsync();
            var userDtos = new List<UserResponseDto>();

            foreach (var user in users)
            {
                var loyaltyProgram = await _loyaltyRepository.GetUserLoyaltyProgramAsync(user.Id);

                userDtos.Add(new UserResponseDto
                {
                    Id = user.Id,
                    Email = user.Email,
                    FirstName = user.FirstName,
                    LastName = user.LastName,
                    PhoneNumber = user.PhoneNumber,
                    Address = user.Address,
                    Role = user.Role.ToString(),
                    RegistrationDate = user.RegistrationDate,
                    LoyaltyPoints = loyaltyProgram?.Points ?? 0,
                    LoyaltyTier = loyaltyProgram?.Tier ?? "None"
                });
            }

            return Ok(userDtos);
        }

        // GET: api/users/5
        [HttpGet("{id}")]
        public async Task<ActionResult<UserResponseDto>> GetUser(int id)
        {
            if (!User.IsInRole("Administrator") && !User.IsInRole("Manager") && id != GetUserId())
                return Forbid();

            var user = await _userRepository.GetUserWithDetailsAsync(id);

            if (user == null)
                return NotFound();

            var loyaltyProgram = await _loyaltyRepository.GetUserLoyaltyProgramAsync(id);

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
                LoyaltyPoints = loyaltyProgram?.Points ?? 0,
                LoyaltyTier = loyaltyProgram?.Tier ?? "None"
            };

            return Ok(userDto);
        }

        // GET: api/users/profile
        [HttpGet("profile")]
        public async Task<ActionResult<UserResponseDto>> GetProfile()
        {
            var userId = GetUserId();
            var user = await _userRepository.GetUserWithDetailsAsync(userId);

            if (user == null)
                return NotFound();

            var loyaltyProgram = await _loyaltyRepository.GetUserLoyaltyProgramAsync(userId);

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
                LoyaltyPoints = loyaltyProgram?.Points ?? 0,
                LoyaltyTier = loyaltyProgram?.Tier ?? "None"
            };

            return Ok(userDto);
        }

        // PUT: api/users/profile
        [HttpPut("profile")]
        public async Task<ActionResult<UserResponseDto>> UpdateProfile(UserUpdateDto userDto)
        {
            var userId = GetUserId();
            var user = await _userRepository.GetUserWithDetailsAsync(userId);

            if (user == null)
                return NotFound();

            // Update user properties
            user.FirstName = userDto.FirstName ?? user.FirstName;
            user.LastName = userDto.LastName ?? user.LastName;
            user.PhoneNumber = userDto.PhoneNumber ?? user.PhoneNumber;
            user.Address = userDto.Address ?? user.Address;

            await _userRepository.UpdateAsync(user);

            var loyaltyProgram = await _loyaltyRepository.GetUserLoyaltyProgramAsync(userId);

            var updatedUserDto = new UserResponseDto
            {
                Id = user.Id,
                Email = user.Email,
                FirstName = user.FirstName,
                LastName = user.LastName,
                PhoneNumber = user.PhoneNumber,
                Address = user.Address,
                Role = user.Role.ToString(),
                RegistrationDate = user.RegistrationDate,
                LoyaltyPoints = loyaltyProgram?.Points ?? 0,
                LoyaltyTier = loyaltyProgram?.Tier ?? "None"
            };

            return Ok(updatedUserDto);
        }

        // PUT: api/users/5/role
        [HttpPut("{id}/role")]
        [Authorize(Roles = "Administrator")]
        public async Task<ActionResult> UpdateUserRole(int id, [FromBody] string roleName)
        {
            var user = await _userRepository.GetByIdAsync(id);

            if (user == null)
                return NotFound();

            // Parse role
            if (!Enum.TryParse<UserRole>(roleName, out var role))
                return BadRequest("Invalid role");

            user.Role = role;
            await _userRepository.UpdateAsync(user);

            return Ok(new { message = "User role updated successfully" });
        }

        // GET: api/users/documents
        [HttpGet("documents")]
        public async Task<ActionResult<List<DocumentResponseDto>>> GetMyDocuments()
        {
            var userId = GetUserId();
            var documents = await _documentRepository.GetUserDocumentsAsync(userId);

            var documentDtos = documents.Select(d => new DocumentResponseDto
            {
                Id = d.Id,
                DocumentType = d.DocumentType,
                DocumentNumber = d.DocumentNumber,
                FileUrl = d.FileUrl,
                UploadDate = d.UploadDate,
                IsVerified = d.IsVerified,
                VerifiedDate = d.VerifiedDate
            }).ToList();

            return Ok(documentDtos);
        }

        // POST: api/users/documents
        [HttpPost("documents")]
        public async Task<ActionResult<DocumentResponseDto>> UploadDocument(DocumentCreateDto documentDto)
        {
            var userId = GetUserId();

            var document = new Document
            {
                UserId = userId,
                DocumentType = documentDto.DocumentType,
                DocumentNumber = documentDto.DocumentNumber,
                FileUrl = documentDto.FileUrl,
                UploadDate = DateTime.UtcNow,
                IsVerified = false
            };

            var createdDocument = await _documentRepository.AddAsync(document);

            var documentResponseDto = new DocumentResponseDto
            {
                Id = createdDocument.Id,
                DocumentType = createdDocument.DocumentType,
                DocumentNumber = createdDocument.DocumentNumber,
                FileUrl = createdDocument.FileUrl,
                UploadDate = createdDocument.UploadDate,
                IsVerified = createdDocument.IsVerified,
                VerifiedDate = createdDocument.VerifiedDate
            };

            return CreatedAtAction(nameof(GetMyDocuments), documentResponseDto);
        }

        // PUT: api/users/documents/5/verify
        [HttpPut("documents/{id}/verify")]
        [Authorize(Roles = "Administrator,Manager")]
        public async Task<ActionResult> VerifyDocument(int id)
        {
            var document = await _documentRepository.GetByIdAsync(id);

            if (document == null)
                return NotFound();

            document.IsVerified = true;
            document.VerifiedDate = DateTime.UtcNow;

            await _documentRepository.UpdateAsync(document);

            return Ok(new { message = "Document verified successfully" });
        }

        // GET: api/users/loyalty
        [HttpGet("loyalty")]
        public async Task<ActionResult<object>> GetMyLoyaltyProgram()
        {
            var userId = GetUserId();
            var loyaltyProgram = await _loyaltyRepository.GetUserLoyaltyProgramAsync(userId);

            if (loyaltyProgram == null)
                return NotFound();

            var transactions = await _loyaltyRepository.GetUserLoyaltyTransactionsAsync(userId);

            var transactionDtos = transactions.Select(t => new
            {
                Id = t.Id,
                Points = t.Points,
                TransactionType = t.TransactionType,
                Description = t.Description,
                TransactionDate = t.TransactionDate
            }).ToList();

            return Ok(new
            {
                Id = loyaltyProgram.Id,
                Points = loyaltyProgram.Points,
                Tier = loyaltyProgram.Tier,
                LastUpdated = loyaltyProgram.LastUpdated,
                Transactions = transactionDtos
            });
        }

        private int GetUserId()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.Name);

            if (userIdClaim == null)
                throw new InvalidOperationException("User ID claim not found");

            return int.Parse(userIdClaim.Value);
        }
    }
}