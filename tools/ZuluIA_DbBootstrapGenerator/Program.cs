using System.Reflection;
using System.Text;
using Microsoft.EntityFrameworkCore;
using ZuluIA_Back.Api.Security;
using ZuluIA_Back.Domain.Enums;
using ZuluIA_Back.Infrastructure.Persistence;

var outputPath = args.Length > 0
    ? Path.GetFullPath(args[0])
    : Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", "database", "zuluia_back_full_compatible.md"));

Directory.CreateDirectory(Path.GetDirectoryName(outputPath)!);

var options = new DbContextOptionsBuilder<AppDbContext>()
    .UseNpgsql("Host=localhost;Database=zuluia_bootstrap;Username=postgres;Password=postgres")
    .UseSnakeCaseNamingConvention()
    .Options;

using var db = new AppDbContext(options);
var schemaSql = db.Database.GenerateCreateScript();
var permissions = Assembly.GetAssembly(typeof(RequirePermissionAttribute))!
    .GetTypes()
    .Where(t => t.IsClass && t.Name.EndsWith("Controller", StringComparison.Ordinal))
    .SelectMany(t => t.GetCustomAttributes<RequirePermissionAttribute>(true)
        .Select(a => a.Permission)
        .Concat(t.GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.DeclaredOnly)
            .SelectMany(m => m.GetCustomAttributes<RequirePermissionAttribute>(true).Select(a => a.Permission))))
    .Distinct(StringComparer.OrdinalIgnoreCase)
    .OrderBy(x => x)
    .ToList();

var sql = new StringBuilder();
sql.AppendLine("-- ZuluIA_Back full bootstrap SQL");
sql.AppendLine("-- Generado desde el modelo actual (.NET 8 / EF Core) y completado con seeds mínimos operativos.");
sql.AppendLine("-- Fuente prioritaria: ZuluIA_Back actual. Compatibilidad legacy tomada de SQLDBSUPABASE.sql y zuluApp cuando aplica.");
sql.AppendLine();
sql.AppendLine("BEGIN;");
sql.AppendLine();
sql.AppendLine(schemaSql.Trim());
sql.AppendLine();
sql.AppendLine("-- =====================================================================");
sql.AppendLine("-- Seeds mínimos / compatibilidad operativa");
sql.AppendLine("-- =====================================================================");
sql.AppendLine();
AppendReferenceSeeds(sql);
AppendOperationalSeeds(sql, permissions);
sql.AppendLine();
sql.AppendLine("COMMIT;");

var markdown = new StringBuilder();
markdown.AppendLine($"# SQL bootstrap: `{Path.GetFileNameWithoutExtension(outputPath)}`");
markdown.AppendLine();
markdown.AppendLine("Generado desde el modelo actual para ejecutar manualmente sobre PostgreSQL local.");
markdown.AppendLine();
markdown.AppendLine("```sql");
markdown.Append(sql);
markdown.AppendLine("```");

File.WriteAllText(outputPath, markdown.ToString(), new UTF8Encoding(false));
Console.WriteLine($"Documento SQL generado en: {outputPath}");

