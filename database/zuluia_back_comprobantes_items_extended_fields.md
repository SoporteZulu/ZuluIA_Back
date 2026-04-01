# Migración: Paridad Funcional Notas de Crédito - Campos Extendidos

**Fecha**: 2024-03-20
**Objetivo**: Agregar campos extendidos a `comprobantes_items` para lograr paridad funcional con zuluApp en notas de crédito.

## Campos Agregados

### comprobantes_items
- `lote` (varchar(100), nullable): Número de lote del producto
- `serie` (varchar(100), nullable): Número de serie del producto
- `fecha_vencimiento` (date, nullable): Fecha de vencimiento del producto
- `unidad_medida_id` (bigint, nullable): FK a catálogo de unidades de medida
- `observacion_renglon` (varchar(500), nullable): Observaciones específicas del renglón
- `precio_lista_original` (decimal(18,4), nullable): Precio de lista antes de descuentos
- `comision_vendedor_renglon` (decimal(18,4), nullable): Comisión del vendedor calculada por renglón
- `comprobante_item_origen_id` (bigint, nullable): FK al renglón origen (trazabilidad)

## Script SQL

```sql
BEGIN;

-- Agregar campos extendidos a comprobantes_items
ALTER TABLE IF EXISTS comprobantes_items
    ADD COLUMN IF NOT EXISTS lote character varying(100),
    ADD COLUMN IF NOT EXISTS serie character varying(100),
    ADD COLUMN IF NOT EXISTS fecha_vencimiento date,
    ADD COLUMN IF NOT EXISTS unidad_medida_id bigint,
    ADD COLUMN IF NOT EXISTS observacion_renglon character varying(500),
    ADD COLUMN IF NOT EXISTS precio_lista_original numeric(18,4),
    ADD COLUMN IF NOT EXISTS comision_vendedor_renglon numeric(18,4),
    ADD COLUMN IF NOT EXISTS comprobante_item_origen_id bigint;

-- Crear índice para trazabilidad de items origen
CREATE INDEX IF NOT EXISTS ix_comprobantes_items_origen_id
    ON comprobantes_items (comprobante_item_origen_id)
    WHERE comprobante_item_origen_id IS NOT NULL;

-- Comentarios descriptivos
COMMENT ON COLUMN comprobantes_items.lote IS 'Número de lote del producto (para trazabilidad)';
COMMENT ON COLUMN comprobantes_items.serie IS 'Número de serie del producto (para productos serializados)';
COMMENT ON COLUMN comprobantes_items.fecha_vencimiento IS 'Fecha de vencimiento del producto (para perecederos)';
COMMENT ON COLUMN comprobantes_items.unidad_medida_id IS 'Unidad de medida específica del renglón (si difiere de la base del item)';
COMMENT ON COLUMN comprobantes_items.observacion_renglon IS 'Observaciones específicas del renglón';
COMMENT ON COLUMN comprobantes_items.precio_lista_original IS 'Precio de lista original antes de aplicar descuentos';
COMMENT ON COLUMN comprobantes_items.comision_vendedor_renglon IS 'Comisión del vendedor calculada a nivel de renglón';
COMMENT ON COLUMN comprobantes_items.comprobante_item_origen_id IS 'FK al renglón origen para trazabilidad (ej: renglón de factura en NC)';

COMMIT;
```

## Validación Post-Migración

```sql
-- Verificar que las columnas fueron agregadas
SELECT column_name, data_type, character_maximum_length, is_nullable
FROM information_schema.columns
WHERE table_name = 'comprobantes_items'
  AND column_name IN (
    'lote', 
    'serie', 
    'fecha_vencimiento', 
    'unidad_medida_id',
    'observacion_renglon',
    'precio_lista_original',
    'comision_vendedor_renglon',
    'comprobante_item_origen_id'
  )
ORDER BY column_name;

-- Verificar índice
SELECT indexname, indexdef
FROM pg_indexes
WHERE tablename = 'comprobantes_items'
  AND indexname = 'ix_comprobantes_items_origen_id';

-- Verificar que no hay datos inconsistentes
SELECT COUNT(*) as total_items,
       COUNT(lote) as con_lote,
       COUNT(serie) as con_serie,
       COUNT(fecha_vencimiento) as con_vencimiento,
       COUNT(comprobante_item_origen_id) as con_origen
FROM comprobantes_items;
```

## Rollback (si es necesario)

```sql
BEGIN;

-- Eliminar índice
DROP INDEX IF EXISTS ix_comprobantes_items_origen_id;

-- Eliminar columnas (usar con precaución en producción)
ALTER TABLE IF EXISTS comprobantes_items
    DROP COLUMN IF EXISTS lote,
    DROP COLUMN IF EXISTS serie,
    DROP COLUMN IF EXISTS fecha_vencimiento,
    DROP COLUMN IF EXISTS unidad_medida_id,
    DROP COLUMN IF EXISTS observacion_renglon,
    DROP COLUMN IF EXISTS precio_lista_original,
    DROP COLUMN IF EXISTS comision_vendedor_renglon,
    DROP COLUMN IF EXISTS comprobante_item_origen_id;

COMMIT;
```

## Notas Importantes

1. **Compatibilidad hacia atrás**: Todos los campos son nullable, por lo que los registros existentes no se ven afectados
2. **Performance**: El índice en `comprobante_item_origen_id` es parcial (solo para valores no nulos) para optimizar espacio
3. **Trazabilidad**: El campo `comprobante_item_origen_id` permite rastrear qué renglón de la factura originó cada renglón de NC
4. **Unidades de medida**: Se recomienda crear tabla `unidades_medida` si aún no existe
5. **Auditoría**: Los campos `CreatedBy` y `UpdatedBy` ya existen en la tabla `comprobantes` (heredados de `AuditableEntity`)

## Dependencias

- Tabla `comprobantes_items` debe existir
- Tabla `unidades_medida` (opcional, para FK de `unidad_medida_id`)

## Referencias

- Documento de paridad: `docs/notas-credito-paridad-funcional.md`
- Entidad: `src/ZuluIA_Back.Domain/Entities/Comprobantes/ComprobanteItem.cs`
- Configuración EF: `src/ZuluIA_Back.Infrastructure/Persistence/Configurations/ComprobanteItemConfiguration.cs`
- DTO: `src/ZuluIA_Back.Application/Features/Comprobantes/DTOs/ComprobanteItemDto.cs`

---

**Última actualización**: 2024-03-20
**Aplicado a**: PostgreSQL 14+
**Estado**: Pendiente de ejecución
