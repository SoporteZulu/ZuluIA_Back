using FluentAssertions;
using MediatR;
using System.Reflection;
using Xunit;
using ZuluIA_Back.Architecture.Tests.Helpers;

namespace ZuluIA_Back.Architecture.Tests;

/// <summary>
/// M32 – Reglas arquitectónicas avanzadas.
/// Complementa M31 con la dirección opuesta:
/// todo Command debe tener su CommandHandler, y todo Query su QueryHandler.
/// </summary>
public class M32_AdvancedArchitectureTests
{
    // ─────────────────────────────────────────────────────────────────────────
    // Cada Command debe tener su CommandHandler (dirección inversa a M31)
    // ─────────────────────────────────────────────────────────────────────────

    [Fact]
    public void CadaCommand_DebeTenerSuCommandHandler()
    {
        var allTypes = AssemblyReferences.ApplicationAssembly.GetTypes();

        var handlersDisponibles = allTypes
            .Where(t => t.IsClass
                     && !t.IsAbstract
                     && t.Name.EndsWith("CommandHandler")
                     && !t.Name.StartsWith("<"))
            .Select(t => t.Name[..^"Handler".Length]) // quita "Handler" → nombre del Command
            .ToHashSet(StringComparer.Ordinal);

        var commandsSinHandler = allTypes
            .Where(t => t.IsClass
                     && !t.IsAbstract
                     && t.Name.EndsWith("Command")
                     && !t.Name.StartsWith("<")
                     && t.GetInterfaces().Any(i =>
                         i.IsGenericType &&
                         i.GetGenericTypeDefinition() == typeof(IRequest<>)))
            .Select(t => t.Name)
            .Where(name => !handlersDisponibles.Contains(name))
            .ToList();

        commandsSinHandler.Should().BeEmpty(
            because: "Todo Command (que implementa IRequest<>) debe tener un " +
                     "CommandHandler con el mismo nombre base. " +
                     "Commands sin Handler: " + string.Join(", ", commandsSinHandler));
    }

    // ─────────────────────────────────────────────────────────────────────────
    // Cada Query debe tener su QueryHandler (dirección inversa a M31)
    // ─────────────────────────────────────────────────────────────────────────

    [Fact]
    public void CadaQuery_DebeTenerSuQueryHandler()
    {
        var allTypes = AssemblyReferences.ApplicationAssembly.GetTypes();

        var handlersDisponibles = allTypes
            .Where(t => t.IsClass
                     && !t.IsAbstract
                     && t.Name.EndsWith("QueryHandler")
                     && !t.Name.StartsWith("<"))
            .Select(t => t.Name[..^"Handler".Length])
            .ToHashSet(StringComparer.Ordinal);

        var queriesSinHandler = allTypes
            .Where(t => t.IsClass
                     && !t.IsAbstract
                     && t.Name.EndsWith("Query")
                     && !t.Name.StartsWith("<")
                     && t.GetInterfaces().Any(i =>
                         i.IsGenericType &&
                         i.GetGenericTypeDefinition() == typeof(IRequest<>)))
            .Select(t => t.Name)
            .Where(name => !handlersDisponibles.Contains(name))
            .ToList();

        queriesSinHandler.Should().BeEmpty(
            because: "Todo Query (que implementa IRequest<>) debe tener un " +
                     "QueryHandler con el mismo nombre base. " +
                     "Queries sin Handler: " + string.Join(", ", queriesSinHandler));
    }

    // ─────────────────────────────────────────────────────────────────────────
    // Todas las features tienen al menos un Handler (no hay features vacías)
    // ─────────────────────────────────────────────────────────────────────────

    [Fact]
    public void Assemblies_DebenTenerAlMenosUnHandler()
    {
        var handlerCount = AssemblyReferences.ApplicationAssembly
            .GetTypes()
            .Count(t => t.IsClass
                     && !t.IsAbstract
                     && (t.Name.EndsWith("CommandHandler") || t.Name.EndsWith("QueryHandler"))
                     && !t.Name.StartsWith("<"));

        handlerCount.Should().BeGreaterThan(0,
            because: "El ensamblado Application debe contener al menos un Handler registrado.");
    }

    // ─────────────────────────────────────────────────────────────────────────
    // Los Handlers no referencian directamente al ensamblado de API
    // ─────────────────────────────────────────────────────────────────────────

