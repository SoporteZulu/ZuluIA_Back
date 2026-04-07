# Items - Upgrade Fase 1: Campos Esenciales de Ventas

Este script agrega los campos prioritarios para lograr la paridad funcional con zuluApp en el módulo de ventas/productos.

## Fase 1: Campos Esenciales de Ventas

```sql
BEGIN;

-- ─────────────────────────────────────────────────────────────────────────────
-- AGREGAR NUEVOS CAMPOS A TABLA ITEMS
-- ─────────────────────────────────────────────────────────────────────────────

-- Campos de aplicabilidad
ALTER TABLE items ADD COLUMN IF NOT EXISTS aplica_ventas boolean NOT NULL DEFAULT TRUE;
ALTER TABLE items ADD COLUMN IF NOT EXISTS aplica_compras boolean NOT NULL DEFAULT TRUE;

-- Campos de precios y descuentos
ALTER TABLE items ADD COLUMN IF NOT EXISTS porcentaje_ganancia numeric(18,4);
ALTER TABLE items ADD COLUMN IF NOT EXISTS porcentaje_maximo_descuento numeric(18,4);

-- Campos de impuestos y régimen
ALTER TABLE items ADD COLUMN IF NOT EXISTS es_rpt boolean NOT NULL DEFAULT FALSE;

-- Campos de sistema
ALTER TABLE items ADD COLUMN IF NOT EXISTS es_sistema boolean NOT NULL DEFAULT FALSE;

-- Comentarios en columnas
COMMENT ON COLUMN items.aplica_ventas IS 'Indica si el item se puede usar en operaciones de venta';
COMMENT ON COLUMN items.aplica_compras IS 'Indica si el item se puede usar en operaciones de compra';
COMMENT ON COLUMN items.porcentaje_ganancia IS 'Porcentaje de ganancia sobre el costo para calcular precio de venta automático';
COMMENT ON COLUMN items.porcentaje_maximo_descuento IS 'Porcentaje máximo de descuento permitido en ventas';
COMMENT ON COLUMN items.es_rpt IS 'Indica si el item está sujeto a Régimen de Percepción y Retención de Impuestos';
COMMENT ON COLUMN items.es_sistema IS 'Indica si es un item del sistema (no modificable por usuarios)';

-- ─────────────────────────────────────────────────────────────────────────────
-- CREAR ÍNDICES PARA MEJORAR PERFORMANCE
-- ─────────────────────────────────────────────────────────────────────────────

CREATE INDEX IF NOT EXISTS idx_items_aplica_ventas 
    ON items(aplica_ventas) WHERE aplica_ventas = TRUE;

CREATE INDEX IF NOT EXISTS idx_items_aplica_compras 
    ON items(aplica_compras) WHERE aplica_compras = TRUE;

CREATE INDEX IF NOT EXISTS idx_items_es_rpt 
    ON items(es_rpt) WHERE es_rpt = TRUE;

CREATE INDEX IF NOT EXISTS idx_items_es_sistema 
    ON items(es_sistema) WHERE es_sistema = TRUE;

-- ─────────────────────────────────────────────────────────────────────────────
-- CONSTRAINTS Y VALIDACIONES
-- ─────────────────────────────────────────────────────────────────────────────

-- Validar que porcentaje de ganancia sea >= 0
ALTER TABLE items ADD CONSTRAINT chk_items_porcentaje_ganancia 
    CHECK (porcentaje_ganancia IS NULL OR porcentaje_ganancia >= 0);

-- Validar que porcentaje máximo descuento esté entre 0 y 100
ALTER TABLE items ADD CONSTRAINT chk_items_porcentaje_maximo_descuento 
    CHECK (porcentaje_maximo_descuento IS NULL OR (porcentaje_maximo_descuento >= 0 AND porcentaje_maximo_descuento <= 100));

-- ─────────────────────────────────────────────────────────────────────────────
-- MIGRACIÓN DE DATOS EXISTENTES
-- ─────────────────────────────────────────────────────────────────────────────

-- Configurar items existentes como items normales (no del sistema)
UPDATE items 
SET es_sistema = FALSE 
WHERE es_sistema IS NULL;

-- Configurar items existentes como aplicables a ventas y compras por defecto
UPDATE items 
SET aplica_ventas = TRUE, aplica_compras = TRUE 
WHERE aplica_ventas IS NULL OR aplica_compras IS NULL;

-- Configurar items financieros como NO aplicables a ventas ni compras de stock
UPDATE items 
SET aplica_ventas = FALSE, aplica_compras = FALSE, maneja_stock = FALSE 
WHERE es_financiero = TRUE;

-- Configurar porcentaje de ganancia basado en precio de venta y costo existentes
UPDATE items 
SET porcentaje_ganancia = CASE 
    WHEN precio_costo > 0 AND precio_venta > precio_costo 
    THEN ((precio_venta - precio_costo) / precio_costo) * 100 
    ELSE NULL 
END
WHERE porcentaje_ganancia IS NULL AND precio_costo > 0;

-- ─────────────────────────────────────────────────────────────────────────────
-- FUNCIÓN: Calcular precio de venta automático basado en ganancia
-- ─────────────────────────────────────────────────────────────────────────────

CREATE OR REPLACE FUNCTION calcular_precio_venta_por_ganancia()
RETURNS TRIGGER AS $$
BEGIN
    -- Si se modifica el costo y existe porcentaje de ganancia, recalcular precio venta
    IF (TG_OP = 'UPDATE' AND OLD.precio_costo <> NEW.precio_costo) 
       OR TG_OP = 'INSERT' THEN
        IF NEW.porcentaje_ganancia IS NOT NULL AND NEW.porcentaje_ganancia > 0 THEN
            NEW.precio_venta := NEW.precio_costo * (1 + (NEW.porcentaje_ganancia / 100));
        END IF;
    END IF;
    
    RETURN NEW;
END;
$$ LANGUAGE plpgsql;

-- Crear trigger (solo si no existe)
DROP TRIGGER IF EXISTS trg_items_calcular_precio_venta ON items;
CREATE TRIGGER trg_items_calcular_precio_venta
    BEFORE INSERT OR UPDATE ON items
    FOR EACH ROW
    EXECUTE FUNCTION calcular_precio_venta_por_ganancia();

-- ─────────────────────────────────────────────────────────────────────────────
-- VISTA: Items para ventas (solo items aplicables y activos)
-- ─────────────────────────────────────────────────────────────────────────────

CREATE OR REPLACE VIEW vw_items_ventas AS
SELECT 
    i.id,
    i.codigo,
    i.codigo_barras,
    i.descripcion,
    i.descripcion_adicional,
    i.categoria_id,
    i.marca_id,
    i.unidad_medida_id,
    i.alicuota_iva_id,
    i.moneda_id,
    i.precio_costo,
    i.precio_venta,
    i.porcentaje_ganancia,
    i.porcentaje_maximo_descuento,
    i.maneja_stock,
    i.stock_minimo,
    i.stock_maximo,
    i.es_rpt,
    i.activo
FROM items i
WHERE i.aplica_ventas = TRUE 
  AND i.activo = TRUE
  AND i.es_producto = TRUE
  AND i.deleted_at IS NULL;

COMMENT ON VIEW vw_items_ventas IS 'Items activos aplicables a operaciones de venta';

-- ─────────────────────────────────────────────────────────────────────────────
-- VISTA: Items para compras (solo items aplicables y activos)
-- ─────────────────────────────────────────────────────────────────────────────

CREATE OR REPLACE VIEW vw_items_compras AS
SELECT 
    i.id,
    i.codigo,
    i.codigo_barras,
    i.descripcion,
    i.descripcion_adicional,
    i.categoria_id,
    i.marca_id,
    i.unidad_medida_id,
    i.alicuota_iva_id,
    i.moneda_id,
    i.precio_costo,
    i.maneja_stock,
    i.stock_minimo,
    i.stock_maximo,
    i.activo
FROM items i
WHERE i.aplica_compras = TRUE 
  AND i.activo = TRUE
  AND i.deleted_at IS NULL;

COMMENT ON VIEW vw_items_compras IS 'Items activos aplicables a operaciones de compra';

-- ─────────────────────────────────────────────────────────────────────────────
-- VERIFICACIÓN
-- ─────────────────────────────────────────────────────────────────────────────

DO $$
BEGIN
    -- Verificar que las columnas existen
    IF NOT EXISTS (
        SELECT 1 FROM information_schema.columns 
        WHERE table_name = 'items' AND column_name = 'aplica_ventas'
    ) THEN
        RAISE EXCEPTION 'Columna aplica_ventas no fue creada correctamente';
    END IF;
    
    IF NOT EXISTS (
        SELECT 1 FROM information_schema.columns 
        WHERE table_name = 'items' AND column_name = 'porcentaje_ganancia'
    ) THEN
        RAISE EXCEPTION 'Columna porcentaje_ganancia no fue creada correctamente';
    END IF;
    
    RAISE NOTICE 'Verificación exitosa: Todas las columnas de Fase 1 fueron creadas';
END $$;

COMMIT;
```

