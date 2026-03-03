using MediatR;
using ZuluIA_Back.Domain.Common;

namespace ZuluIA_Back.Application.Features.Sucursales.Commands;

public record DeleteSucursalCommand(long Id) : IRequest<Result>;