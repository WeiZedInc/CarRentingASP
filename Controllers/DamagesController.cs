using CarRentalSystem.DTOs;
using CarRentalSystem.Models;
using CarRentalSystem.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CarRentalSystem.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "Administrator,Manager")]
    public class DamagesController : ControllerBase
    {
        private readonly IDamageReportRepository _damageReportRepository;
        private readonly IBookingRepository _bookingRepository;

        public DamagesController(
            IDamageReportRepository damageReportRepository,
            IBookingRepository bookingRepository)
        {
            _damageReportRepository = damageReportRepository;
            _bookingRepository = bookingRepository;
        }

        // GET: api/damages
        [HttpGet]
        public async Task<ActionResult<IEnumerable<DamageReportResponseDto>>> GetAllDamageReports()
        {
            var damageReports = await _damageReportRepository.GetAllAsync();
            var damageReportDtos = new List<DamageReportResponseDto>();

            foreach (var report in damageReports)
            {
                damageReportDtos.Add(new DamageReportResponseDto
                {
                    Id = report.Id,
                    Description = report.Description,
                    RepairCost = report.RepairCost,
                    IsCustomerResponsible = report.IsCustomerResponsible,
                    ReportDate = report.ReportDate,
                    ImageUrl = report.ImageUrl,
                    BookingId = report.BookingId
                });
            }

            return Ok(damageReportDtos);
        }

        // GET: api/damages/5
        [HttpGet("{id}")]
        public async Task<ActionResult<DamageReportResponseDto>> GetDamageReport(int id)
        {
            var damageReport = await _damageReportRepository.GetByIdAsync(id);

            if (damageReport == null)
                return NotFound();

            var damageReportDto = new DamageReportResponseDto
            {
                Id = damageReport.Id,
                Description = damageReport.Description,
                RepairCost = damageReport.RepairCost,
                IsCustomerResponsible = damageReport.IsCustomerResponsible,
                ReportDate = damageReport.ReportDate,
                ImageUrl = damageReport.ImageUrl,
                BookingId = damageReport.BookingId
            };

            return Ok(damageReportDto);
        }

        // GET: api/damages/booking/5
        [HttpGet("booking/{bookingId}")]
        public async Task<ActionResult<DamageReportResponseDto>> GetDamageReportByBooking(int bookingId)
        {
            var damageReport = await _damageReportRepository.GetDamageReportByBookingIdAsync(bookingId);

            if (damageReport == null)
                return NotFound();

            var damageReportDto = new DamageReportResponseDto
            {
                Id = damageReport.Id,
                Description = damageReport.Description,
                RepairCost = damageReport.RepairCost,
                IsCustomerResponsible = damageReport.IsCustomerResponsible,
                ReportDate = damageReport.ReportDate,
                ImageUrl = damageReport.ImageUrl,
                BookingId = damageReport.BookingId
            };

            return Ok(damageReportDto);
        }

        // POST: api/damages
        [HttpPost]
        public async Task<ActionResult<DamageReportResponseDto>> CreateDamageReport(DamageReportCreateDto damageReportDto)
        {
            try
            {
                // Check if booking exists
                var booking = await _bookingRepository.GetByIdAsync(damageReportDto.BookingId);
                if (booking == null)
                    return NotFound("Booking not found");

                var existingReport = await _damageReportRepository.GetDamageReportByBookingIdAsync(damageReportDto.BookingId);
                if (existingReport != null)
                    return BadRequest("A damage report already exists for this booking");

                // Create new damage report
                var damageReport = new DamageReport
                {
                    BookingId = damageReportDto.BookingId,
                    Description = damageReportDto.Description,
                    RepairCost = damageReportDto.RepairCost,
                    IsCustomerResponsible = damageReportDto.IsCustomerResponsible,
                    ReportDate = DateTime.UtcNow,
                    ImageUrl = damageReportDto.ImageUrl
                };

                var createdReport = await _damageReportRepository.AddAsync(damageReport);

                var createdReportDto = new DamageReportResponseDto
                {
                    Id = createdReport.Id,
                    Description = createdReport.Description,
                    RepairCost = createdReport.RepairCost,
                    IsCustomerResponsible = createdReport.IsCustomerResponsible,
                    ReportDate = createdReport.ReportDate,
                    ImageUrl = createdReport.ImageUrl,
                    BookingId = createdReport.BookingId
                };

                return CreatedAtAction(nameof(GetDamageReport), new { id = createdReport.Id }, createdReportDto);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        // PUT: api/damages/5
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateDamageReport(int id, DamageReportCreateDto damageReportDto)
        {
            var damageReport = await _damageReportRepository.GetByIdAsync(id);

            if (damageReport == null)
                return NotFound();

            // Update properties
            damageReport.Description = damageReportDto.Description;
            damageReport.RepairCost = damageReportDto.RepairCost;
            damageReport.IsCustomerResponsible = damageReportDto.IsCustomerResponsible;
            damageReport.ImageUrl = damageReportDto.ImageUrl;

            await _damageReportRepository.UpdateAsync(damageReport);

            return NoContent();
        }

        // DELETE: api/damages/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteDamageReport(int id)
        {
            var damageReport = await _damageReportRepository.GetByIdAsync(id);

            if (damageReport == null)
                return NotFound();

            await _damageReportRepository.DeleteAsync(id);

            return NoContent();
        }
    }
}