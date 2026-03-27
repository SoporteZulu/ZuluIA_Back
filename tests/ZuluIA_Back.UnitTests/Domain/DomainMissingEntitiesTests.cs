using FluentAssertions;
using Xunit;
using ZuluIA_Back.Domain.Entities.Configuracion;
using ZuluIA_Back.Domain.Entities.Finanzas;
using ZuluIA_Back.Domain.Entities.Geografia;
using ZuluIA_Back.Domain.Entities.Impuestos;
using ZuluIA_Back.Domain.Entities.Items;
using ZuluIA_Back.Domain.Entities.Precios;
using ZuluIA_Back.Domain.Entities.Sucursales;
using ZuluIA_Back.Domain.Entities.Terceros;
using ZuluIA_Back.Domain.Events.Finanzas;
using ZuluIA_Back.Domain.Events.Terceros;
using ZuluIA_Back.Domain.ValueObjects;

namespace ZuluIA_Back.UnitTests.Domain;

// ─────────────────────────────────────────────────────────────────────────────
// Sucursal
// ─────────────────────────────────────────────────────────────────────────────
public class SucursalEntityTests
{
    [Fact]
    public void Crear_ConDatosValidos_AsignaValores()
    {
        var sucursal = Sucursal.Crear("Mi Empresa S.A.", "20-12345678-9", 1, 1, 1, true, null);

        sucursal.RazonSocial.Should().Be("Mi Empresa S.A.");
        sucursal.Cuit.Should().Be("20-12345678-9");
        sucursal.CondicionIvaId.Should().Be(1);
        sucursal.MonedaId.Should().Be(1);
        sucursal.PaisId.Should().Be(1);
        sucursal.CasaMatriz.Should().BeTrue();
        sucursal.Activa.Should().BeTrue();
    }

