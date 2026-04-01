# Script de Migración - Ampliación de Cobros y Recibos

**Fecha:** 2025-01-20  
**Objetivo:** Agregar campos comerciales, operativos y fiscales a cobros y recibos para paridad funcional con zuluApp

## 1. Ampliación de tabla `cobros`

```sql
-- Campos comerciales
ALTER TABLE cobros ADD COLUMN vendedor_id BIGINT NULL;
ALTER TABLE cobros ADD COLUMN cobrador_id BIGINT NULL;
ALTER TABLE cobros ADD COLUMN zona_comercial_id BIGINT NULL;
ALTER TABLE cobros ADD COLUMN usuario_cajero_id BIGINT NULL;
ALTER TABLE cobros ADD COLUMN ventanilla_turno VARCHAR(50) NULL;
ALTER TABLE cobros ADD COLUMN tipo_cobro INT NOT NULL DEFAULT 0;

-- Observaciones adicionales
ALTER TABLE cobros ADD COLUMN observacion_interna TEXT NULL;

-- Snapshot de datos del tercero
ALTER TABLE cobros ADD COLUMN tercero_cuit VARCHAR(20) NULL;
ALTER TABLE cobros ADD COLUMN tercero_condicion_iva VARCHAR(100) NULL;
ALTER TABLE cobros ADD COLUMN tercero_domicilio_snapshot VARCHAR(500) NULL;

-- Comentarios
COMMENT ON COLUMN cobros.vendedor_id IS 'Vendedor asociado al cliente (referencia)';
COMMENT ON COLUMN cobros.cobrador_id IS 'Cobrador que gestionó/realizó el cobro';
COMMENT ON COLUMN cobros.zona_comercial_id IS 'Zona comercial del cliente';
COMMENT ON COLUMN cobros.usuario_cajero_id IS 'Usuario que registró el cobro en caja';
COMMENT ON COLUMN cobros.ventanilla_turno IS 'Ventanilla o turno del cajero (ej: V1-MAÑANA)';
COMMENT ON COLUMN cobros.tipo_cobro IS '0=Administrativo, 1=VentanillaContraEntrega, 2=VentanillaContraPedido, 3=CobranzaEnRuta, 4=Bancario, 5=Electronico';
COMMENT ON COLUMN cobros.observacion_interna IS 'Observaciones internas no visibles en recibo';
COMMENT ON COLUMN cobros.tercero_cuit IS 'CUIT del tercero en el momento del cobro (snapshot)';
COMMENT ON COLUMN cobros.tercero_condicion_iva IS 'Condición IVA del tercero en el momento del cobro (snapshot)';
COMMENT ON COLUMN cobros.tercero_domicilio_snapshot IS 'Domicilio completo del tercero en el momento del cobro (snapshot)';

-- Índices para búsquedas
CREATE INDEX idx_cobros_vendedor_id ON cobros(vendedor_id) WHERE deleted_at IS NULL;
CREATE INDEX idx_cobros_cobrador_id ON cobros(cobrador_id) WHERE deleted_at IS NULL;
CREATE INDEX idx_cobros_zona_comercial_id ON cobros(zona_comercial_id) WHERE deleted_at IS NULL;
CREATE INDEX idx_cobros_usuario_cajero_id ON cobros(usuario_cajero_id) WHERE deleted_at IS NULL;
CREATE INDEX idx_cobros_tipo_cobro ON cobros(tipo_cobro) WHERE deleted_at IS NULL;
CREATE INDEX idx_cobros_fecha_tipo ON cobros(fecha, tipo_cobro) WHERE deleted_at IS NULL;
```

## 2. Ampliación de tabla `cobros_medios`

