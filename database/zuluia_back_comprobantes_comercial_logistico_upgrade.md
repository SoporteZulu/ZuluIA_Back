# Upgrade de comprobantes comerciales y logísticos

Script histórico migrado desde `.sql` para evitar que bloquee compilaciones y pruebas del workspace.

## SQL original

```sql
-- ============================================================================
-- Ampliación de tabla comprobantes - Datos comerciales y logísticos
-- ============================================================================
-- Fecha: $(Get-Date)
-- Objetivo: Agregar campos comerciales (vendedor, cobrador, zona, comisiones) 
--           y logísticos (transporte, chofer, entrega) para paridad con zuluApp
-- ============================================================================

BEGIN;

-- ───────────────────────────────────────────────────────────────────────────
-- DATOS COMERCIALES
-- ───────────────────────────────────────────────────────────────────────────

-- Vendedor y Cobrador
ALTER TABLE comprobantes
ADD COLUMN IF NOT EXISTS vendedor_id BIGINT,
ADD COLUMN IF NOT EXISTS cobrador_id BIGINT,
ADD COLUMN IF NOT EXISTS porcentaje_comision_vendedor NUMERIC(5,2),
ADD COLUMN IF NOT EXISTS porcentaje_comision_cobrador NUMERIC(5,2);

-- Zona comercial, lista de precios, condición de pago
ALTER TABLE comprobantes
ADD COLUMN IF NOT EXISTS zona_comercial_id BIGINT,
ADD COLUMN IF NOT EXISTS lista_precios_id BIGINT,
ADD COLUMN IF NOT EXISTS condicion_pago_id BIGINT,
ADD COLUMN IF NOT EXISTS plazo_dias INTEGER;

-- Canal de venta (mostrador, web, teléfono, vendedor)
ALTER TABLE comprobantes
ADD COLUMN IF NOT EXISTS canal_venta_id BIGINT;

COMMENT ON COLUMN comprobantes.vendedor_id IS 'Vendedor asignado al comprobante';
COMMENT ON COLUMN comprobantes.cobrador_id IS 'Cobrador asignado al comprobante';
COMMENT ON COLUMN comprobantes.porcentaje_comision_vendedor IS 'Porcentaje de comisión del vendedor';
COMMENT ON COLUMN comprobantes.porcentaje_comision_cobrador IS 'Porcentaje de comisión del cobrador';
COMMENT ON COLUMN comprobantes.zona_comercial_id IS 'Zona comercial del cliente al momento del comprobante';
COMMENT ON COLUMN comprobantes.lista_precios_id IS 'Lista de precios aplicada';
COMMENT ON COLUMN comprobantes.condicion_pago_id IS 'Condición de pago aplicada';
COMMENT ON COLUMN comprobantes.plazo_dias IS 'Plazo en días para el pago';
COMMENT ON COLUMN comprobantes.canal_venta_id IS 'Canal de origen de la venta';

-- ───────────────────────────────────────────────────────────────────────────
-- DATOS LOGÍSTICOS (principalmente para remitos)
-- ───────────────────────────────────────────────────────────────────────────

-- Transporte y chofer
ALTER TABLE comprobantes
ADD COLUMN IF NOT EXISTS transporte_id BIGINT,
ADD COLUMN IF NOT EXISTS chofer_nombre VARCHAR(200),
ADD COLUMN IF NOT EXISTS chofer_dni VARCHAR(20),
ADD COLUMN IF NOT EXISTS pat_vehiculo VARCHAR(20),
ADD COLUMN IF NOT EXISTS pat_acoplado VARCHAR(20);

-- Domicilio y observaciones de entrega
ALTER TABLE comprobantes
ADD COLUMN IF NOT EXISTS domicilio_entrega VARCHAR(500),
ADD COLUMN IF NOT EXISTS observaciones_logisticas VARCHAR(1000),
ADD COLUMN IF NOT EXISTS fecha_estimada_entrega DATE,
ADD COLUMN IF NOT EXISTS fecha_real_entrega DATE;

-- Conformidad de entrega
ALTER TABLE comprobantes
ADD COLUMN IF NOT EXISTS firma_conformidad VARCHAR(500),
ADD COLUMN IF NOT EXISTS nombre_quien_recibe VARCHAR(200);

COMMENT ON COLUMN comprobantes.transporte_id IS 'Empresa de transporte asignada';
COMMENT ON COLUMN comprobantes.chofer_nombre IS 'Nombre del chofer que realiza la entrega';
COMMENT ON COLUMN comprobantes.chofer_dni IS 'DNI del chofer';
COMMENT ON COLUMN comprobantes.pat_vehiculo IS 'Patente del vehículo';
COMMENT ON COLUMN comprobantes.pat_acoplado IS 'Patente del acoplado si aplica';
COMMENT ON COLUMN comprobantes.domicilio_entrega IS 'Domicilio completo de entrega';
COMMENT ON COLUMN comprobantes.observaciones_logisticas IS 'Observaciones específicas de logística/transporte';
COMMENT ON COLUMN comprobantes.fecha_estimada_entrega IS 'Fecha estimada de entrega';
COMMENT ON COLUMN comprobantes.fecha_real_entrega IS 'Fecha real de entrega';
COMMENT ON COLUMN comprobantes.firma_conformidad IS 'Firma digital o referencia de conformidad';
COMMENT ON COLUMN comprobantes.nombre_quien_recibe IS 'Nombre de la persona que recibe la mercadería';

-- ───────────────────────────────────────────────────────────────────────────
-- OBSERVACIONES Y DATOS ADICIONALES
-- ───────────────────────────────────────────────────────────────────────────

-- Observaciones separadas por tipo
ALTER TABLE comprobantes
ADD COLUMN IF NOT EXISTS observacion_interna VARCHAR(1000),
ADD COLUMN IF NOT EXISTS observacion_fiscal VARCHAR(1000);

-- Recargo global
ALTER TABLE comprobantes
ADD COLUMN IF NOT EXISTS recargo_porcentaje NUMERIC(5,2),
ADD COLUMN IF NOT EXISTS recargo_importe NUMERIC(18,4);

-- Descuento global (porcentaje además del importe que ya existe)
ALTER TABLE comprobantes
ADD COLUMN IF NOT EXISTS descuento_porcentaje NUMERIC(5,2);

-- Snapshot del domicilio del tercero al momento de emisión
ALTER TABLE comprobantes
ADD COLUMN IF NOT EXISTS tercero_domicilio_snapshot VARCHAR(500);

COMMENT ON COLUMN comprobantes.observacion_interna IS 'Observaciones internas no visibles al cliente';
COMMENT ON COLUMN comprobantes.observacion_fiscal IS 'Observaciones específicas para la fiscalización';
COMMENT ON COLUMN comprobantes.recargo_porcentaje IS 'Porcentaje de recargo global aplicado';
COMMENT ON COLUMN comprobantes.recargo_importe IS 'Importe de recargo global aplicado';
COMMENT ON COLUMN comprobantes.descuento_porcentaje IS 'Porcentaje de descuento global aplicado';
COMMENT ON COLUMN comprobantes.tercero_domicilio_snapshot IS 'Snapshot del domicilio del tercero al momento de emisión';

-- ───────────────────────────────────────────────────────────────────────────
-- ÍNDICES PARA MEJORAR CONSULTAS
-- ───────────────────────────────────────────────────────────────────────────

CREATE INDEX IF NOT EXISTS idx_comprobantes_vendedor_id 
    ON comprobantes(vendedor_id) WHERE vendedor_id IS NOT NULL;

CREATE INDEX IF NOT EXISTS idx_comprobantes_cobrador_id 
    ON comprobantes(cobrador_id) WHERE cobrador_id IS NOT NULL;

CREATE INDEX IF NOT EXISTS idx_comprobantes_zona_comercial_id 
    ON comprobantes(zona_comercial_id) WHERE zona_comercial_id IS NOT NULL;

CREATE INDEX IF NOT EXISTS idx_comprobantes_transporte_id 
    ON comprobantes(transporte_id) WHERE transporte_id IS NOT NULL;

CREATE INDEX IF NOT EXISTS idx_comprobantes_fecha_estimada_entrega 
    ON comprobantes(fecha_estimada_entrega) WHERE fecha_estimada_entrega IS NOT NULL;

CREATE INDEX IF NOT EXISTS idx_comprobantes_fecha_real_entrega 
    ON comprobantes(fecha_real_entrega) WHERE fecha_real_entrega IS NOT NULL;

-- ───────────────────────────────────────────────────────────────────────────
-- NOTAS DE MIGRACIÓN
-- ───────────────────────────────────────────────────────────────────────────

-- IMPORTANTE:
-- 1. Estos campos son opcionales (nullable) para mantener compatibilidad con datos existentes
-- 2. Se espera que las nuevas facturas/remitos/pedidos los completen según el tipo de documento
-- 3. Los datos logísticos son principalmente para REMITOS
-- 4. Los datos comerciales son para FACTURAS y PEDIDOS
-- 5. El snapshot del domicilio se debe capturar al EMITIR el comprobante, no al crear el borrador

COMMIT;
```
