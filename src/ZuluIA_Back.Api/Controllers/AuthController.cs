using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using ZuluIA_Back.Application.Common.Interfaces;
using ZuluIA_Back.Domain.Interfaces;

namespace ZuluIA_Back.Api.Controllers;

public class AuthController(
    IMediator mediator,
    ICurrentUserService currentUser,
    IUsuarioRepository usuarioRepository,
    IPasswordHasherService passwordHasher,
    IConfiguration configuration)
    : BaseController(mediator)
{
    [HttpPost("login")]
    [HttpPost("token")]
    [AllowAnonymous]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Login([FromBody] AuthLoginRequest request, CancellationToken ct)
    {
        var login = request.UserName ?? request.Email;
        if (string.IsNullOrWhiteSpace(login) || string.IsNullOrWhiteSpace(request.Password))
            return BadRequest(new { error = "invalid_request", error_description = "Usuario/email y contraseña son obligatorios." });

        var user = await usuarioRepository.GetByUserNameOrEmailAsync(login, ct);
        if (user is null || !user.Activo || user.DeletedAt != null || string.IsNullOrWhiteSpace(user.PasswordHash) || !passwordHasher.VerifyPassword(request.Password, user.PasswordHash))
            return Unauthorized(new { error = "invalid_grant", error_description = "Credenciales inválidas." });

        var issuer = configuration["Jwt:Issuer"] ?? throw new InvalidOperationException("Jwt:Issuer no configurado.");
        var audience = configuration["Jwt:Audience"] ?? throw new InvalidOperationException("Jwt:Audience no configurado.");
        var jwtSecret = Environment.GetEnvironmentVariable("SUPABASE_JWT_SECRET")
                        ?? configuration["Supabase:JwtSecret"]
                        ?? throw new InvalidOperationException("Supabase:JwtSecret no configurado.");

        const int expiresInSeconds = 8 * 60 * 60;
        var now = DateTimeOffset.UtcNow;
        var expiresAt = now.AddSeconds(expiresInSeconds);
        var credentials = new SigningCredentials(
            new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSecret)),
            SecurityAlgorithms.HmacSha256);

        var claims = new List<Claim>
        {
            new System.Security.Claims.Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new System.Security.Claims.Claim(JwtRegisteredClaimNames.Email, user.Email ?? string.Empty),
            new System.Security.Claims.Claim("email", user.Email ?? string.Empty),
            new System.Security.Claims.Claim("role", "authenticated"),
            new System.Security.Claims.Claim(JwtRegisteredClaimNames.UniqueName, user.UserName),
            new System.Security.Claims.Claim("preferred_username", user.UserName)
        };

        var token = new JwtSecurityToken(
            issuer: issuer,
            audience: audience,
            claims: claims,
            notBefore: now.UtcDateTime,
            expires: expiresAt.UtcDateTime,
            signingCredentials: credentials);

        var accessToken = new JwtSecurityTokenHandler().WriteToken(token);

        return Ok(new AuthTokenResponse(
            accessToken,
            "bearer",
            expiresInSeconds,
            expiresAt.ToUnixTimeSeconds(),
            null,
            new AuthUserResponse(user.Id, user.UserName, user.Email, user.NombreCompleto, user.Activo)));
    }

    [HttpGet("me")]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Me(CancellationToken ct)
    {
        if (!currentUser.IsAuthenticated || !currentUser.UserId.HasValue)
            return Unauthorized();

        var user = await usuarioRepository.GetByIdAsync(currentUser.UserId.Value, ct);
        if (user is null || !user.Activo || user.DeletedAt != null)
            return Unauthorized();

        return Ok(new AuthUserResponse(user.Id, user.UserName, user.Email, user.NombreCompleto, user.Activo));
    }

    [HttpGet("ping")]
    [AllowAnonymous]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public IActionResult Ping() =>
        Ok(new { message = "ZuluIA_Back API funcionando", auth = "local-jwt", timestamp = DateTime.UtcNow });
}