using AutoMapper;
using ZuluIA_Back.Application.Features.Configuracion.DTOs;
using ZuluIA_Back.Domain.Entities.Configuracion;

namespace ZuluIA_Back.Application.Features.Configuracion.Mappings;

public class ConfiguracionMappingProfile : Profile
{
    public ConfiguracionMappingProfile()
    {
        CreateMap<ConfiguracionSistema, ConfiguracionDto>();
    }
}