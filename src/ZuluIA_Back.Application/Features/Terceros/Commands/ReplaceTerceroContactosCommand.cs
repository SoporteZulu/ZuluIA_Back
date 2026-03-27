using MediatR;
using ZuluIA_Back.Application.Features.Terceros.DTOs;
using ZuluIA_Back.Domain.Common;

namespace ZuluIA_Back.Application.Features.Terceros.Commands;

public record ReplaceTerceroContactosCommand(
    long TerceroId,
    IReadOnlyList<ReplaceTerceroContactoItem> Contactos) : IRequest<Result<IReadOnlyList<TerceroContactoDto>>>;

public record ReplaceTerceroContactoItem(
    long? Id,
    string Nombre,
    string? Cargo,
    string? Email,
    string? Telefono,
    string? Sector,
    bool Principal,
    int? Orden);
