using FluentValidation;
using MediatR;
using ZuluIA_Back.Application.Common.Interfaces;
using ZuluIA_Back.Domain.Entities.Auditoria;
using ZuluIA_Back.Domain.Enums;

namespace ZuluIA_Back.Application.Features.Auditoria.Commands;

public record RegistrarAuditoriaCaeaCommand(
    long CaeaId,
    long? UsuarioId,
    AccionAuditoria Accion,
    string? DetalleCambio,
    string? IpOrigen)
    : IRequest<Unit>;

public class RegistrarAuditoriaCaeaCommandHandler(IApplicationDbContext db)
    : IRequestHandler<RegistrarAuditoriaCaeaCommand, Unit>
{
    public async Task<Unit> Handle(RegistrarAuditoriaCaeaCommand request, CancellationToken ct)
    {
        var registro = AuditoriaCaea.Registrar(
            request.CaeaId,
            request.UsuarioId,
            request.Accion,
            request.DetalleCambio,
            request.IpOrigen);

        db.AuditoriaCaeas.Add(registro);
        await db.SaveChangesAsync(ct);
        return Unit.Value;
    }
}

public class RegistrarAuditoriaCaeaCommandValidator : AbstractValidator<RegistrarAuditoriaCaeaCommand>
{
    public RegistrarAuditoriaCaeaCommandValidator()
    {
        RuleFor(x => x.CaeaId).GreaterThan(0);
    }
}