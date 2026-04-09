BEGIN;

ALTER TABLE IF EXISTS "terceros" ADD COLUMN IF NOT EXISTS "nombre_fantasia" varchar(200);
ALTER TABLE IF EXISTS "terceros" ADD COLUMN IF NOT EXISTS "tipo_personeria" varchar(20) NOT NULL DEFAULT 'JURIDICA';
ALTER TABLE IF EXISTS "terceros" ADD COLUMN IF NOT EXISTS "nombre" varchar(150);
ALTER TABLE IF EXISTS "terceros" ADD COLUMN IF NOT EXISTS "apellido" varchar(150);
ALTER TABLE IF EXISTS "terceros" ADD COLUMN IF NOT EXISTS "tratamiento" varchar(250);
ALTER TABLE IF EXISTS "terceros" ADD COLUMN IF NOT EXISTS "profesion" varchar(250);
ALTER TABLE IF EXISTS "terceros" ADD COLUMN IF NOT EXISTS "estado_persona_id" bigint;
ALTER TABLE IF EXISTS "terceros" ADD COLUMN IF NOT EXISTS "estado_civil_id" bigint;
ALTER TABLE IF EXISTS "terceros" ADD COLUMN IF NOT EXISTS "estado_civil" varchar(250);
ALTER TABLE IF EXISTS "terceros" ADD COLUMN IF NOT EXISTS "nacionalidad" varchar(250);
ALTER TABLE IF EXISTS "terceros" ADD COLUMN IF NOT EXISTS "sexo" varchar(1);
ALTER TABLE IF EXISTS "terceros" ADD COLUMN IF NOT EXISTS "fecha_nacimiento" date;
ALTER TABLE IF EXISTS "terceros" ADD COLUMN IF NOT EXISTS "fecha_registro" date;
ALTER TABLE IF EXISTS "terceros" ADD COLUMN IF NOT EXISTS "es_entidad_gubernamental" boolean NOT NULL DEFAULT FALSE;
ALTER TABLE IF EXISTS "terceros" ADD COLUMN IF NOT EXISTS "clave_fiscal" varchar(50);
ALTER TABLE IF EXISTS "terceros" ADD COLUMN IF NOT EXISTS "valor_clave_fiscal" varchar(30);
ALTER TABLE IF EXISTS "terceros" ADD COLUMN IF NOT EXISTS "categoria_id" bigint;
ALTER TABLE IF EXISTS "terceros" ADD COLUMN IF NOT EXISTS "categoria_cliente_id" bigint;
ALTER TABLE IF EXISTS "terceros" ADD COLUMN IF NOT EXISTS "estado_cliente_id" bigint;
ALTER TABLE IF EXISTS "terceros" ADD COLUMN IF NOT EXISTS "categoria_proveedor_id" bigint;
ALTER TABLE IF EXISTS "terceros" ADD COLUMN IF NOT EXISTS "estado_proveedor_id" bigint;
ALTER TABLE IF EXISTS "terceros" ADD COLUMN IF NOT EXISTS "es_cliente" boolean NOT NULL DEFAULT FALSE;
ALTER TABLE IF EXISTS "terceros" ADD COLUMN IF NOT EXISTS "es_proveedor" boolean NOT NULL DEFAULT FALSE;
ALTER TABLE IF EXISTS "terceros" ADD COLUMN IF NOT EXISTS "es_empleado" boolean NOT NULL DEFAULT FALSE;
ALTER TABLE IF EXISTS "terceros" ADD COLUMN IF NOT EXISTS "calle" varchar(150);
ALTER TABLE IF EXISTS "terceros" ADD COLUMN IF NOT EXISTS "nro" varchar(20);
ALTER TABLE IF EXISTS "terceros" ADD COLUMN IF NOT EXISTS "piso" varchar(10);
ALTER TABLE IF EXISTS "terceros" ADD COLUMN IF NOT EXISTS "dpto" varchar(10);
ALTER TABLE IF EXISTS "terceros" ADD COLUMN IF NOT EXISTS "codigo_postal" varchar(10);
ALTER TABLE IF EXISTS "terceros" ADD COLUMN IF NOT EXISTS "provincia_id" bigint;
ALTER TABLE IF EXISTS "terceros" ADD COLUMN IF NOT EXISTS "localidad_id" bigint;
ALTER TABLE IF EXISTS "terceros" ADD COLUMN IF NOT EXISTS "barrio_id" bigint;
ALTER TABLE IF EXISTS "terceros" ADD COLUMN IF NOT EXISTS "pais_id" bigint;
ALTER TABLE IF EXISTS "terceros" ADD COLUMN IF NOT EXISTS "nro_ingresos_brutos" varchar(30);
ALTER TABLE IF EXISTS "terceros" ADD COLUMN IF NOT EXISTS "nro_municipal" varchar(30);
ALTER TABLE IF EXISTS "terceros" ADD COLUMN IF NOT EXISTS "telefono" varchar(30);
ALTER TABLE IF EXISTS "terceros" ADD COLUMN IF NOT EXISTS "celular" varchar(30);
ALTER TABLE IF EXISTS "terceros" ADD COLUMN IF NOT EXISTS "email" varchar(150);
ALTER TABLE IF EXISTS "terceros" ADD COLUMN IF NOT EXISTS "web" varchar(150);
ALTER TABLE IF EXISTS "terceros" ADD COLUMN IF NOT EXISTS "usuario_id" bigint;
ALTER TABLE IF EXISTS "terceros" ADD COLUMN IF NOT EXISTS "moneda_id" bigint;
ALTER TABLE IF EXISTS "terceros" ADD COLUMN IF NOT EXISTS "limite_credito" numeric(18,2);
ALTER TABLE IF EXISTS "terceros" ADD COLUMN IF NOT EXISTS "porcentaje_maximo_descuento" numeric(5,2);
ALTER TABLE IF EXISTS "terceros" ADD COLUMN IF NOT EXISTS "vigencia_credito_desde" date;
ALTER TABLE IF EXISTS "terceros" ADD COLUMN IF NOT EXISTS "vigencia_credito_hasta" date;
ALTER TABLE IF EXISTS "terceros" ADD COLUMN IF NOT EXISTS "facturable" boolean NOT NULL DEFAULT TRUE;
ALTER TABLE IF EXISTS "terceros" ADD COLUMN IF NOT EXISTS "cobrador_id" bigint;
ALTER TABLE IF EXISTS "terceros" ADD COLUMN IF NOT EXISTS "aplica_comision_cobrador" boolean NOT NULL DEFAULT FALSE;
ALTER TABLE IF EXISTS "terceros" ADD COLUMN IF NOT EXISTS "pct_comision_cobrador" numeric(5,2) NOT NULL DEFAULT 0;
ALTER TABLE IF EXISTS "terceros" ADD COLUMN IF NOT EXISTS "vendedor_id" bigint;
ALTER TABLE IF EXISTS "terceros" ADD COLUMN IF NOT EXISTS "aplica_comision_vendedor" boolean NOT NULL DEFAULT FALSE;
ALTER TABLE IF EXISTS "terceros" ADD COLUMN IF NOT EXISTS "pct_comision_vendedor" numeric(5,2) NOT NULL DEFAULT 0;
ALTER TABLE IF EXISTS "terceros" ADD COLUMN IF NOT EXISTS "observacion" text;
ALTER TABLE IF EXISTS "terceros" ADD COLUMN IF NOT EXISTS "sucursal_id" bigint;
ALTER TABLE IF EXISTS "terceros" ADD COLUMN IF NOT EXISTS "activo" boolean NOT NULL DEFAULT TRUE;
ALTER TABLE IF EXISTS "terceros" ADD COLUMN IF NOT EXISTS "created_at" timestamp with time zone NOT NULL DEFAULT CURRENT_TIMESTAMP;
ALTER TABLE IF EXISTS "terceros" ADD COLUMN IF NOT EXISTS "updated_at" timestamp with time zone NOT NULL DEFAULT CURRENT_TIMESTAMP;
ALTER TABLE IF EXISTS "terceros" ADD COLUMN IF NOT EXISTS "deleted_at" timestamp with time zone NULL;
ALTER TABLE IF EXISTS "terceros" ADD COLUMN IF NOT EXISTS "created_by" bigint NULL;
ALTER TABLE IF EXISTS "terceros" ADD COLUMN IF NOT EXISTS "updated_by" bigint NULL;

