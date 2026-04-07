using MediatR;
using ZuluIA_Back.Domain.Common;

namespace ZuluIA_Back.Application.Features.Diagnosticos.Commands;

public record CreateRegionDiagnosticaCommand(string Codigo, string Descripcion) : IRequest<Result<long>>;