## Fase 2: Campos de stock operativo y logística

Esta fase agrega los campos mínimos detectados en `zuluApp` para mejorar la paridad operativa de `Items` en ventas y logística.

```sql
BEGIN;

ALTER TABLE items ADD COLUMN IF NOT EXISTS punto_reposicion numeric(18,4);
ALTER TABLE items ADD COLUMN IF NOT EXISTS stock_seguridad numeric(18,4);
ALTER TABLE items ADD COLUMN IF NOT EXISTS peso numeric(18,4);
ALTER TABLE items ADD COLUMN IF NOT EXISTS volumen numeric(18,4);

COMMENT ON COLUMN items.punto_reposicion IS 'Cantidad sugerida para disparar reposición operativa';
COMMENT ON COLUMN items.stock_seguridad IS 'Stock reservado como colchón operativo';
COMMENT ON COLUMN items.peso IS 'Peso unitario del item para circuitos logísticos';
COMMENT ON COLUMN items.volumen IS 'Volumen unitario del item para circuitos logísticos';

ALTER TABLE items ADD CONSTRAINT chk_items_punto_reposicion
    CHECK (punto_reposicion IS NULL OR punto_reposicion >= 0);

ALTER TABLE items ADD CONSTRAINT chk_items_stock_seguridad
    CHECK (stock_seguridad IS NULL OR stock_seguridad >= 0);

ALTER TABLE items ADD CONSTRAINT chk_items_peso
    CHECK (peso IS NULL OR peso >= 0);

ALTER TABLE items ADD CONSTRAINT chk_items_volumen
    CHECK (volumen IS NULL OR volumen >= 0);

COMMIT;
```

