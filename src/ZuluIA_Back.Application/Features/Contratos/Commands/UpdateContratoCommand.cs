using MediatR;
using ZuluIA_Back.Domain.Common;

namespace ZuluIA_Back.Application.Features.Contratos.Commands;

public record UpdateContratoCommand(
    long Id,
    string Descripcion,
    DateOnly FechaInicio,
    DateOnly FechaFin,
    decimal Importe,
    bool RenovacionAutomatica,
    string? Observacion,
    bool GenerarImpactoComercial = false,
    bool GenerarImpactoFinanciero = false) : IRequest<Result<long>>;
