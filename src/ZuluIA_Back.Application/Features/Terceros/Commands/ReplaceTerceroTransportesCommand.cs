using MediatR;
using ZuluIA_Back.Application.Features.Terceros.DTOs;
using ZuluIA_Back.Domain.Common;

namespace ZuluIA_Back.Application.Features.Terceros.Commands;

public record ReplaceTerceroTransportesCommand(
    long TerceroId,
    IReadOnlyList<ReplaceTerceroTransporteItem> Transportes) : IRequest<Result<IReadOnlyList<TerceroTransporteDto>>>;

public record ReplaceTerceroTransporteItem(
    long? Id,
    long? TransportistaId,
    string Nombre,
    string? Servicio,
    string? Zona,
    string? Frecuencia,
    string? Observacion,
    bool Activo,
    bool Principal,
    int? Orden);
