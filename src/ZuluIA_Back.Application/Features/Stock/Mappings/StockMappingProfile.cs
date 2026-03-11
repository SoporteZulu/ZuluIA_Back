using AutoMapper;
using ZuluIA_Back.Application.Features.Stock.DTOs;
using ZuluIA_Back.Domain.Entities.Stock;

namespace ZuluIA_Back.Application.Features.Stock.Mappings;

public class StockMappingProfile : Profile
{
    public StockMappingProfile()
    {
        CreateMap<StockItem, StockItemDto>()
            .ForMember(d => d.ItemCodigo, o => o.Ignore())
            .ForMember(d => d.ItemDescripcion, o => o.Ignore())
            .ForMember(d => d.DepositoDescripcion, o => o.Ignore())
            .ForMember(d => d.DepositoEsDefault, o => o.Ignore())
            .ForMember(d => d.StockMinimo, o => o.Ignore())
            .ForMember(d => d.StockMaximo, o => o.Ignore())
            .ForMember(d => d.BajoMinimo, o => o.Ignore());

        CreateMap<MovimientoStock, MovimientoStockDto>()
            .ForMember(d => d.TipoMovimiento,
                o => o.MapFrom(s => s.TipoMovimiento.ToString()))
            .ForMember(d => d.ItemCodigo, o => o.Ignore())
            .ForMember(d => d.ItemDescripcion, o => o.Ignore())
            .ForMember(d => d.DepositoDescripcion, o => o.Ignore());
    }
}