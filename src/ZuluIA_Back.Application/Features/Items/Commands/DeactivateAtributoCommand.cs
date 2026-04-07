using MediatR;
using ZuluIA_Back.Domain.Common;

namespace ZuluIA_Back.Application.Features.Items.Commands;

public record DeactivateAtributoCommand(long Id) : IRequest<Result>;