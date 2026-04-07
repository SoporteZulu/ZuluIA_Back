using FluentAssertions;
using NetArchTest.Rules;
using System.Linq;
using Xunit;
using ZuluIA_Back.Architecture.Tests.Helpers;

namespace ZuluIA_Back.Architecture.Tests;

/// <summary>
/// Valida reglas arquitectónicas para los módulos M25:
/// OpcionVariable (Configuracion), Perfil (Sucursales),
/// EmpleadoXArea / EmpleadoXPerfil (RRHH), CierreCajaDetalle (Finanzas).
/// </summary>
public class M25_ArchitectureTests
{
    // ── Namespaces correctos ────────────────────────────────────────────────

    [Fact]
    public void OpcionVariable_DebeResidirEnNamespaceConfiguracion()
    {
        var result = Types
            .InAssembly(AssemblyReferences.DomainAssembly)
            .That()
            .HaveNameStartingWith("OpcionVariable")
            .Should()
            .ResideInNamespaceContaining("Configuracion")
            .GetResult();

        result.IsSuccessful.Should().BeTrue(
            because: "OpcionVariable debe estar en el namespace Configuracion. " +
                     "Tipos fallidos: " + string.Join(", ", result.FailingTypeNames ?? []));
    }

    [Fact]
    public void Perfil_DebeResidirEnNamespaceSucursales()
    {
        var result = Types
            .InAssembly(AssemblyReferences.DomainAssembly)
            .That()
            .HaveNameStartingWith("Perfil")
            .Should()
            .ResideInNamespaceContaining("Sucursales")
            .GetResult();

        result.IsSuccessful.Should().BeTrue(
            because: "Perfil debe estar en el namespace Sucursales. " +
                     "Tipos fallidos: " + string.Join(", ", result.FailingTypeNames ?? []));
    }

    [Fact]
    public void EmpleadoXArea_DebeResidirEnNamespaceRRHH()
    {
        var result = Types
            .InAssembly(AssemblyReferences.DomainAssembly)
            .That()
            .HaveNameStartingWith("EmpleadoXArea")
            .Should()
            .ResideInNamespaceContaining("RRHH")
            .GetResult();

        result.IsSuccessful.Should().BeTrue(
            because: "EmpleadoXArea debe estar en el namespace RRHH. " +
                     "Tipos fallidos: " + string.Join(", ", result.FailingTypeNames ?? []));
    }

    [Fact]
    public void EmpleadoXPerfil_DebeResidirEnNamespaceRRHH()
    {
        var result = Types
            .InAssembly(AssemblyReferences.DomainAssembly)
            .That()
            .HaveNameStartingWith("EmpleadoXPerfil")
            .Should()
            .ResideInNamespaceContaining("RRHH")
            .GetResult();

        result.IsSuccessful.Should().BeTrue(
            because: "EmpleadoXPerfil debe estar en el namespace RRHH. " +
                     "Tipos fallidos: " + string.Join(", ", result.FailingTypeNames ?? []));
    }

    [Fact]
    public void CierreCajaDetalle_DebeResidirEnNamespaceFinanzas()
    {
        var result = Types
            .InAssembly(AssemblyReferences.DomainAssembly)
            .That()
            .HaveNameStartingWith("CierreCajaDetalle")
            .Should()
            .ResideInNamespaceContaining("Finanzas")
            .GetResult();

        result.IsSuccessful.Should().BeTrue(
            because: "CierreCajaDetalle debe estar en el namespace Finanzas. " +
                     "Tipos fallidos: " + string.Join(", ", result.FailingTypeNames ?? []));
    }

    // ── Encapsulación correcta ───────────────────────────────────────────────

    [Theory]
    [InlineData("OpcionVariable")]
    [InlineData("Perfil")]
    [InlineData("EmpleadoXArea")]
    [InlineData("EmpleadoXPerfil")]
    [InlineData("CierreCajaDetalle")]
    public void EntidadesM25_NoDebenTenerSettersPublicos(string typeName)
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

    // ── Sin dependencias inversas ───────────────────────────────────────────

    [Theory]
    [InlineData("OpcionVariable")]
    [InlineData("Perfil")]
    [InlineData("EmpleadoXArea")]
    [InlineData("EmpleadoXPerfil")]
    [InlineData("CierreCajaDetalle")]
    public void EntidadesM25_NoDependenDeInfraestructura(string typeName)
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

    // ── DbSets registrados en la interfaz ───────────────────────────────────

    [Fact]
    public void IApplicationDbContext_DebeExponer_DbSets_M25()
    {
        var interfaceType = AssemblyReferences.ApplicationAssembly
            .GetType("ZuluIA_Back.Application.Common.Interfaces.IApplicationDbContext");

        interfaceType.Should().NotBeNull();

        var propNames = interfaceType!.GetProperties().Select(p => p.Name).ToList();

        propNames.Should().Contain("OpcionesVariable",
            because: "Debe haber un DbSet<OpcionVariable> en IApplicationDbContext (M25).");
        propNames.Should().Contain("Perfiles",
            because: "Debe haber un DbSet<Perfil> en IApplicationDbContext (M25).");
        propNames.Should().Contain("EmpleadosXArea",
            because: "Debe haber un DbSet<EmpleadoXArea> en IApplicationDbContext (M25).");
        propNames.Should().Contain("EmpleadosXPerfil",
            because: "Debe haber un DbSet<EmpleadoXPerfil> en IApplicationDbContext (M25).");
        propNames.Should().Contain("CierresCajaDetalle",
            because: "Debe haber un DbSet<CierreCajaDetalle> en IApplicationDbContext (M25).");
    }
}
