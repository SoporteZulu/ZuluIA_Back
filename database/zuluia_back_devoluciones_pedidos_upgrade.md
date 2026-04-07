# Upgrade de devoluciones y pedidos

## Objetivo
Documentar los cambios de base de datos necesarios para soportar:
- devoluciones de ventas
- cumplimiento de pedidos
- vistas auxiliares operativas

## Motor objetivo
Validar contra PostgreSQL local en `localhost:5432`.

## Cambios propuestos

### 1. Campos de devolución en `comprobantes`
```sql
ALTER TABLE comprobantes
    ADD COLUMN IF NOT EXISTS motivo_devolucion INTEGER,
    ADD COLUMN IF NOT EXISTS tipo_devolucion INTEGER,
    ADD COLUMN IF NOT EXISTS autorizador_devolucion_id BIGINT,
    ADD COLUMN IF NOT EXISTS fecha_autorizacion_devolucion TIMESTAMPTZ,
    ADD COLUMN IF NOT EXISTS observacion_devolucion VARCHAR(1000),
    ADD COLUMN IF NOT EXISTS reingresa_stock BOOLEAN DEFAULT FALSE NOT NULL,
    ADD COLUMN IF NOT EXISTS acredita_cuenta_corriente BOOLEAN DEFAULT FALSE NOT NULL;
```

### 2. Campos de cumplimiento en `comprobantes_items`
```sql
ALTER TABLE comprobantes_items
    ADD COLUMN IF NOT EXISTS cantidad_entregada NUMERIC(18, 4) DEFAULT 0 NOT NULL,
    ADD COLUMN IF NOT EXISTS cantidad_pendiente NUMERIC(18, 4) DEFAULT 0 NOT NULL,
    ADD COLUMN IF NOT EXISTS estado_entrega INTEGER,
    ADD COLUMN IF NOT EXISTS es_atrasado BOOLEAN DEFAULT FALSE NOT NULL;
```

### 3. Índices sugeridos para devoluciones
```sql
CREATE INDEX IF NOT EXISTS idx_comprobantes_motivo_devolucion
    ON comprobantes(motivo_devolucion)
    WHERE motivo_devolucion IS NOT NULL;

CREATE INDEX IF NOT EXISTS idx_comprobantes_tipo_devolucion
    ON comprobantes(tipo_devolucion)
    WHERE tipo_devolucion IS NOT NULL;

CREATE INDEX IF NOT EXISTS idx_comprobantes_autorizador_devolucion
    ON comprobantes(autorizador_devolucion_id)
    WHERE autorizador_devolucion_id IS NOT NULL;

CREATE INDEX IF NOT EXISTS idx_comprobantes_origen_devolucion
    ON comprobantes(comprobante_origen_id, motivo_devolucion)
    WHERE motivo_devolucion IS NOT NULL;
```

### 4. Índices sugeridos para pedidos
```sql
CREATE INDEX IF NOT EXISTS idx_comprobantes_items_estado_entrega
    ON comprobantes_items(estado_entrega)
    WHERE estado_entrega IS NOT NULL;

CREATE INDEX IF NOT EXISTS idx_comprobantes_items_atrasados
    ON comprobantes_items(es_atrasado)
    WHERE es_atrasado = TRUE;

CREATE INDEX IF NOT EXISTS idx_comprobantes_items_pendientes
    ON comprobantes_items(comprobante_id, cantidad_pendiente)
    WHERE cantidad_pendiente > 0;
```

### 5. Foreign key sugerida para autorizador
```sql
DO $$
BEGIN
    IF EXISTS (SELECT 1 FROM information_schema.tables WHERE table_name = 'usuarios') THEN
        ALTER TABLE comprobantes
            ADD CONSTRAINT fk_comprobantes_autorizador_devolucion
            FOREIGN KEY (autorizador_devolucion_id)
            REFERENCES usuarios(id);
    END IF;
END $$;
```

