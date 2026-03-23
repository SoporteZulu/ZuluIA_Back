using AutoMapper;
using ZuluIA_Back.Application.Features.Comprobantes.DTOs;
using ZuluIA_Back.Domain.Entities.Comprobantes;
using ZuluIA_Back.Domain.Enums;

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

        CreateMap<Comprobante, ComprobanteDto>()
            .ForMember(d => d.Prefijo,        o => o.MapFrom(s => s.Numero.Prefijo))
            .ForMember(d => d.Numero,         o => o.MapFrom(s => s.Numero.Numero))
            .ForMember(d => d.NroFormateado,  o => o.MapFrom(s => s.Numero.Formateado))
            .ForMember(d => d.TieneIdentificadoresSifen,
                o => o.MapFrom(s => s.SifenTrackingId != null || s.SifenCdc != null || s.SifenNumeroLote != null))
            .ForMember(d => d.PuedeReintentarSifen,
                o => o.MapFrom(s => s.EstadoSifen == EstadoSifenParaguay.Rechazado || s.EstadoSifen == EstadoSifenParaguay.Error))
            .ForMember(d => d.PuedeConciliarSifen,
                o => o.MapFrom(s => s.Estado != EstadoComprobante.Borrador
                    && s.EstadoSifen != EstadoSifenParaguay.Aceptado
                    && (s.SifenTrackingId != null || s.SifenCdc != null || s.SifenNumeroLote != null)))
            .ForMember(d => d.Impuestos,      o => o.Ignore())
            .ForMember(d => d.Tributos,       o => o.Ignore());

        CreateMap<ComprobanteItem, ComprobanteItemDto>()
            .ForMember(d => d.ItemCodigo, o => o.Ignore())
            .ForMember(d => d.DepositoDescripcion, o => o.Ignore());

        CreateMap<ComprobanteImpuesto, ComprobanteImpuestoDto>();
        CreateMap<ComprobanteTributo, ComprobanteTributoDto>();

        CreateMap<Imputacion, ImputacionDto>()
            .ForMember(d => d.NumeroOrigen, o => o.Ignore())
            .ForMember(d => d.NumeroDestino, o => o.Ignore());
    }
}