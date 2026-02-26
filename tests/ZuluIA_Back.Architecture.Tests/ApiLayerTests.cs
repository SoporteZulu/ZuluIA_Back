using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using NetArchTest.Rules;
using Xunit;
using ZuluIA_Back.Architecture.Tests.Helpers;

namespace ZuluIA_Back.Architecture.Tests;

public class ApiLayerTests
{
    [Fact]
    public void Controllers_DebenHeredarDeBaseController()
    {
        var result = Types
            .InAssembly(AssemblyReferences.ApiAssembly)
            .That()
            .HaveNameEndingWith("Controller")
            .And()
            .AreNotAbstract()
            .Should()
            .Inherit(typeof(ZuluIA_Back.Api.Controllers.BaseController))
            .GetResult();

        result.IsSuccessful.Should().BeTrue(
            because: "Todos los Controllers deben heredar de BaseController. " +
                     "Tipos fallidos: " + string.Join(", ", result.FailingTypeNames ?? []));
    }

    [Fact]
    public void Controllers_DebenResidirEnCarpetaControllers()
    {
        var result = Types
            .InAssembly(AssemblyReferences.ApiAssembly)
            .That()
            .HaveNameEndingWith("Controller")
            .And()
            .AreNotAbstract()
            .Should()
            .ResideInNamespaceContaining("Controllers")
            .GetResult();

        result.IsSuccessful.Should().BeTrue(
            because: "Todos los Controllers deben residir en la carpeta Controllers. " +
                     "Tipos fallidos: " + string.Join(", ", result.FailingTypeNames ?? []));
    }

    [Fact]
    public void Controllers_DebenTenerAtributoApiController()
    {
        var controllers = AssemblyReferences.ApiAssembly
            .GetTypes()
            .Where(t => t.Name.EndsWith("Controller")
                     && !t.IsAbstract
                     && t.IsClass)
            .ToList();

        foreach (var controller in controllers)
        {
            var tieneAtributo = controller
                .GetCustomAttributes(typeof(ApiControllerAttribute), inherit: true)
                .Any();

            tieneAtributo.Should().BeTrue(
                because: $"{controller.Name} debe tener el atributo [ApiController].");
        }
    }

    [Fact]
    public void Middleware_DebeResidirEnCarpetaMiddleware()
    {
        var result = Types
            .InAssembly(AssemblyReferences.ApiAssembly)
            .That()
            .HaveNameEndingWith("Middleware")
            .And()
            .AreNotAbstract()
            .Should()
            .ResideInNamespaceContaining("Middleware")
            .GetResult();

        result.IsSuccessful.Should().BeTrue(
            because: "Todos los Middleware deben residir en la carpeta Middleware. " +
                     "Tipos fallidos: " + string.Join(", ", result.FailingTypeNames ?? []));
    }

    [Fact]
    public void Api_NoDependeDirectamenteDeInfrastructure()
    {
        var controllersConDependenciaDirecta = AssemblyReferences.ApiAssembly
            .GetTypes()
            .Where(t => t.Name.EndsWith("Controller") && !t.IsAbstract)
            .Where(t =>
            {
                var ctors = t.GetConstructors();
                return ctors.Any(c =>
                    c.GetParameters().Any(p =>
                        p.ParameterType.Namespace?.StartsWith(
                            AssemblyReferences.InfrastructureNamespace) == true
                        && !p.ParameterType.IsInterface));
            })
            .Select(t => t.Name)
            .ToList();

        controllersConDependenciaDirecta.Should().BeEmpty(
            because: "Los Controllers no deben depender directamente de clases de Infrastructure, " +
                     "sino de interfaces. Controladores con dependencia directa: " +
                     string.Join(", ", controllersConDependenciaDirecta));
    }
}