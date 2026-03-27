using MediatR;
using ZuluIA_Back.Domain.Common;

namespace ZuluIA_Back.Application.Features.Colegio.Commands;

public record RegistrarCobinproColegioCommand(long CedulonId, DateOnly Fecha, decimal Importe, string ReferenciaExterna, long CajaId, long FormaPagoId, long MonedaId, decimal Cotizacion, string? Observacion) : IRequest<Result<long>>;
