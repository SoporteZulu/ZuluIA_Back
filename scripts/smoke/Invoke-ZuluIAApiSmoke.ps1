param(
    [string]$BaseUrl = 'https://localhost:5001',
    [string]$BootstrapSqlPath = 'database\zuluia_back_full_compatible.sql',
    [string]$SupplementalSqlPath = 'database\zuluia_back_smoke_dataset.sql',
    [switch]$ApplyBootstrap,
    [switch]$ApplySupplementalDataset,
    [string]$PsqlPath = 'psql',
    [string]$DbHost = 'localhost',
    [int]$DbPort = 5432,
    [string]$DbName = 'zuluia_back',
    [string]$DbUser = 'postgres',
    [string]$DbPassword = '',
    [string]$Token = '',
    [string]$JwtSecret = '',
    [string]$JwtIssuer = 'http://localhost',
    [string]$JwtAudience = 'authenticated',
    [string]$ReportPath = 'artifacts\zuluia-api-smoke-report.json'
)

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'

if (-not $JwtSecret) {
    $JwtSecret = $env:SUPABASE_JWT_SECRET
}

$script:Results = New-Object System.Collections.Generic.List[object]
$script:HttpClient = [System.Net.Http.HttpClient]::new()
$script:HttpClient.Timeout = [TimeSpan]::FromSeconds(60)

function Write-Step([string]$Message) {
    Write-Host "`n==> $Message" -ForegroundColor Cyan
}

function ConvertTo-Base64Url([byte[]]$Bytes) {
    [Convert]::ToBase64String($Bytes).TrimEnd('=') -replace '\+', '-' -replace '/', '_'
}

function New-HmacJwt {
    param(
        [Parameter(Mandatory = $true)][string]$Secret,
        [string]$Issuer,
        [string]$Audience,
        [long]$Subject = 1,
        [string]$Email = 'admin@zuluia.local',
        [int]$ValidHours = 8
    )

    $now = [DateTimeOffset]::UtcNow
    $header = @{ alg = 'HS256'; typ = 'JWT' }
    $payload = @{
        sub = "$Subject"
        email = $Email
        role = 'authenticated'
        iss = $Issuer
        aud = $Audience
        iat = [int]$now.ToUnixTimeSeconds()
        exp = [int]$now.AddHours($ValidHours).ToUnixTimeSeconds()
    }

    $encodedHeader = ConvertTo-Base64Url ([System.Text.Encoding]::UTF8.GetBytes(($header | ConvertTo-Json -Compress)))
    $encodedPayload = ConvertTo-Base64Url ([System.Text.Encoding]::UTF8.GetBytes(($payload | ConvertTo-Json -Compress)))
    $unsigned = "$encodedHeader.$encodedPayload"

    $hmac = [System.Security.Cryptography.HMACSHA256]::new([System.Text.Encoding]::UTF8.GetBytes($Secret))
    try {
        $signature = ConvertTo-Base64Url ($hmac.ComputeHash([System.Text.Encoding]::UTF8.GetBytes($unsigned)))
    }
    finally {
        $hmac.Dispose()
    }

    return "$unsigned.$signature"
}

function Test-PsqlAvailable {
    try {
        Get-Command $PsqlPath -ErrorAction Stop | Out-Null
        return $true
    }
    catch {
        return $false
    }
}

function Invoke-PsqlFile([string]$FilePath) {
    if (-not (Test-Path $FilePath)) {
        throw "No existe el archivo SQL: $FilePath"
    }

    $env:PGPASSWORD = $DbPassword
    try {
        & $PsqlPath -h $DbHost -p $DbPort -U $DbUser -d $DbName -v ON_ERROR_STOP=1 -f $FilePath
        if ($LASTEXITCODE -ne 0) {
            throw "psql devolvió código $LASTEXITCODE al ejecutar $FilePath"
        }
    }
    finally {
        Remove-Item Env:PGPASSWORD -ErrorAction SilentlyContinue
    }
}

function Invoke-PsqlScalar([string]$Sql) {
    $env:PGPASSWORD = $DbPassword
    try {
        $output = & $PsqlPath -h $DbHost -p $DbPort -U $DbUser -d $DbName -v ON_ERROR_STOP=1 -Atqc $Sql
        if ($LASTEXITCODE -ne 0) {
            throw "psql devolvió código $LASTEXITCODE al ejecutar consulta."
        }

        return ($output | Select-Object -First 1).Trim()
    }
    finally {
        Remove-Item Env:PGPASSWORD -ErrorAction SilentlyContinue
    }
}

function Invoke-PsqlNonQuery([string]$Sql) {
    $env:PGPASSWORD = $DbPassword
    try {
        & $PsqlPath -h $DbHost -p $DbPort -U $DbUser -d $DbName -v ON_ERROR_STOP=1 -qc $Sql
        if ($LASTEXITCODE -ne 0) {
            throw "psql devolvió código $LASTEXITCODE al ejecutar sentencia."
        }
    }
    finally {
        Remove-Item Env:PGPASSWORD -ErrorAction SilentlyContinue
    }
}

function Try-ResolveLong([string]$Sql) {
    try {
        $value = Invoke-PsqlScalar $Sql
        if ([string]::IsNullOrWhiteSpace($value)) { return $null }
        return [long]$value
    }
    catch {
        return $null
    }
}

function Add-Result {
    param(
        [string]$Name,
        [string]$Method,
        [string]$Path,
        [int]$StatusCode,
        [bool]$Success,
        [object]$Response,
        [string]$ErrorMessage = ''
    )

    $script:Results.Add([pscustomobject]@{
        Name = $Name
        Method = $Method
        Path = $Path
        StatusCode = $StatusCode
        Success = $Success
        Error = $ErrorMessage
        Response = $Response
        TimestampUtc = [DateTime]::UtcNow
    }) | Out-Null
}

function Invoke-Api {
    param(
        [Parameter(Mandatory = $true)][string]$Name,
        [Parameter(Mandatory = $true)][string]$Method,
        [Parameter(Mandatory = $true)][string]$Path,
        [object]$Body = $null,
        [switch]$Anonymous,
        [switch]$AllowFailure
    )

    $uri = if ($Path.StartsWith('http')) { $Path } else { "$($BaseUrl.TrimEnd('/'))$Path" }
    $request = [System.Net.Http.HttpRequestMessage]::new([System.Net.Http.HttpMethod]::new($Method.ToUpperInvariant()), $uri)
    $request.Headers.Accept.Add([System.Net.Http.Headers.MediaTypeWithQualityHeaderValue]::new('application/json'))

    if (-not $Anonymous) {
        if (-not $Token) {
            throw 'No hay token configurado para requests autenticados.'
        }
        $request.Headers.Authorization = [System.Net.Http.Headers.AuthenticationHeaderValue]::new('Bearer', $Token)
    }

    if ($null -ne $Body) {
        $json = $Body | ConvertTo-Json -Depth 40
        $request.Content = [System.Net.Http.StringContent]::new($json, [System.Text.Encoding]::UTF8, 'application/json')
    }

    $response = $null
    try {
        $response = $script:HttpClient.SendAsync($request).GetAwaiter().GetResult()
        $raw = $response.Content.ReadAsStringAsync().GetAwaiter().GetResult()
        $parsed = $raw
        if (-not [string]::IsNullOrWhiteSpace($raw)) {
            try { $parsed = $raw | ConvertFrom-Json } catch { }
        }

        $ok = [int]$response.StatusCode -ge 200 -and [int]$response.StatusCode -lt 300
        Add-Result -Name $Name -Method $Method -Path $Path -StatusCode ([int]$response.StatusCode) -Success $ok -Response $parsed

        if (-not $ok -and -not $AllowFailure) {
            throw "[$([int]$response.StatusCode)] $Name falló en $Path"
        }

        return [pscustomobject]@{
            StatusCode = [int]$response.StatusCode
            IsSuccess = $ok
            Data = $parsed
            Raw = $raw
        }
    }
    catch {
        Add-Result -Name $Name -Method $Method -Path $Path -StatusCode 0 -Success $false -Response $null -ErrorMessage $_.Exception.Message
        if (-not $AllowFailure) {
            throw
        }

        return [pscustomobject]@{
            StatusCode = 0
            IsSuccess = $false
            Data = $null
            Raw = ''
        }
    }
    finally {
        $request.Dispose()
        if ($null -ne $response) { $response.Dispose() }
    }
}

