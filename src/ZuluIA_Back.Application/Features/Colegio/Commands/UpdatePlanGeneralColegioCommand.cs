using MediatR;
using ZuluIA_Back.Domain.Common;

namespace ZuluIA_Back.Application.Features.Colegio.Commands;

public record UpdatePlanGeneralColegioCommand(
    long Id,
    long PlanPagoId,
    long TipoComprobanteId,
    long ItemId,
    long MonedaId,
    string Codigo,
    string Descripcion,
    decimal ImporteBase,
    string? Observacion) : IRequest<Result>;
