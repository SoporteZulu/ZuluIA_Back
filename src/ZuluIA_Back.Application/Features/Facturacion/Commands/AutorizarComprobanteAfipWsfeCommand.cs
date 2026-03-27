using MediatR;
using ZuluIA_Back.Application.Features.Facturacion.DTOs;
using ZuluIA_Back.Domain.Common;

namespace ZuluIA_Back.Application.Features.Facturacion.Commands;

public record AutorizarComprobanteAfipWsfeCommand(
    long ComprobanteId,
    bool UsarCaea
) : IRequest<Result<AfipWsfeOperacionDto>>;
