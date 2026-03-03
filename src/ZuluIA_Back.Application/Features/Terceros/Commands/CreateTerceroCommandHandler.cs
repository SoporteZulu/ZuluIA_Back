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
    public async Task<Result<long>> Handle(CreateTerceroCommand command, CancellationToken ct)
    {
        // ── 1. Validar unicidad de Legajo ──────────────────────────────────────
        if (await repo.ExisteLegajoAsync(command.Legajo, null, ct))
            return Result.Failure<long>($"Ya existe un tercero con el legajo '{command.Legajo.ToUpperInvariant()}'.");

        // ── 2. Validar unicidad de NroDocumento ────────────────────────────────
        if (await repo.ExisteNroDocumentoAsync(command.NroDocumento, null, ct))
            return Result.Failure<long>($"Ya existe un tercero con el documento '{command.NroDocumento}'.");

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
                command.EsEmpleado,
                command.SucursalId,
                currentUser.UserId);
        }
        catch (ArgumentException ex)
        {
            return Result.Failure<long>(ex.Message);
        }

        // ── 5. Aplicar datos opcionales y comerciales ──────────────────────────
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

        tercero.SetMoneda(command.MonedaId);
        tercero.SetCategoria(command.CategoriaId);

        // ── 6. Roles (si es empleado, asegura el estado correcto) ──────────────
        if (command.EsEmpleado)
            tercero.ActualizarRoles(
                command.EsCliente,
                command.EsProveedor,
                true,
                currentUser.UserId);

        // ── 7. Persistir ───────────────────────────────────────────────────────
        await repo.AddAsync(tercero, ct);
        await uow.SaveChangesAsync(ct);

        return Result.Success(tercero.Id);
    }
}
