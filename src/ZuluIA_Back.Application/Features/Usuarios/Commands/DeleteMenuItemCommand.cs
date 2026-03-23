using MediatR;
using ZuluIA_Back.Domain.Common;

namespace ZuluIA_Back.Application.Features.Usuarios.Commands;

public record DeleteMenuItemCommand(long Id) : IRequest<Result>;