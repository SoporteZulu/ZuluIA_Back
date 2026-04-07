using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using ZuluIA_Back.Application.Common.Interfaces;
using ZuluIA_Back.Domain.Common;
using ZuluIA_Back.Domain.Entities.Auditoria;
using ZuluIA_Back.Domain.Enums;

namespace ZuluIA_Back.Application.Features.Auditoria.Commands;

public record RegistrarAuditoriaComprobanteCommand(
    long ComprobanteId,
    long? UsuarioId,
    AccionAuditoria Accion,
    string? DetalleCambio,
    string? IpOrigen)
    : IRequest<Unit>;

public class RegistrarAuditoriaComprobanteCommandHandler(IApplicationDbContext db)
    : IRequestHandler<RegistrarAuditoriaComprobanteCommand, Unit>
{
    public async Task<Unit> Handle(RegistrarAuditoriaComprobanteCommand request, CancellationToken ct)
    {
        var registro = AuditoriaComprobante.Registrar(
            request.ComprobanteId,
            request.UsuarioId,
            request.Accion,
            request.DetalleCambio,
            request.IpOrigen);
        db.AuditoriaComprobantes.Add(registro);
        await db.SaveChangesAsync(ct);
        return Unit.Value;
    }
}

public class RegistrarAuditoriaComprobanteCommandValidator
    : AbstractValidator<RegistrarAuditoriaComprobanteCommand>
{
    public RegistrarAuditoriaComprobanteCommandValidator()
    {
        RuleFor(x => x.ComprobanteId).GreaterThan(0);
    }
}
