using MediatR;
using ZuluIA_Back.Application.Common.Interfaces;
using ZuluIA_Back.Domain.Common;
using ZuluIA_Back.Domain.Enums;
using ZuluIA_Back.Domain.Interfaces;
using ZuluIA_Back.Domain.ValueObjects;

namespace ZuluIA_Back.Application.Features.Terceros.Commands;

public class UpdateTerceroCommandHandler(
    ITerceroRepository repo,
    IUnitOfWork uow,
    ICurrentUserService currentUser,
    IApplicationDbContext db)
    : IRequestHandler<UpdateTerceroCommand, Result>
{
    public async Task<Result> Handle(UpdateTerceroCommand command, CancellationToken ct)
    {
        // ── 1. Obtener entidad ────────────────────────────────────────────────
        var tercero = await repo.GetByIdAsync(command.Id, ct);
        if (tercero is null)
            return Result.Failure($"No se encontró el tercero con Id {command.Id}.");

        if (tercero.IsDeleted)
            return Result.Failure($"El tercero '{tercero.Legajo}' está dado de baja y no puede modificarse.");

        // ── 2. Validar cambio de NroDocumento (si viene) ───────────────────────
        if (!string.IsNullOrWhiteSpace(command.NroDocumento) &&
            command.NroDocumento != tercero.NroDocumento)
        {
            if (await repo.ExisteNroDocumentoAsync(command.NroDocumento, excludeId: command.Id, ct))
                return Result.Failure($"Ya existe un tercero con el documento '{command.NroDocumento}'.");

            tercero.ActualizarNroDocumento(command.NroDocumento, currentUser.UserId);
        }

        // ── 3. Validar cambio de roles ─────────────────────────────────────────
        var rolesChanged = command.EsCliente   != tercero.EsCliente   ||
                           command.EsProveedor != tercero.EsProveedor ||
                           command.EsEmpleado  != tercero.EsEmpleado;

        if (rolesChanged)
        {
            if (tercero.EsEmpleado && !command.EsEmpleado)
            {
                if (await repo.TieneEmpleadoActivoAsync(command.Id, ct))
                    return Result.Failure(
                        "No se puede quitar el rol Empleado porque el tercero tiene un legajo laboral activo. Dé de baja el empleado primero.");
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

        var tipoPersoneria = ResolveTipoPersoneria(command.TipoPersoneria);
        var nroDocumento = string.IsNullOrWhiteSpace(command.NroDocumento) ? tercero.NroDocumento : command.NroDocumento.Trim();

        var fiscalValidation = await TerceroFiscalRules.ValidateAsync(
            db,
            command.CondicionIvaId,
            tercero.TipoDocumentoId,
            nroDocumento,
            tipoPersoneria,
            command.ClaveFiscal,
            command.ValorClaveFiscal,
            ct);

        if (fiscalValidation is not null)
            return Result.Failure(fiscalValidation);

        // ── 5. Actualizar datos principales ────────────────────────────────────
        try
        {
            var nombreFantasia = string.IsNullOrWhiteSpace(command.NombreFantasia)
                ? command.RazonSocial
                : command.NombreFantasia;

            tercero.ActualizarPersoneriaFiscal(
                tipoPersoneria,
                command.Nombre,
                command.Apellido,
                command.EsEntidadGubernamental,
                command.ClaveFiscal,
                command.ValorClaveFiscal,
                currentUser.UserId);

            tercero.Actualizar(
                command.RazonSocial,
                nombreFantasia,
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

    private static TipoPersoneriaTercero ResolveTipoPersoneria(string? tipoPersoneria)
    {
        if (string.IsNullOrWhiteSpace(tipoPersoneria))
            return TipoPersoneriaTercero.Juridica;

        return Enum.TryParse<TipoPersoneriaTercero>(tipoPersoneria, true, out var parsed)
            ? parsed
            : TipoPersoneriaTercero.Juridica;
    }
}
