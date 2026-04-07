using MediatR;
using ZuluIA_Back.Domain.Common;

namespace ZuluIA_Back.Application.Features.Fiscal.Commands;

public record RegistrarLiquidacionPrimariaGranoCommand(long SucursalId, DateOnly Fecha, string NumeroLiquidacion, string Producto, decimal Cantidad, decimal PrecioUnitario, string? Observacion) : IRequest<Result<long>>;
