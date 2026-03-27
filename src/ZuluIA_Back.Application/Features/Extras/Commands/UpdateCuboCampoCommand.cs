using MediatR;
using ZuluIA_Back.Domain.Common;

namespace ZuluIA_Back.Application.Features.Extras.Commands;

public record UpdateCuboCampoCommand(
    long CuboId,
    long CampoId,
    string? Descripcion,
    int? Ubicacion,
    int? Posicion,
    bool? Visible,
    bool? Calculado,
    string? Filtro,
    long? CampoPadreId,
    int? Orden,
    int? TipoOrden,
    int? FuncionAgregado) : IRequest<Result>;

public record DeleteCuboCampoCommand(long CuboId, long CampoId) : IRequest<Result>;
