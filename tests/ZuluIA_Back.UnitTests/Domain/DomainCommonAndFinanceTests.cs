using FluentAssertions;
using Xunit;
using ZuluIA_Back.Domain.Common;
using ZuluIA_Back.Domain.Entities.BI;
using ZuluIA_Back.Domain.Entities.Comprobantes;
using ZuluIA_Back.Domain.Entities.Contabilidad;
using ZuluIA_Back.Domain.Entities.Facturacion;
using ZuluIA_Back.Domain.Entities.Finanzas;
using ZuluIA_Back.Domain.Entities.Referencia;
using ZuluIA_Back.Domain.Entities.Stock;
using ZuluIA_Back.Domain.Enums;

namespace ZuluIA_Back.UnitTests.Domain;

// ─────────────────────────────────────────────────────────────────────────────
// PagedResult (Common)
// ─────────────────────────────────────────────────────────────────────────────

public class PagedResultTests
{
    [Fact]
    public void TotalPages_Calculado_CorrectamenteRedondeadoArriba()
    {
        var pr = new PagedResult<string>(["a", "b"], 1, 2, 5);
        pr.TotalPages.Should().Be(3);
    }

    [Fact]
    public void TotalPages_ElementosExactos_SinRedondeo()
    {
        var pr = new PagedResult<string>(["a", "b"], 1, 2, 4);
        pr.TotalPages.Should().Be(2);
    }

    [Fact]
    public void HasNext_PaginaNoEsUltima_RetornaTrue()
    {
        var pr = new PagedResult<string>([], 1, 10, 25);
        pr.HasNext.Should().BeTrue();
    }

    [Fact]
    public void HasNext_UltimaPagina_RetornaFalse()
    {
        var pr = new PagedResult<string>([], 3, 10, 25);
        pr.HasNext.Should().BeFalse();
    }

    [Fact]
    public void HasPrev_PaginaMayorA1_RetornaTrue()
    {
        var pr = new PagedResult<string>([], 2, 10, 25);
        pr.HasPrev.Should().BeTrue();
    }

    [Fact]
    public void HasPrev_PrimeraPagina_RetornaFalse()
    {
        var pr = new PagedResult<string>([], 1, 10, 25);
        pr.HasPrev.Should().BeFalse();
    }

    [Fact]
    public void Empty_RetornaListaVaciaConTotalCero()
    {
        var pr = PagedResult<string>.Empty(1, 10);
        pr.Items.Should().BeEmpty();
        pr.TotalCount.Should().Be(0);
        pr.TotalPages.Should().Be(0);
    }
}

// ─────────────────────────────────────────────────────────────────────────────
// AsientoLinea
// ─────────────────────────────────────────────────────────────────────────────

public class AsientoLineaTests
{
    [Fact]
    public void Crear_SoloDebe_RetornaInstanciaDeudora()
    {
        var linea = AsientoLinea.Crear(1, 10, 500m, 0m, "Descripción", 0);
        linea.Debe.Should().Be(500m);
        linea.Haber.Should().Be(0m);
        linea.EsDeudora.Should().BeTrue();
        linea.EsAcreedora.Should().BeFalse();
    }

    [Fact]
    public void Crear_SoloHaber_RetornaInstanciaAcreedora()
    {
        var linea = AsientoLinea.Crear(1, 10, 0m, 500m, null, 0);
        linea.EsAcreedora.Should().BeTrue();
        linea.EsDeudora.Should().BeFalse();
    }

