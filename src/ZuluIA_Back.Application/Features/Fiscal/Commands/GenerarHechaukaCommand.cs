using MediatR;
using ZuluIA_Back.Domain.Common;

namespace ZuluIA_Back.Application.Features.Fiscal.Commands;

public record GenerarHechaukaCommand(long SucursalId, DateOnly Desde, DateOnly Hasta) : IRequest<Result<long>>;
