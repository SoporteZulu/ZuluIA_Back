using MediatR;
using ZuluIA_Back.Domain.Common;

namespace ZuluIA_Back.Application.Features.PuntoVenta.Commands;

public record ConciliarDeuceOperacionCommand(long Id, bool Confirmar, string? Observacion = null) : IRequest<Result>;
