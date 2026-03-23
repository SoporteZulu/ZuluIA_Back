ALTER TABLE comprobantes
    ADD COLUMN IF NOT EXISTS timbrado_id bigint NULL;

ALTER TABLE comprobantes
    ADD COLUMN IF NOT EXISTS nro_timbrado varchar(50) NULL;

CREATE INDEX IF NOT EXISTS ix_comprobantes_timbrado_id
    ON comprobantes(timbrado_id);