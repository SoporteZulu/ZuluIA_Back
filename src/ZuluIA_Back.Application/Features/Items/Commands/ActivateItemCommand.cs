using MediatR;
using ZuluIA_Back.Domain.Common;

namespace ZuluIA_Back.Application.Features.Items.Commands;

public record ActivateItemCommand(long Id) : IRequest<Result>;