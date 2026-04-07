using MediatR;
using ZuluIA_Back.Application.Features.Terceros.DTOs;
using ZuluIA_Back.Domain.Common;

namespace ZuluIA_Back.Application.Features.Terceros.Commands;

public record UpsertTerceroUsuarioClienteCommand(
    long TerceroId,
    string? UserName,
    string? Password,
    string? ConfirmPassword,
    long? UsuarioGrupoId)
    : IRequest<Result<TerceroUsuarioClienteDto>>;
