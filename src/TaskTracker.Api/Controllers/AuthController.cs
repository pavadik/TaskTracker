using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TaskTracker.Api.Services;
using TaskTracker.Domain.Entities;
using TaskTracker.Domain.Repositories;
using TaskTracker.Domain.ValueObjects;
using UserEntity = TaskTracker.Domain.Entities.User;

namespace TaskTracker.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IUserRepository _userRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IJwtTokenService _jwtTokenService;
    private readonly ILogger<AuthController> _logger;

    public AuthController(
        IUserRepository userRepository,
        IUnitOfWork unitOfWork,
        IJwtTokenService jwtTokenService,
        ILogger<AuthController> logger)
    {
        _userRepository = userRepository;
        _unitOfWork = unitOfWork;
        _jwtTokenService = jwtTokenService;
        _logger = logger;
    }

    /// <summary>
    /// Register a new user
    /// </summary>
    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterRequest request, CancellationToken cancellationToken)
    {
        var emailResult = Email.Create(request.Email);
        if (!emailResult.IsSuccess)
            return BadRequest(emailResult.Error);

        var existingUser = await _userRepository.GetByEmailAsync(emailResult.Value, cancellationToken);
        if (existingUser != null)
            return BadRequest("Email already registered");

        // In production, use proper password hashing (e.g., BCrypt, Argon2)
        var passwordHash = BCrypt.Net.BCrypt.HashPassword(request.Password);

        var userResult = UserEntity.Create(
            emailResult.Value,
            passwordHash,
            request.DisplayName);

        if (!userResult.IsSuccess)
            return BadRequest(userResult.Error);

        var user = userResult.Value;
        await _userRepository.AddAsync(user, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        var accessToken = _jwtTokenService.GenerateAccessToken(user);
        var refreshToken = _jwtTokenService.GenerateRefreshToken();

        _logger.LogInformation("User registered: {Email}", request.Email);

        return Ok(new AuthResponse(accessToken, refreshToken, DateTime.UtcNow.AddHours(1)));
    }

    /// <summary>
    /// Login with email and password
    /// </summary>
    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest request, CancellationToken cancellationToken)
    {
        var emailResult = Email.Create(request.Email);
        if (!emailResult.IsSuccess)
            return BadRequest("Invalid email format");

        var user = await _userRepository.GetByEmailAsync(emailResult.Value, cancellationToken);
        if (user == null)
            return Unauthorized("Invalid credentials");

        if (!BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
            return Unauthorized("Invalid credentials");

        var accessToken = _jwtTokenService.GenerateAccessToken(user);
        var refreshToken = _jwtTokenService.GenerateRefreshToken();

        _logger.LogInformation("User logged in: {Email}", request.Email);

        return Ok(new AuthResponse(accessToken, refreshToken, DateTime.UtcNow.AddHours(1)));
    }

    /// <summary>
    /// Get current user profile
    /// </summary>
    [HttpGet("me")]
    [Authorize]
    public async Task<IActionResult> GetCurrentUser(CancellationToken cancellationToken)
    {
        var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier);
        if (userIdClaim == null || !Guid.TryParse(userIdClaim.Value, out var userId))
            return Unauthorized();

        var user = await _userRepository.GetByIdAsync(userId, cancellationToken);
        if (user == null)
            return NotFound();

        return Ok(new UserProfileResponse(
            user.Id,
            user.Email.Value,
            user.DisplayName,
            user.AvatarUrl,
            user.CreatedAt));
    }

    /// <summary>
    /// Refresh access token
    /// </summary>
    [HttpPost("refresh")]
    public async Task<IActionResult> RefreshToken([FromBody] RefreshTokenRequest request, CancellationToken cancellationToken)
    {
        var principal = _jwtTokenService.GetPrincipalFromExpiredToken(request.AccessToken);
        if (principal == null)
            return Unauthorized("Invalid token");

        var userIdClaim = principal.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier);
        if (userIdClaim == null || !Guid.TryParse(userIdClaim.Value, out var userId))
            return Unauthorized("Invalid token");

        var user = await _userRepository.GetByIdAsync(userId, cancellationToken);
        if (user == null)
            return Unauthorized("User not found");

        var newAccessToken = _jwtTokenService.GenerateAccessToken(user);
        var newRefreshToken = _jwtTokenService.GenerateRefreshToken();

        return Ok(new AuthResponse(newAccessToken, newRefreshToken, DateTime.UtcNow.AddHours(1)));
    }
}

public record RegisterRequest(string Email, string Password, string DisplayName);
public record LoginRequest(string Email, string Password);
public record RefreshTokenRequest(string AccessToken, string RefreshToken);
public record AuthResponse(string AccessToken, string RefreshToken, DateTime ExpiresAt);
public record UserProfileResponse(Guid Id, string Email, string DisplayName, string? AvatarUrl, DateTime CreatedAt);
