using MediatR;
using ZuluIA_Back.Application.Features.Usuarios.DTOs;
using ZuluIA_Back.Domain.Common;

namespace ZuluIA_Back.Application.Features.Usuarios.Queries;

public record GetUsuariosPagedQuery(
    int Page,
    int PageSize,
    string? Search,
    bool? SoloActivos
) : IRequest<PagedResult<UsuarioListDto>>;