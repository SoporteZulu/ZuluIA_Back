using FluentAssertions;
using Xunit;
using ZuluIA_Back.Domain.Entities.Contabilidad;
using ZuluIA_Back.Domain.Entities.Facturacion;
using ZuluIA_Back.Domain.Entities.Items;
using ZuluIA_Back.Domain.Entities.Precios;
using ZuluIA_Back.Domain.Entities.Proyectos;
using ZuluIA_Back.Domain.Entities.Usuarios;
using ZuluIA_Back.Domain.Entities.Ventas;
using ZuluIA_Back.Domain.Enums;

namespace ZuluIA_Back.UnitTests.Domain;

// ─────────────────────────────────────────────────────────────────────────────
// ListaPrecios
// ─────────────────────────────────────────────────────────────────────────────

public class ListaPreciosTests
{
    [Fact]
    public void Crear_ConDatosValidos_RetornaInstancia()
    {
        var lista = ListaPrecios.Crear("Lista Mayorista", 1, null, null, null);

        lista.Descripcion.Should().Be("Lista Mayorista");
        lista.MonedaId.Should().Be(1);
        lista.Activa.Should().BeTrue();
        lista.Items.Should().BeEmpty();
    }

    [Fact]
    public void Crear_DescripcionVacia_LanzaExcepcion()
    {
        var act = () => ListaPrecios.Crear("", 1, null, null, null);
        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void Crear_VigenciaHastaAnteriorADesde_LanzaExcepcion()
    {
        var desde = new DateOnly(2025, 6, 1);
        var hasta = new DateOnly(2025, 1, 1);

        var act = () => ListaPrecios.Crear("Lista", 1, desde, hasta, null);
        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void EstaVigente_ListaActivaSinRango_RetornaTrue()
    {
        var lista = ListaPrecios.Crear("Lista", 1, null, null, null);
        lista.EstaVigente(DateOnly.FromDateTime(DateTime.Today)).Should().BeTrue();
    }

    [Fact]
    public void EstaVigente_ListaInactiva_RetornaFalse()
    {
        var lista = ListaPrecios.Crear("Lista", 1, null, null, null);
        lista.Desactivar(null);
        lista.EstaVigente(DateOnly.FromDateTime(DateTime.Today)).Should().BeFalse();
    }

    [Fact]
    public void EstaVigente_FechaFueraDeRango_RetornaFalse()
    {
        var desde = new DateOnly(2026, 1, 1);
        var hasta = new DateOnly(2026, 12, 31);
        var lista = ListaPrecios.Crear("Lista", 1, desde, hasta, null);

        lista.EstaVigente(new DateOnly(2025, 6, 1)).Should().BeFalse();
    }

    [Fact]
    public void UpsertItem_ItemNuevo_SeAgregaALaColeccion()
    {
        var lista = ListaPrecios.Crear("Lista", 1, null, null, null);
        lista.UpsertItem(10, 100m, 0m);

        lista.Items.Should().HaveCount(1);
        lista.Items.First().ItemId.Should().Be(10);
        lista.Items.First().Precio.Should().Be(100m);
    }

    [Fact]
    public void UpsertItem_PrecioNegativo_LanzaExcepcion()
    {
        var lista = ListaPrecios.Crear("Lista", 1, null, null, null);
        var act = () => lista.UpsertItem(10, -1m, 0m);
        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void UpsertItem_DescuentoFueraDeRango_LanzaExcepcion()
    {
        var lista = ListaPrecios.Crear("Lista", 1, null, null, null);
        var act = () => lista.UpsertItem(10, 100m, 150m);
        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void UpsertItem_ItemExistente_ActualizaPrecio()
    {
        var lista = ListaPrecios.Crear("Lista", 1, null, null, null);
        lista.UpsertItem(10, 100m, 0m);
        lista.UpsertItem(10, 200m, 5m);

        lista.Items.Should().HaveCount(1);
        lista.Items.First().Precio.Should().Be(200m);
    }

    [Fact]
    public void RemoverItem_ItemExistente_LoElimina()
    {
        var lista = ListaPrecios.Crear("Lista", 1, null, null, null);
        lista.UpsertItem(10, 100m, 0m);

        var resultado = lista.RemoverItem(10);

        resultado.Should().BeTrue();
        lista.Items.Should().BeEmpty();
    }

    [Fact]
    public void RemoverItem_ItemNoExistente_RetornaFalse()
    {
        var lista = ListaPrecios.Crear("Lista", 1, null, null, null);

        var resultado = lista.RemoverItem(99);

        resultado.Should().BeFalse();
    }

    [Fact]
    public void ObtenerPrecioItem_ItemExistente_RetornaItem()
    {
        var lista = ListaPrecios.Crear("Lista", 1, null, null, null);
        lista.UpsertItem(10, 100m, 0m);

        var item = lista.ObtenerPrecioItem(10);

        item.Should().NotBeNull();
        item!.ItemId.Should().Be(10);
        item.Precio.Should().Be(100m);
    }

    [Fact]
    public void ObtenerPrecioItem_ItemNoExistente_RetornaNulo()
    {
        var lista = ListaPrecios.Crear("Lista", 1, null, null, null);

        var item = lista.ObtenerPrecioItem(99);

        item.Should().BeNull();
    }

    [Fact]
    public void Actualizar_FechaInvalidaHastaAnteriorDesde_LanzaExcepcion()
    {
        var lista = ListaPrecios.Crear("Lista", 1, null, null, null);
        var desde = new DateOnly(2026, 6, 1);
        var hasta = new DateOnly(2026, 1, 1);

        var act = () => lista.Actualizar("Lista", 1, desde, hasta, null);
        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void Actualizar_ConDatosValidos_ActualizaCampos()
    {
        var lista = ListaPrecios.Crear("Lista Original", 1, null, null, null);
        lista.Actualizar("  Lista Modificada  ", 2, null, null, null);
        lista.Descripcion.Should().Be("Lista Modificada");
        lista.MonedaId.Should().Be(2);
    }

    [Fact]
    public void Desactivar_DesactivaYActiva_CambiaEstado()
    {
        var lista = ListaPrecios.Crear("Lista", 1, null, null, null);
        lista.Desactivar(null);
        lista.Activa.Should().BeFalse();

        lista.Activar(null);
        lista.Activa.Should().BeTrue();
    }
}

// ─────────────────────────────────────────────────────────────────────────────
// PlanPago
// ─────────────────────────────────────────────────────────────────────────────

public class PlanPagoTests
{
    [Fact]
    public void Crear_ConDatosValidos_RetornaInstancia()
    {
        var plan = PlanPago.Crear("Contado", 1, 0);

        plan.Descripcion.Should().Be("Contado");
        plan.CantidadCuotas.Should().Be(1);
        plan.InteresPct.Should().Be(0);
        plan.Activo.Should().BeTrue();
    }

    [Fact]
    public void Crear_DescripcionVacia_LanzaExcepcion()
    {
        var act = () => PlanPago.Crear("", 1, 0);
        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void Crear_CuotasMenorA1_LanzaExcepcion()
    {
        var act = () => PlanPago.Crear("Plan", 0, 0);
        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void Crear_InteresNegativo_LanzaExcepcion()
    {
        var act = () => PlanPago.Crear("Plan", 3, -1m);
        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void CalcularCuota_SinInteres_DivideEquitativamente()
    {
        var plan = PlanPago.Crear("Plan 3 cuotas", 3, 0);

        var cuota = plan.CalcularCuota(300m);

        cuota.Should().Be(100m);
    }

    [Fact]
    public void CalcularCuota_ConInteres_AplicaInteres()
    {
        var plan = PlanPago.Crear("Plan con interes", 2, 10m); // 10%

        var cuota = plan.CalcularCuota(100m);

        // 100 * 1.10 / 2 = 55
        cuota.Should().Be(55m);
    }

    [Fact]
    public void CalcularTotalConInteres_ConInteres_AplicaCorrectamente()
    {
        var plan = PlanPago.Crear("Plan", 1, 20m); // 20%

        var total = plan.CalcularTotalConInteres(100m);

        total.Should().Be(120m);
    }

    [Fact]
    public void Actualizar_CuotasInvalidas_LanzaExcepcion()
    {
        var plan = PlanPago.Crear("Plan", 3, 0);
        var act = () => plan.Actualizar("Plan", 0, 0);
        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void Actualizar_ConDatosValidos_ActualizaCampos()
    {
        var plan = PlanPago.Crear("Plan", 3, 0);
        plan.Actualizar("Plan Modificado", 6, 5m);
        plan.Descripcion.Should().Be("Plan Modificado");
        plan.CantidadCuotas.Should().Be(6);
        plan.InteresPct.Should().Be(5m);
    }

    [Fact]
    public void Desactivar_Activar_CambiaEstado()
    {
        var plan = PlanPago.Crear("Plan", 1, 0);
        plan.Desactivar();
        plan.Activo.Should().BeFalse();
        plan.Activar();
        plan.Activo.Should().BeTrue();
    }
}

// ─────────────────────────────────────────────────────────────────────────────
// Ejercicio
// ─────────────────────────────────────────────────────────────────────────────

public class EjercicioTests
{
    private static Ejercicio EjercicioValido() =>
        Ejercicio.Crear("2025", new DateOnly(2025, 1, 1), new DateOnly(2025, 12, 31));

    [Fact]
    public void Crear_ConDatosValidos_RetornaInstancia()
    {
        var ej = EjercicioValido();
        ej.Descripcion.Should().Be("2025");
        ej.Cerrado.Should().BeFalse();
    }

    [Fact]
    public void Crear_FechaFinAnteriorAInicio_LanzaExcepcion()
    {
        var act = () => Ejercicio.Crear("2025",
            new DateOnly(2025, 12, 31), new DateOnly(2025, 1, 1));
        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void Crear_DescripcionVacia_LanzaExcepcion()
    {
        var act = () => Ejercicio.Crear("",
            new DateOnly(2025, 1, 1), new DateOnly(2025, 12, 31));
        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void Cerrar_EjercicioAbierto_LoCierra()
    {
        var ej = EjercicioValido();
        ej.Cerrar();
        ej.Cerrado.Should().BeTrue();
    }

    [Fact]
    public void Cerrar_EjercicioYaCerrado_LanzaExcepcion()
    {
        var ej = EjercicioValido();
        ej.Cerrar();
        var act = () => ej.Cerrar();
        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void Reabrir_EjercicioCerrado_LoAbre()
    {
        var ej = EjercicioValido();
        ej.Cerrar();
        ej.Reabrir();
        ej.Cerrado.Should().BeFalse();
    }

    [Fact]
    public void Reabrir_EjercicioAbierto_LanzaExcepcion()
    {
        var ej = EjercicioValido();
        var act = () => ej.Reabrir();
        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void ContienesFecha_FechaDentroDelRango_RetornaTrue()
    {
        var ej = EjercicioValido();
        ej.ContienesFecha(new DateOnly(2025, 6, 15)).Should().BeTrue();
    }

    [Fact]
    public void ContienesFecha_FechaFueraDelRango_RetornaFalse()
    {
        var ej = EjercicioValido();
        ej.ContienesFecha(new DateOnly(2024, 12, 31)).Should().BeFalse();
    }

    [Fact]
    public void AsignarSucursal_SucursalNueva_SeAgregaUnaVez()
    {
        var ej = EjercicioValido();
        ej.AsignarSucursal(1);
        ej.AsignarSucursal(1); // idempotente

        ej.Sucursales.Should().HaveCount(1);
    }

    [Fact]
    public void ActualizarDescripcion_DescripcionValida_ActualizaPropiedad()
    {
        var ej = EjercicioValido();

        ej.ActualizarDescripcion("Ejercicio 2026");

        ej.Descripcion.Should().Be("Ejercicio 2026");
    }

    [Fact]
    public void ActualizarDescripcion_DescripcionVacia_LanzaExcepcion()
    {
        var ej = EjercicioValido();

        var act = () => ej.ActualizarDescripcion("   ");

        act.Should().Throw<ArgumentException>();
    }
}

// ─────────────────────────────────────────────────────────────────────────────
// CartaPorte
// ─────────────────────────────────────────────────────────────────────────────

public class CartaPorteTests
{
    private static CartaPorte CartaValida() => CartaPorte.Crear(
        null,
        "20-12345678-9",
        "30-99887766-5",
        null,
        new DateOnly(2025, 3, 1),
        null,
        null);

    [Fact]
    public void Crear_ConDatosValidos_EstadoPendiente()
    {
        var carta = CartaValida();
        carta.Estado.Should().Be(EstadoCartaPorte.Pendiente);
        carta.CuitRemitente.Should().Be("20-12345678-9");
    }

    [Fact]
    public void Crear_CuitRemitenteVacio_LanzaExcepcion()
    {
        var act = () => CartaPorte.Crear(null, "", "30-99887766-5", null,
            new DateOnly(2025, 3, 1), null, null);
        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void AsignarCtg_DesdeEstadoPendiente_CambiaAActiva()
    {
        var carta = CartaValida();
        carta.AsignarCtg("CTG123456", null);

        carta.Estado.Should().Be(EstadoCartaPorte.Activa);
        carta.NroCtg.Should().Be("CTG123456");
    }

    [Fact]
    public void AsignarCtg_EstadoNoEsPendiente_LanzaExcepcion()
    {
        var carta = CartaValida();
        carta.AsignarCtg("CTG1", null);
        var act = () => carta.AsignarCtg("CTG2", null);
        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void Confirmar_DesdeEstadoActiva_CambiaAConfirmada()
    {
        var carta = CartaValida();
        carta.AsignarCtg("CTG123", null);
        carta.Confirmar(null);

        carta.Estado.Should().Be(EstadoCartaPorte.Confirmada);
    }

    [Fact]
    public void Confirmar_EstadoNoEsActiva_LanzaExcepcion()
    {
        var carta = CartaValida();
        var act = () => carta.Confirmar(null);
        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void Anular_CartaActiva_CambiaAAnulada()
    {
        var carta = CartaValida();
        carta.AsignarCtg("CTG123", null);
        carta.Anular("Error de datos", null);

        carta.Estado.Should().Be(EstadoCartaPorte.Anulada);
    }

    [Fact]
    public void Anular_CartaYaAnulada_LanzaExcepcion()
    {
        var carta = CartaValida();
        carta.Anular(null, null);
        var act = () => carta.Anular(null, null);
        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void SetObservacion_ConEspacios_Recorta()
    {
        var carta = CartaValida();
        carta.SetObservacion("  obs  ");
        carta.Observacion.Should().Be("obs");
    }

    [Fact]
    public void SetObservacion_Null_AsignaNulo()
    {
        var carta = CartaValida();
        carta.SetObservacion(null);
        carta.Observacion.Should().BeNull();
    }

    [Fact]
    public void SetComprobanteId_AsignaValor()
    {
        var carta = CartaValida();
        carta.SetComprobanteId(99L);
        carta.ComprobanteId.Should().Be(99L);
    }
}

// ─────────────────────────────────────────────────────────────────────────────
// Deposito
// ─────────────────────────────────────────────────────────────────────────────

public class DepositoEntityTests
{
    [Fact]
    public void Crear_ConDatosValidos_RetornaInstancia()
    {
        var dep = Deposito.Crear(1, "Depósito Central", false);
        dep.Descripcion.Should().Be("Depósito Central");
        dep.Activo.Should().BeTrue();
        dep.EsDefault.Should().BeFalse();
    }

    [Fact]
    public void Crear_DescripcionVacia_LanzaExcepcion()
    {
        var act = () => Deposito.Crear(1, "", false);
        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void Actualizar_DescripcionVacia_LanzaExcepcion()
    {
        var dep = Deposito.Crear(1, "Central", false);
        var act = () => dep.Actualizar("", false);
        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void Desactivar_Activar_CambiaEstado()
    {
        var dep = Deposito.Crear(1, "Central", false);
        dep.Desactivar();
        dep.Activo.Should().BeFalse();
        dep.Activar();
        dep.Activo.Should().BeTrue();
    }

    [Fact]
    public void SetDefault_UnsetDefault_CambiaEsDefault()
    {
        var dep = Deposito.Crear(1, "Central", false);
        dep.SetDefault();
        dep.EsDefault.Should().BeTrue();
        dep.UnsetDefault();
        dep.EsDefault.Should().BeFalse();
    }
}

// ─────────────────────────────────────────────────────────────────────────────
// Proyecto
// ─────────────────────────────────────────────────────────────────────────────

public class ProyectoTests
{
    [Fact]
    public void Crear_ConDatosValidos_EstadoActivo()
    {
        var proyecto = Proyecto.Crear("PROY-01", "Proyecto Prueba", 1);
        proyecto.Codigo.Should().Be("PROY-01");
        proyecto.Estado.Should().Be("activo");
        proyecto.Anulada.Should().BeFalse();
    }

    [Fact]
    public void Crear_CodigoVacio_LanzaExcepcion()
    {
        var act = () => Proyecto.Crear("", "Descripcion", 1);
        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void Crear_DescripcionVacia_LanzaExcepcion()
    {
        var act = () => Proyecto.Crear("PROY-01", "", 1);
        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void Crear_SucursalIdCero_LanzaExcepcion()
    {
        var act = () => Proyecto.Crear("PROY-01", "Descripcion", 0);
        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void Crear_CodigoSeNormalizaAMayusculas()
    {
        var proyecto = Proyecto.Crear("proy-01", "Descripcion", 1);
        proyecto.Codigo.Should().Be("PROY-01");
    }

    [Fact]
    public void Finalizar_CambiaEstadoAFinalizado()
    {
        var proyecto = Proyecto.Crear("PROY-01", "Proyecto", 1);
        proyecto.Finalizar();
        proyecto.Estado.Should().Be("finalizado");
    }

    [Fact]
    public void Anular_MarcaAnuladaYCambiaEstado()
    {
        var proyecto = Proyecto.Crear("PROY-01", "Proyecto", 1);
        proyecto.Anular();
        proyecto.Estado.Should().Be("anulado");
        proyecto.Anulada.Should().BeTrue();
    }

    [Fact]
    public void Reactivar_CambiaEstadoAActivo()
    {
        var proyecto = Proyecto.Crear("PROY-01", "Proyecto", 1);
        proyecto.Finalizar();
        proyecto.Reactivar();
        proyecto.Estado.Should().Be("activo");
    }

    [Fact]
    public void Actualizar_ConDescripcionValida_ActualizaCampos()
    {
        var proyecto = Proyecto.Crear("PROY-01", "Original", 1);
        proyecto.Actualizar("Modificado", null, null, 5, true, "obs");
        proyecto.Descripcion.Should().Be("Modificado");
        proyecto.TotalCuotas.Should().Be(5);
        proyecto.SoloPadre.Should().BeTrue();
        proyecto.Observacion.Should().Be("obs");
    }

    [Fact]
    public void Actualizar_DescripcionVacia_LanzaExcepcion()
    {
        var proyecto = Proyecto.Crear("PROY-01", "Original", 1);
        var act = () => proyecto.Actualizar("", null, null, 0, false, null);
        act.Should().Throw<ArgumentException>();
    }
}

// ─────────────────────────────────────────────────────────────────────────────
// Timbrado
// ─────────────────────────────────────────────────────────────────────────────

public class TimbradoTests
{
    private static Timbrado TimbradoValido() => Timbrado.Crear(
        1, 1, 1,
        "12345678",
        new DateOnly(2025, 1, 1),
        new DateOnly(2025, 12, 31),
        1, 999999);

    [Fact]
    public void Crear_ConDatosValidos_RetornaInstancia()
    {
        var t = TimbradoValido();
        t.NroTimbrado.Should().Be("12345678");
        t.Activo.Should().BeTrue();
    }

    [Fact]
    public void Crear_FechaFinAnteriorAInicio_LanzaExcepcion()
    {
        var act = () => Timbrado.Crear(1, 1, 1, "123",
            new DateOnly(2025, 12, 31), new DateOnly(2025, 1, 1), 1, 999);
        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void Crear_RangoComprobanteInvalido_LanzaExcepcion()
    {
        var act = () => Timbrado.Crear(1, 1, 1, "123",
            new DateOnly(2025, 1, 1), new DateOnly(2025, 12, 31), 0, 999);
        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void Crear_RangoHastaMenorQueDesde_LanzaExcepcion()
    {
        var act = () => Timbrado.Crear(1, 1, 1, "123",
            new DateOnly(2025, 1, 1), new DateOnly(2025, 12, 31), 100, 50);
        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void EsVigente_FechaDentroDeRangoYActivo_RetornaTrue()
    {
        var t = TimbradoValido();
        t.EsVigente(new DateOnly(2025, 6, 15)).Should().BeTrue();
    }

    [Fact]
    public void EsVigente_FechaFueraDeRango_RetornaFalse()
    {
        var t = TimbradoValido();
        t.EsVigente(new DateOnly(2024, 12, 31)).Should().BeFalse();
    }

    [Fact]
    public void EsVigente_ActivoFalse_RetornaFalse()
    {
        var t = TimbradoValido();
        t.Desactivar();
        t.EsVigente(new DateOnly(2025, 6, 15)).Should().BeFalse();
    }

    [Fact]
    public void Desactivar_Activar_CambiaEstado()
    {
        var t = TimbradoValido();
        t.Desactivar();
        t.Activo.Should().BeFalse();
        t.Activar();
        t.Activo.Should().BeTrue();
    }

    [Fact]
    public void Actualizar_ConDatosValidos_ActualizaRangos()
    {
        var t = TimbradoValido();
        var nuevaInicio = new DateOnly(2026, 1, 1);
        var nuevaFin    = new DateOnly(2026, 12, 31);

        t.Actualizar(nuevaInicio, nuevaFin, 1, 999);

        t.FechaInicio.Should().Be(nuevaInicio);
        t.FechaFin.Should().Be(nuevaFin);
        t.NroComprobanteDesde.Should().Be(1);
        t.NroComprobanteHasta.Should().Be(999);
    }

    [Fact]
    public void Actualizar_FechaFinMenorQueInicio_LanzaExcepcion()
    {
        var t = TimbradoValido();
        var act = () => t.Actualizar(new DateOnly(2026, 12, 31), new DateOnly(2026, 1, 1), 1, 100);
        act.Should().Throw<ArgumentException>().WithMessage("*fechas*");
    }

    [Fact]
    public void Actualizar_NroHastaMenorQueDesde_LanzaExcepcion()
    {
        var t = TimbradoValido();
        var fecha = new DateOnly(2026, 1, 1);
        var act = () => t.Actualizar(fecha, fecha.AddMonths(6), 100, 50);
        act.Should().Throw<ArgumentException>().WithMessage("*comprobantes*");
    }
}

// ─────────────────────────────────────────────────────────────────────────────
// Usuario
// ─────────────────────────────────────────────────────────────────────────────

public class UsuarioTests
{
    [Fact]
    public void Crear_ConDatosValidos_RetornaInstancia()
    {
        var usuario = Usuario.Crear("jperez", "Juan Pérez", "juan@empresa.com", null, null);
        usuario.UserName.Should().Be("jperez");
        usuario.Activo.Should().BeTrue();
    }

    [Fact]
    public void Crear_UserNameVacio_LanzaExcepcion()
    {
        var act = () => Usuario.Crear("", null, null, null, null);
        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void Crear_UserNameSeNormalizaAMinusculas()
    {
        var usuario = Usuario.Crear("JPEREZ", null, null, null, null);
        usuario.UserName.Should().Be("jperez");
    }

    [Fact]
    public void Crear_EmailSeNormalizaAMinusculas()
    {
        var usuario = Usuario.Crear("jperez", null, "Juan@EMPRESA.COM", null, null);
        usuario.Email.Should().Be("juan@empresa.com");
    }

    [Fact]
    public void AsignarSucursal_SucursalNueva_SeAgregaUnaVez()
    {
        var usuario = Usuario.Crear("jperez", null, null, null, null);
        usuario.AsignarSucursal(1);
        usuario.AsignarSucursal(1); // idempotente

        usuario.Sucursales.Should().HaveCount(1);
    }

    [Fact]
    public void RemoverSucursal_SucursalExistente_LaElimina()
    {
        var usuario = Usuario.Crear("jperez", null, null, null, null);
        usuario.AsignarSucursal(1);
        usuario.RemoverSucursal(1);

        usuario.Sucursales.Should().BeEmpty();
    }

    [Fact]
    public void RemoverSucursal_SucursalNoExistente_NoCambiaColeccion()
    {
        var usuario = Usuario.Crear("jperez", null, null, null, null);
        usuario.RemoverSucursal(99); // no hace nada
        usuario.Sucursales.Should().BeEmpty();
    }

    [Fact]
    public void Desactivar_Activar_CambiaEstado()
    {
        var usuario = Usuario.Crear("jperez", null, null, null, null);
        usuario.Desactivar(null);
        usuario.Activo.Should().BeFalse();
        usuario.Activar(null);
        usuario.Activo.Should().BeTrue();
    }

    [Fact]
    public void SetArqueoActual_AsignaValor()
    {
        var usuario = Usuario.Crear("jperez", null, null, null, null);
        usuario.SetArqueoActual(42);
        usuario.ArqueoActual.Should().Be(42);
    }
}
