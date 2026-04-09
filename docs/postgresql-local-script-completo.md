-- Script unificado para PostgreSQL local.
-- Incluye:
-- 1) Alineación de esquema CRM
-- 2) Alineación de esquema Compras
-- 3) Datos de ejemplo CRM
-- 4) Datos de ejemplo Compras
-- Base de referencia validada: C:\Users\exequ\OneDrive\Desktop\DBLocal.sql

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

CREATE TABLE IF NOT EXISTS public.requisiciones_compra (
    id bigint GENERATED BY DEFAULT AS IDENTITY PRIMARY KEY,
    sucursal_id bigint NOT NULL,
    solicitante_id bigint NOT NULL,
    fecha date NOT NULL,
    descripcion character varying(300) NOT NULL,
    estado character varying(20) NOT NULL DEFAULT 'BORRADOR',
    observacion character varying(500),
    created_at timestamp with time zone NOT NULL DEFAULT CURRENT_TIMESTAMP,
    updated_at timestamp with time zone NOT NULL DEFAULT CURRENT_TIMESTAMP,
    deleted_at timestamp with time zone,
    created_by bigint,
    updated_by bigint
);

ALTER TABLE public.requisiciones_compra ADD COLUMN IF NOT EXISTS sucursal_id bigint;
ALTER TABLE public.requisiciones_compra ADD COLUMN IF NOT EXISTS solicitante_id bigint;
ALTER TABLE public.requisiciones_compra ADD COLUMN IF NOT EXISTS fecha date;
ALTER TABLE public.requisiciones_compra ADD COLUMN IF NOT EXISTS descripcion character varying(300);
ALTER TABLE public.requisiciones_compra ADD COLUMN IF NOT EXISTS estado character varying(20);
ALTER TABLE public.requisiciones_compra ADD COLUMN IF NOT EXISTS observacion character varying(500);
ALTER TABLE public.requisiciones_compra ADD COLUMN IF NOT EXISTS created_at timestamp with time zone;
ALTER TABLE public.requisiciones_compra ADD COLUMN IF NOT EXISTS updated_at timestamp with time zone;
ALTER TABLE public.requisiciones_compra ADD COLUMN IF NOT EXISTS deleted_at timestamp with time zone;
ALTER TABLE public.requisiciones_compra ADD COLUMN IF NOT EXISTS created_by bigint;
ALTER TABLE public.requisiciones_compra ADD COLUMN IF NOT EXISTS updated_by bigint;

ALTER TABLE public.requisiciones_compra ALTER COLUMN estado SET DEFAULT 'BORRADOR';
ALTER TABLE public.requisiciones_compra ALTER COLUMN created_at SET DEFAULT CURRENT_TIMESTAMP;
ALTER TABLE public.requisiciones_compra ALTER COLUMN updated_at SET DEFAULT CURRENT_TIMESTAMP;

UPDATE public.requisiciones_compra
SET estado = COALESCE(NULLIF(TRIM(estado), ''), 'BORRADOR')
WHERE estado IS NULL OR TRIM(estado) = '';

UPDATE public.requisiciones_compra
SET created_at = COALESCE(created_at, CURRENT_TIMESTAMP),
    updated_at = COALESCE(updated_at, CURRENT_TIMESTAMP)
WHERE created_at IS NULL OR updated_at IS NULL;

ALTER TABLE public.requisiciones_compra ALTER COLUMN sucursal_id SET NOT NULL;
ALTER TABLE public.requisiciones_compra ALTER COLUMN solicitante_id SET NOT NULL;
ALTER TABLE public.requisiciones_compra ALTER COLUMN fecha SET NOT NULL;
ALTER TABLE public.requisiciones_compra ALTER COLUMN descripcion SET NOT NULL;
ALTER TABLE public.requisiciones_compra ALTER COLUMN estado SET NOT NULL;
ALTER TABLE public.requisiciones_compra ALTER COLUMN created_at SET NOT NULL;
ALTER TABLE public.requisiciones_compra ALTER COLUMN updated_at SET NOT NULL;

