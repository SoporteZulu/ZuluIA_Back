using MediatR;
using ZuluIA_Back.Domain.Common;

namespace ZuluIA_Back.Application.Features.Contratos.Commands;

public record RenovarContratoCommand(
    long Id,
    DateOnly NuevaFechaFin,
    decimal? NuevoImporte,
    string? Observacion,
    bool GenerarImpactoComercial = true,
    bool GenerarImpactoFinanciero = true) : IRequest<Result<long>>;
