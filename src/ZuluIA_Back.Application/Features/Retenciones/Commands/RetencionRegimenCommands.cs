using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using ZuluIA_Back.Application.Common.Interfaces;
using ZuluIA_Back.Domain.Common;
using ZuluIA_Back.Domain.Entities.Finanzas;

namespace ZuluIA_Back.Application.Features.Retenciones.Commands;

public record CreateRetencionRegimenCommand(
    long RetencionId,
    string Codigo,
    string Descripcion,
    string? Observacion) : IRequest<Result<long>>;

public record UpdateRetencionRegimenCommand(
    long RetencionId,
    long RegimenId,
    bool ControlTipoComprobante,
    bool ControlTipoComprobanteAplica,
    string? BaseImponibleComposicion,
    decimal? NoImponible,
    bool NoImponibleAplica,
    decimal? BaseImponiblePorcentaje,
    bool BaseImponiblePorcentajeAplica,
    decimal? BaseImponibleMinimo,
    bool BaseImponibleMinimoAplica,
    decimal? BaseImponibleMaximo,
    bool BaseImponibleMaximoAplica,
    string? RetencionComposicion,
    decimal? RetencionMinimo,
    bool RetencionMinimoAplica,
    decimal? RetencionMaximo,
    bool RetencionMaximoAplica,
    decimal? Alicuota,
    bool AlicuotaAplica,
    bool AlicuotaEscalaAplica,
    decimal? AlicuotaConvenio,
    bool AlicuotaConvenioAplica,
    string? Observacion) : IRequest<Result>;

public record DeleteRetencionRegimenCommand(long RetencionId, long RegimenId) : IRequest<Result>;

public class CreateRetencionRegimenCommandHandler(IApplicationDbContext db)
    : IRequestHandler<CreateRetencionRegimenCommand, Result<long>>
{
    public async Task<Result<long>> Handle(CreateRetencionRegimenCommand request, CancellationToken ct)
    {
        RetencionRegimen regimen;
        try
        {
            regimen = RetencionRegimen.Crear(request.Codigo, request.Descripcion, request.RetencionId, request.Observacion);
        }
        catch (ArgumentException ex)
        {
            return Result.Failure<long>(ex.Message);
        }

        db.RetencionesRegimenes.Add(regimen);
        await db.SaveChangesAsync(ct);
        return Result.Success(regimen.Id);
    }
}

public class UpdateRetencionRegimenCommandHandler(IApplicationDbContext db)
    : IRequestHandler<UpdateRetencionRegimenCommand, Result>
{
    public async Task<Result> Handle(UpdateRetencionRegimenCommand request, CancellationToken ct)
    {
        var regimen = await db.RetencionesRegimenes
            .FirstOrDefaultAsync(r => r.Id == request.RegimenId && r.RetencionId == request.RetencionId, ct);
        if (regimen is null)
            return Result.Failure($"Régimen {request.RegimenId} no encontrado.");

        regimen.ActualizarParametros(
            request.ControlTipoComprobante,
            request.ControlTipoComprobanteAplica,
            request.BaseImponibleComposicion,
            request.NoImponible,
            request.NoImponibleAplica,
            request.BaseImponiblePorcentaje,
            request.BaseImponiblePorcentajeAplica,
            request.BaseImponibleMinimo,
            request.BaseImponibleMinimoAplica,
            request.BaseImponibleMaximo,
            request.BaseImponibleMaximoAplica,
            request.RetencionComposicion,
            request.RetencionMinimo,
            request.RetencionMinimoAplica,
            request.RetencionMaximo,
            request.RetencionMaximoAplica,
            request.Alicuota,
            request.AlicuotaAplica,
            request.AlicuotaEscalaAplica,
            request.AlicuotaConvenio,
            request.AlicuotaConvenioAplica,
            request.Observacion);

        await db.SaveChangesAsync(ct);
        return Result.Success();
    }
}

public class DeleteRetencionRegimenCommandHandler(IApplicationDbContext db)
    : IRequestHandler<DeleteRetencionRegimenCommand, Result>
{
    public async Task<Result> Handle(DeleteRetencionRegimenCommand request, CancellationToken ct)
    {
        var regimen = await db.RetencionesRegimenes
            .FirstOrDefaultAsync(r => r.Id == request.RegimenId && r.RetencionId == request.RetencionId, ct);
        if (regimen is null)
            return Result.Failure($"Régimen {request.RegimenId} no encontrado.");

        db.RetencionesRegimenes.Remove(regimen);
        await db.SaveChangesAsync(ct);
        return Result.Success();
    }
}

public class CreateRetencionRegimenCommandValidator : AbstractValidator<CreateRetencionRegimenCommand>
{
    public CreateRetencionRegimenCommandValidator()
    {
        RuleFor(x => x.RetencionId).GreaterThan(0);
        RuleFor(x => x.Codigo).NotEmpty();
        RuleFor(x => x.Descripcion).NotEmpty();
    }
}

public class UpdateRetencionRegimenCommandValidator : AbstractValidator<UpdateRetencionRegimenCommand>
{
    public UpdateRetencionRegimenCommandValidator()
    {
        RuleFor(x => x.RetencionId).GreaterThan(0);
        RuleFor(x => x.RegimenId).GreaterThan(0);
    }
}

public class DeleteRetencionRegimenCommandValidator : AbstractValidator<DeleteRetencionRegimenCommand>
{
    public DeleteRetencionRegimenCommandValidator()
    {
        RuleFor(x => x.RetencionId).GreaterThan(0);
        RuleFor(x => x.RegimenId).GreaterThan(0);
    }
}
