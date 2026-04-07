using MediatR;
using ZuluIA_Back.Domain.Common;

namespace ZuluIA_Back.Application.Features.Colegio.Commands;

public record VencerCedulonesColegioCommand(long? LoteId, long? SucursalId, DateOnly FechaCorte) : IRequest<Result<int>>;
