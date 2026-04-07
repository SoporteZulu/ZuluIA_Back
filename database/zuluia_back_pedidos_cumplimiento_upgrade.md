# Migración: Campos de Pedido en Comprobantes

## Fecha
2026-03-21

## Descripción
Agrega campos específicos de pedido a las tablas `comprobantes` y `comprobantes_items` para soportar el ciclo completo de pedidos con estados de cumplimiento, entregas parciales y atrasos.

## Contexto
Esta migración es parte del trabajo de **paridad funcional 100%** entre la vista de pedidos del legado `C:\Zulu\zuluApp` y el nuevo backend `C:\Zulu\ZuluIA_Back`.

## SQL Script

```sql
-- Script de migración: Agregar campos de pedido
-- Fecha: 2026-03-21
-- Workspace: C:\Zulu\ZuluIA_Back

BEGIN;

-- ========================================
-- 1. Agregar campos a tabla comprobantes
-- ========================================

-- Fecha de entrega comprometida con el cliente
ALTER TABLE IF EXISTS comprobantes
    ADD COLUMN IF NOT EXISTS fecha_entrega_compromiso date;

COMMENT ON COLUMN comprobantes.fecha_entrega_compromiso IS 'Fecha comprometida de entrega al cliente (solo para pedidos)';

-- Estado del pedido
ALTER TABLE IF EXISTS comprobantes
    ADD COLUMN IF NOT EXISTS estado_pedido smallint;

COMMENT ON COLUMN comprobantes.estado_pedido IS 'Estado del pedido: 0=Pendiente, 1=EnProceso, 2=Completado, 3=Cerrado, 4=Anulado';

-- Motivo y fecha de cierre del pedido
ALTER TABLE IF EXISTS comprobantes
    ADD COLUMN IF NOT EXISTS motivo_cierre_pedido character varying(500);

ALTER TABLE IF EXISTS comprobantes
    ADD COLUMN IF NOT EXISTS fecha_cierre_pedido timestamp with time zone;

COMMENT ON COLUMN comprobantes.motivo_cierre_pedido IS 'Motivo por el cual se cerró el pedido sin completarse totalmente';
COMMENT ON COLUMN comprobantes.fecha_cierre_pedido IS 'Fecha y hora en que se cerró el pedido';

-- Crear índices sobre campos de pedido
CREATE INDEX IF NOT EXISTS ix_comprobantes_estado_pedido
    ON comprobantes (estado_pedido)
    WHERE estado_pedido IS NOT NULL;

CREATE INDEX IF NOT EXISTS ix_comprobantes_fecha_entrega_compromiso
    ON comprobantes (fecha_entrega_compromiso)
    WHERE fecha_entrega_compromiso IS NOT NULL;

CREATE INDEX IF NOT EXISTS ix_comprobantes_pedidos_activos
    ON comprobantes (estado_pedido, fecha_entrega_compromiso)
    WHERE estado_pedido IN (0, 1);

-- ========================================
-- 2. Agregar campos a tabla comprobantes_items
-- ========================================

-- Cantidades de cumplimiento
ALTER TABLE IF EXISTS comprobantes_items
    ADD COLUMN IF NOT EXISTS cantidad_entregada numeric(18,4) NOT NULL DEFAULT 0;

ALTER TABLE IF EXISTS comprobantes_items
    ADD COLUMN IF NOT EXISTS cantidad_pendiente numeric(18,4) NOT NULL DEFAULT 0;

COMMENT ON COLUMN comprobantes_items.cantidad_entregada IS 'Cantidad total entregada de este item del pedido';
COMMENT ON COLUMN comprobantes_items.cantidad_pendiente IS 'Cantidad pendiente de entregar de este item del pedido';

-- Estado de entrega del item
ALTER TABLE IF EXISTS comprobantes_items
    ADD COLUMN IF NOT EXISTS estado_entrega_item smallint;

COMMENT ON COLUMN comprobantes_items.estado_entrega_item IS 'Estado entrega: 0=NoEntregado, 1=EntregaParcial, 2=EntregaCompleta, 3=EntregaSobrepasada';

-- Indicador de atraso
ALTER TABLE IF EXISTS comprobantes_items
    ADD COLUMN IF NOT EXISTS es_atrasado boolean NOT NULL DEFAULT false;

COMMENT ON COLUMN comprobantes_items.es_atrasado IS 'Indica si el renglón está atrasado respecto a la fecha de entrega comprometida';

-- Crear índices sobre campos de item de pedido
CREATE INDEX IF NOT EXISTS ix_comprobantes_items_estado_entrega
    ON comprobantes_items (estado_entrega_item)
    WHERE estado_entrega_item IS NOT NULL;

CREATE INDEX IF NOT EXISTS ix_comprobantes_items_atrasados
    ON comprobantes_items (es_atrasado)
    WHERE es_atrasado = true;

CREATE INDEX IF NOT EXISTS ix_comprobantes_items_pedido_cumplimiento
    ON comprobantes_items (comprobante_id, estado_entrega_item);

-- ========================================
-- 3. Actualizar datos existentes (opcional)
-- ========================================

-- Inicializar cantidad_pendiente = cantidad para items existentes de pedidos
-- Solo si la tabla ya tenía datos y queremos inicializar
UPDATE comprobantes_items
SET cantidad_pendiente = cantidad
WHERE cantidad_pendiente = 0 
  AND cantidad > 0;

COMMIT;
```