```sql
-- Datos específicos para transferencias
ALTER TABLE cobros_medios ADD COLUMN banco_origen VARCHAR(100) NULL;
ALTER TABLE cobros_medios ADD COLUMN banco_destino VARCHAR(100) NULL;
ALTER TABLE cobros_medios ADD COLUMN numero_operacion VARCHAR(50) NULL;
ALTER TABLE cobros_medios ADD COLUMN fecha_acreditacion DATE NULL;

-- Datos específicos para tarjetas
ALTER TABLE cobros_medios ADD COLUMN terminal_pos VARCHAR(50) NULL;
ALTER TABLE cobros_medios ADD COLUMN numero_cupon VARCHAR(50) NULL;
ALTER TABLE cobros_medios ADD COLUMN numero_lote VARCHAR(50) NULL;
ALTER TABLE cobros_medios ADD COLUMN codigo_autorizacion VARCHAR(50) NULL;
ALTER TABLE cobros_medios ADD COLUMN cantidad_cuotas INT NULL;
ALTER TABLE cobros_medios ADD COLUMN plan_cuotas VARCHAR(100) NULL;
ALTER TABLE cobros_medios ADD COLUMN fecha_acreditacion_estimada DATE NULL;

-- Comentarios
COMMENT ON COLUMN cobros_medios.banco_origen IS 'Banco origen de transferencia';
COMMENT ON COLUMN cobros_medios.banco_destino IS 'Banco destino de transferencia';
COMMENT ON COLUMN cobros_medios.numero_operacion IS 'Número de operación de transferencia';
COMMENT ON COLUMN cobros_medios.fecha_acreditacion IS 'Fecha de acreditación de transferencia';
COMMENT ON COLUMN cobros_medios.terminal_pos IS 'Terminal/POS donde se procesó tarjeta';
COMMENT ON COLUMN cobros_medios.numero_cupon IS 'Número de cupón de tarjeta';
COMMENT ON COLUMN cobros_medios.numero_lote IS 'Número de lote/batch de tarjeta';
COMMENT ON COLUMN cobros_medios.codigo_autorizacion IS 'Código de autorización de tarjeta';
COMMENT ON COLUMN cobros_medios.cantidad_cuotas IS 'Cantidad de cuotas (si aplica)';
COMMENT ON COLUMN cobros_medios.plan_cuotas IS 'Descripción del plan de cuotas';
COMMENT ON COLUMN cobros_medios.fecha_acreditacion_estimada IS 'Fecha estimada de acreditación de tarjeta';

-- Índices para conciliaciones
CREATE INDEX idx_cobros_medios_numero_operacion ON cobros_medios(numero_operacion) WHERE numero_operacion IS NOT NULL;
CREATE INDEX idx_cobros_medios_numero_cupon ON cobros_medios(numero_cupon) WHERE numero_cupon IS NOT NULL;
CREATE INDEX idx_cobros_medios_numero_lote ON cobros_medios(numero_lote) WHERE numero_lote IS NOT NULL;
CREATE INDEX idx_cobros_medios_fecha_acreditacion ON cobros_medios(fecha_acreditacion) WHERE fecha_acreditacion IS NOT NULL;
```

## 3. Ampliación de tabla `recibos`

