using ConferenceHalls.Application.DTOs.ConferenceHall;
using ConferenceHalls.Application.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ConferenceHalls.Controllers;

[Route("api/[controller]")]
[ApiController]
public class ConferenceHallsController(IConferenceHallsService conferenceHallsService) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IEnumerable<ConferenceHallDto>>> GetConferenceHalls()
    {
        return Ok(await conferenceHallsService.GetAllAsync());
    }

    [HttpGet("available")]
    public async Task<ActionResult<IEnumerable<ConferenceHallDto>>> GetAvailableConferenceHalls(
        [FromQuery] DateOnly date,
        [FromQuery] int startHour,
        [FromQuery] int endHour,
        [FromQuery] int capacity)
    {
        try
        {
            var halls = await conferenceHallsService.GetAvailableAsync(
                date,
                startHour,
                endHour,
                capacity);
            return Ok(halls);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<ConferenceHallDto>> GetConferenceHall(int id)
    {
        var hall = await conferenceHallsService.GetByIdAsync(id);
        if (hall is null)
        {
            return NotFound();
        }

        return hall;
    }

    [Authorize(Roles = "Admin")]
    [HttpPost]
    public async Task<ActionResult<ConferenceHallDto>> PostConferenceHall(CreateConferenceHallDto dto)
    {
        try
        {
            var hall = await conferenceHallsService.CreateAsync(dto);
            return CreatedAtAction(nameof(GetConferenceHall), new { id = hall.Id }, hall);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [Authorize(Roles = "Admin")]
    [HttpPut("{id}")]
    public async Task<ActionResult<ConferenceHallDto>> PutConferenceHall(int id, UpdateConferenceHallDto dto)
    {
        try
        {
            var hall = await conferenceHallsService.UpdateAsync(id, dto);
            if (hall is null)
            {
                return NotFound();
            }

            return hall;
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [Authorize(Roles = "Admin")]
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteConferenceHall(int id)
    {
        var deleted = await conferenceHallsService.DeleteAsync(id);
        if (!deleted)
        {
            return NotFound();
        }

        return NoContent();
    }
}
