using AutoMapper;
using ZuluIA_Back.Application.Features.ListasPrecios.DTOs;
using ZuluIA_Back.Domain.Entities.Precios;

namespace ZuluIA_Back.Application.Features.ListasPrecios.Mappings;

public class ListaPrecionMappingProfile : Profile
{
    public ListaPrecionMappingProfile()
    {
        CreateMap<ListaPrecios, ListaPreciosDto>();

        CreateMap<ListaPrecios, ListaPreciosDetalleDto>()
            .ForMember(d => d.Items, o => o.MapFrom(s => s.Items));

        CreateMap<ListaPreciosItem, ListaPreciosItemDto>()
            .ForMember(d => d.PrecioFinal, o => o.MapFrom(s => s.PrecioFinal));
    }
}