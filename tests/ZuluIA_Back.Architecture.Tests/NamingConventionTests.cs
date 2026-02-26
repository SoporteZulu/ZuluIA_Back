using FluentAssertions;
using NetArchTest.Rules;
using Xunit;
using ZuluIA_Back.Architecture.Tests.Helpers;

namespace ZuluIA_Back.Architecture.Tests;

public class NamingConventionTests
{
    [Fact]
    public void Interfaces_DebenEmpezarConI()
    {
        var result = Types
            .InAssemblies([
                AssemblyReferences.DomainAssembly,
                AssemblyReferences.ApplicationAssembly
            ])
            .That()
            .AreInterfaces()
            .Should()
            .HaveNameStartingWith("I")
            .GetResult();

        result.IsSuccessful.Should().BeTrue(
            because: "Todas las interfaces deben empezar con 'I'. " +
                     "Tipos fallidos: " + string.Join(", ", result.FailingTypeNames ?? []));
    }

    [Fact]
    public void AbstractClasses_NoDebenSerInstanciables()
    {
        var result = Types
            .InAssemblies([
                AssemblyReferences.DomainAssembly,
                AssemblyReferences.ApplicationAssembly,
                AssemblyReferences.InfrastructureAssembly
            ])
            .That()
            .AreAbstract()
            .And()
            .AreClasses()
            .Should()
            .NotBePublic()
            .Or()
            .BeAbstract()
            .GetResult();

        result.IsSuccessful.Should().BeTrue(
            because: "Las clases abstractas no deben ser instanciables. " +
                     "Tipos fallidos: " + string.Join(", ", result.FailingTypeNames ?? []));
    }

    [Fact]
    public void Handlers_DebenTerminarEnHandler()
    {
        var result = Types
            .InAssembly(AssemblyReferences.ApplicationAssembly)
            .That()
            .ImplementInterface(typeof(MediatR.IRequestHandler<,>))
            .And()
            .AreNotAbstract()
            .Should()
            .HaveNameEndingWith("Handler")
            .GetResult();

        result.IsSuccessful.Should().BeTrue(
            because: "Todos los IRequestHandler deben terminar en 'Handler'. " +
                     "Tipos fallidos: " + string.Join(", ", result.FailingTypeNames ?? []));
    }

    [Fact]
    public void Commands_DebenTerminarEnCommand()
    {
        var result = Types
            .InAssembly(AssemblyReferences.ApplicationAssembly)
            .That()
            .ResideInNamespaceContaining("Commands")
            .And()
            .AreNotAbstract()
            .And()
            .DoNotHaveNameEndingWith("Handler")
            .And()
            .DoNotHaveNameEndingWith("Validator")
            .And()
            .AreClasses()
            .Should()
            .HaveNameEndingWith("Command")
            .GetResult();

        result.IsSuccessful.Should().BeTrue(
            because: "Todos los Commands deben terminar en 'Command'. " +
                     "Tipos fallidos: " + string.Join(", ", result.FailingTypeNames ?? []));
    }

    [Fact]
    public void Queries_DebenTerminarEnQuery()
    {
        var result = Types
            .InAssembly(AssemblyReferences.ApplicationAssembly)
            .That()
            .ResideInNamespaceContaining("Queries")
            .And()
            .AreNotAbstract()
            .And()
            .DoNotHaveNameEndingWith("Handler")
            .And()
            .AreClasses()
            .Should()
            .HaveNameEndingWith("Query")
            .GetResult();

        result.IsSuccessful.Should().BeTrue(
            because: "Todos los Queries deben terminar en 'Query'. " +
                     "Tipos fallidos: " + string.Join(", ", result.FailingTypeNames ?? []));
    }

    [Fact]
    public void Entities_NoDebenTenerSettersPublicos()
    {
        var entidadesConSettersPublicos = AssemblyReferences.DomainAssembly
            .GetTypes()
            .Where(t => t.Namespace?.Contains(".Entities") == true
                     && t.IsClass
                     && !t.IsAbstract)
            .SelectMany(t => t.GetProperties())
            .Where(p => p.SetMethod?.IsPublic == true)
            .Select(p => $"{p.DeclaringType?.Name}.{p.Name}")
            .ToList();

        entidadesConSettersPublicos.Should().BeEmpty(
            because: "Las entidades del dominio no deben exponer setters públicos " +
                     "(encapsulamiento). Propiedades con setter público: " +
                     string.Join(", ", entidadesConSettersPublicos));
    }
}