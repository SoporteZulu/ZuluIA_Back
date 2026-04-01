# SQL document: `zuluia_back_smoke_dataset`

Migrado desde `zuluia_back_smoke_dataset.sql` para ejecución manual sobre PostgreSQL local.

```sql
-- Dataset complementario para smoke tests de ZuluIA_Back
-- Requiere que previamente se haya aplicado zuluia_back_full_compatible.sql

BEGIN;

-- Geografía base complementaria
INSERT INTO provincias (pais_id, codigo, descripcion, created_at, updated_at)
SELECT p.id, 'AR-MI', 'MISIONES', now(), now()
FROM paises p
WHERE p.codigo = 'AR'
AND NOT EXISTS (SELECT 1 FROM provincias WHERE codigo = 'AR-MI');

INSERT INTO localidades (provincia_id, codigo_postal, descripcion, created_at, updated_at)
SELECT pr.id, '3300', 'POSADAS', now(), now()
FROM provincias pr
WHERE pr.codigo = 'AR-MI'
AND NOT EXISTS (SELECT 1 FROM localidades WHERE descripcion = 'POSADAS' AND provincia_id = pr.id);

INSERT INTO barrios (localidad_id, descripcion, created_at, updated_at)
SELECT l.id, 'CENTRO', now(), now()
FROM localidades l
WHERE l.descripcion = 'POSADAS'
AND NOT EXISTS (SELECT 1 FROM barrios WHERE descripcion = 'CENTRO' AND localidad_id = l.id);

-- Ítem adicional con código de barras
INSERT INTO atributos_comerciales (codigo, descripcion, tipo_dato, activo, created_at, updated_at, created_by, updated_by)
SELECT 'ATR-SMOKE', 'ATRIBUTO SMOKE', 'Texto', true, now(), now(), 1, 1
WHERE NOT EXISTS (SELECT 1 FROM atributos_comerciales WHERE codigo = 'ATR-SMOKE');

INSERT INTO items (
    codigo, codigo_barras, descripcion, categoria_id, marca_id, unidad_medida_id, alicuota_iva_id, moneda_id,
    es_producto, es_servicio, es_financiero, maneja_stock,
    precio_costo, precio_venta, stock_minimo, stock_maximo, activo, created_at, updated_at)
SELECT
    'ITEM-SMOKE', '7790000000001', 'ITEM SMOKE', c.id, mc.id, um.id, ai.id, m.id,
    true, false, false, true,
    850, 1250, 0, 100, true, now(), now()
FROM categorias_items c
CROSS JOIN marcas_comerciales mc
CROSS JOIN unidades_medida um
CROSS JOIN alicuotas_iva ai
CROSS JOIN monedas m
WHERE c.codigo = 'GENERAL' AND mc.codigo = 'GEN' AND um.codigo = 'UN' AND ai.codigo = 5 AND m.codigo = 'ARS'
AND NOT EXISTS (SELECT 1 FROM items WHERE codigo = 'ITEM-SMOKE');

INSERT INTO stock (item_id, deposito_id, cantidad, updated_at)
SELECT i.id, d.id, 25, now()
FROM items i
CROSS JOIN depositos d
WHERE i.codigo = 'ITEM-SMOKE' AND d.descripcion = 'DEPÓSITO PRINCIPAL'
AND NOT EXISTS (SELECT 1 FROM stock s WHERE s.item_id = i.id AND s.deposito_id = d.id);

INSERT INTO movimientos_stock (item_id, deposito_id, fecha, tipo_movimiento, cantidad, saldo_resultante, origen_tabla, origen_id, observacion, created_at, created_by)
SELECT i.id, d.id, now(), 'AJUSTE_INICIAL', 25, 25, 'bootstrap_smoke', 1, 'Stock inicial smoke', now(), 1
FROM items i
CROSS JOIN depositos d
WHERE i.codigo = 'ITEM-SMOKE' AND d.descripcion = 'DEPÓSITO PRINCIPAL'
AND NOT EXISTS (
    SELECT 1 FROM movimientos_stock ms
    WHERE ms.item_id = i.id AND ms.deposito_id = d.id AND ms.tipo_movimiento = 'AJUSTE_INICIAL' AND ms.origen_tabla = 'bootstrap_smoke');

-- Lista de precios
INSERT INTO listas_precios (descripcion, moneda_id, vigencia_desde, vigencia_hasta, activa, created_at, updated_at)
SELECT 'LISTA SMOKE', m.id, CURRENT_DATE, (CURRENT_DATE + INTERVAL '365 day')::date, true, now(), now()
FROM monedas m
WHERE m.codigo = 'ARS'
AND NOT EXISTS (SELECT 1 FROM listas_precios WHERE descripcion = 'LISTA SMOKE');

INSERT INTO lista_precios_items (lista_id, item_id, precio, descuento_pct, updated_at)
SELECT lp.id, i.id, 1200, 0, now()
FROM listas_precios lp
CROSS JOIN items i
WHERE lp.descripcion = 'LISTA SMOKE' AND i.codigo = 'ITEM-SMOKE'
AND NOT EXISTS (SELECT 1 FROM lista_precios_items lpi WHERE lpi.lista_id = lp.id AND lpi.item_id = i.id);

-- Cuenta corriente y movimientos base
INSERT INTO cuenta_corriente (tercero_id, sucursal_id, moneda_id, saldo, updated_at)
SELECT t.id, s.id, m.id, 0, now()
FROM terceros t
CROSS JOIN sucursales s
CROSS JOIN monedas m
WHERE t.legajo = 'CLI0001' AND s.cuit = '30712345678' AND m.codigo = 'ARS'
AND NOT EXISTS (
    SELECT 1 FROM cuenta_corriente cc
    WHERE cc.tercero_id = t.id AND cc.sucursal_id = s.id AND cc.moneda_id = m.id);

INSERT INTO movimientos_cta_cte (tercero_id, sucursal_id, moneda_id, comprobante_id, fecha, debe, haber, saldo, descripcion, created_at)
SELECT t.id, s.id, m.id, NULL, CURRENT_DATE, 0, 0, 0, 'Saldo inicial smoke', now()
FROM terceros t
CROSS JOIN sucursales s
CROSS JOIN monedas m
WHERE t.legajo = 'CLI0001' AND s.cuit = '30712345678' AND m.codigo = 'ARS'
AND NOT EXISTS (
    SELECT 1 FROM movimientos_cta_cte mc
    WHERE mc.tercero_id = t.id AND mc.descripcion = 'Saldo inicial smoke');

-- Período IVA
INSERT INTO periodos_iva (ejercicio_id, sucursal_id, periodo, cerrado, created_at)
SELECT e.id, s.id, date_trunc('month', CURRENT_DATE)::date, false, now()
FROM ejercicios e
CROSS JOIN sucursales s
WHERE CURRENT_DATE BETWEEN e.fecha_inicio AND e.fecha_fin AND s.cuit = '30712345678'
AND NOT EXISTS (
    SELECT 1 FROM periodos_iva pi WHERE pi.ejercicio_id = e.id AND pi.sucursal_id = s.id AND pi.periodo = date_trunc('month', CURRENT_DATE)::date);

-- Asiento contable simple y balanceado
INSERT INTO asientos (ejercicio_id, sucursal_id, fecha, numero, descripcion, origen_tabla, origen_id, cuadra, estado, created_at, updated_at, created_by, updated_by)
SELECT e.id, s.id, CURRENT_DATE, 900001, 'ASIENTO SMOKE', 'bootstrap_smoke', 1, true, 'Contabilizado', now(), now(), 1, 1
FROM ejercicios e
CROSS JOIN sucursales s
WHERE CURRENT_DATE BETWEEN e.fecha_inicio AND e.fecha_fin AND s.cuit = '30712345678'
AND NOT EXISTS (SELECT 1 FROM asientos a WHERE a.numero = 900001 AND a.sucursal_id = s.id AND a.ejercicio_id = e.id);

INSERT INTO asientos_lineas (asiento_id, cuenta_id, debe, haber, descripcion, orden, centro_costo_id)
SELECT a.id, pc.id, 1000, 0, 'Debe smoke', 1, cc.id
FROM asientos a
CROSS JOIN plan_cuentas pc
CROSS JOIN centros_costo cc
WHERE a.numero = 900001 AND pc.codigo_cuenta = '1.1' AND cc.codigo = 'GENERAL'
AND NOT EXISTS (SELECT 1 FROM asientos_lineas al WHERE al.asiento_id = a.id AND al.orden = 1);

INSERT INTO asientos_lineas (asiento_id, cuenta_id, debe, haber, descripcion, orden, centro_costo_id)
SELECT a.id, pc.id, 0, 1000, 'Haber smoke', 2, cc.id
FROM asientos a
CROSS JOIN plan_cuentas pc
CROSS JOIN centros_costo cc
WHERE a.numero = 900001 AND pc.codigo_cuenta = '4.1' AND cc.codigo = 'GENERAL'
AND NOT EXISTS (SELECT 1 FROM asientos_lineas al WHERE al.asiento_id = a.id AND al.orden = 2);

-- Diagnósticos
INSERT INTO regiones_diagnosticas (codigo, descripcion, activa, created_at, updated_at)
SELECT 'REG-SMOKE', 'REGION SMOKE', true, now(), now()
WHERE NOT EXISTS (SELECT 1 FROM regiones_diagnosticas WHERE codigo = 'REG-SMOKE');

INSERT INTO aspectos_diagnostico (region_id, codigo, descripcion, peso, activo, created_at, updated_at)
SELECT r.id, 'ASP-SMOKE', 'ASPECTO SMOKE', 1, true, now(), now()
FROM regiones_diagnosticas r
WHERE r.codigo = 'REG-SMOKE'
AND NOT EXISTS (SELECT 1 FROM aspectos_diagnostico WHERE codigo = 'ASP-SMOKE' AND region_id = r.id);

INSERT INTO variables_diagnosticas (aspecto_id, codigo, descripcion, tipo, requerida, peso, activa, created_at, updated_at)
SELECT a.id, 'VAR-SMOKE', 'VARIABLE SMOKE', 'Opcion', true, 1, true, now(), now()
FROM aspectos_diagnostico a
WHERE a.codigo = 'ASP-SMOKE'
AND NOT EXISTS (SELECT 1 FROM variables_diagnosticas WHERE codigo = 'VAR-SMOKE' AND aspecto_id = a.id);

INSERT INTO variables_diagnosticas_opciones (variable_id, codigo, descripcion, valor_numerico, orden)
SELECT v.id, 'OPC-1', 'OPCION 1', 10, 1
FROM variables_diagnosticas v
WHERE v.codigo = 'VAR-SMOKE'
AND NOT EXISTS (SELECT 1 FROM variables_diagnosticas_opciones WHERE codigo = 'OPC-1' AND variable_id = v.id);

INSERT INTO plantillas_diagnosticas (codigo, descripcion, activa, observacion, created_at, updated_at)
SELECT 'PLT-SMOKE', 'PLANTILLA SMOKE', true, 'Plantilla smoke', now(), now()
WHERE NOT EXISTS (SELECT 1 FROM plantillas_diagnosticas WHERE codigo = 'PLT-SMOKE');

INSERT INTO plantillas_diagnosticas_variables (plantilla_id, variable_id, orden)
SELECT p.id, v.id, 1
FROM plantillas_diagnosticas p
CROSS JOIN variables_diagnosticas v
WHERE p.codigo = 'PLT-SMOKE' AND v.codigo = 'VAR-SMOKE'
AND NOT EXISTS (SELECT 1 FROM plantillas_diagnosticas_variables WHERE plantilla_id = p.id AND variable_id = v.id);

INSERT INTO planillas_diagnosticas (plantilla_id, nombre, fecha, resultado_total, estado, observacion, created_at, updated_at, created_by, updated_by)
SELECT p.id, 'PLANILLA SMOKE', CURRENT_DATE, 10, 'Completada', 'Planilla smoke', now(), now(), 1, 1
FROM plantillas_diagnosticas p
WHERE p.codigo = 'PLT-SMOKE'
AND NOT EXISTS (SELECT 1 FROM planillas_diagnosticas WHERE nombre = 'PLANILLA SMOKE' AND plantilla_id = p.id);

INSERT INTO planillas_diagnosticas_respuestas (planilla_id, variable_id, opcion_id, valor_texto, valor_numerico, puntaje_obtenido)
SELECT pl.id, v.id, o.id, NULL, 10, 10
FROM planillas_diagnosticas pl
CROSS JOIN variables_diagnosticas v
CROSS JOIN variables_diagnosticas_opciones o
WHERE pl.nombre = 'PLANILLA SMOKE' AND v.codigo = 'VAR-SMOKE' AND o.codigo = 'OPC-1' AND o.variable_id = v.id
AND NOT EXISTS (SELECT 1 FROM planillas_diagnosticas_respuestas WHERE planilla_id = pl.id AND variable_id = v.id);

-- Contratos
INSERT INTO contratos (tercero_id, sucursal_id, moneda_id, codigo, descripcion, fecha_inicio, fecha_fin, importe, renovacion_automatica, estado, observacion, created_at, updated_at, created_by, updated_by)
SELECT t.id, s.id, m.id, 'CTR-SMOKE', 'CONTRATO SMOKE', CURRENT_DATE, (CURRENT_DATE + INTERVAL '90 day')::date, 15000, true, 'Vigente', 'Contrato smoke', now(), now(), 1, 1
FROM terceros t
CROSS JOIN sucursales s
CROSS JOIN monedas m
WHERE t.legajo = 'CLI0001' AND s.cuit = '30712345678' AND m.codigo = 'ARS'
AND NOT EXISTS (SELECT 1 FROM contratos WHERE codigo = 'CTR-SMOKE');

INSERT INTO contratos_historial (contrato_id, tipo_evento, fecha, descripcion, importe, created_at, updated_at, created_by, updated_by)
SELECT c.id, 'Alta', CURRENT_DATE, 'Alta contrato smoke', c.importe, now(), now(), 1, 1
FROM contratos c
WHERE c.codigo = 'CTR-SMOKE'
AND NOT EXISTS (SELECT 1 FROM contratos_historial WHERE contrato_id = c.id AND descripcion = 'Alta contrato smoke');

INSERT INTO contratos_impactos (contrato_id, tipo, fecha, importe, descripcion, created_at, updated_at, created_by, updated_by)
SELECT c.id, 'Facturacion', CURRENT_DATE, 1500, 'Impacto smoke', now(), now(), 1, 1
FROM contratos c
WHERE c.codigo = 'CTR-SMOKE'
AND NOT EXISTS (SELECT 1 FROM contratos_impactos WHERE contrato_id = c.id AND descripcion = 'Impacto smoke');

-- Colegio
INSERT INTO colegio_planes_generales (sucursal_id, plan_pago_id, tipo_comprobante_id, item_id, moneda_id, codigo, descripcion, importe_base, activo, observacion, created_at, updated_at, created_by, updated_by)
SELECT s.id, pp.id, tc.id, i.id, m.id, 'COL-SMOKE', 'PLAN GENERAL SMOKE', 2000, true, 'Plan smoke', now(), now(), 1, 1
FROM sucursales s
CROSS JOIN planes_pago pp
CROSS JOIN tipos_comprobante tc
CROSS JOIN items i
CROSS JOIN monedas m
WHERE s.cuit = '30712345678' AND pp.descripcion = 'CONTADO' AND tc.codigo = 'FACC' AND i.codigo = 'ITEM-GENERICO' AND m.codigo = 'ARS'
AND NOT EXISTS (SELECT 1 FROM colegio_planes_generales WHERE codigo = 'COL-SMOKE' AND sucursal_id = s.id);

INSERT INTO colegio_lotes (plan_general_colegio_id, codigo, fecha_emision, fecha_vencimiento, estado, cantidad_cedulones, observacion, created_at, updated_at, created_by, updated_by)
SELECT pg.id, 'LOTE-SMOKE', CURRENT_DATE, (CURRENT_DATE + INTERVAL '30 day')::date, 'Abierto', 1, 'Lote smoke', now(), now(), 1, 1
FROM colegio_planes_generales pg
WHERE pg.codigo = 'COL-SMOKE'
AND NOT EXISTS (SELECT 1 FROM colegio_lotes WHERE codigo = 'LOTE-SMOKE');

INSERT INTO cedulones (tercero_id, sucursal_id, plan_pago_id, plan_general_colegio_id, lote_colegio_id, comprobante_id, nro_cedulon, fecha_emision, fecha_vencimiento, importe, importe_pagado, estado, created_at, updated_at, created_by, updated_by)
SELECT t.id, s.id, pp.id, pg.id, l.id, NULL, 'CED-SMOKE', CURRENT_DATE, (CURRENT_DATE + INTERVAL '30 day')::date, 2000, 0, 'Emitido', now(), now(), 1, 1
FROM terceros t
CROSS JOIN sucursales s
CROSS JOIN planes_pago pp
CROSS JOIN colegio_planes_generales pg
CROSS JOIN colegio_lotes l
WHERE t.legajo = 'CLI0001' AND s.cuit = '30712345678' AND pp.descripcion = 'CONTADO' AND pg.codigo = 'COL-SMOKE' AND l.codigo = 'LOTE-SMOKE'
AND NOT EXISTS (SELECT 1 FROM cedulones WHERE nro_cedulon = 'CED-SMOKE');

INSERT INTO colegio_cobinpro_operaciones (cedulon_id, tercero_id, sucursal_id, cobro_id, fecha, importe, referencia_externa, estado, observacion, created_at, updated_at, created_by, updated_by)
SELECT c.id, c.tercero_id, c.sucursal_id, NULL, CURRENT_DATE, c.importe, 'COB-SMOKE-REF', 'Pendiente', 'Cobinpro smoke', now(), now(), 1, 1
FROM cedulones c
WHERE c.nro_cedulon = 'CED-SMOKE'
AND NOT EXISTS (SELECT 1 FROM colegio_cobinpro_operaciones WHERE referencia_externa = 'COB-SMOKE-REF');

-- Producción
INSERT INTO formulas_produccion (codigo, descripcion, item_resultado_id, cantidad_resultado, unidad_medida_id, activo, observacion, created_at, updated_at, created_by, updated_by)
SELECT 'FOR-SMOKE', 'FORMULA SMOKE', i.id, 1, um.id, true, 'Formula smoke', now(), now(), 1, 1
FROM items i
CROSS JOIN unidades_medida um
WHERE i.codigo = 'ITEM-SMOKE' AND um.codigo = 'UN'
AND NOT EXISTS (SELECT 1 FROM formulas_produccion WHERE codigo = 'FOR-SMOKE');

INSERT INTO formula_ingredientes (formula_id, item_id, cantidad, unidad_medida_id, es_opcional, orden)
SELECT f.id, i.id, 1, um.id, false, 1
FROM formulas_produccion f
CROSS JOIN items i
CROSS JOIN unidades_medida um
WHERE f.codigo = 'FOR-SMOKE' AND i.codigo = 'ITEM-GENERICO' AND um.codigo = 'UN'
AND NOT EXISTS (SELECT 1 FROM formula_ingredientes WHERE formula_id = f.id AND item_id = i.id);

INSERT INTO formulas_produccion_historial (formula_id, version, codigo, descripcion, cantidad_resultado, motivo, snapshot_json, created_at, updated_at, created_by, updated_by)
SELECT f.id, 1, f.codigo, f.descripcion, f.cantidad_resultado, 'Alta inicial', '{"smoke":true}', now(), now(), 1, 1
FROM formulas_produccion f
WHERE f.codigo = 'FOR-SMOKE'
AND NOT EXISTS (SELECT 1 FROM formulas_produccion_historial WHERE formula_id = f.id AND version = 1);

INSERT INTO ordenes_trabajo (sucursal_id, formula_id, deposito_origen_id, deposito_destino_id, fecha, fecha_fin_prevista, fecha_fin_real, cantidad, cantidad_producida, estado, observacion, created_at, updated_at, created_by, updated_by)
SELECT s.id, f.id, d.id, d.id, CURRENT_DATE, (CURRENT_DATE + INTERVAL '1 day')::date, NULL, 2, 0, 'Pendiente', 'OT smoke', now(), now(), 1, 1
FROM sucursales s
CROSS JOIN formulas_produccion f
CROSS JOIN depositos d
WHERE s.cuit = '30712345678' AND f.codigo = 'FOR-SMOKE' AND d.descripcion = 'DEPÓSITO PRINCIPAL'
AND NOT EXISTS (SELECT 1 FROM ordenes_trabajo WHERE observacion = 'OT smoke' AND formula_id = f.id);

INSERT INTO ordenes_trabajo_consumos (orden_trabajo_id, item_id, deposito_id, cantidad_planificada, cantidad_consumida, movimiento_stock_id, observacion, created_at, updated_at, created_by, updated_by)
SELECT ot.id, i.id, d.id, 2, 0, NULL, 'Consumo smoke', now(), now(), 1, 1
FROM ordenes_trabajo ot
CROSS JOIN items i
CROSS JOIN depositos d
WHERE ot.observacion = 'OT smoke' AND i.codigo = 'ITEM-GENERICO' AND d.descripcion = 'DEPÓSITO PRINCIPAL'
AND NOT EXISTS (SELECT 1 FROM ordenes_trabajo_consumos WHERE orden_trabajo_id = ot.id AND item_id = i.id);

INSERT INTO ordenes_empaque (orden_trabajo_id, item_id, deposito_id, fecha, cantidad, lote, observacion, estado, created_at, updated_at, created_by, updated_by)
SELECT ot.id, i.id, d.id, CURRENT_DATE, 1, 'LOT-SMOKE', 'Empaque smoke', 'Pendiente', now(), now(), 1, 1
FROM ordenes_trabajo ot
CROSS JOIN items i
CROSS JOIN depositos d
WHERE ot.observacion = 'OT smoke' AND i.codigo = 'ITEM-SMOKE' AND d.descripcion = 'DEPÓSITO PRINCIPAL'
AND NOT EXISTS (SELECT 1 FROM ordenes_empaque WHERE orden_trabajo_id = ot.id AND lote = 'LOT-SMOKE');

INSERT INTO ordenes_preparacion (sucursal_id, comprobante_origen_id, tercero_id, fecha, estado, observacion, fecha_confirmacion, created_at, updated_at, created_by, updated_by)
SELECT s.id, NULL, t.id, CURRENT_DATE, 'Pendiente', 'Orden preparación smoke', NULL, now(), now(), 1, 1
FROM sucursales s
CROSS JOIN terceros t
WHERE s.cuit = '30712345678' AND t.legajo = 'CLI0001'
AND NOT EXISTS (SELECT 1 FROM ordenes_preparacion WHERE observacion = 'Orden preparación smoke' AND sucursal_id = s.id);

INSERT INTO depositos (sucursal_id, descripcion, es_default, activo, created_at)
SELECT s.id, 'DEPÓSITO PRINCIPAL', true, true, now() FROM sucursales s
WHERE s.cuit = '30712345678'
AND NOT EXISTS (SELECT 1 FROM depositos d WHERE d.sucursal_id = s.id AND d.descripcion = 'DEPÓSITO PRINCIPAL');

INSERT INTO depositos (sucursal_id, descripcion, es_default, activo, created_at)
SELECT s.id, 'DEPÓSITO SECUNDARIO', false, true, now() FROM sucursales s
WHERE s.cuit = '30712345678'
AND NOT EXISTS (SELECT 1 FROM depositos d WHERE d.sucursal_id = s.id AND d.descripcion = 'DEPÓSITO SECUNDARIO');

INSERT INTO ordenes_preparacion_detalles (orden_preparacion_id, item_id, deposito_id, cantidad, cantidad_entregada, observacion)
SELECT op.id, i.id, d.id, 1, 0, 'Detalle smoke'
FROM ordenes_preparacion op
CROSS JOIN items i
CROSS JOIN depositos d
WHERE op.observacion = 'Orden preparación smoke' AND i.codigo = 'ITEM-SMOKE' AND d.descripcion = 'DEPÓSITO PRINCIPAL'
AND NOT EXISTS (SELECT 1 FROM ordenes_preparacion_detalles WHERE orden_preparacion_id = op.id AND item_id = i.id);

INSERT INTO transferencias_deposito (orden_preparacion_id, sucursal_id, deposito_origen_id, deposito_destino_id, fecha, estado, observacion, fecha_confirmacion, created_at, updated_at, created_by, updated_by)
SELECT op.id, s.id, d.id, d.id, CURRENT_DATE, 'Pendiente', 'Transferencia smoke', NULL, now(), now(), 1, 1
FROM ordenes_preparacion op
CROSS JOIN sucursales s
CROSS JOIN depositos d
WHERE op.observacion = 'Orden preparación smoke' AND s.cuit = '30712345678' AND d.descripcion = 'DEPÓSITO PRINCIPAL'
AND NOT EXISTS (SELECT 1 FROM transferencias_deposito WHERE observacion = 'Transferencia smoke' AND sucursal_id = s.id);

INSERT INTO transferencias_deposito_detalles (transferencia_deposito_id, item_id, cantidad, observacion)
SELECT td.id, i.id, 1, 'Detalle transferencia smoke'
FROM transferencias_deposito td
CROSS JOIN items i
WHERE td.observacion = 'Transferencia smoke' AND i.codigo = 'ITEM-SMOKE'
AND NOT EXISTS (SELECT 1 FROM transferencias_deposito_detalles WHERE transferencia_deposito_id = td.id AND item_id = i.id);

INSERT INTO logistica_interna_eventos (orden_preparacion_id, transferencia_deposito_id, tipo_evento, fecha, descripcion, created_at, updated_at, created_by, updated_by)
SELECT op.id, td.id, 'Alta', CURRENT_DATE, 'Evento logístico smoke', now(), now(), 1, 1
FROM ordenes_preparacion op
CROSS JOIN transferencias_deposito td
WHERE op.observacion = 'Orden preparación smoke' AND td.observacion = 'Transferencia smoke'
AND NOT EXISTS (SELECT 1 FROM logistica_interna_eventos WHERE descripcion = 'Evento logístico smoke');

-- RRHH
INSERT INTO terceros (legajo, razon_social, tipo_documento_id, nro_documento, condicion_iva_id, categoria_id, es_cliente, es_proveedor, es_empleado, moneda_id, facturable, sucursal_id, activo, created_at, updated_at)
SELECT 'EMP0001', 'EMPLEADO SMOKE', td.id, '30111222', ci.id, ct.id, false, false, true, m.id, false, s.id, true, now(), now()
FROM tipos_documento td CROSS JOIN condiciones_iva ci CROSS JOIN categorias_terceros ct CROSS JOIN monedas m CROSS JOIN sucursales s
WHERE td.codigo = 96 AND ci.codigo = 5 AND ct.descripcion = 'GENERAL' AND m.codigo = 'ARS' AND s.cuit = '30712345678'
AND NOT EXISTS (SELECT 1 FROM terceros WHERE legajo = 'EMP0001');

INSERT INTO empleados (tercero_id, sucursal_id, legajo, cargo, area, fecha_ingreso, fecha_egreso, sueldo_basico, moneda_id, estado, created_at, updated_at)
SELECT t.id, s.id, 'EMP0001', 'ADMINISTRATIVO', 'RRHH', CURRENT_DATE, NULL, 1000000, m.id, 'Activo', now(), now()
FROM terceros t CROSS JOIN sucursales s CROSS JOIN monedas m
WHERE t.legajo = 'EMP0001' AND s.cuit = '30712345678' AND m.codigo = 'ARS'
AND NOT EXISTS (SELECT 1 FROM empleados WHERE legajo = 'EMP0001');

INSERT INTO liquidaciones_sueldo (empleado_id, sucursal_id, anio, mes, sueldo_basico, total_haberes, total_descuentos, neto, moneda_id, pagada, importe_imputado, fecha_pago, comprobante_empleado_id, observacion, created_at)
SELECT e.id, e.sucursal_id, EXTRACT(YEAR FROM CURRENT_DATE)::int, EXTRACT(MONTH FROM CURRENT_DATE)::int, 1000000, 1000000, 0, 1000000, e.moneda_id, false, 0, NULL, NULL, 'Liquidación smoke', now()
FROM empleados e
WHERE e.legajo = 'EMP0001'
AND NOT EXISTS (SELECT 1 FROM liquidaciones_sueldo WHERE empleado_id = e.id AND anio = EXTRACT(YEAR FROM CURRENT_DATE)::int AND mes = EXTRACT(MONTH FROM CURRENT_DATE)::int);

INSERT INTO comprobantes_empleados (empleado_id, liquidacion_sueldo_id, sucursal_id, fecha, tipo, numero, total, moneda_id, observacion, created_at, updated_at, created_by, updated_by)
SELECT e.id, ls.id, e.sucursal_id, CURRENT_DATE, 'RECIBO', 'REC-SMOKE', ls.neto, ls.moneda_id, 'Comprobante empleado smoke', now(), now(), 1, 1
FROM empleados e
JOIN liquidaciones_sueldo ls ON ls.empleado_id = e.id
WHERE e.legajo = 'EMP0001'
AND NOT EXISTS (SELECT 1 FROM comprobantes_empleados WHERE numero = 'REC-SMOKE');

INSERT INTO imputaciones_empleados (liquidacion_sueldo_id, comprobante_empleado_id, tesoreria_movimiento_id, fecha, importe, observacion, created_at, updated_at, created_by, updated_by)
SELECT ls.id, ce.id, NULL, CURRENT_DATE, ls.neto, 'Imputación empleado smoke', now(), now(), 1, 1
FROM liquidaciones_sueldo ls
JOIN comprobantes_empleados ce ON ce.liquidacion_sueldo_id = ls.id
WHERE ce.numero = 'REC-SMOKE'
AND NOT EXISTS (SELECT 1 FROM imputaciones_empleados WHERE liquidacion_sueldo_id = ls.id AND comprobante_empleado_id = ce.id);

-- Carta de porte
INSERT INTO carta_porte (comprobante_id, orden_carga_id, transportista_id, nro_ctg, cuit_remitente, cuit_destinatario, cuit_transportista, fecha_emision, fecha_solicitud_ctg, intentos_ctg, ultimo_error_ctg, estado, observacion, created_at, updated_at, created_by, updated_by)
SELECT NULL, NULL, tr.id, 'CTG-SMOKE', '30712345678', '30000000000', '30000000001', CURRENT_DATE, CURRENT_DATE, 0, NULL, 'Pendiente', 'Carta porte smoke', now(), now(), 1, 1
FROM transportistas tr
JOIN terceros t ON t.id = tr.tercero_id
WHERE t.legajo = 'TRA0001'
AND NOT EXISTS (SELECT 1 FROM carta_porte WHERE nro_ctg = 'CTG-SMOKE');

INSERT INTO ordenes_carga (carta_porte_id, transportista_id, fecha_carga, origen, destino, patente, confirmada, observacion, created_at, updated_at, created_by, updated_by)
SELECT cp.id, cp.transportista_id, CURRENT_DATE, 'ORIGEN SMOKE', 'DESTINO SMOKE', 'AAA000', false, 'Orden carga smoke', now(), now(), 1, 1
FROM carta_porte cp
WHERE cp.nro_ctg = 'CTG-SMOKE'
AND NOT EXISTS (SELECT 1 FROM ordenes_carga WHERE carta_porte_id = cp.id);

INSERT INTO carta_porte_eventos (carta_porte_id, orden_carga_id, tipo_evento, estado_anterior, estado_nuevo, fecha_evento, mensaje, nro_ctg, intento_ctg, created_at, updated_at, created_by, updated_by)
SELECT cp.id, oc.id, 'Alta', NULL, 'Pendiente', CURRENT_DATE, 'Evento carta porte smoke', cp.nro_ctg, 0, now(), now(), 1, 1
FROM carta_porte cp
LEFT JOIN ordenes_carga oc ON oc.carta_porte_id = cp.id
WHERE cp.nro_ctg = 'CTG-SMOKE'
AND NOT EXISTS (SELECT 1 FROM carta_porte_eventos WHERE carta_porte_id = cp.id AND tipo_evento = 'Alta');

-- Integraciones y batch
UPDATE batch_programaciones
SET tipo_proceso = 'FacturacionAutomatica',
    activa = false,
    payload_json = '{}',
    observacion = 'Batch smoke inactivo para dataset local'
WHERE nombre = 'Batch Smoke';

INSERT INTO batch_programaciones (tipo_proceso, nombre, intervalo_minutos, proxima_ejecucion, ultima_ejecucion, activa, payload_json, observacion, created_at, updated_at, created_by, updated_by)
SELECT 'FacturacionAutomatica', 'Batch Smoke', 60, now(), NULL, false, '{}', 'Batch smoke inactivo para dataset local', now(), now(), 1, 1
WHERE NOT EXISTS (SELECT 1 FROM batch_programaciones WHERE nombre = 'Batch Smoke');

INSERT INTO monitores_exportacion (codigo, descripcion, ultimo_job_id, ultima_ejecucion, ultimo_estado, registros_pendientes, ultimo_mensaje, created_at, updated_at, created_by, updated_by)
SELECT 'MON-SMOKE', 'Monitor Smoke', NULL, now(), 'OK', 0, 'Sin pendientes', now(), now(), 1, 1
WHERE NOT EXISTS (SELECT 1 FROM monitores_exportacion WHERE codigo = 'MON-SMOKE');

INSERT INTO procesos_integracion_jobs (tipo, nombre, clave_idempotencia, estado, fecha_inicio, fecha_fin, total_registros, registros_procesados, registros_exitosos, registros_con_error, payload_resumen, observacion, created_at, updated_at, created_by, updated_by)
SELECT 'Importacion', 'Job Smoke', 'JOB-SMOKE', 'Completado', now(), now(), 1, 1, 1, 0, '{"resultado":"ok"}', 'Job smoke', now(), now(), 1, 1
WHERE NOT EXISTS (SELECT 1 FROM procesos_integracion_jobs WHERE clave_idempotencia = 'JOB-SMOKE');

INSERT INTO procesos_integracion_logs (job_id, nivel, mensaje, referencia, payload, created_at, updated_at, created_by, updated_by)
SELECT j.id, 'Info', 'Log smoke', 'JOB-SMOKE', '{"ok":true}', now(), now(), 1, 1
FROM procesos_integracion_jobs j
WHERE j.clave_idempotencia = 'JOB-SMOKE'
AND NOT EXISTS (SELECT 1 FROM procesos_integracion_logs WHERE job_id = j.id AND referencia = 'JOB-SMOKE');

INSERT INTO integraciones_externas_audit (proveedor, operacion, referencia_tipo, referencia_id, exitoso, reintentos, timeout_ms, circuit_breaker_abierto, duracion_ms, ambiente, endpoint, codigo_error, error_funcional, request_payload, response_payload, mensaje_error, created_at, updated_at, created_by, updated_by)
SELECT 'Deuce', 'Smoke', 'manual', 1, true, 0, 1000, false, 50, 'TEST', 'https://test.deuce.local/api', NULL, false, '{}', '{}', NULL, now(), now(), 1, 1
WHERE NOT EXISTS (SELECT 1 FROM integraciones_externas_audit WHERE operacion = 'Smoke' AND referencia_tipo = 'manual');

INSERT INTO busquedas (nombre, modulo, filtros_json, usuario_id, es_global, created_at, updated_at)
SELECT 'Búsqueda Smoke', 'general', '{"smoke":true}'::jsonb, u.id, false, now(), now()
FROM usuarios u
WHERE u.usuario = 'admin.local'
AND NOT EXISTS (SELECT 1 FROM busquedas WHERE nombre = 'Búsqueda Smoke');

-- Fiscal / reportes regulatorios
INSERT INTO salidas_regulatorias (tipo, sucursal_id, desde, hasta, estado, nombre_archivo, contenido, created_at, updated_at, created_by, updated_by)
SELECT 'LibroIVA', s.id, CURRENT_DATE, CURRENT_DATE, 'Generado', 'libroiva_smoke.txt', 'contenido smoke', now(), now(), 1, 1
FROM sucursales s
WHERE s.cuit = '30712345678'
AND NOT EXISTS (SELECT 1 FROM salidas_regulatorias WHERE nombre_archivo = 'libroiva_smoke.txt');

INSERT INTO rentas_bs_as_registros (sucursal_id, desde, hasta, total_percepciones, total_retenciones, observacion, created_at, updated_at, created_by, updated_by)
SELECT s.id, CURRENT_DATE, CURRENT_DATE, 0, 0, 'Rentas smoke', now(), now(), 1, 1
FROM sucursales s
WHERE s.cuit = '30712345678'
AND NOT EXISTS (SELECT 1 FROM rentas_bs_as_registros WHERE observacion = 'Rentas smoke' AND sucursal_id = s.id);

INSERT INTO libro_viajantes_registros (sucursal_id, desde, hasta, vendedor_id, total_ventas, total_comision, created_at, updated_at, created_by, updated_by)
SELECT s.id, CURRENT_DATE, CURRENT_DATE, NULL, 0, 0, now(), now(), 1, 1
FROM sucursales s
WHERE s.cuit = '30712345678'
AND NOT EXISTS (SELECT 1 FROM libro_viajantes_registros WHERE desde = CURRENT_DATE AND hasta = CURRENT_DATE AND sucursal_id = s.id);

INSERT INTO hechauka_registros (sucursal_id, desde, hasta, total_neto_gravado, total_iva, total_comprobantes, created_at, updated_at, created_by, updated_by)
SELECT s.id, CURRENT_DATE, CURRENT_DATE, 0, 0, 0, now(), now(), 1, 1
FROM sucursales s
WHERE s.cuit = '30712345678'
AND NOT EXISTS (SELECT 1 FROM hechauka_registros WHERE desde = CURRENT_DATE AND hasta = CURRENT_DATE AND sucursal_id = s.id);

INSERT INTO liquidaciones_primarias_granos (sucursal_id, fecha, numero_liquidacion, producto, cantidad, precio_unitario, total, observacion, created_at, updated_at, created_by, updated_by)
SELECT s.id, CURRENT_DATE, 'LPG-SMOKE', 'MAIZ', 10, 100, 1000, 'LPG smoke', now(), now(), 1, 1
FROM sucursales s
WHERE s.cuit = '30712345678'
AND NOT EXISTS (SELECT 1 FROM liquidaciones_primarias_granos WHERE numero_liquidacion = 'LPG-SMOKE');

INSERT INTO cierres_periodo_contable (ejercicio_id, sucursal_id, desde, hasta, observacion, created_at, updated_at, created_by, updated_by)
SELECT e.id, s.id, date_trunc('month', CURRENT_DATE)::date, (date_trunc('month', CURRENT_DATE) + INTERVAL '1 month - 1 day')::date, 'Cierre smoke', now(), now(), 1, 1
FROM ejercicios e
CROSS JOIN sucursales s
WHERE CURRENT_DATE BETWEEN e.fecha_inicio AND e.fecha_fin AND s.cuit = '30712345678'
AND NOT EXISTS (SELECT 1 FROM cierres_periodo_contable WHERE observacion = 'Cierre smoke' AND sucursal_id = s.id);

INSERT INTO reorganizaciones_asientos (ejercicio_id, sucursal_id, desde, hasta, cantidad_asientos, observacion, created_at, updated_at, created_by, updated_by)
SELECT e.id, s.id, CURRENT_DATE, CURRENT_DATE, 1, 'Reorg smoke', now(), now(), 1, 1
FROM ejercicios e
CROSS JOIN sucursales s
WHERE CURRENT_DATE BETWEEN e.fecha_inicio AND e.fecha_fin AND s.cuit = '30712345678'
AND NOT EXISTS (SELECT 1 FROM reorganizaciones_asientos WHERE observacion = 'Reorg smoke' AND sucursal_id = s.id);

-- Retenciones parametrización
INSERT INTO retenciones_por_persona (tercero_id, tipo_retencion_id, descripcion, created_at, updated_at, created_by, updated_by)
SELECT t.id, tr.id, 'Retención persona smoke', now(), now(), 1, 1
FROM terceros t
CROSS JOIN tipos_retencion tr
WHERE t.legajo = 'PRV0001' AND tr.regimen = 'IIBB'
AND NOT EXISTS (SELECT 1 FROM retenciones_por_persona WHERE tercero_id = t.id AND tipo_retencion_id = tr.id);

INSERT INTO escalas_retencion (tipo_retencion_id, descripcion, importe_desde, importe_hasta, porcentaje)
SELECT tr.id, 'Escala smoke', 0, 999999999, 2.5
FROM tipos_retencion tr
WHERE tr.regimen = 'IIBB'
AND NOT EXISTS (SELECT 1 FROM escalas_retencion WHERE tipo_retencion_id = tr.id AND descripcion = 'Escala smoke');

COMMIT;

```
