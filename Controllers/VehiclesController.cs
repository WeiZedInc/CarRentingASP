using CarRentalSystem.DTOs;
using CarRentalSystem.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CarRentalSystem.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class VehiclesController : ControllerBase
    {
        private readonly IVehicleService _vehicleService;

        public VehiclesController(IVehicleService vehicleService)
        {
            _vehicleService = vehicleService;
        }

        // GET: api/vehicles
        [HttpGet]
        public async Task<ActionResult<List<VehicleDto>>> GetAllVehicles()
        {
            var vehicles = await _vehicleService.GetAllVehiclesAsync();
            return Ok(vehicles);
        }

        // GET: api/vehicles/5
        [HttpGet("{id}")]
        public async Task<ActionResult<VehicleDto>> GetVehicle(int id)
        {
            var vehicle = await _vehicleService.GetVehicleByIdAsync(id);

            if (vehicle == null)
                return NotFound();

            return Ok(vehicle);
        }

        // GET: api/vehicles/available?startDate=2023-01-01&endDate=2023-01-05
        [HttpGet("available")]
        public async Task<ActionResult<List<VehicleDto>>> GetAvailableVehicles(
            [FromQuery] DateTime startDate,
            [FromQuery] DateTime endDate)
        {
            if (startDate >= endDate)
                return BadRequest("End date must be after start date");

            var vehicles = await _vehicleService.GetAvailableVehiclesAsync(startDate, endDate);
            return Ok(vehicles);
        }

        // POST: api/vehicles
        [HttpPost]
        [Authorize(Roles = "Administrator,Manager")]
        public async Task<ActionResult<VehicleDto>> CreateVehicle(VehicleCreateDto vehicleDto)
        {
            try
            {
                var vehicle = await _vehicleService.CreateVehicleAsync(vehicleDto);
                return CreatedAtAction(nameof(GetVehicle), new { id = vehicle.Id }, vehicle);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        // PUT: api/vehicles/5
        [HttpPut("{id}")]
        [Authorize(Roles = "Administrator,Manager")]
        public async Task<ActionResult<VehicleDto>> UpdateVehicle(int id, VehicleUpdateDto vehicleDto)
        {
            try
            {
                var vehicle = await _vehicleService.UpdateVehicleAsync(id, vehicleDto);

                if (vehicle == null)
                    return NotFound();

                return Ok(vehicle);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        // DELETE: api/vehicles/5
        [HttpDelete("{id}")]
        [Authorize(Roles = "Administrator")]
        public async Task<ActionResult> DeleteVehicle(int id)
        {
            var vehicle = await _vehicleService.GetVehicleByIdAsync(id);

            if (vehicle == null)
                return NotFound();

            await _vehicleService.DeleteVehicleAsync(id);
            return NoContent();
        }

        // GET: api/vehicles/category/5
        [HttpGet("category/{categoryId}")]
        public async Task<ActionResult<List<VehicleDto>>> GetVehiclesByCategory(int categoryId)
        {
            var vehicles = await _vehicleService.GetVehiclesByCategoryAsync(categoryId);
            return Ok(vehicles);
        }

        // GET: api/vehicles/5/reviews
        [HttpGet("{id}/reviews")]
        public async Task<ActionResult<List<ReviewResponseDto>>> GetVehicleReviews(int id)
        {
            var vehicle = await _vehicleService.GetVehicleByIdAsync(id);

            if (vehicle == null)
                return NotFound();

            var reviews = await _vehicleService.GetVehicleReviewsAsync(id);
            return Ok(reviews);
        }

        // GET: api/vehicles/search?query=toyota
        [HttpGet("search")]
        public async Task<ActionResult<List<VehicleDto>>> SearchVehicles([FromQuery] string query)
        {
            var vehicles = await _vehicleService.SearchVehiclesAsync(query);
            return Ok(vehicles);
        }

        // GET: api/vehicles/paginated?pageNumber=1&pageSize=10
        [HttpGet("paginated")]
        public async Task<ActionResult<PaginatedResponse<VehicleDto>>> GetPaginatedVehicles(
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 10)
        {
            if (pageNumber < 1)
                return BadRequest("Page number must be greater than 0");

            if (pageSize < 1 || pageSize > 50)
                return BadRequest("Page size must be between 1 and 50");

            var vehicles = await _vehicleService.GetPaginatedVehiclesAsync(pageNumber, pageSize);
            return Ok(vehicles);
        }
    }
}