CREATE TABLE IF NOT EXISTS "CRMTIPOCOMUNICADOS" (
    "id" bigint GENERATED BY DEFAULT AS IDENTITY PRIMARY KEY,
    "codigo" varchar(30) NOT NULL,
    "descripcion" varchar(200) NOT NULL,
    "activo" boolean NOT NULL DEFAULT TRUE,
    "created_at" timestamp with time zone NOT NULL DEFAULT CURRENT_TIMESTAMP,
    "updated_at" timestamp with time zone NOT NULL DEFAULT CURRENT_TIMESTAMP,
    "deleted_at" timestamp with time zone NULL,
    "created_by" bigint NULL,
    "updated_by" bigint NULL,
    CONSTRAINT "uq_crmtipocomunicados_codigo" UNIQUE ("codigo"),
    CONSTRAINT "fk_crmtipocomunicados_created_by_usuarios" FOREIGN KEY ("created_by") REFERENCES "usuarios" ("id") ON DELETE SET NULL,
    CONSTRAINT "fk_crmtipocomunicados_updated_by_usuarios" FOREIGN KEY ("updated_by") REFERENCES "usuarios" ("id") ON DELETE SET NULL
);
CREATE INDEX IF NOT EXISTS "ix_crmtipocomunicados_activo" ON "CRMTIPOCOMUNICADOS" ("activo");