function Resolve-Ids {
    [pscustomobject]@{
        SucursalId = [long](Invoke-PsqlScalar "select id from sucursales where cuit = '30712345678' limit 1")
        PuntoFacturacionId = [long](Invoke-PsqlScalar "select pf.id from puntos_facturacion pf join sucursales s on s.id = pf.sucursal_id where s.cuit = '30712345678' and pf.numero = 1 limit 1")
        TipoPuntoManualId = [long](Invoke-PsqlScalar "select id from tipos_punto_facturacion where descripcion = 'MANUAL' limit 1")
        CajaPrincipalId = [long](Invoke-PsqlScalar "select c.id from cajas_cuentas_bancarias c join sucursales s on s.id = c.sucursal_id where s.cuit = '30712345678' and c.descripcion = 'CAJA PRINCIPAL' limit 1")
        TransportistaRegistroId = [long](Invoke-PsqlScalar "select id from transportistas order by id limit 1")
        TipoCajaId = [long](Invoke-PsqlScalar "select id from tipos_caja_cuenta where descripcion = 'CAJA' limit 1")
        MonedaArsId = [long](Invoke-PsqlScalar "select id from monedas where codigo = 'ARS' limit 1")
        PaisId = [long](Invoke-PsqlScalar "select id from paises where codigo = 'AR' limit 1")
        ProvinciaId = [long](Invoke-PsqlScalar "select id from provincias where codigo = 'AR-MI' limit 1")
        LocalidadId = [long](Invoke-PsqlScalar "select id from localidades where descripcion = 'POSADAS' limit 1")
        BarrioId = [long](Invoke-PsqlScalar "select id from barrios where descripcion = 'CENTRO' limit 1")
        CondicionIvaRiId = [long](Invoke-PsqlScalar "select id from condiciones_iva where codigo = 1 limit 1")
        CondicionIvaCfId = [long](Invoke-PsqlScalar "select id from condiciones_iva where codigo = 5 limit 1")
        TipoDocumentoCuitId = [long](Invoke-PsqlScalar "select id from tipos_documento where codigo = 80 limit 1")
        TipoDocumentoCfId = [long](Invoke-PsqlScalar "select id from tipos_documento where codigo = 99 limit 1")
        CategoriaTerceroGeneralId = [long](Invoke-PsqlScalar "select id from categorias_terceros where descripcion = 'GENERAL' limit 1")
        CategoriaItemGeneralId = [long](Invoke-PsqlScalar "select id from categorias_items where codigo = 'GENERAL' limit 1")
        UnidadMedidaUnId = [long](Invoke-PsqlScalar "select id from unidades_medida where codigo = 'UN' limit 1")
        MarcaGenId = [long](Invoke-PsqlScalar "select id from marcas_comerciales where codigo = 'GEN' limit 1")
        ClienteId = [long](Invoke-PsqlScalar "select id from terceros where legajo = 'CLI0001' limit 1")
        ProveedorId = [long](Invoke-PsqlScalar "select id from terceros where legajo = 'PRV0001' limit 1")
        EmpleadoTerceroId = [long](Invoke-PsqlScalar "select id from terceros where legajo = 'EMP0001' limit 1")
        TransportistaId = [long](Invoke-PsqlScalar "select id from terceros where legajo = 'TRA0001' limit 1")
        ItemId = [long](Invoke-PsqlScalar "select id from items where codigo = 'ITEM-GENERICO' limit 1")
        ItemSmokeId = [long](Invoke-PsqlScalar "select id from items where codigo = 'ITEM-SMOKE' limit 1")
        DepositoId = [long](Invoke-PsqlScalar "select d.id from depositos d join sucursales s on s.id = d.sucursal_id where s.cuit = '30712345678' and d.descripcion = 'DEPÓSITO PRINCIPAL' limit 1")
        DepositoSecundarioId = [long](Invoke-PsqlScalar "select d.id from depositos d join sucursales s on s.id = d.sucursal_id where s.cuit = '30712345678' and d.descripcion = 'DEPÓSITO SECUNDARIO' limit 1")
        Alicuota21Id = [long](Invoke-PsqlScalar "select id from alicuotas_iva where codigo = 5 limit 1")
        PlanPagoContadoId = [long](Invoke-PsqlScalar "select id from planes_pago where descripcion = 'CONTADO' limit 1")
        FormaPagoEfectivoId = [long](Invoke-PsqlScalar "select id from formas_pago where descripcion = 'EFECTIVO' limit 1")
        TipoFacturaVentaId = [long](Invoke-PsqlScalar "select id from tipos_comprobante where codigo = 'FACC' limit 1")
        TipoNotaPedidoVentaId = [long](Invoke-PsqlScalar "select id from tipos_comprobante where codigo = 'REM' limit 1")
        TipoOrdenCompraId = [long](Invoke-PsqlScalar "select id from tipos_comprobante where codigo = 'OC' limit 1")
        ListaPreciosId = [long](Invoke-PsqlScalar "select id from listas_precios where descripcion = 'LISTA SMOKE' limit 1")
        ContratoId = [long](Invoke-PsqlScalar "select id from contratos where codigo = 'CTR-SMOKE' limit 1")
        FormulaId = [long](Invoke-PsqlScalar "select id from formulas_produccion where codigo = 'FOR-SMOKE' limit 1")
        OrdenTrabajoId = [long](Invoke-PsqlScalar "select id from ordenes_trabajo where observacion = 'OT smoke' order by id desc limit 1")
        OrdenEmpaqueId = [long](Invoke-PsqlScalar "select id from ordenes_empaque where observacion = 'Empaque smoke' order by id desc limit 1")
        OrdenPreparacionId = [long](Invoke-PsqlScalar "select id from ordenes_preparacion where observacion = 'Orden preparación smoke' order by id desc limit 1")
        TransferenciaDepositoId = [long](Invoke-PsqlScalar "select id from transferencias_deposito where observacion = 'Transferencia smoke' order by id desc limit 1")
        CartaPorteId = [long](Invoke-PsqlScalar "select id from carta_porte where nro_ctg = 'CTG-SMOKE' limit 1")
        OrdenCargaId = [long](Invoke-PsqlScalar "select id from ordenes_carga where observacion = 'Orden carga smoke' order by id desc limit 1")
        RegionDiagnosticaId = [long](Invoke-PsqlScalar "select id from regiones_diagnosticas where codigo = 'REG-SMOKE' limit 1")
        AspectoDiagnosticoId = [long](Invoke-PsqlScalar "select id from aspectos_diagnostico where codigo = 'ASP-SMOKE' limit 1")
        VariableDiagnosticaId = [long](Invoke-PsqlScalar "select id from variables_diagnosticas where codigo = 'VAR-SMOKE' limit 1")
        PlantillaDiagnosticaId = [long](Invoke-PsqlScalar "select id from plantillas_diagnosticas where codigo = 'PLT-SMOKE' limit 1")
        PlanillaDiagnosticaId = [long](Invoke-PsqlScalar "select id from planillas_diagnosticas where nombre = 'PLANILLA SMOKE' limit 1")
        PlanGeneralColegioId = [long](Invoke-PsqlScalar "select id from colegio_planes_generales where codigo = 'COL-SMOKE' limit 1")
        LoteColegioId = [long](Invoke-PsqlScalar "select id from colegio_lotes where codigo = 'LOTE-SMOKE' limit 1")
        CedulonId = [long](Invoke-PsqlScalar "select id from cedulones where nro_cedulon = 'CED-SMOKE' limit 1")
        EmpleadoId = [long](Invoke-PsqlScalar "select id from empleados where legajo = 'EMP0001' limit 1")
        LiquidacionSueldoId = [long](Invoke-PsqlScalar "select id from liquidaciones_sueldo ls join empleados e on e.id = ls.empleado_id where e.legajo = 'EMP0001' order by ls.id desc limit 1")
        ComprobanteEmpleadoId = [long](Invoke-PsqlScalar "select id from comprobantes_empleados where numero = 'REC-SMOKE' limit 1")
        TimbradoFiscalId = [long](Invoke-PsqlScalar "select id from timbrados_fiscales where numero_timbrado = 'TIMBRADO-INICIAL' limit 1")
        AsientoId = [long](Invoke-PsqlScalar "select id from asientos where numero = 900001 order by id desc limit 1")
    }
}

function Add-ContextValue {
    param([object]$Context, [string]$Name, [object]$Value)

    if ($Context.PSObject.Properties[$Name]) {
        $Context.$Name = $Value
    }
    else {
        Add-Member -InputObject $Context -NotePropertyName $Name -NotePropertyValue $Value
    }
}

