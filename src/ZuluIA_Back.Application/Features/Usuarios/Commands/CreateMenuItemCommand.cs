using MediatR;
using ZuluIA_Back.Domain.Common;

namespace ZuluIA_Back.Application.Features.Usuarios.Commands;

public record CreateMenuItemCommand(
    long? ParentId,
    string Descripcion,
    string? Formulario,
    string? Icono,
    short Nivel,
    short Orden) : IRequest<Result<long>>;