using MediatR;
using ZuluIA_Back.Domain.Common;

namespace ZuluIA_Back.Application.Features.Usuarios.Commands;

public record UpdateUsuarioCommand(
    long Id,
    string? NombreCompleto,
    string? Email,
    IReadOnlyList<long> SucursalIds
) : IRequest<Result>;