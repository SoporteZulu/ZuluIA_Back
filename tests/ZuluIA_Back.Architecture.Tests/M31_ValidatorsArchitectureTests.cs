using FluentAssertions;
using FluentValidation;
using MediatR;
using System.Reflection;
using Xunit;
using ZuluIA_Back.Architecture.Tests.Helpers;

namespace ZuluIA_Back.Architecture.Tests;

/// <summary>
/// M31 — Arquitectura de Validadores y Servicios de Dominio.
/// Garantiza que todos los validadores heredan correctamente de AbstractValidator{T},
/// que todos los CommandHandlers tienen un Command correspondiente,
/// y que los domain services residen en el namespace correcto.
/// </summary>
public class M31_ValidatorsArchitectureTests
{
    // ─────────────────────────────────────────────────────────────────────────
    // Validadores deben heredar de AbstractValidator<T>
    // ─────────────────────────────────────────────────────────────────────────

    [Fact]
    public void TodosLosValidadores_DebenHeredarDeAbstractValidator()
    {
        var tiposConProblemas = AssemblyReferences.ApplicationAssembly
            .GetTypes()
            .Where(t => t.IsClass
                     && !t.IsAbstract
                     && t.Name.EndsWith("Validator")
                     && !t.Name.StartsWith("<"))
            .Where(t =>
            {
                // Verificar que la jerarquía de herencia incluye AbstractValidator<>
                var tipo = t.BaseType;
                while (tipo != null)
                {
                    if (tipo.IsGenericType &&
                        tipo.GetGenericTypeDefinition() == typeof(AbstractValidator<>))
                        return false; // buen caso: SI hereda
                    tipo = tipo.BaseType;
                }
                return true; // mal caso: NO hereda de AbstractValidator<>
            })
            .Select(t => t.FullName!)
            .ToList();

        tiposConProblemas.Should().BeEmpty(
            because: "Todos los Validators deben heredar de AbstractValidator<T>. " +
                     "Tipos con problema: " + string.Join(", ", tiposConProblemas));
    }

    // ─────────────────────────────────────────────────────────────────────────
    // Cada CommandHandler debe tener un Command correspondiente
    // ─────────────────────────────────────────────────────────────────────────

    [Fact]
    public void CadaCommandHandler_DebeTenerSuCommandCorrespondiente()
    {
        var comandosDisponibles = AssemblyReferences.ApplicationAssembly
            .GetTypes()
            .Where(t => t.IsClass && t.Name.EndsWith("Command") && !t.Name.StartsWith("<"))
            .Select(t => t.Name)
            .ToHashSet();

        var handlersHuerfanos = AssemblyReferences.ApplicationAssembly
            .GetTypes()
            .Where(t => t.IsClass
                     && !t.IsAbstract
                     && t.Name.EndsWith("CommandHandler")
                     && !t.Name.StartsWith("<"))
            .Select(t => t.Name)
            .Where(handlerName =>
            {
                var commandName = handlerName[..^"Handler".Length]; // quitar "Handler"
                return !comandosDisponibles.Contains(commandName);
            })
            .ToList();

        handlersHuerfanos.Should().BeEmpty(
            because: "Cada CommandHandler debe tener un Command con el mismo nombre base. " +
                     "Handlers sin Command: " + string.Join(", ", handlersHuerfanos));
    }

    // ─────────────────────────────────────────────────────────────────────────
    // Cada QueryHandler debe tener un Query correspondiente
    // ─────────────────────────────────────────────────────────────────────────

