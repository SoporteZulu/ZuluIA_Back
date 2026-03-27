using MediatR;
using ZuluIA_Back.Domain.Common;

namespace ZuluIA_Back.Application.Features.Extras.Commands;

public record AddCuboCampoCommand(
    long CuboId,
    string SourceName,
    string? Descripcion,
    int? Ubicacion,
    int? Posicion,
    bool? Visible,
    bool? Calculado,
    string? Filtro,
    long? CampoPadreId,
    int? Orden) : IRequest<Result<long>>;
