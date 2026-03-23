using FluentAssertions;
using NetArchTest.Rules;
using System.Linq;
using Xunit;
using ZuluIA_Back.Architecture.Tests.Helpers;

namespace ZuluIA_Back.Architecture.Tests;

/// <summary>
/// Valida reglas arquitectónicas para los módulos M26:
/// Cubo / CuboCampo / CuboFiltro (BI), PlanCuentaParametro (Contabilidad).
/// </summary>
public class M26_ArchitectureTests
{
    // ── Namespaces correctos ───────────────────────────────────────────────────

    [Fact]
    public void Cubo_DebeResidirEnNamespaceBI()
    {
        var result = Types
            .InAssembly(AssemblyReferences.DomainAssembly)
            .That()
            .HaveNameStartingWith("Cubo")
            .Should()
            .ResideInNamespaceContaining("BI")
            .GetResult();

        result.IsSuccessful.Should().BeTrue(
            because: "Las entidades Cubo deben estar en el namespace BI. " +
                     "Tipos fallidos: " + string.Join(", ", result.FailingTypeNames ?? []));
    }

    [Fact]
    public void PlanCuentaParametro_DebeResidirEnNamespaceContabilidad()
    {
        var result = Types
            .InAssembly(AssemblyReferences.DomainAssembly)
            .That()
            .HaveNameStartingWith("PlanCuentaParametro")
            .Should()
            .ResideInNamespaceContaining("Contabilidad")
            .GetResult();

        result.IsSuccessful.Should().BeTrue(
            because: "PlanCuentaParametro debe estar en el namespace Contabilidad. " +
                     "Tipos fallidos: " + string.Join(", ", result.FailingTypeNames ?? []));
    }

    // ── Encapsulación (sin setters públicos) ───────────────────────────────────

    [Theory]
    [InlineData("Cubo")]
    [InlineData("CuboCampo")]
    [InlineData("CuboFiltro")]
    [InlineData("PlanCuentaParametro")]
    public void EntidadesM26_NoDebenTenerSettersPublicos(string typeName)
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
    [InlineData("Cubo")]
    [InlineData("CuboCampo")]
    [InlineData("CuboFiltro")]
    [InlineData("PlanCuentaParametro")]
    public void EntidadesM26_NoDependenDeInfraestructura(string typeName)
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
    [InlineData("Cubo")]
    [InlineData("CuboCampo")]
    [InlineData("CuboFiltro")]
    [InlineData("PlanCuentaParametro")]
    public void EntidadesM26_DebenHeredarDeBaseEntity(string typeName)
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