CREATE TABLE IF NOT EXISTS "CRMMOTIVOS" (
    "id" bigint GENERATED BY DEFAULT AS IDENTITY PRIMARY KEY,
    "codigo" varchar(30) NOT NULL,
    "descripcion" varchar(200) NOT NULL,
    "activo" boolean NOT NULL DEFAULT TRUE,
    "created_at" timestamp with time zone NOT NULL DEFAULT CURRENT_TIMESTAMP,
    "updated_at" timestamp with time zone NOT NULL DEFAULT CURRENT_TIMESTAMP,
    "deleted_at" timestamp with time zone NULL,
    "created_by" bigint NULL,
    "updated_by" bigint NULL,
    CONSTRAINT "uq_crmmotivos_codigo" UNIQUE ("codigo"),
    CONSTRAINT "fk_crmmotivos_created_by_usuarios" FOREIGN KEY ("created_by") REFERENCES "usuarios" ("id") ON DELETE SET NULL,
    CONSTRAINT "fk_crmmotivos_updated_by_usuarios" FOREIGN KEY ("updated_by") REFERENCES "usuarios" ("id") ON DELETE SET NULL
);
CREATE INDEX IF NOT EXISTS "ix_crmmotivos_activo" ON "CRMMOTIVOS" ("activo");

CREATE TABLE IF NOT EXISTS "CRMINTERESES" (
    "id" bigint GENERATED BY DEFAULT AS IDENTITY PRIMARY KEY,
    "codigo" varchar(30) NOT NULL,
    "descripcion" varchar(200) NOT NULL,
    "activo" boolean NOT NULL DEFAULT TRUE,
    "created_at" timestamp with time zone NOT NULL DEFAULT CURRENT_TIMESTAMP,
    "updated_at" timestamp with time zone NOT NULL DEFAULT CURRENT_TIMESTAMP,
    "deleted_at" timestamp with time zone NULL,
    "created_by" bigint NULL,
    "updated_by" bigint NULL,
    CONSTRAINT "uq_crmintereses_codigo" UNIQUE ("codigo"),
    CONSTRAINT "fk_crmintereses_created_by_usuarios" FOREIGN KEY ("created_by") REFERENCES "usuarios" ("id") ON DELETE SET NULL,
    CONSTRAINT "fk_crmintereses_updated_by_usuarios" FOREIGN KEY ("updated_by") REFERENCES "usuarios" ("id") ON DELETE SET NULL
);
CREATE INDEX IF NOT EXISTS "ix_crmintereses_activo" ON "CRMINTERESES" ("activo");

CREATE TABLE IF NOT EXISTS "tipos_relaciones_contacto" (
    "id" bigint GENERATED BY DEFAULT AS IDENTITY PRIMARY KEY,
    "codigo" varchar(30) NOT NULL,
    "descripcion" varchar(200) NOT NULL,
    "activo" boolean NOT NULL DEFAULT TRUE,
    "created_at" timestamp with time zone NOT NULL DEFAULT CURRENT_TIMESTAMP,
    "updated_at" timestamp with time zone NOT NULL DEFAULT CURRENT_TIMESTAMP,
    "deleted_at" timestamp with time zone NULL,
    "created_by" bigint NULL,
    "updated_by" bigint NULL,
    CONSTRAINT "ux_tipos_relaciones_contacto_codigo" UNIQUE ("codigo"),
    CONSTRAINT "fk_tipos_relaciones_contacto_created_by_usuarios" FOREIGN KEY ("created_by") REFERENCES "usuarios" ("id") ON DELETE SET NULL,
    CONSTRAINT "fk_tipos_relaciones_contacto_updated_by_usuarios" FOREIGN KEY ("updated_by") REFERENCES "usuarios" ("id") ON DELETE SET NULL
);
CREATE INDEX IF NOT EXISTS "ix_tipos_relaciones_contacto_activo" ON "tipos_relaciones_contacto" ("activo");

CREATE TABLE IF NOT EXISTS "CONTACTOS" (
    "id" bigint GENERATED BY DEFAULT AS IDENTITY PRIMARY KEY,
    "id_persona" bigint NOT NULL,
    "id_contacto" bigint NOT NULL,
    "trel_id" bigint NULL,
    CONSTRAINT "fk_contactos_id_persona_terceros" FOREIGN KEY ("id_persona") REFERENCES "terceros" ("id") ON DELETE CASCADE,
    CONSTRAINT "fk_contactos_id_contacto_terceros" FOREIGN KEY ("id_contacto") REFERENCES "terceros" ("id") ON DELETE CASCADE,
    CONSTRAINT "fk_contactos_trel_id_tipos_relaciones_contacto" FOREIGN KEY ("trel_id") REFERENCES "tipos_relaciones_contacto" ("id") ON DELETE SET NULL
);
CREATE INDEX IF NOT EXISTS "ix_contactos_id_persona" ON "CONTACTOS" ("id_persona");
CREATE INDEX IF NOT EXISTS "ix_contactos_id_persona_contacto" ON "CONTACTOS" ("id_persona", "id_contacto");

