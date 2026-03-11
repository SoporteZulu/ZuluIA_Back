using AutoMapper;
using ZuluIA_Back.Application.Features.Cotizaciones.DTOs;
using ZuluIA_Back.Domain.Entities.Finanzas;

namespace ZuluIA_Back.Application.Features.Cotizaciones.Mappings;

public class CotizacionMappingProfile : Profile
{
    public CotizacionMappingProfile()
    {
        CreateMap<CotizacionMoneda, CotizacionMonedaDto>();
    }
}