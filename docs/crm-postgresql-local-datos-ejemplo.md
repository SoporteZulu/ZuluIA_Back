BEGIN;

DO $$
DECLARE
    v_usuario_1 bigint;
    v_usuario_2 bigint;
    v_sucursal_1 bigint;
    v_tercero_1 bigint;
    v_tercero_2 bigint;

    v_tipo_min bigint;
    v_tipo_full bigint;
    v_motivo_min bigint;
    v_motivo_full bigint;
    v_interes_min bigint;
    v_interes_full bigint;
    v_tipo_relacion_min bigint;
    v_tipo_relacion_full bigint;
    v_segmento_min bigint;
    v_segmento_full bigint;

    v_contacto_min bigint;
    v_contacto_full bigint;
    v_oportunidad_min bigint;
    v_oportunidad_full bigint;
    v_campana_min bigint;
    v_campana_full bigint;
BEGIN
    SELECT id INTO v_usuario_1
    FROM "usuarios"
    WHERE "activo" = TRUE
    ORDER BY id
    LIMIT 1;

    SELECT id INTO v_usuario_2
    FROM "usuarios"
    WHERE "activo" = TRUE
      AND id <> COALESCE(v_usuario_1, -1)
    ORDER BY id
    LIMIT 1;

    IF v_usuario_1 IS NULL THEN
        RAISE EXCEPTION 'CRM seed: no existe un usuario activo en la tabla usuarios.';
    END IF;

    IF v_usuario_2 IS NULL THEN
        v_usuario_2 := v_usuario_1;
    END IF;

    SELECT id INTO v_sucursal_1
    FROM "sucursales"
    WHERE "activa" = TRUE
    ORDER BY "casa_matriz" DESC, id
    LIMIT 1;

    IF v_sucursal_1 IS NULL THEN
        RAISE EXCEPTION 'CRM seed: no existe una sucursal activa en la tabla sucursales.';
    END IF;

    SELECT id INTO v_tercero_1
    FROM "terceros"
    ORDER BY id
    LIMIT 1;

    SELECT id INTO v_tercero_2
    FROM "terceros"
    WHERE id <> COALESCE(v_tercero_1, -1)
    ORDER BY id
    LIMIT 1;

    IF v_tercero_1 IS NULL OR v_tercero_2 IS NULL THEN
        RAISE EXCEPTION 'CRM seed: se requieren al menos 2 registros en la tabla terceros.';
    END IF;

    -- =============================================================
    -- 1) EJEMPLOS MINIMOS (solo obligatorio)
    -- =============================================================

    INSERT INTO "CRMTIPOCOMUNICADOS" (
        "codigo",
        "descripcion"
    )
    VALUES (
        'ZZ_CRM_TIPO_MIN',
        'Tipo comunicado minimo CRM'
    )
    ON CONFLICT ("codigo") DO NOTHING
    RETURNING "id" INTO v_tipo_min;

    IF v_tipo_min IS NULL THEN
        SELECT "id" INTO v_tipo_min
        FROM "CRMTIPOCOMUNICADOS"
        WHERE "codigo" = 'ZZ_CRM_TIPO_MIN';
    END IF;

    INSERT INTO "CRMMOTIVOS" (
        "codigo",
        "descripcion"
    )
    VALUES (
        'ZZ_CRM_MOTIVO_MIN',
        'Motivo minimo CRM'
    )
    ON CONFLICT ("codigo") DO NOTHING
    RETURNING "id" INTO v_motivo_min;

    IF v_motivo_min IS NULL THEN
        SELECT "id" INTO v_motivo_min
        FROM "CRMMOTIVOS"
        WHERE "codigo" = 'ZZ_CRM_MOTIVO_MIN';
    END IF;

    INSERT INTO "CRMINTERESES" (
        "codigo",
        "descripcion"
    )
    VALUES (
        'ZZ_CRM_INTERES_MIN',
        'Interes minimo CRM'
    )
    ON CONFLICT ("codigo") DO NOTHING
    RETURNING "id" INTO v_interes_min;

    IF v_interes_min IS NULL THEN
        SELECT "id" INTO v_interes_min
        FROM "CRMINTERESES"
        WHERE "codigo" = 'ZZ_CRM_INTERES_MIN';
    END IF;

    INSERT INTO "tipos_relaciones_contacto" (
        "codigo",
        "descripcion"
    )
    VALUES (
        'ZZ_CRM_REL_MIN',
        'Relacion minima CRM'
    )
    ON CONFLICT ("codigo") DO NOTHING
    RETURNING "id" INTO v_tipo_relacion_min;

    IF v_tipo_relacion_min IS NULL THEN
        SELECT "id" INTO v_tipo_relacion_min
        FROM "tipos_relaciones_contacto"
        WHERE "codigo" = 'ZZ_CRM_REL_MIN';
    END IF;

    INSERT INTO "CRMSEGMENTOS" (
        "nombre",
        "tipo_segmento"
    )
    VALUES (
        'ZZ CRM SEGMENTO MINIMO',
        'estatico'
    )
    ON CONFLICT ("nombre") DO NOTHING
    RETURNING "id" INTO v_segmento_min;

    IF v_segmento_min IS NULL THEN
        SELECT "id" INTO v_segmento_min
        FROM "CRMSEGMENTOS"
        WHERE "nombre" = 'ZZ CRM SEGMENTO MINIMO';
    END IF;

    INSERT INTO "CRMUSUARIOS" (
        "usuario_id",
        "rol"
    )
    VALUES (
        v_usuario_1,
        'comercial'
    )
    ON CONFLICT ("usuario_id") DO NOTHING;

    INSERT INTO "CRMCLIENTES" (
        "tercero_id",
        "tipo_cliente",
        "segmento",
        "pais",
        "origen_cliente",
        "estado_relacion"
    )
    VALUES (
        v_tercero_1,
        'activo',
        'pyme',
        'Paraguay',
        'web',
        'nuevo'
    )
    ON CONFLICT ("tercero_id") DO NOTHING;

    INSERT INTO "CONTACTOS" (
        "id_persona",
        "id_contacto",
        "trel_id"
    )
    SELECT
        v_tercero_1,
        v_tercero_2,
        v_tipo_relacion_min
    WHERE NOT EXISTS (
        SELECT 1
        FROM "CONTACTOS"
        WHERE "id_persona" = v_tercero_1
          AND "id_contacto" = v_tercero_2
    );

    SELECT "id" INTO v_contacto_min
    FROM "CRMCONTACTOS"
    WHERE "cliente_id" = v_tercero_1
      AND "nombre" = 'ZZ'
      AND "apellido" = 'CONTACTO MINIMO'
    LIMIT 1;

    IF v_contacto_min IS NULL THEN
        INSERT INTO "CRMCONTACTOS" (
            "cliente_id",
            "nombre",
            "apellido",
            "canal_preferido",
            "estado_contacto"
        )
        VALUES (
            v_tercero_1,
            'ZZ',
            'CONTACTO MINIMO',
            'email',
            'activo'
        )
        RETURNING "id" INTO v_contacto_min;
    END IF;

    SELECT "id" INTO v_oportunidad_min
    FROM "CRMOPORTUNIDADES"
    WHERE "cliente_id" = v_tercero_1
      AND "titulo" = 'ZZ CRM OPORTUNIDAD MINIMA'
    LIMIT 1;

    IF v_oportunidad_min IS NULL THEN
        INSERT INTO "CRMOPORTUNIDADES" (
            "cliente_id",
            "titulo",
            "etapa",
            "probabilidad",
            "monto_estimado",
            "moneda",
            "fecha_apertura",
            "origen"
        )
        VALUES (
            v_tercero_1,
            'ZZ CRM OPORTUNIDAD MINIMA',
            'lead',
            25,
            1000.00,
            'USD',
            CURRENT_DATE,
            'web'
        )
        RETURNING "id" INTO v_oportunidad_min;
    END IF;

    INSERT INTO "CRMINTERACCIONES" (
        "cliente_id",
        "tipo_interaccion",
        "canal",
        "fecha_hora",
        "usuario_responsable_id",
        "resultado"
    )
    SELECT
        v_tercero_1,
        'llamada',
        'telefono',
        CURRENT_TIMESTAMP,
        v_usuario_1,
        'exitosa'
    WHERE NOT EXISTS (
        SELECT 1
        FROM "CRMINTERACCIONES"
        WHERE "cliente_id" = v_tercero_1
          AND "tipo_interaccion" = 'llamada'
          AND "resultado" = 'exitosa'
          AND "descripcion" IS NULL
    );

    INSERT INTO "CRMTAREAS" (
        "cliente_id",
        "asignado_a_id",
        "titulo",
        "tipo_tarea",
        "fecha_vencimiento",
        "prioridad",
        "estado"
    )
    SELECT
        v_tercero_1,
        v_usuario_1,
        'ZZ CRM TAREA MINIMA',
        'seguimiento',
        CURRENT_DATE + 3,
        'media',
        'pendiente'
    WHERE NOT EXISTS (
        SELECT 1
        FROM "CRMTAREAS"
        WHERE "titulo" = 'ZZ CRM TAREA MINIMA'
          AND "cliente_id" = v_tercero_1
    );

    SELECT "id" INTO v_campana_min
    FROM "CRMCAMPANAS"
    WHERE "nombre" = 'ZZ CRM CAMPANA MINIMA'
      AND "sucursal_id" = v_sucursal_1
    LIMIT 1;

    IF v_campana_min IS NULL THEN
        INSERT INTO "CRMCAMPANAS" (
            "sucursal_id",
            "nombre",
            "fecha_inicio",
            "fecha_fin"
        )
        VALUES (
            v_sucursal_1,
            'ZZ CRM CAMPANA MINIMA',
            CURRENT_DATE,
            CURRENT_DATE + 10
        )
        RETURNING "id" INTO v_campana_min;
    END IF;

    INSERT INTO "CRMSEGUIMIENTOS" (
        "sucursal_id",
        "tercero_id",
        "fecha",
        "descripcion"
    )
    SELECT
        v_sucursal_1,
        v_tercero_1,
        CURRENT_DATE,
        'ZZ CRM SEGUIMIENTO MINIMO'
    WHERE NOT EXISTS (
        SELECT 1
        FROM "CRMSEGUIMIENTOS"
        WHERE "tercero_id" = v_tercero_1
          AND "descripcion" = 'ZZ CRM SEGUIMIENTO MINIMO'
    );

    INSERT INTO "CRMCOMUNICADOS" (
        "sucursal_id",
        "tercero_id",
        "fecha",
        "asunto"
    )
    SELECT
        v_sucursal_1,
        v_tercero_1,
        CURRENT_DATE,
        'ZZ CRM COMUNICADO MINIMO'
    WHERE NOT EXISTS (
        SELECT 1
        FROM "CRMCOMUNICADOS"
        WHERE "tercero_id" = v_tercero_1
          AND "asunto" = 'ZZ CRM COMUNICADO MINIMO'
    );

    INSERT INTO "CRMSEGMENTOS_MIEMBROS" (
        "segmento_id",
        "cliente_id"
    )
    VALUES (
        v_segmento_min,
        v_tercero_1
    )
    ON CONFLICT ("segmento_id", "cliente_id") DO NOTHING;

    -- =============================================================
    -- 2) EJEMPLOS COMPLETOS (todas las columnas utiles)
    -- =============================================================

    INSERT INTO "CRMTIPOCOMUNICADOS" (
        "codigo",
        "descripcion",
        "activo",
        "created_at",
        "updated_at",
        "deleted_at",
        "created_by",
        "updated_by"
    )
    VALUES (
        'ZZ_CRM_TIPO_FULL',
        'Tipo comunicado completo CRM',
        TRUE,
        CURRENT_TIMESTAMP,
        CURRENT_TIMESTAMP,
        NULL,
        v_usuario_1,
        v_usuario_2
    )
    ON CONFLICT ("codigo") DO NOTHING
    RETURNING "id" INTO v_tipo_full;

    IF v_tipo_full IS NULL THEN
        SELECT "id" INTO v_tipo_full
        FROM "CRMTIPOCOMUNICADOS"
        WHERE "codigo" = 'ZZ_CRM_TIPO_FULL';
    END IF;

    INSERT INTO "CRMMOTIVOS" (
        "codigo",
        "descripcion",
        "activo",
        "created_at",
        "updated_at",
        "deleted_at",
        "created_by",
        "updated_by"
    )
    VALUES (
        'ZZ_CRM_MOTIVO_FULL',
        'Motivo completo CRM',
        TRUE,
        CURRENT_TIMESTAMP,
        CURRENT_TIMESTAMP,
        NULL,
        v_usuario_1,
        v_usuario_2
    )
    ON CONFLICT ("codigo") DO NOTHING
    RETURNING "id" INTO v_motivo_full;

    IF v_motivo_full IS NULL THEN
        SELECT "id" INTO v_motivo_full
        FROM "CRMMOTIVOS"
        WHERE "codigo" = 'ZZ_CRM_MOTIVO_FULL';
    END IF;

    INSERT INTO "CRMINTERESES" (
        "codigo",
        "descripcion",
        "activo",
        "created_at",
        "updated_at",
        "deleted_at",
        "created_by",
        "updated_by"
    )
    VALUES (
        'ZZ_CRM_INTERES_FULL',
        'Interes completo CRM',
        TRUE,
        CURRENT_TIMESTAMP,
        CURRENT_TIMESTAMP,
        NULL,
        v_usuario_1,
        v_usuario_2
    )
    ON CONFLICT ("codigo") DO NOTHING
    RETURNING "id" INTO v_interes_full;

    IF v_interes_full IS NULL THEN
        SELECT "id" INTO v_interes_full
        FROM "CRMINTERESES"
        WHERE "codigo" = 'ZZ_CRM_INTERES_FULL';
    END IF;

    INSERT INTO "tipos_relaciones_contacto" (
        "codigo",
        "descripcion",
        "activo",
        "created_at",
        "updated_at",
        "deleted_at",
        "created_by",
        "updated_by"
    )
    VALUES (
        'ZZ_CRM_REL_FULL',
        'Relacion completa CRM',
        TRUE,
        CURRENT_TIMESTAMP,
        CURRENT_TIMESTAMP,
        NULL,
        v_usuario_1,
        v_usuario_2
    )
    ON CONFLICT ("codigo") DO NOTHING
    RETURNING "id" INTO v_tipo_relacion_full;

    IF v_tipo_relacion_full IS NULL THEN
        SELECT "id" INTO v_tipo_relacion_full
        FROM "tipos_relaciones_contacto"
        WHERE "codigo" = 'ZZ_CRM_REL_FULL';
    END IF;

    INSERT INTO "CRMSEGMENTOS" (
        "nombre",
        "descripcion",
        "criterios_json",
        "tipo_segmento",
        "activo",
        "created_at",
        "updated_at",
        "deleted_at",
        "created_by",
        "updated_by"
    )
    VALUES (
        'ZZ CRM SEGMENTO COMPLETO',
        'Segmento completo de ejemplo para CRM',
        '[{"Campo":"segmento","Operador":"igual","Valor":"corporativo"}]'::jsonb,
        'dinamico',
        TRUE,
        CURRENT_TIMESTAMP,
        CURRENT_TIMESTAMP,
        NULL,
        v_usuario_1,
        v_usuario_2
    )
    ON CONFLICT ("nombre") DO NOTHING
    RETURNING "id" INTO v_segmento_full;

    IF v_segmento_full IS NULL THEN
        SELECT "id" INTO v_segmento_full
        FROM "CRMSEGMENTOS"
        WHERE "nombre" = 'ZZ CRM SEGMENTO COMPLETO';
    END IF;

    INSERT INTO "CRMUSUARIOS" (
        "usuario_id",
        "rol",
        "avatar",
        "activo",
        "created_at",
        "updated_at",
        "deleted_at",
        "created_by",
        "updated_by"
    )
    VALUES (
        v_usuario_2,
        'supervisor',
        'https://example.com/avatar-crm-full.png',
        TRUE,
        CURRENT_TIMESTAMP,
        CURRENT_TIMESTAMP,
        NULL,
        v_usuario_1,
        v_usuario_1
    )
    ON CONFLICT ("usuario_id") DO NOTHING;

    INSERT INTO "CRMCLIENTES" (
        "tercero_id",
        "tipo_cliente",
        "segmento",
        "industria",
        "pais",
        "provincia",
        "ciudad",
        "direccion",
        "origen_cliente",
        "estado_relacion",
        "responsable_id",
        "notas_generales",
        "activo",
        "created_at",
        "updated_at",
        "deleted_at",
        "created_by",
        "updated_by"
    )
    VALUES (
        v_tercero_2,
        'activo',
        'corporativo',
        'Tecnologia',
        'Paraguay',
        'Central',
        'San Lorenzo',
        'Av. CRM 1234',
        'referido',
        'fidelizado',
        v_usuario_2,
        'Cliente completo CRM con todos los campos utiles cargados.',
        TRUE,
        CURRENT_TIMESTAMP,
        CURRENT_TIMESTAMP,
        NULL,
        v_usuario_1,
        v_usuario_2
    )
    ON CONFLICT ("tercero_id") DO NOTHING;

    INSERT INTO "CONTACTOS" (
        "id_persona",
        "id_contacto",
        "trel_id"
    )
    SELECT
        v_tercero_2,
        v_tercero_1,
        v_tipo_relacion_full
    WHERE NOT EXISTS (
        SELECT 1
        FROM "CONTACTOS"
        WHERE "id_persona" = v_tercero_2
          AND "id_contacto" = v_tercero_1
    );

    SELECT "id" INTO v_contacto_full
    FROM "CRMCONTACTOS"
    WHERE "cliente_id" = v_tercero_2
      AND "nombre" = 'ZZ'
      AND "apellido" = 'CONTACTO COMPLETO'
    LIMIT 1;

    IF v_contacto_full IS NULL THEN
        INSERT INTO "CRMCONTACTOS" (
            "cliente_id",
            "nombre",
            "apellido",
            "cargo",
            "email",
            "telefono",
            "canal_preferido",
            "estado_contacto",
            "notas",
            "activo",
            "created_at",
            "updated_at",
            "deleted_at",
            "created_by",
            "updated_by"
        )
        VALUES (
            v_tercero_2,
            'ZZ',
            'CONTACTO COMPLETO',
            'Gerente Comercial',
            'crm.full@example.com',
            '+595981000000',
            'whatsapp',
            'activo',
            'Contacto completo de ejemplo para pruebas CRM.',
            TRUE,
            CURRENT_TIMESTAMP,
            CURRENT_TIMESTAMP,
            NULL,
            v_usuario_1,
            v_usuario_2
        )
        RETURNING "id" INTO v_contacto_full;
    END IF;

    SELECT "id" INTO v_oportunidad_full
    FROM "CRMOPORTUNIDADES"
    WHERE "cliente_id" = v_tercero_2
      AND "titulo" = 'ZZ CRM OPORTUNIDAD COMPLETA'
    LIMIT 1;

    IF v_oportunidad_full IS NULL THEN
        INSERT INTO "CRMOPORTUNIDADES" (
            "cliente_id",
            "contacto_principal_id",
            "titulo",
            "etapa",
            "probabilidad",
            "monto_estimado",
            "moneda",
            "fecha_apertura",
            "fecha_estimada_cierre",
            "responsable_id",
            "origen",
            "motivo_perdida",
            "notas",
            "activa",
            "created_at",
            "updated_at",
            "deleted_at",
            "created_by",
            "updated_by"
        )
        VALUES (
            v_tercero_2,
            v_contacto_full,
            'ZZ CRM OPORTUNIDAD COMPLETA',
            'cerrado_perdido',
            0,
            18500.50,
            'USD',
            CURRENT_DATE - 20,
            CURRENT_DATE - 5,
            v_usuario_2,
            'referido',
            'Presupuesto fuera de rango del cliente.',
            'Oportunidad completa de ejemplo con todos los datos utiles cargados.',
            TRUE,
            CURRENT_TIMESTAMP,
            CURRENT_TIMESTAMP,
            NULL,
            v_usuario_1,
            v_usuario_2
        )
        RETURNING "id" INTO v_oportunidad_full;
    END IF;

    INSERT INTO "CRMINTERACCIONES" (
        "cliente_id",
        "contacto_id",
        "oportunidad_id",
        "tipo_interaccion",
        "canal",
        "fecha_hora",
        "usuario_responsable_id",
        "resultado",
        "descripcion",
        "adjuntos_json",
        "created_at",
        "updated_at",
        "deleted_at",
        "created_by",
        "updated_by"
    )
    SELECT
        v_tercero_2,
        v_contacto_full,
        v_oportunidad_full,
        'reunion',
        'videollamada',
        CURRENT_TIMESTAMP - INTERVAL '2 days',
        v_usuario_2,
        'reprogramada',
        'Interaccion completa de ejemplo con adjuntos y oportunidad asociada.',
        '["propuesta.pdf","minuta.txt"]'::jsonb,
        CURRENT_TIMESTAMP,
        CURRENT_TIMESTAMP,
        NULL,
        v_usuario_1,
        v_usuario_2
    WHERE NOT EXISTS (
        SELECT 1
        FROM "CRMINTERACCIONES"
        WHERE "cliente_id" = v_tercero_2
          AND "descripcion" = 'Interaccion completa de ejemplo con adjuntos y oportunidad asociada.'
    );

    INSERT INTO "CRMTAREAS" (
        "cliente_id",
        "oportunidad_id",
        "asignado_a_id",
        "titulo",
        "descripcion",
        "tipo_tarea",
        "fecha_vencimiento",
        "prioridad",
        "estado",
        "fecha_completado",
        "activa",
        "created_at",
        "updated_at",
        "deleted_at",
        "created_by",
        "updated_by"
    )
    SELECT
        v_tercero_2,
        v_oportunidad_full,
        v_usuario_2,
        'ZZ CRM TAREA COMPLETA',
        'Tarea completa con todos los campos utiles de CRM.',
        'preparar_propuesta',
        CURRENT_DATE - 3,
        'alta',
        'completada',
        CURRENT_DATE - 1,
        TRUE,
        CURRENT_TIMESTAMP,
        CURRENT_TIMESTAMP,
        NULL,
        v_usuario_1,
        v_usuario_2
    WHERE NOT EXISTS (
        SELECT 1
        FROM "CRMTAREAS"
        WHERE "titulo" = 'ZZ CRM TAREA COMPLETA'
          AND "cliente_id" = v_tercero_2
    );

    SELECT "id" INTO v_campana_full
    FROM "CRMCAMPANAS"
    WHERE "nombre" = 'ZZ CRM CAMPANA COMPLETA'
      AND "sucursal_id" = v_sucursal_1
    LIMIT 1;

    IF v_campana_full IS NULL THEN
        INSERT INTO "CRMCAMPANAS" (
            "sucursal_id",
            "nombre",
            "descripcion",
            "tipo_campana",
            "objetivo",
            "segmento_objetivo_id",
            "fecha_inicio",
            "fecha_fin",
            "presupuesto",
            "presupuesto_gastado",
            "responsable_id",
            "notas",
            "leads_generados",
            "oportunidades_generadas",
            "negocios_ganados",
            "activa",
            "created_at",
            "updated_at",
            "deleted_at",
            "created_by",
            "updated_by"
        )
        VALUES (
            v_sucursal_1,
            'ZZ CRM CAMPANA COMPLETA',
            'Campana completa de ejemplo para el modulo CRM.',
            'email',
            'generacion_leads',
            v_segmento_full,
            CURRENT_DATE - 15,
            CURRENT_DATE + 15,
            25000.00,
            9200.00,
            v_usuario_2,
            'Campana de ejemplo con presupuesto y resultados cargados.',
            120,
            18,
            4,
            TRUE,
            CURRENT_TIMESTAMP,
            CURRENT_TIMESTAMP,
            NULL,
            v_usuario_1,
            v_usuario_2
        )
        RETURNING "id" INTO v_campana_full;
    END IF;

    INSERT INTO "CRMSEGUIMIENTOS" (
        "sucursal_id",
        "tercero_id",
        "motivo_id",
        "interes_id",
        "campana_id",
        "fecha",
        "descripcion",
        "proxima_accion",
        "usuario_id",
        "created_at",
        "updated_at",
        "deleted_at",
        "created_by",
        "updated_by"
    )
    SELECT
        v_sucursal_1,
        v_tercero_2,
        v_motivo_full,
        v_interes_full,
        v_campana_full,
        CURRENT_DATE,
        'ZZ CRM SEGUIMIENTO COMPLETO',
        CURRENT_DATE + 7,
        v_usuario_2,
        CURRENT_TIMESTAMP,
        CURRENT_TIMESTAMP,
        NULL,
        v_usuario_1,
        v_usuario_2
    WHERE NOT EXISTS (
        SELECT 1
        FROM "CRMSEGUIMIENTOS"
        WHERE "tercero_id" = v_tercero_2
          AND "descripcion" = 'ZZ CRM SEGUIMIENTO COMPLETO'
    );

    INSERT INTO "CRMCOMUNICADOS" (
        "sucursal_id",
        "tercero_id",
        "campana_id",
        "tipo_id",
        "fecha",
        "asunto",
        "contenido",
        "usuario_id",
        "created_at",
        "updated_at",
        "deleted_at",
        "created_by",
        "updated_by"
    )
    SELECT
        v_sucursal_1,
        v_tercero_2,
        v_campana_full,
        v_tipo_full,
        CURRENT_DATE,
        'ZZ CRM COMUNICADO COMPLETO',
        'Contenido completo de ejemplo para comunicado CRM con campaña y tipo asociados.',
        v_usuario_2,
        CURRENT_TIMESTAMP,
        CURRENT_TIMESTAMP,
        NULL,
        v_usuario_1,
        v_usuario_2
    WHERE NOT EXISTS (
        SELECT 1
        FROM "CRMCOMUNICADOS"
        WHERE "tercero_id" = v_tercero_2
          AND "asunto" = 'ZZ CRM COMUNICADO COMPLETO'
    );

    INSERT INTO "CRMSEGMENTOS_MIEMBROS" (
        "segmento_id",
        "cliente_id",
        "activo",
        "created_at",
        "updated_at",
        "deleted_at",
        "created_by",
        "updated_by"
    )
    VALUES (
        v_segmento_full,
        v_tercero_2,
        TRUE,
        CURRENT_TIMESTAMP,
        CURRENT_TIMESTAMP,
        NULL,
        v_usuario_1,
        v_usuario_2
    )
    ON CONFLICT ("segmento_id", "cliente_id") DO NOTHING;
END $$;

COMMIT;