using FluentAssertions;
using NetArchTest.Rules;
using Xunit;
using ZuluIA_Back.Architecture.Tests.Helpers;

namespace ZuluIA_Back.Architecture.Tests;

/// <summary>
/// Verifica reglas arquitectónicas para los módulos M20 y M21:
/// PeriodoContable (Contabilidad) y Atributo/AtributoItem (Items).
/// También valida reglas generales de cobertura de entidades.
/// </summary>
public class M20_M21_ArchitectureTests
{
    // ── Namespace correcto ───────────────────────────────────────────────────

    [Fact]
    public void PeriodoContable_DebeResidirEnNamespaceContabilidad()
    {
        var result = Types
            .InAssembly(AssemblyReferences.DomainAssembly)
            .That()
            .HaveNameStartingWith("PeriodoContable")
            .Should()
            .ResideInNamespaceContaining("Contabilidad")
            .GetResult();

        result.IsSuccessful.Should().BeTrue(
            because: "PeriodoContable debe estar en el namespace Contabilidad. " +
                     "Tipos fallidos: " + string.Join(", ", result.FailingTypeNames ?? []));
    }

    [Fact]
    public void Atributo_DebeResidirEnNamespaceItems()
    {
        var result = Types
            .InAssembly(AssemblyReferences.DomainAssembly)
            .That()
            .HaveNameStartingWith("Atributo")
            .Should()
            .ResideInNamespaceContaining("Items")
            .GetResult();

        result.IsSuccessful.Should().BeTrue(
            because: "Atributo y AtributoItem deben estar en el namespace Items. " +
                     "Tipos fallidos: " + string.Join(", ", result.FailingTypeNames ?? []));
    }

    [Fact]
    public void UnidadMedida_DebeResidirEnNamespaceReferencia()
    {
        var result = Types
            .InAssembly(AssemblyReferences.DomainAssembly)
            .That()
            .HaveNameStartingWith("UnidadMedida")
            .Should()
            .ResideInNamespaceContaining("Referencia")
            .GetResult();

        result.IsSuccessful.Should().BeTrue(
            because: "UnidadMedida debe estar en el namespace Referencia. " +
                     "Tipos fallidos: " + string.Join(", ", result.FailingTypeNames ?? []));
    }

    // ── Constructores privados ───────────────────────────────────────────────

    [Fact]
    public void EntidadesM20_M21_DebenTenerConstructorPrivado()
    {
        var tiposAVerificar = new[] { "PeriodoContable", "Atributo", "AtributoItem" };

        var entidadesConConstructorPublico = AssemblyReferences.DomainAssembly
            .GetTypes()
            .Where(t => tiposAVerificar.Contains(t.Name))
            .Where(t => t.GetConstructors().Any(c => c.IsPublic))
            .Select(t => t.Name)
            .ToList();

        entidadesConConstructorPublico.Should().BeEmpty(
            because: "Las entidades M20/M21 deben tener constructores privados. " +
                     "Entidades con constructor público: " +
                     string.Join(", ", entidadesConConstructorPublico));
    }

    // ── Métodos de fábrica estáticos ────────────────────────────────────────

    [Fact]
    public void EntidadesM20_M21_DebenTenerMetodoCrear()
    {
        var tiposAVerificar = new[] { "PeriodoContable", "Atributo", "AtributoItem", "UnidadMedida", "CategoriaTercero" };

        var entidadesSinCrear = AssemblyReferences.DomainAssembly
            .GetTypes()
            .Where(t => tiposAVerificar.Contains(t.Name))
            .Where(t =>
            {
                var metodoCrear = t.GetMethods(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static)
                    .FirstOrDefault(m => m.Name == "Crear");
                return metodoCrear is null;
            })
            .Select(t => t.Name)
            .ToList();

        entidadesSinCrear.Should().BeEmpty(
            because: "Las entidades de dominio nuevas deben exponer un método de fábrica estático 'Crear'. " +
                     "Entidades sin método Crear: " +
                     string.Join(", ", entidadesSinCrear));
    }

    // ── Estructura general de nuevas entidades ────────────────────────────

    [Fact]
    public void NuevasEntidades_DebenHeredarDeBaseEntity()
    {
        var tiposAVerificar = new[] { "PeriodoContable", "Atributo", "AtributoItem" };

        var entidadesSinBaseEntity = AssemblyReferences.DomainAssembly
            .GetTypes()
            .Where(t => tiposAVerificar.Contains(t.Name))
            .Where(t => !t.IsSubclassOf(typeof(ZuluIA_Back.Domain.Common.BaseEntity)))
            .Select(t => t.Name)
            .ToList();

        entidadesSinBaseEntity.Should().BeEmpty(
            because: "Todas las nuevas entidades deben heredar de BaseEntity. " +
                     "Tipos fallidos: " + string.Join(", ", entidadesSinBaseEntity));
    }

    // ── Nuevos controllers en namespace correcto ─────────────────────────────

    [Fact]
    public void NuevosControllers_M20_M21_DebenHeredarDeBaseController()
    {
        var controllersNuevos = new[]
        {
            "PeriodosContablesController", "AtributosController", "UnidadesMedidaController"
        };

        var controllersSinBase = AssemblyReferences.ApiAssembly
            .GetTypes()
            .Where(t => controllersNuevos.Contains(t.Name))
            .Where(t => !t.IsSubclassOf(typeof(ZuluIA_Back.Api.Controllers.BaseController)))
            .Select(t => t.Name)
            .ToList();

        controllersSinBase.Should().BeEmpty(
            because: "Los nuevos controllers deben heredar de BaseController. " +
                     "Controllers sin BaseController: " +
                     string.Join(", ", controllersSinBase));
    }
}