function Get-SampleValue {
    param(
        [string]$Path,
        [string]$ParameterName,
        [object]$Context
    )

    $lowerPath = $Path.ToLowerInvariant()
    $param = $ParameterName.ToLowerInvariant()

    switch -Regex ($param) {
        'legajo' { return 'CLI0001' }
        'codigobarras' { return '7790000000001' }
        'codigo' {
            if ($lowerPath -like '*items*') { return 'ITEM-SMOKE' }
            if ($lowerPath -like '*diagnosticos*') { return 'REG-SMOKE' }
            return 'ITEM-SMOKE'
        }
        'search' { return 'smoke' }
    }

    if ($lowerPath -like '/api/sucursales*') { return $Context.SucursalId }
    if ($lowerPath -like '/api/terceros*') { return $Context.ClienteId }
    if ($lowerPath -like '/api/items*') { return $Context.ItemSmokeId }
    if ($lowerPath -like '/api/cajas*') { return $Context.CajaPrincipalId }
    if ($lowerPath -like '/api/cheques*') { return $Context.ChequeId }
    if ($lowerPath -like '/api/cobros*') { return $Context.CobroId }
    if ($lowerPath -like '/api/pagos*') { return $Context.PagoId }
    if ($lowerPath -like '/api/comprobantes*') { return $Context.VentaId }
    if ($lowerPath -like '/api/ventas*') { return $Context.VentaId }
    if ($lowerPath -like '/api/compras/documentos*') { return $Context.CompraDocumentoId }
    if ($lowerPath -like '/api/compras/ordenes*' -or $lowerPath -like '/api/ordenescompra*') { return $Context.OrdenCompraId }
    if ($lowerPath -like '/api/transportistas*') { return $Context.TransportistaRegistroId }
    if ($lowerPath -like '/api/contratos*') { return $Context.ContratoId }
    if ($lowerPath -like '/api/formulasproduccion*') { return $Context.FormulaId }
    if ($lowerPath -like '/api/produccion*' -and $lowerPath -like '*empaque*') { return $Context.OrdenEmpaqueId }
    if ($lowerPath -like '/api/produccion*') { return $Context.OrdenTrabajoId }
    if ($lowerPath -like '/api/ordenespreparacion*') { return $Context.OrdenPreparacionId }
    if ($lowerPath -like '/api/transferenciasdeposito*') { return $Context.TransferenciaDepositoId }
    if ($lowerPath -like '/api/cartaporte*' -and $lowerPath -like '*orden*') { return $Context.OrdenCargaId }
    if ($lowerPath -like '/api/cartaporte*') { return $Context.CartaPorteId }
    if ($lowerPath -like '/api/diagnosticos*' -and $lowerPath -like '*planilla*') { return $Context.PlanillaDiagnosticaId }
    if ($lowerPath -like '/api/diagnosticos*' -and $lowerPath -like '*plantilla*') { return $Context.PlantillaDiagnosticaId }
    if ($lowerPath -like '/api/diagnosticos*' -and $lowerPath -like '*variable*') { return $Context.VariableDiagnosticaId }
    if ($lowerPath -like '/api/diagnosticos*' -and $lowerPath -like '*aspecto*') { return $Context.AspectoDiagnosticoId }
    if ($lowerPath -like '/api/diagnosticos*') { return $Context.RegionDiagnosticaId }
    if ($lowerPath -like '/api/colegio*' -and $lowerPath -like '*cedulon*') { return $Context.CedulonId }
    if ($lowerPath -like '/api/colegio*' -and $lowerPath -like '*lote*') { return $Context.LoteColegioId }
    if ($lowerPath -like '/api/colegio*') { return $Context.PlanGeneralColegioId }
    if ($lowerPath -like '/api/empleados*' -and $lowerPath -like '*liquid*') { return $Context.LiquidacionSueldoId }
    if ($lowerPath -like '/api/empleados*' -and $lowerPath -like '*comprobante*') { return $Context.ComprobanteEmpleadoId }
    if ($lowerPath -like '/api/empleados*') { return $Context.EmpleadoId }
    if ($lowerPath -like '/api/puntoventa*' -and $lowerPath -like '*timbr*') { return $Context.TimbradoFiscalId }
    if ($lowerPath -like '/api/puntosfacturacion*') { return $Context.PuntoFacturacionId }
    if ($lowerPath -like '/api/asientos*' -or $lowerPath -like '/api/contabilidad*') { return $Context.AsientoId }

    switch ($param) {
        'sucursalid' { return $Context.SucursalId }
        'terceroid' { return $Context.ClienteId }
        'proveedorid' { return $Context.ProveedorId }
        'clienteid' { return $Context.ClienteId }
        'empleadoid' { return $Context.EmpleadoId }
        'itemid' { return $Context.ItemSmokeId }
        'depositoid' { return $Context.DepositoId }
        'cajaid' { return $Context.CajaPrincipalId }
        'cajacuentaid' { return $Context.CajaPrincipalId }
        'listapreciosid' { return $Context.ListaPreciosId }
        'monedaid' { return $Context.MonedaArsId }
        'paisid' { return $Context.PaisId }
        'provinciaid' { return $Context.ProvinciaId }
        'localidadid' { return $Context.LocalidadId }
        'barrioid' { return $Context.BarrioId }
        'puntofacturacionid' { return $Context.PuntoFacturacionId }
    }

    return $null
}

function Expand-ApiPath {
    param(
        [string]$PathTemplate,
        [object[]]$Parameters,
        [object]$Context
    )

    $resolved = $PathTemplate
    $query = New-Object System.Collections.Generic.List[string]

    foreach ($parameter in $Parameters) {
        $name = [string]$parameter.name
        $location = [string]$parameter.in
        $value = Get-SampleValue -Path $PathTemplate -ParameterName $name -Context $Context
        if ($null -eq $value) { continue }

        if ($location -eq 'path') {
            $resolved = $resolved -replace "\{$name\}", [string]$value
            continue
        }

        if ($location -eq 'query' -and $parameter.required) {
            $query.Add("$name=$([uri]::EscapeDataString([string]$value))") | Out-Null
        }
    }

    if ($query.Count -gt 0) {
        return "$resolved?$(($query -join '&'))"
    }

    return $resolved
}

function Invoke-AnonymousChecks {
    Invoke-Api -Name 'Health' -Method 'GET' -Path '/health' -Anonymous
    Invoke-Api -Name 'Health detailed' -Method 'GET' -Path '/health/detailed' -Anonymous
    Invoke-Api -Name 'Swagger JSON' -Method 'GET' -Path '/swagger/v1/swagger.json' -Anonymous
}

function Invoke-DiscoveredGetSmoke {
    param([object]$Context)

    $swagger = Invoke-Api -Name 'Swagger discovery' -Method 'GET' -Path '/swagger/v1/swagger.json' -Anonymous
    $paths = $swagger.Data.paths.PSObject.Properties

    foreach ($entry in $paths) {
        $path = [string]$entry.Name
        $getOperation = $entry.Value.get
        if ($null -eq $getOperation) { continue }
        if ($path -like '/health*' -or $path -like '/swagger*') { continue }

        $expandedPath = Expand-ApiPath -PathTemplate $path -Parameters @($getOperation.parameters) -Context $Context
        if ($expandedPath -match '\{.+\}') { continue }

        Invoke-Api -Name "GET $path" -Method 'GET' -Path $expandedPath -AllowFailure
    }
}

function Invoke-PostFlowDataExpansion {
    param([object]$Context)

    Invoke-PsqlNonQuery @"
INSERT INTO comprobantes_items_atributos (comprobante_item_id, atributo_comercial_id, valor, created_at, updated_at, created_by, updated_by)
SELECT ci.id, ac.id, 'SMOKE', now(), now(), 1, 1
FROM comprobantes_items ci
CROSS JOIN atributos_comerciales ac
WHERE ci.comprobante_id = $($Context.VentaId) AND ac.codigo = 'ATR-SMOKE'
AND NOT EXISTS (SELECT 1 FROM comprobantes_items_atributos x WHERE x.comprobante_item_id = ci.id AND x.atributo_comercial_id = ac.id);

INSERT INTO impresion_spool_trabajos (comprobante_id, tipo_trabajo, destino, estado, intentos, proximo_intento, payload_generado, mensaje_error, created_at, updated_at, created_by, updated_by)
SELECT $($Context.VentaId), 'PDF', 'LOCAL', 'Pendiente', 0, NULL, '{}', NULL, now(), now(), 1, 1
WHERE NOT EXISTS (SELECT 1 FROM impresion_spool_trabajos WHERE comprobante_id = $($Context.VentaId));

INSERT INTO operaciones_punto_venta (comprobante_id, sucursal_id, punto_facturacion_id, canal, fecha, referencia_externa, observacion, created_at, updated_at, created_by, updated_by)
SELECT $($Context.VentaId), $($Context.SucursalId), $($Context.PuntoFacturacionId), 'Pos', CURRENT_DATE, 'POS-SMOKE', 'Operacion PV smoke', now(), now(), 1, 1
WHERE NOT EXISTS (SELECT 1 FROM operaciones_punto_venta WHERE referencia_externa = 'POS-SMOKE');

INSERT INTO deuce_operaciones (comprobante_id, sucursal_id, punto_facturacion_id, estado, referencia_externa, request_payload, response_payload, observacion, created_at, updated_at, created_by, updated_by)
SELECT $($Context.VentaId), $($Context.SucursalId), $($Context.PuntoFacturacionId), 'Confirmada', 'DEUCE-SMOKE', '{}', '{}', 'Deuce smoke', now(), now(), 1, 1
WHERE NOT EXISTS (SELECT 1 FROM deuce_operaciones WHERE referencia_externa = 'DEUCE-SMOKE');

INSERT INTO sifen_operaciones (comprobante_id, sucursal_id, punto_facturacion_id, timbrado_fiscal_id, estado, request_payload, response_payload, codigo_seguridad, observacion, created_at, updated_at, created_by, updated_by)
SELECT $($Context.VentaId), $($Context.SucursalId), $($Context.PuntoFacturacionId), $($Context.TimbradoFiscalId), 'Confirmada', '{}', '{}', 'SIFENSMOKE', 'Sifen smoke', now(), now(), 1, 1
WHERE NOT EXISTS (SELECT 1 FROM sifen_operaciones WHERE comprobante_id = $($Context.VentaId));

INSERT INTO colegio_recibos_detalles (cobro_id, cedulon_id, importe)
SELECT $($Context.CobroId), $($Context.CedulonId), 100
WHERE NOT EXISTS (SELECT 1 FROM colegio_recibos_detalles WHERE cobro_id = $($Context.CobroId) AND cedulon_id = $($Context.CedulonId));
"@
}

