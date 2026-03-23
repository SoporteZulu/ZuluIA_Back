using FluentAssertions;
using NetArchTest.Rules;
using Xunit;
using ZuluIA_Back.Architecture.Tests.Helpers;

namespace ZuluIA_Back.Architecture.Tests;

/// <summary>
/// Valida reglas arquitectónicas para las entidades del batch M24:
/// Banco, Region, Aspecto, Variable, ComprobanteFormaPago, CierreCaja,
/// TipoComprobanteSucursal, RetencionRegimen, Area, InventarioConteo.
/// </summary>
public class M24ArchitectureTests
{
    // ── Residencia en namespace correcto ─────────────────────────────────────

    [Fact]
    public void EntidadesFinancierasM24_DebenResidirEnNamespaceFinanzas()
    {
        var result = Types
            .InAssembly(AssemblyReferences.DomainAssembly)
            .That()
            .HaveNameStartingWith("Banco")
            .Or()
            .HaveNameStartingWith("CierreCaja")
            .Or()
            .HaveNameStartingWith("RetencionRegimen")
            .Should()
            .ResideInNamespaceContaining("Finanzas")
            .GetResult();

        result.IsSuccessful.Should().BeTrue(
            because: "Banco, CierreCaja y RetencionRegimen deben estar en el namespace Finanzas. " +
                     "Tipos fallidos: " + string.Join(", ", result.FailingTypeNames ?? []));
    }

    [Fact]
    public void Region_DebeResidirEnNamespaceGeografia()
    {
        var result = Types
            .InAssembly(AssemblyReferences.DomainAssembly)
            .That()
            .HaveNameStartingWith("Region")
            .Should()
            .ResideInNamespaceContaining("Geografia")
            .GetResult();

        result.IsSuccessful.Should().BeTrue(
            because: "Region debe estar en el namespace Geografia. " +
                     "Tipos fallidos: " + string.Join(", ", result.FailingTypeNames ?? []));
    }

    [Fact]
    public void EntidadesConfiguracionM24_DebenResidirEnNamespaceConfiguracion()
    {
        var result = Types
            .InAssembly(AssemblyReferences.DomainAssembly)
            .That()
            .HaveNameStartingWith("Aspecto")
            .Or()
            .HaveNameStartingWith("Variable")
            .Should()
            .ResideInNamespaceContaining("Configuracion")
            .GetResult();

        result.IsSuccessful.Should().BeTrue(
            because: "Aspecto y Variable deben estar en el namespace Configuracion. " +
                     "Tipos fallidos: " + string.Join(", ", result.FailingTypeNames ?? []));
    }

    [Fact]
    public void ComprobanteFormaPago_DebeResidirEnNamespaceComprobantes()
    {
        var result = Types
            .InAssembly(AssemblyReferences.DomainAssembly)
            .That()
            .HaveNameStartingWith("ComprobanteFormaPago")
            .Should()
            .ResideInNamespaceContaining("Comprobantes")
            .GetResult();

        result.IsSuccessful.Should().BeTrue(
            because: "ComprobanteFormaPago debe estar en el namespace Comprobantes. " +
                     "Tipos fallidos: " + string.Join(", ", result.FailingTypeNames ?? []));
    }

    [Fact]
    public void TipoComprobanteSucursal_DebeResidirEnNamespaceFacturacion()
    {
        var result = Types
            .InAssembly(AssemblyReferences.DomainAssembly)
            .That()
            .HaveNameStartingWith("TipoComprobanteSucursal")
            .Should()
            .ResideInNamespaceContaining("Facturacion")
            .GetResult();

        result.IsSuccessful.Should().BeTrue(
            because: "TipoComprobanteSucursal debe estar en el namespace Facturacion. " +
                     "Tipos fallidos: " + string.Join(", ", result.FailingTypeNames ?? []));
    }

    [Fact]
    public void Area_DebeResidirEnNamespaceSucursales()
    {
        var result = Types
            .InAssembly(AssemblyReferences.DomainAssembly)
            .That()
            .HaveNameStartingWith("Area")
            .Should()
            .ResideInNamespaceContaining("Sucursales")
            .GetResult();

        result.IsSuccessful.Should().BeTrue(
            because: "Area debe estar en el namespace Sucursales. " +
                     "Tipos fallidos: " + string.Join(", ", result.FailingTypeNames ?? []));
    }

