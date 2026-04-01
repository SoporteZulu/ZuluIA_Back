# Guía Práctica: Trabajo en Paralelo - Ventas ZuluIA

**Objetivo:** Instrucciones step-by-step para coordinar 7 equipos trabajando en paralelo en las matrices de paridad de ventas.

---

## 1. SETUP INICIAL (Día 0)

### 1.1 Crear Estructura de Branches

```bash
# En C:\Zulu\ZuluIA_Back
cd C:\Zulu\ZuluIA_Back
git checkout develop
git pull origin develop

# Crear branches por matriz
git checkout -b feature/pedidos
git push -u origin feature/pedidos

git checkout develop
git checkout -b feature/remitos
git push -u origin feature/remitos

git checkout develop
git checkout -b feature/facturas
git push -u origin feature/facturas

git checkout develop
git checkout -b feature/notas-credito
git push -u origin feature/notas-credito

git checkout develop
git checkout -b feature/cobros
git push -u origin feature/cobros

git checkout develop
git checkout -b feature/cheques
git push -u origin feature/cheques

git checkout develop
git checkout -b feature/reportes
git push -u origin feature/reportes
```

```bash
# En C:\Zulu\ZuluIA_Front
cd C:\Zulu\ZuluIA_Front
git checkout develop
git pull origin develop

# Crear branches por matriz (mismo patrón)
git checkout -b feature/pedidos-frontend
git push -u origin feature/pedidos-frontend

git checkout develop
git checkout -b feature/remitos-frontend
git push -u origin feature/remitos-frontend

# ... (repetir para cada módulo)
```

### 1.2 Crear Issues en GitHub

**Usar plantilla por módulo:**

```markdown
# [MATRIZ 1 - PEDIDOS] Backend

## Objetivo
Implementar 100% backend de pedidos con paridad funcional vs zuluApp

## Tareas
### Entidades/Enums ✅
- [x] EstadoPedido enum
- [x] EstadoEntregaItem enum
- [x] PrioridadPedido enum
- [x] Campos pedido en Comprobante
- [ ] Campos entrega en ComprobanteItem (CantidadEntregada, CantidadPendiente, EstadoEntregaItem)

### Comandos
- [ ] CerrarPedidoCommand - Implementar handler completo
- [ ] CerrarPedidosMasivoCommand - Implementar handler completo
- [ ] ActualizarCumplimientoPedidoCommand - Implementar handler completo

### Queries
- [ ] GetPedidosConEstadoQuery - Agregar filtros faltantes (atraso, producto)
- [ ] GetPedidoVinculacionesQuery - Verificar completo
- [ ] GetPedidosExpedicionQuery - CREAR (vista logística)
- [ ] GetPedidosParaCierreQuery - CREAR (filtros zuluApp)

### Endpoints API
- [ ] POST /api/ventas/pedidos
- [ ] GET /api/ventas/pedidos (con todos los filtros)
- [ ] GET /api/ventas/pedidos/{id}
- [ ] PUT /api/ventas/pedidos/{id}/cerrar
- [ ] POST /api/ventas/pedidos/cerrar-masivo

### Validaciones
- [ ] Validar stock antes de confirmar pedido
- [ ] Calcular fecha estimada entrega
- [ ] Alertar si fecha entrega < plazo mínimo

### Tests
- [ ] Unit tests comandos (>80% coverage)
- [ ] Unit tests queries (>80% coverage)
- [ ] Integration tests endpoints

## DoD
- [ ] Todos los checkboxes ✅
- [ ] Tests passing
- [ ] Swagger docs actualizado
- [ ] PR aprobado
- [ ] Mergeado a develop

## Referencia
- docs/ventas-pedidos-paridad-zuluapp.md
- docs/matrices-paridad-ventas-paralelo.md Matriz 1

## Responsable
@dev-backend-senior-1

## Estimación
21 SP (2-3 semanas)
```

**Crear issue similar para cada matriz (9 issues backend + 9 issues frontend = 18 issues totales)**

### 1.3 Crear Proyectos en GitHub

