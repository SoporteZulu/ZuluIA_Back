using MediatR;
using ZuluIA_Back.Application.Features.Terceros.DTOs;
using ZuluIA_Back.Domain.Common;

namespace ZuluIA_Back.Application.Features.Terceros.Commands;

public record SetTerceroUsuarioClientePermisosCommand(
    long TerceroId,
    IReadOnlyList<SetTerceroUsuarioClientePermisoItem> Permisos)
    : IRequest<Result<TerceroUsuarioClienteDto>>;

public record SetTerceroUsuarioClientePermisoItem(
    long SeguridadId,
    bool Valor);