## Rollback Script

```sql
-- Rollback: Remover campos de pedido
BEGIN;

-- Remover índices
DROP INDEX IF EXISTS ix_comprobantes_items_pedido_cumplimiento;
DROP INDEX IF EXISTS ix_comprobantes_items_atrasados;
DROP INDEX IF EXISTS ix_comprobantes_items_estado_entrega;
DROP INDEX IF EXISTS ix_comprobantes_pedidos_activos;
DROP INDEX IF EXISTS ix_comprobantes_fecha_entrega_compromiso;
DROP INDEX IF EXISTS ix_comprobantes_estado_pedido;

-- Remover columnas de comprobantes_items
ALTER TABLE IF EXISTS comprobantes_items DROP COLUMN IF EXISTS es_atrasado;
ALTER TABLE IF EXISTS comprobantes_items DROP COLUMN IF EXISTS estado_entrega_item;
ALTER TABLE IF EXISTS comprobantes_items DROP COLUMN IF EXISTS cantidad_pendiente;
ALTER TABLE IF EXISTS comprobantes_items DROP COLUMN IF EXISTS cantidad_entregada;

-- Remover columnas de comprobantes
ALTER TABLE IF EXISTS comprobantes DROP COLUMN IF EXISTS fecha_cierre_pedido;
ALTER TABLE IF EXISTS comprobantes DROP COLUMN IF EXISTS motivo_cierre_pedido;
ALTER TABLE IF EXISTS comprobantes DROP COLUMN IF EXISTS estado_pedido;
ALTER TABLE IF EXISTS comprobantes DROP COLUMN IF EXISTS fecha_entrega_compromiso;

COMMIT;
```

## Verificación

```sql
-- Verificar que las columnas fueron creadas
SELECT 
    column_name, 
    data_type, 
    is_nullable,
    column_default
FROM information_schema.columns
WHERE table_name IN ('comprobantes', 'comprobantes_items')
  AND column_name IN (
      'fecha_entrega_compromiso',
      'estado_pedido',
      'motivo_cierre_pedido',
      'fecha_cierre_pedido',
      'cantidad_entregada',
      'cantidad_pendiente',
      'estado_entrega_item',
      'es_atrasado'
  )
ORDER BY table_name, ordinal_position;

-- Verificar índices creados
SELECT 
    schemaname,
    tablename,
    indexname,
    indexdef
FROM pg_indexes
WHERE indexname LIKE '%pedido%' OR indexname LIKE '%entrega%'
ORDER BY tablename, indexname;
```

## Notas
- Los campos son opcionales (`NULL`) excepto los de cumplimiento en `comprobantes_items` que tienen defaults
- El campo `estado_pedido` solo se llena para comprobantes que son pedidos formales
- El campo `es_atrasado` se calcula dinámicamente en el backend comparando `fecha_entrega_compromiso` con la fecha actual
- Los índices están optimizados para consultas de pedidos activos, atrasados y por estado de entrega

## Referencias
- Documentación de paridad: `docs/ventas-pedidos-paridad-zuluapp.md`
- Entidad de dominio: `src/ZuluIA_Back.Domain/Entities/Comprobantes/Comprobante.cs`
- Enum EstadoPedido: `src/ZuluIA_Back.Domain/Enums/EstadoPedido.cs`
- Enum EstadoEntregaItem: `src/ZuluIA_Back.Domain/Enums/EstadoEntregaItem.cs`
