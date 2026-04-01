using MediatR;
using Microsoft.AspNetCore.Mvc;
using ZuluIA_Back.Application.Features.Terceros.Commands;
using ZuluIA_Back.Application.Features.Terceros.Queries;

namespace ZuluIA_Back.Api.Controllers;

public class TercerosController(IMediator mediator) : BaseController(mediator)
{
    // ─────────────────────────────────────────────────────────────────────────
    // QUERIES
    // ─────────────────────────────────────────────────────────────────────────

    /// <summary>
    /// Listado paginado de terceros con filtros combinables.
    /// Equivalente a la grilla del ABM de Clientes/Proveedores del VB6.
    /// GET /api/terceros?page=1&pageSize=20&search=garcia&soloClientes=true
    /// `soloActivos=true` devuelve solo activos; `soloActivos=false` devuelve todos.
    /// </summary>
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetAll(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        [FromQuery] string? search = null,
        [FromQuery] bool? soloClientes = null,
        [FromQuery] bool? soloProveedores = null,
        [FromQuery] bool? soloEmpleados = null,
        [FromQuery] bool? soloActivos = true,
        [FromQuery] long? condicionIvaId = null,
        [FromQuery] long? categoriaId = null,
        [FromQuery] long? estadoPersonaId = null,
        [FromQuery] long? categoriaClienteId = null,
        [FromQuery] long? estadoClienteId = null,
        [FromQuery] long? categoriaProveedorId = null,
        [FromQuery] long? estadoProveedorId = null,
        [FromQuery] long? sucursalId = null,
        CancellationToken ct = default)
    {
        var result = await Mediator.Send(
            new GetTercerosPagedQuery(
                page,
                pageSize,
                search,
                soloClientes,
                soloProveedores,
                soloEmpleados,
                soloActivos,
                condicionIvaId,
                categoriaId,
                estadoPersonaId,
                categoriaClienteId,
                estadoClienteId,
                categoriaProveedorId,
                estadoProveedorId,
                sucursalId),
            ct);

        return Ok(result);
    }