**Opción 1: Project Board por Matriz**
```
Project: Matriz 1 - Pedidos
├─ To Do
├─ In Progress
├─ In Review
└─ Done

(Repetir para cada matriz)
```

**Opción 2: Project Board Único con Filtros**
```
Project: Ventas - Paridad ZuluApp
├─ To Do
├─ In Progress
├─ In Review
└─ Done

Labels:
- matriz-1-pedidos
- matriz-2-remitos
- matriz-3-facturas
- backend
- frontend
- blocker
- needs-review
```

---

## 2. ORGANIZACIÓN DE EQUIPOS

### 2.1 Asignación Sugerida

| Equipo | Matriz | Rol Backend | Rol Frontend | Branch |
|--------|--------|-------------|--------------|--------|
| **A** | Pedidos | Dev Senior | Dev Junior | feature/pedidos |
| **B** | Remitos | Dev Mid | Dev Mid | feature/remitos |
| **C** | Facturas | Dev Senior | Dev Senior | feature/facturas |
| **D** | NC/ND | Dev Mid | Dev Mid | feature/notas-credito |
| **E** | Cobros | Dev Senior | Dev Senior | feature/cobros |
| **F** | Cheques | Dev Junior | Dev Junior | feature/cheques |
| **G** | Reportes | Dev Full Stack | - | feature/reportes |

### 2.2 Slack/Teams Channels

```
#ventas-general - Anuncios generales
#ventas-backend - Coordinación backend
#ventas-frontend - Coordinación frontend
#ventas-sync - Blockers cross-team
#ventas-matriz-1-pedidos - Equipo A
#ventas-matriz-2-remitos - Equipo B
#ventas-matriz-3-facturas - Equipo C
...
```

---

## 3. WORKFLOW DIARIO

### 3.1 Rutina Diaria del Equipo

**09:00 - Daily Standup (15 min)**
- ¿Qué hiciste ayer?
- ¿Qué harás hoy?
- ¿Bloqueantes?

**09:30 - 12:00 - Coding Bloque 1**
- Trabajar en tasks del issue asignado
- Commits pequeños y frecuentes
- Pushear a branch feature al final del bloque

**12:00 - 13:00 - Almuerzo**

**13:00 - 16:30 - Coding Bloque 2**
- Continuar tasks
- Code review de PRs de otros equipos (si aplica)
- Actualizar issue con progreso

**16:30 - 17:00 - Sync Cross-Team (Lunes y Jueves)**
- Revisar dependencias entre matrices
- Resolver blockers
- Coordinar merges

**17:00 - EOD**
- Commit + push final del día
- Actualizar issue GitHub
- Reportar blockers en Slack

### 3.2 Commits y Push Strategy

**Commits frecuentes:**
```bash
# Formato sugerido: [MATRIZ-X] Descripción breve

git commit -m "[PEDIDOS] Agregar EstadoEntregaItem enum"
git commit -m "[PEDIDOS] Implementar CerrarPedidoCommand handler"
git commit -m "[PEDIDOS] Tests CerrarPedidoCommand - casos edge"

# Push al menos 2 veces al día (mediodía y EOD)
git push origin feature/pedidos
```

**Branch protection:**
- ❌ No pushear directamente a `develop` o `main`
- ✅ Siempre trabajar en feature branch
- ✅ PRs requieren al menos 1 aprobación
- ✅ CI debe pasar antes de merge

---

## 4. DEPENDENCIAS Y STUBS

### 4.1 ¿Cuándo usar Stubs?

**Ejemplo:** Equipo C (Facturas) necesita `remitosApi.getParaFacturar()` pero Equipo B (Remitos) aún no lo implementó.

**Solución: Crear stub temporal**

