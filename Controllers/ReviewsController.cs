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
    public class ReviewsController : ControllerBase
    {
        private readonly IReviewRepository _reviewRepository;
        private readonly IVehicleRepository _vehicleRepository;
        private readonly IBookingRepository _bookingRepository;
        private readonly IUserRepository _userRepository;

        public ReviewsController(
            IReviewRepository reviewRepository,
            IVehicleRepository vehicleRepository,
            IBookingRepository bookingRepository,
            IUserRepository userRepository)
        {
            _reviewRepository = reviewRepository;
            _vehicleRepository = vehicleRepository;
            _bookingRepository = bookingRepository;
            _userRepository = userRepository;
        }

        // GET: api/reviews/vehicle/5
        [HttpGet("vehicle/{vehicleId}")]
        public async Task<ActionResult<List<ReviewResponseDto>>> GetVehicleReviews(int vehicleId)
        {
            var vehicle = await _vehicleRepository.GetByIdAsync(vehicleId);

            if (vehicle == null)
                return NotFound("Vehicle not found");

            var reviews = await _reviewRepository.GetVehicleReviewsAsync(vehicleId);

            var reviewDtos = new List<ReviewResponseDto>();
            foreach (var review in reviews)
            {
                reviewDtos.Add(new ReviewResponseDto
                {
                    Id = review.Id,
                    Rating = review.Rating,
                    Comment = review.Comment,
                    ReviewDate = review.ReviewDate,
                    UserName = $"{review.User.FirstName} {review.User.LastName}"
                });
            }

            return Ok(reviewDtos);
        }

        // GET: api/reviews/user
        [HttpGet("user")]
        [Authorize]
        public async Task<ActionResult<List<ReviewResponseDto>>> GetUserReviews()
        {
            var userId = GetUserId();

            var reviews = await _reviewRepository.GetUserReviewsAsync(userId);

            var reviewDtos = new List<ReviewResponseDto>();
            foreach (var review in reviews)
            {
                reviewDtos.Add(new ReviewResponseDto
                {
                    Id = review.Id,
                    Rating = review.Rating,
                    Comment = review.Comment,
                    ReviewDate = review.ReviewDate,
                    UserName = $"{review.User.FirstName} {review.User.LastName}"
                });
            }

            return Ok(reviewDtos);
        }

        // POST: api/reviews
        [HttpPost]
        [Authorize]
        public async Task<ActionResult<ReviewResponseDto>> CreateReview(ReviewCreateDto reviewDto)
        {
            try
            {
                var userId = GetUserId();

                var vehicle = await _vehicleRepository.GetByIdAsync(reviewDto.VehicleId);
                if (vehicle == null)
                    return NotFound("Vehicle not found");

                var userBookings = await _bookingRepository.GetUserBookingsAsync(userId);
                bool hasRentedVehicle = userBookings.Exists(b =>
                    b.VehicleId == reviewDto.VehicleId &&
                    b.Status == BookingStatus.Completed);

                if (!hasRentedVehicle)
                    return BadRequest("You can only review vehicles that you have rented");

                bool hasReviewed = await _reviewRepository.HasUserReviewedVehicleAsync(userId, reviewDto.VehicleId);
                if (hasReviewed)
                    return BadRequest("You have already reviewed this vehicle");

                var review = new Review
                {
                    UserId = userId,
                    VehicleId = reviewDto.VehicleId,
                    Rating = reviewDto.Rating,
                    Comment = reviewDto.Comment,
                    ReviewDate = DateTime.UtcNow
                };

                var createdReview = await _reviewRepository.AddAsync(review);

                var user = await _userRepository.GetByIdAsync(userId);

                var reviewResponseDto = new ReviewResponseDto
                {
                    Id = createdReview.Id,
                    Rating = createdReview.Rating,
                    Comment = createdReview.Comment,
                    ReviewDate = createdReview.ReviewDate,
                    UserName = $"{user.FirstName} {user.LastName}"
                };

                return CreatedAtAction(nameof(GetVehicleReviews), new { vehicleId = reviewDto.VehicleId }, reviewResponseDto);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        // DELETE: api/reviews/5
        [HttpDelete("{id}")]
        [Authorize]
        public async Task<ActionResult> DeleteReview(int id)
        {
            var review = await _reviewRepository.GetByIdAsync(id);

            if (review == null)
                return NotFound();

            var userId = GetUserId();

            if (review.UserId != userId && !User.IsInRole("Administrator"))
                return Forbid();

            await _reviewRepository.DeleteAsync(id);

            return NoContent();
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