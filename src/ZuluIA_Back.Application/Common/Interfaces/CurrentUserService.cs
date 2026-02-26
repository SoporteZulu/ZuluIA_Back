namespace ZuluIA_Back.Application.Common.Interfaces;

public class CurrentUserService : ICurrentUserService
{
    public long? UserId { get; private set; }
    public string? Email { get; private set; }
    public bool IsAuthenticated => UserId.HasValue;

    public void SetUser(string? userId, string? email)
    {
        if (long.TryParse(userId, out var id))
            UserId = id;

        Email = email;
    }
}