```typescript
// C:\Zulu\ZuluIA_Front\lib\api\stubs\remitos-stub.ts
export const remitosStub = {
  getParaFacturar: async (terceroId: number) => {
    // Simular delay de red
    await new Promise(resolve => setTimeout(resolve, 500));
    
    // Retornar datos mock
    return {
      success: true,
      data: [
        {
          id: 1,
          numero: '0001-00000123',
          fecha: '2026-03-15',
          tercero: 'Cliente Demo SA',
          total: 15000.00,
          items: [
            { id: 1, descripcion: 'Producto A', cantidad: 10, total: 10000 },
            { id: 2, descripcion: 'Producto B', cantidad: 5, total: 5000 }
          ]
        },
        {
          id: 2,
          numero: '0001-00000124',
          fecha: '2026-03-16',
          tercero: 'Cliente Demo SA',
          total: 8500.00,
          items: [
            { id: 3, descripcion: 'Producto C', cantidad: 8, total: 8500 }
          ]
        }
      ]
    };
  }
};
```

**Usar stub temporalmente:**
```typescript
// C:\Zulu\ZuluIA_Front\app\ventas\facturas\desde-remitos\page.tsx

// TEMPORAL (hasta que Equipo B entregue)
import { remitosStub as remitosApi } from '@/lib/api/stubs/remitos-stub';

// FINAL (cuando Equipo B entregue)
// import { remitosApi } from '@/lib/api/remitos';

export default function FacturarDesdeRemitosPage() {
  const [remitos, setRemitos] = useState([]);

  useEffect(() => {
    const fetchRemitos = async () => {
      const result = await remitosApi.getParaFacturar(clienteId);
      setRemitos(result.data);
    };
    fetchRemitos();
  }, [clienteId]);

  // ...resto del componente
}
```

**Eliminar stub cuando API real esté lista:**
```bash
# Equipo B entrega remitosApi.getParaFacturar()
# Equipo C cambia import y elimina stub

git rm lib/api/stubs/remitos-stub.ts
git commit -m "[FACTURAS] Reemplazar stub remitos con API real"
```

### 4.2 Coordinar Entrega de Dependencias

**Calendario de dependencias:**

| Semana | Entrega | Dependiente | Acción |
|--------|---------|-------------|--------|
| 2 | Equipo A: `pedidosApi.getAll()` | Equipo B, C | Equipo B/C eliminar stubs |
| 3 | Equipo B: `remitosApi.getParaFacturar()` | Equipo C | Equipo C eliminar stub |
| 4 | Equipo C: `facturasApi.getPendientesCobro()` | Equipo E | Equipo E eliminar stub |
| 5 | Equipo E: `cobrosApi.create()` | Equipo F | Equipo F eliminar stub |

**Notificar en Slack cuando dependencia esté lista:**
```
@equipo-c [REMITOS] ✅ Entregado remitosApi.getParaFacturar()
- Endpoint: GET /api/ventas/remitos/para-facturar?terceroId={id}
- Branch: feature/remitos
- PR: #123
- Mergeado a develop: Sí

Pueden eliminar stub y consumir API real.
```

---

## 5. CODE REVIEW Y MERGE

### 5.1 Pull Request Template

```markdown
## [MATRIZ 1 - PEDIDOS] Backend - Comandos y Queries

### Descripción
Implementa comandos `CerrarPedidoCommand`, `CerrarPedidosMasivoCommand` y queries `GetPedidosConEstadoQuery`, `GetPedidosExpedicionQuery`.

### Cambios
- ✅ CerrarPedidoCommand + Handler + Validator
- ✅ CerrarPedidosMasivoCommand + Handler + Validator
- ✅ GetPedidosConEstadoQuery + Handler (agregados filtros atraso, producto)
- ✅ GetPedidosExpedicionQuery + Handler (nueva)
- ✅ Tests unitarios (85% coverage)
- ✅ Tests integración endpoints

### Validación vs zuluApp
| Funcionalidad | zuluApp | ZuluIA_Back | Status |
|---------------|---------|-------------|--------|
| Cerrar pedido individual | ✅ | ✅ | ✅ PARIDAD |
| Cerrar pedidos masivo | ✅ | ✅ | ✅ PARIDAD |
| Filtro por atraso | ✅ | ✅ | ✅ PARIDAD |
| Filtro por producto | ✅ | ✅ | ✅ PARIDAD |
| Vista expedición | ✅ | ✅ | ✅ PARIDAD |

### Checklist
- [x] Tests passing (local)
- [x] Tests passing (CI)
- [x] Coverage > 80%
- [x] Swagger docs actualizado
- [x] Sin errores de compilación
- [x] Sin warnings de linter
- [x] Migrations aplicadas correctamente
- [x] Validado contra PostgreSQL local

### Screenshots
(Si aplica - capturas de Swagger, resultados de tests, etc.)

### Issue
Closes #42

### Reviewers
@dev-backend-senior-2 @tech-lead-backend
```

