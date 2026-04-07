# Script de Migración: Cheques - Paridad Funcional con ZuluApp

**Fecha:** 2024-03-20  
**Autor:** Sistema  
**Objetivo:** Agregar campos faltantes a la tabla `cheques` para lograr paridad funcional con ZuluApp

---

## Descripción

Este script agrega las columnas necesarias para soportar toda la funcionalidad de cheques presente en ZuluApp:
- Información bancaria detallada (código sucursal, código postal)
- Titular del cheque
- Tipo de cheque (Propio/Tercero)
- Características (A la orden, Cruzado)
- Referencias (Chequera, Comprobante origen)
- Concepto de rechazo detallado

---

## Script SQL

```sql
-- =========================================
-- AGREGAR COLUMNAS A CHEQUES
-- =========================================

-- Campos bancarios adicionales
ALTER TABLE cheques
ADD COLUMN codigo_sucursal_bancaria VARCHAR(20),
ADD COLUMN codigo_postal VARCHAR(20),
ADD COLUMN titular VARCHAR(200);

-- Tipo y características del cheque
ALTER TABLE cheques
ADD COLUMN tipo VARCHAR(20) NOT NULL DEFAULT 'TERCERO'
    CHECK (tipo IN ('TERCERO', 'PROPIO')),
ADD COLUMN es_a_la_orden BOOLEAN NOT NULL DEFAULT true,
ADD COLUMN es_cruzado BOOLEAN NOT NULL DEFAULT false;

-- Referencias
ALTER TABLE cheques
ADD COLUMN chequera_id BIGINT,
ADD COLUMN comprobante_origen_id BIGINT;

-- Concepto de rechazo
ALTER TABLE cheques
ADD COLUMN concepto_rechazo VARCHAR(500);

-- =========================================
-- CREAR ÍNDICES PARA OPTIMIZAR CONSULTAS
-- =========================================

CREATE INDEX idx_cheques_tipo ON cheques(tipo);
CREATE INDEX idx_cheques_chequera_id ON cheques(chequera_id);
CREATE INDEX idx_cheques_comprobante_origen_id ON cheques(comprobante_origen_id);
CREATE INDEX idx_cheques_nro_cheque ON cheques(nro_cheque);
CREATE INDEX idx_cheques_titular ON cheques(titular);
CREATE INDEX idx_cheques_es_a_la_orden ON cheques(es_a_la_orden);

-- =========================================
-- AGREGAR FOREIGN KEYS SI LAS TABLAS EXISTEN
-- =========================================

-- FK a chequeras (si existe la tabla)
DO $$
BEGIN
    IF EXISTS (SELECT 1 FROM information_schema.tables 
               WHERE table_name = 'chequeras') THEN
        ALTER TABLE cheques
        ADD CONSTRAINT fk_cheques_chequera
        FOREIGN KEY (chequera_id) REFERENCES chequeras(id)
        ON DELETE SET NULL;
    END IF;
END $$;

-- FK a comprobantes (si existe la tabla)
DO $$
BEGIN
    IF EXISTS (SELECT 1 FROM information_schema.tables 
               WHERE table_name = 'comprobantes') THEN
        ALTER TABLE cheques
        ADD CONSTRAINT fk_cheques_comprobante_origen
        FOREIGN KEY (comprobante_origen_id) REFERENCES comprobantes(id)
        ON DELETE SET NULL;
    END IF;
END $$;

-- =========================================
-- ACTUALIZAR ESTADOS PARA INCLUIR NUEVOS
-- =========================================

-- Eliminar restricción anterior si existe
ALTER TABLE cheques DROP CONSTRAINT IF EXISTS chk_cheques_estado;

-- Agregar restricción con estados nuevos
ALTER TABLE cheques
ADD CONSTRAINT chk_cheques_estado
CHECK (estado IN (
    'CARTERA', 
    'DEPOSITADO', 
    'ACREDITADO', 
    'RECHAZADO', 
    'ENTREGADO',
    'ANULADO',      -- NUEVO
    'ENDOSADO',     -- NUEVO
    'ENTRANSITO'    -- NUEVO
));

-- =========================================
-- MIGRAR DATOS EXISTENTES
-- =========================================

-- Establecer titular basado en tercero_id (si es NULL)
UPDATE cheques c
SET titular = t.razon_social
FROM terceros t
WHERE c.tercero_id = t.id
  AND c.titular IS NULL
  AND c.tipo = 'TERCERO';

-- Comentario sobre chequeras:
-- Si existen cheques propios antiguos sin chequera_id,
-- deberán ser actualizados manualmente o mediante lógica de negocio

-- =========================================
-- VERIFICACIÓN
-- =========================================

-- Contar cheques por tipo
SELECT tipo, COUNT(*) as cantidad
FROM cheques
GROUP BY tipo;

-- Contar cheques "A la orden"
SELECT es_a_la_orden, COUNT(*) as cantidad
FROM cheques
GROUP BY es_a_la_orden;

-- Verificar cheques sin titular (deberían ser solo propios)
SELECT tipo, COUNT(*) as cantidad
FROM cheques
WHERE titular IS NULL
GROUP BY tipo;

-- Verificar integridad referencial
SELECT 
    COUNT(*) as cheques_con_chequera,
    COUNT(DISTINCT chequera_id) as chequeras_referenciadas
FROM cheques
WHERE chequera_id IS NOT NULL;

SELECT 
    COUNT(*) as cheques_con_comprobante,
    COUNT(DISTINCT comprobante_origen_id) as comprobantes_referenciados
FROM cheques
WHERE comprobante_origen_id IS NOT NULL;
```

