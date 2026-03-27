using FluentAssertions;
using Xunit;
using ZuluIA_Back.Domain.Entities.Comprobantes;
using ZuluIA_Back.Domain.Entities.Configuracion;
using ZuluIA_Back.Domain.Entities.Contabilidad;
using ZuluIA_Back.Domain.Entities.Extras;
using ZuluIA_Back.Domain.Entities.Finanzas;
using ZuluIA_Back.Domain.Entities.Geografia;
using ZuluIA_Back.Domain.Entities.Items;
using ZuluIA_Back.Domain.Entities.Proyectos;
using ZuluIA_Back.Domain.Entities.Referencia;
using ZuluIA_Back.Domain.Entities.Sucursales;
using ZuluIA_Back.Domain.Entities.Usuarios;
using ZuluIA_Back.Domain.Entities.Ventas;

namespace ZuluIA_Back.UnitTests.Domain;

// ─────────────────────────────────────────────
// ComprobanteDetalleCosto
// ─────────────────────────────────────────────
public class ComprobanteDetalleCostoTests
{
    [Fact]
    public void Crear_ConDatosValidos_AsignaValores()
    {
        var cdc = ComprobanteDetalleCosto.Crear(5, 3);

        cdc.ComprobanteItemId.Should().Be(5);
        cdc.CentroCostoId.Should().Be(3);
        cdc.Procesado.Should().BeFalse();
    }

