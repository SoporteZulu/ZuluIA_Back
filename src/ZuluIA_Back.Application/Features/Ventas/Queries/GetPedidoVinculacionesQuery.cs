using MediatR;
using ZuluIA_Back.Application.Features.Ventas.DTOs;

namespace ZuluIA_Back.Application.Features.Ventas.Queries;

public record GetPedidoVinculacionesQuery(long PedidoId) : IRequest<PedidoVinculacionesDto?>;
