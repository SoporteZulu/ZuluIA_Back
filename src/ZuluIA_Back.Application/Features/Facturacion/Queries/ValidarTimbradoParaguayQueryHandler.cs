using MediatR;
using ZuluIA_Back.Application.Common.Interfaces;
using ZuluIA_Back.Application.Features.Facturacion.DTOs;
using ZuluIA_Back.Application.Features.Facturacion.Services;

namespace ZuluIA_Back.Application.Features.Facturacion.Queries;

public class ValidarTimbradoParaguayQueryHandler(IApplicationDbContext db)
    : IRequestHandler<ValidarTimbradoParaguayQuery, ValidacionTimbradoParaguayDto>
{
    public Task<ValidacionTimbradoParaguayDto> Handle(ValidarTimbradoParaguayQuery request, CancellationToken ct)
        => ParaguayTimbradoResolver.ValidarAsync(
            db,
            request.SucursalId,
            request.PuntoFacturacionId,
            request.TipoComprobanteId,
            request.Fecha,
            request.NumeroComprobante,
            ct);
}