    [Fact]
    public void Crear_ComprobanteItemIdCero_LanzaExcepcion()
    {
        var act = () => ComprobanteDetalleCosto.Crear(0, 1);
        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void Crear_CentroCostoIdCero_LanzaExcepcion()
    {
        var act = () => ComprobanteDetalleCosto.Crear(1, 0);
        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void MarcarProcesado_CambiaProcesadoATrue()
    {
        var cdc = ComprobanteDetalleCosto.Crear(1, 1);
        cdc.MarcarProcesado();
        cdc.Procesado.Should().BeTrue();
    }

    [Fact]
    public void DesmarcarProcesado_CambiaProcesadoAFalse()
    {
        var cdc = ComprobanteDetalleCosto.Crear(1, 1);
        cdc.MarcarProcesado();
        cdc.DesmarcarProcesado();
        cdc.Procesado.Should().BeFalse();
    }
}

// ─────────────────────────────────────────────
// TipoEntrega
// ─────────────────────────────────────────────
public class TipoEntregaTests
{
    [Fact]
    public void Crear_ConDatosValidos_NormalizaCodigoDescripcion()
    {
        var te = TipoEntrega.Crear(" envio ", " Envío a domicilio ");

        te.Codigo.Should().Be("ENVIO");
        te.Descripcion.Should().Be("Envío a domicilio");
    }

    [Fact]
    public void Crear_CodigoVacio_LanzaExcepcion()
    {
        var act = () => TipoEntrega.Crear("", "Descripcion");
        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void Crear_DescripcionVacia_LanzaExcepcion()
    {
        var act = () => TipoEntrega.Crear("ENVIO", "");
        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void Actualizar_ConDescripcionValida_Actualiza()
    {
        var te = TipoEntrega.Crear("ENV", "Envío");
        te.Actualizar(" Retiro ", null, null);
        te.Descripcion.Should().Be("Retiro");
    }

    [Fact]
    public void Actualizar_DescripcionVacia_LanzaExcepcion()
    {
        var te = TipoEntrega.Crear("ENV", "Envío");
        var act = () => te.Actualizar("", null, null);
        act.Should().Throw<ArgumentException>();
    }
}

// ─────────────────────────────────────────────
// ComprobanteImpuesto
// ─────────────────────────────────────────────
public class ComprobanteImpuestoTests
{
    [Fact]
    public void Crear_AsignaTodasLasPropiedades()
    {
        var ci = ComprobanteImpuesto.Crear(10, 2, 21m, 1000m, 210m);

        ci.ComprobanteId.Should().Be(10);
        ci.AlicuotaIvaId.Should().Be(2);
        ci.PorcentajeIva.Should().Be(21m);
        ci.BaseImponible.Should().Be(1000m);
        ci.ImporteIva.Should().Be(210m);
    }
}

// ─────────────────────────────────────────────
// ComprobanteRelacion
// ─────────────────────────────────────────────
public class ComprobanteRelacionTests
{
    [Fact]
    public void Crear_AsignaOrigenDestino()
    {
        var cr = ComprobanteRelacion.Crear(1, 2, "referencia");

        cr.ComprobanteOrigenId.Should().Be(1);
        cr.ComprobanteDestinoId.Should().Be(2);
        cr.Observacion.Should().Be("referencia");
    }

    [Fact]
    public void Crear_SinObservacion_ObservacionNula()
    {
        var cr = ComprobanteRelacion.Crear(1, 2);
        cr.Observacion.Should().BeNull();
    }
}

// ─────────────────────────────────────────────
// Aspecto
// ─────────────────────────────────────────────
public class AspectoTests
{
    [Fact]
    public void Crear_ConDatosValidos_NormalizaCodigoDescripcion()
    {
        var a = Aspecto.Crear(" asp1 ", " descripcion ");

        a.Codigo.Should().Be("ASP1");
        a.Descripcion.Should().Be("descripcion");
    }

    [Fact]
    public void Crear_CodigoVacio_LanzaExcepcion()
    {
        var act = () => Aspecto.Crear("", "Descripcion");
        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void Crear_DescripcionVacia_LanzaExcepcion()
    {
        var act = () => Aspecto.Crear("ASP", "");
        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void Actualizar_ConDescripcionValida_Actualiza()
    {
        var a = Aspecto.Crear("ASP", "Original");
        a.Actualizar(" Nueva descripcion ", null, 1, 0, null, null);
        a.Descripcion.Should().Be("Nueva descripcion");
    }

    [Fact]
    public void Actualizar_DescripcionVacia_LanzaExcepcion()
    {
        var a = Aspecto.Crear("ASP", "Original");
        var act = () => a.Actualizar("", null, 0, 0, null, null);
        act.Should().Throw<ArgumentException>();
    }
}

// ─────────────────────────────────────────────
// ConfiguracionSistema
// ─────────────────────────────────────────────
public class ConfiguracionSistemaTests
{
    [Fact]
    public void Crear_ConDatosValidos_NormalizaCampoAMayusculas()
    {
        var cs = ConfiguracionSistema.Crear(" smtp_host ", "localhost", 1, null);

        cs.Campo.Should().Be("SMTP_HOST");
        cs.Valor.Should().Be("localhost");
    }

    [Fact]
    public void Crear_CampoVacio_LanzaExcepcion()
    {
        var act = () => ConfiguracionSistema.Crear("", "valor", 1, null);
        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void SetValor_ActualizaValor()
    {
        var cs = ConfiguracionSistema.Crear("CAMPO", "viejo", 1, null);
        cs.SetValor("nuevo");
        cs.Valor.Should().Be("nuevo");
    }

    [Fact]
    public void SetValor_Nulo_EstableceNull()
    {
        var cs = ConfiguracionSistema.Crear("CAMPO", "viejo", 1, null);
        cs.SetValor(null);
        cs.Valor.Should().BeNull();
    }
}

// ─────────────────────────────────────────────
// PlanillaDiagnostico
// ─────────────────────────────────────────────
public class PlanillaDiagnosticoTests
{
    [Fact]
    public void Crear_ConClienteId_AsignaClienteId()
    {
        var p = PlanillaDiagnostico.Crear(42);

        p.ClienteId.Should().Be(42);
        p.FechaRegistro.Should().NotBeNull();
    }

    [Fact]
    public void Crear_SinParametros_CreaConValoresNulos()
    {
        var p = PlanillaDiagnostico.Crear(null);

        p.ClienteId.Should().BeNull();
        p.PlantillaId.Should().BeNull();
        p.EstadoId.Should().BeNull();
    }

    [Fact]
    public void Actualizar_CambiaClienteId()
    {
        var p = PlanillaDiagnostico.Crear(1);
        p.Actualizar(99, null, null, null, null, null, null, null, "obs");
        p.ClienteId.Should().Be(99);
        p.Observaciones.Should().Be("obs");
    }
}

// ─────────────────────────────────────────────
// PlanillaDiagnosticoDetalle
// ─────────────────────────────────────────────
public class PlanillaDiagnosticoDetalleTests
{
    [Fact]
    public void Crear_ConDatosValidos_AsignaPlanillaId()
    {
        var d = PlanillaDiagnosticoDetalle.Crear(5, 1, 2, 80m, 75m, 100m, null);

        d.PlanillaId.Should().Be(5);
        d.VariableDetalleId.Should().Be(1);
        d.PuntajeVariable.Should().Be(80m);
    }

    [Fact]
    public void Crear_PlanillaIdCero_LanzaExcepcion()
    {
        var act = () => PlanillaDiagnosticoDetalle.Crear(0, null, null);
        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void Actualizar_CambiaPuntaje()
    {
        var d = PlanillaDiagnosticoDetalle.Crear(1, null, null, 50m);
        d.Actualizar(null, 90m, 85m, 100m, null);
        d.PuntajeVariable.Should().Be(90m);
        d.Valor.Should().Be(85m);
    }
}

// ─────────────────────────────────────────────
// PlantillaDiagnostico
// ─────────────────────────────────────────────
public class PlantillaDiagnosticoTests
{
    [Fact]
    public void Crear_ConDescripcionValida_TrimsDescripcion()
    {
        var p = PlantillaDiagnostico.Crear(" Mi Plantilla ");

        p.Descripcion.Should().Be("Mi Plantilla");
        p.FechaRegistro.Should().NotBeNull();
    }

    [Fact]
    public void Crear_DescripcionVacia_LanzaExcepcion()
    {
        var act = () => PlantillaDiagnostico.Crear("");
        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void Actualizar_ConDescripcionValida_Actualiza()
    {
        var p = PlantillaDiagnostico.Crear("Original");
        p.Actualizar(" Actualizada ", null, null, "obs");
        p.Descripcion.Should().Be("Actualizada");
        p.Observaciones.Should().Be("obs");
    }

    [Fact]
    public void Actualizar_DescripcionVacia_LanzaExcepcion()
    {
        var p = PlantillaDiagnostico.Crear("Original");
        var act = () => p.Actualizar("", null, null, null);
        act.Should().Throw<ArgumentException>();
    }
}

// ─────────────────────────────────────────────
// PlantillaDiagnosticoDetalle
// ─────────────────────────────────────────────
public class PlantillaDiagnosticoDetalleTests
{
    [Fact]
    public void Crear_ConDatosValidos_AsignaPlantillaId()
    {
        var d = PlantillaDiagnosticoDetalle.Crear(7, 3, 50m, 100m);

        d.PlantillaId.Should().Be(7);
        d.VariableDetalleId.Should().Be(3);
        d.PorcentajeIncidencia.Should().Be(50m);
    }

    [Fact]
    public void Crear_PlantillaIdCero_LanzaExcepcion()
    {
        var act = () => PlantillaDiagnosticoDetalle.Crear(0, null);
        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void Actualizar_CambiaPorcentaje()
    {
        var d = PlantillaDiagnosticoDetalle.Crear(1, null, 30m);
        d.Actualizar(null, 70m, null);
        d.PorcentajeIncidencia.Should().Be(70m);
    }
}

// ─────────────────────────────────────────────
// PlanCuenta
// ─────────────────────────────────────────────
public class PlanCuentaTests
{
    private static PlanCuenta CrearValida(char? saldoNormal = 'D') =>
        PlanCuenta.Crear(1, null, "1.1.01", "Caja", 1, "1", true, null, saldoNormal);

    [Fact]
    public void Crear_ConDatosValidos_AsignaValores()
    {
        var pc = CrearValida();

        pc.CodigoCuenta.Should().Be("1.1.01");
        pc.Denominacion.Should().Be("Caja");
        pc.Imputable.Should().BeTrue();
    }

    [Fact]
    public void Crear_CodigoCuentaVacio_LanzaExcepcion()
    {
        var act = () => PlanCuenta.Crear(1, null, "", "Caja", 1, "1", true, null, null);
        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void Crear_DenominacionVacia_LanzaExcepcion()
    {
        var act = () => PlanCuenta.Crear(1, null, "1.1.01", "", 1, "1", true, null, null);
        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void EsDeudora_CuandoSaldoNormalEsD_RetornaTrue()
    {
        var pc = CrearValida('D');
        pc.EsDeudora.Should().BeTrue();
        pc.EsAcreedora.Should().BeFalse();
    }

    [Fact]
    public void EsAcreedora_CuandoSaldoNormalEsA_RetornaTrue()
    {
        var pc = CrearValida('A');
        pc.EsAcreedora.Should().BeTrue();
        pc.EsDeudora.Should().BeFalse();
    }

    [Fact]
    public void EsDeudora_CuandoSaldoNormalMinuscula_RetornaTrue()
    {
        var pc = CrearValida('d');
        pc.EsDeudora.Should().BeTrue();
    }

    [Fact]
    public void EsAcreedora_CuandoSaldoNormalMinuscula_RetornaTrue()
    {
        var pc = CrearValida('a');
        pc.EsAcreedora.Should().BeTrue();
    }

    [Fact]
    public void EsDeudora_CuandoSaldoNormalNulo_RetornaFalse()
    {
        var pc = CrearValida(null);
        pc.EsDeudora.Should().BeFalse();
        pc.EsAcreedora.Should().BeFalse();
    }

    [Fact]
    public void Actualizar_ConDenominacionValida_Actualiza()
    {
        var pc = CrearValida();
        pc.Actualizar("Caja Principal", false, null, 'A');
        pc.Denominacion.Should().Be("Caja Principal");
        pc.Imputable.Should().BeFalse();
    }

    [Fact]
    public void Actualizar_DenominacionVacia_LanzaExcepcion()
    {
        var pc = CrearValida();
        var act = () => pc.Actualizar("", true, null, null);
        act.Should().Throw<ArgumentException>();
    }
}

// ─────────────────────────────────────────────
// PlanCuentaParametro
// ─────────────────────────────────────────────
public class PlanCuentaParametroTests
{
    [Fact]
    public void Crear_ConDatosValidos_NormalizaTablaAMinusculas()
    {
        var pcp = PlanCuentaParametro.Crear(1, 2, " TipoCOMPROBANTE ", 3);

        pcp.Tabla.Should().Be("tipocomprobante");
        pcp.EjercicioId.Should().Be(1);
        pcp.CuentaId.Should().Be(2);
        pcp.IdRegistro.Should().Be(3);
    }

    [Fact]
    public void Crear_TablaVacia_LanzaExcepcion()
    {
        var act = () => PlanCuentaParametro.Crear(1, 2, "", 3);
        act.Should().Throw<ArgumentException>();
    }
}

// ─────────────────────────────────────────────
// EjercicioSucursal
// ─────────────────────────────────────────────
public class EjercicioSucursalTests
{
    [Fact]
    public void Crear_ConDatosValidos_AsignaValores()
    {
        var es = EjercicioSucursal.Crear(1, 2, true);

        es.EjercicioId.Should().Be(1);
        es.SucursalId.Should().Be(2);
        es.UsaContabilidad.Should().BeTrue();
    }

    [Fact]
    public void Crear_ValorPorDefecto_UsaContabilidadTrue()
    {
        var es = EjercicioSucursal.Crear(1, 1);
        es.UsaContabilidad.Should().BeTrue();
    }

    [Fact]
    public void SetUsaContabilidad_CambiaValor()
    {
        var es = EjercicioSucursal.Crear(1, 1, true);
        es.SetUsaContabilidad(false);
        es.UsaContabilidad.Should().BeFalse();
    }
}

// ─────────────────────────────────────────────
// Busqueda
// ─────────────────────────────────────────────
public class BusquedaTests
{
    [Fact]
    public void Crear_ConDatosValidos_NormalizaModuloAMinusculas()
    {
        var b = Busqueda.Crear("Mi Búsqueda", "CLIENTES", "{}", null, false);

        b.Nombre.Should().Be("Mi Búsqueda");
        b.Modulo.Should().Be("clientes");
        b.EsGlobal.Should().BeFalse();
    }

    [Fact]
    public void Crear_NombreVacio_LanzaExcepcion()
    {
        var act = () => Busqueda.Crear("", "clientes", "{}", null, false);
        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void Crear_ModuloVacio_LanzaExcepcion()
    {
        var act = () => Busqueda.Crear("Nombre", "", "{}", null, false);
        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void Actualizar_ConNombreValido_Actualiza()
    {
        var b = Busqueda.Crear("Original", "ventas", "{}", null, false);
        b.Actualizar("Modificado", "{nuevo}", true);
        b.Nombre.Should().Be("Modificado");
        b.EsGlobal.Should().BeTrue();
    }

    [Fact]
    public void Actualizar_NombreVacio_LanzaExcepcion()
    {
        var b = Busqueda.Crear("Original", "ventas", "{}", null, false);
        var act = () => b.Actualizar("", "{}", false);
        act.Should().Throw<ArgumentException>();
    }
}

// ─────────────────────────────────────────────
// Transportista
// ─────────────────────────────────────────────
public class TransportistaTests
{
    [Fact]
    public void Crear_ConPatente_NormalizaAMayusculas()
    {
        var t = Transportista.Crear(1, "30-12345678-9", "Calle 123", " ab123cd ", "Ford");

        t.Patente.Should().Be("AB123CD");
        t.Activo.Should().BeTrue();
        t.TerceroId.Should().Be(1);
    }

    [Fact]
    public void Crear_SinPatente_PatentesNula()
    {
        var t = Transportista.Crear(1, null, null, null, null);
        t.Patente.Should().BeNull();
    }

    [Fact]
    public void Actualizar_CambiaPatente()
    {
        var t = Transportista.Crear(1, null, null, "AB111CD", null);
        t.Actualizar("Nueva calle", " zz999zz ", "Toyota");
        t.Patente.Should().Be("ZZ999ZZ");
        t.DomicilioPartida.Should().Be("Nueva calle");
    }

    [Fact]
    public void Desactivar_CambiaActivoAFalse()
    {
        var t = Transportista.Crear(1, null, null, null, null);
        t.Desactivar();
        t.Activo.Should().BeFalse();
    }

    [Fact]
    public void Activar_CambiaActivoATrue()
    {
        var t = Transportista.Crear(1, null, null, null, null);
        t.Desactivar();
        t.Activar();
        t.Activo.Should().BeTrue();
    }
}

// ─────────────────────────────────────────────
// FormaPagoCaja
// ─────────────────────────────────────────────
public class FormaPagoCajaTests
{
    [Fact]
    public void Crear_ConDatosValidos_HabilitadoPorDefecto()
    {
        var fp = FormaPagoCaja.Crear(1, 2, 3);

        fp.CajaId.Should().Be(1);
        fp.FormaPagoId.Should().Be(2);
        fp.MonedaId.Should().Be(3);
        fp.Habilitado.Should().BeTrue();
    }

    [Fact]
    public void Deshabilitar_CambiaHabilitadoAFalse()
    {
        var fp = FormaPagoCaja.Crear(1, 2, 3);
        fp.Deshabilitar();
        fp.Habilitado.Should().BeFalse();
    }

    [Fact]
    public void Habilitar_CambiaHabilitadoATrue()
    {
        var fp = FormaPagoCaja.Crear(1, 2, 3);
        fp.Deshabilitar();
        fp.Habilitar();
        fp.Habilitado.Should().BeTrue();
    }
}

// ─────────────────────────────────────────────
// RetencionXPersona
// ─────────────────────────────────────────────
public class RetencionXPersonaTests
{
    [Fact]
    public void Crear_ConDatosValidos_AsignaValores()
    {
        var r = RetencionXPersona.Crear(10, 5, " IIBB ", null);

        r.TerceroId.Should().Be(10);
        r.TipoRetencionId.Should().Be(5);
        r.Descripcion.Should().Be("IIBB");
    }

    [Fact]
    public void Actualizar_CambiaDescripcion()
    {
        var r = RetencionXPersona.Crear(1, 1, "Original", null);
        r.Actualizar("Modificada", null);
        r.Descripcion.Should().Be("Modificada");
    }

    [Fact]
    public void Eliminar_SetDeletedEsInvocado()
    {
        var r = RetencionXPersona.Crear(1, 1, "Descripcion", null);
        r.Eliminar(null);
        // Eliminar llama SetDeleted + SetUpdated — no lanza excepción
    }
}

// ─────────────────────────────────────────────
// MovimientoCtaCte
// ─────────────────────────────────────────────
public class MovimientoCtaCteTests
{
    [Fact]
    public void Crear_AsignaTodasLasPropiedades()
    {
        var fecha = new DateOnly(2025, 1, 15);
        var m = MovimientoCtaCte.Crear(1, 2, 3, 4, fecha, 100m, 0m, 100m, "Factura");

        m.TerceroId.Should().Be(1);
        m.SucursalId.Should().Be(2);
        m.MonedaId.Should().Be(3);
        m.ComprobanteId.Should().Be(4);
        m.Fecha.Should().Be(fecha);
        m.Debe.Should().Be(100m);
        m.Haber.Should().Be(0m);
        m.Saldo.Should().Be(100m);
        m.Descripcion.Should().Be("Factura");
    }

    [Fact]
    public void Crear_SinSucursal_SucursalIdNulo()
    {
        var m = MovimientoCtaCte.Crear(1, null, 1, null, DateOnly.FromDateTime(DateTime.Today), 0m, 50m, -50m, null);
        m.SucursalId.Should().BeNull();
        m.Descripcion.Should().BeNull();
    }
}

// ─────────────────────────────────────────────
// Barrio
// ─────────────────────────────────────────────
public class BarrioTests
{
    [Fact]
    public void Crear_ConDescripcionValida_TrimsDescripcion()
    {
        var b = Barrio.Crear(1, " Centro ");

        b.LocalidadId.Should().Be(1);
        b.Descripcion.Should().Be("Centro");
    }

    [Fact]
    public void Crear_DescripcionVacia_LanzaExcepcion()
    {
        var act = () => Barrio.Crear(1, "");
        act.Should().Throw<ArgumentException>();
    }
}

// ─────────────────────────────────────────────
// Pais
// ─────────────────────────────────────────────
public class PaisTests
{
    [Fact]
    public void Crear_ConDatosValidos_NormalizaCodigo()
    {
        var p = Pais.Crear(" ar ", " Argentina ");

        p.Codigo.Should().Be("AR");
        p.Descripcion.Should().Be("Argentina");
    }

    [Fact]
    public void Crear_CodigoVacio_LanzaExcepcion()
    {
        var act = () => Pais.Crear("", "Argentina");
        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void Crear_DescripcionVacia_LanzaExcepcion()
    {
        var act = () => Pais.Crear("AR", "");
        act.Should().Throw<ArgumentException>();
    }
}

// ─────────────────────────────────────────────
// Provincia
// ─────────────────────────────────────────────
public class ProvinciaTests
{
    [Fact]
    public void Crear_ConDatosValidos_NormalizaCodigo()
    {
        var p = Provincia.Crear(1, " ba ", " Buenos Aires ");

        p.PaisId.Should().Be(1);
        p.Codigo.Should().Be("BA");
        p.Descripcion.Should().Be("Buenos Aires");
    }

    [Fact]
    public void Crear_CodigoVacio_LanzaExcepcion()
    {
        var act = () => Provincia.Crear(1, "", "Buenos Aires");
        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void Crear_DescripcionVacia_LanzaExcepcion()
    {
        var act = () => Provincia.Crear(1, "BA", "");
        act.Should().Throw<ArgumentException>();
    }
}

// ─────────────────────────────────────────────
// Region
// ─────────────────────────────────────────────
public class RegionTests
{
    [Fact]
    public void Crear_ConDatosValidos_NormalizaCodigo()
    {
        var r = Region.Crear(" norte ", " Región Norte ");

        r.Codigo.Should().Be("NORTE");
        r.Descripcion.Should().Be("Región Norte");
    }

    [Fact]
    public void Crear_CodigoVacio_LanzaExcepcion()
    {
        var act = () => Region.Crear("", "Región");
        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void Crear_DescripcionVacia_LanzaExcepcion()
    {
        var act = () => Region.Crear("REG", "");
        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void Actualizar_ConDescripcionValida_Actualiza()
    {
        var r = Region.Crear("REG", "Original");
        r.Actualizar(" Actualizada ", null, 1, 0, null, false, null);
        r.Descripcion.Should().Be("Actualizada");
    }

    [Fact]
    public void Actualizar_DescripcionVacia_LanzaExcepcion()
    {
        var r = Region.Crear("REG", "Original");
        var act = () => r.Actualizar("", null, 0, 0, null, false, null);
        act.Should().Throw<ArgumentException>();
    }
}

// ─────────────────────────────────────────────
// CategoriaItem
// ─────────────────────────────────────────────
public class CategoriaItemTests
{
    [Fact]
    public void Crear_ConDatosValidos_NormalizaCodigo()
    {
        var c = CategoriaItem.Crear(null, " cat01 ", " Categoría Principal ", (short)1, null, null);

        c.Codigo.Should().Be("CAT01");
        c.Descripcion.Should().Be("Categoría Principal");
        c.Activo.Should().BeTrue();
    }

    [Fact]
    public void Crear_CodigoVacio_LanzaExcepcion()
    {
        var act = () => CategoriaItem.Crear(null, "", "Descripcion", (short)1, null, null);
        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void Crear_DescripcionVacia_LanzaExcepcion()
    {
        var act = () => CategoriaItem.Crear(null, "CAT", "", (short)1, null, null);
        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void Actualizar_ConDatosValidos_Actualiza()
    {
        var c = CategoriaItem.Crear(null, "CAT", "Original", (short)1, null, null);
        c.Actualizar(" CAT02 ", " Nueva Categoría ", null, null);
        c.Codigo.Should().Be("CAT02");
        c.Descripcion.Should().Be("Nueva Categoría");
    }

    [Fact]
    public void Actualizar_CodigoVacio_LanzaExcepcion()
    {
        var c = CategoriaItem.Crear(null, "CAT", "Original", (short)1, null, null);
        var act = () => c.Actualizar("", "Descripcion", null, null);
        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void Actualizar_DescripcionVacia_LanzaExcepcion()
    {
        var c = CategoriaItem.Crear(null, "CAT", "Original", (short)1, null, null);
        var act = () => c.Actualizar("CAT", "", null, null);
        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void Desactivar_CambiaActivoAFalse()
    {
        var c = CategoriaItem.Crear(null, "CAT", "Desc", (short)1, null, null);
        c.Desactivar(null);
        c.Activo.Should().BeFalse();
    }

    [Fact]
    public void Activar_CambiaActivoATrue()
    {
        var c = CategoriaItem.Crear(null, "CAT", "Desc", (short)1, null, null);
        c.Desactivar(null);
        c.Activar(null);
        c.Activo.Should().BeTrue();
    }
}

// ─────────────────────────────────────────────
// HistorialPrecio
// ─────────────────────────────────────────────
public class HistorialPrecioTests
{
    [Fact]
    public void Crear_AsignaTodasLasPropiedades()
    {
        var hp = HistorialPrecio.Crear(1, 100m, 200m, 110m, 220m, null, "Actualización");

        hp.ItemId.Should().Be(1);
        hp.PrecioCostoAnterior.Should().Be(100m);
        hp.PrecioVentaAnterior.Should().Be(200m);
        hp.PrecioCostoNuevo.Should().Be(110m);
        hp.PrecioVentaNuevo.Should().Be(220m);
        hp.Motivo.Should().Be("Actualización");
        hp.UsuarioId.Should().BeNull();
    }
}

// ─────────────────────────────────────────────
// ComprobanteProyecto
// ─────────────────────────────────────────────
public class ComprobanteProyectoTests
{
    [Fact]
    public void Crear_ConDatosValidos_AsignaValores()
    {
        var cp = ComprobanteProyecto.Crear(1, 2, 50m, 5000m);

        cp.ComprobanteId.Should().Be(1);
        cp.ProyectoId.Should().Be(2);
        cp.Porcentaje.Should().Be(50m);
        cp.Importe.Should().Be(5000m);
        cp.Deshabilitada.Should().BeFalse();
    }

    [Fact]
    public void Crear_ComprobanteIdCero_LanzaExcepcion()
    {
        var act = () => ComprobanteProyecto.Crear(0, 1, 50m, 1000m);
        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void Crear_ProyectoIdCero_LanzaExcepcion()
    {
        var act = () => ComprobanteProyecto.Crear(1, 0, 50m, 1000m);
        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void Crear_PorcentajeNegativo_LanzaExcepcion()
    {
        var act = () => ComprobanteProyecto.Crear(1, 1, -1m, 1000m);
        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void Crear_PorcentajeMayorA100_LanzaExcepcion()
    {
        var act = () => ComprobanteProyecto.Crear(1, 1, 101m, 1000m);
        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void Crear_PorcentajeCero_EsValido()
    {
        var cp = ComprobanteProyecto.Crear(1, 1, 0m, 0m);
        cp.Porcentaje.Should().Be(0m);
    }

    [Fact]
    public void Crear_Porcentaje100_EsValido()
    {
        var cp = ComprobanteProyecto.Crear(1, 1, 100m, 1000m);
        cp.Porcentaje.Should().Be(100m);
    }

    [Fact]
    public void Deshabilitar_CambiaDeshabilitadaATrue()
    {
        var cp = ComprobanteProyecto.Crear(1, 1, 50m, 1000m);
        cp.Deshabilitar();
        cp.Deshabilitada.Should().BeTrue();
    }

    [Fact]
    public void Habilitar_CambiaDeshabilitadaAFalse()
    {
        var cp = ComprobanteProyecto.Crear(1, 1, 50m, 1000m);
        cp.Deshabilitar();
        cp.Habilitar();
        cp.Deshabilitada.Should().BeFalse();
    }

    [Fact]
    public void ActualizarImporte_CambiaImporte()
    {
        var cp = ComprobanteProyecto.Crear(1, 1, 50m, 1000m);
        cp.ActualizarImporte(2500m);
        cp.Importe.Should().Be(2500m);
    }
}

// ─────────────────────────────────────────────
// CategoriaTercero
// ─────────────────────────────────────────────
public class CategoriaTerceroTests
{
    [Fact]
    public void Crear_ConDescripcionValida_TrimsDescripcion()
    {
        var ct = CategoriaTercero.Crear(" Categoría A ");

        ct.Descripcion.Should().Be("Categoría A");
        ct.DiasVencimiento.Should().Be(0);
    }

    [Fact]
    public void Crear_DescripcionVacia_LanzaExcepcion()
    {
        var act = () => CategoriaTercero.Crear("");
        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void Actualizar_ConDescripcionValida_Actualiza()
    {
        var ct = CategoriaTercero.Crear("Original");
        ct.Actualizar(" Modificada ", false, null, null, 30, 60, 0.001m, true);
        ct.Descripcion.Should().Be("Modificada");
        ct.DiasVencimiento.Should().Be(30);
        ct.CobrarInteres.Should().BeTrue();
    }

    [Fact]
    public void Actualizar_DescripcionVacia_LanzaExcepcion()
    {
        var ct = CategoriaTercero.Crear("Original");
        var act = () => ct.Actualizar("", false, null, null, 0, 0, 0m, false);
        act.Should().Throw<ArgumentException>();
    }
}

// ─────────────────────────────────────────────
// SucursalMedioContacto
// ─────────────────────────────────────────────
public class SucursalMedioContactoTests
{
    [Fact]
    public void Crear_ConDatosValidos_AsignaValores()
    {
        var smc = SucursalMedioContacto.Crear(1, " 011-1234-5678 ");

        smc.SucursalId.Should().Be(1);
        smc.Valor.Should().Be("011-1234-5678");
    }

    [Fact]
    public void Crear_SucursalIdCero_LanzaExcepcion()
    {
        var act = () => SucursalMedioContacto.Crear(0, "valor");
        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void Crear_ValorVacio_LanzaExcepcion()
    {
        var act = () => SucursalMedioContacto.Crear(1, "");
        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void Actualizar_ConValorValido_Actualiza()
    {
        var smc = SucursalMedioContacto.Crear(1, "011-1234");
        smc.Actualizar(" 011-9999 ", null, 1, true, null);
        smc.Valor.Should().Be("011-9999");
        smc.EsDefecto.Should().BeTrue();
    }

    [Fact]
    public void Actualizar_ValorVacio_LanzaExcepcion()
    {
        var smc = SucursalMedioContacto.Crear(1, "011-1234");
        var act = () => smc.Actualizar("", null, 0, false, null);
        act.Should().Throw<ArgumentException>();
    }
}

// ─────────────────────────────────────────────
// MenuItem
// ─────────────────────────────────────────────
public class MenuItemTests
{
    [Fact]
    public void Crear_ConDescripcionValida_AsignaValores()
    {
        var mi = MenuItem.Crear(null, " Clientes ", "frmClientes", "icon.png", (short)1, (short)1);

        mi.Descripcion.Should().Be("Clientes");
        mi.Formulario.Should().Be("frmClientes");
        mi.Activo.Should().BeTrue();
    }

    [Fact]
    public void Crear_DescripcionVacia_LanzaExcepcion()
    {
        var act = () => MenuItem.Crear(null, "", null, null, (short)1, (short)1);
        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void Actualizar_ConDescripcionValida_Actualiza()
    {
        var mi = MenuItem.Crear(null, "Clientes", null, null, (short)1, (short)1);
        mi.Actualizar(" Proveedores ", "frmProv", null, (short)2);
        mi.Descripcion.Should().Be("Proveedores");
        mi.Formulario.Should().Be("frmProv");
        mi.Orden.Should().Be(2);
    }

    [Fact]
    public void Actualizar_DescripcionVacia_LanzaExcepcion()
    {
        var mi = MenuItem.Crear(null, "Clientes", null, null, (short)1, (short)1);
        var act = () => mi.Actualizar("", null, null, (short)1);
        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void Desactivar_CambiaActivoAFalse()
    {
        var mi = MenuItem.Crear(null, "Clientes", null, null, (short)1, (short)1);
        mi.Desactivar();
        mi.Activo.Should().BeFalse();
    }

    [Fact]
    public void Activar_CambiaActivoATrue()
    {
        var mi = MenuItem.Crear(null, "Clientes", null, null, (short)1, (short)1);
        mi.Desactivar();
        mi.Activar();
        mi.Activo.Should().BeTrue();
    }
}

// ─────────────────────────────────────────────
// MenuUsuario
// ─────────────────────────────────────────────
public class MenuUsuarioTests
{
    [Fact]
    public void Crear_AsignaMenuIdYUsuarioId()
    {
        var mu = MenuUsuario.Crear(5, 10);

        mu.MenuId.Should().Be(5);
        mu.UsuarioId.Should().Be(10);
    }
}

// ─────────────────────────────────────────────
// ParametroUsuario
// ─────────────────────────────────────────────
public class ParametroUsuarioTests
{
    [Fact]
    public void Crear_ConDatosValidos_NormalizaClaveAMayusculas()
    {
        var p = ParametroUsuario.Crear(1, " tema_color ", "oscuro");

        p.UsuarioId.Should().Be(1);
        p.Clave.Should().Be("TEMA_COLOR");
        p.Valor.Should().Be("oscuro");
    }

    [Fact]
    public void Crear_ClaveVacia_LanzaExcepcion()
    {
        var act = () => ParametroUsuario.Crear(1, "", "valor");
        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void SetValor_ActualizaValor()
    {
        var p = ParametroUsuario.Crear(1, "CLAVE", "viejo");
        p.SetValor("nuevo");
        p.Valor.Should().Be("nuevo");
    }

    [Fact]
    public void SetValor_Nulo_EstableceNull()
    {
        var p = ParametroUsuario.Crear(1, "CLAVE", "valor");
        p.SetValor(null);
        p.Valor.Should().BeNull();
    }
}

// ─────────────────────────────────────────────
// SeguridadUsuario
// ─────────────────────────────────────────────
public class SeguridadUsuarioTests
{
    [Fact]
    public void Crear_AsignaTodasLasPropiedades()
    {
        var su = SeguridadUsuario.Crear(1, 2, true);

        su.SeguridadId.Should().Be(1);
        su.UsuarioId.Should().Be(2);
        su.Valor.Should().BeTrue();
    }

    [Fact]
    public void SetValor_CambiaValor()
    {
        var su = SeguridadUsuario.Crear(1, 2, true);
        su.SetValor(false);
        su.Valor.Should().BeFalse();
    }
}

// ─────────────────────────────────────────────
// Seguridad
// ─────────────────────────────────────────────
public class SeguridadEntityTests
{
    [Fact]
    public void Crear_ConIdentificadorValido_NormalizaAMayusculas()
    {
        var s = Seguridad.Crear(" ventas.ver ", "Ver ventas");

        s.Identificador.Should().Be("VENTAS.VER");
        s.Descripcion.Should().Be("Ver ventas");
        s.AplicaSeguridadPorUsuario.Should().BeTrue();
    }

    [Fact]
    public void Crear_IdentificadorVacio_LanzaExcepcion()
    {
        var act = () => Seguridad.Crear("", null);
        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void Crear_SinAplicarSeguridad_AplicaSeguridadFalse()
    {
        var s = Seguridad.Crear("PERMISO", null, false);
        s.AplicaSeguridadPorUsuario.Should().BeFalse();
    }
}

// ─────────────────────────────────────────────
// PresupuestoItem
// ─────────────────────────────────────────────
public class PresupuestoItemTests
{
    [Fact]
    public void Crear_SinDescuento_SubtotalIgualACantidadPorPrecio()
    {
        var pi = PresupuestoItem.Crear(1, 1, "Item A", 10m, 100m, 0m);

        pi.Subtotal.Should().Be(1000m);
    }

    [Fact]
    public void Crear_ConDescuento10Pct_SubtotalReducido()
    {
        var pi = PresupuestoItem.Crear(1, 1, "Item B", 10m, 100m, 10m);

        // subtotal = 10 * 100 * (1 - 10/100) = 900
        pi.Subtotal.Should().Be(900m);
    }

    [Fact]
    public void Crear_SubtotalRedondeadoA4Decimales()
    {
        // 3 units * 1/3 price = 1 exactly, but force a rounding case
        var pi = PresupuestoItem.Crear(1, 1, "Item", 3m, 0.3333m, 0m);

        // 3 * 0.3333 * 1 = 0.9999 → Math.Round to 4 decimals = 0.9999
        pi.Subtotal.Should().Be(Math.Round(3m * 0.3333m * 1m, 4));
    }

    [Fact]
    public void Crear_AsignaDescripcionTrimada()
    {
        var pi = PresupuestoItem.Crear(1, 1, " Descripción ", 1m, 1m, 0m);
        pi.Descripcion.Should().Be("Descripción");
    }

    [Fact]
    public void Crear_AsignaCampos()
    {
        var pi = PresupuestoItem.Crear(5, 3, "Prod", 2m, 50m, 20m, (short)2);

        pi.PresupuestoId.Should().Be(5);
        pi.ItemId.Should().Be(3);
        pi.Cantidad.Should().Be(2m);
        pi.PrecioUnitario.Should().Be(50m);
        pi.DescuentoPct.Should().Be(20m);
        pi.Orden.Should().Be(2);
    }
}
