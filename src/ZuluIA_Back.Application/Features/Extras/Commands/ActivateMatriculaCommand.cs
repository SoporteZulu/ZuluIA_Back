using MediatR;
using ZuluIA_Back.Domain.Common;

namespace ZuluIA_Back.Application.Features.Extras.Commands;

public record ActivateMatriculaCommand(long Id) : IRequest<Result>;