static void AppendReferenceSeeds(StringBuilder sql)
{
    sql.AppendLine("-- Países");
    AppendInsertIfMissing(sql, "paises", "codigo", "'AR'", "codigo, descripcion, created_at, updated_at", "'AR', 'ARGENTINA', now(), now()");
    AppendInsertIfMissing(sql, "paises", "codigo", "'PY'", "codigo, descripcion, created_at, updated_at", "'PY', 'PARAGUAY', now(), now()");
    sql.AppendLine();

    sql.AppendLine("-- Monedas");
    AppendInsertIfMissing(sql, "monedas", "codigo", "'ARS'", "codigo, descripcion, simbolo, sin_decimales, activa, created_at, updated_at", "'ARS', 'PESO ARGENTINO', '$', false, true, now(), now()");
    AppendInsertIfMissing(sql, "monedas", "codigo", "'USD'", "codigo, descripcion, simbolo, sin_decimales, activa, created_at, updated_at", "'USD', 'DÓLAR ESTADOUNIDENSE', 'US$', false, true, now(), now()");
    AppendInsertIfMissing(sql, "monedas", "codigo", "'PYG'", "codigo, descripcion, simbolo, sin_decimales, activa, created_at, updated_at", "'PYG', 'GUARANÍ', '₲', true, true, now(), now()");
    sql.AppendLine();

    sql.AppendLine("-- Tipos de documento");
    AppendInsertIfMissing(sql, "tipos_documento", "codigo", "80", "codigo, descripcion", "80, 'CUIT'");
    AppendInsertIfMissing(sql, "tipos_documento", "codigo", "86", "codigo, descripcion", "86, 'CUIL'");
    AppendInsertIfMissing(sql, "tipos_documento", "codigo", "96", "codigo, descripcion", "96, 'DNI'");
    AppendInsertIfMissing(sql, "tipos_documento", "codigo", "99", "codigo, descripcion", "99, 'CONSUMIDOR FINAL'");
    sql.AppendLine();

    sql.AppendLine("-- Condiciones IVA");
    AppendInsertIfMissing(sql, "condiciones_iva", "codigo", "1", "codigo, descripcion", "1, 'RESPONSABLE INSCRIPTO'");
    AppendInsertIfMissing(sql, "condiciones_iva", "codigo", "4", "codigo, descripcion", "4, 'IVA EXENTO'");
    AppendInsertIfMissing(sql, "condiciones_iva", "codigo", "5", "codigo, descripcion", "5, 'CONSUMIDOR FINAL'");
    AppendInsertIfMissing(sql, "condiciones_iva", "codigo", "6", "codigo, descripcion", "6, 'MONOTRIBUTO'");
    sql.AppendLine();

    sql.AppendLine("-- Alícuotas IVA");
    AppendInsertIfMissing(sql, "alicuotas_iva", "codigo", "0", "codigo, descripcion, porcentaje", "0, 'NO GRAVADO', 0.00");
    AppendInsertIfMissing(sql, "alicuotas_iva", "codigo", "3", "codigo, descripcion, porcentaje", "3, 'IVA 10.5%', 10.50");
    AppendInsertIfMissing(sql, "alicuotas_iva", "codigo", "5", "codigo, descripcion, porcentaje", "5, 'IVA 21%', 21.00");
    AppendInsertIfMissing(sql, "alicuotas_iva", "codigo", "6", "codigo, descripcion, porcentaje", "6, 'IVA 27%', 27.00");
    sql.AppendLine();

    sql.AppendLine("-- Unidades de medida");
    AppendInsertIfMissing(sql, "unidades_medida", "codigo", "'UN'", "codigo, descripcion", "'UN', 'UNIDAD'");
    AppendInsertIfMissing(sql, "unidades_medida", "codigo", "'KG'", "codigo, descripcion", "'KG', 'KILOGRAMO'");
    AppendInsertIfMissing(sql, "unidades_medida", "codigo", "'LT'", "codigo, descripcion", "'LT', 'LITRO'");
    AppendInsertIfMissing(sql, "unidades_medida", "codigo", "'HS'", "codigo, descripcion", "'HS', 'HORA'");
    sql.AppendLine();

    sql.AppendLine("-- Formas de pago");
    AppendInsertIfMissing(sql, "formas_pago", "descripcion", "'EFECTIVO'", "descripcion, activa", "'EFECTIVO', true");
    AppendInsertIfMissing(sql, "formas_pago", "descripcion", "'TRANSFERENCIA'", "descripcion, activa", "'TRANSFERENCIA', true");
    AppendInsertIfMissing(sql, "formas_pago", "descripcion", "'CHEQUE'", "descripcion, activa", "'CHEQUE', true");
    AppendInsertIfMissing(sql, "formas_pago", "descripcion", "'TARJETA'", "descripcion, activa", "'TARJETA', true");
    AppendInsertIfMissing(sql, "formas_pago", "descripcion", "'RETENCION IIBB MISIONES'", "descripcion, activa", "'RETENCION IIBB MISIONES', true");
    sql.AppendLine();

    sql.AppendLine("-- Categorías de terceros");
    AppendInsertIfMissing(sql, "categorias_terceros", "descripcion", "'GENERAL'", "descripcion, es_importador", "'GENERAL', false");
    AppendInsertIfMissing(sql, "categorias_terceros", "descripcion", "'IMPORTADOR'", "descripcion, es_importador", "'IMPORTADOR', true");
    sql.AppendLine();

    sql.AppendLine("-- Tipos de caja/cuenta");
    AppendInsertIfMissing(sql, "tipos_caja_cuenta", "descripcion", "'CAJA'", "descripcion, es_caja", "'CAJA', true");
    AppendInsertIfMissing(sql, "tipos_caja_cuenta", "descripcion", "'CUENTA BANCARIA'", "descripcion, es_caja", "'CUENTA BANCARIA', false");
    sql.AppendLine();

    sql.AppendLine("-- Tipos de punto de facturación");
    AppendInsertIfMissing(sql, "tipos_punto_facturacion", "descripcion", "'MANUAL'", "descripcion, por_defecto", "'MANUAL', true");
    AppendInsertIfMissing(sql, "tipos_punto_facturacion", "descripcion", "'ELECTRONICO'", "descripcion, por_defecto", "'ELECTRONICO', false");
    sql.AppendLine();

    sql.AppendLine("-- Tipos de comprobante mínimos");
    AppendInsertIfMissing(sql, "tipos_comprobante", "codigo", "'FACA'", "codigo, descripcion, es_venta, es_compra, es_interno, afecta_stock, afecta_cta_cte, genera_asiento, tipo_afip, letra_afip, activo", "'FACA', 'FACTURA A', true, false, false, false, true, true, 1, 'A', true");
    AppendInsertIfMissing(sql, "tipos_comprobante", "codigo", "'FACB'", "codigo, descripcion, es_venta, es_compra, es_interno, afecta_stock, afecta_cta_cte, genera_asiento, tipo_afip, letra_afip, activo", "'FACB', 'FACTURA B', true, false, false, false, true, true, 6, 'B', true");
    AppendInsertIfMissing(sql, "tipos_comprobante", "codigo", "'FACC'", "codigo, descripcion, es_venta, es_compra, es_interno, afecta_stock, afecta_cta_cte, genera_asiento, tipo_afip, letra_afip, activo", "'FACC', 'FACTURA C', true, false, false, false, true, true, 11, 'C', true");
    AppendInsertIfMissing(sql, "tipos_comprobante", "codigo", "'NCA'", "codigo, descripcion, es_venta, es_compra, es_interno, afecta_stock, afecta_cta_cte, genera_asiento, tipo_afip, letra_afip, activo", "'NCA', 'NOTA DE CREDITO A', true, false, false, false, true, true, 3, 'A', true");
    AppendInsertIfMissing(sql, "tipos_comprobante", "codigo", "'NCB'", "codigo, descripcion, es_venta, es_compra, es_interno, afecta_stock, afecta_cta_cte, genera_asiento, tipo_afip, letra_afip, activo", "'NCB', 'NOTA DE CREDITO B', true, false, false, false, true, true, 8, 'B', true");
    AppendInsertIfMissing(sql, "tipos_comprobante", "codigo", "'NDA'", "codigo, descripcion, es_venta, es_compra, es_interno, afecta_stock, afecta_cta_cte, genera_asiento, tipo_afip, letra_afip, activo", "'NDA', 'NOTA DE DEBITO A', true, false, false, false, true, true, 2, 'A', true");
    AppendInsertIfMissing(sql, "tipos_comprobante", "codigo", "'NDB'", "codigo, descripcion, es_venta, es_compra, es_interno, afecta_stock, afecta_cta_cte, genera_asiento, tipo_afip, letra_afip, activo", "'NDB', 'NOTA DE DEBITO B', true, false, false, false, true, true, 7, 'B', true");
    AppendInsertIfMissing(sql, "tipos_comprobante", "codigo", "'REM'", "codigo, descripcion, es_venta, es_compra, es_interno, afecta_stock, afecta_cta_cte, genera_asiento, activo", "'REM', 'REMITO', true, false, false, true, false, false, true");
    AppendInsertIfMissing(sql, "tipos_comprobante", "codigo", "'OC'", "codigo, descripcion, es_venta, es_compra, es_interno, afecta_stock, afecta_cta_cte, genera_asiento, activo", "'OC', 'ORDEN DE COMPRA', false, true, false, false, false, false, true");
    AppendInsertIfMissing(sql, "tipos_comprobante", "codigo", "'FCIC'", "codigo, descripcion, es_venta, es_compra, es_interno, afecta_stock, afecta_cta_cte, genera_asiento, activo", "'FCIC', 'FACTURA COMPRA IMPORTACION CONTADO', false, true, false, false, true, true, true");
    AppendInsertIfMissing(sql, "tipos_comprobante", "codigo", "'FCCR'", "codigo, descripcion, es_venta, es_compra, es_interno, afecta_stock, afecta_cta_cte, genera_asiento, activo", "'FCCR', 'FACTURA COMPRA IMPORTACION CREDITO', false, true, false, false, true, true, true");
    sql.AppendLine();
}