    [Fact]
    public void CadaQueryHandler_DebeTenerSuQueryCorrespondiente()
    {
        var queriesDisponibles = AssemblyReferences.ApplicationAssembly
            .GetTypes()
            .Where(t => t.IsClass && t.Name.EndsWith("Query") && !t.Name.StartsWith("<"))
            .Select(t => t.Name)
            .ToHashSet();

        var handlersHuerfanos = AssemblyReferences.ApplicationAssembly
            .GetTypes()
            .Where(t => t.IsClass
                     && !t.IsAbstract
                     && t.Name.EndsWith("QueryHandler")
                     && !t.Name.StartsWith("<"))
            .Select(t => t.Name)
            .Where(handlerName =>
            {
                var queryName = handlerName[..^"Handler".Length];
                return !queriesDisponibles.Contains(queryName);
            })
            .ToList();

        handlersHuerfanos.Should().BeEmpty(
            because: "Cada QueryHandler debe tener un Query con el mismo nombre base. " +
                     "Handlers sin Query: " + string.Join(", ", handlersHuerfanos));
    }

    // ─────────────────────────────────────────────────────────────────────────
    // Los domain services deben residir en Domain.Services namespace
    // ─────────────────────────────────────────────────────────────────────────

    [Fact]
    public void DomainServices_DebenResidirEnNamespaceServices()
    {
        // Servicios de dominio conocidos: terminan en "Service" dentro del dominio
        var servicesConProblemas = AssemblyReferences.DomainAssembly
            .GetTypes()
            .Where(t => t.IsClass
                     && !t.IsAbstract
                     && t.Name.EndsWith("Service")
                     && !t.Name.StartsWith("<"))
            .Where(t => !(t.Namespace?.Contains("Services") == true))
            .Select(t => $"{t.Namespace}.{t.Name}")
            .ToList();

        servicesConProblemas.Should().BeEmpty(
            because: "Los domain services deben residir en el namespace *Services*. " +
                     "Tipos con problema: " + string.Join(", ", servicesConProblemas));
    }

    // ─────────────────────────────────────────────────────────────────────────
    // Los Behaviors del pipeline deben existir y estar en Common.Behaviors
    // ─────────────────────────────────────────────────────────────────────────

    [Fact]
    public void PipelineBehaviors_DebenResidirEnCommonBehaviors()
    {
        var behaviorsConProblemas = AssemblyReferences.ApplicationAssembly
            .GetTypes()
            .Where(t => t.IsClass
                     && !t.IsAbstract
                     && t.Name.EndsWith("Behavior")
                     && !t.Name.StartsWith("<"))
            .Where(t => !(t.Namespace?.Contains("Behaviors") == true))
            .Select(t => t.FullName!)
            .ToList();

        behaviorsConProblemas.Should().BeEmpty(
            because: "Los pipeline behaviors deben residir en Application.Common.Behaviors. " +
                     "Tipos con problema: " + string.Join(", ", behaviorsConProblemas));
    }

    // ─────────────────────────────────────────────────────────────────────────
    // Los pipeline behaviors deben implementar IPipelineBehavior<,>
    // ─────────────────────────────────────────────────────────────────────────

    [Fact]
    public void PipelineBehaviors_DebenImplementarIPipelineBehavior()
    {
        var pipelineBehaviorInterface = typeof(MediatR.IPipelineBehavior<,>);

        var behaviorsConProblemas = AssemblyReferences.ApplicationAssembly
            .GetTypes()
            .Where(t => t.IsClass
                     && !t.IsAbstract
                     && t.Name.EndsWith("Behavior")
                     && !t.Name.StartsWith("<"))
            .Where(t => !t.GetInterfaces()
                          .Any(i => i.IsGenericType &&
                                    i.GetGenericTypeDefinition() == pipelineBehaviorInterface))
            .Select(t => t.FullName!)
            .ToList();

        behaviorsConProblemas.Should().BeEmpty(
            because: "Todos los pipeline behaviors deben implementar IPipelineBehavior<TRequest, TResponse>. " +
                     "Tipos con problema: " + string.Join(", ", behaviorsConProblemas));
    }

    // ─────────────────────────────────────────────────────────────────────────
    // DTOs no deben tener lógica de negocio (sin métodos propios más allá de
    // los generados por record/class)
    // ─────────────────────────────────────────────────────────────────────────

