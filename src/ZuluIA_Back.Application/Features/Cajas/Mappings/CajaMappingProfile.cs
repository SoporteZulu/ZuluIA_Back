using AutoMapper;
using ZuluIA_Back.Application.Features.Cajas.DTOs;
using ZuluIA_Back.Domain.Entities.Finanzas;

namespace ZuluIA_Back.Application.Features.Cajas.Mappings;

public class CajaMappingProfile : Profile
{
    public CajaMappingProfile()
    {
        CreateMap<CajaCuentaBancaria, CajaDto>();
        CreateMap<CajaCuentaBancaria, CajaListDto>();
    }
}