### 6. Comentarios descriptivos
```sql
COMMENT ON COLUMN comprobantes.motivo_devolucion IS 'Motivo de la devolución: 1=Defectuoso, 2=Error Entrega, 3=Desistimiento, 4=Vencido, 5=Dif.Precio, 6=Daño Tránsito, 7=Garantía, 8=Sobrante, 9=Cambio, 10=Ajuste, 99=Otro';
COMMENT ON COLUMN comprobantes.tipo_devolucion IS 'Tipo de operación: 0=Sin reintegro stock, 1=Con reintegro stock, 2=Con reintegro + acreditación, 3=Solo acreditación';
COMMENT ON COLUMN comprobantes.autorizador_devolucion_id IS 'Usuario que autorizó la devolución';
COMMENT ON COLUMN comprobantes.fecha_autorizacion_devolucion IS 'Fecha y hora de autorización de la devolución';
COMMENT ON COLUMN comprobantes.observacion_devolucion IS 'Observación específica de la devolución';
COMMENT ON COLUMN comprobantes.reingresa_stock IS 'Indica si la devolución reingresa mercadería al stock';
COMMENT ON COLUMN comprobantes.acredita_cuenta_corriente IS 'Indica si la devolución acredita en cuenta corriente del cliente';

COMMENT ON COLUMN comprobantes_items.cantidad_entregada IS 'Cantidad efectivamente entregada del item (para pedidos)';
COMMENT ON COLUMN comprobantes_items.cantidad_pendiente IS 'Cantidad pendiente de entrega del item (para pedidos)';
COMMENT ON COLUMN comprobantes_items.estado_entrega IS 'Estado de entrega: 0=No entregado, 1=Parcial, 2=Completo, 3=Sobrepasado';
COMMENT ON COLUMN comprobantes_items.es_atrasado IS 'Indica si el item está atrasado respecto a fecha de entrega compromiso';
```

### 7. Inicialización de registros existentes
```sql
UPDATE comprobantes_items
SET 
    cantidad_entregada = 0,
    cantidad_pendiente = cantidad,
    es_atrasado = FALSE
WHERE cantidad_entregada IS NULL OR cantidad_pendiente IS NULL;
```

### 8. Vista auxiliar `v_devoluciones_ventas`
```sql
CREATE OR REPLACE VIEW v_devoluciones_ventas AS
SELECT 
    c.id,
    c.numero,
    c.fecha,
    c.tercero_id,
    c.total,
    c.motivo_devolucion,
    CASE c.motivo_devolucion
        WHEN 1 THEN 'Producto Defectuoso'
        WHEN 2 THEN 'Error en Entrega'
        WHEN 3 THEN 'Desistimiento Cliente'
        WHEN 4 THEN 'Producto Vencido'
        WHEN 5 THEN 'Diferencia de Precio'
        WHEN 6 THEN 'Daño en Tránsito'
        WHEN 7 THEN 'Garantía'
        WHEN 8 THEN 'Sobrante'
        WHEN 9 THEN 'Cambio'
        WHEN 10 THEN 'Ajuste Inventario'
        WHEN 99 THEN 'Otro'
        ELSE NULL
    END AS motivo_devolucion_desc,
    c.tipo_devolucion,
    CASE c.tipo_devolucion
        WHEN 0 THEN 'Sin Reintegro Stock'
        WHEN 1 THEN 'Con Reintegro Stock'
        WHEN 2 THEN 'Con Reintegro Stock y Acreditación'
        WHEN 3 THEN 'Solo Acreditación'
        ELSE NULL
    END AS tipo_devolucion_desc,
    c.autorizador_devolucion_id,
    c.fecha_autorizacion_devolucion,
    c.observacion_devolucion,
    c.reingresa_stock,
    c.acredita_cuenta_corriente,
    c.comprobante_origen_id,
    c.estado
FROM comprobantes c
WHERE c.motivo_devolucion IS NOT NULL;
```

### 9. Vista auxiliar `v_pedidos_cumplimiento`
```sql
CREATE OR REPLACE VIEW v_pedidos_cumplimiento AS
SELECT 
    c.id AS pedido_id,
    c.numero,
    c.fecha,
    c.tercero_id,
    c.fecha_entrega_compromiso,
    c.estado_pedido,
    COUNT(ci.id) AS cantidad_items,
    COUNT(CASE WHEN ci.estado_entrega = 2 THEN 1 END) AS items_completados,
    COUNT(CASE WHEN ci.estado_entrega = 1 THEN 1 END) AS items_parciales,
    COUNT(CASE WHEN ci.estado_entrega = 0 OR ci.estado_entrega IS NULL THEN 1 END) AS items_pendientes,
    SUM(ci.cantidad) AS cantidad_total_pedido,
    SUM(ci.cantidad_entregada) AS cantidad_total_entregada,
    SUM(ci.cantidad_pendiente) AS cantidad_total_pendiente,
    ROUND((SUM(ci.cantidad_entregada) / NULLIF(SUM(ci.cantidad), 0)) * 100, 2) AS porcentaje_cumplimiento,
    BOOL_OR(ci.es_atrasado) AS tiene_items_atrasados
FROM comprobantes c
INNER JOIN comprobantes_items ci ON ci.comprobante_id = c.id
WHERE c.estado_pedido IS NOT NULL
GROUP BY c.id, c.numero, c.fecha, c.tercero_id, c.fecha_entrega_compromiso, c.estado_pedido;
```

## Observaciones
- Este contenido reemplaza al antiguo archivo `.sql` para evitar bloqueos de compilación en el workspace.
- La ejecución debe hacerse manualmente contra PostgreSQL local cuando se decida aplicar cambios de esquema.
