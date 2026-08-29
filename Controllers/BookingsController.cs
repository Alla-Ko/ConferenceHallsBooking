using ConferenceHalls.Application.DTOs.Booking;
using ConferenceHalls.Application.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace ConferenceHalls.Controllers;

[Authorize]
[Route("api/[controller]")]
[ApiController]
public class BookingsController(IBookingsService bookingsService) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IEnumerable<BookingDto>>> GetBookings()
    {
        var userId = GetUserId();
        if (userId is null)
        {
            return Unauthorized();
        }

        return Ok(await bookingsService.GetAllAsync(userId, IsAdmin()));
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<BookingDto>> GetBooking(int id)
    {
        var userId = GetUserId();
        if (userId is null)
        {
            return Unauthorized();
        }

        var booking = await bookingsService.GetByIdAsync(id, userId, IsAdmin());
        if (booking is null)
        {
            return NotFound();
        }

        return booking;
    }

    [HttpPost]
    public async Task<ActionResult<BookingDto>> PostBooking(CreateBookingDto dto)
    {
        var userId = GetUserId();
        if (userId is null)
        {
            return Unauthorized();
        }

        try
        {
            var booking = await bookingsService.CreateAsync(dto, userId);
            return CreatedAtAction(nameof(GetBooking), new { id = booking.Id }, booking);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPatch("{id}/status")]
    public async Task<ActionResult<BookingDto>> PatchBookingStatus(int id, UpdateBookingStatusDto dto)
    {
        var userId = GetUserId();
        if (userId is null)
        {
            return Unauthorized();
        }

        var result = await bookingsService.UpdateStatusAsync(id, dto, userId, IsAdmin());
        if (result.Booking is null)
        {
            return StatusCode(result.HttpStatusCode, result.Error is null
                ? null
                : new { message = result.Error });
        }

        return StatusCode(result.HttpStatusCode, result.Booking);
    }

    private string? GetUserId() => User.FindFirstValue(ClaimTypes.NameIdentifier);

    private bool IsAdmin() => User.IsInRole("Admin");
}
