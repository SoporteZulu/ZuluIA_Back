using FluentAssertions;
using NetArchTest.Rules;
using Xunit;
using ZuluIA_Back.Architecture.Tests.Helpers;

namespace ZuluIA_Back.Architecture.Tests;

public class DomainLayerTests
{
    [Fact]
    public void Domain_NoDependeDeApplication()
    {
        var result = Types
            .InAssembly(AssemblyReferences.DomainAssembly)
            .ShouldNot()
            .HaveDependencyOn(AssemblyReferences.ApplicationNamespace)
            .GetResult();

        result.IsSuccessful.Should().BeTrue(
            because: "Domain no debe depender de Application. " +
                     "Tipos fallidos: " + string.Join(", ", result.FailingTypeNames ?? []));
    }

    [Fact]
    public void Domain_NoDependeDeInfrastructure()
    {
        var result = Types
            .InAssembly(AssemblyReferences.DomainAssembly)
            .ShouldNot()
            .HaveDependencyOn(AssemblyReferences.InfrastructureNamespace)
            .GetResult();

        result.IsSuccessful.Should().BeTrue(
            because: "Domain no debe depender de Infrastructure. " +
                     "Tipos fallidos: " + string.Join(", ", result.FailingTypeNames ?? []));
    }

    [Fact]
    public void Domain_NoDependeDeApi()
    {
        var result = Types
            .InAssembly(AssemblyReferences.DomainAssembly)
            .ShouldNot()
            .HaveDependencyOn(AssemblyReferences.ApiNamespace)
            .GetResult();

        result.IsSuccessful.Should().BeTrue(
            because: "Domain no debe depender de Api. " +
                     "Tipos fallidos: " + string.Join(", ", result.FailingTypeNames ?? []));
    }

    [Fact]
    public void Entities_DebenHeredarDeBaseEntity()
    {
        var result = Types
            .InAssembly(AssemblyReferences.DomainAssembly)
            .That()
            .ResideInNamespace($"{AssemblyReferences.DomainNamespace}.Entities")
            .And()
            .AreClasses()
            .And()
            .AreNotAbstract()
            .Should()
            .Inherit(typeof(ZuluIA_Back.Domain.Common.BaseEntity))
            .GetResult();

        result.IsSuccessful.Should().BeTrue(
            because: "Todas las entidades deben heredar de BaseEntity. " +
                     "Tipos fallidos: " + string.Join(", ", result.FailingTypeNames ?? []));
    }

    [Fact]
    public void DomainEvents_DebenImplementarIDomainEvent()
    {
        var result = Types
            .InAssembly(AssemblyReferences.DomainAssembly)
            .That()
            .ResideInNamespace($"{AssemblyReferences.DomainNamespace}.Events")
            .And()
            .AreClasses()
            .Should()
            .ImplementInterface(typeof(ZuluIA_Back.Domain.Common.IDomainEvent))
            .GetResult();

        result.IsSuccessful.Should().BeTrue(
            because: "Todos los domain events deben implementar IDomainEvent. " +
                     "Tipos fallidos: " + string.Join(", ", result.FailingTypeNames ?? []));
    }

    [Fact]
    public void ValueObjects_DebenHeredarDeValueObject()
    {
        var result = Types
            .InAssembly(AssemblyReferences.DomainAssembly)
            .That()
            .ResideInNamespace($"{AssemblyReferences.DomainNamespace}.ValueObjects")
            .And()
            .AreClasses()
            .Should()
            .Inherit(typeof(ZuluIA_Back.Domain.Common.ValueObject))
            .GetResult();

        result.IsSuccessful.Should().BeTrue(
            because: "Todos los value objects deben heredar de ValueObject. " +
                     "Tipos fallidos: " + string.Join(", ", result.FailingTypeNames ?? []));
    }
}