### 5.2 Code Review Checklist

**Reviewer debe verificar:**

**Funcional:**
- [ ] ¿Implementa todos los requisitos del issue?
- [ ] ¿Cumple paridad con zuluApp?
- [ ] ¿Validaciones correctas?

**Calidad de Código:**
- [ ] ¿Sigue convenciones del proyecto?
- [ ] ¿Nombres descriptivos?
- [ ] ¿Complejidad ciclomática aceptable (< 10)?
- [ ] ¿Sin código duplicado?
- [ ] ¿Sin código comentado/dead code?

**Tests:**
- [ ] ¿Tests unitarios > 80% coverage?
- [ ] ¿Tests integración de endpoints críticos?
- [ ] ¿Casos edge cubiertos?
- [ ] ¿Tests passing en CI?

**Documentación:**
- [ ] ¿Swagger/OpenAPI actualizado?
- [ ] ¿Comentarios en código donde necesario?
- [ ] ¿README actualizado si aplica?

**Seguridad:**
- [ ] ¿Validación de inputs?
- [ ] ¿Sin SQL injection posible?
- [ ] ¿Sin secrets hardcodeados?

**Performance:**
- [ ] ¿Queries optimizadas?
- [ ] ¿Paginación implementada?
- [ ] ¿Sin N+1 queries?

### 5.3 Merge Strategy

**Frecuencia de merges:**
- **Backend:** 1 merge/semana por equipo (lunes integración)
- **Frontend:** 1 merge/semana por equipo (jueves integración)

**Orden de merges (crítico):**
```
Semana 2: feature/pedidos → develop
Semana 3: feature/remitos → develop
Semana 4: feature/facturas → develop
Semana 5: feature/notas-credito → develop
Semana 6: feature/cobros → develop
Semana 6: feature/cheques → develop
Semana 8: feature/reportes → develop
```

**Antes de merge:**
```bash
# 1. Actualizar branch feature con últimos cambios de develop
git checkout feature/pedidos
git fetch origin
git merge origin/develop

# 2. Resolver conflictos si hay
# 3. Correr tests localmente
dotnet test # Backend
npm run test # Frontend

# 4. Push
git push origin feature/pedidos

# 5. Crear PR en GitHub
# 6. Esperar aprobación de reviewer
# 7. Mergear (squash commits si es necesario)
```

---

## 6. TESTING STRATEGY

### 6.1 Backend - .NET

**Unit Tests (xUnit + FluentAssertions):**
```csharp
// tests/ZuluIA_Back.UnitTests/Application/CerrarPedidoCommandHandlerTests.cs
public class CerrarPedidoCommandHandlerTests
{
    [Fact]
    public async Task Handle_PedidoValido_DeberaCerrarPedido()
    {
        // Arrange
        var comprobante = ComprobanteFactory.CreatePedido();
        var mockRepo = MockComprobanteRepository(comprobante);
        var handler = new CerrarPedidoCommandHandler(mockRepo.Object);
        var command = new CerrarPedidoCommand(comprobante.Id, "Cierre por falta de stock");

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        comprobante.EstadoPedido.Should().Be(EstadoPedido.Cerrado);
        comprobante.MotivoCierrePedido.Should().Be("Cierre por falta de stock");
        comprobante.FechaCierrePedido.Should().NotBeNull();
    }

    [Fact]
    public async Task Handle_PedidoConEntregasParciales_DeberaFallar()
    {
        // Arrange
        var comprobante = ComprobanteFactory.CreatePedidoConEntregasParciales();
        var mockRepo = MockComprobanteRepository(comprobante);
        var handler = new CerrarPedidoCommandHandler(mockRepo.Object);
        var command = new CerrarPedidoCommand(comprobante.Id, "Intentar cerrar");

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Errors.Should().Contain(e => e.Contains("entregas parciales"));
    }
}
```

