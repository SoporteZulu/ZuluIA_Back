using MediatR;
using ZuluIA_Back.Domain.Common;

namespace ZuluIA_Back.Application.Features.Recibos.Commands;

public record AnularReciboCommand(long Id) : IRequest<Result>;
