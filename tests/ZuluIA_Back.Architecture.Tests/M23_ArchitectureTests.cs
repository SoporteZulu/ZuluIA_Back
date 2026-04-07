using FluentAssertions;
using NetArchTest.Rules;
using Xunit;
using ZuluIA_Back.Architecture.Tests.Helpers;

namespace ZuluIA_Back.Architecture.Tests;

/// <summary>
/// Valida reglas arquitectónicas para las entidades del batch M23:
/// ImpuestoPorSucursal, ItemComponente (BOM), Proyecto, ComprobanteProyecto,
/// MovimientoStockAtributo, TipoEntrega, ComprobanteEntrega, ComprobanteDetalleCosto.
/// </summary>
public class M23ArchitectureTests
{
    // ── Residencia en namespace correcto ─────────────────────────────────────

    [Fact]
    public void ImpuestoPorSucursal_DebeResidirEnNamespaceImpuestos()
    {
        var result = Types
            .InAssembly(AssemblyReferences.DomainAssembly)
            .That()
            .HaveNameStartingWith("ImpuestoPorSucursal")
            .Should()
            .ResideInNamespaceContaining("Impuestos")
            .GetResult();

        result.IsSuccessful.Should().BeTrue(
            because: "ImpuestoPorSucursal debe estar en el namespace Impuestos. " +
                     "Tipos fallidos: " + string.Join(", ", result.FailingTypeNames ?? []));
    }

    [Fact]
    public void ItemComponente_DebeResidirEnNamespaceItems()
    {
        var result = Types
            .InAssembly(AssemblyReferences.DomainAssembly)
            .That()
            .HaveNameStartingWith("ItemComponente")
            .Should()
            .ResideInNamespaceContaining("Items")
            .GetResult();

        result.IsSuccessful.Should().BeTrue(
            because: "ItemComponente debe estar en el namespace Items. " +
                     "Tipos fallidos: " + string.Join(", ", result.FailingTypeNames ?? []));
    }

    [Fact]
    public void EntidadesProyectos_DebenResidirEnNamespaceProyectos()
    {
        var result = Types
            .InAssembly(AssemblyReferences.DomainAssembly)
            .That()
            .HaveNameStartingWith("Proyecto")
            .Or()
            .HaveNameStartingWith("ComprobanteProyecto")
            .Should()
            .ResideInNamespaceContaining("Proyectos")
            .GetResult();

        result.IsSuccessful.Should().BeTrue(
            because: "Proyecto y ComprobanteProyecto deben estar en el namespace Proyectos. " +
                     "Tipos fallidos: " + string.Join(", ", result.FailingTypeNames ?? []));
    }

    [Fact]
    public void MovimientoStockAtributo_DebeResidirEnNamespaceStock()
    {
        var result = Types
            .InAssembly(AssemblyReferences.DomainAssembly)
            .That()
            .HaveNameStartingWith("MovimientoStockAtributo")
            .Should()
            .ResideInNamespaceContaining("Stock")
            .GetResult();

        result.IsSuccessful.Should().BeTrue(
            because: "MovimientoStockAtributo debe estar en el namespace Stock. " +
                     "Tipos fallidos: " + string.Join(", ", result.FailingTypeNames ?? []));
    }

    [Fact]
    public void EntidadesEntregaYCostos_DebenResidirEnNamespaceComprobantes()
    {
        var result = Types
            .InAssembly(AssemblyReferences.DomainAssembly)
            .That()
            .HaveNameStartingWith("TipoEntrega")
            .Or()
            .HaveNameStartingWith("ComprobanteEntrega")
            .Or()
            .HaveNameStartingWith("ComprobanteDetalleCosto")
            .Should()
            .ResideInNamespaceContaining("Comprobantes")
            .GetResult();

        result.IsSuccessful.Should().BeTrue(
            because: "TipoEntrega, ComprobanteEntrega y ComprobanteDetalleCosto deben estar en el namespace Comprobantes. " +
                     "Tipos fallidos: " + string.Join(", ", result.FailingTypeNames ?? []));
    }

    // ── Constructor privado + fábrica estática (encapsulación) ───────────────

