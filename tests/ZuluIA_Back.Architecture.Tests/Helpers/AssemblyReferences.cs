using System.Reflection;

namespace ZuluIA_Back.Architecture.Tests.Helpers;

public static class AssemblyReferences
{
    public static readonly Assembly DomainAssembly =
        typeof(ZuluIA_Back.Domain.Common.BaseEntity).Assembly;

    public static readonly Assembly ApplicationAssembly =
        typeof(ZuluIA_Back.Application.DependencyInjection).Assembly;

    public static readonly Assembly InfrastructureAssembly =
        typeof(ZuluIA_Back.Infrastructure.DependencyInjection).Assembly;

    public static readonly Assembly ApiAssembly =
        typeof(ZuluIA_Back.Api.Middleware.ExceptionMiddleware).Assembly;

    public const string DomainNamespace = "ZuluIA_Back.Domain";
    public const string ApplicationNamespace = "ZuluIA_Back.Application";
    public const string InfrastructureNamespace = "ZuluIA_Back.Infrastructure";
    public const string ApiNamespace = "ZuluIA_Back.Api";
}