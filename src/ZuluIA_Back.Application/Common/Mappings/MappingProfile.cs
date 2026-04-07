using AutoMapper;
using ZuluIA_Back.Application.Features.Cheques.DTOs;
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
            .ForMember(d => d.Medios,             o => o.Ignore())
            .ForMember(d => d.SucursalDescripcion, o => o.Ignore())
            .ForMember(d => d.TerceroLegajo, o => o.Ignore())
            .ForMember(d => d.VendedorNombre, o => o.Ignore())
            .ForMember(d => d.VendedorLegajo, o => o.Ignore())
            .ForMember(d => d.CobradorNombre, o => o.Ignore())
            .ForMember(d => d.CobradorLegajo, o => o.Ignore())
            .ForMember(d => d.ZonaComercialDescripcion, o => o.Ignore())
            .ForMember(d => d.UsuarioCajeroNombre, o => o.Ignore())
            .ForMember(d => d.TotalEfectivo, o => o.Ignore())
            .ForMember(d => d.TotalCheques, o => o.Ignore())
            .ForMember(d => d.TotalElectronico, o => o.Ignore())
            .ForMember(d => d.CreatedByUsuario, o => o.Ignore())
            .ForMember(d => d.UpdatedByUsuario, o => o.Ignore());
        CreateMap<CobroMedio, CobroMedioDto>()
            .ForMember(d => d.CajaDescripcion,      o => o.Ignore())
            .ForMember(d => d.FormaPagoDescripcion, o => o.Ignore())
            .ForMember(d => d.MonedaSimbolo,        o => o.Ignore())
            .ForMember(d => d.ChequeNumero, o => o.Ignore())
            .ForMember(d => d.ChequeBanco, o => o.Ignore());
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
        CreateMap<Cheque, ChequeDto>()
            .ForMember(d => d.Estado, o => o.MapFrom(s => s.Estado.ToString().ToUpperInvariant()))
            .ForMember(d => d.Tipo, o => o.MapFrom(s => s.Tipo.ToString().ToUpperInvariant()))
            .ForMember(d => d.CajaDescripcion, o => o.Ignore())
            .ForMember(d => d.TerceroRazonSocial, o => o.Ignore())
            .ForMember(d => d.MonedaSimbolo, o => o.Ignore())
            .ForMember(d => d.ChequeraDescripcion, o => o.Ignore())
            .ForMember(d => d.ComprobanteOrigenNumero, o => o.Ignore());

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
            .ForMember(d => d.ProvinciaDescripcion, o => o.Ignore())
            .ForMember(d => d.LocalidadDescripcion, o => o.Ignore())
            .ForMember(d => d.BarrioDescripcion, o => o.Ignore())
            .ForMember(d => d.GeografiaCompleta, o => o.Ignore())
            .ForMember(d => d.UbicacionCompleta, o => o.Ignore())
            .ForMember(d => d.Completo, o => o.MapFrom(s => s.Completo));

        // ── Tercero → TerceroDto (detalle completo) ───────────────────────────
        CreateMap<Tercero, TerceroDto>()
            .ForMember(d => d.Domicilio,
                       o => o.MapFrom(s => s.Domicilio))
            .ForMember(d => d.NroInterno, o => o.MapFrom(s => s.Id))
            .ForMember(d => d.FechaAlta, o => o.Ignore())
            .ForMember(d => d.TipoPersoneria, o => o.MapFrom(s => s.TipoPersoneria.ToString().ToUpperInvariant()))
            .ForMember(d => d.Contactos, o => o.Ignore())
            .ForMember(d => d.SucursalesEntrega, o => o.Ignore())
            .ForMember(d => d.Transportes, o => o.Ignore())
            .ForMember(d => d.VentanasCobranza, o => o.Ignore())
            .ForMember(d => d.PerfilComercial, o => o.Ignore())
            .ForMember(d => d.CuentaCorriente, o => o.Ignore())
            .ForMember(d => d.MediosContacto, o => o.Ignore())
            .ForMember(d => d.EstadoPersonaDescripcion, o => o.Ignore())
            .ForMember(d => d.UsuarioCliente, o => o.Ignore())
            .ForMember(d => d.Domicilios, o => o.Ignore())
            .ForMember(d => d.SucursalEntregaPrincipal, o => o.Ignore())
            .ForMember(d => d.RequiereDefinirEntrega, o => o.Ignore())

            // Campos aplanados del Domicilio (derivados del ValueObject)
            .ForMember(d => d.Calle,               o => o.MapFrom(s => s.Domicilio.Calle))
            .ForMember(d => d.Nro,                 o => o.MapFrom(s => s.Domicilio.Nro))
            .ForMember(d => d.Piso,                o => o.MapFrom(s => s.Domicilio.Piso))
            .ForMember(d => d.Dpto,                o => o.MapFrom(s => s.Domicilio.Dpto))
            .ForMember(d => d.CodigoPostal,        o => o.MapFrom(s => s.Domicilio.CodigoPostal))
            .ForMember(d => d.PaisId,              o => o.MapFrom(s => s.PaisId))
            .ForMember(d => d.ProvinciaId,         o => o.MapFrom(s => s.Domicilio.ProvinciaId))
            .ForMember(d => d.LocalidadId,         o => o.MapFrom(s => s.Domicilio.LocalidadId))
            .ForMember(d => d.BarrioId,            o => o.MapFrom(s => s.Domicilio.BarrioId))
            .ForMember(d => d.Completo,            o => o.MapFrom(s => s.Domicilio.Completo))
            .ForMember(d => d.AplicaComisionCobrador, o => o.MapFrom(s => s.AplicaComisionCobrador))
            .ForMember(d => d.AplicaComisionVendedor, o => o.MapFrom(s => s.AplicaComisionVendedor))

            // Descripciones resueltas por el Handler (joins a catálogos)
            .ForMember(d => d.TipoDocumentoDescripcion, o => o.Ignore())
            .ForMember(d => d.CondicionIvaDescripcion, o => o.Ignore())
            .ForMember(d => d.CategoriaDescripcion, o => o.Ignore())
            .ForMember(d => d.CategoriaClienteDescripcion, o => o.Ignore())
            .ForMember(d => d.EstadoClienteDescripcion, o => o.Ignore())
            .ForMember(d => d.EstadoClienteBloquea, o => o.Ignore())
            .ForMember(d => d.CategoriaProveedorDescripcion, o => o.Ignore())
            .ForMember(d => d.EstadoProveedorDescripcion, o => o.Ignore())
            .ForMember(d => d.EstadoProveedorBloquea, o => o.Ignore())
            .ForMember(d => d.MonedaDescripcion, o => o.Ignore())
            .ForMember(d => d.CobradorUserName, o => o.Ignore())
            .ForMember(d => d.CobradorNombre, o => o.Ignore())
            .ForMember(d => d.VendedorUserName, o => o.Ignore())
            .ForMember(d => d.VendedorNombre, o => o.Ignore())
            .ForMember(d => d.SucursalDescripcion, o => o.Ignore())
            .ForMember(d => d.EstadoCivilDescripcion, o => o.Ignore())
            .ForMember(d => d.AccesoUsuarioCliente, o => o.Ignore())
            .ForMember(d => d.UsuarioClienteUserName, o => o.Ignore())
            .ForMember(d => d.UsuarioClienteGrupoUserName, o => o.Ignore())
            .ForMember(d => d.UsuarioClienteActivo, o => o.Ignore())
            .ForMember(d => d.TieneUsuarioCliente, o => o.MapFrom(s => s.UsuarioId.HasValue))
            .ForMember(d => d.CuentaContableId, o => o.Ignore())
            .ForMember(d => d.CuentaContableCodigo, o => o.Ignore())
            .ForMember(d => d.CuentaContableDescripcion, o => o.Ignore())
            .ForMember(d => d.PaisDescripcion, o => o.Ignore())
            .ForMember(d => d.ProvinciaDescripcion, o => o.Ignore())
            .ForMember(d => d.LocalidadDescripcion, o => o.Ignore())
            .ForMember(d => d.BarrioDescripcion, o => o.Ignore())
            .ForMember(d => d.GeografiaCompleta, o => o.Ignore())
            .ForMember(d => d.UbicacionCompleta, o => o.Ignore())
            .ForMember(d => d.EstadoVisibleDescripcion, o => o.Ignore())
            .ForMember(d => d.EstadoVisibleBloquea, o => o.Ignore())
            .ForMember(d => d.EstadoOperativo, o => o.Ignore())
            .ForMember(d => d.EstadoOperativoDescripcion, o => o.Ignore())
            .ForMember(d => d.EstadoOperativoBloquea, o => o.Ignore());

        // ── Tercero → TerceroListDto (fila de grilla paginada) ────────────────
        CreateMap<Tercero, TerceroListDto>()
            .ForMember(d => d.NroInterno, o => o.MapFrom(s => s.Id))
            .ForMember(d => d.FechaAlta, o => o.Ignore())
            .ForMember(d => d.TipoPersoneria, o => o.MapFrom(s => s.TipoPersoneria.ToString().ToUpperInvariant()))
            .ForMember(d => d.Nombre, o => o.MapFrom(s => s.Nombre))
            .ForMember(d => d.Apellido, o => o.MapFrom(s => s.Apellido))
            .ForMember(d => d.Sexo, o => o.MapFrom(s => s.Sexo))
            .ForMember(d => d.Calle, o => o.MapFrom(s => s.Domicilio.Calle))
            .ForMember(d => d.Nro, o => o.MapFrom(s => s.Domicilio.Nro))
            .ForMember(d => d.Piso, o => o.MapFrom(s => s.Domicilio.Piso))
            .ForMember(d => d.Dpto, o => o.MapFrom(s => s.Domicilio.Dpto))
            .ForMember(d => d.CodigoPostal, o => o.MapFrom(s => s.Domicilio.CodigoPostal))
            .ForMember(d => d.PaisId, o => o.MapFrom(s => s.PaisId))
            .ForMember(d => d.ProvinciaId, o => o.MapFrom(s => s.Domicilio.ProvinciaId))
            .ForMember(d => d.LocalidadId, o => o.MapFrom(s => s.Domicilio.LocalidadId))
            .ForMember(d => d.BarrioId, o => o.MapFrom(s => s.Domicilio.BarrioId))
            .ForMember(d => d.AplicaComisionCobrador, o => o.MapFrom(s => s.AplicaComisionCobrador))
            .ForMember(d => d.AplicaComisionVendedor, o => o.MapFrom(s => s.AplicaComisionVendedor))
            .ForMember(d => d.CategoriaClienteDescripcion, o => o.Ignore())
            .ForMember(d => d.CategoriaDescripcion, o => o.Ignore())
            .ForMember(d => d.EstadoClienteDescripcion, o => o.Ignore())
            .ForMember(d => d.EstadoClienteBloquea, o => o.Ignore())
            .ForMember(d => d.CategoriaProveedorDescripcion, o => o.Ignore())
            .ForMember(d => d.EstadoProveedorDescripcion, o => o.Ignore())
            .ForMember(d => d.EstadoProveedorBloquea, o => o.Ignore())
            .ForMember(d => d.MonedaDescripcion, o => o.Ignore())
            .ForMember(d => d.CobradorUserName, o => o.Ignore())
            .ForMember(d => d.CobradorNombre, o => o.Ignore())
            .ForMember(d => d.VendedorUserName, o => o.Ignore())
            .ForMember(d => d.VendedorNombre, o => o.Ignore())
            .ForMember(d => d.CondicionIvaDescripcion, o => o.Ignore())
            .ForMember(d => d.EstadoCivilDescripcion, o => o.Ignore())
            .ForMember(d => d.AccesoUsuarioCliente, o => o.Ignore())
            .ForMember(d => d.UsuarioClienteUserName, o => o.Ignore())
            .ForMember(d => d.UsuarioClienteGrupoUserName, o => o.Ignore())
            .ForMember(d => d.TieneUsuarioCliente, o => o.MapFrom(s => s.UsuarioId.HasValue))
            .ForMember(d => d.UsuarioClienteActivo, o => o.Ignore())
            .ForMember(d => d.LimiteSaldo, o => o.Ignore())
            .ForMember(d => d.LimiteCreditoResumen, o => o.Ignore())
            .ForMember(d => d.LimiteSaldoResumen, o => o.Ignore())
            .ForMember(d => d.VigenciaLimiteSaldoDesde, o => o.Ignore())
            .ForMember(d => d.VigenciaLimiteSaldoHasta, o => o.Ignore())
            .ForMember(d => d.VigenciaCreditoResumen, o => o.Ignore())
            .ForMember(d => d.VigenciaLimiteSaldoResumen, o => o.Ignore())
            .ForMember(d => d.PaisDescripcion, o => o.Ignore())
            .ForMember(d => d.ProvinciaDescripcion, o => o.Ignore())
            .ForMember(d => d.LocalidadDescripcion, o => o.Ignore())
            .ForMember(d => d.BarrioDescripcion, o => o.Ignore())
            .ForMember(d => d.GeografiaCompleta, o => o.Ignore())
            .ForMember(d => d.UbicacionCompleta, o => o.Ignore())
            .ForMember(d => d.SucursalDescripcion, o => o.Ignore())
            .ForMember(d => d.EstadoVisibleDescripcion, o => o.Ignore())
            .ForMember(d => d.EstadoVisibleBloquea, o => o.Ignore())
            .ForMember(d => d.EstadoOperativo, o => o.Ignore())
            .ForMember(d => d.EstadoOperativoDescripcion, o => o.Ignore())
            .ForMember(d => d.EstadoOperativoBloquea, o => o.Ignore())
            .ForMember(d => d.EstadoPersonaDescripcion, o => o.Ignore())
            .ForMember(d => d.TieneSucursalesEntrega, o => o.Ignore())
            .ForMember(d => d.SucursalEntregaPrincipalDescripcion, o => o.Ignore())
            .ForMember(d => d.RequiereDefinirEntrega, o => o.Ignore())
            .ForMember(d => d.PuedeVender, o => o.Ignore())
            .ForMember(d => d.PuedeComprar, o => o.Ignore())
            .ForMember(d => d.MotivoBloqueoVentas, o => o.Ignore())
            .ForMember(d => d.MotivoBloqueoCompras, o => o.Ignore())
            // RolDisplay se calcula en el AfterMap
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
        CreateMap<Tercero, TerceroSelectorDto>()
            .ForMember(d => d.GeografiaCompleta, o => o.Ignore())
            .ForMember(d => d.UbicacionCompleta, o => o.Ignore());

        CreateMap<TerceroPerfilComercial, TerceroPerfilComercialDto>()
            .ForMember(d => d.RiesgoCrediticio, o => o.MapFrom(s => s.RiesgoCrediticio.ToString().ToUpperInvariant()))
            .ForMember(d => d.ZonaComercialDescripcion, o => o.Ignore());

        CreateMap<PersonaDomicilio, TerceroDomicilioDto>()
            .ForMember(d => d.TerceroId, o => o.MapFrom(s => s.PersonaId))
            .ForMember(d => d.GeografiaCompleta, o => o.Ignore())
            .ForMember(d => d.UbicacionCompleta, o => o.Ignore())
            .ForMember(d => d.ProvinciaDescripcion, o => o.Ignore())
            .ForMember(d => d.LocalidadDescripcion, o => o.Ignore())
            .ForMember(d => d.TipoDomicilioDescripcion, o => o.Ignore());
        CreateMap<TerceroContacto, TerceroContactoDto>();
        CreateMap<TerceroSucursalEntrega, TerceroSucursalEntregaDto>();
        CreateMap<TerceroTransporte, TerceroTransporteDto>()
            .ForMember(d => d.TransportistaNombre, o => o.Ignore());
        CreateMap<TerceroVentanaCobranza, TerceroVentanaCobranzaDto>();
    }
}