using Microsoft.AspNetCore.Mvc;

namespace ZuluIA_Back.Api.Security;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = false, Inherited = true)]
public sealed class AuditCriticalOperationAttribute(string operationName) : TypeFilterAttribute(typeof(CriticalOperationAuditFilter))
{
    public string OperationName { get; } = operationName.Trim().ToUpperInvariant();
}
