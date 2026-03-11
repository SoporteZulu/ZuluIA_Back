using AutoMapper;
using ZuluIA_Back.Application.Features.Facturacion.DTOs;
using ZuluIA_Back.Domain.Entities.Facturacion;

namespace ZuluIA_Back.Application.Features.Facturacion.Mappings;

public class FacturacionMappingProfile : Profile
{
    public FacturacionMappingProfile()
    {
        CreateMap<PuntoFacturacion, PuntoFacturacionDto>();
        CreateMap<PuntoFacturacion, PuntoFacturacionListDto>();

        CreateMap<CartaPorte, CartaPorteDto>()
            .ForMember(d => d.Estado,
                o => o.MapFrom(s => s.Estado.ToString().ToUpperInvariant()));

        CreateMap<PeriodoIva, PeriodoIvaDto>()
            .ForMember(d => d.PeriodoDescripcion,
                o => o.MapFrom(s => s.PeriodoDescripcion));
    }
}