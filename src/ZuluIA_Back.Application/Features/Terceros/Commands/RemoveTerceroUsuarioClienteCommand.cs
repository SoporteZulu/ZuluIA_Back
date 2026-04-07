using MediatR;
using ZuluIA_Back.Domain.Common;

namespace ZuluIA_Back.Application.Features.Terceros.Commands;

public record RemoveTerceroUsuarioClienteCommand(long TerceroId)
    : IRequest<Result>;