CREATE TABLE IF NOT EXISTS "CRMSEGMENTOS" (
    "id" bigint GENERATED BY DEFAULT AS IDENTITY PRIMARY KEY,
    "nombre" varchar(160) NOT NULL,
    "descripcion" varchar(1000) NULL,
    "criterios_json" jsonb NOT NULL DEFAULT '[]'::jsonb,
    "tipo_segmento" varchar(20) NOT NULL,
    "activo" boolean NOT NULL DEFAULT TRUE,
    "created_at" timestamp with time zone NOT NULL DEFAULT CURRENT_TIMESTAMP,
    "updated_at" timestamp with time zone NOT NULL DEFAULT CURRENT_TIMESTAMP,
    "deleted_at" timestamp with time zone NULL,
    "created_by" bigint NULL,
    "updated_by" bigint NULL,
    CONSTRAINT "uq_crmsegmentos_nombre" UNIQUE ("nombre"),
    CONSTRAINT "fk_crmsegmentos_created_by_usuarios" FOREIGN KEY ("created_by") REFERENCES "usuarios" ("id") ON DELETE SET NULL,
    CONSTRAINT "fk_crmsegmentos_updated_by_usuarios" FOREIGN KEY ("updated_by") REFERENCES "usuarios" ("id") ON DELETE SET NULL
);

CREATE TABLE IF NOT EXISTS "CRMUSUARIOS" (
    "id" bigint GENERATED BY DEFAULT AS IDENTITY PRIMARY KEY,
    "usuario_id" bigint NOT NULL,
    "rol" varchar(30) NOT NULL,
    "avatar" varchar(500) NULL,
    "activo" boolean NOT NULL DEFAULT TRUE,
    "created_at" timestamp with time zone NOT NULL DEFAULT CURRENT_TIMESTAMP,
    "updated_at" timestamp with time zone NOT NULL DEFAULT CURRENT_TIMESTAMP,
    "deleted_at" timestamp with time zone NULL,
    "created_by" bigint NULL,
    "updated_by" bigint NULL,
    CONSTRAINT "uq_crmusuarios_usuario_id" UNIQUE ("usuario_id"),
    CONSTRAINT "fk_crmusuarios_usuario_id_usuarios" FOREIGN KEY ("usuario_id") REFERENCES "usuarios" ("id") ON DELETE RESTRICT,
    CONSTRAINT "fk_crmusuarios_created_by_usuarios" FOREIGN KEY ("created_by") REFERENCES "usuarios" ("id") ON DELETE SET NULL,
    CONSTRAINT "fk_crmusuarios_updated_by_usuarios" FOREIGN KEY ("updated_by") REFERENCES "usuarios" ("id") ON DELETE SET NULL
);
CREATE INDEX IF NOT EXISTS "ix_crmusuarios_rol_activo" ON "CRMUSUARIOS" ("rol", "activo");

CREATE TABLE IF NOT EXISTS "CRMCLIENTES" (
    "id" bigint GENERATED BY DEFAULT AS IDENTITY PRIMARY KEY,
    "tercero_id" bigint NOT NULL,
    "tipo_cliente" varchar(30) NOT NULL,
    "segmento" varchar(30) NOT NULL,
    "industria" varchar(120) NULL,
    "pais" varchar(120) NOT NULL,
    "provincia" varchar(120) NULL,
    "ciudad" varchar(120) NULL,
    "direccion" varchar(300) NULL,
    "origen_cliente" varchar(30) NOT NULL,
    "estado_relacion" varchar(30) NOT NULL,
    "responsable_id" bigint NULL,
    "notas_generales" varchar(2000) NULL,
    "activo" boolean NOT NULL DEFAULT TRUE,
    "created_at" timestamp with time zone NOT NULL DEFAULT CURRENT_TIMESTAMP,
    "updated_at" timestamp with time zone NOT NULL DEFAULT CURRENT_TIMESTAMP,
    "deleted_at" timestamp with time zone NULL,
    "created_by" bigint NULL,
    "updated_by" bigint NULL,
    CONSTRAINT "uq_crmclientes_tercero_id" UNIQUE ("tercero_id"),
    CONSTRAINT "fk_crmclientes_tercero_id_terceros" FOREIGN KEY ("tercero_id") REFERENCES "terceros" ("id") ON DELETE RESTRICT,
    CONSTRAINT "fk_crmclientes_responsable_id_usuarios" FOREIGN KEY ("responsable_id") REFERENCES "usuarios" ("id") ON DELETE SET NULL,
    CONSTRAINT "fk_crmclientes_created_by_usuarios" FOREIGN KEY ("created_by") REFERENCES "usuarios" ("id") ON DELETE SET NULL,
    CONSTRAINT "fk_crmclientes_updated_by_usuarios" FOREIGN KEY ("updated_by") REFERENCES "usuarios" ("id") ON DELETE SET NULL
);
CREATE INDEX IF NOT EXISTS "ix_crmclientes_responsable_estado" ON "CRMCLIENTES" ("responsable_id", "estado_relacion");