```sql
-- Campos comerciales
ALTER TABLE recibos ADD COLUMN vendedor_id BIGINT NULL;
ALTER TABLE recibos ADD COLUMN cobrador_id BIGINT NULL;
ALTER TABLE recibos ADD COLUMN zona_comercial_id BIGINT NULL;
ALTER TABLE recibos ADD COLUMN usuario_cajero_id BIGINT NULL;

-- Snapshot de datos del tercero
ALTER TABLE recibos ADD COLUMN tercero_cuit VARCHAR(20) NULL;
ALTER TABLE recibos ADD COLUMN tercero_condicion_iva VARCHAR(100) NULL;
ALTER TABLE recibos ADD COLUMN tercero_domicilio VARCHAR(500) NULL;

-- Leyendas y observaciones
ALTER TABLE recibos ADD COLUMN leyenda_fiscal TEXT NULL;

-- Metadatos de impresión
ALTER TABLE recibos ADD COLUMN formato_impresion VARCHAR(50) NULL;
ALTER TABLE recibos ADD COLUMN copias_impresas INT NULL DEFAULT 0;
ALTER TABLE recibos ADD COLUMN fecha_impresion TIMESTAMPTZ NULL;

-- Comentarios
COMMENT ON COLUMN recibos.vendedor_id IS 'Vendedor asociado al cliente';
COMMENT ON COLUMN recibos.cobrador_id IS 'Cobrador que realizó el cobro';
COMMENT ON COLUMN recibos.zona_comercial_id IS 'Zona comercial del cliente';
COMMENT ON COLUMN recibos.usuario_cajero_id IS 'Usuario que emitió el recibo';
COMMENT ON COLUMN recibos.tercero_cuit IS 'CUIT del tercero (snapshot)';
COMMENT ON COLUMN recibos.tercero_condicion_iva IS 'Condición IVA del tercero (snapshot)';
COMMENT ON COLUMN recibos.tercero_domicilio IS 'Domicilio del tercero (snapshot)';
COMMENT ON COLUMN recibos.leyenda_fiscal IS 'Leyenda fiscal adicional para el recibo';
COMMENT ON COLUMN recibos.formato_impresion IS 'Formato utilizado para impresión (PDF, Thermal, etc)';
COMMENT ON COLUMN recibos.copias_impresas IS 'Cantidad de copias impresas del recibo';
COMMENT ON COLUMN recibos.fecha_impresion IS 'Última fecha de impresión del recibo';

-- Índices
CREATE INDEX idx_recibos_vendedor_id ON recibos(vendedor_id) WHERE deleted_at IS NULL;
CREATE INDEX idx_recibos_cobrador_id ON recibos(cobrador_id) WHERE deleted_at IS NULL;
CREATE INDEX idx_recibos_zona_comercial_id ON recibos(zona_comercial_id) WHERE deleted_at IS NULL;
CREATE INDEX idx_recibos_usuario_cajero_id ON recibos(usuario_cajero_id) WHERE deleted_at IS NULL;
CREATE INDEX idx_recibos_fecha_impresion ON recibos(fecha_impresion) WHERE fecha_impresion IS NOT NULL;
```

## 4. Ampliación de tabla `recibos_items`

```sql
-- Vinculación a comprobante imputado
ALTER TABLE recibos_items ADD COLUMN comprobante_imputado_id BIGINT NULL;

-- Comentario
COMMENT ON COLUMN recibos_items.comprobante_imputado_id IS 'ID del comprobante al que corresponde este item del recibo';

-- Índice
CREATE INDEX idx_recibos_items_comprobante_imputado_id ON recibos_items(comprobante_imputado_id) WHERE comprobante_imputado_id IS NOT NULL;
```

## 5. Foreign Keys (Opcional - según modelo de datos)

```sql
-- Si las tablas de terceros y usuarios existen como entidades separadas
-- ALTER TABLE cobros ADD CONSTRAINT fk_cobros_vendedor_id FOREIGN KEY (vendedor_id) REFERENCES terceros(id);
-- ALTER TABLE cobros ADD CONSTRAINT fk_cobros_cobrador_id FOREIGN KEY (cobrador_id) REFERENCES terceros(id);
-- ALTER TABLE cobros ADD CONSTRAINT fk_cobros_usuario_cajero_id FOREIGN KEY (usuario_cajero_id) REFERENCES usuarios(id);

-- ALTER TABLE recibos ADD CONSTRAINT fk_recibos_vendedor_id FOREIGN KEY (vendedor_id) REFERENCES terceros(id);
-- ALTER TABLE recibos ADD CONSTRAINT fk_recibos_cobrador_id FOREIGN KEY (cobrador_id) REFERENCES terceros(id);
-- ALTER TABLE recibos ADD CONSTRAINT fk_recibos_usuario_cajero_id FOREIGN KEY (usuario_cajero_id) REFERENCES usuarios(id);

-- ALTER TABLE recibos_items ADD CONSTRAINT fk_recibos_items_comprobante_imputado_id FOREIGN KEY (comprobante_imputado_id) REFERENCES comprobantes(id);
```

## 6. Actualizar datos existentes (si es necesario)