CREATE TABLE IF NOT EXISTS public.requisiciones_compra_items (
    id bigint GENERATED BY DEFAULT AS IDENTITY PRIMARY KEY,
    requisicion_id bigint NOT NULL,
    item_id bigint,
    descripcion character varying(300) NOT NULL,
    cantidad numeric(18,4) NOT NULL,
    unidad_medida character varying(20) NOT NULL,
    observacion character varying(300)
);

ALTER TABLE public.requisiciones_compra_items ADD COLUMN IF NOT EXISTS requisicion_id bigint;
ALTER TABLE public.requisiciones_compra_items ADD COLUMN IF NOT EXISTS item_id bigint;
ALTER TABLE public.requisiciones_compra_items ADD COLUMN IF NOT EXISTS descripcion character varying(300);
ALTER TABLE public.requisiciones_compra_items ADD COLUMN IF NOT EXISTS cantidad numeric(18,4);
ALTER TABLE public.requisiciones_compra_items ADD COLUMN IF NOT EXISTS unidad_medida character varying(20);
ALTER TABLE public.requisiciones_compra_items ADD COLUMN IF NOT EXISTS observacion character varying(300);

UPDATE public.requisiciones_compra_items
SET descripcion = COALESCE(NULLIF(TRIM(descripcion), ''), 'ITEM SIN DESCRIPCION'),
    cantidad = COALESCE(cantidad, 1),
    unidad_medida = COALESCE(NULLIF(TRIM(unidad_medida), ''), 'unid')
WHERE descripcion IS NULL
   OR TRIM(descripcion) = ''
   OR cantidad IS NULL
   OR unidad_medida IS NULL
   OR TRIM(unidad_medida) = '';

ALTER TABLE public.requisiciones_compra_items ALTER COLUMN requisicion_id SET NOT NULL;
ALTER TABLE public.requisiciones_compra_items ALTER COLUMN descripcion SET NOT NULL;
ALTER TABLE public.requisiciones_compra_items ALTER COLUMN cantidad SET NOT NULL;
ALTER TABLE public.requisiciones_compra_items ALTER COLUMN unidad_medida SET NOT NULL;

CREATE TABLE IF NOT EXISTS public.cotizaciones_compra (
    id bigint GENERATED BY DEFAULT AS IDENTITY PRIMARY KEY,
    sucursal_id bigint NOT NULL,
    requisicion_id bigint,
    proveedor_id bigint NOT NULL,
    fecha date NOT NULL,
    fecha_vencimiento date,
    total numeric(18,4) NOT NULL DEFAULT 0,
    estado character varying(20) NOT NULL DEFAULT 'PENDIENTE',
    observacion character varying(500),
    created_at timestamp with time zone NOT NULL DEFAULT CURRENT_TIMESTAMP,
    updated_at timestamp with time zone NOT NULL DEFAULT CURRENT_TIMESTAMP,
    deleted_at timestamp with time zone,
    created_by bigint,
    updated_by bigint
);

ALTER TABLE public.cotizaciones_compra ADD COLUMN IF NOT EXISTS sucursal_id bigint;
ALTER TABLE public.cotizaciones_compra ADD COLUMN IF NOT EXISTS requisicion_id bigint;
ALTER TABLE public.cotizaciones_compra ADD COLUMN IF NOT EXISTS proveedor_id bigint;
ALTER TABLE public.cotizaciones_compra ADD COLUMN IF NOT EXISTS fecha date;
ALTER TABLE public.cotizaciones_compra ADD COLUMN IF NOT EXISTS fecha_vencimiento date;
ALTER TABLE public.cotizaciones_compra ADD COLUMN IF NOT EXISTS total numeric(18,4);
ALTER TABLE public.cotizaciones_compra ADD COLUMN IF NOT EXISTS estado character varying(20);
ALTER TABLE public.cotizaciones_compra ADD COLUMN IF NOT EXISTS observacion character varying(500);
ALTER TABLE public.cotizaciones_compra ADD COLUMN IF NOT EXISTS created_at timestamp with time zone;
ALTER TABLE public.cotizaciones_compra ADD COLUMN IF NOT EXISTS updated_at timestamp with time zone;
ALTER TABLE public.cotizaciones_compra ADD COLUMN IF NOT EXISTS deleted_at timestamp with time zone;
ALTER TABLE public.cotizaciones_compra ADD COLUMN IF NOT EXISTS created_by bigint;
ALTER TABLE public.cotizaciones_compra ADD COLUMN IF NOT EXISTS updated_by bigint;

