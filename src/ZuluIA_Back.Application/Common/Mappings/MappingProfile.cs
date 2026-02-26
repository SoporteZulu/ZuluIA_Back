using AutoMapper;
using ZuluIA_Back.Application.Features.Terceros.DTOs;
using ZuluIA_Back.Application.Features.Items.DTOs;
using ZuluIA_Back.Application.Features.Comprobantes.DTOs;
using ZuluIA_Back.Application.Features.Cobros.DTOs;
using ZuluIA_Back.Application.Features.Pagos.DTOs;
using ZuluIA_Back.Application.Features.Contabilidad.DTOs;
using ZuluIA_Back.Domain.Entities.Terceros;
using ZuluIA_Back.Domain.Entities.Items;
using ZuluIA_Back.Domain.Entities.Comprobantes;
using ZuluIA_Back.Domain.Entities.Finanzas;
using ZuluIA_Back.Domain.Entities.Contabilidad;

namespace ZuluIA_Back.Application.Common.Mappings;

public class MappingProfile : Profile
{
    public MappingProfile()
    {
        CreateMap<Tercero, TerceroDto>()
            .ForMember(d => d.Calle, o => o.MapFrom(s => s.Domicilio.Calle))
            .ForMember(d => d.Nro, o => o.MapFrom(s => s.Domicilio.Nro))
            .ForMember(d => d.CodigoPostal, o => o.MapFrom(s => s.Domicilio.CodigoPostal))
            .ForMember(d => d.LocalidadId, o => o.MapFrom(s => s.Domicilio.LocalidadId));

        CreateMap<Tercero, TerceroListDto>();

        CreateMap<Item, ItemDto>();
        CreateMap<Item, ItemListDto>();

        CreateMap<Comprobante, ComprobanteDto>()
            .ForMember(d => d.Prefijo, o => o.MapFrom(s => s.Numero.Prefijo))
            .ForMember(d => d.Numero, o => o.MapFrom(s => s.Numero.Numero))
            .ForMember(d => d.NroFormateado, o => o.MapFrom(s => s.Numero.Formateado));

        CreateMap<Comprobante, ComprobanteListDto>()
            .ForMember(d => d.NroFormateado, o => o.MapFrom(s => s.Numero.Formateado));

        CreateMap<ComprobanteItem, ComprobanteItemDto>();

        CreateMap<Cobro, CobroDto>();
        CreateMap<CobroMedio, CobroMedioDto>();
        CreateMap<Pago, PagoDto>();
        CreateMap<PagoMedio, PagoMedioDto>();

        CreateMap<Asiento, AsientoDto>();
        CreateMap<AsientoLinea, AsientoLineaDto>();
    }
}