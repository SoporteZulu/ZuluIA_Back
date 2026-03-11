using MediatR;
using ZuluIA_Back.Domain.Common;

namespace ZuluIA_Back.Application.Features.Finanzas.Commands;

public record AnularCobroCommand(long Id) : IRequest<Result>;