**Integration Tests (WebApplicationFactory):**
```csharp
// tests/ZuluIA_Back.IntegrationTests/Api/PedidosControllerTests.cs
public class PedidosControllerTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client;

    public PedidosControllerTests(WebApplicationFactory<Program> factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task POST_Pedidos_DeberaCrearPedido()
    {
        // Arrange
        var request = new
        {
            sucursalId = 1,
            terceroId = 100,
            items = new[]
            {
                new { itemId = 1, cantidad = 10, precioUnitario = 1000 }
            }
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/ventas/pedidos", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        var pedidoId = await response.Content.ReadFromJsonAsync<long>();
        pedidoId.Should().BeGreaterThan(0);
    }
}
```

**Correr tests localmente:**
```bash
cd C:\Zulu\ZuluIA_Back

# Todos los tests
dotnet test

# Tests de un proyecto específico
dotnet test tests/ZuluIA_Back.UnitTests

# Tests con coverage
dotnet test /p:CollectCoverage=true /p:CoverletOutputFormat=cobertura

# Generar reporte HTML de coverage
reportgenerator -reports:coverage.cobertura.xml -targetdir:coverage-report
```

### 6.2 Frontend - Next.js

**Unit Tests Componentes (Jest + React Testing Library):**
```typescript
// app/ventas/pedidos/components/__tests__/PedidoForm.test.tsx
import { render, screen, fireEvent, waitFor } from '@testing-library/react';
import { PedidoForm } from '../PedidoForm';

describe('PedidoForm', () => {
  it('debería validar campos obligatorios', async () => {
    render(<PedidoForm />);
    
    const submitButton = screen.getByRole('button', { name: /crear pedido/i });
    fireEvent.click(submitButton);

    await waitFor(() => {
      expect(screen.getByText(/cliente es requerido/i)).toBeInTheDocument();
      expect(screen.getByText(/fecha entrega es requerida/i)).toBeInTheDocument();
    });
  });

  it('debería enviar formulario con datos válidos', async () => {
    const mockSubmit = jest.fn();
    render(<PedidoForm onSubmit={mockSubmit} />);

    // Fill form
    fireEvent.change(screen.getByLabelText(/cliente/i), { target: { value: '100' } });
    fireEvent.change(screen.getByLabelText(/fecha entrega/i), { target: { value: '2026-04-15' } });

    // Submit
    fireEvent.click(screen.getByRole('button', { name: /crear pedido/i }));

    await waitFor(() => {
      expect(mockSubmit).toHaveBeenCalledWith(expect.objectContaining({
        terceroId: 100,
        fechaEntrega: '2026-04-15'
      }));
    });
  });
});
```

**E2E Tests (Playwright):**
```typescript
// e2e/pedidos.spec.ts
import { test, expect } from '@playwright/test';

test.describe('Pedidos E2E', () => {
  test('debería crear pedido completo', async ({ page }) => {
    // Login
    await page.goto('/login');
    await page.fill('input[name="email"]', 'test@zulu.com');
    await page.fill('input[name="password"]', 'password123');
    await page.click('button[type="submit"]');

    // Navegar a pedidos
    await page.goto('/ventas/pedidos/nuevo');

    // Llenar formulario
    await page.selectOption('select[name="terceroId"]', '100');
    await page.fill('input[name="fechaEntrega"]', '2026-04-15');
    
    // Agregar item
    await page.click('button:has-text("Agregar Item")');
    await page.selectOption('select[name="items.0.itemId"]', '1');
    await page.fill('input[name="items.0.cantidad"]', '10');

    // Submit
    await page.click('button:has-text("Crear Pedido")');

    // Verificar redirección y mensaje
    await expect(page).toHaveURL(/\/ventas\/pedidos\/\d+/);
    await expect(page.locator('text=Pedido creado exitosamente')).toBeVisible();
  });

  test('debería cerrar pedido', async ({ page }) => {
    await page.goto('/ventas/pedidos/123'); // Pedido existente
    
    await page.click('button:has-text("Cerrar Pedido")');
    await page.fill('textarea[name="motivo"]', 'Falta de stock');
    await page.click('button:has-text("Confirmar Cierre")');

    await expect(page.locator('text=Pedido cerrado correctamente')).toBeVisible();
    await expect(page.locator('[data-estado="cerrado"]')).toBeVisible();
  });
});
```

