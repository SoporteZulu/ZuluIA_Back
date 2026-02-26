using MediatR;
using ZuluIA_Back.Domain.Common;

namespace ZuluIA_Back.Application.Features.Comprobantes.Commands;

public record AnularComprobanteCommand(long Id) : IRequest<Result>;