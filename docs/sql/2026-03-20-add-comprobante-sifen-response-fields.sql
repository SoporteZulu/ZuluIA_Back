alter table if exists comprobantes
    add column if not exists sifen_codigo_respuesta varchar(100),
    add column if not exists sifen_mensaje_respuesta varchar(500);

create index if not exists ix_comprobantes_sifen_codigo_respuesta
    on comprobantes (sifen_codigo_respuesta);

do $$
begin
    if exists (
        select 1
        from information_schema.tables
        where table_schema = current_schema()
          and table_name = 'historial_sifen_comprobantes'
    ) then
        execute 'alter table historial_sifen_comprobantes
            add column if not exists codigo_respuesta varchar(100),
            add column if not exists mensaje_respuesta varchar(500)';
    end if;
end $$;