    [Fact]
    public void InventarioConteo_DebeResidirEnNamespaceStock()
    {
        var result = Types
            .InAssembly(AssemblyReferences.DomainAssembly)
            .That()
            .HaveNameStartingWith("InventarioConteo")
            .Should()
            .ResideInNamespaceContaining("Stock")
            .GetResult();

        result.IsSuccessful.Should().BeTrue(
            because: "InventarioConteo debe estar en el namespace Stock. " +
                     "Tipos fallidos: " + string.Join(", ", result.FailingTypeNames ?? []));
    }

    // ── Constructor privado + fábrica estática (encapsulación) ───────────────

    [Fact]
    public void EntidadesM24_DebenTenerConstructorPrivado()
    {
        var entidadesM24 = new[]
        {
            "Banco",
            "Region",
            "Aspecto",
            "Variable",
            "ComprobanteFormaPago",
            "CierreCaja",
            "TipoComprobanteSucursal",
            "RetencionRegimen",
            "Area",
            "InventarioConteo"
        };

        var tiposEncontrados = AssemblyReferences.DomainAssembly
            .GetTypes()
            .Where(t => t.IsClass && !t.IsAbstract && entidadesM24.Contains(t.Name))
            .ToList();

        tiposEncontrados.Should().HaveCount(10,
            because: "deben existir exactamente 10 entidades M24 en el ensamblado Domain.");

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
    public void EntidadesM24_DebenTenerMetodoCrear()
    {
        var entidadesM24 = new[]
        {
            "Banco",
            "Region",
            "Aspecto",
            "Variable",
            "ComprobanteFormaPago",
            "CierreCaja",
            "TipoComprobanteSucursal",
            "RetencionRegimen",
            "Area",
            "InventarioConteo"
        };

        var tiposEncontrados = AssemblyReferences.DomainAssembly
            .GetTypes()
            .Where(t => t.IsClass && !t.IsAbstract && entidadesM24.Contains(t.Name))
            .ToList();

        tiposEncontrados.Should().HaveCount(10,
            because: "deben existir exactamente 10 entidades M24.");

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
    public void EntidadesM24_DebenHeredarDeBaseEntity()
    {
        var baseEntityType = typeof(ZuluIA_Back.Domain.Common.BaseEntity);

        var entidadesM24 = new[]
        {
            "Banco",
            "Region",
            "Aspecto",
            "Variable",
            "ComprobanteFormaPago",
            "CierreCaja",
            "TipoComprobanteSucursal",
            "RetencionRegimen",
            "Area",
            "InventarioConteo"
        };

        var result = Types
            .InAssembly(AssemblyReferences.DomainAssembly)
            .That()
            .HaveNameMatching("^(" + string.Join("|", entidadesM24) + ")$")
            .And()
            .AreClasses()
            .Should()
            .Inherit(baseEntityType)
            .GetResult();

        result.IsSuccessful.Should().BeTrue(
            because: "Todas las entidades M24 deben heredar de BaseEntity. " +
                     "Tipos fallidos: " + string.Join(", ", result.FailingTypeNames ?? []));
    }

    // ── DbSets expuestos en IApplicationDbContext ─────────────────────────────

    [Fact]
    public void IApplicationDbContext_DebeExponerDbSetsM24()
    {
        var iface = typeof(ZuluIA_Back.Application.Common.Interfaces.IApplicationDbContext);

        var dbSetNames = new[]
        {
            "Bancos",
            "Regiones",
            "Aspectos",
            "Variables",
            "ComprobantesFormasPago",
            "CierresCaja",
            "TiposComprobantesSucursal",
            "RetencionesRegimenes",
            "Areas",
            "InventariosConteo"
        };

        foreach (var name in dbSetNames)
        {
            var prop = iface.GetProperty(name);
            prop.Should().NotBeNull(
                because: $"IApplicationDbContext debe exponer la propiedad '{name}' (DbSet M24).");
        }
    }
}