function Invoke-MasterDataMutationScenarios {
    param([object]$Context)

    $suffix = [DateTime]::UtcNow.ToString('MMddHHmmss')

    Write-Step 'Escenarios de maestros: sucursales y puntos de facturación'
    $cuit = "3079$suffix"
    Invoke-Api -Name 'Sucursal create api' -Method 'POST' -Path '/api/sucursales' -Body @{
        razonSocial = 'Sucursal API Smoke'
        nombreFantasia = 'Sucursal API'
        cuit = $cuit
        nroIngresosBrutos = 'IB-API'
        condicionIvaId = $Context.CondicionIvaRiId
        monedaId = $Context.MonedaArsId
        paisId = $Context.PaisId
        calle = 'CALLE API'
        nro = '123'
        piso = $null
        dpto = $null
        codigoPostal = '3300'
        localidadId = $Context.LocalidadId
        barrioId = $Context.BarrioId
        telefono = '3764000000'
        email = 'sucursal-api@zuluia.local'
        web = $null
        cbu = $null
        aliasCbu = $null
        cai = $null
        puertoAfip = 443
        casaMatriz = $false
    } -AllowFailure
    $sucursalApiId = Try-ResolveLong "select id from sucursales where cuit = '$cuit' limit 1"
    if ($sucursalApiId) {
        Invoke-Api -Name 'Sucursal update api' -Method 'PUT' -Path "/api/sucursales/$sucursalApiId" -Body @{
            id = $sucursalApiId
            razonSocial = 'Sucursal API Smoke Updated'
            nombreFantasia = 'Sucursal API Upd'
            cuit = $cuit
            nroIngresosBrutos = 'IB-API-UPD'
            condicionIvaId = $Context.CondicionIvaRiId
            monedaId = $Context.MonedaArsId
            paisId = $Context.PaisId
            calle = 'CALLE API'
            nro = '124'
            piso = $null
            dpto = $null
            codigoPostal = '3300'
            localidadId = $Context.LocalidadId
            barrioId = $Context.BarrioId
            telefono = '3764000001'
            email = 'sucursal-api-upd@zuluia.local'
            web = $null
            cbu = $null
            aliasCbu = $null
            cai = $null
            puertoAfip = 443
            casaMatriz = $false
        } -AllowFailure

        $numeroPf = [int](Get-Random -Minimum 20 -Maximum 90)
        Invoke-Api -Name 'Punto facturación create api' -Method 'POST' -Path '/api/puntosfacturacion' -Body @{ sucursalId = $sucursalApiId; tipoId = $Context.TipoPuntoManualId; numero = $numeroPf; descripcion = 'PF API smoke' } -AllowFailure
        $pfApiId = Try-ResolveLong "select id from puntos_facturacion where sucursal_id = $sucursalApiId and numero = $numeroPf limit 1"
        if ($pfApiId) {
            Invoke-Api -Name 'Punto facturación update api' -Method 'PUT' -Path "/api/puntosfacturacion/$pfApiId" -Body @{ tipoId = $Context.TipoPuntoManualId; descripcion = 'PF API smoke updated' } -AllowFailure
            Invoke-Api -Name 'Punto facturación afip api' -Method 'PUT' -Path "/api/puntosfacturacion/$pfApiId/afip" -Body @{ habilitado = $true; produccion = $false; usaCaeaPorDefecto = $false; cuitEmisor = $cuit; certificadoAlias = $null; observacion = 'AFIP api smoke' } -AllowFailure
            Invoke-Api -Name 'Punto facturación delete api' -Method 'DELETE' -Path "/api/puntosfacturacion/$pfApiId" -AllowFailure
        }
    }

    Write-Step 'Escenarios de maestros: terceros'
    $legajo = "TMP$suffix"
    Invoke-Api -Name 'Tercero create api' -Method 'POST' -Path '/api/terceros' -Body @{
        legajo = $legajo
        razonSocial = 'TERCERO API SMOKE'
        nombreFantasia = 'TERCERO API'
        tipoDocumentoId = $Context.TipoDocumentoCfId
        nroDocumento = "DOC$suffix"
        condicionIvaId = $Context.CondicionIvaCfId
        esCliente = $true
        esProveedor = $false
        esEmpleado = $false
        calle = 'CALLE API'
        nro = '100'
        piso = $null
        dpto = $null
        codigoPostal = '3300'
        localidadId = $Context.LocalidadId
        barrioId = $Context.BarrioId
        nroIngresosBrutos = $null
        nroMunicipal = $null
        telefono = '3764111111'
        celular = '3764222222'
        email = 'tercero-api@zuluia.local'
        web = $null
        monedaId = $Context.MonedaArsId
        categoriaId = $Context.CategoriaTerceroGeneralId
        limiteCredito = 1000
        facturable = $true
        cobradorId = $null
        pctComisionCobrador = 0
        vendedorId = $null
        pctComisionVendedor = 0
        observacion = 'Tercero api smoke'
        sucursalId = $Context.SucursalId
    } -AllowFailure
    $terceroApiId = Try-ResolveLong "select id from terceros where legajo = '$legajo' limit 1"
    if ($terceroApiId) {
        Invoke-Api -Name 'Tercero update api' -Method 'PUT' -Path "/api/terceros/$terceroApiId" -Body @{
            id = $terceroApiId
            razonSocial = 'TERCERO API SMOKE UPDATED'
            nombreFantasia = 'TERCERO API UPD'
            nroDocumento = "DOCU$suffix"
            condicionIvaId = $Context.CondicionIvaCfId
            esCliente = $true
            esProveedor = $false
            esEmpleado = $false
            calle = 'CALLE API'
            nro = '101'
            piso = $null
            dpto = $null
            codigoPostal = '3300'
            localidadId = $Context.LocalidadId
            barrioId = $Context.BarrioId
            nroIngresosBrutos = $null
            nroMunicipal = $null
            telefono = '3764333333'
            celular = '3764444444'
            email = 'tercero-api-upd@zuluia.local'
            web = $null
            monedaId = $Context.MonedaArsId
            categoriaId = $Context.CategoriaTerceroGeneralId
            limiteCredito = 2000
            facturable = $true
            cobradorId = $null
            pctComisionCobrador = 0
            vendedorId = $null
            pctComisionVendedor = 0
            observacion = 'Update tercero api'
        } -AllowFailure
        Invoke-Api -Name 'Tercero delete api' -Method 'DELETE' -Path "/api/terceros/$terceroApiId" -AllowFailure
        Invoke-Api -Name 'Tercero activar api' -Method 'PATCH' -Path "/api/terceros/$terceroApiId/activar" -AllowFailure
        Add-ContextValue -Context $Context -Name 'TerceroApiId' -Value $terceroApiId
    }

    Write-Step 'Escenarios de maestros: items'
    $itemCodigo = "ITAPI$suffix"
    Invoke-Api -Name 'Item create api' -Method 'POST' -Path '/api/items' -Body @{
        codigo = $itemCodigo
        descripcion = 'ITEM API SMOKE'
        descripcionAdicional = $null
        codigoBarras = "779$suffix"
        unidadMedidaId = $Context.UnidadMedidaUnId
        alicuotaIvaId = $Context.Alicuota21Id
        monedaId = $Context.MonedaArsId
        esProducto = $true
        esServicio = $false
        esFinanciero = $false
        manejaStock = $true
        categoriaId = $Context.CategoriaItemGeneralId
        precioCosto = 100
        precioVenta = 150
        stockMinimo = 0
        stockMaximo = 10
        codigoAfip = $null
        sucursalId = $Context.SucursalId
        marcaId = $Context.MarcaGenId
    } -AllowFailure
    $itemApiId = Try-ResolveLong "select id from items where codigo = '$itemCodigo' limit 1"
    if ($itemApiId) {
        Invoke-Api -Name 'Item update api' -Method 'PUT' -Path "/api/items/$itemApiId" -Body @{
            id = $itemApiId
            descripcion = 'ITEM API SMOKE UPDATED'
            descripcionAdicional = 'Adicional'
            codigoBarras = "779$suffix"
            unidadMedidaId = $Context.UnidadMedidaUnId
            alicuotaIvaId = $Context.Alicuota21Id
            monedaId = $Context.MonedaArsId
            esProducto = $true
            esServicio = $false
            esFinanciero = $false
            manejaStock = $true
            categoriaId = $Context.CategoriaItemGeneralId
            precioCosto = 120
            precioVenta = 180
            stockMinimo = 0
            stockMaximo = 20
            codigoAfip = $null
            marcaId = $Context.MarcaGenId
        } -AllowFailure
        Invoke-Api -Name 'Item update precios api' -Method 'PATCH' -Path "/api/items/$itemApiId/precios" -Body @{ precioCosto = 130; precioVenta = 190 } -AllowFailure
        Invoke-Api -Name 'Item delete api' -Method 'DELETE' -Path "/api/items/$itemApiId" -AllowFailure
        Add-ContextValue -Context $Context -Name 'ItemApiId' -Value $itemApiId
    }
}