---

## Rollback (Si es necesario)

```sql
-- =========================================
-- ROLLBACK - ELIMINAR CAMBIOS
-- =========================================

-- Eliminar foreign keys
ALTER TABLE cheques DROP CONSTRAINT IF EXISTS fk_cheques_chequera;
ALTER TABLE cheques DROP CONSTRAINT IF EXISTS fk_cheques_comprobante_origen;

-- Eliminar índices
DROP INDEX IF EXISTS idx_cheques_tipo;
DROP INDEX IF EXISTS idx_cheques_chequera_id;
DROP INDEX IF EXISTS idx_cheques_comprobante_origen_id;
DROP INDEX IF EXISTS idx_cheques_nro_cheque;
DROP INDEX IF EXISTS idx_cheques_titular;
DROP INDEX IF EXISTS idx_cheques_es_a_la_orden;

-- Eliminar columnas
ALTER TABLE cheques
DROP COLUMN IF EXISTS codigo_sucursal_bancaria,
DROP COLUMN IF EXISTS codigo_postal,
DROP COLUMN IF EXISTS titular,
DROP COLUMN IF EXISTS tipo,
DROP COLUMN IF EXISTS es_a_la_orden,
DROP COLUMN IF EXISTS es_cruzado,
DROP COLUMN IF EXISTS chequera_id,
DROP COLUMN IF EXISTS comprobante_origen_id,
DROP COLUMN IF EXISTS concepto_rechazo;

-- Restaurar restricción de estados original
ALTER TABLE cheques DROP CONSTRAINT IF EXISTS chk_cheques_estado;
ALTER TABLE cheques
ADD CONSTRAINT chk_cheques_estado
CHECK (estado IN (
    'CARTERA', 
    'DEPOSITADO', 
    'ACREDITADO', 
    'RECHAZADO', 
    'ENTREGADO'
));
```

---

## Notas de Implementación

### Campos Obligatorios vs Opcionales
- `tipo`: **Obligatorio** - Default 'TERCERO' para compatibilidad
- `es_a_la_orden`: **Obligatorio** - Default true (mayoría de casos)
- `es_cruzado`: **Obligatorio** - Default false
- `titular`: **Opcional** - Requerido en lógica de negocio para terceros
- `chequera_id`: **Opcional** - Solo para cheques propios
- `comprobante_origen_id`: **Opcional** - Se completa en operaciones

### Validaciones de Negocio
Las siguientes validaciones se hacen en el dominio (Cheque.cs):
1. Cheques propios **deben** tener `chequera_id`
2. Cheques de terceros **deben** tener `titular`
3. Solo se pueden endosar cheques `es_a_la_orden = true`
4. Solo se pueden anular cheques propios

### Migración de Datos Históricos
- Cheques existentes: Se asume tipo 'TERCERO' por defecto
- El `titular` se puede derivar de `terceros.razon_social`
- Cheques propios antiguos necesitarán revisión manual

### Impacto en Aplicación
- **DTOs**: Actualizar todos los DTOs de cheques
- **Commands**: Actualizar CreateChequeCommand
- **Queries**: Agregar filtros por nuevo campos
- **Frontend**: Actualizar formularios y vistas

---

## Testing

Después de ejecutar este script, verificar:

1. ✅ Todos los cheques tienen `tipo` asignado
2. ✅ Cheques de terceros sin `titular` son 0 o están justificados
3. ✅ Índices creados correctamente
4. ✅ Foreign keys funcionan si las tablas existen
5. ✅ Constraint de estados incluye ANULADO, ENDOSADO, ENTRANSITO

```sql
-- Query de verificación completa
SELECT 
    COUNT(*) as total_cheques,
    COUNT(CASE WHEN tipo = 'TERCERO' THEN 1 END) as terceros,
    COUNT(CASE WHEN tipo = 'PROPIO' THEN 1 END) as propios,
    COUNT(CASE WHEN titular IS NOT NULL THEN 1 END) as con_titular,
    COUNT(CASE WHEN es_a_la_orden = true THEN 1 END) as a_la_orden,
    COUNT(CASE WHEN es_cruzado = true THEN 1 END) as cruzados,
    COUNT(CASE WHEN chequera_id IS NOT NULL THEN 1 END) as con_chequera,
    COUNT(CASE WHEN comprobante_origen_id IS NOT NULL THEN 1 END) as con_comprobante_origen
FROM cheques;
```

---

## Referencias

- **Análisis Completo:** `docs/cheques-paridad-zuluapp-analisis.md`
- **Entidad Actualizada:** `src/ZuluIA_Back.Domain/Entities/Finanzas/Cheque.cs`
- **Configuración EF:** `src/ZuluIA_Back.Infrastructure/Persistence/Configurations/ChequeConfiguration.cs`
