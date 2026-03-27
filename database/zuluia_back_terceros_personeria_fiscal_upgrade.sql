BEGIN;

ALTER TABLE IF EXISTS terceros
    ADD COLUMN IF NOT EXISTS tipo_personeria character varying(20) NOT NULL DEFAULT 'JURIDICA';

ALTER TABLE IF EXISTS terceros
    ADD COLUMN IF NOT EXISTS nombre character varying(150);

ALTER TABLE IF EXISTS terceros
    ADD COLUMN IF NOT EXISTS apellido character varying(150);

ALTER TABLE IF EXISTS terceros
    ADD COLUMN IF NOT EXISTS es_entidad_gubernamental boolean NOT NULL DEFAULT FALSE;

ALTER TABLE IF EXISTS terceros
    ADD COLUMN IF NOT EXISTS clave_fiscal character varying(50);

ALTER TABLE IF EXISTS terceros
    ADD COLUMN IF NOT EXISTS valor_clave_fiscal character varying(30);

COMMIT;