function Invoke-ExtendedMutationScenarios {
    param([object]$Context)

    $suffix = [DateTime]::UtcNow.ToString('MMddHHmmss')

    Write-Step 'Escenarios extendidos: contratos'
    $contratoCodigo = "CTR-API-$suffix"
    Invoke-Api -Name 'Contrato create' -Method 'POST' -Path '/api/contratos' -Body @{
        terceroId = $Context.ClienteId
        sucursalId = $Context.SucursalId
        monedaId = $Context.MonedaArsId
        codigo = $contratoCodigo
        descripcion = 'Contrato API Smoke'
        fechaInicio = (Get-Date).ToString('yyyy-MM-dd')
        fechaFin = (Get-Date).AddDays(30).ToString('yyyy-MM-dd')
        importe = 22000
        renovacionAutomatica = $true
        observacion = 'Contrato api smoke'
        generarImpactoComercial = $true
        generarImpactoFinanciero = $false
    } -AllowFailure
    $contratoApiId = Try-ResolveLong "select id from contratos where codigo = '$contratoCodigo' limit 1"
    if ($contratoApiId) {
        Invoke-Api -Name 'Contrato update' -Method 'PUT' -Path "/api/contratos/$contratoApiId" -Body @{
            id = $contratoApiId
            descripcion = 'Contrato API Smoke Updated'
            fechaInicio = (Get-Date).ToString('yyyy-MM-dd')
            fechaFin = (Get-Date).AddDays(45).ToString('yyyy-MM-dd')
            importe = 25000
            renovacionAutomatica = $true
            observacion = 'Contrato api smoke update'
            generarImpactoComercial = $false
            generarImpactoFinanciero = $false
        } -AllowFailure
        Invoke-Api -Name 'Contrato impacto' -Method 'POST' -Path "/api/contratos/$contratoApiId/impactos" -Body @{ tipo = 'Comercial'; fecha = (Get-Date).ToString('yyyy-MM-dd'); importe = 1000; descripcion = 'Impacto API smoke'; impactarCuentaCorriente = $false } -AllowFailure
        Invoke-Api -Name 'Contrato renovar' -Method 'POST' -Path "/api/contratos/$contratoApiId/renovar" -Body @{ nuevaFechaFin = (Get-Date).AddDays(60).ToString('yyyy-MM-dd'); nuevoImporte = 26000; observacion = 'Renovación api'; generarImpactoComercial = $true; generarImpactoFinanciero = $false } -AllowFailure
        Invoke-Api -Name 'Contrato cancelar' -Method 'POST' -Path "/api/contratos/$contratoApiId/cancelar" -Body @{ observacion = 'Cancelación api smoke' } -AllowFailure
        Invoke-Api -Name 'Contrato detalle api' -Method 'GET' -Path "/api/contratos/$contratoApiId/detalle" -AllowFailure
    }

    Write-Step 'Escenarios extendidos: diagnósticos'
    $regionCodigo = "REGAPI$suffix"
    Invoke-Api -Name 'Diagnóstico región create' -Method 'POST' -Path '/api/diagnosticos/regiones' -Body @{ codigo = $regionCodigo; descripcion = 'Región API smoke' } -AllowFailure
    $regionApiId = Try-ResolveLong "select id from regiones_diagnosticas where codigo = '$regionCodigo' limit 1"
    if ($regionApiId) {
        $aspectoCodigo = "ASPAPI$suffix"
        Invoke-Api -Name 'Diagnóstico aspecto create' -Method 'POST' -Path '/api/diagnosticos/aspectos' -Body @{ regionId = $regionApiId; codigo = $aspectoCodigo; descripcion = 'Aspecto API smoke'; peso = 1 } -AllowFailure
        $aspectoApiId = Try-ResolveLong "select id from aspectos_diagnostico where codigo = '$aspectoCodigo' limit 1"
        if ($aspectoApiId) {
            $variableCodigo = "VARAPI$suffix"
            Invoke-Api -Name 'Diagnóstico variable create' -Method 'POST' -Path '/api/diagnosticos/variables' -Body @{ aspectoId = $aspectoApiId; codigo = $variableCodigo; descripcion = 'Variable API smoke'; tipo = 'Opcion'; requerida = $true; peso = 1; opciones = @(@{ codigo = 'A'; descripcion = 'Opcion A'; valorNumerico = 5; orden = 1 }) } -AllowFailure
            $variableApiId = Try-ResolveLong "select id from variables_diagnosticas where codigo = '$variableCodigo' limit 1"
            if ($variableApiId) {
                $plantillaCodigo = "PLAPI$suffix"
                Invoke-Api -Name 'Diagnóstico plantilla create' -Method 'POST' -Path '/api/diagnosticos/plantillas' -Body @{ codigo = $plantillaCodigo; descripcion = 'Plantilla API smoke'; observacion = 'Plantilla api'; variables = @(@{ variableId = $variableApiId; orden = 1 }) } -AllowFailure
                $plantillaApiId = Try-ResolveLong "select id from plantillas_diagnosticas where codigo = '$plantillaCodigo' limit 1"
                if ($plantillaApiId) {
                    $planillaNombre = "Planilla API $suffix"
                    Invoke-Api -Name 'Diagnóstico planilla create' -Method 'POST' -Path '/api/diagnosticos/planillas' -Body @{ plantillaId = $plantillaApiId; nombre = $planillaNombre; fecha = (Get-Date).ToString('yyyy-MM-dd'); observacion = 'Planilla api' } -AllowFailure
                    $planillaApiId = Try-ResolveLong "select id from planillas_diagnosticas where nombre = '$planillaNombre' limit 1"
                    $opcionApiId = Try-ResolveLong "select id from variables_diagnosticas_opciones where variable_id = $variableApiId and codigo = 'A' limit 1"
                    if ($planillaApiId -and $opcionApiId) {
                        Invoke-Api -Name 'Diagnóstico evaluar' -Method 'POST' -Path "/api/diagnosticos/planillas/$planillaApiId/evaluar" -Body @{ respuestas = @(@{ variableId = $variableApiId; opcionId = $opcionApiId; valorTexto = $null; valorNumerico = 5 }); observacion = 'Evaluación api' } -AllowFailure
                    }
                }
            }
        }
    }

    Write-Step 'Escenarios extendidos: fórmulas y producción'
    $formulaCodigo = "FORAPI$suffix"
    Invoke-Api -Name 'Fórmula create' -Method 'POST' -Path '/api/formulasproduccion' -Body @{
        codigo = $formulaCodigo
        descripcion = 'Fórmula API smoke'
        itemResultadoId = $Context.ItemSmokeId
        cantidadResultado = 1
        unidadMedidaId = $Context.UnidadMedidaUnId
        observacion = 'Formula api'
        ingredientes = @(@{ itemId = $Context.ItemId; cantidad = 1; unidadMedidaId = $Context.UnidadMedidaUnId; esOpcional = $false; orden = 1 })
    } -AllowFailure
    $formulaApiId = Try-ResolveLong "select id from formulas_produccion where codigo = '$formulaCodigo' limit 1"
    if ($formulaApiId) {
        Invoke-Api -Name 'Fórmula update' -Method 'PUT' -Path "/api/formulasproduccion/$formulaApiId" -Body @{ descripcion = 'Fórmula API smoke updated'; cantidadResultado = 2; observacion = 'Update api' } -AllowFailure
        Invoke-Api -Name 'OT create' -Method 'POST' -Path '/api/produccion/ordenes-trabajo' -Body @{ sucursalId = $Context.SucursalId; formulaId = $formulaApiId; depositoOrigenId = $Context.DepositoId; depositoDestinoId = $Context.DepositoSecundarioId; fecha = (Get-Date).ToString('yyyy-MM-dd'); fechaFinPrevista = (Get-Date).AddDays(1).ToString('yyyy-MM-dd'); cantidad = 2; observacion = "OT API $suffix" } -AllowFailure
        $otApiId = Try-ResolveLong "select id from ordenes_trabajo where observacion = 'OT API $suffix' limit 1"
        if ($otApiId) {
            Invoke-Api -Name 'OT iniciar' -Method 'POST' -Path "/api/produccion/ordenes-trabajo/$otApiId/iniciar" -AllowFailure
            Invoke-Api -Name 'Empaque create' -Method 'POST' -Path '/api/produccion/ordenes-empaque' -Body @{ ordenTrabajoId = $otApiId; itemId = $Context.ItemSmokeId; depositoId = $Context.DepositoSecundarioId; fecha = (Get-Date).ToString('yyyy-MM-dd'); cantidad = 1; lote = "LOTAPI$suffix"; observacion = 'Empaque api' } -AllowFailure
            Invoke-Api -Name 'OT finalizar' -Method 'POST' -Path "/api/produccion/ordenes-trabajo/$otApiId/finalizar" -Body @{ fechaFinReal = (Get-Date).ToString('yyyy-MM-dd'); cantidadProducida = 1; consumos = @(@{ itemId = $Context.ItemId; cantidadConsumida = 1; observacion = 'Consumo api' }) } -AllowFailure
        }
    }

    Write-Step 'Escenarios extendidos: órdenes de preparación y transferencias'
    Invoke-Api -Name 'Orden preparación create' -Method 'POST' -Path '/api/ordenespreparacion' -Body @{ sucursalId = $Context.SucursalId; comprobanteOrigenId = $null; terceroId = $Context.ClienteId; fecha = (Get-Date).ToString('yyyy-MM-dd'); observacion = "OP API $suffix"; detalles = @(@{ itemId = $Context.ItemSmokeId; depositoId = $Context.DepositoId; cantidad = 1; observacion = 'Detalle api' }) } -AllowFailure
    $opApiId = Try-ResolveLong "select id from ordenes_preparacion where observacion = 'OP API $suffix' limit 1"
    if ($opApiId) {
        Invoke-Api -Name 'Orden preparación iniciar' -Method 'POST' -Path "/api/ordenespreparacion/$opApiId/iniciar" -AllowFailure
        $detalleApiId = Try-ResolveLong "select id from ordenes_preparacion_detalles where orden_preparacion_id = $opApiId order by id limit 1"
        if ($detalleApiId) {
            Invoke-Api -Name 'Orden preparación picking' -Method 'POST' -Path "/api/ordenespreparacion/$opApiId/picking" -Body @{ detalles = @(@{ detalleId = $detalleApiId; cantidadEntregada = 1 }) } -AllowFailure
        }
        Invoke-Api -Name 'Orden preparación despachar' -Method 'POST' -Path "/api/ordenespreparacion/$opApiId/despachar" -Body @{ depositoDestinoId = $Context.DepositoSecundarioId; fecha = (Get-Date).ToString('yyyy-MM-dd'); observacion = 'Despacho api' } -AllowFailure
        Invoke-Api -Name 'Orden preparación confirmar' -Method 'POST' -Path "/api/ordenespreparacion/$opApiId/confirmar" -AllowFailure
    }

    Invoke-Api -Name 'Transferencia depósito create' -Method 'POST' -Path '/api/transferenciasdeposito' -Body @{ sucursalId = $Context.SucursalId; depositoOrigenId = $Context.DepositoId; depositoDestinoId = $Context.DepositoSecundarioId; fecha = (Get-Date).ToString('yyyy-MM-dd'); observacion = "TD API $suffix"; detalles = @(@{ itemId = $Context.ItemSmokeId; cantidad = 1; observacion = 'Detalle td api' }); ordenPreparacionId = $opApiId } -AllowFailure
    $tdApiId = Try-ResolveLong "select id from transferencias_deposito where observacion = 'TD API $suffix' limit 1"
    if ($tdApiId) {
        Invoke-Api -Name 'Transferencia depósito confirmar' -Method 'POST' -Path "/api/transferenciasdeposito/$tdApiId/confirmar" -AllowFailure
        Invoke-Api -Name 'Transferencia depósito trazabilidad' -Method 'GET' -Path "/api/transferenciasdeposito/$tdApiId/trazabilidad" -AllowFailure
    }

    Write-Step 'Escenarios extendidos: carta de porte'
    Invoke-Api -Name 'Carta porte create' -Method 'POST' -Path '/api/cartaporte' -Body @{ comprobanteId = $null; cuitRemitente = '30712345678'; cuitDestinatario = '30000000000'; cuitTransportista = '30000000001'; fechaEmision = (Get-Date).ToString('yyyy-MM-dd'); observacion = "Carta API $suffix" } -AllowFailure
    $cpApiId = Try-ResolveLong "select id from carta_porte where observacion = 'Carta API $suffix' limit 1"
    if ($cpApiId) {
        Invoke-Api -Name 'Carta porte asignar ctg' -Method 'POST' -Path "/api/cartaporte/$cpApiId/asignar-ctg" -Body @{ nroCtg = "CTGAPI$suffix" } -AllowFailure
        Invoke-Api -Name 'Carta porte orden carga' -Method 'POST' -Path "/api/cartaporte/$cpApiId/orden-carga" -Body @{ transportistaId = $Context.TransportistaRegistroId; fechaCarga = (Get-Date).ToString('yyyy-MM-dd'); origen = 'Origen API'; destino = 'Destino API'; patente = 'API123'; observacion = 'Orden carga api' } -AllowFailure
        Invoke-Api -Name 'Carta porte solicitar ctg' -Method 'POST' -Path "/api/cartaporte/$cpApiId/ctg/solicitar" -Body @{ fechaSolicitud = (Get-Date).ToString('yyyy-MM-dd'); observacion = 'Solicitud api' } -AllowFailure
        Invoke-Api -Name 'Carta porte consultar ctg' -Method 'POST' -Path "/api/cartaporte/$cpApiId/ctg/consultar" -Body @{ fechaConsulta = (Get-Date).ToString('yyyy-MM-dd'); nroCtg = "CTGAPI$suffix"; error = $null; observacion = 'Consulta api' } -AllowFailure
        Invoke-Api -Name 'Carta porte confirmar' -Method 'POST' -Path "/api/cartaporte/$cpApiId/confirmar" -AllowFailure
        Invoke-Api -Name 'Carta porte historial api' -Method 'GET' -Path "/api/cartaporte/$cpApiId/historial" -AllowFailure
    }

    Write-Step 'Escenarios extendidos: colegio'
    $planCodigo = "COLAPI$suffix"
    Invoke-Api -Name 'Colegio plan general create' -Method 'POST' -Path '/api/colegio/planes-generales' -Body @{ sucursalId = $Context.SucursalId; planPagoId = $Context.PlanPagoContadoId; tipoComprobanteId = $Context.TipoFacturaVentaId; itemId = $Context.ItemId; monedaId = $Context.MonedaArsId; codigo = $planCodigo; descripcion = 'Plan API smoke'; importeBase = 1800; observacion = 'Plan api' } -AllowFailure
    $planApiId = Try-ResolveLong "select id from colegio_planes_generales where codigo = '$planCodigo' limit 1"
    if ($planApiId) {
        Invoke-Api -Name 'Colegio plan general update' -Method 'PUT' -Path "/api/colegio/planes-generales/$planApiId" -Body @{ id = $planApiId; planPagoId = $Context.PlanPagoContadoId; tipoComprobanteId = $Context.TipoFacturaVentaId; itemId = $Context.ItemId; monedaId = $Context.MonedaArsId; codigo = $planCodigo; descripcion = 'Plan API smoke updated'; importeBase = 1900; observacion = 'Update plan api' } -AllowFailure
        $loteCodigo = "LOAPI$suffix"
        Invoke-Api -Name 'Colegio lote create' -Method 'POST' -Path '/api/colegio/lotes' -Body @{ planGeneralColegioId = $planApiId; codigo = $loteCodigo; fechaEmision = (Get-Date).ToString('yyyy-MM-dd'); fechaVencimiento = (Get-Date).AddDays(20).ToString('yyyy-MM-dd'); observacion = 'Lote api' } -AllowFailure
        $loteApiId = Try-ResolveLong "select id from colegio_lotes where codigo = '$loteCodigo' limit 1"
        if ($loteApiId) {
            Invoke-Api -Name 'Colegio lote update' -Method 'PUT' -Path "/api/colegio/lotes/$loteApiId" -Body @{ id = $loteApiId; fechaEmision = (Get-Date).ToString('yyyy-MM-dd'); fechaVencimiento = (Get-Date).AddDays(25).ToString('yyyy-MM-dd'); observacion = 'Lote api update' } -AllowFailure
            Invoke-Api -Name 'Colegio emitir cedulones api' -Method 'POST' -Path "/api/colegio/lotes/$loteApiId/emitir-cedulones" -Body @{ terceroIds = @($Context.ClienteId) } -AllowFailure
            $cedulonApiId = Try-ResolveLong "select id from cedulones where lote_colegio_id = $loteApiId order by id desc limit 1"
            if ($cedulonApiId) {
                Invoke-Api -Name 'Colegio cobinpro api' -Method 'POST' -Path '/api/colegio/cobinpro' -Body @{ cedulonId = $cedulonApiId; fecha = (Get-Date).ToString('yyyy-MM-dd'); importe = 500; referenciaExterna = "COBAPI$suffix"; cajaId = $Context.CajaPrincipalId; formaPagoId = $Context.FormaPagoEfectivoId; monedaId = $Context.MonedaArsId; cotizacion = 1; observacion = 'Cobinpro api' } -AllowFailure
                $cobinproApiId = Try-ResolveLong "select id from colegio_cobinpro_operaciones where referencia_externa = 'COBAPI$suffix' limit 1"
                if ($cobinproApiId) {
                    Invoke-Api -Name 'Colegio cobinpro conciliar api' -Method 'POST' -Path "/api/colegio/cobinpro/$cobinproApiId/conciliar" -Body @{ confirmar = $true; observacion = 'Conciliar api' } -AllowFailure
                }
                Invoke-Api -Name 'Colegio facturar api' -Method 'POST' -Path '/api/colegio/facturacion' -Body @{ cedulonIds = @($cedulonApiId); puntoFacturacionId = $Context.PuntoFacturacionId; fecha = (Get-Date).ToString('yyyy-MM-dd'); fechaVencimiento = (Get-Date).AddDays(10).ToString('yyyy-MM-dd'); observacion = 'Facturación api' } -AllowFailure
            }
            Invoke-Api -Name 'Colegio lote cerrar api' -Method 'POST' -Path "/api/colegio/lotes/$loteApiId/cerrar" -Body @{ observacion = 'Cierre api' } -AllowFailure
        }
    }

    Write-Step 'Escenarios extendidos: punto de venta'
    $timbradoNumero = "TIMB-API-$suffix"
    Invoke-Api -Name 'Timbrado create api' -Method 'POST' -Path '/api/puntoventa/timbrados' -Body @{ sucursalId = $Context.SucursalId; puntoFacturacionId = $Context.PuntoFacturacionId; numeroTimbrado = $timbradoNumero; vigenciaDesde = (Get-Date).ToString('yyyy-MM-dd'); vigenciaHasta = (Get-Date).AddDays(180).ToString('yyyy-MM-dd'); observacion = 'Timbrado api' } -AllowFailure
    $timbradoApiId = Try-ResolveLong "select id from timbrados_fiscales where numero_timbrado = '$timbradoNumero' limit 1"
    if ($timbradoApiId) {
        Invoke-Api -Name 'Timbrado update api' -Method 'PUT' -Path "/api/puntoventa/timbrados/$timbradoApiId" -Body @{ id = $timbradoApiId; numeroTimbrado = $timbradoNumero; vigenciaDesde = (Get-Date).ToString('yyyy-MM-dd'); vigenciaHasta = (Get-Date).AddDays(200).ToString('yyyy-MM-dd'); observacion = 'Timbrado api update' } -AllowFailure
    }
    $timbradoIdForSifen = if ($timbradoApiId) { $timbradoApiId } else { $Context.TimbradoFiscalId }
    Invoke-Api -Name 'POS deuce procesar api' -Method 'POST' -Path "/api/puntoventa/deuce/comprobantes/$($Context.VentaId)" -Body @{ referenciaExterna = "DEUCEAPI$suffix"; requestPayload = '{}'; responsePayload = '{}'; observacion = 'Deuce api'; confirmada = $true } -AllowFailure
    $deuceApiId = Try-ResolveLong "select id from deuce_operaciones where referencia_externa = 'DEUCEAPI$suffix' limit 1"
    if ($deuceApiId) {
        Invoke-Api -Name 'POS deuce conciliar api' -Method 'POST' -Path "/api/puntoventa/deuce/operaciones/$deuceApiId/conciliar" -Body @{ confirmar = $true; observacion = 'Conciliar deuce api' } -AllowFailure
    }
    Invoke-Api -Name 'POS sifen procesar api' -Method 'POST' -Path "/api/puntoventa/sifen/comprobantes/$($Context.VentaId)" -Body @{ timbradoFiscalId = $timbradoIdForSifen; requestPayload = '{}'; responsePayload = '{}'; codigoSeguridad = 'SECAPI'; observacion = 'Sifen api'; confirmada = $true } -AllowFailure
    $sifenApiId = Try-ResolveLong "select id from sifen_operaciones where comprobante_id = $($Context.VentaId) order by id desc limit 1"
    if ($sifenApiId) {
        Invoke-Api -Name 'POS sifen conciliar api' -Method 'POST' -Path "/api/puntoventa/sifen/operaciones/$sifenApiId/conciliar" -Body @{ confirmar = $true; observacion = 'Conciliar sifen api' } -AllowFailure
    }
    if ($timbradoApiId) {
        Invoke-Api -Name 'Timbrado delete api' -Method 'DELETE' -Path "/api/puntoventa/timbrados/$timbradoApiId" -Body @{ observacion = 'Desactivar api' } -AllowFailure
    }

    Write-Step 'Escenarios extendidos: empleados'
    Invoke-Api -Name 'Empleado update api' -Method 'PUT' -Path "/api/empleados/$($Context.EmpleadoId)" -Body @{ id = $Context.EmpleadoId; cargo = 'ADMINISTRATIVO SR'; area = 'RRHH'; sueldoBasico = 1100000; monedaId = $Context.MonedaArsId } -AllowFailure
    $nextMonth = (Get-Date).AddMonths(1)
    Invoke-Api -Name 'Empleado liquidación create api' -Method 'POST' -Path "/api/empleados/$($Context.EmpleadoId)/liquidaciones" -Body @{ empleadoId = $Context.EmpleadoId; sucursalId = $Context.SucursalId; anio = $nextMonth.Year; mes = $nextMonth.Month; sueldoBasico = 1100000; totalHaberes = 1100000; totalDescuentos = 50000; monedaId = $Context.MonedaArsId; observacion = 'Liquidación api' } -AllowFailure
    $liqApiId = Try-ResolveLong "select id from liquidaciones_sueldo where empleado_id = $($Context.EmpleadoId) and anio = $($nextMonth.Year) and mes = $($nextMonth.Month) limit 1"
    if ($liqApiId) {
        Invoke-Api -Name 'Empleado liquidación update api' -Method 'PUT' -Path "/api/empleados/liquidaciones/$liqApiId" -Body @{ id = $liqApiId; sueldoBasico = 1100000; totalHaberes = 1120000; totalDescuentos = 50000; monedaId = $Context.MonedaArsId; observacion = 'Liquidación api update' } -AllowFailure
        Invoke-Api -Name 'Empleado comprobante api' -Method 'POST' -Path "/api/empleados/liquidaciones/$liqApiId/comprobante" -Body @{ fecha = (Get-Date).ToString('yyyy-MM-dd'); tipo = 'RECIBO_SUELDO'; observacion = 'Comp api' } -AllowFailure
        Invoke-Api -Name 'Empleado imputación api' -Method 'POST' -Path "/api/empleados/liquidaciones/$liqApiId/imputar" -Body @{ cajaId = $Context.CajaPrincipalId; fecha = (Get-Date).ToString('yyyy-MM-dd'); importe = 100000; observacion = 'Imputación api'; generarComprobanteSiNoExiste = $true } -AllowFailure
    }
    Invoke-Api -Name 'Empleado suspender api' -Method 'POST' -Path "/api/empleados/$($Context.EmpleadoId)/suspender" -AllowFailure
    Invoke-Api -Name 'Empleado reactivar api' -Method 'POST' -Path "/api/empleados/$($Context.EmpleadoId)/reactivar" -AllowFailure
}

