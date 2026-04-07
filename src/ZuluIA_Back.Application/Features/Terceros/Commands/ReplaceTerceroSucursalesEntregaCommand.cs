using MediatR;
using ZuluIA_Back.Application.Features.Terceros.DTOs;
using ZuluIA_Back.Domain.Common;

namespace ZuluIA_Back.Application.Features.Terceros.Commands;

public record ReplaceTerceroSucursalesEntregaCommand(
    long TerceroId,
    IReadOnlyList<ReplaceTerceroSucursalEntregaItem> Sucursales) : IRequest<Result<IReadOnlyList<TerceroSucursalEntregaDto>>>;

public record ReplaceTerceroSucursalEntregaItem(
    long? Id,
    string Descripcion,
    string? Direccion,
    string? Localidad,
    string? Responsable,
    string? Telefono,
    string? Horario,
    bool Principal,
    int? Orden);
