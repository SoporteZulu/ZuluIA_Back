using MediatR;
using ZuluIA_Back.Domain.Common;

namespace ZuluIA_Back.Application.Features.Colegio.Commands;

public record FacturarCedulonesColegioAutomaticoCommand(long? LoteId, long? SucursalId, DateOnly? HastaVencimiento, long? PuntoFacturacionId, DateOnly Fecha, DateOnly? FechaVencimiento, string? Observacion) : IRequest<Result<IReadOnlyList<long>>>;
