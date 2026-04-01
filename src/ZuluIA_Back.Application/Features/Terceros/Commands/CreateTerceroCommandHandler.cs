using MediatR;
using Microsoft.EntityFrameworkCore;
using ZuluIA_Back.Application.Common.Interfaces;
using ZuluIA_Back.Domain.Common;
using ZuluIA_Back.Domain.Entities.Terceros;
using ZuluIA_Back.Domain.Enums;
using ZuluIA_Back.Domain.Interfaces;
using ZuluIA_Back.Domain.ValueObjects;

namespace ZuluIA_Back.Application.Features.Terceros.Commands;

public class CreateTerceroCommandHandler(
    ITerceroRepository repo,
    IUnitOfWork uow,
    ICurrentUserService currentUser,
    IApplicationDbContext db)
    : IRequestHandler<CreateTerceroCommand, Result<long>>
{
    public async Task<Result<long>> Handle(CreateTerceroCommand command, CancellationToken ct)
    {
        var legajo = await ResolveLegajoAsync(command, ct);
        var tipoDocumentoId = await ResolveTipoDocumentoIdAsync(command, ct);
        var nroDocumento = ResolveNumeroDocumento(command);
        var tipoPersoneria = ResolveTipoPersoneria(command.TipoPersoneria);

        var fiscalValidation = await TerceroFiscalRules.ValidateAsync(
            db,
            command.CondicionIvaId,
            tipoDocumentoId,
            nroDocumento,
            tipoPersoneria,
            command.ClaveFiscal,
            command.ValorClaveFiscal,
            ct);

        if (fiscalValidation is not null)
            return Result.Failure<long>(fiscalValidation);

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
            return Result.Failure<long>(roleCatalogValidation);

        string? estadoCivilDescripcion = command.EstadoCivil;
        if (command.EstadoCivilId.HasValue)
        {
            estadoCivilDescripcion = await db.EstadosCiviles
                .AsNoTracking()
                .Where(x => x.Id == command.EstadoCivilId.Value && x.DeletedAt == null && x.Activo)
                .Select(x => x.Descripcion)
                .FirstOrDefaultAsync(ct);

            if (string.IsNullOrWhiteSpace(estadoCivilDescripcion))
                return Result.Failure<long>("El estado civil indicado no existe o está inactivo.");
        }

        if (command.EstadoPersonaId.HasValue)
        {
            var estadoPersonaActivo = await db.EstadosPersonas
                .AsNoTracking()
                .AnyAsync(x => x.Id == command.EstadoPersonaId.Value && x.DeletedAt == null && x.Activo, ct);

            if (!estadoPersonaActivo)
                return Result.Failure<long>("El estado general indicado no existe o está inactivo.");
        }

        var provinciaPrincipalResult = await TerceroGeografiaRules.ResolveProvinciaIdAsync(
            db,
            command.ProvinciaId,
            command.LocalidadId,
            command.BarrioId,
            ct);

        if (provinciaPrincipalResult.IsFailure)
            return Result.Failure<long>(provinciaPrincipalResult.Error!);

        // ── 1. Validar unicidad de Legajo ──────────────────────────────────────
        if (await repo.ExisteLegajoAsync(legajo, null, ct))
            return Result.Failure<long>($"Ya existe un tercero con el legajo '{legajo.ToUpperInvariant()}'.");

        // ── 2. Validar unicidad de NroDocumento ────────────────────────────────
        if (!string.IsNullOrWhiteSpace(nroDocumento) &&
            await repo.ExisteNroDocumentoAsync(nroDocumento, null, ct))
            return Result.Failure<long>($"Ya existe un tercero con el documento '{nroDocumento}'.");

        // ── 3. Construir el Value Object Domicilio ─────────────────────────────
        var domicilio = Domicilio.Crear(
            command.Calle,
            command.Nro,
            command.Piso,
            command.Dpto,
            command.CodigoPostal,
            command.LocalidadId,
            command.BarrioId,
            provinciaPrincipalResult.Value);
        var domicilioPrincipal = await TerceroDomicilioPrincipalSync.ResolvePrincipalAsync(db, domicilio, command.Domicilios, ct);

        // ── 4. Crear la entidad usando el factory method ───────────────────────
        Tercero tercero;
        try
        {
            tercero = Tercero.Crear(
                legajo,
                command.RazonSocial,
                tipoDocumentoId,
                nroDocumento,
                command.CondicionIvaId,
                command.EsCliente,
                command.EsProveedor,
                command.EsEmpleado,
                command.SucursalId,
                currentUser.UserId);
        }
        catch (ArgumentException ex)
        {
            return Result.Failure<long>(ex.Message);
        }

        // ── 5. Aplicar datos opcionales y comerciales ──────────────────────────
        var nombreFantasia = string.IsNullOrWhiteSpace(command.NombreFantasia)
            ? command.RazonSocial
            : command.NombreFantasia;

        try
        {
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
            return Result.Failure<long>(ex.Message);
        }

        tercero.SetMoneda(command.MonedaId);
        tercero.SetCategoria(command.CategoriaId);
        tercero.SetPais(command.PaisId);
        tercero.SetEstadoPersona(command.EstadoPersonaId);
        tercero.SetFechaRegistro(command.FechaRegistro ?? DateOnly.FromDateTime(DateTime.Today), currentUser.UserId);
        tercero.SetCategoriaCliente(command.EsCliente ? command.CategoriaClienteId : null);
        tercero.SetEstadoCliente(command.EsCliente ? command.EstadoClienteId : null);
        tercero.SetCategoriaProveedor(command.EsProveedor ? command.CategoriaProveedorId : null);
        tercero.SetEstadoProveedor(command.EsProveedor ? command.EstadoProveedorId : null);

        // ── 6. Roles (si es empleado, asegura el estado correcto) ──────────────
        if (command.EsEmpleado)
            tercero.ActualizarRoles(
                command.EsCliente,
                command.EsProveedor,
                true,
                currentUser.UserId);

        // ── 7. Persistir ───────────────────────────────────────────────────────
        try
        {
            await uow.ExecuteInTransactionAsync(async transactionCt =>
            {
                await repo.AddAsync(tercero, transactionCt);
                await uow.SaveChangesAsync(transactionCt);

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

                await uow.SaveChangesAsync(transactionCt);
            }, ct);
        }
        catch (InvalidOperationException ex)
        {
            return Result.Failure<long>(ex.Message);
        }
        catch (ArgumentException ex)
        {
            return Result.Failure<long>(ex.Message);
        }

        return Result.Success(tercero.Id);
    }

    private static string ResolveNumeroDocumento(CreateTerceroCommand command)
    {
        return command.NroDocumento?.Trim() ?? string.Empty;
    }

    private async Task<long> ResolveTipoDocumentoIdAsync(CreateTerceroCommand command, CancellationToken ct)
    {
        if (command.TipoDocumentoId.HasValue && command.TipoDocumentoId.Value > 0)
            return command.TipoDocumentoId.Value;

        var nroDocumento = ResolveNumeroDocumento(command);
        var codigo = ResolveTipoDocumentoCodigo(command.CondicionIvaId, nroDocumento);

        var tipoDocumentoId = await db.TiposDocumento
            .AsNoTracking()
            .Where(x => x.Codigo == codigo)
            .Select(x => x.Id)
            .FirstOrDefaultAsync(ct);

        if (tipoDocumentoId <= 0)
            throw new InvalidOperationException($"No se encontró el tipo de documento con código {codigo}.");

        return tipoDocumentoId;
    }

    private static short ResolveTipoDocumentoCodigo(long condicionIvaId, string nroDocumento)
    {
        var digits = new string((nroDocumento ?? string.Empty).Where(char.IsDigit).ToArray());

        if (digits.Length == 11)
            return 80;

        if (digits.Length > 0)
            return 96;

        return condicionIvaId == 5 ? (short)99 : (short)96;
    }

    private static TipoPersoneriaTercero ResolveTipoPersoneria(string? tipoPersoneria)
    {
        if (string.IsNullOrWhiteSpace(tipoPersoneria))
            return TipoPersoneriaTercero.Juridica;

        return Enum.TryParse<TipoPersoneriaTercero>(tipoPersoneria, true, out var parsed)
            ? parsed
            : TipoPersoneriaTercero.Juridica;
    }

    private async Task<string> ResolveLegajoAsync(CreateTerceroCommand command, CancellationToken ct)
    {
        if (!string.IsNullOrWhiteSpace(command.Legajo))
            return command.Legajo.Trim().ToUpperInvariant();

        var prefix = ResolvePrefix(command);

        for (var attempt = 0; attempt < 5; attempt++)
        {
            var candidate = $"{prefix}{DateTime.UtcNow:yyMMddHHmmss}{attempt}";
            if (!await repo.ExisteLegajoAsync(candidate, null, ct))
                return candidate;

            await Task.Delay(20, ct);
        }

        throw new InvalidOperationException("No se pudo generar un legajo automático para el tercero.");
    }

    private static string ResolvePrefix(CreateTerceroCommand command)
    {
        if (command.EsEmpleado)
            return "EMP";

        if (command.EsCliente && command.EsProveedor)
            return "TER";

        if (command.EsCliente)
            return "CLI";

        if (command.EsProveedor)
            return "PRV";

        return "TER";
    }
}
