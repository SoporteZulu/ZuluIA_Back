using MediatR;
using Microsoft.EntityFrameworkCore;
using ZuluIA_Back.Application.Common.Interfaces;
using ZuluIA_Back.Domain.Common;
using ZuluIA_Back.Domain.Entities.Comercial;
using ZuluIA_Back.Domain.Entities.Items;
using ZuluIA_Back.Domain.Interfaces;

namespace ZuluIA_Back.Application.Features.Items.Commands;

public class CreateItemCommandHandler(
    IItemRepository repo,
    IApplicationDbContext db,
    IUnitOfWork uow,
    ICurrentUserService currentUser)
    : IRequestHandler<CreateItemCommand, Result<long>>
{
    public async Task<Result<long>> Handle(
        CreateItemCommand request,
        CancellationToken ct)
    {
        var existe = request.SucursalId.HasValue
            ? await repo.ExisteCodigoAsync(request.Codigo, request.SucursalId, null, ct)
            : await repo.ExisteCodigoAsync(request.Codigo, null, ct);

        if (existe)
            return Result.Failure<long>(
                $"Ya existe un ítem con el código '{request.Codigo}'.");

        try
        {
            Dictionary<long, AtributoComercial>? atributosComerciales = null;
            List<long>? componenteIds = null;
            if (request.AtributosComerciales is { Count: > 0 })
            {
                var atributoIds = request.AtributosComerciales
                    .Select(x => x.AtributoComercialId)
                    .Distinct()
                    .ToList();

                atributosComerciales = await db.AtributosComerciales
                    .AsNoTracking()
                    .Where(x => atributoIds.Contains(x.Id) && x.Activo)
                    .ToDictionaryAsync(x => x.Id, ct);

                if (atributosComerciales.Count != atributoIds.Count)
                    return Result.Failure<long>("Uno o más atributos comerciales no existen o están inactivos.");

                foreach (var atributoInput in request.AtributosComerciales)
                    atributosComerciales[atributoInput.AtributoComercialId].ValidarValor(atributoInput.Valor);
            }

            if (request.Componentes is { Count: > 0 })
            {
                componenteIds = request.Componentes
                    .Select(x => x.ComponenteId)
                    .Distinct()
                    .ToList();

                var componentesValidos = await db.Items
                    .AsNoTracking()
                    .CountAsync(x => componenteIds.Contains(x.Id) && x.Activo, ct);

                if (componentesValidos != componenteIds.Count)
                    return Result.Failure<long>("Uno o más componentes no existen o están inactivos.");
            }

            var item = Item.Crear(
                request.Codigo,
                request.Descripcion,
                request.UnidadMedidaId,
                request.AlicuotaIvaId,
                request.MonedaId,
                request.EsProducto,
                request.EsServicio,
                request.EsFinanciero,
                request.ManejaStock,
                request.PrecioCosto,
                request.PrecioVenta,
                request.CategoriaId,
                request.MarcaId,
                request.StockMinimo,
                request.StockMaximo,
                request.CodigoBarras,
                request.DescripcionAdicional,
                request.CodigoAfip,
                request.SucursalId,
                request.AplicaVentas,
                request.AplicaCompras,
                request.PorcentajeGanancia,
                request.PorcentajeMaximoDescuento,
                request.EsRpt,
                request.EsSistema,
                currentUser.UserId,
                request.PuntoReposicion,
                request.StockSeguridad,
                request.Peso,
                request.Volumen,
                request.CodigoAlternativo,
                request.EsTrazable,
                request.PermiteFraccionamiento,
                request.DiasVencimientoLimite,
                request.DepositoDefaultId,
                request.AlicuotaIvaCompraId,
                request.ImpuestoInternoId);

            await repo.AddAsync(item, ct);
            await uow.SaveChangesAsync(ct);

            if (request.AtributosComerciales is { Count: > 0 })
            {
                foreach (var atributoInput in request.AtributosComerciales)
                {
                    db.ItemsAtributosComerciales.Add(ItemAtributoComercial.Crear(
                        item.Id,
                        atributoInput.AtributoComercialId,
                        atributoInput.Valor,
                        currentUser.UserId));
                }

                await uow.SaveChangesAsync(ct);
            }

            if (request.Componentes is { Count: > 0 })
            {
                foreach (var componente in request.Componentes)
                {
                    db.ItemsComponentes.Add(ItemComponente.Crear(
                        item.Id,
                        componente.ComponenteId,
                        componente.Cantidad,
                        componente.UnidadMedidaId));
                }

                await uow.SaveChangesAsync(ct);
            }

            return Result.Success(item.Id);
        }
        catch (InvalidOperationException ex)
        {
            return Result.Failure<long>(ex.Message);
        }
    }
}
