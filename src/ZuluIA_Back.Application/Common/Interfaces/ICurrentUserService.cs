namespace ZuluIA_Back.Application.Common.Interfaces;

public interface ICurrentUserService
{
    long? UserId { get; }
    string? Email { get; }
    bool IsAuthenticated { get; }
}