using MediatR;
using ZuluIA_Back.Domain.Common;

namespace ZuluIA_Back.Application.Features.RRHH.Commands;

public record GenerarLiquidacionesMasivasCommand(
    long SucursalId,
    int Anio,
    int Mes,
    decimal? PorcentajeAjuste = null,
    IReadOnlyList<long>? EmpleadoIds = null,
    string? Observacion = null) : IRequest<Result<IReadOnlyList<long>>>;
