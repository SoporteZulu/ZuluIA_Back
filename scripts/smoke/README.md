# Smoke test de `ZuluIA_Back`

## Archivos
- `scripts\smoke\Invoke-ZuluIAApiSmoke.ps1`
- `database\zuluia_back_full_compatible.sql`

## Qué hace
La smoke suite:
- opcionalmente aplica el bootstrap SQL sobre una base PostgreSQL vacía
- opcionalmente aplica `database\zuluia_back_smoke_dataset.sql` para ampliar datos de prueba
- genera un JWT HS256 para `admin.local` usando `sub = 1`
- verifica `/health`, `/health/detailed` y `swagger`
- ejecuta un barrido ampliado de `GET` discoverables desde OpenAPI, incluyendo rutas con `path params` y `query params` resueltos automáticamente
- crea y consulta datos de prueba en flujos principales:
  - cajas
  - ventas
  - cobros
  - compras
  - pagos
  - cheques
  - transferencias
  - tesorería
  - imputaciones
- además ejecuta mutaciones extendidas por API sobre módulos adicionales:
  - sucursales
  - puntos de facturación
  - terceros
  - items
  - contratos
  - diagnósticos
  - fórmulas y producción
  - órdenes de preparación
  - transferencias de depósito
  - carta de porte
  - colegio
  - punto de venta
  - RRHH
- genera un reporte JSON con el detalle de cada request

## Requisitos
- `psql` disponible en la máquina donde corras la prueba
- una base PostgreSQL creada previamente
- el backend `ZuluIA_Back.Api` levantado
- el secreto JWT del ambiente (`SUPABASE_JWT_SECRET` o `-JwtSecret`)

## Caso 1: crear base nueva de prueba y correr smoke
```powershell
powershell -ExecutionPolicy Bypass -File .\scripts\smoke\Invoke-ZuluIAApiSmoke.ps1 `
  -BaseUrl "https://localhost:5001" `
  -ApplyBootstrap `
  -ApplySupplementalDataset `
  -PsqlPath "psql" `
  -DbHost "localhost" `
  -DbPort 5432 `
  -DbName "zuluia_back_test" `
  -DbUser "postgres" `
  -DbPassword "TU_PASSWORD" `
  -JwtSecret "dev-only-dummy-secret-32-chars-min!!"
```

## Caso 2: base ya cargada, solo correr endpoints
```powershell
powershell -ExecutionPolicy Bypass -File .\scripts\smoke\Invoke-ZuluIAApiSmoke.ps1 `
  -BaseUrl "https://localhost:5001" `
  -ApplySupplementalDataset `
  -PsqlPath "psql" `
  -DbHost "localhost" `
  -DbPort 5432 `
  -DbName "zuluia_back_test" `
  -DbUser "postgres" `
  -DbPassword "TU_PASSWORD" `
  -JwtSecret "TU_SUPABASE_JWT_SECRET"
```

## Si ya tenés un bearer token válido
```powershell
powershell -ExecutionPolicy Bypass -File .\scripts\smoke\Invoke-ZuluIAApiSmoke.ps1 `
  -BaseUrl "https://localhost:5001" `
  -PsqlPath "psql" `
  -DbHost "localhost" `
  -DbPort 5432 `
  -DbName "zuluia_back_test" `
  -DbUser "postgres" `
  -DbPassword "TU_PASSWORD" `
  -Token "TU_BEARER"
```

## Orden recomendado de prueba
1. Crear una base vacía.
2. Levantar el backend apuntando a esa base.
3. Ejecutar la smoke suite con `-ApplyBootstrap`.
4. Revisar `artifacts\zuluia-api-smoke-report.json`.
5. Si algo falla, corregir el bootstrap o el endpoint y volver a correr.

## Dataset complementario
El archivo `database\zuluia_back_smoke_dataset.sql` agrega datos para módulos menos cubiertos por el bootstrap base, por ejemplo:
- geografía
- diagnósticos
- contratos
- colegio
- producción
- logística
- RRHH
- fiscal
- integraciones

Conviene usarlo casi siempre en pruebas integrales.

## Qué revisar en el reporte
- `StatusCode`
- `Success`
- `Error`
- `Response`

## Importante
Esta suite es una prueba de humo operativa.
No garantiza cobertura funcional total de todos los casos de negocio.
Sirve para validar rápidamente que la base creada y los endpoints principales arrancan y responden con consistencia.
