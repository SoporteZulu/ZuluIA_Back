using MediatR;
using ZuluIA_Back.Domain.Common;

namespace ZuluIA_Back.Application.Features.PuntoVenta.Commands;

public record DesactivarTimbradoFiscalCommand(long Id, string? Observacion = null) : IRequest<Result>;