```sql
-- Asignar tipo de cobro por defecto a registros existentes
UPDATE cobros
SET tipo_cobro = 0  -- Administrativo por defecto
WHERE tipo_cobro IS NULL OR tipo_cobro = 0;

-- Inicializar copias impresas en 0 para recibos existentes
UPDATE recibos
SET copias_impresas = 0
WHERE copias_impresas IS NULL;
```

## 7. Validaciones y verificaciones

```sql
-- Verificar que los campos se crearon correctamente
SELECT 
    column_name, 
    data_type, 
    is_nullable,
    column_default
FROM information_schema.columns
WHERE table_name IN ('cobros', 'cobros_medios', 'recibos', 'recibos_items')
    AND column_name IN (
        'vendedor_id', 'cobrador_id', 'zona_comercial_id', 'usuario_cajero_id',
        'ventanilla_turno', 'tipo_cobro', 'observacion_interna',
        'tercero_cuit', 'tercero_condicion_iva', 'tercero_domicilio_snapshot',
        'banco_origen', 'banco_destino', 'numero_operacion', 'fecha_acreditacion',
        'terminal_pos', 'numero_cupon', 'numero_lote', 'codigo_autorizacion',
        'cantidad_cuotas', 'plan_cuotas', 'fecha_acreditacion_estimada',
        'tercero_domicilio', 'leyenda_fiscal', 'formato_impresion',
        'copias_impresas', 'fecha_impresion', 'comprobante_imputado_id'
    )
ORDER BY table_name, column_name;

-- Verificar índices creados
SELECT
    schemaname,
    tablename,
    indexname,
    indexdef
FROM pg_indexes
WHERE tablename IN ('cobros', 'cobros_medios', 'recibos', 'recibos_items')
    AND indexname LIKE 'idx_%'
ORDER BY tablename, indexname;
```

## 8. Rollback (si es necesario)

