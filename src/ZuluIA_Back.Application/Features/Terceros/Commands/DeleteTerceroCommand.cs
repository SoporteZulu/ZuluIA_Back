using MediatR;
using ZuluIA_Back.Domain.Common;

namespace ZuluIA_Back.Application.Features.Terceros.Commands;

public record DeleteTerceroCommand(long Id) : IRequest<Result>;