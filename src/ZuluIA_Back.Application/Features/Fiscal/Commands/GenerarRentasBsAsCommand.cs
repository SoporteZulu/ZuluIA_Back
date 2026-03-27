using MediatR;
using ZuluIA_Back.Domain.Common;

namespace ZuluIA_Back.Application.Features.Fiscal.Commands;

public record GenerarRentasBsAsCommand(long SucursalId, DateOnly Desde, DateOnly Hasta, string? Observacion) : IRequest<Result<long>>;