CREATE TABLE IF NOT EXISTS "CRMCONTACTOS" (
    "id" bigint GENERATED BY DEFAULT AS IDENTITY PRIMARY KEY,
    "cliente_id" bigint NOT NULL,
    "nombre" varchar(120) NOT NULL,
    "apellido" varchar(120) NOT NULL,
    "cargo" varchar(120) NULL,
    "email" varchar(200) NULL,
    "telefono" varchar(60) NULL,
    "canal_preferido" varchar(30) NOT NULL,
    "estado_contacto" varchar(30) NOT NULL,
    "notas" varchar(2000) NULL,
    "activo" boolean NOT NULL DEFAULT TRUE,
    "created_at" timestamp with time zone NOT NULL DEFAULT CURRENT_TIMESTAMP,
    "updated_at" timestamp with time zone NOT NULL DEFAULT CURRENT_TIMESTAMP,
    "deleted_at" timestamp with time zone NULL,
    "created_by" bigint NULL,
    "updated_by" bigint NULL,
    CONSTRAINT "fk_crmcontactos_cliente_id_crmclientes" FOREIGN KEY ("cliente_id") REFERENCES "CRMCLIENTES" ("tercero_id") ON DELETE RESTRICT,
    CONSTRAINT "fk_crmcontactos_created_by_usuarios" FOREIGN KEY ("created_by") REFERENCES "usuarios" ("id") ON DELETE SET NULL,
    CONSTRAINT "fk_crmcontactos_updated_by_usuarios" FOREIGN KEY ("updated_by") REFERENCES "usuarios" ("id") ON DELETE SET NULL
);
CREATE INDEX IF NOT EXISTS "ix_crmcontactos_cliente_activo" ON "CRMCONTACTOS" ("cliente_id", "activo");

CREATE TABLE IF NOT EXISTS "CRMOPORTUNIDADES" (
    "id" bigint GENERATED BY DEFAULT AS IDENTITY PRIMARY KEY,
    "cliente_id" bigint NOT NULL,
    "contacto_principal_id" bigint NULL,
    "titulo" varchar(200) NOT NULL,
    "etapa" varchar(40) NOT NULL,
    "probabilidad" integer NOT NULL,
    "monto_estimado" numeric(18,2) NOT NULL DEFAULT 0,
    "moneda" varchar(10) NOT NULL,
    "fecha_apertura" date NOT NULL,
    "fecha_estimada_cierre" date NULL,
    "responsable_id" bigint NULL,
    "origen" varchar(30) NOT NULL,
    "motivo_perdida" varchar(500) NULL,
    "notas" varchar(2000) NULL,
    "activa" boolean NOT NULL DEFAULT TRUE,
    "created_at" timestamp with time zone NOT NULL DEFAULT CURRENT_TIMESTAMP,
    "updated_at" timestamp with time zone NOT NULL DEFAULT CURRENT_TIMESTAMP,
    "deleted_at" timestamp with time zone NULL,
    "created_by" bigint NULL,
    "updated_by" bigint NULL,
    CONSTRAINT "ck_crmoportunidades_probabilidad" CHECK ("probabilidad" BETWEEN 0 AND 100),
    CONSTRAINT "fk_crmoportunidades_cliente_id_crmclientes" FOREIGN KEY ("cliente_id") REFERENCES "CRMCLIENTES" ("tercero_id") ON DELETE RESTRICT,
    CONSTRAINT "fk_crmoportunidades_contacto_id_crmcontactos" FOREIGN KEY ("contacto_principal_id") REFERENCES "CRMCONTACTOS" ("id") ON DELETE SET NULL,
    CONSTRAINT "fk_crmoportunidades_responsable_id_usuarios" FOREIGN KEY ("responsable_id") REFERENCES "usuarios" ("id") ON DELETE SET NULL,
    CONSTRAINT "fk_crmoportunidades_created_by_usuarios" FOREIGN KEY ("created_by") REFERENCES "usuarios" ("id") ON DELETE SET NULL,
    CONSTRAINT "fk_crmoportunidades_updated_by_usuarios" FOREIGN KEY ("updated_by") REFERENCES "usuarios" ("id") ON DELETE SET NULL
);
CREATE INDEX IF NOT EXISTS "ix_crmoportunidades_cliente_activa" ON "CRMOPORTUNIDADES" ("cliente_id", "activa");
CREATE INDEX IF NOT EXISTS "ix_crmoportunidades_responsable_etapa" ON "CRMOPORTUNIDADES" ("responsable_id", "etapa");

