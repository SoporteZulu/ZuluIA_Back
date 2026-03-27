ALTER TABLE comprobantes
    ADD COLUMN IF NOT EXISTS estado_sifen varchar(30) NULL,
    ADD COLUMN IF NOT EXISTS sifen_tracking_id varchar(100) NULL,
    ADD COLUMN IF NOT EXISTS sifen_fecha_respuesta timestamptz NULL;

CREATE INDEX IF NOT EXISTS ix_comprobantes_estado_sifen
    ON comprobantes (estado_sifen);

CREATE INDEX IF NOT EXISTS ix_comprobantes_sifen_tracking_id
    ON comprobantes (sifen_tracking_id);