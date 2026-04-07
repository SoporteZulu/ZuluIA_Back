using MediatR;
using Microsoft.EntityFrameworkCore;
using ZuluIA_Back.Application.Common.Interfaces;
using ZuluIA_Back.Domain.Common;
using ZuluIA_Back.Domain.Entities.Comercial;
using ZuluIA_Back.Domain.Entities.Items;
using ZuluIA_Back.Domain.Interfaces;

namespace ZuluIA_Back.Application.Features.Items.Commands;

public class UpdateItemCommandHandler(
    IItemRepository repo,
    IApplicationDbContext db,
    IUnitOfWork uow,
    ICurrentUserService currentUser)
    : IRequestHandler<UpdateItemCommand, Result>
{
    public async Task<Result> Handle(UpdateItemCommand request, CancellationToken ct)
    {
        var item = await repo.GetByIdAsync(request.Id, ct);
        if (item is null)
            return Result.Failure($"No se encontró el ítem con ID {request.Id}.");

        var precioCostoAnterior = item.PrecioCosto;
        var precioVentaAnterior = item.PrecioVenta;
        var porcentajeGananciaObjetivo = request.PorcentajeGanancia ?? item.PorcentajeGanancia;

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
                    return Result.Failure("Uno o más atributos comerciales no existen o están inactivos.");

                foreach (var atributoInput in request.AtributosComerciales)
                    atributosComerciales[atributoInput.AtributoComercialId].ValidarValor(atributoInput.Valor);
            }

            if (request.Componentes is { Count: > 0 })
            {
                componenteIds = request.Componentes
                    .Select(x => x.ComponenteId)
                    .Distinct()
                    .ToList();

                if (componenteIds.Contains(request.Id))
                    return Result.Failure("Un ítem no puede ser componente de sí mismo.");

                var componentesValidos = await db.Items
                    .AsNoTracking()
                    .CountAsync(x => componenteIds.Contains(x.Id) && x.Activo, ct);

                if (componentesValidos != componenteIds.Count)
                    return Result.Failure("Uno o más componentes no existen o están inactivos.");
            }

            item.ValidarEdicionPermitida();

            var actualizaPrecios = request.PrecioCosto != precioCostoAnterior || request.PrecioVenta != precioVentaAnterior;
            var actualizaConfiguracionVentas =
                request.AplicaVentas.HasValue ||
                request.AplicaCompras.HasValue ||
                request.PorcentajeMaximoDescuento.HasValue ||
                request.EsRpt.HasValue;
            var actualizaPorcentajeGanancia = request.PorcentajeGanancia != item.PorcentajeGanancia;

            item.Actualizar(
                request.Descripcion,
                request.DescripcionAdicional,
                request.CodigoBarras,
                request.UnidadMedidaId,
                request.AlicuotaIvaId,
                request.MonedaId,
                request.EsProducto,
                request.EsServicio,
                request.EsFinanciero,
                request.ManejaStock,
                request.CategoriaId,
                request.MarcaId,
                request.CodigoAfip,
                request.StockMinimo,
                request.StockMaximo,
                item.SucursalId,
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

            if (actualizaPrecios && !porcentajeGananciaObjetivo.HasValue)
                item.ActualizarPrecios(request.PrecioCosto, request.PrecioVenta, currentUser.UserId);

            if (actualizaConfiguracionVentas)
            {
                item.ActualizarConfiguracionVentas(
                    request.AplicaVentas ?? item.AplicaVentas,
                    request.AplicaCompras ?? item.AplicaCompras,
                    request.PorcentajeMaximoDescuento ?? item.PorcentajeMaximoDescuento,
                    request.EsRpt ?? item.EsRpt,
                    currentUser.UserId);
            }

            if (actualizaPorcentajeGanancia)
                item.ActualizarPorcentajeGanancia(request.PorcentajeGanancia, currentUser.UserId);

            if (porcentajeGananciaObjetivo.HasValue &&
                (actualizaPorcentajeGanancia || request.PrecioCosto != precioCostoAnterior))
            {
                var precioVentaCalculado = Math.Round(
                    request.PrecioCosto * (1 + (porcentajeGananciaObjetivo.Value / 100m)),
                    2);

                item.ActualizarPrecios(
                    request.PrecioCosto,
                    precioVentaCalculado,
                    currentUser.UserId);
            }

            if (request.AtributosComerciales is not null)
            {
                var existentes = await db.ItemsAtributosComerciales
                    .Where(x => x.ItemId == request.Id && !x.IsDeleted)
                    .ToListAsync(ct);

                foreach (var existente in existentes)
                    db.ItemsAtributosComerciales.Remove(existente);

                foreach (var atributoInput in request.AtributosComerciales)
                {
                    db.ItemsAtributosComerciales.Add(ItemAtributoComercial.Crear(
                        request.Id,
                        atributoInput.AtributoComercialId,
                        atributoInput.Valor,
                        currentUser.UserId));
                }
            }

            if (request.Componentes is not null)
            {
                var componentesExistentes = await db.ItemsComponentes
                    .Where(x => x.ItemPadreId == request.Id)
                    .ToListAsync(ct);

                foreach (var componenteExistente in componentesExistentes)
                    db.ItemsComponentes.Remove(componenteExistente);

                foreach (var componente in request.Componentes)
                {
                    db.ItemsComponentes.Add(ItemComponente.Crear(
                        request.Id,
                        componente.ComponenteId,
                        componente.Cantidad,
                        componente.UnidadMedidaId));
                }
            }

            repo.Update(item);
            await uow.SaveChangesAsync(ct);

            return Result.Success();
        }
        catch (InvalidOperationException ex)
        {
            return Result.Failure(ex.Message);
        }
    }
}