CREATE TABLE IF NOT EXISTS "CRMINTERACCIONES" (
    "id" bigint GENERATED BY DEFAULT AS IDENTITY PRIMARY KEY,
    "cliente_id" bigint NOT NULL,
    "contacto_id" bigint NULL,
    "oportunidad_id" bigint NULL,
    "tipo_interaccion" varchar(40) NOT NULL,
    "canal" varchar(40) NOT NULL,
    "fecha_hora" timestamp with time zone NOT NULL,
    "usuario_responsable_id" bigint NOT NULL,
    "resultado" varchar(40) NOT NULL,
    "descripcion" varchar(2000) NULL,
    "adjuntos_json" jsonb NOT NULL DEFAULT '[]'::jsonb,
    "created_at" timestamp with time zone NOT NULL DEFAULT CURRENT_TIMESTAMP,
    "updated_at" timestamp with time zone NOT NULL DEFAULT CURRENT_TIMESTAMP,
    "deleted_at" timestamp with time zone NULL,
    "created_by" bigint NULL,
    "updated_by" bigint NULL,
    CONSTRAINT "fk_crminteracciones_cliente_id_crmclientes" FOREIGN KEY ("cliente_id") REFERENCES "CRMCLIENTES" ("tercero_id") ON DELETE RESTRICT,
    CONSTRAINT "fk_crminteracciones_contacto_id_crmcontactos" FOREIGN KEY ("contacto_id") REFERENCES "CRMCONTACTOS" ("id") ON DELETE SET NULL,
    CONSTRAINT "fk_crminteracciones_oportunidad_id_crmoportunidades" FOREIGN KEY ("oportunidad_id") REFERENCES "CRMOPORTUNIDADES" ("id") ON DELETE SET NULL,
    CONSTRAINT "fk_crminteracciones_usuario_id_usuarios" FOREIGN KEY ("usuario_responsable_id") REFERENCES "usuarios" ("id") ON DELETE RESTRICT,
    CONSTRAINT "fk_crminteracciones_created_by_usuarios" FOREIGN KEY ("created_by") REFERENCES "usuarios" ("id") ON DELETE SET NULL,
    CONSTRAINT "fk_crminteracciones_updated_by_usuarios" FOREIGN KEY ("updated_by") REFERENCES "usuarios" ("id") ON DELETE SET NULL
);
CREATE INDEX IF NOT EXISTS "ix_crminteracciones_cliente_fecha_hora" ON "CRMINTERACCIONES" ("cliente_id", "fecha_hora");
CREATE INDEX IF NOT EXISTS "ix_crminteracciones_oportunidad_id" ON "CRMINTERACCIONES" ("oportunidad_id");

CREATE TABLE IF NOT EXISTS "CRMTAREAS" (
    "id" bigint GENERATED BY DEFAULT AS IDENTITY PRIMARY KEY,
    "cliente_id" bigint NULL,
    "oportunidad_id" bigint NULL,
    "asignado_a_id" bigint NOT NULL,
    "titulo" varchar(200) NOT NULL,
    "descripcion" varchar(2000) NULL,
    "tipo_tarea" varchar(40) NOT NULL,
    "fecha_vencimiento" date NOT NULL,
    "prioridad" varchar(20) NOT NULL,
    "estado" varchar(20) NOT NULL,
    "fecha_completado" date NULL,
    "activa" boolean NOT NULL DEFAULT TRUE,
    "created_at" timestamp with time zone NOT NULL DEFAULT CURRENT_TIMESTAMP,
    "updated_at" timestamp with time zone NOT NULL DEFAULT CURRENT_TIMESTAMP,
    "deleted_at" timestamp with time zone NULL,
    "created_by" bigint NULL,
    "updated_by" bigint NULL,
    CONSTRAINT "fk_crmtareas_cliente_id_crmclientes" FOREIGN KEY ("cliente_id") REFERENCES "CRMCLIENTES" ("tercero_id") ON DELETE SET NULL,
    CONSTRAINT "fk_crmtareas_oportunidad_id_crmoportunidades" FOREIGN KEY ("oportunidad_id") REFERENCES "CRMOPORTUNIDADES" ("id") ON DELETE SET NULL,
    CONSTRAINT "fk_crmtareas_asignado_a_id_usuarios" FOREIGN KEY ("asignado_a_id") REFERENCES "usuarios" ("id") ON DELETE RESTRICT,
    CONSTRAINT "fk_crmtareas_created_by_usuarios" FOREIGN KEY ("created_by") REFERENCES "usuarios" ("id") ON DELETE SET NULL,
    CONSTRAINT "fk_crmtareas_updated_by_usuarios" FOREIGN KEY ("updated_by") REFERENCES "usuarios" ("id") ON DELETE SET NULL
);
CREATE INDEX IF NOT EXISTS "ix_crmtareas_asignado_estado_vencimiento" ON "CRMTAREAS" ("asignado_a_id", "estado", "fecha_vencimiento");
CREATE INDEX IF NOT EXISTS "ix_crmtareas_cliente_id" ON "CRMTAREAS" ("cliente_id");
CREATE INDEX IF NOT EXISTS "ix_crmtareas_oportunidad_id" ON "CRMTAREAS" ("oportunidad_id");

