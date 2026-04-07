using AutoMapper;
using ZuluIA_Back.Application.Features.Cheques.DTOs;
using ZuluIA_Back.Domain.Entities.Finanzas;

namespace ZuluIA_Back.Application.Features.Cheques.Mappings;

public class ChequeMappingProfile : Profile
{
    public ChequeMappingProfile()
    {
        CreateMap<Cheque, ChequeDto>()
            .ForMember(d => d.Estado, o => o.MapFrom(s => s.Estado.ToString().ToUpperInvariant()))
            .ForMember(d => d.Tipo, o => o.MapFrom(s => s.Tipo.ToString().ToUpperInvariant()))
            .ForMember(d => d.CajaDescripcion, o => o.Ignore())
            .ForMember(d => d.TerceroRazonSocial, o => o.Ignore())
            .ForMember(d => d.MonedaSimbolo, o => o.Ignore())
            .ForMember(d => d.ChequeraDescripcion, o => o.Ignore())
            .ForMember(d => d.ComprobanteOrigenNumero, o => o.Ignore());
    }
}