using MediatR;
using ZuluIA_Back.Application.Features.Sucursales.DTOs;

namespace ZuluIA_Back.Application.Features.Sucursales.Queries;

public record GetSucursalesQuery(bool SoloActivas = true) : IRequest<IReadOnlyList<SucursalListDto>>;