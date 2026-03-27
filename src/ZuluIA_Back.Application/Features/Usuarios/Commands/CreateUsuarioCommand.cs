using MediatR;
using ZuluIA_Back.Domain.Common;

namespace ZuluIA_Back.Application.Features.Usuarios.Commands;

public record CreateUsuarioCommand(
    string UserName,
    string? NombreCompleto,
    string? Email,
    Guid? SupabaseUserId,
    string? Password,
    IReadOnlyList<long> SucursalIds
) : IRequest<Result<long>>;