using MediatR;
using ZuluIA_Back.Domain.Common;

namespace ZuluIA_Back.Application.Features.Miembros.Commands;

public record ActivateMiembroCommand(long Id) : IRequest<Result>;