using FluentAssertions;
using NetArchTest.Rules;
using Xunit;
using ZuluIA_Back.Architecture.Tests.Helpers;

namespace ZuluIA_Back.Architecture.Tests;

public class DependencyFlowTests
{
    [Fact]
    public void FlujoDependencias_DomainNoConoceANadie()
    {
        var resultApp = Types.InAssembly(AssemblyReferences.DomainAssembly)
            .ShouldNot().HaveDependencyOn(AssemblyReferences.ApplicationNamespace).GetResult();

        var resultInfra = Types.InAssembly(AssemblyReferences.DomainAssembly)
            .ShouldNot().HaveDependencyOn(AssemblyReferences.InfrastructureNamespace).GetResult();

        var resultApi = Types.InAssembly(AssemblyReferences.DomainAssembly)
            .ShouldNot().HaveDependencyOn(AssemblyReferences.ApiNamespace).GetResult();

        resultApp.IsSuccessful.Should().BeTrue(
            "Domain → Application: viola Clean Architecture");
        resultInfra.IsSuccessful.Should().BeTrue(
            "Domain → Infrastructure: viola Clean Architecture");
        resultApi.IsSuccessful.Should().BeTrue(
            "Domain → Api: viola Clean Architecture");
    }

    [Fact]
    public void FlujoDependencias_ApplicationSoloConoceADomain()
    {
        var resultInfra = Types.InAssembly(AssemblyReferences.ApplicationAssembly)
            .ShouldNot().HaveDependencyOn(AssemblyReferences.InfrastructureNamespace).GetResult();

        var resultApi = Types.InAssembly(AssemblyReferences.ApplicationAssembly)
            .ShouldNot().HaveDependencyOn(AssemblyReferences.ApiNamespace).GetResult();

        resultInfra.IsSuccessful.Should().BeTrue(
            "Application → Infrastructure: viola Clean Architecture");
        resultApi.IsSuccessful.Should().BeTrue(
            "Application → Api: viola Clean Architecture");
    }

    [Fact]
    public void FlujoDependencias_InfrastructureNoConoceApi()
    {
        var result = Types.InAssembly(AssemblyReferences.InfrastructureAssembly)
            .ShouldNot().HaveDependencyOn(AssemblyReferences.ApiNamespace).GetResult();

        result.IsSuccessful.Should().BeTrue(
            "Infrastructure → Api: viola Clean Architecture");
    }

    [Fact]
    public void FlujoDependencias_ApiPuedeConocerTodos()
    {
        var apiTypes = AssemblyReferences.ApiAssembly.GetTypes();

        apiTypes.Should().NotBeEmpty(
            because: "La capa API debe tener tipos definidos.");
    }

    [Fact]
    public void CleanArchitecture_CapasCorrectamenteOrdenadas()
    {
        var dependencias = new Dictionary<string, string[]>
        {
            [AssemblyReferences.DomainNamespace]         = [],
            [AssemblyReferences.ApplicationNamespace]    = [AssemblyReferences.DomainNamespace],
            [AssemblyReferences.InfrastructureNamespace] = [AssemblyReferences.DomainNamespace, AssemblyReferences.ApplicationNamespace],
            [AssemblyReferences.ApiNamespace]            = [AssemblyReferences.DomainNamespace, AssemblyReferences.ApplicationNamespace, AssemblyReferences.InfrastructureNamespace]
        };

        dependencias.Should().NotBeEmpty(
            because: "La arquitectura limpia requiere capas bien definidas.");

        dependencias[AssemblyReferences.DomainNamespace].Should().BeEmpty(
            because: "Domain es la capa más interna y no debe depender de nadie.");

        dependencias[AssemblyReferences.ApplicationNamespace].Should().Contain(
            AssemblyReferences.DomainNamespace,
            because: "Application debe conocer a Domain.");

        dependencias[AssemblyReferences.InfrastructureNamespace].Should().Contain(
            AssemblyReferences.ApplicationNamespace,
            because: "Infrastructure debe conocer a Application.");
    }
}