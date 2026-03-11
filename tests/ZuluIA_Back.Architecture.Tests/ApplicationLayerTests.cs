using FluentAssertions;
using MediatR;
using NetArchTest.Rules;
using Xunit;
using ZuluIA_Back.Architecture.Tests.Helpers;

namespace ZuluIA_Back.Architecture.Tests;

public class ApplicationLayerTests
{
    [Fact]
    public void Application_NoDependeDeInfrastructure()
    {
        var result = Types
            .InAssembly(AssemblyReferences.ApplicationAssembly)
            .ShouldNot()
            .HaveDependencyOn(AssemblyReferences.InfrastructureNamespace)
            .GetResult();

        result.IsSuccessful.Should().BeTrue(
            because: "Application no debe depender de Infrastructure. " +
                     "Tipos fallidos: " + string.Join(", ", result.FailingTypeNames ?? []));
    }

    [Fact]
    public void Application_NoDependeDeApi()
    {
        var result = Types
            .InAssembly(AssemblyReferences.ApplicationAssembly)
            .ShouldNot()
            .HaveDependencyOn(AssemblyReferences.ApiNamespace)
            .GetResult();

        result.IsSuccessful.Should().BeTrue(
            because: "Application no debe depender de Api. " +
                     "Tipos fallidos: " + string.Join(", ", result.FailingTypeNames ?? []));
    }

    [Fact]
    public void Commands_DebenImplementarIRequest()
    {
        var result = Types
            .InAssembly(AssemblyReferences.ApplicationAssembly)
            .That()
            .HaveNameEndingWith("Command")
            .And()
            .AreNotAbstract()
            .Should()
            .ImplementInterface(typeof(IRequest<>))
            .GetResult();

        result.IsSuccessful.Should().BeTrue(
            because: "Todos los Commands deben implementar IRequest<>. " +
                     "Tipos fallidos: " + string.Join(", ", result.FailingTypeNames ?? []));
    }

    [Fact]
    public void Queries_DebenImplementarIRequest()
    {
        var result = Types
            .InAssembly(AssemblyReferences.ApplicationAssembly)
            .That()
            .HaveNameEndingWith("Query")
            .And()
            .AreNotAbstract()
            .Should()
            .ImplementInterface(typeof(IRequest<>))
            .GetResult();

        result.IsSuccessful.Should().BeTrue(
            because: "Todos los Queries deben implementar IRequest<>. " +
                     "Tipos fallidos: " + string.Join(", ", result.FailingTypeNames ?? []));
    }

    [Fact]
    public void CommandHandlers_DebenImplementarIRequestHandler()
    {
        var result = Types
            .InAssembly(AssemblyReferences.ApplicationAssembly)
            .That()
            .HaveNameEndingWith("CommandHandler")
            .And()
            .AreNotAbstract()
            .Should()
            .ImplementInterface(typeof(IRequestHandler<,>))
            .GetResult();

        result.IsSuccessful.Should().BeTrue(
            because: "Todos los CommandHandlers deben implementar IRequestHandler<,>. " +
                     "Tipos fallidos: " + string.Join(", ", result.FailingTypeNames ?? []));
    }

    [Fact]
    public void QueryHandlers_DebenImplementarIRequestHandler()
    {
        var result = Types
            .InAssembly(AssemblyReferences.ApplicationAssembly)
            .That()
            .HaveNameEndingWith("QueryHandler")
            .And()
            .AreNotAbstract()
            .Should()
            .ImplementInterface(typeof(IRequestHandler<,>))
            .GetResult();

        result.IsSuccessful.Should().BeTrue(
            because: "Todos los QueryHandlers deben implementar IRequestHandler<,>. " +
                     "Tipos fallidos: " + string.Join(", ", result.FailingTypeNames ?? []));
    }

    [Fact]
    public void Validators_DebenResidirEnCarpetaCommandsOQueries()
    {
        var validatorsEnLugarIncorrecto = AssemblyReferences.ApplicationAssembly
            .GetTypes()
            .Where(t => t.Name.EndsWith("Validator")
                     && !t.IsAbstract
                     && t.IsClass)
            .Where(t =>
            {
                var ns = t.Namespace ?? "";
                return !ns.Contains("Commands") && !ns.Contains("Queries");
            })
            .Select(t => t.Name)
            .ToList();

        validatorsEnLugarIncorrecto.Should().BeEmpty(
            because: "Todos los Validators deben residir en carpetas Commands o Queries. " +
                     "Tipos fuera de lugar: " + string.Join(", ", validatorsEnLugarIncorrecto));
    }

    [Fact]
    public void DTOs_DebenResidirEnCarpetaDTOs()
    {
        // Excluimos DTOs de Commands/Queries que son input DTOs del command/query
        var result = Types
            .InAssembly(AssemblyReferences.ApplicationAssembly)
            .That()
            .HaveNameEndingWith("Dto")
            .And()
            .AreClasses()
            .And()
            .DoNotResideInNamespaceContaining("Commands")
            .And()
            .DoNotResideInNamespaceContaining("Queries")
            .Should()
            .ResideInNamespaceContaining("DTOs")
            .GetResult();

        result.IsSuccessful.Should().BeTrue(
            because: "Todos los DTOs de respuesta deben residir en carpetas DTOs. " +
                     "Tipos fallidos: " + string.Join(", ", result.FailingTypeNames ?? []));
    }

    [Fact]
    public void Handlers_NoDebenTenerConstructorPublico_ConMasDeDosParametros()
    {
        var handlers = AssemblyReferences.ApplicationAssembly
            .GetTypes()
            .Where(t => t.Name.EndsWith("Handler") && !t.IsAbstract && t.IsClass)
            .ToList();

        foreach (var handler in handlers)
        {
            var constructors = handler.GetConstructors();
            foreach (var ctor in constructors)
            {
                var paramCount = ctor.GetParameters().Length;
                paramCount.Should().BeLessOrEqualTo(7,
                    because: $"{handler.Name} no debería tener más de 7 dependencias inyectadas.");
            }
        }
    }
}