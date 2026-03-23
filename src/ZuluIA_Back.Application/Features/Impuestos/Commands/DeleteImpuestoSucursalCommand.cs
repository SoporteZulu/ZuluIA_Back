using MediatR;
using ZuluIA_Back.Domain.Common;

namespace ZuluIA_Back.Application.Features.Impuestos.Commands;

public record DeleteImpuestoSucursalCommand(long ImpuestoId, long AsignacionId)
    : IRequest<Result>;
