using MediatR;
using ZuluIA_Back.Domain.Common;

namespace ZuluIA_Back.Application.Features.Cobros.Commands;

public record AnularCobroCommand(long Id) : IRequest<Result>;