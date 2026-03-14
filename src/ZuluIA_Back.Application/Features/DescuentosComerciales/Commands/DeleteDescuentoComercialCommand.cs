using MediatR;
using ZuluIA_Back.Domain.Common;

namespace ZuluIA_Back.Application.Features.DescuentosComerciales.Commands;

public record DeleteDescuentoComercialCommand(long Id) : IRequest<Result>;
