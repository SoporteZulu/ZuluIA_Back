using MediatR;
using ZuluIA_Back.Application.Features.Facturacion.DTOs;
using ZuluIA_Back.Domain.Common;

namespace ZuluIA_Back.Application.Features.Facturacion.Commands;

public record UpsertAfipWsfeConfiguracionCommand(
    long PuntoFacturacionId,
    bool Habilitado,
    bool Produccion,
    bool UsaCaeaPorDefecto,
    string CuitEmisor,
    string? CertificadoAlias,
    string? Observacion
) : IRequest<Result<AfipWsfeConfiguracionDto>>;