static void AppendOperationalSeeds(StringBuilder sql, IReadOnlyList<string> permissions)
{
    sql.AppendLine("-- Usuario administrador local y sucursal base");
    sql.AppendLine("INSERT INTO sucursales (razon_social, nombre_fantasia, cuit, condicion_iva_id, moneda_id, pais_id, puerto_afip, casa_matriz, activa, created_at, updated_at)");
    sql.AppendLine("SELECT 'SUCURSAL PRINCIPAL', 'CASA CENTRAL', '30712345678', ci.id, m.id, p.id, 443, true, true, now(), now()");
    sql.AppendLine("FROM condiciones_iva ci, monedas m, paises p");
    sql.AppendLine("WHERE ci.codigo = 1 AND m.codigo = 'ARS' AND p.codigo = 'AR'");
    sql.AppendLine("AND NOT EXISTS (SELECT 1 FROM sucursales WHERE cuit = '30712345678');");
    sql.AppendLine();

    sql.AppendLine("INSERT INTO usuarios (usuario, nombre_completo, email, activo, created_at, updated_at)");
    sql.AppendLine("SELECT 'admin.local', 'Administrador Local', 'admin@zuluia.local', true, now(), now()");
    sql.AppendLine("WHERE NOT EXISTS (SELECT 1 FROM usuarios WHERE usuario = 'admin.local');");
    sql.AppendLine();

    sql.AppendLine("INSERT INTO usuarios_sucursal (usuario_id, sucursal_id)");
    sql.AppendLine("SELECT u.id, s.id FROM usuarios u CROSS JOIN sucursales s");
    sql.AppendLine("WHERE u.usuario = 'admin.local' AND s.cuit = '30712345678'");
    sql.AppendLine("AND NOT EXISTS (SELECT 1 FROM usuarios_sucursal us WHERE us.usuario_id = u.id AND us.sucursal_id = s.id);");
    sql.AppendLine();

    sql.AppendLine("-- Punto de facturación y caja base");
    sql.AppendLine("INSERT INTO puntos_facturacion (sucursal_id, tipo_id, numero, descripcion, activo, created_at, updated_at)");
    sql.AppendLine("SELECT s.id, t.id, 1, 'PUNTO DE FACTURACION PRINCIPAL', true, now(), now() FROM sucursales s CROSS JOIN tipos_punto_facturacion t");
    sql.AppendLine("WHERE s.cuit = '30712345678' AND t.descripcion = 'MANUAL'");
    sql.AppendLine("AND NOT EXISTS (SELECT 1 FROM puntos_facturacion pf WHERE pf.sucursal_id = s.id AND pf.numero = 1);");
    sql.AppendLine();

    sql.AppendLine("INSERT INTO afip_wsfe_configuracion (sucursal_id, punto_facturacion_id, habilitado, produccion, usa_caea_por_defecto, cuit_emisor, certificado_alias, observacion, created_at, updated_at)");
    sql.AppendLine("SELECT s.id, pf.id, true, false, false, s.cuit, NULL, 'Configuración AFIP inicial generada por bootstrap', now(), now()");
    sql.AppendLine("FROM sucursales s JOIN puntos_facturacion pf ON pf.sucursal_id = s.id");
    sql.AppendLine("WHERE s.cuit = '30712345678' AND pf.numero = 1");
    sql.AppendLine("AND NOT EXISTS (SELECT 1 FROM afip_wsfe_configuracion a WHERE a.sucursal_id = s.id AND a.punto_facturacion_id = pf.id);");
    sql.AppendLine();

    sql.AppendLine("INSERT INTO timbrados_fiscales (sucursal_id, punto_facturacion_id, numero_timbrado, vigencia_desde, vigencia_hasta, activo, observacion, created_at, updated_at)");
    sql.AppendLine("SELECT s.id, pf.id, 'TIMBRADO-INICIAL', CURRENT_DATE, (CURRENT_DATE + INTERVAL '365 day')::date, true, 'Timbrado inicial generado por bootstrap', now(), now()");
    sql.AppendLine("FROM sucursales s JOIN puntos_facturacion pf ON pf.sucursal_id = s.id");
    sql.AppendLine("WHERE s.cuit = '30712345678' AND pf.numero = 1");
    sql.AppendLine("AND NOT EXISTS (SELECT 1 FROM timbrados_fiscales t WHERE t.punto_facturacion_id = pf.id AND t.numero_timbrado = 'TIMBRADO-INICIAL');");
    sql.AppendLine();

    sql.AppendLine("INSERT INTO cajas_cuentas_bancarias (sucursal_id, tipo_id, descripcion, moneda_id, nro_cierre_actual, es_caja, activa, esta_abierta, saldo_apertura, created_at, updated_at)");
    sql.AppendLine("SELECT s.id, t.id, 'CAJA PRINCIPAL', m.id, 0, true, true, false, 0, now(), now() FROM sucursales s CROSS JOIN tipos_caja_cuenta t CROSS JOIN monedas m");
    sql.AppendLine("WHERE s.cuit = '30712345678' AND t.descripcion = 'CAJA' AND m.codigo = 'ARS'");
    sql.AppendLine("AND NOT EXISTS (SELECT 1 FROM cajas_cuentas_bancarias c WHERE c.sucursal_id = s.id AND c.descripcion = 'CAJA PRINCIPAL');");
    sql.AppendLine();

    sql.AppendLine("INSERT INTO forma_pago_caja (caja_id, forma_pago_id, moneda_id, habilitado)");
    sql.AppendLine("SELECT c.id, fp.id, m.id, true FROM cajas_cuentas_bancarias c CROSS JOIN formas_pago fp CROSS JOIN monedas m");
    sql.AppendLine("WHERE c.descripcion = 'CAJA PRINCIPAL' AND m.codigo = 'ARS' AND fp.descripcion IN ('EFECTIVO', 'TRANSFERENCIA', 'CHEQUE')");
    sql.AppendLine("AND NOT EXISTS (SELECT 1 FROM forma_pago_caja fpc WHERE fpc.caja_id = c.id AND fpc.forma_pago_id = fp.id AND fpc.moneda_id = m.id);");
    sql.AppendLine();

    sql.AppendLine("-- Depósito, categoría e ítem base");
    sql.AppendLine("INSERT INTO depositos (sucursal_id, descripcion, es_default, activo, created_at)");
    sql.AppendLine("SELECT s.id, 'DEPÓSITO PRINCIPAL', true, true, now() FROM sucursales s");
    sql.AppendLine("WHERE s.cuit = '30712345678'");
    sql.AppendLine("AND NOT EXISTS (SELECT 1 FROM depositos d WHERE d.sucursal_id = s.id AND d.descripcion = 'DEPÓSITO PRINCIPAL');");
    sql.AppendLine();

    sql.AppendLine("INSERT INTO categorias_items (codigo, descripcion, nivel, orden_nivel, activo, created_at, updated_at)");
    sql.AppendLine("SELECT 'GENERAL', 'GENERAL', 1, '001', true, now(), now()");
    sql.AppendLine("WHERE NOT EXISTS (SELECT 1 FROM categorias_items WHERE codigo = 'GENERAL');");
    sql.AppendLine();

    sql.AppendLine("INSERT INTO items (codigo, descripcion, categoria_id, unidad_medida_id, alicuota_iva_id, moneda_id, precio_costo, precio_venta, es_producto, es_servicio, maneja_stock, stock_minimo, stock_maximo, activo, created_at, updated_at)");
    sql.AppendLine("SELECT 'ITEM-GENERICO', 'ITEM GENÉRICO', c.id, um.id, ai.id, m.id, 0, 0, true, false, true, 0, 0, true, now(), now()");
    sql.AppendLine("FROM categorias_items c CROSS JOIN unidades_medida um CROSS JOIN alicuotas_iva ai CROSS JOIN monedas m");
    sql.AppendLine("WHERE c.codigo = 'GENERAL' AND um.codigo = 'UN' AND ai.codigo = 5 AND m.codigo = 'ARS'");
    sql.AppendLine("AND NOT EXISTS (SELECT 1 FROM items WHERE codigo = 'ITEM-GENERICO');");
    sql.AppendLine();

    sql.AppendLine("-- Plan de pago y cotización base");
    sql.AppendLine("INSERT INTO planes_pago (descripcion, cantidad_cuotas, interes_pct, activo, created_at)");
    sql.AppendLine("SELECT 'CONTADO', 1, 0, true, now()");
    sql.AppendLine("WHERE NOT EXISTS (SELECT 1 FROM planes_pago WHERE descripcion = 'CONTADO');");
    sql.AppendLine("INSERT INTO planes_pago (descripcion, cantidad_cuotas, interes_pct, activo, created_at)");
    sql.AppendLine("SELECT '30 DÍAS', 1, 0, true, now()");
    sql.AppendLine("WHERE NOT EXISTS (SELECT 1 FROM planes_pago WHERE descripcion = '30 DÍAS');");
    sql.AppendLine();

    sql.AppendLine("INSERT INTO cotizaciones_moneda (moneda_id, fecha, cotizacion, created_at)");
    sql.AppendLine("SELECT m.id, CURRENT_DATE, 1, now() FROM monedas m");
    sql.AppendLine("WHERE m.codigo = 'ARS'");
    sql.AppendLine("AND NOT EXISTS (SELECT 1 FROM cotizaciones_moneda cm WHERE cm.moneda_id = m.id AND cm.fecha = CURRENT_DATE);");
    sql.AppendLine("INSERT INTO cotizaciones_moneda (moneda_id, fecha, cotizacion, created_at)");
    sql.AppendLine("SELECT m.id, CURRENT_DATE, 1000, now() FROM monedas m");
    sql.AppendLine("WHERE m.codigo = 'USD'");
    sql.AppendLine("AND NOT EXISTS (SELECT 1 FROM cotizaciones_moneda cm WHERE cm.moneda_id = m.id AND cm.fecha = CURRENT_DATE);");
    sql.AppendLine("INSERT INTO cotizaciones_moneda (moneda_id, fecha, cotizacion, created_at)");
    sql.AppendLine("SELECT m.id, CURRENT_DATE, 0.125, now() FROM monedas m");
    sql.AppendLine("WHERE m.codigo = 'PYG'");
    sql.AppendLine("AND NOT EXISTS (SELECT 1 FROM cotizaciones_moneda cm WHERE cm.moneda_id = m.id AND cm.fecha = CURRENT_DATE);");
    sql.AppendLine();

    sql.AppendLine("-- Maestros comerciales base");
    sql.AppendLine("INSERT INTO marcas_comerciales (codigo, descripcion, activo, created_at, updated_at)");
    sql.AppendLine("SELECT 'GEN', 'GENÉRICA', true, now(), now()");
    sql.AppendLine("WHERE NOT EXISTS (SELECT 1 FROM marcas_comerciales WHERE codigo = 'GEN');");
    sql.AppendLine("INSERT INTO zonas_comerciales (codigo, descripcion, activo, created_at, updated_at)");
    sql.AppendLine("SELECT 'CENTRO', 'ZONA CENTRO', true, now(), now()");
    sql.AppendLine("WHERE NOT EXISTS (SELECT 1 FROM zonas_comerciales WHERE codigo = 'CENTRO');");
    sql.AppendLine("INSERT INTO jurisdicciones_comerciales (codigo, descripcion, activo, created_at, updated_at)");
    sql.AppendLine("SELECT 'AR-GRAL', 'JURISDICCIÓN GENERAL', true, now(), now()");
    sql.AppendLine("WHERE NOT EXISTS (SELECT 1 FROM jurisdicciones_comerciales WHERE codigo = 'AR-GRAL');");
    sql.AppendLine();

    sql.AppendLine("-- Terceros genéricos base");
    sql.AppendLine("INSERT INTO terceros (legajo, razon_social, tipo_documento_id, nro_documento, condicion_iva_id, categoria_id, es_cliente, es_proveedor, es_empleado, moneda_id, facturable, sucursal_id, activo, created_at, updated_at)");
    sql.AppendLine("SELECT 'CLI0001', 'CLIENTE GENÉRICO', td.id, '0', ci.id, ct.id, true, false, false, m.id, true, s.id, true, now(), now()");
    sql.AppendLine("FROM tipos_documento td CROSS JOIN condiciones_iva ci CROSS JOIN categorias_terceros ct CROSS JOIN monedas m CROSS JOIN sucursales s");
    sql.AppendLine("WHERE td.codigo = 99 AND ci.codigo = 5 AND ct.descripcion = 'GENERAL' AND m.codigo = 'ARS' AND s.cuit = '30712345678'");
    sql.AppendLine("AND NOT EXISTS (SELECT 1 FROM terceros WHERE legajo = 'CLI0001');");
    sql.AppendLine();

    sql.AppendLine("INSERT INTO terceros (legajo, razon_social, tipo_documento_id, nro_documento, condicion_iva_id, categoria_id, es_cliente, es_proveedor, es_empleado, moneda_id, facturable, sucursal_id, activo, created_at, updated_at)");
    sql.AppendLine("SELECT 'PRV0001', 'PROVEEDOR GENÉRICO', td.id, '30000000000', ci.id, ct.id, false, true, false, m.id, true, s.id, true, now(), now()");
    sql.AppendLine("FROM tipos_documento td CROSS JOIN condiciones_iva ci CROSS JOIN categorias_terceros ct CROSS JOIN monedas m CROSS JOIN sucursales s");
    sql.AppendLine("WHERE td.codigo = 80 AND ci.codigo = 1 AND ct.descripcion = 'GENERAL' AND m.codigo = 'ARS' AND s.cuit = '30712345678'");
    sql.AppendLine("AND NOT EXISTS (SELECT 1 FROM terceros WHERE legajo = 'PRV0001');");
    sql.AppendLine();

    sql.AppendLine("INSERT INTO terceros (legajo, razon_social, tipo_documento_id, nro_documento, condicion_iva_id, categoria_id, es_cliente, es_proveedor, es_empleado, moneda_id, facturable, sucursal_id, activo, created_at, updated_at)");
    sql.AppendLine("SELECT 'TRA0001', 'TRANSPORTISTA GENÉRICO', td.id, '30000000001', ci.id, ct.id, false, true, false, m.id, true, s.id, true, now(), now()");
    sql.AppendLine("FROM tipos_documento td CROSS JOIN condiciones_iva ci CROSS JOIN categorias_terceros ct CROSS JOIN monedas m CROSS JOIN sucursales s");
    sql.AppendLine("WHERE td.codigo = 80 AND ci.codigo = 1 AND ct.descripcion = 'GENERAL' AND m.codigo = 'ARS' AND s.cuit = '30712345678'");
    sql.AppendLine("AND NOT EXISTS (SELECT 1 FROM terceros WHERE legajo = 'TRA0001');");
    sql.AppendLine();

    sql.AppendLine("INSERT INTO transportistas (tercero_id, nro_cuit_transportista, domicilio_partida, patente, marca_vehiculo, activo, created_at, updated_at)");
    sql.AppendLine("SELECT t.id, '30000000001', 'DOMICILIO BASE', 'AAA000', 'GENÉRICO', true, now(), now() FROM terceros t");
    sql.AppendLine("WHERE t.legajo = 'TRA0001'");
    sql.AppendLine("AND NOT EXISTS (SELECT 1 FROM transportistas tr WHERE tr.tercero_id = t.id);");
    sql.AppendLine();

    sql.AppendLine("-- Contabilidad base");
    sql.AppendLine("INSERT INTO ejercicios (descripcion, fecha_inicio, fecha_fin, mascara_cuenta_contable, cerrado, created_at, updated_at)");
    sql.AppendLine("SELECT 'EJERCICIO ACTUAL', date_trunc('year', CURRENT_DATE)::date, (date_trunc('year', CURRENT_DATE) + INTERVAL '1 year - 1 day')::date, '00.00.00.00', false, now(), now()");
    sql.AppendLine("WHERE NOT EXISTS (SELECT 1 FROM ejercicios WHERE CURRENT_DATE BETWEEN fecha_inicio AND fecha_fin);");
    sql.AppendLine();

    sql.AppendLine("INSERT INTO ejercicio_sucursal (ejercicio_id, sucursal_id, usa_contabilidad, created_at)");
    sql.AppendLine("SELECT e.id, s.id, true, now() FROM ejercicios e CROSS JOIN sucursales s");
    sql.AppendLine("WHERE CURRENT_DATE BETWEEN e.fecha_inicio AND e.fecha_fin AND s.cuit = '30712345678'");
    sql.AppendLine("AND NOT EXISTS (SELECT 1 FROM ejercicio_sucursal es WHERE es.ejercicio_id = e.id AND es.sucursal_id = s.id);");
    sql.AppendLine();

    sql.AppendLine("INSERT INTO centros_costo (codigo, descripcion, activo, created_at)");
    sql.AppendLine("SELECT 'GENERAL', 'CENTRO DE COSTO GENERAL', true, now()");
    sql.AppendLine("WHERE NOT EXISTS (SELECT 1 FROM centros_costo WHERE codigo = 'GENERAL');");
    sql.AppendLine();

    AppendPlanCuenta(sql, "1", "ACTIVO", 1, "001", false, "ACTIVO", "D");
    AppendPlanCuenta(sql, "2", "PASIVO", 1, "002", false, "PASIVO", "H");
    AppendPlanCuenta(sql, "3", "PATRIMONIO NETO", 1, "003", false, "PATRIMONIO", "H");
    AppendPlanCuenta(sql, "4", "INGRESOS", 1, "004", false, "RESULTADO", "H");
    AppendPlanCuenta(sql, "5", "EGRESOS", 1, "005", false, "RESULTADO", "D");
    AppendPlanCuenta(sql, "1.1", "CAJA Y BANCOS", 2, "001.001", true, "ACTIVO", "D", "1");
    AppendPlanCuenta(sql, "2.1", "PROVEEDORES", 2, "002.001", true, "PASIVO", "H", "2");
    AppendPlanCuenta(sql, "4.1", "VENTAS", 2, "004.001", true, "RESULTADO", "H", "4");
    AppendPlanCuenta(sql, "5.1", "COMPRAS", 2, "005.001", true, "RESULTADO", "D", "5");
    sql.AppendLine();

    sql.AppendLine("-- Tipos de retención base");
    sql.AppendLine("INSERT INTO tipos_retencion (descripcion, regimen, minimo_no_imponible, acumula_pago, tipo_comprobante_id, item_id, activo, created_at, updated_at)");
    sql.AppendLine("SELECT 'RETENCIÓN IIBB', 'IIBB', 0, false, NULL, NULL, true, now(), now()");
    sql.AppendLine("WHERE NOT EXISTS (SELECT 1 FROM tipos_retencion WHERE regimen = 'IIBB');");
    sql.AppendLine("INSERT INTO tipos_retencion (descripcion, regimen, minimo_no_imponible, acumula_pago, tipo_comprobante_id, item_id, activo, created_at, updated_at)");
    sql.AppendLine("SELECT 'RETENCIÓN GANANCIAS', 'GANANCIAS', 0, true, NULL, NULL, true, now(), now()");
    sql.AppendLine("WHERE NOT EXISTS (SELECT 1 FROM tipos_retencion WHERE regimen = 'GANANCIAS');");
    sql.AppendLine();

    sql.AppendLine("-- Configuración operativa base");
    AppendConfig(sql, "OPERACIONES.BATCH.SCHEDULER.HABILITADO", "true", "Habilita scheduler batch operativo");
    AppendConfig(sql, "OPERACIONES.BATCH.SCHEDULER.POLL_SECONDS", "60", "Polling del scheduler batch");
    AppendConfig(sql, "OPERACIONES.BATCH.SCHEDULER.LOTE", "20", "Lote por iteración del scheduler batch");
    AppendConfig(sql, "OPERACIONES.BATCH.SCHEDULER.REINTENTO_ERROR_MINUTOS", "15", "Reintento ante error del scheduler batch");
    AppendConfig(sql, "OPERACIONES.BATCH.SCHEDULER.QUEUE_MODE", "DATABASE", "Modo de cola del scheduler batch");
    AppendConfig(sql, "OPERACIONES.IMPRESION.SPOOL.HABILITADO", "true", "Habilita spool de impresión");
    AppendConfig(sql, "OPERACIONES.IMPRESION.SPOOL.POLL_SECONDS", "15", "Polling del spool");
    AppendConfig(sql, "OPERACIONES.IMPRESION.SPOOL.LOTE", "10", "Lote del spool");
    AppendConfig(sql, "OPERACIONES.IMPRESION.SPOOL.REINTENTO_MINUTOS", "5", "Reintento del spool");
    AppendConfig(sql, "OPERACIONES.IMPRESION.SPOOL.MAX_INTENTOS", "5", "Máximo de intentos del spool");
    AppendConfig(sql, "OPERACIONES.IMPRESION.SPOOL.BACKOFF_FACTOR", "2", "Factor de backoff del spool");
    AppendConfig(sql, "OPERACIONES.IMPRESION.SPOOL.MAX_RETRY_MINUTES", "60", "Máximo de retry del spool");
    AppendConfig(sql, "OPERACIONES.IMPRESION.SPOOL.QUEUE_MODE", "DATABASE", "Modo de cola del spool");
    AppendConfig(sql, "OPERACIONES.IMPORTACION.PARSERS.HABILITADOS", "true", "Habilita parsers de importación");
    AppendConfig(sql, "OPERACIONES.IMPORTACION.LAYOUT_LEGACY_PROFILE", "DEFAULT", "Perfil de compatibilidad de layout legacy");
    AppendConfig(sql, "OPERACIONES.IMPRESION.PDF.LAYOUT_PROFILE", "DEFAULT", "Perfil PDF por defecto");
    AppendConfig(sql, "OPERACIONES.IMPRESION.FISCAL.HABILITADA", "true", "Habilita impresión fiscal");
    AppendConfig(sql, "OPERACIONES.IMPRESION.FISCAL.EPSON.HABILITADA", "true", "Habilita adaptador Epson");
    AppendConfig(sql, "OPERACIONES.IMPRESION.FISCAL.HASAR.HABILITADA", "true", "Habilita adaptador Hasar");
    AppendConfig(sql, "ServerSQLLocalProcesoArchivos", "C:\\Tmp\\", "Path local para proceso de archivos legado");
    AppendConfig(sql, "ServerSQLProcesoArchivos", "\\\\serverApp\\path", "Path compartido para proceso de archivos legado");
    sql.AppendLine();

    foreach (var provider in Enum.GetValues<ProveedorIntegracionExterna>())
    {
        var prefix = $"INTEGRACION.{provider.ToString().ToUpperInvariant()}";
        AppendConfig(sql, $"{prefix}.AMBIENTE", "TEST", $"Ambiente para {provider}");
        AppendConfig(sql, $"{prefix}.HABILITADA", "true", $"Habilita proveedor {provider}");
        AppendConfig(sql, $"{prefix}.ENDPOINT", $"https://test.{provider.ToString().ToLowerInvariant()}.local/api", $"Endpoint base de {provider}");
        AppendConfig(sql, $"{prefix}.TIMEOUT_MS", "15000", $"Timeout de {provider}");
        AppendConfig(sql, $"{prefix}.REINTENTOS", "2", $"Reintentos de {provider}");
        AppendConfig(sql, $"{prefix}.CIRCUIT_THRESHOLD", "3", $"Threshold de circuit breaker de {provider}");
        AppendConfig(sql, $"{prefix}.CIRCUIT_OPEN_SECONDS", "60", $"Ventana de circuit breaker de {provider}");
        AppendConfig(sql, $"{prefix}.API_KEY", "", $"API key de {provider}");
        AppendConfig(sql, $"{prefix}.USERNAME", "", $"Usuario de {provider}");
        AppendConfig(sql, $"{prefix}.PASSWORD", "", $"Password de {provider}");
        AppendConfig(sql, $"{prefix}.TOKEN", "", $"Token de {provider}");
        AppendConfig(sql, $"{prefix}.CERTIFICATE_ALIAS", "", $"Alias de certificado de {provider}");
    }
    sql.AppendLine();

    sql.AppendLine("-- Permisos detectados desde atributos RequirePermission");
    foreach (var permission in permissions)
    {
        var safe = permission.Replace("'", "''");
        sql.AppendLine($"INSERT INTO seguridad (identificador, descripcion, aplica_seguridad_por_usuario)");
        sql.AppendLine($"SELECT '{safe}', 'Permiso generado automáticamente para {safe}', true");
        sql.AppendLine($"WHERE NOT EXISTS (SELECT 1 FROM seguridad WHERE identificador = '{safe}');");
    }
    sql.AppendLine();

    sql.AppendLine("INSERT INTO seguridad_usuario (seguridad_id, usuario_id, valor)");
    sql.AppendLine("SELECT s.id, u.id, true FROM seguridad s CROSS JOIN usuarios u");
    sql.AppendLine("WHERE u.usuario = 'admin.local'");
    sql.AppendLine("AND NOT EXISTS (SELECT 1 FROM seguridad_usuario su WHERE su.seguridad_id = s.id AND su.usuario_id = u.id);");
    sql.AppendLine();

    sql.AppendLine("-- Menú base del frontend");
    AppendMenu(sql, "Dashboard", null, null, "layout-dashboard", 1, 1);
    AppendMenu(sql, "Ventas", null, null, "shopping-cart", 1, 2);
    AppendMenu(sql, "Compras", null, null, "package-plus", 1, 3);
    AppendMenu(sql, "Contabilidad", null, null, "book-open-text", 1, 4);
    AppendMenu(sql, "Tesorería", null, null, "wallet", 1, 4);
    AppendMenu(sql, "Facturación", null, null, "receipt-text", 1, 5);
    AppendMenu(sql, "Inventario", null, null, "boxes", 1, 6);
    AppendMenu(sql, "Logística", null, null, "truck", 1, 7);
    AppendMenu(sql, "Producción", null, null, "factory", 1, 8);
    AppendMenu(sql, "Colegio", null, null, "graduation-cap", 1, 9);
    AppendMenu(sql, "Contratos", null, null, "file-signature", 1, 10);
    AppendMenu(sql, "Punto de Venta", null, null, "store", 1, 11);
    AppendMenu(sql, "Integraciones", null, null, "plug", 1, 12);
    AppendMenu(sql, "Reportes", null, null, "chart-column", 1, 13);
    AppendMenu(sql, "Seguridad", null, null, "shield-check", 1, 14);

    AppendMenu(sql, "Clientes", "/ventas/clientes", "Ventas", "users", 2, 1);
    AppendMenu(sql, "Facturas", "/ventas/facturas", "Ventas", "receipt", 2, 2);
    AppendMenu(sql, "Cobros", "/ventas/cobros", "Ventas", "badge-dollar-sign", 2, 3);
    AppendMenu(sql, "Imputaciones", "/ventas/imputaciones", "Ventas", "link", 2, 4);
    AppendMenu(sql, "Remitos", "/ventas/remitos", "Ventas", "truck", 2, 5);
    AppendMenu(sql, "Listas de precios", "/ventas/listas-precios", "Ventas", "tags", 2, 6);

    AppendMenu(sql, "Proveedores", "/compras/proveedores", "Compras", "users-round", 2, 1);
    AppendMenu(sql, "Órdenes", "/compras/ordenes", "Compras", "file-text", 2, 2);
    AppendMenu(sql, "Recepciones", "/compras/recepciones", "Compras", "package-check", 2, 3);
    AppendMenu(sql, "Facturas compra", "/compras/facturas", "Compras", "receipt", 2, 4);
    AppendMenu(sql, "Devoluciones", "/compras/devoluciones", "Compras", "rotate-ccw", 2, 5);

    AppendMenu(sql, "Asientos", "/contabilidad/asientos", "Contabilidad", "notebook-tabs", 2, 1);
    AppendMenu(sql, "Cajas", "/contabilidad/cajas", "Contabilidad", "wallet", 2, 2);
    AppendMenu(sql, "Cheques", "/contabilidad/cheques", "Contabilidad", "scroll-text", 2, 3);
    AppendMenu(sql, "Pagos", "/contabilidad/pagos", "Contabilidad", "arrow-down-circle", 2, 4);
    AppendMenu(sql, "Retenciones", "/contabilidad/retenciones", "Contabilidad", "percent", 2, 5);
    AppendMenu(sql, "Transferencias", "/contabilidad/transferencias", "Contabilidad", "arrow-right-left", 2, 6);

    AppendMenu(sql, "Productos", "/inventario/productos", "Inventario", "box", 2, 1);
    AppendMenu(sql, "Categorías", "/inventario/categorias", "Inventario", "folder-tree", 2, 2);
    AppendMenu(sql, "Marcas", "/inventario/marcas", "Inventario", "badge-info", 2, 3);
    AppendMenu(sql, "Atributos", "/inventario/atributos", "Inventario", "sliders-horizontal", 2, 4);
    AppendMenu(sql, "Movimientos", "/inventario/movimientos", "Inventario", "arrow-up-down", 2, 5);

    AppendMenu(sql, "Carta Porte", "/almacenes/carta-porte", "Logística", "truck", 2, 1);
    AppendMenu(sql, "Picking", "/almacenes/picking", "Logística", "scan-line", 2, 2);
    AppendMenu(sql, "Transportistas", "/almacenes/transportistas", "Logística", "truck", 2, 3);

    AppendMenu(sql, "Formulas", "/almacenes/formulas-produccion", "Producción", "flask-conical", 2, 1);
    AppendMenu(sql, "Órdenes de trabajo", "/almacenes/ordenes-trabajo", "Producción", "clipboard-list", 2, 2);
    AppendMenu(sql, "Producción", "/almacenes/produccion", "Producción", "factory", 2, 3);

    AppendMenu(sql, "Planes", "/colegio/planes", "Colegio", "notebook-pen", 2, 1);
    AppendMenu(sql, "Cartera", "/colegio/cartera", "Colegio", "wallet-cards", 2, 2);

    AppendMenu(sql, "Operaciones", "/operaciones/dashboard-operativo-final", "Integraciones", "activity", 2, 1);
    AppendMenu(sql, "Importaciones", "/api/integraciones/importar/layouts", "Integraciones", "file-input", 2, 2);

    AppendMenu(sql, "Seguridad", "/api/seguridad", "Seguridad", "shield-check", 2, 1);
    AppendMenu(sql, "Usuarios", "/api/usuarios", "Seguridad", "users", 2, 2);
    sql.AppendLine();

    sql.AppendLine("INSERT INTO menu_usuario (menu_id, usuario_id)");
    sql.AppendLine("SELECT m.id, u.id FROM menu m CROSS JOIN usuarios u");
    sql.AppendLine("WHERE u.usuario = 'admin.local' AND m.activo = true");
    sql.AppendLine("AND NOT EXISTS (SELECT 1 FROM menu_usuario mu WHERE mu.menu_id = m.id AND mu.usuario_id = u.id);");
    sql.AppendLine();

    sql.AppendLine("-- Parámetros operativos base para admin.local");
    sql.AppendLine("INSERT INTO parametros_usuario (usuario_id, clave, valor)");
    sql.AppendLine("SELECT u.id, 'DEFAULT_SUCURSAL_ID', s.id::text FROM usuarios u CROSS JOIN sucursales s");
    sql.AppendLine("WHERE u.usuario = 'admin.local' AND s.cuit = '30712345678'");
    sql.AppendLine("AND NOT EXISTS (SELECT 1 FROM parametros_usuario pu WHERE pu.usuario_id = u.id AND pu.clave = 'DEFAULT_SUCURSAL_ID');");
    sql.AppendLine("INSERT INTO parametros_usuario (usuario_id, clave, valor)");
    sql.AppendLine("SELECT u.id, 'DEFAULT_LAYOUT_PROFILE', 'DEFAULT' FROM usuarios u");
    sql.AppendLine("WHERE u.usuario = 'admin.local'");
    sql.AppendLine("AND NOT EXISTS (SELECT 1 FROM parametros_usuario pu WHERE pu.usuario_id = u.id AND pu.clave = 'DEFAULT_LAYOUT_PROFILE');");
    sql.AppendLine();

    sql.AppendLine("-- Validaciones rápidas post-bootstrap");
    sql.AppendLine("-- SELECT COUNT(*) FROM usuarios;");
    sql.AppendLine("-- SELECT COUNT(*) FROM seguridad;");
    sql.AppendLine("-- SELECT COUNT(*) FROM menu;");
    sql.AppendLine("-- SELECT COUNT(*) FROM tipos_comprobante;");
    sql.AppendLine("-- SELECT * FROM config WHERE campo LIKE 'OPERACIONES.%' ORDER BY campo;");
}