    [Fact]
    public void Crear_DebeNegativo_LanzaExcepcion()
    {
        var act = () => AsientoLinea.Crear(1, 10, -1m, 0m, null, 0);
        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void Crear_HaberNegativo_LanzaExcepcion()
    {
        var act = () => AsientoLinea.Crear(1, 10, 0m, -1m, null, 0);
        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void Crear_AmbosCero_LanzaExcepcion()
    {
        var act = () => AsientoLinea.Crear(1, 10, 0m, 0m, null, 0);
        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void Crear_DebeYHaberSimultaneos_LanzaExcepcion()
    {
        var act = () => AsientoLinea.Crear(1, 10, 100m, 100m, null, 0);
        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void Crear_MontosSeRedondeanA2Decimales()
    {
        var linea = AsientoLinea.Crear(1, 10, 100.999m, 0m, null, 0);
        linea.Debe.Should().Be(101.00m);
    }
}

// ─────────────────────────────────────────────────────────────────────────────
// Imputacion
// ─────────────────────────────────────────────────────────────────────────────

public class ImputacionFinanceTests
{
    [Fact]
    public void Crear_ConDatosValidos_RetornaInstancia()
    {
        var imp = Imputacion.Crear(1, 2, 500m, new DateOnly(2025, 3, 1), null);
        imp.ComprobanteOrigenId.Should().Be(1);
        imp.ComprobanteDestinoId.Should().Be(2);
        imp.Importe.Should().Be(500m);
    }

    [Fact]
    public void Crear_MismoOrigenYDestino_LanzaExcepcion()
    {
        var act = () => Imputacion.Crear(1, 1, 500m, new DateOnly(2025, 3, 1), null);
        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void Crear_ImporteCero_LanzaExcepcion()
    {
        var act = () => Imputacion.Crear(1, 2, 0m, new DateOnly(2025, 3, 1), null);
        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void Crear_ImporteNegativo_LanzaExcepcion()
    {
        var act = () => Imputacion.Crear(1, 2, -100m, new DateOnly(2025, 3, 1), null);
        act.Should().Throw<InvalidOperationException>();
    }
}

// ─────────────────────────────────────────────────────────────────────────────
// CuentaCorriente
// ─────────────────────────────────────────────────────────────────────────────

public class CuentaCorrienteTests
{
    [Fact]
    public void Crear_SaldoInicialCero()
    {
        var cta = CuentaCorriente.Crear(1, null, 1);
        cta.Saldo.Should().Be(0m);
    }

    [Fact]
    public void Debitar_ImportePositivo_AumentaSaldo()
    {
        var cta = CuentaCorriente.Crear(1, null, 1);
        cta.Debitar(1000m);
        cta.Saldo.Should().Be(1000m);
    }

    [Fact]
    public void Debitar_ImporteCero_LanzaExcepcion()
    {
        var cta = CuentaCorriente.Crear(1, null, 1);
        var act = () => cta.Debitar(0m);
        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void Acreditar_ImportePositivo_DisminuyeSaldo()
    {
        var cta = CuentaCorriente.Crear(1, null, 1);
        cta.Debitar(1000m);
        cta.Acreditar(400m);
        cta.Saldo.Should().Be(600m);
    }

    [Fact]
    public void Acreditar_ImporteCero_LanzaExcepcion()
    {
        var cta = CuentaCorriente.Crear(1, null, 1);
        var act = () => cta.Acreditar(0m);
        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void Debitar_Luego_Acreditar_SaldoFinalCorrecto()
    {
        var cta = CuentaCorriente.Crear(1, null, 1);
        cta.Debitar(500m);
        cta.Debitar(300m);
        cta.Acreditar(200m);
        cta.Saldo.Should().Be(600m);
    }
}

// ─────────────────────────────────────────────────────────────────────────────
// PeriodoContable
// ─────────────────────────────────────────────────────────────────────────────

public class PeriodoContableTests
{
    private static PeriodoContable PeriodoValido() =>
        PeriodoContable.Crear("2025-01",
            new DateOnly(2025, 1, 1), new DateOnly(2025, 1, 31));

    [Fact]
    public void Crear_ConDatosValidos_EsAbierto()
    {
        var p = PeriodoValido();
        p.Abierto.Should().BeTrue();
        p.Periodo.Should().Be("2025-01");
    }

    [Fact]
    public void Crear_PeriodoVacio_LanzaExcepcion()
    {
        var act = () => PeriodoContable.Crear("",
            new DateOnly(2025, 1, 1), new DateOnly(2025, 1, 31));
        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void Crear_FechaFinAnteriorAInicio_LanzaExcepcion()
    {
        var act = () => PeriodoContable.Crear("2025-01",
            new DateOnly(2025, 1, 31), new DateOnly(2025, 1, 1));
        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void Cerrar_PeriodoAbierto_LoCierra()
    {
        var p = PeriodoValido();
        p.Cerrar();
        p.Abierto.Should().BeFalse();
    }

    [Fact]
    public void Cerrar_PeriodoYaCerrado_LanzaExcepcion()
    {
        var p = PeriodoValido();
        p.Cerrar();
        var act = () => p.Cerrar();
        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void Abrir_PeriodoCerrado_LoAbre()
    {
        var p = PeriodoValido();
        p.Cerrar();
        p.Abrir();
        p.Abierto.Should().BeTrue();
    }

    [Fact]
    public void Abrir_PeriodoYaAbierto_LanzaExcepcion()
    {
        var p = PeriodoValido();
        var act = () => p.Abrir();
        act.Should().Throw<InvalidOperationException>();
    }
}

// ─────────────────────────────────────────────────────────────────────────────
// PeriodoIva
// ─────────────────────────────────────────────────────────────────────────────

public class PeriodoIvaTests
{
    private static PeriodoIva PeriodoValido() =>
        PeriodoIva.Crear(1, 1, new DateOnly(2025, 3, 15));

    [Fact]
    public void Crear_NormalizaAlPrimerDelMes()
    {
        var p = PeriodoValido();
        p.Periodo.Should().Be(new DateOnly(2025, 3, 1));
    }

    [Fact]
    public void Crear_NoCerrado()
    {
        var p = PeriodoValido();
        p.Cerrado.Should().BeFalse();
    }

    [Fact]
    public void Cerrar_PeriodoAbierto_LoCierra()
    {
        var p = PeriodoValido();
        p.Cerrar();
        p.Cerrado.Should().BeTrue();
    }

    [Fact]
    public void Cerrar_PeriodoYaCerrado_LanzaExcepcion()
    {
        var p = PeriodoValido();
        p.Cerrar();
        var act = () => p.Cerrar();
        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void Reabrir_PeriodoCerrado_LoAbre()
    {
        var p = PeriodoValido();
        p.Cerrar();
        p.Reabrir();
        p.Cerrado.Should().BeFalse();
    }

    [Fact]
    public void Reabrir_PeriodoAbierto_LanzaExcepcion()
    {
        var p = PeriodoValido();
        var act = () => p.Reabrir();
        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void PeriodoDescripcion_DevuelveFormatoCorrectoMMMMyyyy()
    {
        var p = PeriodoIva.Crear(1, 1, new DateOnly(2025, 3, 15));

        p.PeriodoDescripcion.Should().Be(p.Periodo.ToString("MMMM yyyy"));
    }
}

// ─────────────────────────────────────────────────────────────────────────────
// CierreCaja
// ─────────────────────────────────────────────────────────────────────────────

public class CierreCajaTests
{
    private static DateTimeOffset _apertura = new DateTimeOffset(2025, 3, 1, 8, 0, 0, TimeSpan.Zero);
    private static DateTimeOffset _cierre   = new DateTimeOffset(2025, 3, 1, 18, 0, 0, TimeSpan.Zero);

    [Fact]
    public void Crear_ConDatosValidos_RetornaInstancia()
    {
        var c = CierreCaja.Crear(_apertura, _cierre, 1, 1);
        c.NroCierre.Should().Be(1);
        c.UsuarioId.Should().Be(1);
        c.FechaControlTesoreria.Should().BeNull();
    }

    [Fact]
    public void Crear_UsuarioIdCero_LanzaExcepcion()
    {
        var act = () => CierreCaja.Crear(_apertura, _cierre, 0, 1);
        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void Crear_NroCierreCero_LanzaExcepcion()
    {
        var act = () => CierreCaja.Crear(_apertura, _cierre, 1, 0);
        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void Crear_FechaCierreAnteriorApertura_LanzaExcepcion()
    {
        var act = () => CierreCaja.Crear(_cierre, _apertura, 1, 1);
        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void RegistrarControlTesoreria_AsignaFecha()
    {
        var c = CierreCaja.Crear(_apertura, _cierre, 1, 1);
        var fechaControl = DateTimeOffset.UtcNow;
        c.RegistrarControlTesoreria(fechaControl);
        c.FechaControlTesoreria.Should().Be(fechaControl);
    }
}

// ─────────────────────────────────────────────────────────────────────────────
// MovimientoStock
// ─────────────────────────────────────────────────────────────────────────────

public class MovimientoStockTests
{
    [Fact]
    public void Crear_ConDatosValidos_RetornaInstancia()
    {
        var mov = MovimientoStock.Crear(
            1, 1, TipoMovimientoStock.Ingreso, 50m, 150m);

        mov.Cantidad.Should().Be(50m);
        mov.SaldoResultante.Should().Be(150m);
        mov.TipoMovimiento.Should().Be(TipoMovimientoStock.Ingreso);
    }

    [Fact]
    public void Crear_CantidadCero_LanzaExcepcion()
    {
        var act = () => MovimientoStock.Crear(
            1, 1, TipoMovimientoStock.Ingreso, 0m, 0m);
        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void Crear_CantidadNegativa_EsValidaParaEgresos()
    {
        // Los egresos se representan con cantidad negativa
        var mov = MovimientoStock.Crear(
            1, 1, TipoMovimientoStock.Egreso, -25m, 75m);
        mov.Cantidad.Should().Be(-25m);
    }

    [Fact]
    public void Crear_ObservacionSeRecorta()
    {
        var mov = MovimientoStock.Crear(
            1, 1, TipoMovimientoStock.Ingreso, 10m, 10m,
            observacion: "  nota  ");
        mov.Observacion.Should().Be("nota");
    }
}

// ─────────────────────────────────────────────────────────────────────────────
// CentroCosto
// ─────────────────────────────────────────────────────────────────────────────

public class CentroCostoTests
{
    [Fact]
    public void Crear_CodigoSeNormalizaAMayusculas()
    {
        var cc = CentroCosto.Crear("adm", "Administración");
        cc.Codigo.Should().Be("ADM");
    }

    [Fact]
    public void Crear_CodigoVacio_LanzaExcepcion()
    {
        var act = () => CentroCosto.Crear("", "Descripción");
        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void Crear_DescripcionVacia_LanzaExcepcion()
    {
        var act = () => CentroCosto.Crear("ADM", "");
        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void Desactivar_Activar_CambiaEstado()
    {
        var cc = CentroCosto.Crear("ADM", "Administración");
        cc.Desactivar();
        cc.Activo.Should().BeFalse();
        cc.Activar();
        cc.Activo.Should().BeTrue();
    }

    [Fact]
    public void Actualizar_DescripcionVacia_LanzaExcepcion()
    {
        var cc = CentroCosto.Crear("ADM", "Admin");
        var act = () => cc.Actualizar("");
        act.Should().Throw<ArgumentException>();
    }
}

// ─────────────────────────────────────────────────────────────────────────────
// UnidadMedida
// ─────────────────────────────────────────────────────────────────────────────

public class UnidadMedidaTests
{
    [Fact]
    public void Crear_CodigoSeNormalizaAMayusculas()
    {
        var um = UnidadMedida.Crear("kg", "Kilogramo");
        um.Codigo.Should().Be("KG");
    }

    [Fact]
    public void Crear_CodigoVacio_LanzaExcepcion()
    {
        var act = () => UnidadMedida.Crear("", "Kilogramo");
        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void Crear_MultiplicadorCero_UsaUno()
    {
        var um = UnidadMedida.Crear("KG", "Kilogramo", multiplicador: 0m);
        um.Multiplicador.Should().Be(1m);
    }

    [Fact]
    public void Crear_EsUnidadBase_UnidadBaseIdEsNull()
    {
        var um = UnidadMedida.Crear("KG", "Kilogramo", esUnidadBase: true, unidadBaseId: 99);
        um.UnidadBaseId.Should().BeNull();
    }

    [Fact]
    public void Crear_NoEsUnidadBase_GuardaUnidadBaseId()
    {
        var um = UnidadMedida.Crear("G", "Gramo",
            multiplicador: 0.001m, esUnidadBase: false, unidadBaseId: 1);
        um.UnidadBaseId.Should().Be(1);
    }

    [Fact]
    public void Activar_Desactivar_CambiaEstado()
    {
        var um = UnidadMedida.Crear("KG", "Kilogramo");
        um.Desactivar();
        um.Activa.Should().BeFalse();
        um.Activar();
        um.Activa.Should().BeTrue();
    }

    [Fact]
    public void Actualizar_ConDescripcionValida_ActualizaCampos()
    {
        var um = UnidadMedida.Crear("KG", "Kilogramo", esUnidadBase: true);
        um.Actualizar("Kilogramo Actualizado", "kg", 0.001m, false, 1L);
        um.Descripcion.Should().Be("Kilogramo Actualizado");
        um.Disminutivo.Should().Be("kg");
        um.Multiplicador.Should().Be(0.001m);
        um.EsUnidadBase.Should().BeFalse();
        um.UnidadBaseId.Should().Be(1L);
    }

    [Fact]
    public void Actualizar_DescripcionVacia_LanzaExcepcion()
    {
        var um = UnidadMedida.Crear("KG", "Kilogramo");
        var act = () => um.Actualizar("", null, 1m, true, null);
        act.Should().Throw<ArgumentException>();
    }
}

// ─────────────────────────────────────────────────────────────────────────────
// Zona, Jurisdiccion, Marca (referencia simple con misma estructura)
// ─────────────────────────────────────────────────────────────────────────────

public class ZonaTests
{
    [Fact]
    public void Crear_DescripcionVacia_LanzaExcepcion()
    {
        var act = () => Zona.Crear("");
        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void Crear_DescripcionSeTrimea()
    {
        var z = Zona.Crear("  Norte  ");
        z.Descripcion.Should().Be("Norte");
        z.Activo.Should().BeTrue();
    }

    [Fact]
    public void Desactivar_Activar_CambiaEstado()
    {
        var z = Zona.Crear("Norte");
        z.Desactivar();
        z.Activo.Should().BeFalse();
        z.Activar();
        z.Activo.Should().BeTrue();
    }

    [Fact]
    public void Actualizar_DescripcionVacia_LanzaExcepcion()
    {
        var z = Zona.Crear("Norte");
        var act = () => z.Actualizar("");
        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void Actualizar_ConDescripcionValida_Actualiza()
    {
        var z = Zona.Crear("Norte");
        z.Actualizar("  Sur  ");
        z.Descripcion.Should().Be("Sur");
    }
}

public class JurisdiccionTests
{
    [Fact]
    public void Crear_DescripcionVacia_LanzaExcepcion()
    {
        var act = () => Jurisdiccion.Crear("");
        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void Crear_DescripcionValida_EsActiva()
    {
        var j = Jurisdiccion.Crear("Buenos Aires");
        j.Descripcion.Should().Be("Buenos Aires");
        j.Activo.Should().BeTrue();
    }

    [Fact]
    public void Desactivar_Activar_CambiaEstado()
    {
        var j = Jurisdiccion.Crear("Buenos Aires");
        j.Desactivar();
        j.Activo.Should().BeFalse();
        j.Activar();
        j.Activo.Should().BeTrue();
    }

    [Fact]
    public void Actualizar_ConDescripcionValida_Actualiza()
    {
        var j = Jurisdiccion.Crear("Buenos Aires");
        j.Actualizar(" Córdoba ");
        j.Descripcion.Should().Be("Córdoba");
    }

    [Fact]
    public void Actualizar_DescripcionVacia_LanzaExcepcion()
    {
        var j = Jurisdiccion.Crear("Buenos Aires");
        var act = () => j.Actualizar("");
        act.Should().Throw<ArgumentException>();
    }
}

public class MarcaTests
{
    [Fact]
    public void Crear_DescripcionVacia_LanzaExcepcion()
    {
        var act = () => Marca.Crear("");
        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void Crear_DescripcionValida_EsActiva()
    {
        var m = Marca.Crear("Samsung");
        m.Descripcion.Should().Be("Samsung");
        m.Activo.Should().BeTrue();
    }

    [Fact]
    public void Desactivar_Activar_CambiaEstado()
    {
        var m = Marca.Crear("Samsung");
        m.Desactivar();
        m.Activo.Should().BeFalse();
        m.Activar();
        m.Activo.Should().BeTrue();
    }

    [Fact]
    public void Actualizar_ConDescripcionValida_Actualiza()
    {
        var m = Marca.Crear("Samsung");
        m.Actualizar(" LG ");
        m.Descripcion.Should().Be("LG");
    }

    [Fact]
    public void Actualizar_DescripcionVacia_LanzaExcepcion()
    {
        var m = Marca.Crear("Samsung");
        var act = () => m.Actualizar("");
        act.Should().Throw<ArgumentException>();
    }
}

// ─────────────────────────────────────────────────────────────────────────────
// CuboCampo
// ─────────────────────────────────────────────────────────────────────────────

public class CuboCampoTests
{
    [Fact]
    public void Crear_ConDatosValidos_RetornaInstancia()
    {
        var campo = CuboCampo.Crear(1, "VentasNetas");
        campo.CuboId.Should().Be(1);
        campo.SourceName.Should().Be("VentasNetas");
        campo.Visible.Should().BeTrue();
    }

    [Fact]
    public void Crear_CuboIdCero_LanzaExcepcion()
    {
        var act = () => CuboCampo.Crear(0, "Campo");
        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void Crear_SourceNameVacio_LanzaExcepcion()
    {
        var act = () => CuboCampo.Crear(1, "");
        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void Crear_SourceNameSeTrimea()
    {
        var campo = CuboCampo.Crear(1, "  Ventas  ");
        campo.SourceName.Should().Be("Ventas");
    }

    [Fact]
    public void Actualizar_CambiaLasPropiedades()
    {
        var campo = CuboCampo.Crear(1, "Ventas");
        campo.Actualizar("Ven", 1, 0, false, true, "filtro", null, 5, 1, 2);

        campo.Visible.Should().BeFalse();
        campo.Calculado.Should().BeTrue();
        campo.Filtro.Should().Be("filtro");
        campo.Orden.Should().Be(5);
    }
}
