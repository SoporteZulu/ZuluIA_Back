using AutoMapper;
using ZuluIA_Back.Application.Features.ListasPrecios.DTOs;
using ZuluIA_Back.Domain.Entities.Precios;

namespace ZuluIA_Back.Application.Features.ListasPrecios.Mappings;

public class ListaPrecionMappingProfile : Profile
{
    public ListaPrecionMappingProfile()
    {
        CreateMap<ListaPrecios, ListaPreciosDto>()
            .ForMember(d => d.MonedaDescripcion, o => o.Ignore())
            .ForMember(d => d.MonedaSimbolo, o => o.Ignore())
            .ForMember(d => d.EstaVigenteHoy, o => o.Ignore())
            .ForMember(d => d.TieneHerencia, o => o.Ignore())
            .ForMember(d => d.CantidadItems, o => o.Ignore())
            .ForMember(d => d.CantidadPersonasAsignadas, o => o.Ignore())
            .ForMember(d => d.CantidadPromocionesActivas, o => o.Ignore());

        CreateMap<ListaPrecios, ListaPreciosDetalleDto>()
            .ForMember(d => d.Items, o => o.MapFrom(s => s.Items));

        CreateMap<ListaPreciosItem, ListaPreciosItemDto>()
            .ForMember(d => d.PrecioFinal, o => o.MapFrom(s => s.PrecioFinal));
    }
}