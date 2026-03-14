using MediatR;
using ZuluIA_Back.Application.Features.Retenciones.DTOs;

namespace ZuluIA_Back.Application.Features.Retenciones.Queries;

public record GetTiposRetencionQuery(bool SoloActivos = true)
    : IRequest<IReadOnlyList<TipoRetencionDto>>;
