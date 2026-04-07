using MediatR;
using Microsoft.EntityFrameworkCore;
using ZuluIA_Back.Application.Common.Interfaces;
using ZuluIA_Back.Application.Features.Integraciones.Services;
using ZuluIA_Back.Application.Features.Terceros.Commands;
using ZuluIA_Back.Domain.Common;
using ZuluIA_Back.Domain.Interfaces;
using ZuluIA_Back.Domain.ValueObjects;

namespace ZuluIA_Back.Application.Features.Integraciones.Commands;

public class ImportarClientesCommandHandler(
    IApplicationDbContext db,
    ITerceroRepository repo,
    IMediator mediator,
    IUnitOfWork uow,
    IntegracionProcesoService procesoService)
    : IRequestHandler<ImportarClientesCommand, Result<long>>
{
    public async Task<Result<long>> Handle(ImportarClientesCommand request, CancellationToken ct)
    {
        var job = await procesoService.CrearJobAsync(Domain.Enums.TipoProcesoIntegracion.ImportacionClientes, "Importación de clientes", request.Clientes.Count, request.Observacion, ct);
        await uow.SaveChangesAsync(ct);

        try
        {
            foreach (var cliente in request.Clientes)
            {
                var existente = await db.Terceros.FirstOrDefaultAsync(
                    x => x.Legajo == cliente.Legajo.Trim().ToUpperInvariant() || x.NroDocumento == cliente.NroDocumento.Trim(), ct);

                if (existente is null)
                {
                    var result = await mediator.Send(new CreateTerceroCommand(
                        Legajo: cliente.Legajo,
                        RazonSocial: cliente.RazonSocial,
                        NombreFantasia: cliente.NombreFantasia,
                        TipoPersoneria: null,
                        Nombre: null,
                        Apellido: null,
                        Tratamiento: null,
                        Profesion: null,
                        EstadoPersonaId: null,
                        EstadoCivilId: null,
                        EstadoCivil: null,
                        Nacionalidad: null,
                        Sexo: null,
                        FechaNacimiento: null,
                        FechaRegistro: null,
                        EsEntidadGubernamental: false,
                        ClaveFiscal: null,
                        ValorClaveFiscal: null,
                        TipoDocumentoId: cliente.TipoDocumentoId,
                        NroDocumento: cliente.NroDocumento,
                        CondicionIvaId: cliente.CondicionIvaId,
                        EsCliente: cliente.EsCliente,
                        EsProveedor: cliente.EsProveedor,
                        EsEmpleado: cliente.EsEmpleado,
                        Calle: cliente.Calle,
                        Nro: cliente.Nro,
                        Piso: cliente.Piso,
                        Dpto: cliente.Dpto,
                        CodigoPostal: cliente.CodigoPostal,
                        PaisId: null,
                        ProvinciaId: null,
                        LocalidadId: cliente.LocalidadId,
                        BarrioId: cliente.BarrioId,
                        NroIngresosBrutos: cliente.NroIngresosBrutos,
                        NroMunicipal: cliente.NroMunicipal,
                        Telefono: cliente.Telefono,
                        Celular: cliente.Celular,
                        Email: cliente.Email,
                        Web: cliente.Web,
                        MonedaId: cliente.MonedaId,
                        CategoriaId: cliente.CategoriaId,
                        CategoriaClienteId: null,
                        EstadoClienteId: null,
                        CategoriaProveedorId: null,
                        EstadoProveedorId: null,
                        LimiteCredito: cliente.LimiteCredito,
                        PorcentajeMaximoDescuento: null,
                        VigenciaCreditoDesde: null,
                        VigenciaCreditoHasta: null,
                        Facturable: cliente.Facturable,
                        CobradorId: cliente.CobradorId,
                        AplicaComisionCobrador: false,
                        PctComisionCobrador: cliente.PctComisionCobrador,
                        VendedorId: cliente.VendedorId,
                        AplicaComisionVendedor: false,
                        PctComisionVendedor: cliente.PctComisionVendedor,
                        Observacion: cliente.Observacion,
                        SucursalId: cliente.SucursalId,
                        PerfilComercial: null,
                        Domicilios: null,
                        Contactos: null,
                        SucursalesEntrega: null,
                        Transportes: null,
                        VentanasCobranza: null), ct);

                    if (!result.IsSuccess)
                    {
                        procesoService.RegistrarError(job, result.Error);
                        await procesoService.RegistrarLogAsync(job.Id, Domain.Enums.NivelLogIntegracion.Error, result.Error ?? "Error al importar cliente.", cliente.Legajo, cliente.NroDocumento, ct);
                        continue;
                    }

                    procesoService.RegistrarExito(job);
                    await procesoService.RegistrarLogAsync(job.Id, Domain.Enums.NivelLogIntegracion.Informacion, "Cliente importado correctamente.", cliente.Legajo, result.Value.ToString(), ct);
                    continue;
                }

                if (!request.ActualizarExistentes)
                {
                    var mensaje = $"El cliente '{cliente.Legajo}' ya existe y la actualización está deshabilitada.";
                    procesoService.RegistrarError(job, mensaje);
                    await procesoService.RegistrarLogAsync(job.Id, Domain.Enums.NivelLogIntegracion.Advertencia, mensaje, cliente.Legajo, cliente.NroDocumento, ct);
                    continue;
                }

                var domicilio = Domicilio.Crear(cliente.Calle, cliente.Nro, cliente.Piso, cliente.Dpto, cliente.CodigoPostal, cliente.LocalidadId, cliente.BarrioId);
                existente.ActualizarPersoneriaFiscal(
                    Domain.Enums.TipoPersoneriaTercero.Juridica,
                    null,
                    null,
                    null,
                    null,
                    null,
                    null,
                    null,
                    null,
                    null,
                    false,
                    null,
                    null,
                    null);
                existente.Actualizar(
                    cliente.RazonSocial,
                    cliente.NombreFantasia,
                    cliente.CondicionIvaId,
                    cliente.Telefono,
                    cliente.Celular,
                    cliente.Email,
                    cliente.Web,
                    domicilio,
                    cliente.NroIngresosBrutos,
                    cliente.NroMunicipal,
                    cliente.LimiteCredito,
                    null,
                    null,
                    null,
                    cliente.Facturable,
                    cliente.CobradorId,
                    false,
                    cliente.PctComisionCobrador,
                    cliente.VendedorId,
                    false,
                    cliente.PctComisionVendedor,
                    cliente.Observacion,
                    null);
                existente.ActualizarRoles(cliente.EsCliente, cliente.EsProveedor, cliente.EsEmpleado, null);
                existente.SetMoneda(cliente.MonedaId);
                existente.SetCategoria(cliente.CategoriaId);
                existente.SetPais(null);
                existente.SetCategoriaCliente(null);
                existente.SetEstadoCliente(null);
                existente.SetCategoriaProveedor(null);
                existente.SetEstadoProveedor(null);
                existente.SetSucursal(cliente.SucursalId);
                repo.Update(existente);
                await uow.SaveChangesAsync(ct);

                procesoService.RegistrarExito(job);
                await procesoService.RegistrarLogAsync(job.Id, Domain.Enums.NivelLogIntegracion.Informacion, "Cliente actualizado correctamente.", cliente.Legajo, existente.Id.ToString(), ct);
            }

            procesoService.Finalizar(job, request.Observacion);
            await uow.SaveChangesAsync(ct);
            return Result.Success(job.Id);
        }
        catch (Exception ex)
        {
            procesoService.Fallar(job, ex.Message);
            await procesoService.RegistrarLogAsync(job.Id, Domain.Enums.NivelLogIntegracion.Error, ex.Message, null, null, ct);
            await uow.SaveChangesAsync(ct);
            return Result.Failure<long>(ex.Message);
        }
    }
}
