using MediatR;
using ZuluIA_Back.Domain.Common;

namespace ZuluIA_Back.Application.Features.Diagnosticos.Commands;

public record CreateAspectoDiagnosticoCommand(long RegionId, string Codigo, string Descripcion, decimal Peso) : IRequest<Result<long>>;
