using FluentAssertions;
using NetArchTest.Rules;
using Xunit;
using ZuluIA_Back.Architecture.Tests.Helpers;

namespace ZuluIA_Back.Architecture.Tests;

/// <summary>
/// Valida reglas arquitectónicas para el módulo M22:
/// Timbrado (Paraguay/SET) y el motor de Impuestos/Percepciones
/// (IMP_IMPUESTO, IMP_IMPUESTOXPERSONA, IMP_IMPUESTOXITEM).
/// </summary>
public class M22_ArchitectureTests
{
    // ── Namespaces correctos ──────────────────────────────────────────────────

    [Fact]
    public void Timbrado_DebeResidirEnNamespaceFacturacion()
    {
        var result = Types
            .InAssembly(AssemblyReferences.DomainAssembly)
            .That()
            .HaveNameStartingWith("Timbrado")
            .Should()
            .ResideInNamespaceContaining("Facturacion")
            .GetResult();

        result.IsSuccessful.Should().BeTrue(
            because: "Timbrado debe estar en el namespace Facturacion. " +
                     "Tipos fallidos: " + string.Join(", ", result.FailingTypeNames ?? []));
    }

    [Fact]
    public void EntidadesImpuestos_DebenResidirEnNamespaceImpuestos()
    {
        var result = Types
            .InAssembly(AssemblyReferences.DomainAssembly)
            .That()
            .HaveNameStartingWith("Impuesto")
            .Should()
            .ResideInNamespaceContaining("Impuestos")
            .GetResult();

        result.IsSuccessful.Should().BeTrue(
            because: "Las entidades de Impuestos deben estar en el namespace Impuestos. " +
                     "Tipos fallidos: " + string.Join(", ", result.FailingTypeNames ?? []));
    }

    // ── Herencia de BaseEntity ────────────────────────────────────────────────

    [Fact]
    public void Timbrado_DebeHeredarDeBaseEntity()
    {
        var baseEntityType = typeof(ZuluIA_Back.Domain.Common.BaseEntity);

        var result = Types
            .InAssembly(AssemblyReferences.DomainAssembly)
            .That()
            .HaveNameStartingWith("Timbrado")
            .And()
            .AreClasses()
            .Should()
            .Inherit(baseEntityType)
            .GetResult();

        result.IsSuccessful.Should().BeTrue(
            because: "Timbrado debe heredar de BaseEntity. " +
                     "Tipos fallidos: " + string.Join(", ", result.FailingTypeNames ?? []));
    }

    [Fact]
    public void EntidadesImpuestos_DebenHeredarDeBaseEntity()
    {
        var baseEntityType = typeof(ZuluIA_Back.Domain.Common.BaseEntity);

        var result = Types
            .InAssembly(AssemblyReferences.DomainAssembly)
            .That()
            .HaveNameStartingWith("Impuesto")
            .And()
            .AreClasses()
            .Should()
            .Inherit(baseEntityType)
            .GetResult();

        result.IsSuccessful.Should().BeTrue(
            because: "Las entidades Impuesto* deben heredar de BaseEntity. " +
                     "Tipos fallidos: " + string.Join(", ", result.FailingTypeNames ?? []));
    }

    // ── Constructores privados (encapsulación) ────────────────────────────────

    [Fact]
    public void EntidadesM22_DebenTenerConstructorPrivado()
    {
        var entityNames = new[]
        {
            "Timbrado",
            "Impuesto",
            "ImpuestoPorPersona",
            "ImpuestoPorItem"
        };

        var entityTypes = AssemblyReferences.DomainAssembly
            .GetTypes()
            .Where(t => t.IsClass && !t.IsAbstract && entityNames.Contains(t.Name))
            .ToList();

        entityTypes.Should().HaveCount(4,
            because: "Deben existir exactamente 4 entidades M22: Timbrado, Impuesto, ImpuestoPorPersona, ImpuestoPorItem.");

        foreach (var type in entityTypes)
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
                because: $"{type.Name} no debe tener constructor público — debe usarse Crear().");
        }
    }

    // ── Métodos de fábrica estáticos ──────────────────────────────────────────

    [Fact]
    public void EntidadesM22_DebenTenerMetodoCrear()
    {
        var entityNames = new[]
        {
            "Timbrado",
            "Impuesto",
            "ImpuestoPorPersona",
            "ImpuestoPorItem"
        };

        var entityTypes = AssemblyReferences.DomainAssembly
            .GetTypes()
            .Where(t => t.IsClass && !t.IsAbstract && entityNames.Contains(t.Name))
            .ToList();

        foreach (var type in entityTypes)
        {
            var tieneCrear = type
                .GetMethods(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static)
                .Any(m => m.Name == "Crear");

            tieneCrear.Should().BeTrue(
                because: $"{type.Name} debe exponer un método estático público Crear() como fábrica.");
        }
    }

    // ── DbSets registrados en IApplicationDbContext ───────────────────────────

    [Fact]
    public void IApplicationDbContext_DebeExponerDbSetsTimbradoEImpuestos()
    {
        var contextType = AssemblyReferences.ApplicationAssembly
            .GetTypes()
            .FirstOrDefault(t => t.Name == "IApplicationDbContext");

        contextType.Should().NotBeNull(because: "IApplicationDbContext debe existir en Application.");

        var propNames = contextType!
            .GetProperties()
            .Select(p => p.Name)
            .ToHashSet();

        propNames.Should().Contain("Timbrados",           because: "IApplicationDbContext debe exponer DbSet<Timbrado>.");
        propNames.Should().Contain("Impuestos",           because: "IApplicationDbContext debe exponer DbSet<Impuesto>.");
        propNames.Should().Contain("ImpuestosPorPersona", because: "IApplicationDbContext debe exponer DbSet<ImpuestoPorPersona>.");
        propNames.Should().Contain("ImpuestosPorItem",    because: "IApplicationDbContext debe exponer DbSet<ImpuestoPorItem>.");
    }
}