static void AppendInsertIfMissing(StringBuilder sql, string table, string keyColumn, string keyValueSql, string columns, string values)
{
    sql.AppendLine($"INSERT INTO {table} ({columns})");
    sql.AppendLine($"SELECT {values}");
    sql.AppendLine($"WHERE NOT EXISTS (SELECT 1 FROM {table} WHERE {keyColumn} = {keyValueSql});");
}

static void AppendConfig(StringBuilder sql, string campo, string valor, string descripcion)
{
    var campoSql = Escape(campo);
    var valorSql = Escape(valor);
    var descripcionSql = Escape(descripcion);
    sql.AppendLine("INSERT INTO config (campo, valor, tipo_dato, descripcion, created_at, updated_at)");
    sql.AppendLine($"SELECT '{campoSql}', '{valorSql}', 1, '{descripcionSql}', now(), now()");
    sql.AppendLine($"WHERE NOT EXISTS (SELECT 1 FROM config WHERE campo = '{campoSql}');");
}

static void AppendMenu(StringBuilder sql, string descripcion, string? formulario, string? parentDescripcion, string? icono, short nivel, short orden)
{
    var descripcionSql = Escape(descripcion);
    var formularioSql = formulario is null ? "NULL" : $"'{Escape(formulario)}'";
    var iconoSql = icono is null ? "NULL" : $"'{Escape(icono)}'";
    var parentClause = parentDescripcion is null
        ? "parent_id IS NULL"
        : $"parent_id = (SELECT id FROM menu WHERE descripcion = '{Escape(parentDescripcion)}' LIMIT 1)";
    var parentSelect = parentDescripcion is null
        ? "NULL"
        : $"(SELECT id FROM menu WHERE descripcion = '{Escape(parentDescripcion)}' LIMIT 1)";

    sql.AppendLine("INSERT INTO menu (parent_id, descripcion, formulario, icono, nivel, orden, activo, created_at)");
    sql.AppendLine($"SELECT {parentSelect}, '{descripcionSql}', {formularioSql}, {iconoSql}, {nivel}, {orden}, true, now()");
    sql.AppendLine($"WHERE NOT EXISTS (SELECT 1 FROM menu WHERE descripcion = '{descripcionSql}' AND {parentClause});");
}

