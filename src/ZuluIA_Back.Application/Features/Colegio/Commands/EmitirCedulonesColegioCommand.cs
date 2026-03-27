using MediatR;
using ZuluIA_Back.Domain.Common;

namespace ZuluIA_Back.Application.Features.Colegio.Commands;

public record EmitirCedulonesColegioCommand(long LoteId, IReadOnlyList<long> TerceroIds) : IRequest<Result<IReadOnlyList<long>>>;
