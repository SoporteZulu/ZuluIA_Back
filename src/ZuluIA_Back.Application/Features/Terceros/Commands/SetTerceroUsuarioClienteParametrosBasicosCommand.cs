using MediatR;
using ZuluIA_Back.Application.Features.Terceros.DTOs;
using ZuluIA_Back.Domain.Common;

namespace ZuluIA_Back.Application.Features.Terceros.Commands;

public record SetTerceroUsuarioClienteParametrosBasicosCommand(
    long TerceroId,
    long? DefaultSucursalId,
    string? DefaultLayoutProfile)
    : IRequest<Result<TerceroUsuarioClienteDto>>;