**Correr tests:**
```bash
cd C:\Zulu\ZuluIA_Front

# Unit tests
npm run test

# E2E tests
npm run test:e2e

# Coverage
npm run test:coverage
```

---

## 7. CI/CD PIPELINE

### 7.1 GitHub Actions - Backend

```yaml
# .github/workflows/backend-ci.yml
name: Backend CI

on:
  push:
    branches: [develop, feature/*]
  pull_request:
    branches: [develop]

jobs:
  build-and-test:
    runs-on: ubuntu-latest
    
    services:
      postgres:
        image: postgres:16
        env:
          POSTGRES_PASSWORD: postgres
          POSTGRES_DB: zuluia_test
        options: >-
          --health-cmd pg_isready
          --health-interval 10s
          --health-timeout 5s
          --health-retries 5
        ports:
          - 5432:5432

    steps:
      - uses: actions/checkout@v3
      
      - name: Setup .NET
        uses: actions/setup-dotnet@v3
        with:
          dotnet-version: '8.0.x'
      
      - name: Restore dependencies
        run: dotnet restore
      
      - name: Build
        run: dotnet build --no-restore --configuration Release
      
      - name: Run migrations
        run: dotnet ef database update --project src/ZuluIA_Back.Infrastructure --startup-project src/ZuluIA_Back.Api
        env:
          ConnectionStrings__DefaultConnection: "Host=localhost;Port=5432;Database=zuluia_test;Username=postgres;Password=postgres"
      
      - name: Run unit tests
        run: dotnet test tests/ZuluIA_Back.UnitTests --no-build --configuration Release --logger "trx;LogFileName=unit-tests.trx"
      
      - name: Run integration tests
        run: dotnet test tests/ZuluIA_Back.IntegrationTests --no-build --configuration Release --logger "trx;LogFileName=integration-tests.trx"
        env:
          ConnectionStrings__DefaultConnection: "Host=localhost;Port=5432;Database=zuluia_test;Username=postgres;Password=postgres"
      
      - name: Code coverage
        run: dotnet test /p:CollectCoverage=true /p:CoverletOutputFormat=cobertura /p:Threshold=80
      
      - name: Upload test results
        uses: actions/upload-artifact@v3
        if: always()
        with:
          name: test-results
          path: '**/*.trx'
```

### 7.2 GitHub Actions - Frontend

```yaml
# .github/workflows/frontend-ci.yml
name: Frontend CI

on:
  push:
    branches: [develop, feature/*-frontend]
  pull_request:
    branches: [develop]

jobs:
  build-and-test:
    runs-on: ubuntu-latest

    steps:
      - uses: actions/checkout@v3
      
      - name: Setup Node.js
        uses: actions/setup-node@v3
        with:
          node-version: '20.x'
          cache: 'npm'
      
      - name: Install dependencies
        run: npm ci
      
      - name: Lint
        run: npm run lint
      
      - name: Type check
        run: npm run type-check
      
      - name: Build
        run: npm run build
      
      - name: Run unit tests
        run: npm run test -- --coverage
      
      - name: Run E2E tests
        run: npm run test:e2e
        env:
          NEXT_PUBLIC_API_URL: http://localhost:5000
      
      - name: Upload coverage
        uses: codecov/codecov-action@v3
        with:
          files: ./coverage/coverage-final.json
```

