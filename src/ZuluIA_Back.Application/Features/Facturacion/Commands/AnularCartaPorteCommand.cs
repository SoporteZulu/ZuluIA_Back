using MediatR;
using ZuluIA_Back.Domain.Common;

namespace ZuluIA_Back.Application.Features.Facturacion.Commands;

public record AnularCartaPorteCommand(long CartaPorteId, string? Observacion) : IRequest<Result>;