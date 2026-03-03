using MediatR;
using ZuluIA_Back.Application.Features.Usuarios.DTOs;

namespace ZuluIA_Back.Application.Features.Usuarios.Queries;

public record GetMenuUsuarioQuery(long UsuarioId) : IRequest<IReadOnlyList<MenuItemDto>>;