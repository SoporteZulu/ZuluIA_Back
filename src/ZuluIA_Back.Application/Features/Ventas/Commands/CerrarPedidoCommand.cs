using MediatR;
using ZuluIA_Back.Domain.Common;

namespace ZuluIA_Back.Application.Features.Ventas.Commands;

public record CerrarPedidoCommand(
    long PedidoId,
    string? MotivoCierre) : IRequest<Result>;
