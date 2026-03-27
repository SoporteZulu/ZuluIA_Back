using MediatR;
using ZuluIA_Back.Domain.Common;

namespace ZuluIA_Back.Application.Features.Contratos.Commands;

public record RenovarContratosAutomaticamenteCommand(
    long? SucursalId,
    DateOnly FechaCorte,
    decimal? PorcentajeAjuste = null,
    bool GenerarImpactoComercial = true,
    bool GenerarImpactoFinanciero = true) : IRequest<Result<int>>;
