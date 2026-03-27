using MediatR;
using ZuluIA_Back.Domain.Common;

namespace ZuluIA_Back.Application.Features.Compras.Commands;

public record RecibirOrdenCompraCommand(long Id) : IRequest<Result>;
public record CancelarOrdenCompraCommand(long Id) : IRequest<Result>;
