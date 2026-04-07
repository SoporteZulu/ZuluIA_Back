using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using ZuluIA_Back.Application.Common.Interfaces;
using ZuluIA_Back.Domain.Common;
using ZuluIA_Back.Domain.Entities.Ventas;

namespace ZuluIA_Back.Application.Features.ObjetivosVenta.DTOs
{
    public record ObjetivoVentaDto(
        long Id,
        long SucursalId,
        long VendedorId,
        int Periodo,
        decimal MontoObjetivo,
        decimal MontoRealizado,
        decimal PorcentajeCumplimiento,
        string? Descripcion,
        bool Cerrado);
}

namespace ZuluIA_Back.Application.Features.ObjetivosVenta.Commands
{
    using ZuluIA_Back.Application.Features.ObjetivosVenta.DTOs;

    public record CrearObjetivoVentaCommand(
        long SucursalId, long VendedorId, int Periodo,
        decimal MontoObjetivo, string? Descripcion, long? UserId)
        : IRequest<Result<long>>;

    public class CrearObjetivoVentaCommandHandler(IApplicationDbContext db)
        : IRequestHandler<CrearObjetivoVentaCommand, Result<long>>
    {
        public async Task<Result<long>> Handle(CrearObjetivoVentaCommand request, CancellationToken ct)
        {
            var existe = await db.ObjetivosVenta
                .AnyAsync(o => o.SucursalId == request.SucursalId
                            && o.VendedorId == request.VendedorId
                            && o.Periodo == request.Periodo
                            && o.DeletedAt == null, ct);
            if (existe) return Result.Failure<long>("Ya existe un objetivo para este vendedor en el periodo indicado.");

            var objetivo = ObjetivoVenta.Crear(
                request.SucursalId, request.VendedorId, request.Periodo,
                request.MontoObjetivo, request.Descripcion, request.UserId);
            db.ObjetivosVenta.Add(objetivo);
            await db.SaveChangesAsync(ct);
            return Result.Success(objetivo.Id);
        }
    }

    public class CrearObjetivoVentaCommandValidator : AbstractValidator<CrearObjetivoVentaCommand>
    {
        public CrearObjetivoVentaCommandValidator()
        {
            RuleFor(x => x.SucursalId).GreaterThan(0);
            RuleFor(x => x.VendedorId).GreaterThan(0);
            RuleFor(x => x.Periodo).InclusiveBetween(190001, 209912);
            RuleFor(x => x.MontoObjetivo).GreaterThan(0);
        }
    }

    public record ActualizarObjetivoVentaCommand(long Id, decimal NuevoMonto, long? UserId)
        : IRequest<Result>;

    public class ActualizarObjetivoVentaCommandHandler(IApplicationDbContext db)
        : IRequestHandler<ActualizarObjetivoVentaCommand, Result>
    {
        public async Task<Result> Handle(ActualizarObjetivoVentaCommand request, CancellationToken ct)
        {
            var objetivo = await db.ObjetivosVenta.FindAsync([request.Id], ct);
            if (objetivo is null) return Result.Failure("Objetivo no encontrado.");
            objetivo.ActualizarMontoObjetivo(request.NuevoMonto, request.UserId);
            await db.SaveChangesAsync(ct);
            return Result.Success();
        }
    }

    public record CerrarPeriodoObjetivoCommand(long Id, long? UserId) : IRequest<Result>;

    public class CerrarPeriodoObjetivoCommandHandler(IApplicationDbContext db)
        : IRequestHandler<CerrarPeriodoObjetivoCommand, Result>
    {
        public async Task<Result> Handle(CerrarPeriodoObjetivoCommand request, CancellationToken ct)
        {
            var objetivo = await db.ObjetivosVenta.FindAsync([request.Id], ct);
            if (objetivo is null) return Result.Failure("Objetivo no encontrado.");
            objetivo.CerrarPeriodo(request.UserId);
            await db.SaveChangesAsync(ct);
            return Result.Success();
        }
    }
}

namespace ZuluIA_Back.Application.Features.ObjetivosVenta.Queries
{
    using ZuluIA_Back.Application.Features.ObjetivosVenta.DTOs;

    public record GetObjetivosVentaQuery(
        long SucursalId, long? VendedorId, int? Periodo, bool? Cerrado,
        int Page = 1, int PageSize = 20)
        : IRequest<PagedResult<ObjetivoVentaDto>>;

    public class GetObjetivosVentaQueryHandler(IApplicationDbContext db)
        : IRequestHandler<GetObjetivosVentaQuery, PagedResult<ObjetivoVentaDto>>
    {
        public async Task<PagedResult<ObjetivoVentaDto>> Handle(GetObjetivosVentaQuery request, CancellationToken ct)
        {
            var query = db.ObjetivosVenta.AsNoTracking()
                .Where(o => o.SucursalId == request.SucursalId && o.DeletedAt == null);

            if (request.VendedorId.HasValue) query = query.Where(o => o.VendedorId == request.VendedorId);
            if (request.Periodo.HasValue)    query = query.Where(o => o.Periodo == request.Periodo);
            if (request.Cerrado.HasValue)    query = query.Where(o => o.Cerrado == request.Cerrado);

            var total = await query.CountAsync(ct);
            var items = await query
                .OrderByDescending(o => o.Periodo)
                .ThenBy(o => o.VendedorId)
                .Skip((request.Page - 1) * request.PageSize)
                .Take(request.PageSize)
                .Select(o => new ObjetivoVentaDto(
                    o.Id, o.SucursalId, o.VendedorId, o.Periodo,
                    o.MontoObjetivo, o.MontoRealizado,
                    o.MontoObjetivo > 0 ? o.MontoRealizado / o.MontoObjetivo * 100 : 0,
                    o.Descripcion, o.Cerrado))
                .ToListAsync(ct);

            return new PagedResult<ObjetivoVentaDto>(items, request.Page, request.PageSize, total);
        }
    }
}