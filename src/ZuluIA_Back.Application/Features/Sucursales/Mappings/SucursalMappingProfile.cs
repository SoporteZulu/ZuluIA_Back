using AutoMapper;
using ZuluIA_Back.Application.Features.Sucursales.DTOs;
using ZuluIA_Back.Domain.Entities.Sucursales;

namespace ZuluIA_Back.Application.Features.Sucursales.Mappings;

public class SucursalMappingProfile : Profile
{
    public SucursalMappingProfile()
    {
        CreateMap<Sucursal, SucursalDto>()
            .ForMember(d => d.Calle, o => o.MapFrom(s => s.Domicilio.Calle))
            .ForMember(d => d.Nro, o => o.MapFrom(s => s.Domicilio.Nro))
            .ForMember(d => d.Piso, o => o.MapFrom(s => s.Domicilio.Piso))
            .ForMember(d => d.Dpto, o => o.MapFrom(s => s.Domicilio.Dpto))
            .ForMember(d => d.CodigoPostal, o => o.MapFrom(s => s.Domicilio.CodigoPostal))
            .ForMember(d => d.LocalidadId, o => o.MapFrom(s => s.Domicilio.LocalidadId))
            .ForMember(d => d.BarrioId, o => o.MapFrom(s => s.Domicilio.BarrioId));

        CreateMap<Sucursal, SucursalListDto>();
    }
}