ALTER TABLE public.cotizaciones_compra ALTER COLUMN total SET DEFAULT 0;
ALTER TABLE public.cotizaciones_compra ALTER COLUMN estado SET DEFAULT 'PENDIENTE';
ALTER TABLE public.cotizaciones_compra ALTER COLUMN created_at SET DEFAULT CURRENT_TIMESTAMP;
ALTER TABLE public.cotizaciones_compra ALTER COLUMN updated_at SET DEFAULT CURRENT_TIMESTAMP;

UPDATE public.cotizaciones_compra
SET total = COALESCE(total, 0),
    estado = COALESCE(NULLIF(TRIM(estado), ''), 'PENDIENTE'),
    created_at = COALESCE(created_at, CURRENT_TIMESTAMP),
    updated_at = COALESCE(updated_at, CURRENT_TIMESTAMP)
WHERE total IS NULL
   OR estado IS NULL
   OR TRIM(estado) = ''
   OR created_at IS NULL
   OR updated_at IS NULL;

ALTER TABLE public.cotizaciones_compra ALTER COLUMN sucursal_id SET NOT NULL;
ALTER TABLE public.cotizaciones_compra ALTER COLUMN proveedor_id SET NOT NULL;
ALTER TABLE public.cotizaciones_compra ALTER COLUMN fecha SET NOT NULL;
ALTER TABLE public.cotizaciones_compra ALTER COLUMN total SET NOT NULL;
ALTER TABLE public.cotizaciones_compra ALTER COLUMN estado SET NOT NULL;
ALTER TABLE public.cotizaciones_compra ALTER COLUMN created_at SET NOT NULL;
ALTER TABLE public.cotizaciones_compra ALTER COLUMN updated_at SET NOT NULL;

CREATE TABLE IF NOT EXISTS public.cotizaciones_compra_items (
    id bigint GENERATED BY DEFAULT AS IDENTITY PRIMARY KEY,
    cotizacion_id bigint NOT NULL,
    item_id bigint,
    descripcion character varying(300) NOT NULL,
    cantidad numeric(18,4) NOT NULL,
    precio_unitario numeric(18,4) NOT NULL,
    total numeric(18,4) NOT NULL
);

ALTER TABLE public.cotizaciones_compra_items ADD COLUMN IF NOT EXISTS cotizacion_id bigint;
ALTER TABLE public.cotizaciones_compra_items ADD COLUMN IF NOT EXISTS item_id bigint;
ALTER TABLE public.cotizaciones_compra_items ADD COLUMN IF NOT EXISTS descripcion character varying(300);
ALTER TABLE public.cotizaciones_compra_items ADD COLUMN IF NOT EXISTS cantidad numeric(18,4);
ALTER TABLE public.cotizaciones_compra_items ADD COLUMN IF NOT EXISTS precio_unitario numeric(18,4);
ALTER TABLE public.cotizaciones_compra_items ADD COLUMN IF NOT EXISTS total numeric(18,4);

UPDATE public.cotizaciones_compra_items
SET descripcion = COALESCE(NULLIF(TRIM(descripcion), ''), 'ITEM SIN DESCRIPCION'),
    cantidad = COALESCE(cantidad, 1),
    precio_unitario = COALESCE(precio_unitario, 0),
    total = COALESCE(total, COALESCE(cantidad, 1) * COALESCE(precio_unitario, 0))