    /// <summary>
    /// Detalle completo de un tercero por Id.
    /// Equivalente a abrir el formulario de ABM al hacer doble click en una fila.
    /// GET /api/terceros/42
    /// </summary>
    [HttpGet("{id:long}", Name = "GetTerceroById")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(long id, CancellationToken ct)
    {
        var result = await Mediator.Send(new GetTerceroByIdQuery(id), ct);
        return FromResult(result);
    }

    /// <summary>
    /// Busca un tercero por su legajo (identificador de negocio).
    /// Equivalente a la búsqueda rápida por legajo en el VB6.
    /// GET /api/terceros/legajo/CLI001
    /// </summary>
    [HttpGet("legajo/{legajo}", Name = "GetTerceroByLegajo")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetByLegajo(string legajo, CancellationToken ct)
    {
        var result = await Mediator.Send(new GetTerceroByLegajoQuery(legajo), ct);
        return FromResult(result);
    }

    /// <summary>
    /// Busca un tercero por su número de documento (CUIT/DNI).
    /// Equivalente a la búsqueda rápida por documento en el VB6.
    /// GET /api/terceros/documento/20111111112
    /// </summary>
    [HttpGet("documento/{nroDocumento}", Name = "GetTerceroByNroDocumento")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetByNroDocumento(string nroDocumento, CancellationToken ct)
    {
        var result = await Mediator.Send(new GetTerceroByNroDocumentoQuery(nroDocumento), ct);
        return FromResult(result);
    }

    /// <summary>
    /// Selector optimizado de clientes para módulos de ventas.
    /// Retorna lista acotada con validaciones de cliente vendible.
    /// Equivalente al autocomplete/combo de cliente en Pedidos/Remitos/Facturas del VB6.
    /// GET /api/terceros/clientes/selector-ventas?search=garcia&sucursalId=1
    /// </summary>
    [HttpGet("clientes/selector-ventas")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetClientesSelectorVentas(
        [FromQuery] string? search = null,
        [FromQuery] long? sucursalId = null,
        [FromQuery] int maxResults = 50,
        CancellationToken ct = default)
    {
        var result = await Mediator.Send(
            new GetClientesSelectorVentasQuery(search, sucursalId, maxResults),
            ct);
        return Ok(result);
    }

    /// <summary>
    /// Retorna el perfil comercial ampliado del tercero.
    /// Equivalente al bloque comercial ampliado del ABM histórico.
    /// GET /api/terceros/42/perfil-comercial
    /// </summary>
    [HttpGet("{id:long}/perfil-comercial")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetPerfilComercial(long id, CancellationToken ct)
    {
        var result = await Mediator.Send(new GetTerceroPerfilComercialQuery(id), ct);
        return FromResult(result);
    }

    /// <summary>
    /// Retorna la configuración de cuenta corriente del tercero.
    /// GET /api/terceros/42/cuenta-corriente
    /// </summary>
    [HttpGet("{id:long}/cuenta-corriente")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetCuentaCorriente(long id, CancellationToken ct)
    {
        var result = await Mediator.Send(new GetTerceroCuentaCorrienteQuery(id), ct);
        return FromResult(result);
    }

    /// <summary>
    /// Retorna el usuario vinculado al tercero, si existe.
    /// GET /api/terceros/42/usuario
    /// </summary>
    [HttpGet("{id:long}/usuario")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetUsuarioCliente(long id, CancellationToken ct)
    {
        var result = await Mediator.Send(new GetTerceroUsuarioClienteQuery(id), ct);
        return FromResult(result);
    }

    /// <summary>
    /// Retorna los contactos múltiples del tercero.
    /// GET /api/terceros/42/contactos
    /// </summary>
    [HttpGet("{id:long}/contactos")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetContactos(long id, CancellationToken ct)
    {
        var result = await Mediator.Send(new GetTerceroContactosQuery(id), ct);
        return FromResult(result);
    }

    /// <summary>
    /// Retorna los medios de contacto legacy del tercero.
    /// GET /api/terceros/42/medios-contacto
    /// </summary>
    [HttpGet("{id:long}/medios-contacto")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetMediosContacto(long id, CancellationToken ct)
    {
        var result = await Mediator.Send(new GetTerceroMediosContactoQuery(id), ct);
        return FromResult(result);
    }

    /// <summary>
    /// Retorna los domicilios legacy del tercero.
    /// GET /api/terceros/42/domicilios
    /// </summary>
    [HttpGet("{id:long}/domicilios")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetDomicilios(long id, CancellationToken ct)
    {
        var result = await Mediator.Send(new GetTerceroDomiciliosQuery(id), ct);
        return FromResult(result);
    }

    /// <summary>
    /// Retorna las sucursales / puntos de entrega del tercero.
    /// GET /api/terceros/42/sucursales-entrega
    /// </summary>
    [HttpGet("{id:long}/sucursales-entrega")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetSucursalesEntrega(long id, CancellationToken ct)
    {
        var result = await Mediator.Send(new GetTerceroSucursalesEntregaQuery(id), ct);
        return FromResult(result);
    }

    /// <summary>
    /// Retorna los transportes vinculados al tercero.
    /// GET /api/terceros/42/transportes
    /// </summary>
    [HttpGet("{id:long}/transportes")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetTransportes(long id, CancellationToken ct)
    {
        var result = await Mediator.Send(new GetTerceroTransportesQuery(id), ct);
        return FromResult(result);
    }

    /// <summary>
    /// Retorna las ventanas de cobranza del tercero.
    /// GET /api/terceros/42/ventanas-cobranza
    /// </summary>
    [HttpGet("{id:long}/ventanas-cobranza")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetVentanasCobranza(long id, CancellationToken ct)
    {
        var result = await Mediator.Send(new GetTerceroVentanasCobranzaQuery(id), ct);
        return FromResult(result);
    }

    /// <summary>
    /// Lista de clientes activos para combos y selectores.
    /// Equivalente al llenarComboClientes() del VB6.
    /// GET /api/terceros/clientes-activos?sucursalId=1
    /// </summary>
    [HttpGet("clientes-activos")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetClientesActivos(
        [FromQuery] long? sucursalId = null,
        CancellationToken ct = default)
    {
        var result = await Mediator.Send(
            new GetClientesActivosQuery(sucursalId), ct);
        return Ok(result);
    }

    /// <summary>
    /// Lista de proveedores activos para combos y selectores.
    /// Equivalente al llenarComboProveedores() del VB6.
    /// GET /api/terceros/proveedores-activos?sucursalId=1
    /// </summary>
    [HttpGet("proveedores-activos")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetProveedoresActivos(
        [FromQuery] long? sucursalId = null,
        CancellationToken ct = default)
    {
        var result = await Mediator.Send(
            new GetProveedoresActivosQuery(sucursalId), ct);
        return Ok(result);
    }

    /// <summary>
    /// Retorna en una sola respuesta los catálogos legacy de terceros.
    /// GET /api/terceros/catalogos?soloActivos=true
    /// </summary>
    [HttpGet("catalogos")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetCatalogos(
        [FromQuery] bool soloActivos = false,
        CancellationToken ct = default)
    {
        var result = await Mediator.Send(new GetCatalogosTercerosQuery(soloActivos), ct);
        return Ok(result);
    }

    /// <summary>
    /// Retorna en una sola respuesta la configuración base de la ficha de clientes.
    /// GET /api/terceros/configuracion-clientes?soloActivos=true
    /// </summary>
    [HttpGet("configuracion-clientes")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetConfiguracionClientes(
        [FromQuery] bool soloActivos = true,
        CancellationToken ct = default)
    {
        var result = await Mediator.Send(new GetConfiguracionClientesTercerosQuery(soloActivos), ct);
        return Ok(result);
    }

    /// <summary>
    /// Retorna el catálogo de categorías de clientes.
    /// Equivalente al mantenimiento auxiliar usado por el ABM legacy.
    /// GET /api/terceros/categorias-clientes?soloActivas=true
    /// </summary>
    [HttpGet("categorias-clientes")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetCategoriasClientes(
        [FromQuery] bool soloActivas = false,
        CancellationToken ct = default)
    {
        var result = await Mediator.Send(new GetCategoriasClientesQuery(soloActivas), ct);
        return Ok(result);
    }

    /// <summary>
    /// Retorna el catálogo de categorías de proveedores.
    /// GET /api/terceros/categorias-proveedores?soloActivas=true
    /// </summary>
    [HttpGet("categorias-proveedores")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetCategoriasProveedores(
        [FromQuery] bool soloActivas = false,
        CancellationToken ct = default)
    {
        var result = await Mediator.Send(new GetCategoriasProveedoresQuery(soloActivas), ct);
        return Ok(result);
    }

    /// <summary>
    /// Retorna el catálogo de estados de clientes.
    /// GET /api/terceros/estados-clientes?soloActivos=true
    /// </summary>
    [HttpGet("estados-clientes")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetEstadosClientes(
        [FromQuery] bool soloActivos = false,
        CancellationToken ct = default)
    {
        var result = await Mediator.Send(new GetEstadosClientesQuery(soloActivos), ct);
        return Ok(result);
    }

    /// <summary>
    /// Retorna el catálogo de estados de proveedores.
    /// GET /api/terceros/estados-proveedores?soloActivos=true
    /// </summary>
    [HttpGet("estados-proveedores")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetEstadosProveedores(
        [FromQuery] bool soloActivos = false,
        CancellationToken ct = default)
    {
        var result = await Mediator.Send(new GetEstadosProveedoresQuery(soloActivos), ct);
        return Ok(result);
    }

    /// <summary>
    /// Retorna el catálogo de estados generales legacy.
    /// GET /api/terceros/estados-personas?soloActivos=true
    /// </summary>
    [HttpGet("estados-personas")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetEstadosPersonas(
        [FromQuery] bool soloActivos = false,
        CancellationToken ct = default)
    {
        var result = await Mediator.Send(new GetEstadosPersonasQuery(soloActivos), ct);
        return Ok(result);
    }

    /// <summary>
    /// Retorna el catálogo de estados civiles para personas físicas.
    /// GET /api/terceros/estados-civiles?soloActivos=true
    /// </summary>
    [HttpGet("estados-civiles")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetEstadosCiviles(
        [FromQuery] bool soloActivos = false,
        CancellationToken ct = default)
    {
        var result = await Mediator.Send(new GetEstadosCivilesQuery(soloActivos), ct);
        return Ok(result);
    }

    /// <summary>
    /// Retorna el catálogo de tipos de domicilio legacy.
    /// GET /api/terceros/tipos-domicilio
    /// </summary>
    [HttpGet("tipos-domicilio")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetTiposDomicilio(CancellationToken ct = default)
    {
        var result = await Mediator.Send(new GetTiposDomicilioQuery(), ct);
        return Ok(result);
    }

    /// <summary>
    /// Alias histórico para crear una categoría de cliente desde el módulo de terceros.
    /// POST /api/terceros/categorias-clientes
    /// </summary>
    [HttpPost("categorias-clientes")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> CreateCategoriaCliente(
        [FromBody] CategoriaClienteRequest request,
        CancellationToken ct)
    {
        var result = await Mediator.Send(new CreateCategoriaClienteCommand(request.Codigo, request.Descripcion), ct);
        if (result.IsFailure)
            return result.Error?.Contains("ya existe", StringComparison.OrdinalIgnoreCase) == true
                ? Conflict(new { error = result.Error })
                : BadRequest(new { error = result.Error });

        return CreatedAtAction(nameof(GetCategoriasClientes), new { }, new { id = result.Value });
    }

    /// <summary>
    /// Alias histórico para crear un estado de cliente desde el módulo de terceros.
    /// POST /api/terceros/estados-clientes
    /// </summary>
    [HttpPost("estados-clientes")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> CreateEstadoCliente(
        [FromBody] EstadoClienteRequest request,
        CancellationToken ct)
    {
        var result = await Mediator.Send(new CreateEstadoClienteCommand(request.Codigo, request.Descripcion, request.Bloquea), ct);
        if (result.IsFailure)
            return result.Error?.Contains("ya existe", StringComparison.OrdinalIgnoreCase) == true
                ? Conflict(new { error = result.Error })
                : BadRequest(new { error = result.Error });

        return CreatedAtAction(nameof(GetEstadosClientes), new { }, new { id = result.Value });
    }

    /// <summary>
    /// Alias histórico para crear una categoría de proveedor desde el módulo de terceros.
    /// POST /api/terceros/categorias-proveedores
    /// </summary>
    [HttpPost("categorias-proveedores")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> CreateCategoriaProveedor(
        [FromBody] CategoriaProveedorRequest request,
        CancellationToken ct)
    {
        var result = await Mediator.Send(new CreateCategoriaProveedorCommand(request.Codigo, request.Descripcion), ct);
        if (result.IsFailure)
            return result.Error?.Contains("ya existe", StringComparison.OrdinalIgnoreCase) == true
                ? Conflict(new { error = result.Error })
                : BadRequest(new { error = result.Error });

        return CreatedAtAction(nameof(GetCategoriasProveedores), new { }, new { id = result.Value });
    }

    /// <summary>
    /// Alias histórico para crear un estado de proveedor desde el módulo de terceros.
    /// POST /api/terceros/estados-proveedores
    /// </summary>
    [HttpPost("estados-proveedores")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> CreateEstadoProveedor(
        [FromBody] EstadoProveedorRequest request,
        CancellationToken ct)
    {
        var result = await Mediator.Send(new CreateEstadoProveedorCommand(request.Codigo, request.Descripcion, request.Bloquea), ct);
        if (result.IsFailure)
            return result.Error?.Contains("ya existe", StringComparison.OrdinalIgnoreCase) == true
                ? Conflict(new { error = result.Error })
                : BadRequest(new { error = result.Error });

        return CreatedAtAction(nameof(GetEstadosProveedores), new { }, new { id = result.Value });
    }

    /// <summary>
    /// Alias histórico para crear un estado civil desde el módulo de terceros.
    /// POST /api/terceros/estados-civiles
    /// </summary>
    [HttpPost("estados-civiles")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> CreateEstadoCivil(
        [FromBody] EstadoCivilCatalogoRequest request,
        CancellationToken ct)
    {
        var result = await Mediator.Send(new CreateEstadoCivilCommand(request.Descripcion), ct);
        if (result.IsFailure)
            return result.Error?.Contains("ya existe", StringComparison.OrdinalIgnoreCase) == true
                ? Conflict(new { error = result.Error })
                : BadRequest(new { error = result.Error });

        return CreatedAtAction(nameof(GetEstadosCiviles), new { }, new { id = result.Value });
    }

    // ─────────────────────────────────────────────────────────────────────────
    // COMMANDS
    // ─────────────────────────────────────────────────────────────────────────

    /// <summary>
    /// Crea un nuevo tercero.
    /// Equivalente al agregarNuevo() → Guardar() del VB6.
    /// POST /api/terceros
    /// Retorna 201 Created con header Location: /api/terceros/{id}
    /// </summary>
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create(
        [FromBody] CreateTerceroCommand command,
        CancellationToken ct)
    {
        var result = await Mediator.Send(command, ct);
        return CreatedFromResult(
            result,
            "GetTerceroById",
            new { id = result.IsSuccess ? result.Value : 0 });
    }

    /// <summary>
    /// Actualiza los datos de un tercero existente.
    /// Equivalente al Guardar() en modo edición del VB6.
    /// PUT /api/terceros/42
    /// </summary>
    [HttpPut("{id:long}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(
        long id,
        [FromBody] UpdateTerceroCommand command,
        CancellationToken ct)
    {
        // Guardia: el Id de la URL debe coincidir con el del body.
        // Patrón del proyecto (ver ItemsController).
        if (id != command.Id)
            return BadRequest(new { error = "El Id de la URL no coincide con el del body." });

        var result = await Mediator.Send(command, ct);
        return FromResult(result);
    }

    /// <summary>
    /// Crea o actualiza el usuario vinculado al tercero.
    /// PUT /api/terceros/42/usuario
    /// </summary>
    [HttpPut("{id:long}/usuario")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpsertUsuarioCliente(
        long id,
        [FromBody] UpsertTerceroUsuarioClienteCommand command,
        CancellationToken ct)
    {
        if (id != command.TerceroId)
            return BadRequest(new { error = "El Id de la URL no coincide con el del body." });

        var result = await Mediator.Send(command, ct);
        return FromResult(result);
    }

    /// <summary>
    /// Elimina el usuario vinculado al tercero desactivándolo y quitando el vínculo.
    /// DELETE /api/terceros/42/usuario
    /// </summary>
    [HttpDelete("{id:long}/usuario")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> RemoveUsuarioCliente(long id, CancellationToken ct)
    {
        var result = await Mediator.Send(new RemoveTerceroUsuarioClienteCommand(id), ct);
        return FromResult(result);
    }

    /// <summary>
    /// Reemplaza los permisos básicos del usuario vinculado al tercero.
    /// PUT /api/terceros/42/usuario/permisos
    /// </summary>
    [HttpPut("{id:long}/usuario/permisos")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> SetUsuarioClientePermisos(
        long id,
        [FromBody] SetTerceroUsuarioClientePermisosCommand command,
        CancellationToken ct)
    {
        if (id != command.TerceroId)
            return BadRequest(new { error = "El Id de la URL no coincide con el del body." });

        var result = await Mediator.Send(command, ct);
        return FromResult(result);
    }

    /// <summary>
    /// Actualiza los parámetros operativos básicos del usuario vinculado al tercero.
    /// PUT /api/terceros/42/usuario/parametros-basicos
    /// </summary>
    [HttpPut("{id:long}/usuario/parametros-basicos")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> SetUsuarioClienteParametrosBasicos(
        long id,
        [FromBody] SetTerceroUsuarioClienteParametrosBasicosCommand command,
        CancellationToken ct)
    {
        if (id != command.TerceroId)
            return BadRequest(new { error = "El Id de la URL no coincide con el del body." });

        var result = await Mediator.Send(command, ct);
        return FromResult(result);
    }

    /// <summary>
    /// Da de baja lógica (soft delete) a un tercero.
    /// Equivalente al eliminar() + validarEliminar() del VB6.
    /// DELETE /api/terceros/42
    /// Retorna 400 con mensaje si tiene comprobantes, cuenta corriente o empleado activo.
    /// </summary>
    [HttpDelete("{id:long}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(long id, CancellationToken ct)
    {
        var result = await Mediator.Send(new DeleteTerceroCommand(id), ct);
        return FromResult(result);
    }

    /// <summary>
    /// Reactiva un tercero dado de baja lógica.
    /// No existía en VB6 (se hacía manual en BD),
    /// pero es necesario para el flujo de administración.
    /// PATCH /api/terceros/42/activar
    /// </summary>
    [HttpPatch("{id:long}/activar")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Activar(long id, CancellationToken ct)
    {
        var result = await Mediator.Send(new ActivarTerceroCommand(id), ct);
        return FromResult(result);
    }

    /// <summary>
    /// Crea o actualiza el perfil comercial ampliado del tercero.
    /// PUT /api/terceros/42/perfil-comercial
    /// </summary>
    [HttpPut("{id:long}/perfil-comercial")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpsertPerfilComercial(
        long id,
        [FromBody] UpsertTerceroPerfilComercialCommand command,
        CancellationToken ct)
    {
        if (id != command.TerceroId)
            return BadRequest(new { error = "El Id de la URL no coincide con el del body." });

        var result = await Mediator.Send(command, ct);
        return FromResult(result);
    }

    /// <summary>
    /// Crea o actualiza la configuración de cuenta corriente del tercero.
    /// PUT /api/terceros/42/cuenta-corriente
    /// </summary>
    [HttpPut("{id:long}/cuenta-corriente")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpsertCuentaCorriente(
        long id,
        [FromBody] UpsertTerceroCuentaCorrienteCommand command,
        CancellationToken ct)
    {
        if (id != command.TerceroId)
            return BadRequest(new { error = "El Id de la URL no coincide con el del body." });

        var result = await Mediator.Send(command, ct);
        return FromResult(result);
    }

    /// <summary>
    /// Reemplaza la colección de contactos múltiples del tercero.
    /// PUT /api/terceros/42/contactos
    /// </summary>
    [HttpPut("{id:long}/contactos")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ReplaceContactos(
        long id,
        [FromBody] ReplaceTerceroContactosCommand command,
        CancellationToken ct)
    {
        if (id != command.TerceroId)
            return BadRequest(new { error = "El Id de la URL no coincide con el del body." });

        var result = await Mediator.Send(command, ct);
        return FromResult(result);
    }

    /// <summary>
    /// Reemplaza la colección de domicilios legacy del tercero.
    /// PUT /api/terceros/42/domicilios
    /// </summary>
    [HttpPut("{id:long}/domicilios")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ReplaceDomicilios(
        long id,
        [FromBody] ReplaceTerceroDomiciliosCommand command,
        CancellationToken ct)
    {
        if (id != command.TerceroId)
            return BadRequest(new { error = "El Id de la URL no coincide con el del body." });

        var result = await Mediator.Send(command, ct);
        return FromResult(result);
    }

    /// <summary>
    /// Reemplaza la colección de sucursales / puntos de entrega del tercero.
    /// PUT /api/terceros/42/sucursales-entrega
    /// </summary>
    [HttpPut("{id:long}/sucursales-entrega")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ReplaceSucursalesEntrega(
        long id,
        [FromBody] ReplaceTerceroSucursalesEntregaCommand command,
        CancellationToken ct)
    {
        if (id != command.TerceroId)
            return BadRequest(new { error = "El Id de la URL no coincide con el del body." });

        var result = await Mediator.Send(command, ct);
        return FromResult(result);
    }

    /// <summary>
    /// Reemplaza la colección de transportes vinculados al tercero.
    /// PUT /api/terceros/42/transportes
    /// </summary>
    [HttpPut("{id:long}/transportes")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ReplaceTransportes(
        long id,
        [FromBody] ReplaceTerceroTransportesCommand command,
        CancellationToken ct)
    {
        if (id != command.TerceroId)
            return BadRequest(new { error = "El Id de la URL no coincide con el del body." });

        var result = await Mediator.Send(command, ct);
        return FromResult(result);
    }

    /// <summary>
    /// Reemplaza la colección de ventanas de cobranza del tercero.
    /// PUT /api/terceros/42/ventanas-cobranza
    /// </summary>
    [HttpPut("{id:long}/ventanas-cobranza")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ReplaceVentanasCobranza(
        long id,
        [FromBody] ReplaceTerceroVentanasCobranzaCommand command,
        CancellationToken ct)
    {
        if (id != command.TerceroId)
            return BadRequest(new { error = "El Id de la URL no coincide con el del body." });

        var result = await Mediator.Send(command, ct);
        return FromResult(result);
    }

    /// <summary>
    /// Crea una categoría de cliente.
    /// POST /api/terceros/categorias-clientes
    /// </summary>
    [NonAction]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CreateCategoriaCliente(
        [FromBody] CreateCategoriaClienteCommand command,
        CancellationToken ct)
    {
        var result = await Mediator.Send(command, ct);
        return result.IsSuccess
            ? StatusCode(StatusCodes.Status201Created, new { id = result.Value })
            : BadRequest(new { error = result.Error });
    }

    /// <summary>
    /// Actualiza una categoría de cliente.
    /// PUT /api/terceros/categorias-clientes/1
    /// </summary>
    [HttpPut("categorias-clientes/{id:long}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> UpdateCategoriaCliente(
        long id,
        [FromBody] UpdateCategoriaClienteCommand command,
        CancellationToken ct)
    {
        if (id != command.Id)
            return BadRequest(new { error = "El Id de la URL no coincide con el del body." });

        var result = await Mediator.Send(command, ct);
        return FromResult(result);
    }

    /// <summary>
    /// Activa una categoría de cliente.
    /// PATCH /api/terceros/categorias-clientes/1/activar
    /// </summary>
    [HttpPatch("categorias-clientes/{id:long}/activar")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> ActivateCategoriaCliente(long id, CancellationToken ct)
    {
        var result = await Mediator.Send(new ActivateCategoriaClienteCommand(id), ct);
        return FromResult(result);
    }

    /// <summary>
    /// Desactiva una categoría de cliente.
    /// PATCH /api/terceros/categorias-clientes/1/desactivar
    /// </summary>
    [HttpPatch("categorias-clientes/{id:long}/desactivar")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> DeactivateCategoriaCliente(long id, CancellationToken ct)
    {
        var result = await Mediator.Send(new DeactivateCategoriaClienteCommand(id), ct);
        return FromResult(result);
    }

    /// <summary>
    /// Crea una categoría de proveedor.
    /// POST /api/terceros/categorias-proveedores
    /// </summary>
    [NonAction]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CreateCategoriaProveedor(
        [FromBody] CreateCategoriaProveedorCommand command,
        CancellationToken ct)
    {
        var result = await Mediator.Send(command, ct);
        return result.IsSuccess
            ? StatusCode(StatusCodes.Status201Created, new { id = result.Value })
            : BadRequest(new { error = result.Error });
    }

    /// <summary>
    /// Actualiza una categoría de proveedor.
    /// PUT /api/terceros/categorias-proveedores/1
    /// </summary>
    [HttpPut("categorias-proveedores/{id:long}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> UpdateCategoriaProveedor(
        long id,
        [FromBody] UpdateCategoriaProveedorCommand command,
        CancellationToken ct)
    {
        if (id != command.Id)
            return BadRequest(new { error = "El Id de la URL no coincide con el del body." });

        var result = await Mediator.Send(command, ct);
        return FromResult(result);
    }

    /// <summary>
    /// Activa una categoría de proveedor.
    /// PATCH /api/terceros/categorias-proveedores/1/activar
    /// </summary>
    [HttpPatch("categorias-proveedores/{id:long}/activar")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> ActivateCategoriaProveedor(long id, CancellationToken ct)
    {
        var result = await Mediator.Send(new ActivateCategoriaProveedorCommand(id), ct);
        return FromResult(result);
    }

    /// <summary>
    /// Desactiva una categoría de proveedor.
    /// PATCH /api/terceros/categorias-proveedores/1/desactivar
    /// </summary>
    [HttpPatch("categorias-proveedores/{id:long}/desactivar")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> DeactivateCategoriaProveedor(long id, CancellationToken ct)
    {
        var result = await Mediator.Send(new DeactivateCategoriaProveedorCommand(id), ct);
        return FromResult(result);
    }

    /// <summary>
    /// Crea un estado de cliente.
    /// POST /api/terceros/estados-clientes
    /// </summary>
    [NonAction]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CreateEstadoCliente(
        [FromBody] CreateEstadoClienteCommand command,
        CancellationToken ct)
    {
        var result = await Mediator.Send(command, ct);
        return result.IsSuccess
            ? StatusCode(StatusCodes.Status201Created, new { id = result.Value })
            : BadRequest(new { error = result.Error });
    }

    /// <summary>
    /// Actualiza un estado de cliente.
    /// PUT /api/terceros/estados-clientes/1
    /// </summary>
    [HttpPut("estados-clientes/{id:long}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> UpdateEstadoCliente(
        long id,
        [FromBody] UpdateEstadoClienteCommand command,
        CancellationToken ct)
    {
        if (id != command.Id)
            return BadRequest(new { error = "El Id de la URL no coincide con el del body." });

        var result = await Mediator.Send(command, ct);
        return FromResult(result);
    }

    /// <summary>
    /// Activa un estado de cliente.
    /// PATCH /api/terceros/estados-clientes/1/activar
    /// </summary>
    [HttpPatch("estados-clientes/{id:long}/activar")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> ActivateEstadoCliente(long id, CancellationToken ct)
    {
        var result = await Mediator.Send(new ActivateEstadoClienteCommand(id), ct);
        return FromResult(result);
    }

    /// <summary>
    /// Desactiva un estado de cliente.
    /// PATCH /api/terceros/estados-clientes/1/desactivar
    /// </summary>
    [HttpPatch("estados-clientes/{id:long}/desactivar")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> DeactivateEstadoCliente(long id, CancellationToken ct)
    {
        var result = await Mediator.Send(new DeactivateEstadoClienteCommand(id), ct);
        return FromResult(result);
    }

    /// <summary>
    /// Crea un estado de proveedor.
    /// POST /api/terceros/estados-proveedores
    /// </summary>
    [NonAction]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CreateEstadoProveedor(
        [FromBody] CreateEstadoProveedorCommand command,
        CancellationToken ct)
    {
        var result = await Mediator.Send(command, ct);
        return result.IsSuccess
            ? StatusCode(StatusCodes.Status201Created, new { id = result.Value })
            : BadRequest(new { error = result.Error });
    }

    /// <summary>
    /// Actualiza un estado de proveedor.
    /// PUT /api/terceros/estados-proveedores/1
    /// </summary>
    [HttpPut("estados-proveedores/{id:long}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> UpdateEstadoProveedor(
        long id,
        [FromBody] UpdateEstadoProveedorCommand command,
        CancellationToken ct)
    {
        if (id != command.Id)
            return BadRequest(new { error = "El Id de la URL no coincide con el del body." });

        var result = await Mediator.Send(command, ct);
        return FromResult(result);
    }

    /// <summary>
    /// Activa un estado de proveedor.
    /// PATCH /api/terceros/estados-proveedores/1/activar
    /// </summary>
    [HttpPatch("estados-proveedores/{id:long}/activar")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> ActivateEstadoProveedor(long id, CancellationToken ct)
    {
        var result = await Mediator.Send(new ActivateEstadoProveedorCommand(id), ct);
        return FromResult(result);
    }

    /// <summary>
    /// Desactiva un estado de proveedor.
    /// PATCH /api/terceros/estados-proveedores/1/desactivar
    /// </summary>
    [HttpPatch("estados-proveedores/{id:long}/desactivar")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> DeactivateEstadoProveedor(long id, CancellationToken ct)
    {
        var result = await Mediator.Send(new DeactivateEstadoProveedorCommand(id), ct);
        return FromResult(result);
    }
}