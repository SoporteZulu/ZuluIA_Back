using FluentAssertions;
using Xunit;
using ZuluIA_Back.Architecture.Tests.Helpers;

namespace ZuluIA_Back.Architecture.Tests;

public class CircularDependencyTests
{
    [Fact]
    public void Assemblies_NoDependenciasCirculares_EntreCapas()
    {
        // Domain no referencia a nadie
        var domainRefs = AssemblyReferences.DomainAssembly
            .GetReferencedAssemblies()
            .Select(a => a.Name ?? "")
            .Where(n => n.StartsWith("ZuluIA_Back"))
            .ToList();

        domainRefs.Should().BeEmpty(
            because: "Domain no debe referenciar ningún otro proyecto de ZuluIA_Back. " +
                     "Referencias encontradas: " + string.Join(", ", domainRefs));

        // Application solo referencia a Domain
        var appRefs = AssemblyReferences.ApplicationAssembly
            .GetReferencedAssemblies()
            .Select(a => a.Name ?? "")
            .Where(n => n.StartsWith("ZuluIA_Back"))
            .ToList();

        appRefs.Should().OnlyContain(
            n => n == "ZuluIA_Back.Domain",
            because: "Application solo debe referenciar a Domain. " +
                     "Referencias encontradas: " + string.Join(", ", appRefs));

        // Infrastructure referencia Application y Domain (no Api)
        var infraRefs = AssemblyReferences.InfrastructureAssembly
            .GetReferencedAssemblies()
            .Select(a => a.Name ?? "")
            .Where(n => n.StartsWith("ZuluIA_Back"))
            .ToList();

        infraRefs.Should().NotContain(
            "ZuluIA_Back.Api",
            because: "Infrastructure no debe referenciar a Api. " +
                     "Referencias encontradas: " + string.Join(", ", infraRefs));
    }

    [Fact]
    public void Domain_NoReferenciaFrameworksExternas_Prohibidos()
    {
        var refsProhibidas = new[]
        {
            "Microsoft.EntityFrameworkCore",
            "Dapper",
            "Npgsql",
            "Microsoft.AspNetCore"
        };

        var domainRefs = AssemblyReferences.DomainAssembly
            .GetReferencedAssemblies()
            .Select(a => a.Name ?? "")
            .ToList();

        foreach (var refProhibida in refsProhibidas)
        {
            domainRefs.Should().NotContain(
                refProhibida,
                because: $"Domain no debe referenciar '{refProhibida}'. " +
                         "Domain debe ser puro y sin dependencias de infraestructura.");
        }
    }

    [Fact]
    public void Application_NoReferenciaEFCore()
    {
        var appRefs = AssemblyReferences.ApplicationAssembly
            .GetReferencedAssemblies()
            .Select(a => a.Name ?? "")
            .ToList();

        appRefs.Should().NotContain(
            "Microsoft.EntityFrameworkCore",
            because: "Application no debe referenciar EF Core directamente. " +
                     "Debe usar IApplicationDbContext.");

        appRefs.Should().NotContain(
            "Npgsql",
            because: "Application no debe referenciar Npgsql directamente.");
    }
}