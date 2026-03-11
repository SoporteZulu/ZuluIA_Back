using MediatR;
using ZuluIA_Back.Application.Features.Produccion.DTOs;

namespace ZuluIA_Back.Application.Features.Produccion.Queries;

public record GetFormulasProduccionQuery(
    bool SoloActivas = true)
    : IRequest<IReadOnlyList<FormulaProduccionDto>>;