using MediatR;
using ZuluIA_Back.Domain.Common;

namespace ZuluIA_Back.Application.Features.Facturacion.Commands;

public record CrearOrdenCargaCommand(
    long CartaPorteId,
    long? TransportistaId,
    DateOnly FechaCarga,
    string Origen,
    string Destino,
    string? Patente,
    string? Observacion
) : IRequest<Result<long>>;