static void AppendPlanCuenta(StringBuilder sql, string codigoCuenta, string denominacion, short nivel, string ordenNivel, bool imputable, string? tipo, string? saldoNormal, string? integradoraCodigo = null)
{
    var codigoSql = Escape(codigoCuenta);
    var denominacionSql = Escape(denominacion);
    var ordenSql = Escape(ordenNivel);
    var tipoSql = tipo is null ? "NULL" : $"'{Escape(tipo)}'";
    var saldoSql = saldoNormal is null ? "NULL" : $"'{Escape(saldoNormal)}'";
    var integradoraSql = integradoraCodigo is null
        ? "NULL"
        : $"(SELECT pc2.id FROM plan_cuentas pc2 JOIN ejercicios e2 ON e2.id = pc2.ejercicio_id WHERE pc2.codigo_cuenta = '{Escape(integradoraCodigo)}' AND CURRENT_DATE BETWEEN e2.fecha_inicio AND e2.fecha_fin LIMIT 1)";

    sql.AppendLine("INSERT INTO plan_cuentas (ejercicio_id, integradora_id, codigo_cuenta, denominacion, nivel, orden_nivel, imputable, tipo, saldo_normal, created_at, updated_at)");
    sql.AppendLine($"SELECT e.id, {integradoraSql}, '{codigoSql}', '{denominacionSql}', {nivel}, '{ordenSql}', {(imputable ? "true" : "false")}, {tipoSql}, {saldoSql}, now(), now() FROM ejercicios e");
    sql.AppendLine("WHERE CURRENT_DATE BETWEEN e.fecha_inicio AND e.fecha_fin");
    sql.AppendLine($"AND NOT EXISTS (SELECT 1 FROM plan_cuentas pc WHERE pc.ejercicio_id = e.id AND pc.codigo_cuenta = '{codigoSql}');");
}

static string Escape(string value) => value.Replace("'", "''");