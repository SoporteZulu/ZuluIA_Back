using MediatR;
using ZuluIA_Back.Domain.Common;

namespace ZuluIA_Back.Application.Features.Diagnosticos.Commands;

public record CreatePlanillaDiagnosticaCommand(long PlantillaId, string Nombre, DateOnly Fecha, string? Observacion) : IRequest<Result<long>>;
