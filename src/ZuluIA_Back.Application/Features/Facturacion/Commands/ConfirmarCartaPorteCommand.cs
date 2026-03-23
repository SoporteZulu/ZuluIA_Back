using MediatR;
using ZuluIA_Back.Domain.Common;

namespace ZuluIA_Back.Application.Features.Facturacion.Commands;

public record ConfirmarCartaPorteCommand(long CartaPorteId) : IRequest<Result>;