    [Fact]
    public void EntidadesM23_DebenTenerConstructorPrivado()
    {
        var entidadesM23 = new[]
        {
            "ImpuestoPorSucursal",
            "ItemComponente",
            "Proyecto",
            "ComprobanteProyecto",
            "MovimientoStockAtributo",
            "TipoEntrega",
            "ComprobanteEntrega",
            "ComprobanteDetalleCosto"
        };

        var tiposEncontrados = AssemblyReferences.DomainAssembly
            .GetTypes()
            .Where(t => t.IsClass && !t.IsAbstract && entidadesM23.Contains(t.Name))
            .ToList();

        tiposEncontrados.Should().HaveCount(8,
            because: "deben existir exactamente 8 entidades M23 en el ensamblado Domain.");

        foreach (var type in tiposEncontrados)
        {
            var tieneConstructorPrivado = type
                .GetConstructors(System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                .Any();

            tieneConstructorPrivado.Should().BeTrue(
                because: $"{type.Name} debe tener un constructor privado para forzar el uso de la fábrica estática.");

            var tieneConstructorPublico = type
                .GetConstructors(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance)
                .Any();

            tieneConstructorPublico.Should().BeFalse(
                because: $"{type.Name} no debe exponer constructor público — use Crear().");
        }
    }

    [Fact]
    public void EntidadesM23_DebenTenerMetodoCrear()
    {
        var entidadesM23 = new[]
        {
            "ImpuestoPorSucursal",
            "ItemComponente",
            "Proyecto",
            "ComprobanteProyecto",
            "MovimientoStockAtributo",
            "TipoEntrega",
            "ComprobanteEntrega",
            "ComprobanteDetalleCosto"
        };

        var tiposEncontrados = AssemblyReferences.DomainAssembly
            .GetTypes()
            .Where(t => t.IsClass && !t.IsAbstract && entidadesM23.Contains(t.Name))
            .ToList();

        tiposEncontrados.Should().HaveCount(8,
            because: "deben existir exactamente 8 entidades M23.");

        foreach (var type in tiposEncontrados)
        {
            var tieneCrear = type
                .GetMethods(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static)
                .Any(m => m.Name == "Crear");

            tieneCrear.Should().BeTrue(
                because: $"{type.Name} debe exponer un método estático Crear() como fábrica principal.");
        }
    }

    // ── Herencia de BaseEntity ────────────────────────────────────────────────

    [Fact]
    public void EntidadesM23_DebenHeredarDeBaseEntity()
    {
        var baseEntityType = typeof(ZuluIA_Back.Domain.Common.BaseEntity);

        var entidadesM23 = new[]
        {
            "ImpuestoPorSucursal",
            "ItemComponente",
            "Proyecto",
            "ComprobanteProyecto",
            "MovimientoStockAtributo",
            "TipoEntrega",
            "ComprobanteEntrega",
            "ComprobanteDetalleCosto"
        };

        var result = Types
            .InAssembly(AssemblyReferences.DomainAssembly)
            .That()
            .HaveNameMatching("^(" + string.Join("|", entidadesM23) + ")$")
            .And()
            .AreClasses()
            .Should()
            .Inherit(baseEntityType)
            .GetResult();

        result.IsSuccessful.Should().BeTrue(
            because: "Todas las entidades M23 deben heredar de BaseEntity. " +
                     "Tipos fallidos: " + string.Join(", ", result.FailingTypeNames ?? []));
    }

    // ── DbSets expuestos en IApplicationDbContext ─────────────────────────────

    [Fact]
    public void IApplicationDbContext_DebeExponerDbSetsM23()
    {
        var iface = typeof(ZuluIA_Back.Application.Common.Interfaces.IApplicationDbContext);

        var dbSetNames = new[]
        {
            "ImpuestosPorSucursal",
            "ItemsComponentes",
            "Proyectos",
            "ComprobantesProyectos",
            "MovimientosStockAtributos",
            "TiposEntrega",
            "ComprobantesEntregas",
            "ComprobantesDetallesCostos"
        };

        foreach (var name in dbSetNames)
        {
            var prop = iface.GetProperty(name);
            prop.Should().NotBeNull(
                because: $"IApplicationDbContext debe exponer la propiedad '{name}' (DbSet M23).");
        }
    }
}
