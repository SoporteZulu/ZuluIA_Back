CREATE TABLE IF NOT EXISTS comprobantes_tributos (
    id bigserial PRIMARY KEY,
    comprobante_id bigint NOT NULL,
    impuesto_id bigint NULL,
    codigo varchar(20) NOT NULL,
    descripcion varchar(200) NOT NULL,
    base_imponible numeric(18,4) NOT NULL DEFAULT 0,
    alicuota numeric(10,4) NOT NULL DEFAULT 0,
    importe numeric(18,4) NOT NULL DEFAULT 0,
    orden integer NOT NULL DEFAULT 0,
    CONSTRAINT fk_comprobantes_tributos_comprobante
        FOREIGN KEY (comprobante_id)
        REFERENCES comprobantes(id)
        ON DELETE CASCADE
);

CREATE INDEX IF NOT EXISTS ix_comprobantes_tributos_comprobante_id
    ON comprobantes_tributos(comprobante_id);

CREATE INDEX IF NOT EXISTS ix_comprobantes_tributos_impuesto_id
    ON comprobantes_tributos(impuesto_id);

CREATE INDEX IF NOT EXISTS ix_comprobantes_tributos_codigo
    ON comprobantes_tributos(codigo);