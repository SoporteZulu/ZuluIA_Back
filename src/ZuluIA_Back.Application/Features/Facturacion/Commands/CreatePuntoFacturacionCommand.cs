using MediatR;
using ZuluIA_Back.Domain.Common;

namespace ZuluIA_Back.Application.Features.Facturacion.Commands;

public record CreatePuntoFacturacionCommand(
    long SucursalId,
    long TipoId,
    short Numero,
    string? Descripcion
) : IRequest<Result<long>>;