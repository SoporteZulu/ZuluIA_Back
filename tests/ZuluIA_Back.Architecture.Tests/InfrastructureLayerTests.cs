using FluentAssertions;
using NetArchTest.Rules;
using Xunit;
using ZuluIA_Back.Application.Common.Interfaces;
using ZuluIA_Back.Architecture.Tests.Helpers;
using ZuluIA_Back.Domain.Interfaces;
using ZuluIA_Back.Infrastructure.Persistence.Repositories;
using ZuluIA_Back.Infrastructure.Services;

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

    [Fact]
    public void HttpCurrentUserService_DebeImplementarICurrentUserService()
    {
        typeof(HttpCurrentUserService)
            .Should().Implement<ICurrentUserService>(
                because: "HttpCurrentUserService debe implementar ICurrentUserService para inyectarse en Application.");
    }

    [Fact]
    public void DomainEventDispatcher_DebeImplementarIDomainEventDispatcher()
    {
        typeof(DomainEventDispatcher)
            .Should().Implement<IDomainEventDispatcher>(
                because: "DomainEventDispatcher debe implementar IDomainEventDispatcher para inyectarse en Application.");
    }

    [Fact]
    public void InfrastructureServices_DebenImplementarInterfacesDeApplicationODomain()
    {
        var serviciosConProblemas = AssemblyReferences.InfrastructureAssembly
            .GetTypes()
            .Where(t => t.Name.EndsWith("Service")
                     && !t.IsAbstract
                     && t.IsClass)
            .Where(t =>
            {
                var interfaces = t.GetInterfaces();
                return !interfaces.Any(i =>
                    i.Namespace?.StartsWith("ZuluIA_Back.Application") == true ||
                    i.Namespace?.StartsWith("ZuluIA_Back.Domain") == true);
            })
            .Select(t => t.Name)
            .ToList();

        serviciosConProblemas.Should().BeEmpty(
            because: "Todos los Services de Infrastructure deben implementar al menos una interfaz de Application o Domain. " +
                     "Tipos fallidos: " + string.Join(", ", serviciosConProblemas));
    }

    [Fact]
    public void BaseRepository_ImplementaIRepositoryDeT()
    {
        var baseRepoType = typeof(BaseRepository<>);
        var iRepoType    = typeof(IRepository<>);

        baseRepoType.GetInterfaces()
            .Any(i => i.IsGenericType && i.GetGenericTypeDefinition() == iRepoType)
            .Should().BeTrue(
                because: "BaseRepository<T> debe implementar IRepository<T> para " +
                         "ser la implementación base de todos los repositorios.");
    }

    [Fact]
    public void TodoasLasInterfacesRepositorio_DebenTenerImplementacionEnInfrastructure()
    {
        // Collect all non-generic IXxxRepository interfaces from Domain
        var repoInterfaces = AssemblyReferences.DomainAssembly
            .GetTypes()
            .Where(t => t.IsInterface
                     && t.Name.StartsWith("I")
                     && t.Name.EndsWith("Repository")
                     && !t.IsGenericTypeDefinition
                     && t.Name != "IRepository")
            .ToList();

        // Collect all concrete classes in Infrastructure that implement any of those interfaces
        var implementedInterfaces = AssemblyReferences.InfrastructureAssembly
            .GetTypes()
            .Where(t => t.IsClass && !t.IsAbstract)
            .SelectMany(t => t.GetInterfaces())
            .Where(i => i.IsInterface
                     && i.Namespace?.StartsWith("ZuluIA_Back.Domain") == true
                     && i.Name.EndsWith("Repository"))
            .Select(i => i.Name)
            .ToHashSet();

        var sinImplementacion = repoInterfaces
            .Where(i => !implementedInterfaces.Contains(i.Name))
            .Select(i => i.Name)
            .OrderBy(n => n)
            .ToList();

        sinImplementacion.Should().BeEmpty(
            because: "Toda interfaz IXxxRepository del Domain debe tener al menos una clase concreta " +
                     "en Infrastructure que la implemente. Sin implementación: " +
                     string.Join(", ", sinImplementacion));
    }
}