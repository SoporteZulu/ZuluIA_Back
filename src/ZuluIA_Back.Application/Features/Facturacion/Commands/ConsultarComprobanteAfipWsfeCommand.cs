using MediatR;
using ZuluIA_Back.Application.Features.Facturacion.DTOs;
using ZuluIA_Back.Domain.Common;

namespace ZuluIA_Back.Application.Features.Facturacion.Commands;

public record ConsultarComprobanteAfipWsfeCommand(long ComprobanteId) : IRequest<Result<AfipWsfeOperacionDto>>;
