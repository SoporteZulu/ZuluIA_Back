using MediatR;
using ZuluIA_Back.Application.Common.Interfaces;
using ZuluIA_Back.Domain.Common;
using ZuluIA_Back.Domain.Entities.Terceros;
using ZuluIA_Back.Domain.Interfaces;
using ZuluIA_Back.Domain.ValueObjects;

namespace ZuluIA_Back.Application.Features.Terceros.Commands;

public class CreateTerceroCommandHandler(
    ITerceroRepository repo,
    IUnitOfWork uow,
    ICurrentUserService currentUser)
    : IRequestHandler<CreateTerceroCommand, Result<long>>
{
    public async Task<Result<long>> Handle(
        CreateTerceroCommand command,
        CancellationToken ct)
    {
        // ── 1. Validar unicidad de Legajo ──────────────────────────────────────
        // Equivalente al validarDatos() del VB6 que chequeaba duplicados
        // de legajo antes de permitir el alta.
        if (await repo.ExisteLegajoAsync(command.Legajo, excludeId: null, ct))
            return (Result<long>)Result<long>.Failure(
                $"Ya existe un tercero con el legajo '{command.Legajo.ToUpperInvariant()}'.");

        // ── 2. Validar unicidad de NroDocumento ────────────────────────────────
        // VB6: "Ya existe un cliente/proveedor con ese CUIT/DNI."
        if (await repo.ExisteNroDocumentoAsync(command.NroDocumento, excludeId: null, ct))
            return (Result<long>)Result<long>.Failure(
                $"Ya existe un tercero con el documento '{command.NroDocumento}'.");

        // ── 3. Construir el Value Object Domicilio ─────────────────────────────
        var domicilio = Domicilio.Crear(
            command.Calle,
            command.Nro,
            command.Piso,
            command.Dpto,
            command.CodigoPostal,
            command.LocalidadId,
            command.BarrioId);

        // ── 4. Crear la entidad usando el factory method ───────────────────────
        // Tercero.Crear() valida las reglas de negocio internas (roles, campos
        // requeridos) y dispara TerceroCreadoEvent.
        Tercero tercero;
        try
        {
            tercero = Tercero.Crear(
                command.Legajo,
                command.RazonSocial,
                command.TipoDocumentoId,
                command.NroDocumento,
                command.CondicionIvaId,
                command.EsCliente,
                command.EsProveedor,
                command.SucursalId,
                currentUser.UserId);
        }
        catch (ArgumentException ex)
        {
            return (Result<long>)Result<long>.Failure(ex.Message);
        }

        // ── 5. Aplicar datos opcionales ────────────────────────────────────────
        // Actualizar() aplica todos los campos editables de una vez.
        tercero.Actualizar(
            command.RazonSocial,
            command.NombreFantasia,
            command.CondicionIvaId,
            command.Telefono,
            command.Celular,
            command.Email,
            command.Web,
            domicilio,
            command.NroIngresosBrutos,
            command.NroMunicipal,
            command.LimiteCredito,
            command.Facturable,
            command.CobradorId,
            command.PctComisionCobrador,
            command.VendedorId,
            command.PctComisionVendedor,
            command.Observacion,
            currentUser.UserId);

        // Opcionales con setter propio
        tercero.SetMoneda(command.MonedaId);
        tercero.SetCategoria(command.CategoriaId);

        if (command.EsEmpleado)
            tercero.ActualizarRoles(
                command.EsCliente,
                command.EsProveedor,
                esEmpleado: true,
                currentUser.UserId);

        // ── 6. Persistir ───────────────────────────────────────────────────────
        await repo.AddAsync(tercero, ct);
        await uow.SaveChangesAsync(ct);

        return Result<long>.Success(tercero.Id);
    }
}