using MediatR;
using ZuluIA_Back.Domain.Common;

namespace ZuluIA_Back.Application.Features.Usuarios.Commands;

public record ActivateUsuarioCommand(long Id) : IRequest<Result>;