CREATE TABLE IF NOT EXISTS "CRMCAMPANAS" (
    "id" bigint GENERATED BY DEFAULT AS IDENTITY PRIMARY KEY,
    "sucursal_id" bigint NOT NULL,
    "nombre" varchar(200) NOT NULL,
    "descripcion" varchar(1000) NULL,
    "tipo_campana" varchar(30) NOT NULL DEFAULT 'email',
    "objetivo" varchar(40) NOT NULL DEFAULT 'generacion_leads',
    "segmento_objetivo_id" bigint NULL,
    "fecha_inicio" date NOT NULL,
    "fecha_fin" date NOT NULL,
    "presupuesto" numeric(18,2) NULL,
    "presupuesto_gastado" numeric(18,2) NULL,
    "responsable_id" bigint NULL,
    "notas" varchar(2000) NULL,
    "leads_generados" integer NOT NULL DEFAULT 0,
    "oportunidades_generadas" integer NOT NULL DEFAULT 0,
    "negocios_ganados" integer NOT NULL DEFAULT 0,
    "activa" boolean NOT NULL DEFAULT TRUE,
    "created_at" timestamp with time zone NOT NULL DEFAULT CURRENT_TIMESTAMP,
    "updated_at" timestamp with time zone NOT NULL DEFAULT CURRENT_TIMESTAMP,
    "deleted_at" timestamp with time zone NULL,
    "created_by" bigint NULL,
    "updated_by" bigint NULL,
    CONSTRAINT "ck_crmcampanas_fechas" CHECK ("fecha_fin" >= "fecha_inicio"),
    CONSTRAINT "fk_crmcampanas_sucursal_id_sucursales" FOREIGN KEY ("sucursal_id") REFERENCES "sucursales" ("id") ON DELETE RESTRICT,
    CONSTRAINT "fk_crmcampanas_segmento_id_crmsegmentos" FOREIGN KEY ("segmento_objetivo_id") REFERENCES "CRMSEGMENTOS" ("id") ON DELETE SET NULL,
    CONSTRAINT "fk_crmcampanas_responsable_id_usuarios" FOREIGN KEY ("responsable_id") REFERENCES "usuarios" ("id") ON DELETE SET NULL,
    CONSTRAINT "fk_crmcampanas_created_by_usuarios" FOREIGN KEY ("created_by") REFERENCES "usuarios" ("id") ON DELETE SET NULL,
    CONSTRAINT "fk_crmcampanas_updated_by_usuarios" FOREIGN KEY ("updated_by") REFERENCES "usuarios" ("id") ON DELETE SET NULL
);
CREATE INDEX IF NOT EXISTS "ix_crmcampanas_sucursal_fecha_inicio" ON "CRMCAMPANAS" ("sucursal_id", "fecha_inicio");
CREATE INDEX IF NOT EXISTS "ix_crmcampanas_segmento_objetivo_id" ON "CRMCAMPANAS" ("segmento_objetivo_id");

CREATE TABLE IF NOT EXISTS "CRMSEGUIMIENTOS" (
    "id" bigint GENERATED BY DEFAULT AS IDENTITY PRIMARY KEY,
    "sucursal_id" bigint NOT NULL,
    "tercero_id" bigint NOT NULL,
    "motivo_id" bigint NULL,
    "interes_id" bigint NULL,
    "campana_id" bigint NULL,
    "fecha" date NOT NULL,
    "descripcion" varchar(1000) NOT NULL,
    "proxima_accion" date NULL,
    "usuario_id" bigint NULL,
    "created_at" timestamp with time zone NOT NULL DEFAULT CURRENT_TIMESTAMP,
    "updated_at" timestamp with time zone NOT NULL DEFAULT CURRENT_TIMESTAMP,
    "deleted_at" timestamp with time zone NULL,
    "created_by" bigint NULL,
    "updated_by" bigint NULL,
    CONSTRAINT "fk_crmseguimientos_sucursal_id_sucursales" FOREIGN KEY ("sucursal_id") REFERENCES "sucursales" ("id") ON DELETE RESTRICT,
    CONSTRAINT "fk_crmseguimientos_tercero_id_terceros" FOREIGN KEY ("tercero_id") REFERENCES "terceros" ("id") ON DELETE RESTRICT,
    CONSTRAINT "fk_crmseguimientos_motivo_id_crmmotivos" FOREIGN KEY ("motivo_id") REFERENCES "CRMMOTIVOS" ("id") ON DELETE SET NULL,
    CONSTRAINT "fk_crmseguimientos_interes_id_crmintereses" FOREIGN KEY ("interes_id") REFERENCES "CRMINTERESES" ("id") ON DELETE SET NULL,
    CONSTRAINT "fk_crmseguimientos_campana_id_crmcampanas" FOREIGN KEY ("campana_id") REFERENCES "CRMCAMPANAS" ("id") ON DELETE SET NULL,
    CONSTRAINT "fk_crmseguimientos_usuario_id_usuarios" FOREIGN KEY ("usuario_id") REFERENCES "usuarios" ("id") ON DELETE SET NULL,
    CONSTRAINT "fk_crmseguimientos_created_by_usuarios" FOREIGN KEY ("created_by") REFERENCES "usuarios" ("id") ON DELETE SET NULL,
    CONSTRAINT "fk_crmseguimientos_updated_by_usuarios" FOREIGN KEY ("updated_by") REFERENCES "usuarios" ("id") ON DELETE SET NULL
);
CREATE INDEX IF NOT EXISTS "ix_crmseguimientos_sucursal_fecha" ON "CRMSEGUIMIENTOS" ("sucursal_id", "fecha");
CREATE INDEX IF NOT EXISTS "ix_crmseguimientos_tercero_id" ON "CRMSEGUIMIENTOS" ("tercero_id");
CREATE INDEX IF NOT EXISTS "ix_crmseguimientos_campana_id" ON "CRMSEGUIMIENTOS" ("campana_id");