WHERE descripcion IS NULL
   OR TRIM(descripcion) = ''
   OR cantidad IS NULL
   OR precio_unitario IS NULL
   OR total IS NULL;

ALTER TABLE public.cotizaciones_compra_items ALTER COLUMN cotizacion_id SET NOT NULL;
ALTER TABLE public.cotizaciones_compra_items ALTER COLUMN descripcion SET NOT NULL;
ALTER TABLE public.cotizaciones_compra_items ALTER COLUMN cantidad SET NOT NULL;
ALTER TABLE public.cotizaciones_compra_items ALTER COLUMN precio_unitario SET NOT NULL;
ALTER TABLE public.cotizaciones_compra_items ALTER COLUMN total SET NOT NULL;

DO $$
BEGIN
    IF NOT EXISTS (SELECT 1 FROM pg_constraint WHERE conname = 'fk_requisiciones_compra_sucursal') THEN
        ALTER TABLE public.requisiciones_compra
            ADD CONSTRAINT fk_requisiciones_compra_sucursal
            FOREIGN KEY (sucursal_id)
            REFERENCES public.sucursales(id)
            ON DELETE RESTRICT;
    END IF;
END $$;

DO $$
BEGIN
    IF NOT EXISTS (SELECT 1 FROM pg_constraint WHERE conname = 'fk_requisiciones_compra_solicitante') THEN
        ALTER TABLE public.requisiciones_compra
            ADD CONSTRAINT fk_requisiciones_compra_solicitante
            FOREIGN KEY (solicitante_id)
            REFERENCES public.usuarios(id)
            ON DELETE RESTRICT;
    END IF;
END $$;

DO $$
BEGIN
    IF NOT EXISTS (SELECT 1 FROM pg_constraint WHERE conname = 'fk_requisiciones_compra_items_requisicion') THEN
        ALTER TABLE public.requisiciones_compra_items
            ADD CONSTRAINT fk_requisiciones_compra_items_requisicion
            FOREIGN KEY (requisicion_id)
            REFERENCES public.requisiciones_compra(id)
            ON DELETE CASCADE;
    END IF;
END $$;

DO $$
BEGIN
    IF NOT EXISTS (SELECT 1 FROM pg_constraint WHERE conname = 'fk_requisiciones_compra_items_item') THEN
        ALTER TABLE public.requisiciones_compra_items
            ADD CONSTRAINT fk_requisiciones_compra_items_item
            FOREIGN KEY (item_id)
            REFERENCES public.items(id)
            ON DELETE SET NULL;
    END IF;
END $$;

DO $$
BEGIN
    IF NOT EXISTS (SELECT 1 FROM pg_constraint WHERE conname = 'fk_cotizaciones_compra_sucursal') THEN
        ALTER TABLE public.cotizaciones_compra
            ADD CONSTRAINT fk_cotizaciones_compra_sucursal
            FOREIGN KEY (sucursal_id)
            REFERENCES public.sucursales(id)
            ON DELETE RESTRICT;
    END IF;
END $$;

DO $$
BEGIN
    IF NOT EXISTS (SELECT 1 FROM pg_constraint WHERE conname = 'fk_cotizaciones_compra_requisicion') THEN
        ALTER TABLE public.cotizaciones_compra
            ADD CONSTRAINT fk_cotizaciones_compra_requisicion
            FOREIGN KEY (requisicion_id)
            REFERENCES public.requisiciones_compra(id)
            ON DELETE SET NULL;
    END IF;
END $$;

DO $$
BEGIN
    IF NOT EXISTS (SELECT 1 FROM pg_constraint WHERE conname = 'fk_cotizaciones_compra_proveedor') THEN
        ALTER TABLE public.cotizaciones_compra
            ADD CONSTRAINT fk_cotizaciones_compra_proveedor
            FOREIGN KEY (proveedor_id)
            REFERENCES public.terceros(id)
            ON DELETE RESTRICT;
    END IF;
