using AutoMapper;
using ZuluIA_Back.Application.Features.Comprobantes.DTOs;
using ZuluIA_Back.Domain.Entities.Comprobantes;

namespace ZuluIA_Back.Application.Features.Comprobantes.Mappings;

public class ComprobanteMappingProfile : Profile
{
    public ComprobanteMappingProfile()
    {
        CreateMap<Comprobante, ComprobanteListDto>()
            .ForMember(d => d.Prefijo,
                o => o.MapFrom(s => s.Numero.Prefijo))
            .ForMember(d => d.Numero,
                o => o.MapFrom(s => s.Numero.Numero))
            .ForMember(d => d.NumeroFormateado,
                o => o.MapFrom(s => s.Numero.Formateado))
            .ForMember(d => d.Estado,
                o => o.MapFrom(s => s.Estado.ToString().ToUpperInvariant()))
            .ForMember(d => d.TieneCae,
                o => o.MapFrom(s => !string.IsNullOrEmpty(s.Cae)))
            .ForMember(d => d.TipoComprobanteDescripcion, o => o.Ignore())
            .ForMember(d => d.TerceroRazonSocial, o => o.Ignore())
            .ForMember(d => d.MonedaSimbolo, o => o.Ignore());

        CreateMap<ComprobanteItem, ComprobanteItemDto>()
            .ForMember(d => d.ItemCodigo, o => o.Ignore())
            .ForMember(d => d.DepositoDescripcion, o => o.Ignore());

        CreateMap<Imputacion, ImputacionDto>()
            .ForMember(d => d.NumeroOrigen, o => o.Ignore())
            .ForMember(d => d.NumeroDestino, o => o.Ignore());
    }
}