CREATE TABLE IF NOT EXISTS "CRMCOMUNICADOS" (
    "id" bigint GENERATED BY DEFAULT AS IDENTITY PRIMARY KEY,
    "sucursal_id" bigint NOT NULL,
    "tercero_id" bigint NOT NULL,
    "campana_id" bigint NULL,
    "tipo_id" bigint NULL,
    "fecha" date NOT NULL,
    "asunto" varchar(300) NOT NULL,
    "contenido" text NULL,
    "usuario_id" bigint NULL,
    "created_at" timestamp with time zone NOT NULL DEFAULT CURRENT_TIMESTAMP,
    "updated_at" timestamp with time zone NOT NULL DEFAULT CURRENT_TIMESTAMP,
    "deleted_at" timestamp with time zone NULL,
    "created_by" bigint NULL,
    "updated_by" bigint NULL,
    CONSTRAINT "fk_crmcomunicados_sucursal_id_sucursales" FOREIGN KEY ("sucursal_id") REFERENCES "sucursales" ("id") ON DELETE RESTRICT,
    CONSTRAINT "fk_crmcomunicados_tercero_id_terceros" FOREIGN KEY ("tercero_id") REFERENCES "terceros" ("id") ON DELETE RESTRICT,
    CONSTRAINT "fk_crmcomunicados_campana_id_crmcampanas" FOREIGN KEY ("campana_id") REFERENCES "CRMCAMPANAS" ("id") ON DELETE SET NULL,
    CONSTRAINT "fk_crmcomunicados_tipo_id_crmtipocomunicados" FOREIGN KEY ("tipo_id") REFERENCES "CRMTIPOCOMUNICADOS" ("id") ON DELETE SET NULL,
    CONSTRAINT "fk_crmcomunicados_usuario_id_usuarios" FOREIGN KEY ("usuario_id") REFERENCES "usuarios" ("id") ON DELETE SET NULL,
    CONSTRAINT "fk_crmcomunicados_created_by_usuarios" FOREIGN KEY ("created_by") REFERENCES "usuarios" ("id") ON DELETE SET NULL,
    CONSTRAINT "fk_crmcomunicados_updated_by_usuarios" FOREIGN KEY ("updated_by") REFERENCES "usuarios" ("id") ON DELETE SET NULL
);
CREATE INDEX IF NOT EXISTS "ix_crmcomunicados_sucursal_fecha" ON "CRMCOMUNICADOS" ("sucursal_id", "fecha");
CREATE INDEX IF NOT EXISTS "ix_crmcomunicados_tercero_id" ON "CRMCOMUNICADOS" ("tercero_id");
CREATE INDEX IF NOT EXISTS "ix_crmcomunicados_campana_id" ON "CRMCOMUNICADOS" ("campana_id");

CREATE TABLE IF NOT EXISTS "CRMSEGMENTOS_MIEMBROS" (
    "id" bigint GENERATED BY DEFAULT AS IDENTITY PRIMARY KEY,
    "segmento_id" bigint NOT NULL,
    "cliente_id" bigint NOT NULL,
    "activo" boolean NOT NULL DEFAULT TRUE,
    "created_at" timestamp with time zone NOT NULL DEFAULT CURRENT_TIMESTAMP,
    "updated_at" timestamp with time zone NOT NULL DEFAULT CURRENT_TIMESTAMP,
    "deleted_at" timestamp with time zone NULL,
    "created_by" bigint NULL,
    "updated_by" bigint NULL,
    CONSTRAINT "uq_crmsegmentos_miembros_segmento_cliente" UNIQUE ("segmento_id", "cliente_id"),
    CONSTRAINT "fk_crmsegmentos_miembros_segmento_id" FOREIGN KEY ("segmento_id") REFERENCES "CRMSEGMENTOS" ("id") ON DELETE RESTRICT,
    CONSTRAINT "fk_crmsegmentos_miembros_cliente_id" FOREIGN KEY ("cliente_id") REFERENCES "CRMCLIENTES" ("tercero_id") ON DELETE RESTRICT,
    CONSTRAINT "fk_crmsegmentos_miembros_created_by_usuarios" FOREIGN KEY ("created_by") REFERENCES "usuarios" ("id") ON DELETE SET NULL,
    CONSTRAINT "fk_crmsegmentos_miembros_updated_by_usuarios" FOREIGN KEY ("updated_by") REFERENCES "usuarios" ("id") ON DELETE SET NULL
);

COMMIT;
