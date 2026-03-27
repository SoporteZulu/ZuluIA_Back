alter table if exists comprobantes
    add column if not exists sifen_cdc varchar(100),
    add column if not exists sifen_numero_lote varchar(100);

create index if not exists ix_comprobantes_sifen_cdc
    on comprobantes (sifen_cdc);

do $$
begin
    if exists (
        select 1
        from information_schema.tables
        where table_schema = current_schema()
          and table_name = 'historial_sifen_comprobantes'
    ) then
        execute 'alter table historial_sifen_comprobantes
            add column if not exists cdc varchar(100),
            add column if not exists numero_lote varchar(100)';

        execute 'create index if not exists ix_historial_sifen_comprobantes_cdc
            on historial_sifen_comprobantes (cdc)';
    end if;
end $$;