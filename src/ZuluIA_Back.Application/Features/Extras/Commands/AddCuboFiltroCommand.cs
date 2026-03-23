using MediatR;
using ZuluIA_Back.Domain.Common;

namespace ZuluIA_Back.Application.Features.Extras.Commands;

public record AddCuboFiltroCommand(
    long CuboId,
    string Filtro,
    int? Operador,
    int? Orden) : IRequest<Result<long>>;

public record UpdateCuboFiltroCommand(
    long CuboId,
    long FiltroId,
    string Filtro,
    int? Operador,
    int? Orden) : IRequest<Result>;

public record DeleteCuboFiltroCommand(long CuboId, long FiltroId) : IRequest<Result>;
