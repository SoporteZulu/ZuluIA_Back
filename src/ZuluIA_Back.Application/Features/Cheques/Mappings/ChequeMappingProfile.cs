using AutoMapper;
using ZuluIA_Back.Application.Features.Cheques.DTOs;
using ZuluIA_Back.Domain.Entities.Finanzas;

namespace ZuluIA_Back.Application.Features.Cheques.Mappings;

public class ChequeMappingProfile : Profile
{
    public ChequeMappingProfile()
    {
        CreateMap<Cheque, ChequeDto>()
            .ForMember(d => d.Estado, o => o.MapFrom(s => s.Estado.ToString().ToUpperInvariant()));
    }
}