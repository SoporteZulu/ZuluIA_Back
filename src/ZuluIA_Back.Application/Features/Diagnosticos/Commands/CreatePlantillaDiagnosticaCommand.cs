using MediatR;
using ZuluIA_Back.Domain.Common;

namespace ZuluIA_Back.Application.Features.Diagnosticos.Commands;

public record PlantillaVariableInput(long VariableId, short Orden);

public record CreatePlantillaDiagnosticaCommand(
    string Codigo,
    string Descripcion,
    string? Observacion,
    IReadOnlyList<PlantillaVariableInput> Variables
) : IRequest<Result<long>>;
