using AutoMapper;
using ZuluIA_Back.Application.Features.Usuarios.DTOs;
using ZuluIA_Back.Domain.Entities.Usuarios;

namespace ZuluIA_Back.Application.Features.Usuarios.Mappings;

public class UsuarioMappingProfile : Profile
{
    public UsuarioMappingProfile()
    {
        CreateMap<Usuario, UsuarioListDto>();

        CreateMap<Usuario, UsuarioDto>()
            .ForMember(d => d.SucursalIds,
                o => o.MapFrom(s => s.Sucursales.Select(x => x.SucursalId).ToList()));

        CreateMap<MenuItem, MenuItemDto>()
            .ForMember(d => d.Hijos,
                o => o.MapFrom(s => s.Hijos));

        CreateMap<ParametroUsuario, ParametroUsuarioDto>();
    }
}