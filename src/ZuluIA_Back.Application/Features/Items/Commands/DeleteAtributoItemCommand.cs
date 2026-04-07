using MediatR;
using ZuluIA_Back.Domain.Common;

namespace ZuluIA_Back.Application.Features.Items.Commands;

public record DeleteAtributoItemCommand(long ItemId, long AtributoId)
    : IRequest<Result>;