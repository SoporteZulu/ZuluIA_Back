using FluentAssertions;
using NetArchTest.Rules;
using Xunit;
using ZuluIA_Back.Architecture.Tests.Helpers;

namespace ZuluIA_Back.Architecture.Tests;

/// <summary>
/// Valida reglas arquitectónicas para los módulos nuevos añadidos:
/// OrdenPreparacion (Logística), TipoRetencion / TransferenciaCaja (Finanzas),
/// DescuentoComercial (Ventas) y RetencionXPersona (Finanzas).
/// </summary>
public class NuevosModulosArchitectureTests
{
    // ── Namespaces correctos ─────────────────────────────────────────────────

    [Fact]
    public void OrdenPreparacion_DebeResidirEnNamespaceLogistica()
    {
        var result = Types
            .InAssembly(AssemblyReferences.DomainAssembly)
            .That()
            .HaveNameStartingWith("OrdenPreparacion")
            .Should()
            .ResideInNamespaceContaining("Logistica")
            .GetResult();

        result.IsSuccessful.Should().BeTrue(
            because: "OrdenPreparacion debe estar en el namespace Logistica. " +
                     "Tipos fallidos: " + string.Join(", ", result.FailingTypeNames ?? []));
    }

    [Fact]
    public void DescuentoComercial_DebeResidirEnNamespaceVentas()
    {
        var result = Types
            .InAssembly(AssemblyReferences.DomainAssembly)
            .That()
            .HaveNameStartingWith("DescuentoComercial")
            .Should()
            .ResideInNamespaceContaining("Ventas")
            .GetResult();

        result.IsSuccessful.Should().BeTrue(
            because: "DescuentoComercial debe estar en el namespace Ventas. " +
                     "Tipos fallidos: " + string.Join(", ", result.FailingTypeNames ?? []));
    }

    [Fact]
    public void TipoRetencion_DebeResidirEnNamespaceFinanzas()
    {
        var result = Types
            .InAssembly(AssemblyReferences.DomainAssembly)
            .That()
            .HaveNameStartingWith("TipoRetencion")
            .Or()
            .HaveNameStartingWith("EscalaRetencion")
            .Or()
            .HaveNameStartingWith("TransferenciaCaja")
            .Or()
            .HaveNameStartingWith("RetencionXPersona")
            .Should()
            .ResideInNamespaceContaining("Finanzas")
            .GetResult();

        result.IsSuccessful.Should().BeTrue(
            because: "Las entidades Finanzas nuevas deben estar en el namespace Finanzas. " +
                     "Tipos fallidos: " + string.Join(", ", result.FailingTypeNames ?? []));
    }

    // ── Constructores privados (encapsulación) ───────────────────────────────

    [Fact]
    public void EntidadesFinanzasNuevas_DebenTenerConstructorPrivado()
    {
        var entityTypes = AssemblyReferences.DomainAssembly
            .GetTypes()
            .Where(t => t.IsClass
                     && !t.IsAbstract
                     && (t.Name == "TipoRetencion"
                      || t.Name == "EscalaRetencion"
                      || t.Name == "TransferenciaCaja"
                      || t.Name == "RetencionXPersona"))
            .ToList();

        entityTypes.Should().NotBeEmpty(because: "las entidades de Finanzas nuevas deben existir.");

        foreach (var type in entityTypes)
        {
            var tieneConstructorPrivadoOProtegido = type
                .GetConstructors(System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                .Any();

            tieneConstructorPrivadoOProtegido.Should().BeTrue(
                because: $"{type.Name} debe tener un constructor privado/protegido para forzar el uso de fábrica estática.");

            var tieneConstructorPublico = type
                .GetConstructors(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance)
                .Any();

            tieneConstructorPublico.Should().BeFalse(
                because: $"{type.Name} no debe tener constructor público — debe usarse la fábrica estática.");
        }
    }

    [Fact]
    public void OrdenPreparacion_DebeHeredarDeBaseEntity()
    {
        var baseEntityType = typeof(ZuluIA_Back.Domain.Common.BaseEntity);

        var result = Types
            .InAssembly(AssemblyReferences.DomainAssembly)
            .That()
            .HaveNameStartingWith("OrdenPreparacion")
            .And()
            .AreClasses()
            .Should()
            .Inherit(baseEntityType)
            .GetResult();

        result.IsSuccessful.Should().BeTrue(
            because: "OrdenPreparacion debe heredar de BaseEntity. " +
                     "Tipos fallidos: " + string.Join(", ", result.FailingTypeNames ?? []));
    }

    [Fact]
    public void DescuentoComercial_DebeHeredarDeBaseEntity()
    {
        var baseEntityType = typeof(ZuluIA_Back.Domain.Common.BaseEntity);

        var result = Types
            .InAssembly(AssemblyReferences.DomainAssembly)
            .That()
            .HaveNameStartingWith("DescuentoComercial")
            .And()
            .AreClasses()
            .Should()
            .Inherit(baseEntityType)
            .GetResult();

        result.IsSuccessful.Should().BeTrue(
            because: "DescuentoComercial debe heredar de BaseEntity. " +
                     "Tipos fallidos: " + string.Join(", ", result.FailingTypeNames ?? []));
    }

    // ── Handlers en namespace correcto ───────────────────────────────────────

    [Fact]
    public void HandlersNuevosModulos_DebenResidirEnApplicationAssembly()
    {
        var handlerNames = new[]
        {
            "CreateOrdenPreparacionCommandHandler",
            "ConfirmarOrdenPreparacionCommandHandler",
            "AnularOrdenPreparacionCommandHandler",
            "CreateTipoRetencionCommandHandler",
            "RegistrarTransferenciaCommandHandler",
            "CreateDescuentoComercialCommandHandler",
            "ConvertirPresupuestoCommandHandler",
            "ImputarComprobantesMasivosCommandHandler"
        };

        var assemblyTypes = AssemblyReferences.ApplicationAssembly.GetTypes()
            .Select(t => t.Name)
            .ToHashSet();

        foreach (var handlerName in handlerNames)
        {
            assemblyTypes.Should().Contain(handlerName,
                because: $"{handlerName} debe estar registrado en el Application assembly.");
        }
    }

    // ── Commands/Queries implementan IRequest<> ──────────────────────────────

    [Fact]
    public void NuevosCommands_DebenImplementarIRequest()
    {
        var commandNames = new[]
        {
            "CreateOrdenPreparacionCommand",
            "ConfirmarOrdenPreparacionCommand",
            "AnularOrdenPreparacionCommand",
            "CreateTipoRetencionCommand",
            "RegistrarTransferenciaCommand",
            "CreateDescuentoComercialCommand",
            "ConvertirPresupuestoCommand",
            "ImputarComprobantesMasivosCommand"
        };

        var assemblyTypesByName = AssemblyReferences.ApplicationAssembly.GetTypes()
            .GroupBy(t => t.Name)
            .ToDictionary(g => g.Key, g => g.First());

        foreach (var commandName in commandNames)
        {
            assemblyTypesByName.Should().ContainKey(commandName,
                because: $"{commandName} debe existir en el Application assembly.");

            var type = assemblyTypesByName[commandName];
            var implementsIRequest = type.GetInterfaces()
                .Any(i => i.Namespace == "MediatR" && i.Name.StartsWith("IRequest"));

            implementsIRequest.Should().BeTrue(
                because: $"{commandName} debe implementar IRequest<> de MediatR.");
        }
    }
}
