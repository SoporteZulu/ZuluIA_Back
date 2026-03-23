using MediatR;
using ZuluIA_Back.Domain.Common;

namespace ZuluIA_Back.Application.Features.Usuarios.Commands;

public record UpdateMenuItemCommand(
    long Id,
    string Descripcion,
    string? Formulario,
    string? Icono,
    short Orden) : IRequest<Result>;