END $$;

DO $$
BEGIN
    IF NOT EXISTS (SELECT 1 FROM pg_constraint WHERE conname = 'fk_cotizaciones_compra_items_cotizacion') THEN
        ALTER TABLE public.cotizaciones_compra_items
            ADD CONSTRAINT fk_cotizaciones_compra_items_cotizacion
            FOREIGN KEY (cotizacion_id)
            REFERENCES public.cotizaciones_compra(id)
            ON DELETE CASCADE;
    END IF;
END $$;

DO $$
BEGIN
    IF NOT EXISTS (SELECT 1 FROM pg_constraint WHERE conname = 'fk_cotizaciones_compra_items_item') THEN
        ALTER TABLE public.cotizaciones_compra_items
            ADD CONSTRAINT fk_cotizaciones_compra_items_item
            FOREIGN KEY (item_id)
            REFERENCES public.items(id)
            ON DELETE SET NULL;
    END IF;
END $$;

DO $$
BEGIN
    IF NOT EXISTS (SELECT 1 FROM pg_constraint WHERE conname = 'ck_requisiciones_compra_estado') THEN
        ALTER TABLE public.requisiciones_compra
            ADD CONSTRAINT ck_requisiciones_compra_estado
            CHECK (upper(estado) IN ('BORRADOR', 'ENVIADA', 'APROBADA', 'RECHAZADA', 'CANCELADA', 'PROCESADA'));
    END IF;
END $$;

DO $$
BEGIN
    IF NOT EXISTS (SELECT 1 FROM pg_constraint WHERE conname = 'ck_requisiciones_compra_items_cantidad') THEN
        ALTER TABLE public.requisiciones_compra_items
            ADD CONSTRAINT ck_requisiciones_compra_items_cantidad
            CHECK (cantidad > 0);
    END IF;
END $$;

DO $$
BEGIN
    IF NOT EXISTS (SELECT 1 FROM pg_constraint WHERE conname = 'ck_cotizaciones_compra_estado') THEN
        ALTER TABLE public.cotizaciones_compra
            ADD CONSTRAINT ck_cotizaciones_compra_estado
            CHECK (upper(estado) IN ('PENDIENTE', 'ACEPTADA', 'RECHAZADA', 'PROCESADA'));
    END IF;
END $$;

DO $$
BEGIN
    IF NOT EXISTS (SELECT 1 FROM pg_constraint WHERE conname = 'ck_cotizaciones_compra_total') THEN
        ALTER TABLE public.cotizaciones_compra
            ADD CONSTRAINT ck_cotizaciones_compra_total
            CHECK (total >= 0);
    END IF;
END $$;

DO $$
BEGIN
    IF NOT EXISTS (SELECT 1 FROM pg_constraint WHERE conname = 'ck_cotizaciones_compra_items_cantidad') THEN
        ALTER TABLE public.cotizaciones_compra_items
            ADD CONSTRAINT ck_cotizaciones_compra_items_cantidad
            CHECK (cantidad > 0);
    END IF;
END $$;

DO $$
BEGIN
    IF NOT EXISTS (SELECT 1 FROM pg_constraint WHERE conname = 'ck_cotizaciones_compra_items_precio_unitario') THEN
        ALTER TABLE public.cotizaciones_compra_items
            ADD CONSTRAINT ck_cotizaciones_compra_items_precio_unitario
            CHECK (precio_unitario >= 0);
    END IF;
END $$;

DO $$
BEGIN
    IF NOT EXISTS (SELECT 1 FROM pg_constraint WHERE conname = 'ck_cotizaciones_compra_items_total') THEN
        ALTER TABLE public.cotizaciones_compra_items
            ADD CONSTRAINT ck_cotizaciones_compra_items_total
            CHECK (total >= 0);
    END IF;
END $$;

