# SQL document: `zuluia_back_local_auth_upgrade`

Migrado desde `zuluia_back_local_auth_upgrade.sql` para ejecución manual sobre PostgreSQL local.

```sql
BEGIN;

ALTER TABLE IF EXISTS usuarios
    ADD COLUMN IF NOT EXISTS password_hash character varying(500);

UPDATE usuarios
SET password_hash = 'pbkdf2-sha1$100000$4JAptect0dgmxQO2pOs7Fw==$2zW1nSgilRNVzcZ8x9glyWEBfHkgu4/coU2E5QF0i9s='
WHERE usuario = 'admin.local'
  AND (password_hash IS NULL OR btrim(password_hash) = '');

COMMIT;

```
