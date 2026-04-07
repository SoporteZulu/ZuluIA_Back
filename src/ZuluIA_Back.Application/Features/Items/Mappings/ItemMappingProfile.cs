using AutoMapper;
using ZuluIA_Back.Application.Features.Items.DTOs;
using ZuluIA_Back.Domain.Entities.Items;

namespace ZuluIA_Back.Application.Features.Items.Mappings;

public class ItemMappingProfile : Profile
{
    public ItemMappingProfile()
    {
        CreateMap<CategoriaItem, CategoriaItemDto>()
            .ForMember(d => d.Hijos, o => o.MapFrom(s => s.Hijos));

        CreateMap<Deposito, DepositoDto>();

        CreateMap<Item, ItemDto>()
            .ForMember(d => d.CategoriaDescripcion, o => o.Ignore())
            .ForMember(d => d.MarcaDescripcion, o => o.Ignore())
            .ForMember(d => d.UnidadMedidaDescripcion, o => o.Ignore())
            .ForMember(d => d.AlicuotaIvaCompraDescripcion, o => o.Ignore())
            .ForMember(d => d.AlicuotaIvaCompraPorcentaje, o => o.Ignore())
            .ForMember(d => d.ImpuestoInternoDescripcion, o => o.Ignore())
            .ForMember(d => d.DepositoDefaultDescripcion, o => o.Ignore())
            .ForMember(d => d.AlicuotaIvaPorcentaje, o => o.Ignore())
            .ForMember(d => d.PrecioVentaCalculado, o => o.MapFrom(s => s.CalcularPrecioVentaPorGanancia()))
            .ForMember(d => d.PuedeEditar, o => o.MapFrom(s => !s.EsSistema))
            .ForMember(d => d.Stock, o => o.Ignore())
            .ForMember(d => d.StockDisponible, o => o.Ignore())
            .ForMember(d => d.StockComprometido, o => o.Ignore())
            .ForMember(d => d.StockReservado, o => o.Ignore())
            .ForMember(d => d.StockEnTransito, o => o.Ignore())
            .ForMember(d => d.EsPack, o => o.Ignore())
            .ForMember(d => d.ListasPrecios, o => o.Ignore())
            .ForMember(d => d.AtributosComerciales, o => o.Ignore())
            .ForMember(d => d.Componentes, o => o.Ignore());

        CreateMap<Item, ItemListDto>()
            .ForMember(d => d.CategoriaDescripcion, o => o.Ignore())
            .ForMember(d => d.MarcaDescripcion, o => o.Ignore())
            .ForMember(d => d.UnidadMedidaDescripcion, o => o.Ignore())
            .ForMember(d => d.AlicuotaIvaCompraDescripcion, o => o.Ignore())
            .ForMember(d => d.AlicuotaIvaCompraPorcentaje, o => o.Ignore())
            .ForMember(d => d.ImpuestoInternoDescripcion, o => o.Ignore())
            .ForMember(d => d.DepositoDefaultDescripcion, o => o.Ignore())
            .ForMember(d => d.AlicuotaIvaDescripcion, o => o.Ignore())
            .ForMember(d => d.AlicuotaIvaPorcentaje, o => o.Ignore())
            .ForMember(d => d.MonedaSimbol, o => o.Ignore())
            .ForMember(d => d.Stock, o => o.Ignore())
            .ForMember(d => d.StockDisponible, o => o.Ignore())
            .ForMember(d => d.StockComprometido, o => o.Ignore())
            .ForMember(d => d.StockReservado, o => o.Ignore())
            .ForMember(d => d.StockEnTransito, o => o.Ignore())
            .ForMember(d => d.EsVendible, o => o.Ignore())
            .ForMember(d => d.EsPack, o => o.Ignore())
            .ForMember(d => d.CantidadComponentes, o => o.Ignore())
            .ForMember(d => d.PrecioVentaCalculado, o => o.MapFrom(s => s.CalcularPrecioVentaPorGanancia()))
            .ForMember(d => d.PuedeEditar, o => o.MapFrom(s => !s.EsSistema));

        CreateMap<Item, ItemPrecioDto>()
            .ForMember(d => d.AlicuotaIvaPorcentaje, o => o.Ignore())
            .ForMember(d => d.EsVendible, o => o.Ignore())
            .ForMember(d => d.Stock, o => o.Ignore())
            .ForMember(d => d.StockDisponible, o => o.Ignore())
            .ForMember(d => d.StockComprometido, o => o.Ignore())
            .ForMember(d => d.StockReservado, o => o.Ignore());
    }
}