CREATE INDEX IF NOT EXISTS ix_requisiciones_compra_sucursal_id ON public.requisiciones_compra (sucursal_id);
CREATE INDEX IF NOT EXISTS ix_requisiciones_compra_solicitante_id ON public.requisiciones_compra (solicitante_id);
CREATE INDEX IF NOT EXISTS ix_requisiciones_compra_estado ON public.requisiciones_compra (estado);
CREATE INDEX IF NOT EXISTS ix_requisiciones_compra_items_requisicion_id ON public.requisiciones_compra_items (requisicion_id);
CREATE INDEX IF NOT EXISTS ix_requisiciones_compra_items_item_id ON public.requisiciones_compra_items (item_id);
CREATE INDEX IF NOT EXISTS ix_cotizaciones_compra_sucursal_id ON public.cotizaciones_compra (sucursal_id);
CREATE INDEX IF NOT EXISTS ix_cotizaciones_compra_requisicion_id ON public.cotizaciones_compra (requisicion_id);
CREATE INDEX IF NOT EXISTS ix_cotizaciones_compra_proveedor_id ON public.cotizaciones_compra (proveedor_id);
CREATE INDEX IF NOT EXISTS ix_cotizaciones_compra_estado ON public.cotizaciones_compra (estado);
CREATE INDEX IF NOT EXISTS ix_cotizaciones_compra_items_cotizacion_id ON public.cotizaciones_compra_items (cotizacion_id);
CREATE INDEX IF NOT EXISTS ix_cotizaciones_compra_items_item_id ON public.cotizaciones_compra_items (item_id);

