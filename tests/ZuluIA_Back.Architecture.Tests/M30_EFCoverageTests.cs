using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using NetArchTest.Rules;
using Xunit;
using ZuluIA_Back.Architecture.Tests.Helpers;
using ZuluIA_Back.Domain.Entities.Finanzas;
using ZuluIA_Back.Domain.Entities.Geografia;
using ZuluIA_Back.Domain.Entities.Logistica;
using ZuluIA_Back.Domain.Entities.Ventas;
using ZuluIA_Back.Infrastructure.Persistence;

namespace ZuluIA_Back.Architecture.Tests;

/// <summary>
/// M30 — Verifies EF configuration coverage for all domain entities.
/// Every entity registered as a DbSet in AppDbContext must have an
/// IEntityTypeConfiguration&lt;T&gt; implementation in the Infrastructure assembly.
/// Also validates namespace placement for entities configured in this milestone.
/// </summary>
public class M30_EFCoverageTests
{
    // ── DbSet → IEntityTypeConfiguration coverage ────────────────────────────

    [Fact]
    public void TodosLosDbSets_DebenTenerEFConfiguration()
    {
        // Collect all entity types registered as DbSet<T> in AppDbContext
        var dbSetEntityTypes = typeof(AppDbContext)
            .GetProperties()
            .Where(p => p.PropertyType.IsGenericType
                     && p.PropertyType.GetGenericTypeDefinition() == typeof(DbSet<>))
            .Select(p => p.PropertyType.GetGenericArguments()[0])
            .ToHashSet();

        // Collect all entity types that have IEntityTypeConfiguration<T> in Infrastructure
        var configuredTypes = AssemblyReferences.InfrastructureAssembly
            .GetTypes()
            .Where(t => t.IsClass && !t.IsAbstract)
            .SelectMany(t => t.GetInterfaces()
                .Where(i => i.IsGenericType
                         && i.GetGenericTypeDefinition() == typeof(Microsoft.EntityFrameworkCore.IEntityTypeConfiguration<>))
                .Select(i => i.GetGenericArguments()[0]))
            .ToHashSet();

        var sinConfiguracion = dbSetEntityTypes
            .Where(e => !configuredTypes.Contains(e))
            .Select(e => e.Name)
            .OrderBy(n => n)
            .ToList();

        sinConfiguracion.Should().BeEmpty(
            because: "Cada DbSet<T> en AppDbContext debe tener un IEntityTypeConfiguration<T> " +
                     "en Infrastructure. Sin configuración: " + string.Join(", ", sinConfiguracion));
    }

    // ── Namespace placement for newly configured entities ────────────────────

    [Fact]
    public void Barrio_DebeResidirEnNamespaceGeografia()
    {
        var result = Types
            .InAssembly(AssemblyReferences.DomainAssembly)
            .That()
            .HaveNameMatching("^Barrio$")
            .Should()
            .ResideInNamespaceContaining("Geografia")
            .GetResult();

        result.IsSuccessful.Should().BeTrue(
            because: "Barrio debe estar en el namespace Geografia. " +
                     "Tipos fallidos: " + string.Join(", ", result.FailingTypeNames ?? []));
    }

    [Fact]
    public void EscalaRetencion_DebeResidirEnNamespaceFinanzas()
    {
        var result = Types
            .InAssembly(AssemblyReferences.DomainAssembly)
            .That()
            .HaveNameMatching("^EscalaRetencion$")
            .Should()
            .ResideInNamespaceContaining("Finanzas")
            .GetResult();

        result.IsSuccessful.Should().BeTrue(
            because: "EscalaRetencion debe estar en el namespace Finanzas. " +
                     "Tipos fallidos: " + string.Join(", ", result.FailingTypeNames ?? []));
    }

    [Fact]
    public void TipoRetencion_DebeResidirEnNamespaceFinanzas()
    {
        var result = Types
            .InAssembly(AssemblyReferences.DomainAssembly)
            .That()
            .HaveNameMatching("^TipoRetencion$")
            .Should()
            .ResideInNamespaceContaining("Finanzas")
            .GetResult();

        result.IsSuccessful.Should().BeTrue(
            because: "TipoRetencion debe estar en el namespace Finanzas. " +
                     "Tipos fallidos: " + string.Join(", ", result.FailingTypeNames ?? []));
    }

    // ── Constructor encapsulation for newly configured entities ─────────────

    [Fact]
    public void EntidadesNuevamenteConfiguradas_DebenTenerConstructorPrivado()
    {
        var entityTypes = new[]
        {
            typeof(Barrio),
            typeof(DescuentoComercial),
            typeof(EscalaRetencion),
            typeof(OrdenPreparacion),
            typeof(OrdenPreparacionDetalle),
            typeof(RetencionXPersona),
            typeof(TipoRetencion)
        };

        foreach (var type in entityTypes)
        {
            var tieneConstructorPrivadoOProtegido = type
                .GetConstructors(System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                .Any();

            tieneConstructorPrivadoOProtegido.Should().BeTrue(
                because: $"{type.Name} debe tener constructor privado/protegido para uso por EF Core.");

            var tieneConstructorPublico = type
                .GetConstructors(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance)
                .Any();

            tieneConstructorPublico.Should().BeFalse(
                because: $"{type.Name} no debe exponer constructor público — usar fábricas estáticas.");
        }
    }

    // ── EF Configuration classes are in the correct namespace ───────────────

    [Fact]
    public void NuevasConfigurations_DebenResidirEnCarpetaConfigurations()
    {
        var configNames = new[]
        {
            "BarrioConfiguration",
            "DescuentoComercialConfiguration",
            "EscalaRetencionConfiguration",
            "OrdenPreparacionConfiguration",
            "OrdenPreparacionDetalleConfiguration",
            "RetencionXPersonaConfiguration",
            "TipoRetencionConfiguration"
        };

        var assemblyTypes = AssemblyReferences.InfrastructureAssembly
            .GetTypes()
            .Where(t => !t.Name.StartsWith('<'))   // exclude compiler-generated types
            .GroupBy(t => t.Name)
            .ToDictionary(g => g.Key, g => g.First());

        foreach (var configName in configNames)
        {
            assemblyTypes.Should().ContainKey(configName,
                because: $"{configName} debe existir en el Infrastructure assembly.");

            var type = assemblyTypes[configName];
            type.Namespace.Should().Contain("Configurations",
                because: $"{configName} debe residir en el namespace Configurations.");
        }
    }
}