    [Fact]
    public void Handlers_NoDebenReferenciarApiAssembly()
    {
        var apiNamespace = AssemblyReferences.ApiNamespace;

        var handlersConDependenciaApi = AssemblyReferences.ApplicationAssembly
            .GetTypes()
            .Where(t => t.IsClass
                     && !t.IsAbstract
                     && (t.Name.EndsWith("CommandHandler") || t.Name.EndsWith("QueryHandler"))
                     && !t.Name.StartsWith("<"))
            .Where(t =>
            {
                var ctors = t.GetConstructors();
                return ctors.Any(c =>
                    c.GetParameters().Any(p =>
                        p.ParameterType.Namespace?.StartsWith(apiNamespace) == true));
            })
            .Select(t => t.Name)
            .ToList();

        handlersConDependenciaApi.Should().BeEmpty(
            because: "Los Handlers no deben depender directamente de tipos del API layer. " +
                     "Handlers con problema: " + string.Join(", ", handlersConDependenciaApi));
    }

    // ─────────────────────────────────────────────────────────────────────────
    // Las entidades de dominio no referencian servicios de infraestructura
    // ─────────────────────────────────────────────────────────────────────────

    [Fact]
    public void DomainEntities_NoReferenciamInfrastructureNamespace()
    {
        var infraNamespace = AssemblyReferences.InfrastructureNamespace;

        var entidadesConProblema = AssemblyReferences.DomainAssembly
            .GetTypes()
            .Where(t => t.Namespace?.Contains(".Entities") == true
                     && t.IsClass
                     && !t.IsAbstract)
            .Where(t =>
            {
                // Verificar constructores y métodos que referencian tipos de infra
                var allMembers = t.GetConstructors()
                    .SelectMany(c => c.GetParameters())
                    .Select(p => p.ParameterType.Namespace);

                return allMembers.Any(ns => ns?.StartsWith(infraNamespace) == true);
            })
            .Select(t => $"{t.Namespace}.{t.Name}")
            .ToList();

        entidadesConProblema.Should().BeEmpty(
            because: "Las entidades de dominio no deben tener dependencias de Infrastructure. " +
                     "Tipos con problema: " + string.Join(", ", entidadesConProblema));
    }

    // ─────────────────────────────────────────────────────────────────────────
    // No deben existir Commands ni Queries con el mismo nombre en distintos
    // namespaces (duplicados confusos que pueden registrar handlers de más).
    // ─────────────────────────────────────────────────────────────────────────
    [Fact]
    public void Commands_NoDebenTenerNombresDuplicados()
    {
        var allTypes = AssemblyReferences.ApplicationAssembly.GetTypes();

        var duplicados = allTypes
            .Where(t => t.IsClass
                     && !t.IsAbstract
                     && t.Name.EndsWith("Command")
                     && !t.Name.StartsWith("<")
                     && t.GetInterfaces().Any(i =>
                         i.IsGenericType &&
                         i.GetGenericTypeDefinition() == typeof(IRequest<>)))
            .GroupBy(t => t.Name)
            .Where(g => g.Count() > 1)
            .Select(g => $"{g.Key} → [{string.Join(", ", g.Select(t => t.Namespace))}]")
            .ToList();

        duplicados.Should().BeEmpty(
            because: "Dos Commands con el mismo nombre en distintos namespaces generan " +
                     "ambigüedad y registros duplicados en MediatR. Duplicados: " +
                     string.Join("; ", duplicados));
    }

    [Fact]
    public void Queries_NoDebenTenerNombresDuplicados()
    {
        var allTypes = AssemblyReferences.ApplicationAssembly.GetTypes();

        var duplicados = allTypes
            .Where(t => t.IsClass
                     && !t.IsAbstract
                     && t.Name.EndsWith("Query")
                     && !t.Name.StartsWith("<")
                     && t.GetInterfaces().Any(i =>
                         i.IsGenericType &&
                         i.GetGenericTypeDefinition() == typeof(IRequest<>)))
            .GroupBy(t => t.Name)
            .Where(g => g.Count() > 1)
            .Select(g => $"{g.Key} → [{string.Join(", ", g.Select(t => t.Namespace))}]")
            .ToList();

        duplicados.Should().BeEmpty(
            because: "Dos Queries con el mismo nombre en distintos namespaces generan " +
                     "ambigüedad. Duplicados: " + string.Join("; ", duplicados));
    }
}
