using MediatR;
using ZuluIA_Back.Domain.Common;

namespace ZuluIA_Back.Application.Features.Contabilidad.Commands;

public record CreateEjercicioCommand(
    string Descripcion,
    DateOnly FechaInicio,
    DateOnly FechaFin,
    string Mascara,
    IReadOnlyList<long> SucursalIds
) : IRequest<Result<long>>;