---

## 8. DAILY TRACKING

### 8.1 Burndown Chart (Excel/Google Sheets)

**Plantilla:**

| Día | SP Restantes | SP Ideal | Equipo A | Equipo B | Equipo C | Equipo D | Equipo E |
|-----|--------------|----------|----------|----------|----------|----------|----------|
| Lun 1 | 337 | 337 | 34 | 55 | 68 | 34 | 55 |
| Mar 2 | 325 | 324 | 32 | 53 | 66 | 34 | 55 |
| Mie 3 | 310 | 311 | 28 | 50 | 63 | 34 | 55 |
| ... | ... | ... | ... | ... | ... | ... | ... |

**Gráfico:**
```
350 │
    │╲
300 │ ╲___  (Real)
    │     ╲╲
250 │       ╲╲___ (Ideal)
    │           ╲╲
200 │             ╲╲___
    │                 ╲╲
150 │                   ╲╲___
    └────────────────────────────────
     D1  D5   D10   D15  D20  D25  D30
```

### 8.2 Tablero Kanban (GitHub Projects)

**Columnas:**
- **Backlog** - Issues pendientes de arrancar
- **To Do** - Sprint actual
- **In Progress** - En desarrollo
- **In Review** - PR creado, esperando review
- **Done** - Mergeado a develop

**Cards:**
```
[PEDIDOS-BACKEND] Implementar CerrarPedidoCommand
├─ Assignee: @dev-backend-1
├─ Label: matriz-1-pedidos, backend
├─ Estimate: 3 SP
├─ Status: In Progress
└─ PR: #123 (draft)
```

---

## 9. COMUNICACIÓN

### 9.1 Daily Standup Template

**Mensaje Slack (si no hay reunión presencial):**

```
🔹 DAILY STANDUP - Equipo A (Pedidos)
📅 Fecha: 2026-04-01

👤 @dev-backend-1
✅ Ayer: Implementé CerrarPedidoCommand + tests unitarios
📝 Hoy: Implementar CerrarPedidosMasivoCommand
🚫 Blockers: Ninguno

👤 @dev-frontend-1
✅ Ayer: Creé PedidoForm con validaciones Zod
📝 Hoy: Integrar PedidoForm con pedidosApi
🚫 Blockers: Necesito que backend entregue POST /api/ventas/pedidos (estimado: jueves)
```

### 9.2 Blocker Escalation

**Si hay blocker:**
```
🚨 BLOCKER - Equipo C (Facturas)

Descripción: No puedo avanzar facturación desde remitos porque Equipo B no entregó GET /api/ventas/remitos/para-facturar

Impacto: Bloqueado 2 días de desarrollo (6 SP)

Acción solicitada: 
@equipo-b ¿Pueden priorizar ese endpoint? 
¿O proveer mock/stub que podamos usar temporalmente?

Alternativa: Crear stub nosotros y continuar (requiere refactor después)
```

**Resolución:**
```
✅ RESUELTO - Blocker Equipo C

Solución: Equipo B priorizó endpoint, estará listo mañana.
Mientras tanto, usar stub temporal:
https://github.com/SoporteZulu/ZuluIA_Front/blob/feature/facturas-frontend/lib/api/stubs/remitos-stub.ts

Acción Equipo C: Continuar con stub, reemplazar cuando API real esté lista.
```

---

## 10. RETROSPECTIVA SEMANAL

### 10.1 Template Retrospectiva

**Viernes 17:00 - 30 min**

**Qué salió bien ✅:**
- Equipo A completó backend pedidos (100% de tareas)
- CI/CD funcionando sin problemas
- Code reviews en < 24hs

**Qué salió mal ❌:**
- Equipo B tuvo blocker por falta de acceso a BD de prueba (2 días perdidos)
- Merge conflict entre Equipo C y Equipo E (4hs para resolver)

**Qué mejorar 🔧:**
- Setup BD de prueba más temprano (acción: @tech-lead)
- Coordinar mejor merges (acción: sync diario de ramas con develop)

