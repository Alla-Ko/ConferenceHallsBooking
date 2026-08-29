using ConferenceHalls.Application.DTOs;
using ConferenceHalls.Domain.Entities;
using ConferenceHalls.Infrastructure.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using System.IdentityModel.Tokens.Jwt;
using ConferenceHalls.Application.DTOs.Auth;

namespace ConferenceHalls.Controllers;

[Route("api/[controller]")]
[ApiController]
public class AuthController(UserManager<ApplicationUser> userManager, JwtTokenService jwtTokenService) : ControllerBase
{
    [HttpPost("register")]
    public async Task<IActionResult> Register(RegisterUserDto dto)
    {
        var existingUser = await userManager.FindByEmailAsync(dto.Email);

        if (existingUser != null)
        {
            return Conflict(new
            {
                message = "User with this email already exists."
            });
        }

        var user = new ApplicationUser
        {
            UserName = dto.Email,
            Email = dto.Email,
            FullName = dto.FullName,
            PhoneNumber = dto.Phone
        };

        var result = await userManager.CreateAsync(user, dto.Password);

        if (!result.Succeeded)
        {
            return BadRequest(new
            {
                message = "User registration failed.",
                errors = result.Errors.Select(e => e.Description)
            });
        }

        var roleResult = await userManager.AddToRoleAsync(user, "User");

        if (!roleResult.Succeeded)
        {
            return BadRequest(new
            {
                message = "Failed to assign user role.",
                errors = roleResult.Errors.Select(e => e.Description)
            });
        }

        var response = new RegisterResponseDto
        {
            Id = user.Id,
            Email = user.Email!,
            FullName = user.FullName,
            Phone = user.PhoneNumber
        };

        return Created("", response);
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login(LoginDto dto)
    {
        var user = await userManager.FindByEmailAsync(dto.Email);
        if (user == null || !await userManager.CheckPasswordAsync(user, dto.Password))
        {
            return Unauthorized(new
            {
                message = "Invalid email or password."
            });
        }
        var token = await jwtTokenService.GenerateTokenAsync(user);
        var response = new LoginResponseDto
        {
            Token = token
        };

        return Ok(response);
    }

    [Authorize]
    [HttpGet("me")]
    public async Task<IActionResult> GetCurrentUser()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (userId == null)
        {
            return Unauthorized();
        }

        var user = await userManager.FindByIdAsync(userId);

        if (user == null)
        {
            return Unauthorized();
        }
        var phone = user.PhoneNumber ?? string.Empty;
        var response = new RegisterResponseDto
        {
            Id = user.Id,
            Email = user.Email!,
            FullName = user.FullName,
            Phone = phone
        };

        return Ok(response);
    }
}