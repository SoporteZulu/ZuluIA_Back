using MediatR;
using ZuluIA_Back.Application.Features.Sucursales.DTOs;

namespace ZuluIA_Back.Application.Features.Sucursales.Queries;

public record GetSucursalByIdQuery(long Id) : IRequest<SucursalDto?>;