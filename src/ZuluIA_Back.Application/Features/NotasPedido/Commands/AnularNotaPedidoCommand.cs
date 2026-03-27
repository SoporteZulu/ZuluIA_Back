using MediatR;
using ZuluIA_Back.Domain.Common;

namespace ZuluIA_Back.Application.Features.NotasPedido.Commands;

public record AnularNotaPedidoCommand(long Id) : IRequest<Result>;
