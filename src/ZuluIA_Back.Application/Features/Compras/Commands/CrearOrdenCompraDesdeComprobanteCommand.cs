using MediatR;
using ZuluIA_Back.Domain.Common;

namespace ZuluIA_Back.Application.Features.Compras.Commands;

public record CrearOrdenCompraDesdeComprobanteCommand(
    long ComprobanteId,
    long ProveedorId,
    DateOnly? FechaEntregaReq,
    string? CondicionesEntrega) : IRequest<Result<long>>;