```sql
-- Eliminar foreign keys primero (si se crearon)
-- ALTER TABLE cobros DROP CONSTRAINT IF EXISTS fk_cobros_vendedor_id;
-- ALTER TABLE cobros DROP CONSTRAINT IF EXISTS fk_cobros_cobrador_id;
-- ALTER TABLE cobros DROP CONSTRAINT IF EXISTS fk_cobros_usuario_cajero_id;
-- ALTER TABLE recibos DROP CONSTRAINT IF EXISTS fk_recibos_vendedor_id;
-- ALTER TABLE recibos DROP CONSTRAINT IF EXISTS fk_recibos_cobrador_id;
-- ALTER TABLE recibos DROP CONSTRAINT IF EXISTS fk_recibos_usuario_cajero_id;
-- ALTER TABLE recibos_items DROP CONSTRAINT IF EXISTS fk_recibos_items_comprobante_imputado_id;

-- Eliminar índices
DROP INDEX IF EXISTS idx_cobros_vendedor_id;
DROP INDEX IF EXISTS idx_cobros_cobrador_id;
DROP INDEX IF EXISTS idx_cobros_zona_comercial_id;
DROP INDEX IF EXISTS idx_cobros_usuario_cajero_id;
DROP INDEX IF EXISTS idx_cobros_tipo_cobro;
DROP INDEX IF EXISTS idx_cobros_fecha_tipo;
DROP INDEX IF EXISTS idx_cobros_medios_numero_operacion;
DROP INDEX IF EXISTS idx_cobros_medios_numero_cupon;
DROP INDEX IF EXISTS idx_cobros_medios_numero_lote;
DROP INDEX IF EXISTS idx_cobros_medios_fecha_acreditacion;
DROP INDEX IF EXISTS idx_recibos_vendedor_id;
DROP INDEX IF EXISTS idx_recibos_cobrador_id;
DROP INDEX IF EXISTS idx_recibos_zona_comercial_id;
DROP INDEX IF EXISTS idx_recibos_usuario_cajero_id;
DROP INDEX IF EXISTS idx_recibos_fecha_impresion;
DROP INDEX IF EXISTS idx_recibos_items_comprobante_imputado_id;

-- Eliminar columnas de cobros
ALTER TABLE cobros DROP COLUMN IF EXISTS vendedor_id;
ALTER TABLE cobros DROP COLUMN IF EXISTS cobrador_id;
ALTER TABLE cobros DROP COLUMN IF EXISTS zona_comercial_id;
ALTER TABLE cobros DROP COLUMN IF EXISTS usuario_cajero_id;
ALTER TABLE cobros DROP COLUMN IF EXISTS ventanilla_turno;
ALTER TABLE cobros DROP COLUMN IF EXISTS tipo_cobro;
ALTER TABLE cobros DROP COLUMN IF EXISTS observacion_interna;
ALTER TABLE cobros DROP COLUMN IF EXISTS tercero_cuit;
ALTER TABLE cobros DROP COLUMN IF EXISTS tercero_condicion_iva;
ALTER TABLE cobros DROP COLUMN IF EXISTS tercero_domicilio_snapshot;

-- Eliminar columnas de cobros_medios
ALTER TABLE cobros_medios DROP COLUMN IF EXISTS banco_origen;
ALTER TABLE cobros_medios DROP COLUMN IF EXISTS banco_destino;
ALTER TABLE cobros_medios DROP COLUMN IF EXISTS numero_operacion;
ALTER TABLE cobros_medios DROP COLUMN IF EXISTS fecha_acreditacion;
ALTER TABLE cobros_medios DROP COLUMN IF EXISTS terminal_pos;
ALTER TABLE cobros_medios DROP COLUMN IF EXISTS numero_cupon;
ALTER TABLE cobros_medios DROP COLUMN IF EXISTS numero_lote;
ALTER TABLE cobros_medios DROP COLUMN IF EXISTS codigo_autorizacion;
ALTER TABLE cobros_medios DROP COLUMN IF EXISTS cantidad_cuotas;
ALTER TABLE cobros_medios DROP COLUMN IF EXISTS plan_cuotas;
ALTER TABLE cobros_medios DROP COLUMN IF EXISTS fecha_acreditacion_estimada;

-- Eliminar columnas de recibos
ALTER TABLE recibos DROP COLUMN IF EXISTS vendedor_id;
ALTER TABLE recibos DROP COLUMN IF EXISTS cobrador_id;
ALTER TABLE recibos DROP COLUMN IF EXISTS zona_comercial_id;
ALTER TABLE recibos DROP COLUMN IF EXISTS usuario_cajero_id;
ALTER TABLE recibos DROP COLUMN IF EXISTS tercero_cuit;
ALTER TABLE recibos DROP COLUMN IF EXISTS tercero_condicion_iva;
ALTER TABLE recibos DROP COLUMN IF EXISTS tercero_domicilio;
ALTER TABLE recibos DROP COLUMN IF EXISTS leyenda_fiscal;
ALTER TABLE recibos DROP COLUMN IF EXISTS formato_impresion;
ALTER TABLE recibos DROP COLUMN IF EXISTS copias_impresas;
ALTER TABLE recibos DROP COLUMN IF EXISTS fecha_impresion;

-- Eliminar columnas de recibos_items
ALTER TABLE recibos_items DROP COLUMN IF EXISTS comprobante_imputado_id;
```

## 9. Notas de implementación

1. **Ejecutar en entorno de desarrollo primero** y validar que no rompe funcionalidad existente
2. **Backup de base de datos** antes de ejecutar en producción
3. Los campos agregados son **nullable** para no afectar registros existentes
4. El tipo de cobro por defecto es `0` (Administrativo)
5. Las copias impresas se inicializan en `0`
6. Los índices mejoran performance de búsquedas por campos comerciales y operativos
7. Las foreign keys son opcionales y dependen del modelo de datos completo

## 10. Próximos pasos

Una vez aplicada esta migración:
1. Actualizar Entity Framework Configuration (`CobroConfiguration`, `ReciboConfiguration`)
2. Actualizar Commands para aceptar los nuevos parámetros
3. Actualizar Query Handlers para exponer los nuevos campos en DTOs
4. Crear tests unitarios para validar nuevos campos
5. Actualizar documentación de API (Swagger)
