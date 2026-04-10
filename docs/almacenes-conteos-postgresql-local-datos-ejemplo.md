BEGIN;

-- Datos ejemplo para bootstrap manual de la agenda de conteos cíclicos de Almacenes.
-- Base analizada: C:\Users\exequ\OneDrive\Desktop\DBLocal.sql
-- Objetivo: sembrar el mismo set inicial que usa el backend como referencia legacy,
-- sin duplicar registros si ya existen por depósito y zona.

INSERT INTO public.conteos_ciclicos (
    deposito,
    zona,
    frecuencia,
    proximo_conteo,
    estado,
    divergencia_pct,
    responsable,
    observacion,
    next_step,
    execution_note
)
SELECT
    'Central',
    'A1-PICK',
    'Semanal',
    DATE '2026-03-26',
    'programado',
    1.80,
    'Equipo Picking AM',
    'Conteo corto de alta rotación previo al cierre semanal.',
    'Preparar equipo y validar cobertura antes del próximo conteo.',
    ''
WHERE NOT EXISTS (
    SELECT 1
    FROM public.conteos_ciclicos
    WHERE upper(deposito) = 'CENTRAL'
      AND upper(zona) = 'A1-PICK'
);

INSERT INTO public.conteos_ciclicos (
    deposito,
    zona,
    frecuencia,
    proximo_conteo,
    estado,
    divergencia_pct,
    responsable,
    observacion,
    next_step,
    execution_note
)
SELECT
    'Planta Norte',
    'B1-MP',
    'Quincenal',
    DATE '2026-03-24',
    'en-ejecucion',
    3.60,
    'Control de planta',
    'Revisión focalizada sobre materia prima crítica y merma visible.',
    'Preparar equipo y validar cobertura antes del próximo conteo.',
    ''
WHERE NOT EXISTS (
    SELECT 1
    FROM public.conteos_ciclicos
    WHERE upper(deposito) = 'PLANTA NORTE'
      AND upper(zona) = 'B1-MP'
);

INSERT INTO public.conteos_ciclicos (
    deposito,
    zona,
    frecuencia,
    proximo_conteo,
    estado,
    divergencia_pct,
    responsable,
    observacion,
    next_step,
    execution_note
)
SELECT
    'Obra Delta',
    'C1-OBR',
    'Mensual',
    DATE '2026-03-29',
    'observado',
    6.20,
    'Capataz de obra',
    'Requiere conciliación entre remitos consumidos y stock remanente.',
    'Conciliar diferencias y definir ajuste manual o re-conteo.',
    ''
WHERE NOT EXISTS (
    SELECT 1
    FROM public.conteos_ciclicos
    WHERE upper(deposito) = 'OBRA DELTA'
      AND upper(zona) = 'C1-OBR'
);

COMMIT;
