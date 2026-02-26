using FluentAssertions;
using Xunit;
using ZuluIA_Back.Architecture.Tests.Helpers;

namespace ZuluIA_Back.Architecture.Tests;

public class EncapsulationTests
{
    [Fact]
    public void Entities_ConstructoresPublicos_DebenSerPrivados()
    {
        var entidadesConConstructorPublico = AssemblyReferences.DomainAssembly
            .GetTypes()
            .Where(t => t.Namespace?.Contains(".Entities") == true
                     && t.IsClass
                     && !t.IsAbstract)
            .Where(t => t.GetConstructors()
                .Any(c => c.IsPublic && c.GetParameters().Length == 0))
            .Select(t => t.Name)
            .ToList();

        entidadesConConstructorPublico.Should().BeEmpty(
            because: "Las entidades del dominio no deben tener constructores públicos sin parámetros. " +
                     "Deben usar métodos de fábrica estáticos (Crear). " +
                     "Entidades con constructor público: " +
                     string.Join(", ", entidadesConConstructorPublico));
    }

    [Fact]
    public void DomainEvents_DebenSerSealed()
    {
        var eventosNoSealed = AssemblyReferences.DomainAssembly
            .GetTypes()
            .Where(t => t.Namespace?.Contains(".Events") == true
                     && t.IsClass
                     && !t.IsAbstract
                     && !t.IsSealed)
            .Select(t => t.Name)
            .ToList();

        eventosNoSealed.Should().BeEmpty(
            because: "Los Domain Events deben ser sealed para evitar herencia. " +
                     "Eventos no sealed: " +
                     string.Join(", ", eventosNoSealed));
    }

    [Fact]
    public void ValueObjects_DebenSerSealed()
    {
        var voNoSealed = AssemblyReferences.DomainAssembly
            .GetTypes()
            .Where(t => t.Namespace?.Contains(".ValueObjects") == true
                     && t.IsClass
                     && !t.IsAbstract
                     && !t.IsSealed)
            .Select(t => t.Name)
            .ToList();

        voNoSealed.Should().BeEmpty(
            because: "Los Value Objects deben ser sealed para evitar herencia. " +
                     "Value Objects no sealed: " +
                     string.Join(", ", voNoSealed));
    }

    [Fact]
    public void Repositories_DebenSerInterfaces_EnDomain()
    {
        var reposEnDomain = AssemblyReferences.DomainAssembly
            .GetTypes()
            .Where(t => t.Namespace?.Contains(".Interfaces") == true
                     && t.Name.Contains("Repository"))
            .ToList();

        foreach (var repo in reposEnDomain)
        {
            repo.IsInterface.Should().BeTrue(
                because: $"{repo.Name} en Domain debe ser una interface, no una clase concreta.");
        }
    }

    [Fact]
    public void Collections_EnEntidades_DebenSerReadOnly()
    {
        var propiedadesNoReadOnly = AssemblyReferences.DomainAssembly
            .GetTypes()
            .Where(t => t.Namespace?.Contains(".Entities") == true
                     && t.IsClass
                     && !t.IsAbstract)
            .SelectMany(t => t.GetProperties())
            .Where(p =>
            {
                var type = p.PropertyType;
                var esColeccion = type.IsGenericType &&
                    (type.GetGenericTypeDefinition() == typeof(IList<>)  ||
                     type.GetGenericTypeDefinition() == typeof(List<>));
                return esColeccion;
            })
            .Select(p => $"{p.DeclaringType?.Name}.{p.Name}")
            .ToList();

        propiedadesNoReadOnly.Should().BeEmpty(
            because: "Las colecciones en entidades deben ser IReadOnlyCollection<> o IReadOnlyList<>, " +
                     "no List<> o IList<> públicas. Propiedades que violan esto: " +
                     string.Join(", ", propiedadesNoReadOnly));
    }
}