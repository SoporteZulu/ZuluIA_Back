using AutoMapper;
using ZuluIA_Back.Application.Features.Comprobantes.DTOs;
using ZuluIA_Back.Application.Features.Contabilidad.DTOs;
using ZuluIA_Back.Application.Features.Cobros.DTOs;
using ZuluIA_Back.Application.Features.Items.DTOs;
using ZuluIA_Back.Application.Features.Pagos.DTOs;
using ZuluIA_Back.Application.Features.Terceros.DTOs;
using ZuluIA_Back.Domain.Entities.Comprobantes;
using ZuluIA_Back.Domain.Entities.Contabilidad;
using ZuluIA_Back.Domain.Entities.Finanzas;
using ZuluIA_Back.Domain.Entities.Items;
using ZuluIA_Back.Domain.Entities.Terceros;
using ZuluIA_Back.Domain.ValueObjects;

namespace ZuluIA_Back.Application.Common.Mappings;

public class MappingProfile : Profile
{
    public MappingProfile()
    {
        // ─── Terceros ─────────────────────────────────────────────────────────
        ConfigurarTerceros();

        // ─── Items ────────────────────────────────────────────────────────────
        CreateMap<Item, ItemDto>();
        CreateMap<Item, ItemListDto>();

        // ─── Comprobantes ─────────────────────────────────────────────────────
        CreateMap<Comprobante, ComprobanteDto>()
            .ForMember(d => d.Prefijo, o => o.MapFrom(s => s.Numero.Prefijo))
            .ForMember(d => d.Numero, o => o.MapFrom(s => s.Numero.Numero))
            .ForMember(d => d.NroFormateado, o => o.MapFrom(s => s.Numero.Formateado));

        CreateMap<Comprobante, ComprobanteListDto>()
            .ForMember(d => d.NumeroFormateado, o => o.MapFrom(s => s.Numero.Formateado));

        CreateMap<ComprobanteItem, ComprobanteItemDto>();

        // ─── Finanzas ─────────────────────────────────────────────────────────
        CreateMap<Cobro, CobroDto>();
        CreateMap<CobroMedio, CobroMedioDto>();
        CreateMap<Pago, PagoDto>();
        CreateMap<PagoMedio, PagoMedioDto>();

        // ─── Contabilidad ─────────────────────────────────────────────────────
        CreateMap<Asiento, AsientoDto>();
        CreateMap<AsientoLinea, AsientoLineaDto>();
    }

    // ─── Configuración Terceros (separado por volumen) ────────────────────────
    private void ConfigurarTerceros()
    {
        // ── Domicilio (ValueObject → DomicilioDto) ────────────────────────────
        // El ValueObject Domicilio no tiene descripción de localidad/barrio:
        // esas descripciones las resuelve el QueryHandler con un join o lookup.
        // Aquí solo se mapean los campos escalares del VO.
        CreateMap<Domicilio, DomicilioDto>()
            .ForMember(d => d.LocalidadDescripcion, o => o.Ignore())
            .ForMember(d => d.BarrioDescripcion, o => o.Ignore())
            .ForMember(d => d.Completo, o => o.MapFrom(s => s.Completo));

        // ── Tercero → TerceroDto (detalle completo) ───────────────────────────
        // Los campos de descripción (TipoDocumentoDescripcion, CondicionIvaDescripcion,
        // CobradorNombre, VendedorNombre, etc.) se ignoran aquí porque requieren
        // joins a otras tablas que el QueryHandler resuelve por separado.
        // El Domicilio se mapea como objeto anidado.
        CreateMap<Tercero, TerceroDto>()
            .ForMember(d => d.Domicilio,
                       o => o.MapFrom(s => s.Domicilio))

            // Descripciones resueltas por el Handler (joins a catálogos)
            .ForMember(d => d.TipoDocumentoDescripcion, o => o.Ignore())
            .ForMember(d => d.CondicionIvaDescripcion, o => o.Ignore())
            .ForMember(d => d.CategoriaDescripcion, o => o.Ignore())
            .ForMember(d => d.MonedaDescripcion, o => o.Ignore())
            .ForMember(d => d.CobradorNombre, o => o.Ignore())
            .ForMember(d => d.VendedorNombre, o => o.Ignore())
            .ForMember(d => d.SucursalDescripcion, o => o.Ignore());

        // ── Tercero → TerceroListDto (fila de grilla paginada) ────────────────
        // RolDisplay se calcula en el AfterMap para no duplicar lógica
        // en cada query handler.
        CreateMap<Tercero, TerceroListDto>()
            .ForMember(d => d.CondicionIvaDescripcion, o => o.Ignore())
            .ForMember(d => d.LocalidadDescripcion, o => o.Ignore())
            .AfterMap((src, dst) =>
            {
                dst.RolDisplay = (src.EsCliente, src.EsProveedor, src.EsEmpleado) switch
                {
                    (true, true, true) => "Cliente / Proveedor / Empleado",
                    (true, true, false) => "Cliente / Proveedor",
                    (true, false, true) => "Cliente / Empleado",
                    (false, true, true) => "Proveedor / Empleado",
                    (true, false, false) => "Cliente",
                    (false, true, false) => "Proveedor",
                    (false, false, true) => "Empleado",
                    _ => "Sin rol"
                };
            });

        // ── Tercero → TerceroSelectorDto (combos y lookups) ───────────────────
        // Display es una propiedad calculada en el DTO, no requiere mapeo.
        CreateMap<Tercero, TerceroSelectorDto>();
    }
}