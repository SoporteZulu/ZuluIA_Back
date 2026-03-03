using AutoMapper;
using MediatR;
using ZuluIA_Back.Application.Features.Usuarios.DTOs;
using ZuluIA_Back.Domain.Interfaces;

namespace ZuluIA_Back.Application.Features.Usuarios.Queries;

public class GetUsuarioByIdQueryHandler(
    IUsuarioRepository repo,
    IMapper mapper)
    : IRequestHandler<GetUsuarioByIdQuery, UsuarioDto?>
{
    public async Task<UsuarioDto?> Handle(GetUsuarioByIdQuery request, CancellationToken ct)
    {
        var usuario = await repo.GetByIdConSucursalesAsync(request.Id, ct);
        return usuario is null ? null : mapper.Map<UsuarioDto>(usuario);
    }
}