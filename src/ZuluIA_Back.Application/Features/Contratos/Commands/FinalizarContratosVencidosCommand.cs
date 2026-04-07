using MediatR;
using ZuluIA_Back.Domain.Common;

namespace ZuluIA_Back.Application.Features.Contratos.Commands;

public record FinalizarContratosVencidosCommand(long? SucursalId, DateOnly FechaCorte) : IRequest<Result<int>>;
