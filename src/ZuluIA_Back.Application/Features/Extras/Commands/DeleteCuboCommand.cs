using MediatR;
using ZuluIA_Back.Domain.Common;

namespace ZuluIA_Back.Application.Features.Extras.Commands;

public record DeleteCuboCommand(long Id) : IRequest<Result>;
