using MediatR;
using ZuluIA_Back.Domain.Common;

namespace ZuluIA_Back.Application.Features.Pagos.Commands;

public record AnularPagoCommand(long Id) : IRequest<Result>;