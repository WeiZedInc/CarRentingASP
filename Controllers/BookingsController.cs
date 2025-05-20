using CarRentalSystem.DTOs;
using CarRentalSystem.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace CarRentalSystem.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class BookingsController : ControllerBase
    {
        private readonly IBookingService _bookingService;

        public BookingsController(IBookingService bookingService)
        {
            _bookingService = bookingService;
        }

        // GET: api/bookings
        [HttpGet]
        [Authorize(Roles = "Administrator,Manager")]
        public async Task<ActionResult<PaginatedResponse<BookingResponseDto>>> GetAllBookings(
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 10)
        {
            if (pageNumber < 1)
                return BadRequest("Page number must be greater than 0");

            if (pageSize < 1 || pageSize > 50)
                return BadRequest("Page size must be between 1 and 50");

            var bookings = await _bookingService.GetPaginatedBookingsAsync(pageNumber, pageSize);
            return Ok(bookings);
        }

        // GET: api/bookings/5
        [HttpGet("{id}")]
        public async Task<ActionResult<BookingResponseDto>> GetBooking(int id)
        {
            var booking = await _bookingService.GetBookingByIdAsync(id);

            if (booking == null)
                return NotFound();

            // Regular users can only view their own bookings
            if (User.IsInRole("Customer") && booking.User.Id != GetUserId())
                return Forbid();

            return Ok(booking);
        }

        // POST: api/bookings
        [HttpPost]
        public async Task<ActionResult<BookingResponseDto>> CreateBooking(BookingCreateDto bookingDto)
        {
            try
            {
                var userId = GetUserId();
                var booking = await _bookingService.CreateBookingAsync(userId, bookingDto);
                return CreatedAtAction(nameof(GetBooking), new { id = booking.Id }, booking);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        // GET: api/bookings/my
        [HttpGet("my")]
        public async Task<ActionResult<List<BookingResponseDto>>> GetMyBookings()
        {
            var userId = GetUserId();
            var bookings = await _bookingService.GetUserBookingsAsync(userId);
            return Ok(bookings);
        }

        // PUT: api/bookings/5/status
        [HttpPut("{id}/status")]
        [Authorize(Roles = "Administrator,Manager")]
        public async Task<ActionResult<BookingResponseDto>> UpdateBookingStatus(int id, [FromBody] string status)
        {
            try
            {
                var booking = await _bookingService.UpdateBookingStatusAsync(id, status);

                if (booking == null)
                    return NotFound();

                return Ok(booking);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        // PUT: api/bookings/5/extend
        [HttpPut("{id}/extend")]
        public async Task<ActionResult<BookingResponseDto>> ExtendBooking(int id, [FromBody] DateTime newReturnDate)
        {
            try
            {
                var booking = await _bookingService.GetBookingByIdAsync(id);

                if (booking == null)
                    return NotFound();

                // Regular users can only extend their own bookings
                if (User.IsInRole("Customer") && booking.User.Id != GetUserId())
                    return Forbid();

                var updatedBooking = await _bookingService.ExtendBookingAsync(id, newReturnDate);
                return Ok(updatedBooking);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        // GET: api/bookings/pending
        [HttpGet("pending")]
        [Authorize(Roles = "Administrator,Manager")]
        public async Task<ActionResult<List<BookingResponseDto>>> GetPendingBookings()
        {
            var bookings = await _bookingService.GetPendingBookingsAsync();
            return Ok(bookings);
        }

        // PUT: api/bookings/5/cancel
        [HttpPut("{id}/cancel")]
        public async Task<ActionResult<BookingResponseDto>> CancelBooking(int id)
        {
            try
            {
                var booking = await _bookingService.GetBookingByIdAsync(id);

                if (booking == null)
                    return NotFound();

                // Regular users can only cancel their own bookings
                if (User.IsInRole("Customer") && booking.User.Id != GetUserId())
                    return Forbid();

                var cancelledBooking = await _bookingService.CancelBookingAsync(id);
                return Ok(cancelledBooking);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ex.Message);
            }
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