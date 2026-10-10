public sealed record LoginResponse(
    Guid UserId,
    Guid BusinessId,
    string Name,
    string Email,
    string Role,
    string AccessToken,
    DateTime AccessTokenExpiresAtUtc,
    string RefreshToken,
    DateTime RefreshTokenExpiresAtUtc);