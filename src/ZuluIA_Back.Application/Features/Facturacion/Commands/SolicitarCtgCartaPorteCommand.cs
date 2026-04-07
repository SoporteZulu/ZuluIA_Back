using MediatR;
using ZuluIA_Back.Domain.Common;

namespace ZuluIA_Back.Application.Features.Facturacion.Commands;

public record SolicitarCtgCartaPorteCommand(
    long CartaPorteId,
    DateOnly FechaSolicitud,
    string? Observacion,
    bool EsReintento = false
) : IRequest<Result>;
