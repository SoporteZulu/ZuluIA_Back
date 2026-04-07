using MediatR;
using ZuluIA_Back.Domain.Common;

namespace ZuluIA_Back.Application.Features.Contratos.Commands;

public record CreateContratoCommand(
    long TerceroId,
    long SucursalId,
    long MonedaId,
    string Codigo,
    string Descripcion,
    DateOnly FechaInicio,
    DateOnly FechaFin,
    decimal Importe,
    bool RenovacionAutomatica,
    string? Observacion,
    bool GenerarImpactoComercial = true,
    bool GenerarImpactoFinanciero = false) : IRequest<Result<long>>;