## Query de validación Fase 2

```sql
SELECT
    codigo,
    stock_minimo,
    stock_maximo,
    punto_reposicion,
    stock_seguridad,
    peso,
    volumen
FROM items
WHERE deleted_at IS NULL
ORDER BY codigo;
```

## Fase 3: Campos avanzados de operación comercial y stock

Esta fase agrega campos detectados en `zuluApp` para cerrar brechas funcionales del maestro de ítems.

```sql
BEGIN;

ALTER TABLE items ADD COLUMN IF NOT EXISTS codigo_alternativo varchar(50);
ALTER TABLE items ADD COLUMN IF NOT EXISTS es_trazable boolean NOT NULL DEFAULT FALSE;
ALTER TABLE items ADD COLUMN IF NOT EXISTS permite_fraccionamiento boolean NOT NULL DEFAULT FALSE;
ALTER TABLE items ADD COLUMN IF NOT EXISTS dias_vencimiento_limite integer;
ALTER TABLE items ADD COLUMN IF NOT EXISTS deposito_default_id bigint;

COMMENT ON COLUMN items.codigo_alternativo IS 'Mapea al Codigo2 legado o código secundario del ítem';
COMMENT ON COLUMN items.es_trazable IS 'Indica si el ítem requiere control por lote/serie/vencimiento';
COMMENT ON COLUMN items.permite_fraccionamiento IS 'Indica si el ítem puede operarse en cantidades fraccionarias';
COMMENT ON COLUMN items.dias_vencimiento_limite IS 'Días mínimos de vigencia exigidos para operar stock vencible';
COMMENT ON COLUMN items.deposito_default_id IS 'Depósito por defecto para operaciones de stock del ítem';

ALTER TABLE items ADD CONSTRAINT chk_items_dias_vencimiento_limite
    CHECK (dias_vencimiento_limite IS NULL OR dias_vencimiento_limite >= 0);

CREATE INDEX IF NOT EXISTS idx_items_codigo_alternativo ON items(codigo_alternativo);
CREATE INDEX IF NOT EXISTS idx_items_es_trazable ON items(es_trazable) WHERE es_trazable = TRUE;
CREATE INDEX IF NOT EXISTS idx_items_deposito_default_id ON items(deposito_default_id) WHERE deposito_default_id IS NOT NULL;

COMMIT;
```

## Query de validación Fase 3

```sql
SELECT
    codigo,
    codigo_alternativo,
    es_trazable,
    permite_fraccionamiento,
    dias_vencimiento_limite,
    deposito_default_id
FROM items
WHERE deleted_at IS NULL
ORDER BY codigo;
```

## Fase 4: IVA compra e impuesto interno por item

Esta fase agrega campos fiscales de compra para acercar el maestro de ítems al comportamiento legado de `zuluApp`.

