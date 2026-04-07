using MediatR;
using Microsoft.EntityFrameworkCore;
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
        var domicilioPrincipal = await TerceroDomicilioPrincipalSync.ResolvePrincipalAsync(db, domicilio, command.Domicilios, ct);

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

        var roleCatalogValidation = await TerceroRoleCatalogValidation.ValidateAsync(
            db,
            command.EsCliente,
            command.EsProveedor,
            command.CategoriaClienteId,
            command.EstadoClienteId,
            command.CategoriaProveedorId,
            command.EstadoProveedorId,
            ct);

        if (roleCatalogValidation is not null)
            return Result.Failure(roleCatalogValidation);

        string? estadoCivilDescripcion = command.EstadoCivil;
        if (command.EstadoCivilId.HasValue)
        {
            estadoCivilDescripcion = await db.EstadosCiviles
                .AsNoTracking()
                .Where(x => x.Id == command.EstadoCivilId.Value && x.DeletedAt == null && x.Activo)
                .Select(x => x.Descripcion)
                .FirstOrDefaultAsync(ct);

            if (string.IsNullOrWhiteSpace(estadoCivilDescripcion))
                return Result.Failure("El estado civil indicado no existe o está inactivo.");
        }

        if (command.EstadoPersonaId.HasValue)
        {
            var estadoPersonaActivo = await db.EstadosPersonas
                .AsNoTracking()
                .AnyAsync(x => x.Id == command.EstadoPersonaId.Value && x.DeletedAt == null && x.Activo, ct);

            if (!estadoPersonaActivo)
                return Result.Failure("El estado general indicado no existe o está inactivo.");
        }

        var provinciaPrincipalResult = await TerceroGeografiaRules.ResolveProvinciaIdAsync(
            db,
            command.ProvinciaId,
            command.LocalidadId,
            command.BarrioId,
            ct);

        if (provinciaPrincipalResult.IsFailure)
            return Result.Failure(provinciaPrincipalResult.Error!);

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
                command.Tratamiento,
                command.Profesion,
                command.EstadoCivilId,
                estadoCivilDescripcion,
                command.Nacionalidad,
                command.Sexo,
                command.FechaNacimiento,
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
                domicilioPrincipal,
                command.NroIngresosBrutos,
                command.NroMunicipal,
                command.LimiteCredito,
                command.PorcentajeMaximoDescuento,
                command.VigenciaCreditoDesde,
                command.VigenciaCreditoHasta,
                command.Facturable,
                command.CobradorId,
                command.AplicaComisionCobrador,
                command.PctComisionCobrador,
                command.VendedorId,
                command.AplicaComisionVendedor,
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
        tercero.SetPais(command.PaisId);
        tercero.SetEstadoPersona(command.EstadoPersonaId);
        if (command.FechaRegistro.HasValue)
            tercero.SetFechaRegistro(command.FechaRegistro, currentUser.UserId);
        tercero.SetCategoriaCliente(command.EsCliente ? command.CategoriaClienteId : null);
        tercero.SetEstadoCliente(command.EsCliente ? command.EstadoClienteId : null);
        tercero.SetCategoriaProveedor(command.EsProveedor ? command.CategoriaProveedorId : null);
        tercero.SetEstadoProveedor(command.EsProveedor ? command.EstadoProveedorId : null);

        // ── 7. Persistir ───────────────────────────────────────────────────────
        try
        {
            await uow.ExecuteInTransactionAsync(async transactionCt =>
            {
                var aggregateError = await TerceroAggregatePersistence.ApplyOptionalSectionsAsync(
                    db,
                    tercero.Id,
                    currentUser.UserId,
                    command.PerfilComercial,
                    command.Domicilios,
                    command.Contactos,
                    command.SucursalesEntrega,
                    command.Transportes,
                    command.VentanasCobranza,
                    transactionCt);

                if (aggregateError is not null)
                    throw new InvalidOperationException(aggregateError);

                if (command.Domicilios is null)
                    await TerceroAggregatePersistence.SyncPrincipalDomicilioAsync(db, tercero.Id, domicilioPrincipal, transactionCt);

                repo.Update(tercero);
                await uow.SaveChangesAsync(transactionCt);
            }, ct);
        }
        catch (InvalidOperationException ex)
        {
            return Result.Failure(ex.Message);
        }
        catch (ArgumentException ex)
        {
            return Result.Failure(ex.Message);
        }

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
