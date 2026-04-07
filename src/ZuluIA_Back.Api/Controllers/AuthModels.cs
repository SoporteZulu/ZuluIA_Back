using System.Text.Json.Serialization;

namespace ZuluIA_Back.Api.Controllers;

public sealed record AuthLoginRequest(
    [property: JsonPropertyName("user_name")] string? UserName,
    [property: JsonPropertyName("email")] string? Email,
    [property: JsonPropertyName("password")] string Password);

public sealed record AuthUserResponse(
    [property: JsonPropertyName("id")] long Id,
    [property: JsonPropertyName("user_name")] string UserName,
    [property: JsonPropertyName("email")] string? Email,
    [property: JsonPropertyName("nombre_completo")] string? NombreCompleto,
    [property: JsonPropertyName("activo")] bool Activo);

public sealed record AuthTokenResponse(
    [property: JsonPropertyName("access_token")] string AccessToken,
    [property: JsonPropertyName("token_type")] string TokenType,
    [property: JsonPropertyName("expires_in")] int ExpiresIn,
    [property: JsonPropertyName("expires_at")] long ExpiresAt,
    [property: JsonPropertyName("refresh_token")] string? RefreshToken,
    [property: JsonPropertyName("user")] AuthUserResponse User);
