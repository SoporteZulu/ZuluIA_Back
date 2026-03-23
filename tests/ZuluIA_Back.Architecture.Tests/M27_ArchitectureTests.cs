using FluentAssertions;
using NetArchTest.Rules;
using System.Linq;
using Xunit;
using ZuluIA_Back.Architecture.Tests.Helpers;

namespace ZuluIA_Back.Architecture.Tests;

/// <summary>
/// Valida reglas arquitectónicas para los módulos M27:
/// MenuItem / MenuUsuario (Usuarios), UsuarioXUsuario (Usuarios).
/// </summary>
public class M27_ArchitectureTests
{
    // ── Namespaces correctos ───────────────────────────────────────────────────

    [Theory]
    [InlineData("MenuItem")]
    [InlineData("MenuUsuario")]
    [InlineData("UsuarioXUsuario")]
    public void EntidadesM27_DebenResidirEnNamespaceUsuarios(string typeName)
    {
        var result = Types
            .InAssembly(AssemblyReferences.DomainAssembly)
            .That()
            .HaveNameStartingWith(typeName)
            .Should()
            .ResideInNamespaceContaining("Usuarios")
            .GetResult();

        result.IsSuccessful.Should().BeTrue(
            because: $"{typeName} debe estar en el namespace Usuarios. " +
                     "Tipos fallidos: " + string.Join(", ", result.FailingTypeNames ?? []));
    }

    // ── Encapsulación (sin setters públicos) ───────────────────────────────────

    [Theory]
    [InlineData("MenuItem")]
    [InlineData("MenuUsuario")]
    [InlineData("UsuarioXUsuario")]
    public void EntidadesM27_NoDebenTenerSettersPublicos(string typeName)
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
    [InlineData("MenuItem")]
    [InlineData("MenuUsuario")]
    [InlineData("UsuarioXUsuario")]
    public void EntidadesM27_NoDependenDeInfraestructura(string typeName)
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
    [InlineData("MenuItem")]
    [InlineData("MenuUsuario")]
    [InlineData("UsuarioXUsuario")]
    public void EntidadesM27_DebenHeredarDeBaseEntity(string typeName)
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
