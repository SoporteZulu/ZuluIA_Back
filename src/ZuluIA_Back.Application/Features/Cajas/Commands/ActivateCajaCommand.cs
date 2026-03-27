using MediatR;
using ZuluIA_Back.Domain.Common;

namespace ZuluIA_Back.Application.Features.Cajas.Commands;

public record ActivateCajaCommand(long Id) : IRequest<Result>;