using ConferenceHalls.Application.DTOs.HallService;
using ConferenceHalls.Domain.Entities;
using ConferenceHalls.Infrastructure.Persistence;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ConferenceHalls.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class HallServicesController(AppDbContext _context) : ControllerBase
    {
        [HttpGet]
        public async Task<ActionResult<IEnumerable<HallServiceDto>>> GetHallServices()
        {
            return await _context.HallServices.Select(h => new HallServiceDto
            {
                Id = h.Id,
                Name = h.Name,
                Price = h.Price
            }).ToListAsync();
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<HallServiceDto>> GetHallService(int id)
        {
            var hallService = await _context.HallServices.FindAsync(id);

            if (hallService == null)
            {
                return NotFound();
            }
            return new HallServiceDto
            {
                Id = hallService.Id,
                Name = hallService.Name,
                Price = hallService.Price
            };
        }

        [Authorize(Roles = "Admin")]
        [HttpPut("{id}")]
        public async Task<IActionResult> PutHallService(int id, CreateHallServiceDto dto)
        {
            var hallService = await _context.HallServices.FindAsync(id);

            if (hallService == null)
            {
                return NotFound();
            }
            hallService.Name = dto.Name;
            hallService.Price = dto.Price;

            await _context.SaveChangesAsync();

            return NoContent();
        }

        [Authorize(Roles = "Admin")]
        [HttpPost]
        public async Task<ActionResult<HallServiceDto>> PostHallService(CreateHallServiceDto dto)
        {
            var hallservice = new HallService
            {
                Name = dto.Name,
                Price = dto.Price
            };
            _context.HallServices.Add(hallservice);
            await _context.SaveChangesAsync();
            var response = new HallServiceDto
            {
                Id = hallservice.Id,
                Name = hallservice.Name,
                Price = hallservice.Price
            };

            return CreatedAtAction(
                nameof(GetHallService),
                new { id = hallservice.Id },
                response);
        }

        [Authorize(Roles = "Admin")]
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteHallService(int id)
        {
            var hallService = await _context.HallServices.FindAsync(id);
            if (hallService == null)
            {
                return NotFound();
            }

            _context.HallServices.Remove(hallService);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}