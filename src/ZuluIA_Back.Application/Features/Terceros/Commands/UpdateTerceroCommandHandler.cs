using MediatR;
using ZuluIA_Back.Application.Common.Interfaces;
using ZuluIA_Back.Domain.Common;
using ZuluIA_Back.Domain.Interfaces;
using ZuluIA_Back.Domain.ValueObjects;

namespace ZuluIA_Back.Application.Features.Terceros.Commands;

public class UpdateTerceroCommandHandler(
    ITerceroRepository repo,
    IUnitOfWork uow,
    ICurrentUserService currentUser)
    : IRequestHandler<UpdateTerceroCommand, Result>
{
    public async Task<Result> Handle(
        UpdateTerceroCommand command,
        CancellationToken ct)
    {
        // ── 1. Obtener entidad ────────────────────────────────────────────────
        var tercero = await repo.GetByIdAsync(command.Id, ct);

        if (tercero is null)
            return Result.Failure(
                $"No se encontró el tercero con Id {command.Id}.");

        if (tercero.IsDeleted)
            return Result.Failure(
                $"El tercero '{tercero.Legajo}' está dado de baja y no puede modificarse.");

        // ── 2. Validar cambio de NroDocumento (si viene) ───────────────────────
        // VB6: al cambiar el CUIT/DNI mostraba advertencia y verificaba
        // que no existiera otro tercero con ese documento.
        if (!string.IsNullOrWhiteSpace(command.NroDocumento) &&
            command.NroDocumento != tercero.NroDocumento)
        {
            if (await repo.ExisteNroDocumentoAsync(
                    command.NroDocumento, excludeId: command.Id, ct))
                return Result.Failure(
                    $"Ya existe un tercero con el documento '{command.NroDocumento}'.");

            tercero.ActualizarNroDocumento(command.NroDocumento, currentUser.UserId);
        }

        // ── 3. Validar cambio de roles ─────────────────────────────────────────
        // Si cambió algún rol, verificar que el cambio sea válido.
        // Ej: no se puede quitar EsEmpleado si tiene empleado activo asociado.
        var rolesChanged = command.EsCliente   != tercero.EsCliente   ||
                           command.EsProveedor != tercero.EsProveedor ||
                           command.EsEmpleado  != tercero.EsEmpleado;

        if (rolesChanged)
        {
            // Si está quitando EsEmpleado, verificar que no tenga empleado activo
            if (tercero.EsEmpleado && !command.EsEmpleado)
            {
                if (await repo.TieneEmpleadoActivoAsync(command.Id, ct))
                    return Result.Failure(
                        "No se puede quitar el rol Empleado porque el tercero " +
                        "tiene un legajo laboral activo. Dé de baja el empleado primero.");
            }

            try
            {
                tercero.ActualizarRoles(
                    command.EsCliente,
                    command.EsProveedor,
                    command.EsEmpleado,
                    currentUser.UserId);
            }
            catch (ArgumentException ex)
            {
                return Result.Failure(ex.Message);
            }
        }

        // ── 4. Construir el VO Domicilio ───────────────────────────────────────
        var domicilio = Domicilio.Crear(
            command.Calle,
            command.Nro,
            command.Piso,
            command.Dpto,
            command.CodigoPostal,
            command.LocalidadId,
            command.BarrioId);

        // ── 5. Actualizar datos principales ────────────────────────────────────
        // Equivalente al Guardar() en modo edición del VB6.
        try
        {
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
        }
        catch (ArgumentException ex)
        {
            return Result.Failure(ex.Message);
        }

        // ── 6. Opcionales con setter propio ────────────────────────────────────
        tercero.SetMoneda(command.MonedaId);
        tercero.SetCategoria(command.CategoriaId);

        // ── 7. Persistir ───────────────────────────────────────────────────────
        repo.Update(tercero);
        await uow.SaveChangesAsync(ct);

        return Result.Success();
    }
}