COMMIT;

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

    v_proveedor_compra bigint;
    v_item_compra_1 bigint;
    v_item_compra_2 bigint;
    v_req_min bigint;
    v_req_full bigint;
    v_cot_min bigint;
    v_cot_full bigint;
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
        RAISE EXCEPTION 'Seed local: no existe un usuario activo en la tabla usuarios.';
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
        RAISE EXCEPTION 'Seed local: no existe una sucursal activa en la tabla sucursales.';
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
        RAISE EXCEPTION 'Seed local: se requieren al menos 2 registros en la tabla terceros.';
    END IF;

    -- =============================================================
    -- CRM - ejemplos mínimos y completos
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

    -- =============================================================
    -- Compras - ejemplos mínimos y completos
    -- =============================================================

    SELECT id INTO v_proveedor_compra
    FROM "terceros"
    ORDER BY COALESCE("es_proveedor", FALSE) DESC, id
    LIMIT 1;

    IF v_proveedor_compra IS NULL THEN
        RAISE EXCEPTION 'Compras seed: no existe un tercero utilizable como proveedor.';
    END IF;

    SELECT id INTO v_item_compra_1
    FROM "items"
    WHERE "activo" = TRUE
    ORDER BY id
    LIMIT 1;

    SELECT id INTO v_item_compra_2
    FROM "items"
    WHERE "activo" = TRUE
      AND id <> COALESCE(v_item_compra_1, -1)
    ORDER BY id
    LIMIT 1;

    IF v_item_compra_1 IS NULL THEN
        RAISE EXCEPTION 'Compras seed: no existe al menos un item activo en la tabla items.';
    END IF;

    IF v_item_compra_2 IS NULL THEN
        v_item_compra_2 := v_item_compra_1;
    END IF;

    SELECT id INTO v_req_min
    FROM public.requisiciones_compra
    WHERE descripcion = 'ZZ REQUISICION COMPRA MINIMA'
      AND sucursal_id = v_sucursal_1
      AND solicitante_id = v_usuario_1
    LIMIT 1;

    IF v_req_min IS NULL THEN
        INSERT INTO public.requisiciones_compra (
            sucursal_id,
            solicitante_id,
            fecha,
            descripcion,
            estado,
            observacion,
            created_at,
            updated_at,
            created_by,
            updated_by
        )
        VALUES (
            v_sucursal_1,
            v_usuario_1,
            CURRENT_DATE,
            'ZZ REQUISICION COMPRA MINIMA',
            'BORRADOR',
            'Requisicion minima de ejemplo para validar el circuito de compras.',
            CURRENT_TIMESTAMP,
            CURRENT_TIMESTAMP,
            v_usuario_1,
            v_usuario_1
        )
        RETURNING id INTO v_req_min;
    END IF;

    INSERT INTO public.requisiciones_compra_items (
        requisicion_id,
        item_id,
        descripcion,
        cantidad,
        unidad_medida,
        observacion
    )
    SELECT
        v_req_min,
        v_item_compra_1,
        'ZZ ITEM REQUISICION MINIMA',
        2,
        'unid',
        'Linea minima de requisicion de compras.'
    WHERE NOT EXISTS (
        SELECT 1
        FROM public.requisiciones_compra_items
        WHERE requisicion_id = v_req_min
          AND descripcion = 'ZZ ITEM REQUISICION MINIMA'
    );

    SELECT id INTO v_req_full
    FROM public.requisiciones_compra
    WHERE descripcion = 'ZZ REQUISICION COMPRA COMPLETA'
      AND sucursal_id = v_sucursal_1
      AND solicitante_id = v_usuario_2
    LIMIT 1;

    IF v_req_full IS NULL THEN
        INSERT INTO public.requisiciones_compra (
            sucursal_id,
            solicitante_id,
            fecha,
            descripcion,
            estado,
            observacion,
            created_at,
            updated_at,
            created_by,
            updated_by
        )
        VALUES (
            v_sucursal_1,
            v_usuario_2,
            CURRENT_DATE - 2,
            'ZZ REQUISICION COMPRA COMPLETA',
            'PROCESADA',
            'Requisicion completa de ejemplo para alimentar cotizaciones y consultas de compras.',
            CURRENT_TIMESTAMP,
            CURRENT_TIMESTAMP,
            v_usuario_1,
            v_usuario_2
        )
        RETURNING id INTO v_req_full;
    END IF;

    INSERT INTO public.requisiciones_compra_items (
        requisicion_id,
        item_id,
        descripcion,
        cantidad,
        unidad_medida,
        observacion
    )
    SELECT
        v_req_full,
        v_item_compra_1,
        'ZZ ITEM REQUISICION COMPLETA A',
        5,
        'unid',
        'Primera linea completa de requisicion.'
    WHERE NOT EXISTS (
        SELECT 1
        FROM public.requisiciones_compra_items
        WHERE requisicion_id = v_req_full
          AND descripcion = 'ZZ ITEM REQUISICION COMPLETA A'
    );

    INSERT INTO public.requisiciones_compra_items (
        requisicion_id,
        item_id,
        descripcion,
        cantidad,
        unidad_medida,
        observacion
    )
    SELECT
        v_req_full,
        v_item_compra_2,
        'ZZ ITEM REQUISICION COMPLETA B',
        8,
        'unid',
        'Segunda linea completa de requisicion.'
    WHERE NOT EXISTS (
        SELECT 1
        FROM public.requisiciones_compra_items
        WHERE requisicion_id = v_req_full
          AND descripcion = 'ZZ ITEM REQUISICION COMPLETA B'
    );

    SELECT id INTO v_cot_min
    FROM public.cotizaciones_compra
    WHERE requisicion_id = v_req_min
      AND proveedor_id = v_proveedor_compra
      AND observacion = 'ZZ COTIZACION COMPRA MINIMA'
    LIMIT 1;

    IF v_cot_min IS NULL THEN
        INSERT INTO public.cotizaciones_compra (
            sucursal_id,
            requisicion_id,
            proveedor_id,
            fecha,
            fecha_vencimiento,
            total,
            estado,
            observacion,
            created_at,
            updated_at,
            created_by,
            updated_by
        )
        VALUES (
            v_sucursal_1,
            v_req_min,
            v_proveedor_compra,
            CURRENT_DATE,
            CURRENT_DATE + 7,
            0,
            'PENDIENTE',
            'ZZ COTIZACION COMPRA MINIMA',
            CURRENT_TIMESTAMP,
            CURRENT_TIMESTAMP,
            v_usuario_1,
            v_usuario_1
        )
        RETURNING id INTO v_cot_min;
    END IF;

    INSERT INTO public.cotizaciones_compra_items (
        cotizacion_id,
        item_id,
        descripcion,
        cantidad,
        precio_unitario,
        total
    )
    SELECT
        v_cot_min,
        v_item_compra_1,
        'ZZ ITEM COTIZACION MINIMA',
        2,
        150.00,
        300.00
    WHERE NOT EXISTS (
        SELECT 1
        FROM public.cotizaciones_compra_items
        WHERE cotizacion_id = v_cot_min
          AND descripcion = 'ZZ ITEM COTIZACION MINIMA'
    );

    UPDATE public.cotizaciones_compra
    SET total = COALESCE((
        SELECT SUM(total)
        FROM public.cotizaciones_compra_items
        WHERE cotizacion_id = v_cot_min
    ), 0),
        updated_at = CURRENT_TIMESTAMP,
        updated_by = v_usuario_1
    WHERE id = v_cot_min;

    SELECT id INTO v_cot_full
    FROM public.cotizaciones_compra
    WHERE requisicion_id = v_req_full
      AND proveedor_id = v_proveedor_compra
      AND observacion = 'ZZ COTIZACION COMPRA COMPLETA'
    LIMIT 1;

    IF v_cot_full IS NULL THEN
        INSERT INTO public.cotizaciones_compra (
            sucursal_id,
            requisicion_id,
            proveedor_id,
            fecha,
            fecha_vencimiento,
            total,
            estado,
            observacion,
            created_at,
            updated_at,
            created_by,
            updated_by
        )
        VALUES (
            v_sucursal_1,
            v_req_full,
            v_proveedor_compra,
            CURRENT_DATE - 1,
            CURRENT_DATE + 14,
            0,
            'ACEPTADA',
            'ZZ COTIZACION COMPRA COMPLETA',
            CURRENT_TIMESTAMP,
            CURRENT_TIMESTAMP,
            v_usuario_1,
            v_usuario_2
        )
        RETURNING id INTO v_cot_full;
    END IF;

    INSERT INTO public.cotizaciones_compra_items (
        cotizacion_id,
        item_id,
        descripcion,
        cantidad,
        precio_unitario,
        total
    )
    SELECT
        v_cot_full,
        v_item_compra_1,
        'ZZ ITEM COTIZACION COMPLETA A',
        5,
        220.50,
        1102.50
    WHERE NOT EXISTS (
        SELECT 1
        FROM public.cotizaciones_compra_items
        WHERE cotizacion_id = v_cot_full
          AND descripcion = 'ZZ ITEM COTIZACION COMPLETA A'
    );

    INSERT INTO public.cotizaciones_compra_items (
        cotizacion_id,
        item_id,
        descripcion,
        cantidad,
        precio_unitario,
        total
    )
    SELECT
        v_cot_full,
        v_item_compra_2,
        'ZZ ITEM COTIZACION COMPLETA B',
        8,
        95.75,
        766.00
    WHERE NOT EXISTS (
        SELECT 1
        FROM public.cotizaciones_compra_items
        WHERE cotizacion_id = v_cot_full
          AND descripcion = 'ZZ ITEM COTIZACION COMPLETA B'
    );

    UPDATE public.cotizaciones_compra
    SET total = COALESCE((
        SELECT SUM(total)
        FROM public.cotizaciones_compra_items
        WHERE cotizacion_id = v_cot_full
    ), 0),
        updated_at = CURRENT_TIMESTAMP,
        updated_by = v_usuario_2
    WHERE id = v_cot_full;
END $$;

COMMIT;