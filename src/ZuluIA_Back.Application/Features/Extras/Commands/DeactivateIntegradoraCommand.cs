using MediatR;
using ZuluIA_Back.Domain.Common;

namespace ZuluIA_Back.Application.Features.Extras.Commands;

public record DeactivateIntegradoraCommand(long Id) : IRequest<Result>;