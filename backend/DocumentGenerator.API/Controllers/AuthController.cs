using DocumentGenerator.Business.Interfaces;
using DocumentGenerator.Contracts.Common;
using DocumentGenerator.Contracts.Models.Auth;
using DocumentGenerator.Contracts.Resources.Auth;
using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace DocumentGenerator.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;
    private readonly IValidator<LoginModel> _loginValidator;
    private readonly IValidator<RefreshTokenModel> _refreshValidator;

    public AuthController(
        IAuthService authService,
        IValidator<LoginModel> loginValidator,
        IValidator<RefreshTokenModel> refreshValidator)
    {
        _authService = authService;
        _loginValidator = loginValidator;
        _refreshValidator = refreshValidator;
    }

    [HttpPost("login")]
    public async Task<ActionResult<ApiResponse<AuthResource>>> Login([FromBody] LoginModel model)
    {
        try
        {
            var validation = await _loginValidator.ValidateAsync(model);
            if (!validation.IsValid)
            {
                return BadRequest(ApiResponse<AuthResource>.ErrorResponse(
                    "Validation failed",
                    validation.Errors.Select(e => e.ErrorMessage).ToList()));
            }

            var result = await _authService.LoginAsync(model);
            return Ok(ApiResponse<AuthResource>.SuccessResponse(result, "Login successful"));
        }
        catch (UnauthorizedAccessException ex)
        {
            return Unauthorized(ApiResponse<AuthResource>.ErrorResponse(ex.Message));
        }
    }

    [HttpPost("refresh")]
    public async Task<ActionResult<ApiResponse<AuthResource>>> Refresh([FromBody] RefreshTokenModel model)
    {
        try
        {
            var validation = await _refreshValidator.ValidateAsync(model);
            if (!validation.IsValid)
            {
                return BadRequest(ApiResponse<AuthResource>.ErrorResponse(
                    "Validation failed",
                    validation.Errors.Select(e => e.ErrorMessage).ToList()));
            }

            var result = await _authService.RefreshTokenAsync(model);
            return Ok(ApiResponse<AuthResource>.SuccessResponse(result, "Token refreshed"));
        }
        catch (UnauthorizedAccessException ex)
        {
            return Unauthorized(ApiResponse<AuthResource>.ErrorResponse(ex.Message));
        }
    }

    [Authorize]
    [HttpGet("me")]
    public async Task<ActionResult<ApiResponse<UserResource>>> GetCurrentUser()
    {
        try
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
            {
                return Unauthorized(ApiResponse<UserResource>.ErrorResponse("Invalid token"));
            }

            var result = await _authService.GetUserByIdAsync(userId);
            return Ok(ApiResponse<UserResource>.SuccessResponse(result));
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ApiResponse<UserResource>.ErrorResponse(ex.Message));
        }
    }
}