function Save-Report {
    $directory = Split-Path -Parent $ReportPath
    if ($directory -and -not (Test-Path $directory)) {
        New-Item -ItemType Directory -Path $directory | Out-Null
    }

    $summary = [pscustomobject]@{
        BaseUrl = $BaseUrl
        GeneratedAtUtc = [DateTime]::UtcNow
        Total = $script:Results.Count
        Success = @($script:Results | Where-Object { $_.Success }).Count
        Failed = @($script:Results | Where-Object { -not $_.Success }).Count
        Results = $script:Results
    }

    $summary | ConvertTo-Json -Depth 40 | Set-Content -Path $ReportPath -Encoding UTF8
    Write-Host "Reporte generado en $ReportPath" -ForegroundColor Green
}

try {
    if ($ApplyBootstrap) {
        Write-Step 'Aplicando bootstrap SQL'
        if (-not (Test-PsqlAvailable)) {
            throw 'No se encontró psql. Instalalo o quitá -ApplyBootstrap.'
        }
        Invoke-PsqlFile $BootstrapSqlPath
    }

    if ($ApplySupplementalDataset -or ($ApplyBootstrap -and (Test-Path $SupplementalSqlPath))) {
        Write-Step 'Aplicando dataset complementario smoke'
        if (-not (Test-PsqlAvailable)) {
            throw 'No se encontró psql. Instalalo o quitá -ApplySupplementalDataset.'
        }
        Invoke-PsqlFile $SupplementalSqlPath
    }

    if (-not (Test-PsqlAvailable)) {
        throw 'Para la smoke suite completa se requiere psql disponible para resolver IDs y validar la base.'
    }

    Write-Step 'Resolviendo IDs base desde la base de datos'
    $ids = Resolve-Ids

    if (-not $Token) {
        if (-not $JwtSecret) {
            throw 'No se indicó -Token ni -JwtSecret. No es posible generar el bearer token de prueba.'
        }

        Write-Step 'Generando token JWT de desarrollo para admin.local'
        $Token = New-HmacJwt -Secret $JwtSecret -Issuer $JwtIssuer -Audience $JwtAudience -Subject 1 -Email 'admin@zuluia.local'
    }

    Write-Step 'Pruebas anónimas'
    Invoke-AnonymousChecks

    Write-Step 'Lookups autenticados'
    Invoke-Api -Name 'Tipos de comprobante venta' -Method 'GET' -Path '/api/comprobantes/tipos?esVenta=true'
    Invoke-Api -Name 'Tipos de comprobante compra' -Method 'GET' -Path '/api/comprobantes/tipos?esCompra=true'
    Invoke-Api -Name 'Estados de comprobante' -Method 'GET' -Path '/api/comprobantes/estados'
    Invoke-Api -Name 'Tipos de caja' -Method 'GET' -Path '/api/cajas/tipos'
    Invoke-Api -Name 'Cajas por sucursal' -Method 'GET' -Path "/api/cajas?sucursalId=$($ids.SucursalId)"
    Invoke-Api -Name 'Movimientos de tesorería' -Method 'GET' -Path "/api/tesoreria/movimientos?cajaCuentaId=$($ids.CajaPrincipalId)"
    Invoke-Api -Name 'Cheques paginados' -Method 'GET' -Path '/api/cheques?page=1&pageSize=10'
    Invoke-Api -Name 'Cobros paginados' -Method 'GET' -Path '/api/cobros?page=1&pageSize=10'
    Invoke-Api -Name 'Pagos paginados' -Method 'GET' -Path '/api/pagos?page=1&pageSize=10'

    Write-Step 'Creando caja secundaria'
    Invoke-Api -Name 'Crear caja secundaria' -Method 'POST' -Path '/api/cajas' -Body @{ sucursalId = $ids.SucursalId; tipoId = $ids.TipoCajaId; descripcion = 'CAJA SMOKE'; monedaId = $ids.MonedaArsId; esCaja = $true; banco = $null; nroCuenta = $null; cbu = $null; usuarioId = 1 } -AllowFailure
    $cajaSecundariaId = [long](Invoke-PsqlScalar "select id from cajas_cuentas_bancarias where descripcion = 'CAJA SMOKE' order by id desc limit 1")
    Invoke-Api -Name 'Abrir caja principal' -Method 'POST' -Path "/api/cajas/$($ids.CajaPrincipalId)/abrir" -Body @{ fechaApertura = (Get-Date).ToString('yyyy-MM-dd'); saldoInicial = 100000; observacion = 'Smoke open principal' } -AllowFailure
    Invoke-Api -Name 'Abrir caja secundaria' -Method 'POST' -Path "/api/cajas/$cajaSecundariaId/abrir" -Body @{ fechaApertura = (Get-Date).ToString('yyyy-MM-dd'); saldoInicial = 50000; observacion = 'Smoke open secundaria' } -AllowFailure

    Write-Step 'Creando documento de venta'
    $ventaCreate = Invoke-Api -Name 'Crear nota de pedido' -Method 'POST' -Path '/api/ventas/notas-pedido' -Body @{ sucursalId = $ids.SucursalId; puntoFacturacionId = $ids.PuntoFacturacionId; tipoComprobanteId = $ids.TipoFacturaVentaId; fecha = (Get-Date).ToString('yyyy-MM-dd'); fechaVencimiento = (Get-Date).AddDays(15).ToString('yyyy-MM-dd'); terceroId = $ids.ClienteId; monedaId = $ids.MonedaArsId; cotizacion = 1; percepciones = 0; observacion = 'Smoke venta'; comprobanteOrigenId = $null; items = @(@{ itemId = $ids.ItemId; descripcion = 'ITEM-GENERICO'; cantidad = 1; cantidadBonificada = 0; precioUnitario = 1000; descuentoPct = 0; alicuotaIvaId = $ids.Alicuota21Id; depositoId = $ids.DepositoId; orden = 1 }) }
    $ventaId = [long]$ventaCreate.Data.id
    Invoke-Api -Name 'Emitir venta' -Method 'POST' -Path "/api/ventas/documentos/$ventaId/emitir" -Body @{ afectaStock = $true; afectaCuentaCorriente = $true }
    Invoke-Api -Name 'Detalle venta' -Method 'GET' -Path "/api/ventas/documentos/$ventaId"
    Invoke-Api -Name 'Saldo pendiente tercero' -Method 'GET' -Path "/api/comprobantes/saldo-pendiente?terceroId=$($ids.ClienteId)&sucursalId=$($ids.SucursalId)"

    Write-Step 'Registrando cobro'
    $cobroCreate = Invoke-Api -Name 'Registrar cobro' -Method 'POST' -Path '/api/cobros' -Body @{ sucursalId = $ids.SucursalId; terceroId = $ids.ClienteId; fecha = (Get-Date).ToString('yyyy-MM-dd'); monedaId = $ids.MonedaArsId; cotizacion = 1; observacion = 'Smoke cobro'; medios = @(@{ cajaId = $ids.CajaPrincipalId; formaPagoId = $ids.FormaPagoEfectivoId; chequeId = $null; importe = 1210; monedaId = $ids.MonedaArsId; cotizacion = 1 }); comprobantesAImputar = @(@{ comprobanteId = $ventaId; importe = 1210 }) }
    $cobroId = [long]$cobroCreate.Data.id
    Invoke-Api -Name 'Detalle cobro' -Method 'GET' -Path "/api/cobros/$cobroId"

    Write-Step 'Creando orden de compra'
    $ordenCreate = Invoke-Api -Name 'Crear orden de compra' -Method 'POST' -Path '/api/compras/ordenes' -Body @{ sucursalId = $ids.SucursalId; puntoFacturacionId = $ids.PuntoFacturacionId; tipoComprobanteId = $ids.TipoOrdenCompraId; fecha = (Get-Date).ToString('yyyy-MM-dd'); fechaVencimiento = (Get-Date).AddDays(10).ToString('yyyy-MM-dd'); terceroId = $ids.ProveedorId; monedaId = $ids.MonedaArsId; cotizacion = 1; percepciones = 0; observacion = 'Smoke orden compra'; fechaEntregaReq = (Get-Date).AddDays(5).ToString('yyyy-MM-dd'); condicionesEntrega = 'Entrega parcial permitida'; items = @(@{ itemId = $ids.ItemId; descripcion = 'ITEM-GENERICO'; cantidad = 2; cantidadBonificada = 0; precioUnitario = 900; descuentoPct = 0; alicuotaIvaId = $ids.Alicuota21Id; depositoId = $ids.DepositoId; orden = 1 }) }
    $ordenId = [long]$ordenCreate.Data.id
    Invoke-Api -Name 'Detalle orden de compra' -Method 'GET' -Path "/api/compras/ordenes/$ordenId"
    Invoke-Api -Name 'Listado ordenes de compra' -Method 'GET' -Path "/api/compras/ordenes?proveedorId=$($ids.ProveedorId)"

    Write-Step 'Registrando pago simple'
    $pagoCreate = Invoke-Api -Name 'Registrar pago' -Method 'POST' -Path '/api/pagos' -Body @{ sucursalId = $ids.SucursalId; terceroId = $ids.ProveedorId; fecha = (Get-Date).ToString('yyyy-MM-dd'); monedaId = $ids.MonedaArsId; cotizacion = 1; observacion = 'Smoke pago'; medios = @(@{ cajaId = $ids.CajaPrincipalId; formaPagoId = $ids.FormaPagoEfectivoId; chequeId = $null; importe = 500; monedaId = $ids.MonedaArsId; cotizacion = 1 }); retenciones = @(); comprobantesAImputar = @() }
    $pagoId = [long]$pagoCreate.Data.id
    Invoke-Api -Name 'Detalle pago' -Method 'GET' -Path "/api/pagos/$pagoId"

    Write-Step 'Registrando cheque'
    $chequeCreate = Invoke-Api -Name 'Crear cheque' -Method 'POST' -Path '/api/cheques' -Body @{ cajaId = $ids.CajaPrincipalId; terceroId = $ids.ClienteId; nroCheque = "SMK$([DateTime]::UtcNow.ToString('HHmmss'))"; banco = 'BANCO SMOKE'; fechaEmision = (Get-Date).ToString('yyyy-MM-dd'); fechaVencimiento = (Get-Date).AddDays(30).ToString('yyyy-MM-dd'); importe = 2500; monedaId = $ids.MonedaArsId; observacion = 'Smoke cheque' }
    $chequeId = [long]$chequeCreate.Data.id
    Invoke-Api -Name 'Depositar cheque' -Method 'POST' -Path "/api/cheques/$chequeId/depositar" -Body @{ fechaDeposito = (Get-Date).ToString('yyyy-MM-dd'); fechaAcreditacion = (Get-Date).AddDays(2).ToString('yyyy-MM-dd') } -AllowFailure
    Invoke-Api -Name 'Acreditar cheque' -Method 'POST' -Path "/api/cheques/$chequeId/acreditar" -Body @{ fechaAcreditacion = (Get-Date).AddDays(2).ToString('yyyy-MM-dd') } -AllowFailure
    Invoke-Api -Name 'Historial cheque' -Method 'GET' -Path "/api/cheques/$chequeId/historial"

    Write-Step 'Registrando transferencia entre cajas'
    Invoke-Api -Name 'Transferencia entre cajas' -Method 'POST' -Path '/api/cajas/transferencias' -Body @{ sucursalId = $ids.SucursalId; cajaOrigenId = $ids.CajaPrincipalId; cajaDestinoId = $cajaSecundariaId; fecha = (Get-Date).ToString('yyyy-MM-dd'); importe = 300; monedaId = $ids.MonedaArsId; cotizacion = 1; concepto = 'Smoke transferencia' } -AllowFailure
    Invoke-Api -Name 'Transferencias caja principal' -Method 'GET' -Path "/api/cajas/$($ids.CajaPrincipalId)/transferencias"

    Write-Step 'Auditoría tesorería y cierres'
    Invoke-Api -Name 'Auditoría tesorería' -Method 'GET' -Path "/api/tesoreria/auditoria?cajaCuentaId=$($ids.CajaPrincipalId)"
    Invoke-Api -Name 'Cierres caja principal' -Method 'GET' -Path "/api/tesoreria/cajas/$($ids.CajaPrincipalId)/cierres"

    Write-Step 'Cerrando cajas'
    Invoke-Api -Name 'Cerrar caja secundaria' -Method 'POST' -Path "/api/tesoreria/cajas/$cajaSecundariaId/cerrar" -Body @{ fechaCierre = (Get-Date).ToString('yyyy-MM-dd'); saldoInformado = 50300; observacion = 'Smoke close secundaria' } -AllowFailure
    Invoke-Api -Name 'Cerrar caja principal' -Method 'POST' -Path "/api/tesoreria/cajas/$($ids.CajaPrincipalId)/cerrar" -Body @{ fechaCierre = (Get-Date).ToString('yyyy-MM-dd'); saldoInformado = 100410; observacion = 'Smoke close principal' } -AllowFailure

    Write-Step 'Consultas finales básicas'
    Invoke-Api -Name 'Comprobante detalle' -Method 'GET' -Path "/api/comprobantes/$ventaId"
    Invoke-Api -Name 'Imputaciones destino' -Method 'GET' -Path "/api/imputaciones/destino/$ventaId"
    Invoke-Api -Name 'Imputaciones historial' -Method 'GET' -Path "/api/imputaciones/historial/$ventaId"
    Invoke-Api -Name 'Caja secundaria detalle' -Method 'GET' -Path "/api/cajas/$cajaSecundariaId"

    $context = Resolve-Ids | Select-Object *
    Add-ContextValue -Context $context -Name 'VentaId' -Value $ventaId
    Add-ContextValue -Context $context -Name 'CobroId' -Value $cobroId
    Add-ContextValue -Context $context -Name 'PagoId' -Value $pagoId
    Add-ContextValue -Context $context -Name 'ChequeId' -Value $chequeId
    Add-ContextValue -Context $context -Name 'CajaSecundariaId' -Value $cajaSecundariaId
    Add-ContextValue -Context $context -Name 'OrdenCompraId' -Value $ordenId
    Add-ContextValue -Context $context -Name 'CompraDocumentoId' -Value ([long](Invoke-PsqlScalar "select comprobante_id from ordenes_compra_meta where id = $ordenId"))

    Write-Step 'Expandiendo datos dependientes post-flujo'
    Invoke-PostFlowDataExpansion -Context $context

    Write-Step 'Ejecutando mutaciones de maestros'
    Invoke-MasterDataMutationScenarios -Context $context

    Write-Step 'Ejecutando mutaciones extendidas por módulo'
    Invoke-ExtendedMutationScenarios -Context $context

    Write-Step 'Sweep ampliado de GETs discoverables'
    Invoke-DiscoveredGetSmoke -Context $context

    Save-Report

    $failed = @($script:Results | Where-Object { -not $_.Success })
    if ($failed.Count -gt 0) {
        Write-Warning "Smoke finalizada con $($failed.Count) requests no exitosos. Revisá $ReportPath."
    }
    else {
        Write-Host 'Smoke finalizada sin errores HTTP.' -ForegroundColor Green
    }
}
finally {
    $script:HttpClient.Dispose()
}