**Action Items:**
- [ ] @tech-lead: Setup BD PostgreSQL en Docker Compose para todos
- [ ] @all-teams: Mergear develop a feature branches DIARIO (antes de EOD)

---

## 11. VALIDACIÓN FINAL

### 11.1 Acceptance Testing vs zuluApp

**Para cada funcionalidad:**

1. **Preparar caso de prueba:**
   ```
   Funcionalidad: Cerrar pedido individual
   
   zuluApp:
   1. Abrir frmNotaPedido
   2. Buscar pedido 0001-00000123
   3. Verificar estado = Pendiente
   4. Click "Cerrar Pedido"
   5. Ingresar motivo "Falta de stock"
   6. Confirmar
   7. Verificar estado = Cerrada
   8. Verificar motivo guardado
   
   ZuluIA_Front:
   1. Navegar a /ventas/pedidos
   2. Buscar pedido 0001-00000123
   3. Verificar estado = Pendiente
   4. Click "Cerrar Pedido"
   5. Ingresar motivo "Falta de stock"
   6. Confirmar
   7. Verificar estado = Cerrado
   8. Verificar motivo guardado
   ```

2. **Ejecutar en zuluApp:**
   - Capturar pantallas
   - Anotar comportamiento

3. **Ejecutar en ZuluIA_Front:**
   - Capturar pantallas
   - Anotar comportamiento

4. **Comparar:**
   - ¿Mismos resultados?
   - ¿Mismas validaciones?
   - ¿Mismos mensajes de error?

5. **Aprobar o rechazar:**
   - ✅ Si comportamiento idéntico → Aprobar
   - ❌ Si diferencias → Crear issue de corrección

### 11.2 Checklist Final por Matriz

```markdown
## Matriz 1 - Pedidos - Acceptance Checklist

### Backend
- [ ] Todos los endpoints implementados y documentados en Swagger
- [ ] Tests unitarios > 80% coverage
- [ ] Tests integración de endpoints críticos
- [ ] Validaciones coinciden con zuluApp
- [ ] Migrations aplicadas y funcionando en dev/staging

### Frontend
- [ ] Todas las páginas implementadas
- [ ] Todos los componentes implementados
- [ ] Servicios API implementados
- [ ] Validaciones de formularios (Zod)
- [ ] Manejo de errores y loading states
- [ ] Responsive mobile/tablet/desktop
- [ ] Tests E2E de flujos críticos

### Paridad Funcional
- [ ] Crear pedido = zuluApp ✅
- [ ] Editar pedido = zuluApp ✅
- [ ] Cerrar pedido individual = zuluApp ✅
- [ ] Cerrar pedidos masivo = zuluApp ✅
- [ ] Consultar estado con filtros = zuluApp ✅
- [ ] Ver vinculaciones = zuluApp ✅
- [ ] Vista expedición = zuluApp ✅

### Sign-off
- [ ] Tech Lead Backend aprueba
- [ ] Tech Lead Frontend aprueba
- [ ] QA Lead aprueba
- [ ] Product Owner aprueba
- [ ] Mergeado a develop ✅
```

---

## CONCLUSIÓN

Con esta guía tienes:

1. ✅ **Setup inicial completo** (branches, issues, projects)
2. ✅ **Organización de equipos** (7 equipos paralelos)
3. ✅ **Workflow diario** (standups, coding blocks, sync)
4. ✅ **Manejo de dependencias** (stubs temporales)
5. ✅ **Code review y merge strategy**
6. ✅ **Testing completo** (unit, integration, E2E)
7. ✅ **CI/CD pipelines** (GitHub Actions)
8. ✅ **Tracking y comunicación** (burndown, Kanban, Slack)
9. ✅ **Retrospectivas** (mejora continua)
10. ✅ **Validación final** (acceptance testing vs zuluApp)

**¡Ahora a trabajar en paralelo y cerrar la paridad al 100%!** 🚀

---

**Última actualización:** 2026-03-31  
**Versión:** 1.0
