using FluentAssertions;
using NetArchTest.Rules;
using System.Linq;
using Xunit;
using ZuluIA_Back.Architecture.Tests.Helpers;

namespace ZuluIA_Back.Architecture.Tests;

/// <summary>
/// Valida reglas arquitectónicas para los módulos M28 y M29:
/// ListaPrecioPersona (Precios), UnidadManipulacion (Items),
/// ConfiguracionFiscal (Facturacion), ImpuestoPorTipoComprobante (Impuestos),
/// TipoComprobantePuntoFacturacion (Facturacion).
/// </summary>
public class M28_M29_ArchitectureTests
{
    // ── Namespaces correctos ───────────────────────────────────────────────────

    [Fact]
    public void ListaPrecioPersona_DebeResidirEnNamespacePrecios()
    {
        var result = Types
            .InAssembly(AssemblyReferences.DomainAssembly)
            .That()
            .HaveNameStartingWith("ListaPrecioPersona")
            .Should()
            .ResideInNamespaceContaining("Precios")
            .GetResult();

        result.IsSuccessful.Should().BeTrue(
            because: "ListaPrecioPersona debe estar en el namespace Precios. " +
                     "Tipos fallidos: " + string.Join(", ", result.FailingTypeNames ?? []));
    }

    [Fact]
    public void UnidadManipulacion_DebeResidirEnNamespaceItems()
    {
        var result = Types
            .InAssembly(AssemblyReferences.DomainAssembly)
            .That()
            .HaveNameStartingWith("UnidadManipulacion")
            .Should()
            .ResideInNamespaceContaining("Items")
            .GetResult();

        result.IsSuccessful.Should().BeTrue(
            because: "UnidadManipulacion debe estar en el namespace Items. " +
                     "Tipos fallidos: " + string.Join(", ", result.FailingTypeNames ?? []));
    }

    [Theory]
    [InlineData("ConfiguracionFiscal")]
    [InlineData("TipoComprobantePuntoFacturacion")]
    public void EntidadesFacturacionM28M29_DebenResidirEnNamespaceFacturacion(string typeName)
    {
        var result = Types
            .InAssembly(AssemblyReferences.DomainAssembly)
            .That()
            .HaveNameStartingWith(typeName)
            .Should()
            .ResideInNamespaceContaining("Facturacion")
            .GetResult();

        result.IsSuccessful.Should().BeTrue(
            because: $"{typeName} debe estar en el namespace Facturacion. " +
                     "Tipos fallidos: " + string.Join(", ", result.FailingTypeNames ?? []));
    }

    [Fact]
    public void ImpuestoPorTipoComprobante_DebeResidirEnNamespaceImpuestos()
    {
        var result = Types
            .InAssembly(AssemblyReferences.DomainAssembly)
            .That()
            .HaveNameStartingWith("ImpuestoPorTipoComprobante")
            .Should()
            .ResideInNamespaceContaining("Impuestos")
            .GetResult();

        result.IsSuccessful.Should().BeTrue(
            because: "ImpuestoPorTipoComprobante debe estar en el namespace Impuestos. " +
                     "Tipos fallidos: " + string.Join(", ", result.FailingTypeNames ?? []));
    }

    // ── Encapsulación (sin setters públicos) ───────────────────────────────────

    [Theory]
    [InlineData("ListaPrecioPersona")]
    [InlineData("UnidadManipulacion")]
    [InlineData("ConfiguracionFiscal")]
    [InlineData("ImpuestoPorTipoComprobante")]
    [InlineData("TipoComprobantePuntoFacturacion")]
    public void EntidadesM28M29_NoDebenTenerSettersPublicos(string typeName)
    {
        var tipo = AssemblyReferences.DomainAssembly
            .GetTypes()
            .FirstOrDefault(t => t.Name == typeName);

        tipo.Should().NotBeNull(because: $"El tipo {typeName} debe existir en el ensamblado de dominio.");

        var settersPublicos = tipo!
            .GetProperties()
            .Where(p => p.SetMethod?.IsPublic == true)
            .Select(p => p.Name)
            .ToList();

        settersPublicos.Should().BeEmpty(
            because: $"{typeName} no debería exponer setters públicos (encapsulación DDD). " +
                     "Propiedades con setter público: " + string.Join(", ", settersPublicos));
    }

    // ── Sin dependencias inversas ──────────────────────────────────────────────

    [Theory]
    [InlineData("ListaPrecioPersona")]
    [InlineData("UnidadManipulacion")]
    [InlineData("ConfiguracionFiscal")]
    [InlineData("ImpuestoPorTipoComprobante")]
    [InlineData("TipoComprobantePuntoFacturacion")]
    public void EntidadesM28M29_NoDependenDeInfraestructura(string typeName)
    {
        var result = Types
            .InAssembly(AssemblyReferences.DomainAssembly)
            .That()
            .HaveNameStartingWith(typeName)
            .ShouldNot()
            .HaveDependencyOn("ZuluIA_Back.Infrastructure")
            .GetResult();

        result.IsSuccessful.Should().BeTrue(
            because: $"{typeName} no debe depender de Infrastructure. " +
                     "Tipos fallidos: " + string.Join(", ", result.FailingTypeNames ?? []));
    }

    // ── Herencia de BaseEntity ─────────────────────────────────────────────────

    [Theory]
    [InlineData("ListaPrecioPersona")]
    [InlineData("UnidadManipulacion")]
    [InlineData("ConfiguracionFiscal")]
    [InlineData("ImpuestoPorTipoComprobante")]
    [InlineData("TipoComprobantePuntoFacturacion")]
    public void EntidadesM28M29_DebenHeredarDeBaseEntity(string typeName)
    {
        var baseEntityType = typeof(ZuluIA_Back.Domain.Common.BaseEntity);

        var result = Types
            .InAssembly(AssemblyReferences.DomainAssembly)
            .That()
            .HaveNameStartingWith(typeName)
            .And()
            .AreClasses()
            .Should()
            .Inherit(baseEntityType)
            .GetResult();

        result.IsSuccessful.Should().BeTrue(
            because: $"{typeName} debe heredar de BaseEntity. " +
                     "Tipos fallidos: " + string.Join(", ", result.FailingTypeNames ?? []));
    }
}
