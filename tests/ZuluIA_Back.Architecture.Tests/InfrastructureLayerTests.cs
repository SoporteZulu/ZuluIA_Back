using FluentAssertions;
using NetArchTest.Rules;
using Xunit;
using ZuluIA_Back.Architecture.Tests.Helpers;

namespace ZuluIA_Back.Architecture.Tests;

public class InfrastructureLayerTests
{
    [Fact]
    public void Infrastructure_NoDependeDeApi()
    {
        var result = Types
            .InAssembly(AssemblyReferences.InfrastructureAssembly)
            .ShouldNot()
            .HaveDependencyOn(AssemblyReferences.ApiNamespace)
            .GetResult();

        result.IsSuccessful.Should().BeTrue(
            because: "Infrastructure no debe depender de Api. " +
                     "Tipos fallidos: " + string.Join(", ", result.FailingTypeNames ?? []));
    }

    [Fact]
    public void Repositories_DebenResidirEnCarpetaRepositories()
    {
        var result = Types
            .InAssembly(AssemblyReferences.InfrastructureAssembly)
            .That()
            .HaveNameEndingWith("Repository")
            .And()
            .AreNotAbstract()
            .Should()
            .ResideInNamespaceContaining("Repositories")
            .GetResult();

        result.IsSuccessful.Should().BeTrue(
            because: "Todos los Repositories deben residir en Persistence.Repositories. " +
                     "Tipos fallidos: " + string.Join(", ", result.FailingTypeNames ?? []));
    }

    [Fact]
    public void Configurations_DebenResidirEnCarpetaConfigurations()
    {
        var result = Types
            .InAssembly(AssemblyReferences.InfrastructureAssembly)
            .That()
            .HaveNameEndingWith("Configuration")
            .And()
            .AreNotAbstract()
            .Should()
            .ResideInNamespaceContaining("Configurations")
            .GetResult();

        result.IsSuccessful.Should().BeTrue(
            because: "Todas las Configurations deben residir en Persistence.Configurations. " +
                     "Tipos fallidos: " + string.Join(", ", result.FailingTypeNames ?? []));
    }

    [Fact]
    public void Repositories_DebenImplementarIRepository()
    {
        var result = Types
            .InAssembly(AssemblyReferences.InfrastructureAssembly)
            .That()
            .HaveNameEndingWith("Repository")
            .And()
            .AreNotAbstract()
            .Should()
            .ImplementInterface(typeof(ZuluIA_Back.Domain.Interfaces.IUnitOfWork))
            .Or()
            .ImplementInterface(typeof(ZuluIA_Back.Domain.Interfaces.IRepository<>))
            .GetResult();

        result.IsSuccessful.Should().BeTrue(
            because: "Todos los Repositories deben implementar IRepository<>. " +
                     "Tipos fallidos: " + string.Join(", ", result.FailingTypeNames ?? []));
    }

    [Fact]
    public void Services_DebenResidirEnCarpetaServices()
    {
        var result = Types
            .InAssembly(AssemblyReferences.InfrastructureAssembly)
            .That()
            .HaveNameEndingWith("Service")
            .And()
            .AreNotAbstract()
            .Should()
            .ResideInNamespaceContaining("Services")
            .GetResult();

        result.IsSuccessful.Should().BeTrue(
            because: "Todos los Services deben residir en la carpeta Services. " +
                     "Tipos fallidos: " + string.Join(", ", result.FailingTypeNames ?? []));
    }
}