    [Fact]
    public void Crear_RazonSocialVacia_LanzaArgumentException()
    {
        var act = () => Sucursal.Crear("", "20-12345678-9", 1, 1, 1, false, null);
        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void Crear_CuitVacio_LanzaArgumentException()
    {
        var act = () => Sucursal.Crear("Empresa", "   ", 1, 1, 1, false, null);
        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void Crear_TrimeaRazonSocialYCuit()
    {
        var sucursal = Sucursal.Crear("  Empresa S.A.  ", "  20-12345678-9  ", 1, 1, 1, false, null);

        sucursal.RazonSocial.Should().Be("Empresa S.A.");
        sucursal.Cuit.Should().Be("20-12345678-9");
    }

    [Fact]
    public void Desactivar_CambiaActivaAFalse()
    {
        var sucursal = Sucursal.Crear("Empresa", "20-12345678-9", 1, 1, 1, false, null);

        sucursal.Desactivar(null);

        sucursal.Activa.Should().BeFalse();
    }

    [Fact]
    public void Activar_RestableceActivaATrue()
    {
        var sucursal = Sucursal.Crear("Empresa", "20-12345678-9", 1, 1, 1, false, null);
        sucursal.Desactivar(null);

        sucursal.Activar(null);

        sucursal.Activa.Should().BeTrue();
    }

    [Fact]
    public void Actualizar_RazonSocialVacia_LanzaArgumentException()
    {
        var sucursal = Sucursal.Crear("Empresa", "20-12345678-9", 1, 1, 1, false, null);
        var domicilio = ZuluIA_Back.Domain.ValueObjects.Domicilio.Vacio();

        var act = () => sucursal.Actualizar("", null, "20-12345678-9", null, 1, 1, 1,
            domicilio, null, null, null, null, null, null, 443, false, null);
        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void Actualizar_ActualizaEmailEnMinusculas()
    {
        var sucursal = Sucursal.Crear("Empresa", "20-12345678-9", 1, 1, 1, false, null);
        var domicilio = ZuluIA_Back.Domain.ValueObjects.Domicilio.Vacio();

        sucursal.Actualizar("Empresa", null, "20-12345678-9", null, 1, 1, 1,
            domicilio, null, "ADMIN@EMPRESA.COM", null, null, null, null, 443, false, null);

        sucursal.Email.Should().Be("admin@empresa.com");
    }
}

// ─────────────────────────────────────────────────────────────────────────────
// Impuesto
// ─────────────────────────────────────────────────────────────────────────────
public class ImpuestoEntityTests
{
    [Fact]
    public void Crear_ConDatosValidos_AsignaValores()
    {
        var imp = Impuesto.Crear("IIBB-PBA", "IIBB Provincia Buenos Aires", 3.5m);

        imp.Codigo.Should().Be("IIBB-PBA");
        imp.Descripcion.Should().Be("IIBB Provincia Buenos Aires");
        imp.Alicuota.Should().Be(3.5m);
        imp.Tipo.Should().Be("percepcion");
        imp.Activo.Should().BeTrue();
    }

    [Fact]
    public void Crear_CodigoVacio_LanzaArgumentException()
    {
        var act = () => Impuesto.Crear("", "Descripcion", 3.5m);
        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void Crear_DescripcionVacia_LanzaArgumentException()
    {
        var act = () => Impuesto.Crear("COD", "   ", 3.5m);
        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void Crear_AlicuotaNegativa_LanzaArgumentException()
    {
        var act = () => Impuesto.Crear("COD", "Descripcion", -1m);
        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void Crear_CodigoEnUppercase()
    {
        var imp = Impuesto.Crear("iibb-pba", "Descripcion", 3.5m);
        imp.Codigo.Should().Be("IIBB-PBA");
    }

    [Fact]
    public void Crear_TipoEnLowercase()
    {
        var imp = Impuesto.Crear("COD", "Descripcion", 3.5m, tipo: "PERCEPCION");
        imp.Tipo.Should().Be("percepcion");
    }

    [Fact]
    public void Activar_CambiaActivoATrue()
    {
        var imp = Impuesto.Crear("COD", "Descripcion", 3.5m);
        imp.Desactivar();

        imp.Activar();

        imp.Activo.Should().BeTrue();
    }

    [Fact]
    public void Desactivar_CambiaActivoAFalse()
    {
        var imp = Impuesto.Crear("COD", "Descripcion", 3.5m);

        imp.Desactivar();

        imp.Activo.Should().BeFalse();
    }

    [Fact]
    public void Actualizar_DescripcionVacia_LanzaArgumentException()
    {
        var imp = Impuesto.Crear("COD", "Descripcion", 3.5m);

        var act = () => imp.Actualizar("", 3.5m, 0m, "percepcion", null);
        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void Actualizar_CambiaValores()
    {
        var imp = Impuesto.Crear("COD", "Descripcion antigua", 3m);

        imp.Actualizar("Descripcion nueva", 5m, 100m, "retencion", "obs");

        imp.Descripcion.Should().Be("Descripcion nueva");
        imp.Alicuota.Should().Be(5m);
        imp.MinimoBaseCalculo.Should().Be(100m);
        imp.Tipo.Should().Be("retencion");
        imp.Observacion.Should().Be("obs");
    }
}

// ─────────────────────────────────────────────────────────────────────────────
// Variable
// ─────────────────────────────────────────────────────────────────────────────
public class VariableEntityTests
{
    [Fact]
    public void Crear_ConDatosValidos_AsignaValores()
    {
        var variable = Variable.Crear("NROVIAJE", "Número de Viaje");

        variable.Codigo.Should().Be("NROVIAJE");
        variable.Descripcion.Should().Be("Número de Viaje");
        variable.Editable.Should().BeTrue();
    }

    [Fact]
    public void Crear_CodigoVacio_LanzaArgumentException()
    {
        var act = () => Variable.Crear("", "Descripcion");
        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void Crear_DescripcionVacia_LanzaArgumentException()
    {
        var act = () => Variable.Crear("COD", "  ");
        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void Crear_CodigoEnUppercase()
    {
        var variable = Variable.Crear("nroviaje", "Número de Viaje");
        variable.Codigo.Should().Be("NROVIAJE");
    }

    [Fact]
    public void Actualizar_DescripcionVacia_LanzaArgumentException()
    {
        var variable = Variable.Crear("COD", "Descripcion");

        var act = () => variable.Actualizar("", null, null, null, 0, 0, null, null, null, true);
        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void Actualizar_CambiaValores()
    {
        var variable = Variable.Crear("COD", "Descripcion original");

        variable.Actualizar("Descripcion nueva", 1L, 2L, 3L, 1, 2, "ESTRUCT1", "obs", "cond", false);

        variable.Descripcion.Should().Be("Descripcion nueva");
        variable.TipoVariableId.Should().Be(1L);
        variable.TipoComprobanteId.Should().Be(2L);
        variable.AspectoId.Should().Be(3L);
        variable.Nivel.Should().Be(1);
        variable.Orden.Should().Be(2);
        variable.CodigoEstructura.Should().Be("ESTRUCT1");
        variable.Observacion.Should().Be("obs");
        variable.Condicionante.Should().Be("cond");
        variable.Editable.Should().BeFalse();
    }
}

// ─────────────────────────────────────────────────────────────────────────────
// OpcionVariable
// ─────────────────────────────────────────────────────────────────────────────
public class OpcionVariableEntityTests
{
    [Fact]
    public void Crear_ConDatosValidos_AsignaValores()
    {
        var opcion = OpcionVariable.Crear("OPT1", "Opción 1");

        opcion.Codigo.Should().Be("OPT1");
        opcion.Descripcion.Should().Be("Opción 1");
        opcion.Observaciones.Should().BeNull();
    }

    [Fact]
    public void Crear_CodigoVacio_LanzaArgumentException()
    {
        var act = () => OpcionVariable.Crear("", "Descripcion");
        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void Crear_DescripcionVacia_LanzaArgumentException()
    {
        var act = () => OpcionVariable.Crear("OPT1", "   ");
        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void Crear_CodigoEnUppercase()
    {
        var opcion = OpcionVariable.Crear("opt1", "Descripcion");
        opcion.Codigo.Should().Be("OPT1");
    }

    [Fact]
    public void Crear_ConObservaciones_AsignaObservaciones()
    {
        var opcion = OpcionVariable.Crear("OPT1", "Descripcion", "Observaciones de prueba");
        opcion.Observaciones.Should().Be("Observaciones de prueba");
    }

    [Fact]
    public void Actualizar_CodigoVacio_LanzaArgumentException()
    {
        var opcion = OpcionVariable.Crear("OPT1", "Descripcion");
        var act = () => opcion.Actualizar("", "Desc", null);
        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void Actualizar_CambiaValores()
    {
        var opcion = OpcionVariable.Crear("OPT1", "Original");

        opcion.Actualizar("OPT2", "Modificado", "Nueva observación");

        opcion.Codigo.Should().Be("OPT2");
        opcion.Descripcion.Should().Be("Modificado");
        opcion.Observaciones.Should().Be("Nueva observación");
    }
}

// ─────────────────────────────────────────────────────────────────────────────
// Banco
// ─────────────────────────────────────────────────────────────────────────────
public class BancoEntityTests
{
    [Fact]
    public void Crear_ConDescripcionValida_AsignaValor()
    {
        var banco = Banco.Crear("Banco Nación Argentina");

        banco.Descripcion.Should().Be("Banco Nación Argentina");
    }

    [Fact]
    public void Crear_DescripcionVacia_LanzaArgumentException()
    {
        var act = () => Banco.Crear("");
        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void Crear_DescripcionConEspacios_LanzaArgumentException()
    {
        var act = () => Banco.Crear("   ");
        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void Crear_TrimeaDescripcion()
    {
        var banco = Banco.Crear("  Banco BBVA  ");
        banco.Descripcion.Should().Be("Banco BBVA");
    }

    [Fact]
    public void Actualizar_DescripcionVacia_LanzaArgumentException()
    {
        var banco = Banco.Crear("Banco Nación");
        var act = () => banco.Actualizar("");
        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void Actualizar_CambiaDescripcion()
    {
        var banco = Banco.Crear("Banco Nación");

        banco.Actualizar("Banco Ciudad");

        banco.Descripcion.Should().Be("Banco Ciudad");
    }
}

// ─────────────────────────────────────────────────────────────────────────────
// Localidad
// ─────────────────────────────────────────────────────────────────────────────
public class LocalidadEntityTests
{
    [Fact]
    public void Crear_ConDatosValidos_AsignaValores()
    {
        var localidad = Localidad.Crear(1, "Buenos Aires", "1000");

        localidad.ProvinciaId.Should().Be(1);
        localidad.Descripcion.Should().Be("Buenos Aires");
        localidad.CodigoPostal.Should().Be("1000");
    }

    [Fact]
    public void Crear_DescripcionVacia_LanzaArgumentException()
    {
        var act = () => Localidad.Crear(1, "  ");
        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void Crear_SinCodigoPostal_DejaEnNull()
    {
        var localidad = Localidad.Crear(1, "Localidad");
        localidad.CodigoPostal.Should().BeNull();
    }

    [Fact]
    public void Crear_TrimeaDescripcionYCodigoPostal()
    {
        var localidad = Localidad.Crear(1, "  Cordoba  ", "  5000  ");
        localidad.Descripcion.Should().Be("Cordoba");
        localidad.CodigoPostal.Should().Be("5000");
    }
}

// ─────────────────────────────────────────────────────────────────────────────
// Atributo
// ─────────────────────────────────────────────────────────────────────────────
public class AtributoEntityTests
{
    [Fact]
    public void Crear_ConDatosValidos_AsignaValores()
    {
        var atributo = Atributo.Crear("Color", "texto");

        atributo.Descripcion.Should().Be("Color");
        atributo.Tipo.Should().Be("texto");
        atributo.Requerido.Should().BeFalse();
        atributo.Activo.Should().BeTrue();
    }

    [Fact]
    public void Crear_DescripcionVacia_LanzaArgumentException()
    {
        var act = () => Atributo.Crear("");
        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void Crear_TipoEnLowercase()
    {
        var atributo = Atributo.Crear("Talla", "TEXTO");
        atributo.Tipo.Should().Be("texto");
    }

    [Fact]
    public void Crear_ConRequeridoTrue_AsignaRequerido()
    {
        var atributo = Atributo.Crear("CUIT", requerido: true);
        atributo.Requerido.Should().BeTrue();
    }

    [Fact]
    public void Activar_CambiaActivoATrue()
    {
        var atributo = Atributo.Crear("Color");
        atributo.Desactivar();

        atributo.Activar();

        atributo.Activo.Should().BeTrue();
    }

    [Fact]
    public void Desactivar_CambiaActivoAFalse()
    {
        var atributo = Atributo.Crear("Color");

        atributo.Desactivar();

        atributo.Activo.Should().BeFalse();
    }

    [Fact]
    public void Actualizar_DescripcionVacia_LanzaArgumentException()
    {
        var atributo = Atributo.Crear("Color");
        var act = () => atributo.Actualizar("", "texto", false);
        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void Actualizar_CambiaValores()
    {
        var atributo = Atributo.Crear("Color", "texto", false);

        atributo.Actualizar("Talla", "numero", true);

        atributo.Descripcion.Should().Be("Talla");
        atributo.Tipo.Should().Be("numero");
        atributo.Requerido.Should().BeTrue();
    }
}

// ─────────────────────────────────────────────────────────────────────────────
// ListaPreciosItem – PrecioFinal
// ─────────────────────────────────────────────────────────────────────────────
public class ListaPreciosItemPrecioFinalTests
{
    // We test PrecioFinal via ListaPrecios.UpsertItem which creates ListaPreciosItem internally.

    [Fact]
    public void PrecioFinal_SinDescuento_EsIgualAlPrecio()
    {
        var lista = ListaPrecios.Crear("Lista Test", 1, null, null, null);
        lista.UpsertItem(1, 100m, 0m);

        var item = lista.Items.First();
        item.PrecioFinal.Should().Be(100m);
    }

    [Fact]
    public void PrecioFinal_ConDescuentoDel10Pct_CalculaCorrectamente()
    {
        var lista = ListaPrecios.Crear("Lista Test", 1, null, null, null);
        lista.UpsertItem(1, 200m, 10m);

        var item = lista.Items.First();
        item.PrecioFinal.Should().Be(180m); // 200 * (1 - 10/100) = 180
    }

    [Fact]
    public void PrecioFinal_ConDescuentoDel50Pct_CalculaCorrectamente()
    {
        var lista = ListaPrecios.Crear("Lista Test", 1, null, null, null);
        lista.UpsertItem(1, 1000m, 50m);

        var item = lista.Items.First();
        item.PrecioFinal.Should().Be(500m);
    }

    [Fact]
    public void PrecioFinal_RondeoA4Decimales()
    {
        var lista = ListaPrecios.Crear("Lista Test", 1, null, null, null);
        lista.UpsertItem(1, 100m, 33.333m);

        var item = lista.Items.First();
        // 100 * (1 - 33.333/100) = 100 * 0.66667 = 66.667
        item.PrecioFinal.Should().Be(Math.Round(100m * (1 - 33.333m / 100), 4));
    }
}

// ─────────────────────────────────────────────────────────────────────────────
// Domain events – Cobro
// ─────────────────────────────────────────────────────────────────────────────
public class CobroDomainEventTests
{
    private static readonly DateOnly Hoy = DateOnly.FromDateTime(DateTime.Today);

    [Fact]
    public void AgregarMedio_DebeRaisearCobroRegistradoEvent()
    {
        var cobro = Cobro.Crear(1L, 2L, Hoy, 1L, 1m, null, null);
        cobro.ClearDomainEvents();
        var medio = CobroMedio.Crear(cobro.Id, 10L, 5L, null, 500m, 1L, 1m);

        cobro.AgregarMedio(medio);

        cobro.DomainEvents.Should().ContainSingle()
             .Which.Should().BeOfType<CobroRegistradoEvent>();
    }

    [Fact]
    public void Anular_DebeRaisearCobroAnuladoEvent()
    {
        var cobro = Cobro.Crear(1L, 2L, Hoy, 1L, 1m, null, null);
        cobro.AgregarMedio(CobroMedio.Crear(cobro.Id, 10L, 5L, null, 500m, 1L, 1m));
        cobro.ClearDomainEvents();

        cobro.Anular(null);

        cobro.DomainEvents.Should().ContainSingle()
             .Which.Should().BeOfType<CobroAnuladoEvent>();
    }
}

// ─────────────────────────────────────────────────────────────────────────────
// Domain events – Pago
// ─────────────────────────────────────────────────────────────────────────────
public class PagoDomainEventTests
{
    private static readonly DateOnly Hoy = DateOnly.FromDateTime(DateTime.Today);

    [Fact]
    public void AgregarMedio_DebeRaisearPagoRegistradoEvent()
    {
        var pago = Pago.Crear(1L, 2L, Hoy, 1L, 1m, null, null);
        pago.ClearDomainEvents();
        var medio = PagoMedio.Crear(pago.Id, 10L, 5L, null, 500m, 1L, 1m);

        pago.AgregarMedio(medio);

        pago.DomainEvents.Should().ContainSingle()
             .Which.Should().BeOfType<PagoRegistradoEvent>();
    }

    [Fact]
    public void Anular_DebeRaisearPagoAnuladoEvent()
    {
        var pago = Pago.Crear(1L, 2L, Hoy, 1L, 1m, null, null);
        pago.AgregarMedio(PagoMedio.Crear(pago.Id, 10L, 5L, null, 500m, 1L, 1m));
        pago.ClearDomainEvents();

        pago.Anular(null);

        pago.DomainEvents.Should().ContainSingle()
             .Which.Should().BeOfType<PagoAnuladoEvent>();
    }
}

// ─────────────────────────────────────────────────────────────────────────────
// Domain events – Tercero (Activar / ActualizarRoles)
// ─────────────────────────────────────────────────────────────────────────────
public class TerceroDomainEventExtendedTests
{
    [Fact]
    public void Activar_DebeRaisearTerceroReactivadoEvent()
    {
        var tercero = Tercero.Crear("CLI001", "Empresa SA", 1, "30-11111111-1", 1, true, false, false, null, null);
        tercero.Desactivar(null);
        tercero.ClearDomainEvents();

        tercero.Activar(null);

        tercero.DomainEvents.Should().ContainSingle()
               .Which.Should().BeOfType<TerceroReactivadoEvent>();
    }

    [Fact]
    public void ActualizarRoles_DebeRaisearTerceroRolesActualizadosEvent()
    {
        var tercero = Tercero.Crear("CLI001", "Empresa SA", 1, "30-11111111-1", 1, true, false, false, null, null);
        tercero.ClearDomainEvents();

        tercero.ActualizarRoles(true, true, false, null);

        tercero.DomainEvents.Should().ContainSingle()
               .Which.Should().BeOfType<TerceroRolesActualizadosEvent>();
    }

    [Fact]
    public void ActualizarRoles_TodosRolesFalse_LanzaArgumentException()
    {
        var tercero = Tercero.Crear("CLI001", "Empresa SA", 1, "30-11111111-1", 1, true, false, false, null, null);

        var act = () => tercero.ActualizarRoles(false, false, false, null);

        act.Should().Throw<ArgumentException>()
           .WithMessage("*al menos un rol*");
    }
}
