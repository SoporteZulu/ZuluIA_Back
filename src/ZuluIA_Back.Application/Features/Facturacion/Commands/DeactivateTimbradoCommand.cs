using MediatR;
using ZuluIA_Back.Domain.Common;

namespace ZuluIA_Back.Application.Features.Facturacion.Commands;

public record DeactivateTimbradoCommand(long Id) : IRequest<Result>;