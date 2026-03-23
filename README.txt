# ZuluIA_Back

Backend del sistema de gestión empresarial ZuluIA, desarrollado en **.NET 8** siguiendo los principios de **Clean Architecture**, **CQRS** y **Domain-Driven Design**.

---

## 📋 Índice

- [Tecnologías](#tecnologías)
- [Arquitectura](#arquitectura)
- [Estructura del proyecto](#estructura-del-proyecto)
- [Requisitos previos](#requisitos-previos)
- [Configuración inicial](#configuración-inicial)
- [Variables de entorno](#variables-de-entorno)
- [Ejecutar el proyecto](#ejecutar-el-proyecto)
- [Ejecutar con Docker](#ejecutar-con-docker)
- [Tests](#tests)
- [Endpoints principales](#endpoints-principales)
- [Módulos implementados](#módulos-implementados)
- [Convenciones de código](#convenciones-de-código)
- [Contribución](#contribución)

---

## 🛠 Tecnologías

| Tecnología | Versión | Uso |
|---|---|---|
| .NET | 8.0 | Framework principal |
| ASP.NET Core | 8.0 | Web API |
| Entity Framework Core | 8.0 | ORM principal |
| Npgsql | 8.0 | Driver PostgreSQL |
| Dapper | 2.1 | Queries de lectura complejas |
| MediatR | 12.2 | CQRS / Mediator |
| FluentValidation | 11.9 | Validaciones |
| AutoMapper | 13.0 | Mapeo entidad → DTO |
| Serilog | 8.0 | Logging estructurado |
| Supabase | - | Base de datos + Auth (JWT) |
| xUnit | 2.6 | Testing |
| FluentAssertions | 6.12 | Assertions en tests |
| NetArchTest | 1.3 | Architecture tests |
| NSubstitute | 5.1 | Mocking |
| Docker | - | Containerización |

---

## 🏗 Arquitectura

El proyecto sigue **Clean Architecture** con 4 capas:

```
┌─────────────────────────────────────┐
│           ZuluIA_Back.Api           │  ← Controllers, Middleware
├───────────────────────────────────   ─┤
│      ZuluIA_Back.Infrastructure     │  ← EF Core, Repositories, Servicios externos
├─────────────────────────────────────┤
│       ZuluIA_Back.Application       │  ← CQRS, Commands, Queries, DTOs, Validators
├─────────────────────────────────────┤
│         ZuluIA_Back.Domain          │  ← Entidades, Value Objects, Domain Events
└─────────────────────────────────────┘
```

### Flujo de dependencias

```
Api → Application → Domain
Api → Infrastructure → Application → Domain
```

> ⚠️ **Domain no depende de nadie**. Es el núcleo del sistema.

### Patrón CQRS

```
HTTP Request
    │
    ▼
Controller
    │
    ▼
MediatR.Send(Command / Query)
    │
    ├── Command → CommandHandler → Repository → UnitOfWork → DB
    └── Query   → QueryHandler  → Repository → DB → DTO
```

---

## 📁 Estructura del proyecto

```
ZuluIA_Back/
├── .env                          ← Variables de entorno (NO subir a git)
├── .env.example                  ← Plantilla de variables
├── .gitignore
├── docker-compose.yml
├── Dockerfile
├── README.md
├── ZuluIA_Back.sln
│
├── src/
│   ├── ZuluIA_Back.Domain/
│   │   ├── Common/               ← BaseEntity, AuditableEntity, Result<T>, ValueObject
│   │   ├── Entities/             ← Entidades del dominio
│   │   │   ├── Comprobantes/
│   │   │   ├── Contabilidad/
│   │   │   ├── Finanzas/
│   │   │   ├── Geografia/
│   │   │   ├── Items/
│   │   │   ├── Stock/
│   │   │   └── Terceros/
│   │   ├── Enums/                ← Enumeraciones del dominio
│   │   ├── Events/               ← Domain Events
│   │   ├── Interfaces/           ← Contratos de repositorios
│   │   └── ValueObjects/         ← Domicilio, Dinero, NroComprobante
│   │
│   ├── ZuluIA_Back.Application/
│   │   ├── Common/
│   │   │   ├── Behaviors/        ← ValidationBehavior, LoggingBehavior
│   │   │   ├── Interfaces/       ← ICurrentUserService, IApplicationDbContext
│   │   │   └── Mappings/         ← AutoMapper profiles
│   │   ├── DependencyInjection.cs
│   │   └── Features/             ← Módulos organizados por feature
│   │       ├── Cobros/
│   │       ├── Comprobantes/
│   │       ├── Contabilidad/
│   │       ├── Items/
│   │       ├── Pagos/
│   │       └── Terceros/
│   │
│   ├── ZuluIA_Back.Infrastructure/
│   │   ├── DependencyInjection.cs
│   │   ├── Persistence/
│   │   │   ├── AppDbContext.cs
│   │   │   ├── UnitOfWork.cs
│   │   │   ├── Configurations/   ← EF Core entity configurations
│   │   │   └── Repositories/     ← Implementaciones de repositorios
│   │   └── Services/             ← DomainEventDispatcher, CurrentUserService
│   │
│   └── ZuluIA_Back.Api/
│       ├── Program.cs
│       ├── appsettings.json
│       ├── appsettings.Development.json
│       ├── Controllers/          ← API Controllers
│       └── Middleware/           ← ExceptionMiddleware, CurrentUserMiddleware
│
└── tests/
    ├── ZuluIA_Back.UnitTests/
    │   ├── Domain/               ← Tests de entidades y value objects
    │   └── Application/          ← Tests de validators
    └── ZuluIA_Back.Architecture.Tests/
        ├── Helpers/              ← AssemblyReferences
        ├── DomainLayerTests.cs
        ├── ApplicationLayerTests.cs
        ├── InfrastructureLayerTests.cs
        ├── ApiLayerTests.cs
        ├── NamingConventionTests.cs
        ├── DependencyFlowTests.cs
        ├── EncapsulationTests.cs
        └── CircularDependencyTests.cs
```

---

## ✅ Requisitos previos

- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- [Docker Desktop](https://www.docker.com/products/docker-desktop/) _(opcional)_
- Cuenta en [Supabase](https://supabase.com/) con el proyecto configurado
- Variables de entorno configuradas (ver sección siguiente)

---

## Endpoints principales

Comprobantes Paraguay / SIFEN:

- `GET /api/comprobantes/{id}`: detalle completo del comprobante; para Paraguay tambien expone `timbradoId`, `nroTimbrado`, `estadoSifen`, `trackingId`, `cdc`, `numeroLote`, `codigoRespuesta`, `mensajeRespuesta` y las banderas `tieneIdentificadoresSifen`, `puedeReintentarSifen`, `puedeConciliarSifen`.
- `GET /api/comprobantes/{id}/resumen`: vista resumida del comprobante con los mismos campos operativos SIFEN y las banderas `tieneIdentificadoresSifen`, `puedeReintentarSifen`, `puedeConciliarSifen`.
- `GET /api/comprobantes/{id}/paraguay/sifen-preview`: valida y arma el paquete previo al envio.
- `GET /api/comprobantes/{id}/paraguay/sifen-estado`: devuelve el ultimo estado SIFEN persistido sobre el comprobante.
- `GET /api/comprobantes/{id}/paraguay/sifen-historial`: devuelve el historial de intentos y respuestas persistidas.
- `GET /api/comprobantes/paraguay/sifen/pendientes`: lista comprobantes Paraguay emitidos aun no aceptados, con filtros operativos por sucursal, estado, codigo de respuesta, reintento, rango de fechas, ordenamiento e identificadores.
- `GET /api/comprobantes/paraguay/sifen/pendientes/resumen`: devuelve totales agregados de la bandeja, incluyendo reintentables, casos con identificadores, conciliables y distribucion por estado SIFEN.
- `POST /api/comprobantes/{id}/paraguay/sifen`: envia el comprobante al integrador configurado.
- `POST /api/comprobantes/{id}/paraguay/sifen/conciliar`: consulta el estado actual del integrador usando TrackingId, CDC o numero de lote persistidos.
- `POST /api/comprobantes/paraguay/sifen/conciliar-pendientes`: concilia en lote comprobantes Paraguay emitidos aun no aceptados, usando identificadores persistidos y filtros por sucursal, estado, codigo de respuesta, reintento y rango de fechas; el resultado informa `totalElegibles` y `hayMasResultados` ademas del lote efectivamente procesado.
- `POST /api/comprobantes/{id}/paraguay/sifen/reintentar`: reintenta el envio cuando el ultimo estado fue rechazo o error.

Los endpoints de estado, detalle e historial exponen tambien `cdc`, `numeroLote`, `codigoRespuesta` y `mensajeRespuesta` cuando el integrador los devuelve.

Tareas / Proyectos:

- `PATCH /api/tareas/estimadas/{id}/desactivar`: desactiva una tarea estimada activa.
- `PATCH /api/tareas/estimadas/{id}/activar`: reactiva una tarea estimada previamente desactivada.

Tasas de interes:

- `PATCH /api/tasas-interes/{id}/activar?userId={userId}`: reactiva una tasa de interes previamente desactivada y registra auditoria opcional por usuario.
- `PATCH /api/tasas-interes/{id}/desactivar?userId={userId}`: desactiva una tasa de interes activa y registra auditoria opcional por usuario.
- `POST /api/tasas-interes/{id}/desactivar?userId={userId}`: alias legacy mantenido por compatibilidad para clientes existentes.

Menu:

- `GET /api/menu`: devuelve el arbol completo de menu activo.
- `GET /api/menu/{id}`: devuelve un item puntual de menu por identificador.
- `POST /api/menu`: crea un item de menu.
- `PUT /api/menu/{id}`: actualiza un item existente.
- `DELETE /api/menu/{id}`: desactiva un item de menu.
- `PATCH /api/menu/{id}/activar`: reactiva un item de menu previamente desactivado.

---

## ⚙️ Configuración inicial

### 1. Clonar el repositorio

```bash
git clone https://github.com/TU_USUARIO/ZuluIA_Back.git
cd ZuluIA_Back
```

### 2. Crear el archivo `.env`

```bash
cp .env.example .env
```

Editá `.env` con tus credenciales reales de Supabase.

### 3. Restaurar dependencias

```bash
dotnet restore
```

### 4. Compilar

```bash
dotnet build
```

### 5. Aplicar cambios manuales de esquema si corresponde

Actualmente el repositorio no incluye migraciones EF Core versionadas. Cuando una
feature agrega tablas nuevas, el cambio de esquema debe aplicarse manualmente en
PostgreSQL con los scripts de `docs/sql`.

Para el desglose persistido de tributos/percepciones por comprobante ejecutar:

```sql
docs/sql/2026-03-20-add-comprobantes-tributos.sql
```

Para persistir el timbrado aplicado sobre comprobantes Paraguay ejecutar:

```sql
docs/sql/2026-03-20-add-comprobante-timbrado-columns.sql
```

Para persistir el estado y tracking de envios SIFEN Paraguay ejecutar:

```sql
docs/sql/2026-03-20-add-comprobante-sifen-columns.sql
```

Para persistir el historial operativo de intentos y respuestas SIFEN ejecutar:

```sql
docs/sql/2026-03-20-add-comprobante-sifen-history.sql
```

Para persistir identificadores explicitos devueltos por el integrador SIFEN, como CDC y numero de lote, ejecutar:

```sql
docs/sql/2026-03-20-add-comprobante-sifen-provider-identifiers.sql
```

Para persistir campos estructurados de respuesta SIFEN, como codigo y mensaje de rechazo/error, ejecutar:

```sql
docs/sql/2026-03-20-add-comprobante-sifen-response-fields.sql
```

---

## 🔐 Variables de entorno

Copiá `.env.example` como `.env` y completá los valores:

```bash
# Entorno
ASPNETCORE_ENVIRONMENT=Development

# Base de datos (Supabase)
DB_CONNECTION_STRING=postgresql://postgres:TU_PASSWORD@db.TU_PROJECT_ID.supabase.co:5432/postgres

# Supabase
SUPABASE_URL=https://TU_PROJECT_ID.supabase.co
SUPABASE_ANON_KEY=TU_ANON_KEY
SUPABASE_SERVICE_ROLE_KEY=TU_SERVICE_ROLE_KEY
SUPABASE_JWT_SECRET=TU_JWT_SECRET

# AFIP WSFE / WSAA (opcional)
AFIP_WSFE_ENABLED=false
AFIP_WSFE_BASE_URL=https://wswhomo.afip.gov.ar/wsfev1/service.asmx
AFIP_WSFE_CUIT=20123456789
AFIP_WSFE_USE_WSAA=true

# Si se usan credenciales manuales en lugar de WSAA
AFIP_WSFE_TOKEN=
AFIP_WSFE_SIGN=
AFIP_WSFE_TOKEN_EXPIRATION=2026-03-19T18:30:00-03:00

AFIP_WSAA_ENABLED=false
AFIP_WSAA_BASE_URL=https://wsaahomo.afip.gov.ar/ws/services/LoginCms
AFIP_WSAA_CERTIFICATE_PATH=
AFIP_WSAA_CERTIFICATE_PASSWORD=

# Paraguay SIFEN / SET (opcional)
PARAGUAY_SIFEN_ENABLED=false
PARAGUAY_SIFEN_BASE_URL=https://sifen.test/api
PARAGUAY_SIFEN_STATUS_URL=https://sifen.test/api/estado
PARAGUAY_SIFEN_API_KEY=
PARAGUAY_SIFEN_API_SECRET=
PARAGUAY_SIFEN_ENVIRONMENT=test
PARAGUAY_SIFEN_TRANSPORT_MODE=stub
AFIP_WSAA_SERVICE=wsfe

# JWT
JWT_AUTHORITY=https://TU_PROJECT_ID.supabase.co/auth/v1
JWT_ISSUER=https://TU_PROJECT_ID.supabase.co/auth/v1
JWT_AUDIENCE=authenticated

# CORS (opcional)
CORS_ALLOWED_ORIGINS=http://localhost:3000,https://TU_FRONTEND.vercel.app
```

### ¿Dónde encontrar cada valor en Supabase?

| Variable | Ubicación en Supabase Dashboard |
|---|---|
| `DB_CONNECTION_STRING` | Settings → Database → Connection string (URI) |
| `SUPABASE_URL` | Settings → API → Project URL |
| `SUPABASE_ANON_KEY` | Settings → API → anon public |
| `SUPABASE_SERVICE_ROLE_KEY` | Settings → API → service_role |
| `SUPABASE_JWT_SECRET` | Settings → API → JWT Secret |

> ⚠️ **Nunca subas `.env` a git.** Está incluido en `.gitignore`.

---

## ▶️ Ejecutar el proyecto

### Modo desarrollo

```bash
cd src/ZuluIA_Back.Api
dotnet run
```

La API estará disponible en:
- **Swagger UI**: `http://localhost:5000`
- **Health check**: `http://localhost:5000/health`

### Con hot reload

```bash
dotnet watch run --project src/ZuluIA_Back.Api
```

---

## 🐳 Ejecutar con Docker

### Solo la API

```bash
docker build -t zulusia-back .
docker run -p 8080:8080 --env-file .env zulusia-back
```

### Con docker-compose (API + Seq para logs)

```bash
# Levantar
docker-compose up -d

# Ver logs
docker-compose logs -f api

# Bajar
docker-compose down
```

Servicios disponibles:
| Servicio | URL |
|---|---|
| API | `http://localhost:8080` |
| Swagger | `http://localhost:8080` |
| Seq (logs) | `http://localhost:5341` |

---

## 🧪 Tests

### Ejecutar todos los tests

```bash
dotnet test
```

### Preview SIFEN Paraguay

- `GET /api/comprobantes/{id}/paraguay/sifen-preview` devuelve el paquete fiscal preparado para un comprobante Paraguay.
- `GET /api/comprobantes/{id}/paraguay/sifen-estado` devuelve el ultimo estado persistido de integracion SIFEN sobre el comprobante, incluyendo `tieneIdentificadores`, `puedeReintentar` y `puedeConciliar`.
- `GET /api/comprobantes/paraguay/sifen/pendientes` devuelve una bandeja operativa de comprobantes no aceptados para seguimiento y conciliacion masiva; acepta `puedeReintentar`, `fechaDesde`, `fechaHasta` y `sortBy` (`fechaAsc`, `fechaDesc`, `estadoAsc`, `estadoDesc`, por defecto ultima respuesta desc), y ahora expone `tieneIdentificadores` y `puedeConciliar` por fila.
- `GET /api/comprobantes/paraguay/sifen/pendientes/export` descarga la misma bandeja filtrada en CSV para seguimiento externo o soporte operativo, incluyendo columnas `tieneIdentificadores`, `puedeReintentar` y `puedeConciliar`.
- `GET /api/comprobantes/paraguay/sifen/pendientes/resumen` usa los mismos filtros operativos de la bandeja y resume `total`, `reintentables`, `conIdentificadores`, `conciliables`, `sinEstadoSifen`, el conteo por estado y los `codigoRespuesta` y `mensajesRespuesta` mas frecuentes; acepta `topCodigos` y `topMensajes` para limitar esos rankings.
- `GET /api/comprobantes/paraguay/sifen/reintentar-pendientes/preview` devuelve el lote exacto que entraria en el reintento masivo segun filtros, orden y `maxItems`, sin ejecutar el reenvio; tambien informa `totalElegibles`, `hayMasResultados` y un resumen del slice previsto por `estados`, `codigosRespuesta` y `mensajesRespuesta`.
- `POST /api/comprobantes/paraguay/sifen/reintentar-pendientes` reintenta en lote comprobantes Paraguay con ultimo estado rechazado o error, usando filtros por sucursal, estado, codigo de respuesta y rango de fechas; el resultado informa `totalElegibles`, `hayMasResultados`, un resumen por `estados` exitosos y un ranking de `errores` del lote procesado.
- `GET /api/comprobantes/paraguay/sifen/conciliar-pendientes/preview` devuelve el lote exacto que entraria en la conciliacion masiva segun filtros, orden y `maxItems`; solo incluye comprobantes con `trackingId`, `cdc` o `numeroLote` persistidos, e informa `totalElegibles`, `hayMasResultados` y un resumen del slice previsto por `estados`, `codigosRespuesta` y `mensajesRespuesta`.
- `POST /api/comprobantes/paraguay/sifen/conciliar-pendientes` acepta `sucursalId`, `estadoSifen`, `codigoRespuesta`, `puedeReintentar`, `fechaDesde` y `fechaHasta`; internamente sigue exigiendo `trackingId`, `cdc` o `numeroLote` persistidos para poder consultar al integrador, y el resultado informa `totalElegibles`, `hayMasResultados`, un resumen por `estados` exitosos y un ranking de `errores` del lote procesado.
- Los endpoints de estado, detalle e historial exponen `trackingId`, `cdc`, `numeroLote`, `codigoRespuesta` y `mensajeRespuesta` para soporte operativo.
- El endpoint valida estado emitido, timbrado persistido, sucursal/tercero y configuracion `Paraguay:Sifen` o variables `PARAGUAY_SIFEN_*`.
- Esta iteracion no transmite al SET todavia; deja lista y verificable la superficie de integracion.
- `POST /api/comprobantes/{id}/paraguay/sifen` ejecuta el envio contra el integrador configurado y registra auditoria del intento, respuesta exitosa o error.
- `POST /api/comprobantes/{id}/paraguay/sifen/reintentar` reintenta solo si el comprobante no quedo aceptado previamente.
- Si `PARAGUAY_SIFEN_TRANSPORT_MODE=stub`, el envio responde aceptado localmente para pruebas integradas sin dependencia externa.

### Solo Unit Tests

```bash
dotnet test tests/ZuluIA_Back.UnitTests
```

### Solo Architecture Tests

```bash
dotnet test tests/ZuluIA_Back.Architecture.Tests
```

### Con cobertura de código

```bash
dotnet test --collect:"XPlat Code Coverage"
```

### Con output detallado

```bash
dotnet test --verbosity normal --logger "console;verbosity=detailed"
```

---

## 🌐 Endpoints principales

Todos los endpoints requieren autenticación con **Bearer token JWT de Supabase**, excepto los marcados con 🔓.

### Auth
| Método | Endpoint | Descripción |
|---|---|---|
| GET 🔓 | `/api/auth/ping` | Health check de autenticación |
| GET | `/api/auth/me` | Datos del usuario autenticado |

### Terceros
| Método | Endpoint | Descripción |
|---|---|---|
| GET | `/api/terceros` | Listar terceros (paginado) |
| GET | `/api/terceros/{id}` | Obtener tercero por ID |
| POST | `/api/terceros` | Crear tercero |
| PUT | `/api/terceros/{id}` | Actualizar tercero |
| DELETE | `/api/terceros/{id}` | Desactivar tercero |

**Query params para GET /api/terceros:**
```
page=1&pageSize=20&search=texto&soloClientes=true&soloProveedores=false
```

### Items
| Método | Endpoint | Descripción |
|---|---|---|
| GET | `/api/items` | Listar items (paginado) |
| GET | `/api/items/{id}` | Obtener item por ID |
| POST | `/api/items` | Crear item |
| PUT | `/api/items/{id}` | Actualizar item |
| DELETE | `/api/items/{id}` | Desactivar item |

### Comprobantes
| Método | Endpoint | Descripción |
|---|---|---|
| GET | `/api/comprobantes` | Listar comprobantes (paginado) |
| GET | `/api/comprobantes/{id}` | Obtener comprobante por ID |
| POST | `/api/comprobantes` | Crear comprobante |
| POST | `/api/comprobantes/{id}/emitir` | Emitir comprobante |
| POST | `/api/comprobantes/{id}/anular` | Anular comprobante |

### Cobros
| Método | Endpoint | Descripción |
|---|---|---|
| GET | `/api/cobros/{id}` | Obtener cobro por ID |
| POST | `/api/cobros` | Registrar cobro |
| POST | `/api/cobros/{id}/anular` | Anular cobro |

### Pagos
| Método | Endpoint | Descripción |
|---|---|---|
| POST | `/api/pagos` | Registrar pago |
| POST | `/api/pagos/{id}/anular` | Anular pago |

### Contabilidad
| Método | Endpoint | Descripción |
|---|---|---|
| GET | `/api/contabilidad/asientos` | Listar asientos (paginado) |
| GET | `/api/contabilidad/asientos/{id}` | Obtener asiento por ID |
| POST | `/api/contabilidad/asientos` | Crear asiento manual |

### Stock
| Método | Endpoint | Descripción |
|---|---|---|
| GET | `/api/stock/item/{itemId}` | Stock por ítem en todos los depósitos |
| GET | `/api/stock/item/{itemId}/deposito/{depositoId}` | Saldo en depósito específico |
| GET | `/api/stock/movimientos` | Movimientos de stock (paginado) |

### Geografía 🔓
| Método | Endpoint | Descripción |
|---|---|---|
| GET 🔓 | `/api/geografia/paises` | Listado de países |
| GET 🔓 | `/api/geografia/provincias` | Listado de provincias |
| GET 🔓 | `/api/geografia/localidades` | Listado de localidades |
| GET 🔓 | `/api/geografia/barrios` | Listado de barrios |

### Configuración 🔓
| Método | Endpoint | Descripción |
|---|---|---|
| GET 🔓 | `/api/configuracion/condiciones-iva` | Condiciones IVA |
| GET 🔓 | `/api/configuracion/tipos-documento` | Tipos de documento |

---

## 📦 Módulos implementados

| Módulo | Commands | Queries | Events |
|---|---|---|---|
| Terceros | Crear, Actualizar, Desactivar | GetById, GetPaged | TerceroCreado, TerceroDesactivado |
| Items | Crear, Actualizar, Desactivar | GetById, GetPaged | ItemCreado, PrecioActualizado |
| Stock | — | GetByItem, GetSaldo, GetMovimientos | StockAjustado |
| Comprobantes | Crear, Emitir, Anular | GetById, GetPaged | ComprobanteEmitido, ComprobanteAnulado |
| Cobros | Crear, Anular | GetById | CobroRegistrado, CobroAnulado |
| Pagos | Crear, Anular | — | PagoRegistrado, PagoAnulado |
| Contabilidad | CrearAsiento | GetById, GetPaged | — |
| Geografía | — | GetPaises, GetProvincias, GetLocalidades, GetBarrios | — |

---

## 📐 Convenciones de código

### Entidades del dominio
- Constructor **privado** sin parámetros (para EF Core)
- Método estático `Crear(...)` como fábrica
- Setters **privados** o `private set`
- Lógica de negocio **dentro** de la entidad
- Domain Events para efectos secundarios

```csharp
// ✅ Correcto
public class Tercero : AuditableEntity
{
    public string Legajo { get; private set; }

    private Tercero() { }

    public static Tercero Crear(string legajo, ...) { ... }
    public void Desactivar() { ... }
}

// ❌ Incorrecto
public class Tercero
{
    public string Legajo { get; set; }  // setter público
    public Tercero() { }               // constructor público
}
```

### Commands y Queries
- Usar `record` para inmutabilidad
- Terminar en `Command` o `Query`
- Un archivo por Command/Query + Handler

```csharp
// ✅ Correcto
public record CreateTerceroCommand(...) : IRequest<Result<long>>;
public record GetTerceroByIdQuery(long Id) : IRequest<TerceroDto?>;
```

### Handlers
- Un Handler por archivo
- Inyección por constructor (primary constructor)
- No más de 5 dependencias

```csharp
// ✅ Correcto
public class CreateTerceroCommandHandler(
    ITerceroRepository repo,
    IUnitOfWork        uow,
    ICurrentUserService currentUser)
    : IRequestHandler<CreateTerceroCommand, Result<long>>
```

### Interfaces
- Siempre empezar con `I`
- Definir en `Domain.Interfaces` o `Application.Common.Interfaces`
- Implementar en `Infrastructure`

---

## 🤝 Contribución

### Flujo de trabajo

```bash
# 1. Crear rama desde main
git checkout -b feature/nombre-feature

# 2. Desarrollar y commitear
git add .
git commit -m "feat: descripción del cambio"

# 3. Correr tests antes de pushear
dotnet test

# 4. Push y Pull Request
git push origin feature/nombre-feature
```

### Convención de commits

```
feat:     Nueva funcionalidad
fix:      Corrección de bug
refactor: Refactorización sin cambio de comportamiento
test:     Agregar o modificar tests
docs:     Cambios en documentación
chore:    Cambios de configuración o dependencias
```

### Checklist antes de un PR

- [ ] Todos los tests pasan (`dotnet test`)
- [ ] No hay warnings de compilación
- [ ] Se agregaron tests para la nueva funcionalidad
- [ ] Los architecture tests pasan
- [ ] El código sigue las convenciones del proyecto
- [ ] Se actualizó el README si corresponde

---

## 📄 Licencia

Proyecto privado — ZuluIA © 2026
