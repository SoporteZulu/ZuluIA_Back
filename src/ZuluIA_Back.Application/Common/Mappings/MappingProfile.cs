using AutoMapper;
using ZuluIA_Back.Application.Features.Contabilidad.DTOs;
using ZuluIA_Back.Application.Features.Finanzas.DTOs;
using ZuluIA_Back.Application.Features.Terceros.DTOs;
using ZuluIA_Back.Domain.Entities.Contabilidad;
using ZuluIA_Back.Domain.Entities.Finanzas;
using ZuluIA_Back.Domain.Entities.Terceros;
using ZuluIA_Back.Domain.ValueObjects;

namespace ZuluIA_Back.Application.Common.Mappings;

public class MappingProfile : Profile
{
    public MappingProfile()
    {
        // ─── Terceros ─────────────────────────────────────────────────────────
        ConfigurarTerceros();

        // ─── Finanzas ─────────────────────────────────────────────────────────
        CreateMap<Cobro, CobroDto>()
            .ForMember(d => d.Estado,             o => o.MapFrom(s => s.Estado.ToString().ToUpperInvariant()))
            .ForMember(d => d.TerceroRazonSocial, o => o.Ignore())
            .ForMember(d => d.MonedaSimbolo,      o => o.Ignore())
            .ForMember(d => d.Medios,             o => o.Ignore());
        CreateMap<CobroMedio, CobroMedioDto>()
            .ForMember(d => d.CajaDescripcion,      o => o.Ignore())
            .ForMember(d => d.FormaPagoDescripcion, o => o.Ignore())
            .ForMember(d => d.MonedaSimbolo,        o => o.Ignore());
        CreateMap<Pago, PagoDto>()
            .ForMember(d => d.Estado,             o => o.MapFrom(s => s.Estado.ToString().ToUpperInvariant()))
            .ForMember(d => d.TerceroRazonSocial, o => o.Ignore())
            .ForMember(d => d.MonedaSimbolo,      o => o.Ignore())
            .ForMember(d => d.Medios,             o => o.Ignore())
            .ForMember(d => d.Retenciones,        o => o.Ignore());
        CreateMap<PagoMedio, PagoMedioDto>()
            .ForMember(d => d.CajaDescripcion,      o => o.Ignore())
            .ForMember(d => d.FormaPagoDescripcion, o => o.Ignore())
            .ForMember(d => d.MonedaSimbolo,        o => o.Ignore());

        // ─── Contabilidad ─────────────────────────────────────────────────────
        CreateMap<Asiento, AsientoDto>()
            .ForMember(d => d.Estado,               o => o.MapFrom(s => s.Estado.ToString()))
            .ForMember(d => d.EjercicioDescripcion, o => o.Ignore());
        CreateMap<AsientoLinea, AsientoLineaDto>()
            // Código y denominación de cuenta resueltos por el Handler
            .ForMember(d => d.CuentaCodigo,           o => o.Ignore())
            .ForMember(d => d.CuentaDenominacion,     o => o.Ignore())
            .ForMember(d => d.CentroCostoDescripcion, o => o.Ignore());
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
            .ForMember(d => d.TipoPersoneria, o => o.MapFrom(s => s.TipoPersoneria.ToString().ToUpperInvariant()))
            .ForMember(d => d.Contactos, o => o.Ignore())
            .ForMember(d => d.SucursalesEntrega, o => o.Ignore())
            .ForMember(d => d.Transportes, o => o.Ignore())
            .ForMember(d => d.VentanasCobranza, o => o.Ignore())
            .ForMember(d => d.PerfilComercial, o => o.Ignore())

            // Campos aplanados del Domicilio (derivados del ValueObject)
            .ForMember(d => d.Calle,               o => o.MapFrom(s => s.Domicilio.Calle))
            .ForMember(d => d.Nro,                 o => o.MapFrom(s => s.Domicilio.Nro))
            .ForMember(d => d.Piso,                o => o.MapFrom(s => s.Domicilio.Piso))
            .ForMember(d => d.Dpto,                o => o.MapFrom(s => s.Domicilio.Dpto))
            .ForMember(d => d.CodigoPostal,        o => o.MapFrom(s => s.Domicilio.CodigoPostal))
            .ForMember(d => d.LocalidadId,         o => o.MapFrom(s => s.Domicilio.LocalidadId))
            .ForMember(d => d.BarrioId,            o => o.MapFrom(s => s.Domicilio.BarrioId))
            .ForMember(d => d.Completo,            o => o.MapFrom(s => s.Domicilio.Completo))

            // Descripciones resueltas por el Handler (joins a catálogos)
            .ForMember(d => d.TipoDocumentoDescripcion, o => o.Ignore())
            .ForMember(d => d.CondicionIvaDescripcion, o => o.Ignore())
            .ForMember(d => d.CategoriaDescripcion, o => o.Ignore())
            .ForMember(d => d.MonedaDescripcion, o => o.Ignore())
            .ForMember(d => d.CobradorNombre, o => o.Ignore())
            .ForMember(d => d.VendedorNombre, o => o.Ignore())
            .ForMember(d => d.SucursalDescripcion, o => o.Ignore())
            .ForMember(d => d.LocalidadDescripcion, o => o.Ignore())
            .ForMember(d => d.BarrioDescripcion, o => o.Ignore());

        // ── Tercero → TerceroListDto (fila de grilla paginada) ────────────────
        // RolDisplay se calcula en el AfterMap para no duplicar lógica
        // en cada query handler.
        CreateMap<Tercero, TerceroListDto>()
            .ForMember(d => d.TipoPersoneria, o => o.MapFrom(s => s.TipoPersoneria.ToString().ToUpperInvariant()))
            .ForMember(d => d.Calle, o => o.MapFrom(s => s.Domicilio.Calle))
            .ForMember(d => d.Nro, o => o.MapFrom(s => s.Domicilio.Nro))
            .ForMember(d => d.Piso, o => o.MapFrom(s => s.Domicilio.Piso))
            .ForMember(d => d.Dpto, o => o.MapFrom(s => s.Domicilio.Dpto))
            .ForMember(d => d.CodigoPostal, o => o.MapFrom(s => s.Domicilio.CodigoPostal))
            .ForMember(d => d.LocalidadId, o => o.MapFrom(s => s.Domicilio.LocalidadId))
            .ForMember(d => d.BarrioId, o => o.MapFrom(s => s.Domicilio.BarrioId))
            .ForMember(d => d.CondicionIvaDescripcion, o => o.Ignore())
            .ForMember(d => d.LocalidadDescripcion, o => o.Ignore())
            // RolDisplay se calcula en el AfterMap — Ignore en la validación estática
            .ForMember(d => d.RolDisplay, o => o.Ignore())
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

        CreateMap<TerceroPerfilComercial, TerceroPerfilComercialDto>()
            .ForMember(d => d.RiesgoCrediticio, o => o.MapFrom(s => s.RiesgoCrediticio.ToString().ToUpperInvariant()))
            .ForMember(d => d.ZonaComercialDescripcion, o => o.Ignore());

        CreateMap<TerceroContacto, TerceroContactoDto>();
        CreateMap<TerceroSucursalEntrega, TerceroSucursalEntregaDto>();
        CreateMap<TerceroTransporte, TerceroTransporteDto>()
            .ForMember(d => d.TransportistaNombre, o => o.Ignore());
        CreateMap<TerceroVentanaCobranza, TerceroVentanaCobranzaDto>();
    }
}