```sql
BEGIN;

ALTER TABLE items ADD COLUMN IF NOT EXISTS alicuota_iva_compra_id bigint;
ALTER TABLE items ADD COLUMN IF NOT EXISTS impuesto_interno_id bigint;

COMMENT ON COLUMN items.alicuota_iva_compra_id IS 'Alícuota de IVA específica para circuitos de compra del item';
COMMENT ON COLUMN items.impuesto_interno_id IS 'Impuesto interno asociado al item para compras o ventas especiales';

CREATE INDEX IF NOT EXISTS idx_items_alicuota_iva_compra_id ON items(alicuota_iva_compra_id) WHERE alicuota_iva_compra_id IS NOT NULL;
CREATE INDEX IF NOT EXISTS idx_items_impuesto_interno_id ON items(impuesto_interno_id) WHERE impuesto_interno_id IS NOT NULL;

COMMIT;
```

## Query de validación Fase 4

```sql
SELECT
    codigo,
    alicuota_iva_id,
    alicuota_iva_compra_id,
    impuesto_interno_id
FROM items
WHERE deleted_at IS NULL
ORDER BY codigo;
```

## Query de Validación

```sql
-- Verificar estructura de tabla items con nuevos campos
SELECT 
    column_name,
    data_type,
    is_nullable,
    column_default
FROM information_schema.columns
WHERE table_name = 'items'
  AND column_name IN (
      'aplica_ventas',
      'aplica_compras',
      'porcentaje_ganancia',
      'porcentaje_maximo_descuento',
      'es_rpt',
      'es_sistema'
  )
ORDER BY ordinal_position;
```

## Query de Smoke Test

```sql
-- Crear item de prueba con nuevos campos
INSERT INTO items (
    codigo,
    descripcion,
    unidad_medida_id,
    alicuota_iva_id,
    moneda_id,
    es_producto,
    precio_costo,
    porcentaje_ganancia,

    porcentaje_maximo_descuento,
    aplica_ventas,
    aplica_compras,
    es_rpt,
    maneja_stock,
    created_at,
    updated_at
) VALUES (
    'TEST-FASE1-001',
    'Item de prueba Fase 1',
    1,  -- Ajustar según unidad_medida existente
    1,  -- Ajustar según alicuota_iva existente
    1,  -- Ajustar según moneda existente
    TRUE,
    100.00,  -- Costo
    30.00,   -- 30% de ganancia
    15.00,   -- Máximo 15% de descuento
    TRUE,    -- Aplica ventas
    TRUE,    -- Aplica compras
    FALSE,   -- No es RPT
    TRUE,    -- Maneja stock
    now(),
    now()
);

-- Verificar que el precio de venta se calculó automáticamente (trigger)
SELECT 
    codigo,
    precio_costo,
    porcentaje_ganancia,
    precio_venta,
    (precio_costo * (1 + (porcentaje_ganancia / 100))) as precio_venta_calculado,
    CASE 
        WHEN ABS(precio_venta - (precio_costo * (1 + (porcentaje_ganancia / 100)))) < 0.01 
        THEN 'OK' 
        ELSE 'ERROR' 
    END as validacion
FROM items
WHERE codigo = 'TEST-FASE1-001';

-- Limpiar
-- DELETE FROM items WHERE codigo = 'TEST-FASE1-001';
```

## Rollback (si es necesario)

```sql
BEGIN;

-- Eliminar vistas
DROP VIEW IF EXISTS vw_items_compras;
DROP VIEW IF EXISTS vw_items_ventas;

-- Eliminar trigger y función
DROP TRIGGER IF EXISTS trg_items_calcular_precio_venta ON items;
DROP FUNCTION IF EXISTS calcular_precio_venta_por_ganancia();

-- Eliminar constraints
ALTER TABLE items DROP CONSTRAINT IF EXISTS chk_items_porcentaje_ganancia;
ALTER TABLE items DROP CONSTRAINT IF EXISTS chk_items_porcentaje_maximo_descuento;

-- Eliminar columnas (cuidado: esto elimina los datos)
ALTER TABLE items DROP COLUMN IF EXISTS aplica_ventas;
ALTER TABLE items DROP COLUMN IF EXISTS aplica_compras;
ALTER TABLE items DROP COLUMN IF EXISTS porcentaje_ganancia;
ALTER TABLE items DROP COLUMN IF EXISTS porcentaje_maximo_descuento;
ALTER TABLE items DROP COLUMN IF EXISTS es_rpt;
ALTER TABLE items DROP COLUMN IF EXISTS es_sistema;

COMMIT;
```
