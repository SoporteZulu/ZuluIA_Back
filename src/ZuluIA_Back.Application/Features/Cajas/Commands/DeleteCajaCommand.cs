using MediatR;
using ZuluIA_Back.Domain.Common;

namespace ZuluIA_Back.Application.Features.Cajas.Commands;

public record DeleteCajaCommand(long Id) : IRequest<Result>;