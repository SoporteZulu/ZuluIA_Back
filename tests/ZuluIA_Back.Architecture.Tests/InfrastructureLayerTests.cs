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
    public void Repositories_DebenImplementarInterfazDeDomain()
    {
        var reposConProblemas = AssemblyReferences.InfrastructureAssembly
            .GetTypes()
            .Where(t => t.Name.EndsWith("Repository")
                     && !t.IsAbstract
                     && t.IsClass)
            .Where(t =>
            {
                // Debe implementar AL MENOS UNA interfaz del namespace ZuluIA_Back.Domain
                var interfaces = t.GetInterfaces();
                return !interfaces.Any(i =>
                    i.Namespace?.StartsWith("ZuluIA_Back.Domain") == true);
            })
            .Select(t => t.Name)
            .ToList();

        reposConProblemas.Should().BeEmpty(
            because: "Todos los Repositories deben implementar al menos una interfaz de Domain. " +
                     "Tipos fallidos: " + string.Join(", ", reposConProblemas));
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