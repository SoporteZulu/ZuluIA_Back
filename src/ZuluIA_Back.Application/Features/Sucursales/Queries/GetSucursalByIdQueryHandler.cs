using AutoMapper;
using MediatR;
using ZuluIA_Back.Application.Features.Sucursales.DTOs;
using ZuluIA_Back.Domain.Interfaces;

namespace ZuluIA_Back.Application.Features.Sucursales.Queries;

public class GetSucursalByIdQueryHandler(
    ISucursalRepository repo,
    IMapper mapper)
    : IRequestHandler<GetSucursalByIdQuery, SucursalDto?>
{
    public async Task<SucursalDto?> Handle(GetSucursalByIdQuery request, CancellationToken ct)
    {
        var sucursal = await repo.GetByIdAsync(request.Id, ct);
        return sucursal is null ? null : mapper.Map<SucursalDto>(sucursal);
    }
}