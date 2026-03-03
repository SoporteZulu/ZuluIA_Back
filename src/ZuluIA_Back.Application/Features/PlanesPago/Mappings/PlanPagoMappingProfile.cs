using AutoMapper;
using ZuluIA_Back.Application.Features.PlanesPago.DTOs;
using ZuluIA_Back.Domain.Entities.Ventas;

namespace ZuluIA_Back.Application.Features.PlanesPago.Mappings;

public class PlanPagoMappingProfile : Profile
{
    public PlanPagoMappingProfile()
    {
        CreateMap<PlanPago, PlanPagoDto>();
    }
}