    [Fact]
    public void DTOs_NoDebenTenerMetodosDeNegocio()
    {
        var dtosConMetodos = AssemblyReferences.ApplicationAssembly
            .GetTypes()
            .Where(t => t.IsClass
                     && !t.IsAbstract
                     && t.Name.EndsWith("Dto")
                     && !t.Name.StartsWith("<"))
            .Where(t =>
            {
                // Métodos declarados en el tipo mismo (no heredados), excluyendo
                // los métodos generados por el compilador para records
                var metodos = t.GetMethods(BindingFlags.Public | BindingFlags.DeclaredOnly)
                    .Where(m => !m.IsSpecialName           // excluir get_/set_
                             && m.Name != "Deconstruct"
                             && m.Name != "ToString"
                             && m.Name != "GetHashCode"
                             && m.Name != "Equals"
                             && m.Name != "<Clone>$"
                             && !m.Name.StartsWith("<"))
                    .ToList();
                return metodos.Count > 0;
            })
            .Select(t => t.FullName!)
            .ToList();

        dtosConMetodos.Should().BeEmpty(
            because: "Los DTOs no deben tener métodos de negocio. Deben ser simples contenedores de datos. " +
                     "DTOs con métodos: " + string.Join(", ", dtosConMetodos));
    }

    // ─────────────────────────────────────────────────────────────────────────
    // Interfaces en Domain deben empezar con I (ya cubierto en NamingConventionTests,
    // pero aquí verificamos específicamente las del namespace Domain.Interfaces)
    // ─────────────────────────────────────────────────────────────────────────

    [Fact]
    public void TodosLosRepositoriosEnDomain_DebenSerInterfaces()
    {
        var repositoriosDominio = AssemblyReferences.DomainAssembly
            .GetTypes()
            .Where(t => t.Name.EndsWith("Repository")
                     && !t.Name.StartsWith("<"))
            .ToList();

        var concretosConProblemas = repositoriosDominio
            .Where(t => t.IsClass && !t.IsAbstract)
            .Select(t => t.FullName!)
            .ToList();

        concretosConProblemas.Should().BeEmpty(
            because: "El Domain no debe contener clases concretas de repositorios. " +
                     "Solo interfaces. Tipos con problema: " + string.Join(", ", concretosConProblemas));
    }

    // ─────────────────────────────────────────────────────────────────────────
    // Create/Update/Registrar commands deben tener un validator correspondiente
    // ─────────────────────────────────────────────────────────────────────────

    [Fact]
    public void CommandsDeCreacionYActualizacion_DebenTenerValidator()
    {
        var validatedCommands = AssemblyReferences.ApplicationAssembly
            .GetTypes()
            .Where(t => t.IsClass
                     && !t.IsAbstract
                     && t.Name.EndsWith("Validator")
                     && !t.Name.StartsWith("<"))
            .Select(t =>
            {
                // AbstractValidator<TCommand> → get the TCommand name
                var baseType = t.BaseType;
                while (baseType != null)
                {
                    if (baseType.IsGenericType &&
                        baseType.GetGenericTypeDefinition() == typeof(AbstractValidator<>))
                        return baseType.GetGenericArguments()[0].Name;
                    baseType = baseType.BaseType;
                }
                return null;
            })
            .Where(name => name != null)
            .ToHashSet();

        var commandsWithoutValidator = AssemblyReferences.ApplicationAssembly
            .GetTypes()
            .Where(t => t.IsClass
                     && !t.IsAbstract
                     && !t.Name.StartsWith("<")
                     && (t.Name.StartsWith("Create") || t.Name.StartsWith("Update") || t.Name.StartsWith("Registrar"))
                     && t.Name.EndsWith("Command"))
            .Select(t => t.Name)
            .Where(name => !validatedCommands.Contains(name))
            .OrderBy(n => n)
            .ToList();

        commandsWithoutValidator.Should().BeEmpty(
            because: "Todos los commands de creación/actualización deben tener un Validator. " +
                     "Commands sin validator: " + string.Join(", ", commandsWithoutValidator));
    }
}
