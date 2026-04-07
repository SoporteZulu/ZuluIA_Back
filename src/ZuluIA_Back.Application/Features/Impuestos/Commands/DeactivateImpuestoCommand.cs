using MediatR;
using ZuluIA_Back.Domain.Common;

namespace ZuluIA_Back.Application.Features.Impuestos.Commands;

public record DeactivateImpuestoCommand(long Id) : IRequest<Result>;