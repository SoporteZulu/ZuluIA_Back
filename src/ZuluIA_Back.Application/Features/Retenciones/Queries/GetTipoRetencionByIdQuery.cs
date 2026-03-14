using MediatR;
using ZuluIA_Back.Application.Features.Retenciones.DTOs;

namespace ZuluIA_Back.Application.Features.Retenciones.Queries;

public record GetTipoRetencionByIdQuery(long Id)
    : IRequest<TipoRetencionDto?>;
