# Vylersant Facturación — Documentación Técnica Unificada

## 1. Objetivo del proyecto

Vylersant Facturación es una aplicación de escritorio orientada a pequeños negocios.

La aplicación principal será desarrollada con **WPF y .NET 10**. Su propósito es permitir operaciones de facturación y convertir las ventas registradas en información útil para el propietario del negocio.

La API ASP.NET Core **no constituye la interfaz principal del producto**. Su función es servir como backend seguro para autenticación, persistencia en nube, sincronización, analítica futura, funciones SaaS, control de sesiones e integraciones.

```text
Desktop WPF
    ↓
API ASP.NET Core
    ↓
Application
    ↓
Domain / Infrastructure
    ↓
SQL Server
```

La aplicación de escritorio podrá incorporar posteriormente SQLite para permitir operación temporal sin conexión.

---

## 2. Arquitectura general

```text
┌──────────────────────────────┐
│      Vylersant Desktop       │
│        WPF / .NET 10         │
│                              │
│ Facturación                  │
│ Dashboard                    │
│ Productos                    │
│ Clientes                     │
│ Reportes                     │
└──────────────┬───────────────┘
               │ HTTPS
               ▼
┌──────────────────────────────┐
│       Vylersant API          │
│    ASP.NET Core / .NET 10    │
└──────────────┬───────────────┘
               │
               ▼
┌──────────────────────────────┐
│        Application           │
└──────────────┬───────────────┘
               │
               ▼
┌──────────────────────────────┐
│          Domain              │
└──────────────────────────────┘

        Infrastructure
              │
              ▼
          SQL Server
```

La solución mantiene las responsabilidades separadas para evitar que la lógica de negocio dependa de tecnologías concretas.

---

## 3. Proyectos de la solución

### 3.1 Vylersant-Facturacion.Domain

Contiene entidades, reglas de negocio, estados y comportamientos del dominio.

Entidades actuales:

```text
Business
User
UserRole
RefreshToken
Product
Customer
```

Entidades previstas:

```text
Invoice
InvoiceLine
Payment
```

Domain no debe depender de EF Core, SQL Server, WPF, ASP.NET Core, BCrypt, JWT ni HTTP.

Principio:

```text
Domain conoce negocio.
Domain no conoce infraestructura.
```

### 3.2 Vylersant-Facturacion.Application

Contiene casos de uso, interfaces, servicios de aplicación, requests/results internos y abstracciones.

Interfaces actuales:

```text
IUserRepository
IBussinesRepository
IProductRepository
ICustomerRepository
IRefreshTokenRepository
IPasswordHasher
ITokenService
IRefreshTokenService
IUnitOfWork
```

Casos de uso actuales:

```text
RegisterBusinessService
LoginService
RefreshSessionService

CreateProductService
GetProductsService
GetProductByIdService
UpdateProductService
ChangeProductStatusService

CreateCustomerService
GetCustomersService
GetCustomerByIdService
UpdateCustomerService
ChangeCustomerStatusService
```

Application no depende directamente de EF Core, SQL Server, BCrypt ni de la implementación concreta de JWT.

### 3.3 Vylersant-Facturacion.Infrastructure

Implementa las abstracciones técnicas.

Actualmente incluye:

```text
VylersantFacturacionDbContext

UserRepository
BussinesRepository
ProductRepository
CustomerRepository
RefreshTokenRepository

BCryptPasswordHasher
JwtTokenService
RefreshTokenService
```

Tecnologías actuales:

```text
Entity Framework Core
SQL Server
BCrypt
JWT
SHA-256
RandomNumberGenerator
```

### 3.4 Vylersant-Facturacion.Api

Representa la frontera HTTP.

Responsabilidades:

- Exponer endpoints.
- Recibir contratos HTTP.
- Convertirlos a requests de Application.
- Ejecutar casos de uso.
- Validar JWT.
- Aplicar autorización.
- Manejar errores HTTP.
- Devolver ProblemDetails.
- Servir como backend para Desktop.

La API no es una aplicación web para el usuario final.

### 3.5 Vylersant-Facturacion.Contracts

Contiene contratos compartidos entre Desktop y API.

Ejemplos:

```text
RegisterBusinessRequest
RegisterBusinessResponse
LoginRequest
LoginResponse
RefreshSessionRequest
RefreshSessionResponse
CurrentUserResponse

CreateProductRequest
CreateProductResponse
ProductResponse
UpdateProductRequest
ChangeProductStatusRequest

CreateCustomerRequest
CreateCustomerResponse
CustomerResponse
UpdateCustomerRequest
ChangeCustomerStatusRequest
```

Contracts no contiene reglas de negocio.

### 3.6 Vylersant-Facturacion.Desktop

Es la aplicación principal utilizada por el negocio.

Tecnología:

```text
WPF
.NET 10
```

Experiencias previstas:

```text
Empleado
├── Facturación
├── Productos permitidos
├── Clientes
└── Historial permitido

Propietario
├── Dashboard
├── Facturación
├── Reportes
├── Analítica
├── Usuarios
└── Configuración
```

Desktop consumirá la API por HTTPS.

---

## 4. Dependencias entre capas

Dirección principal:

```text
Domain
   ↑
Application
   ↑
Infrastructure
```

La API utiliza Application, Infrastructure y Contracts. Desktop utiliza Contracts.

Dependencias que debemos evitar:

```text
Domain → Infrastructure
Domain → EF Core
Domain → SQL Server
Domain → WPF
Application → EF Core
Application → BCrypt
Application → JWT concreto
```

---

## 5. Persistencia

Persistencia remota actual:

```text
Entity Framework Core 10
SQL Server
```

DbContext principal:

```text
VylersantFacturacionDbContext
```

Configuraciones Fluent API actuales:

```text
BusinessConfiguration
UserConfiguration
RefreshTokenConfiguration
ProductConfiguration
CustomerConfiguration
```

Se descubren mediante:

```csharp
modelBuilder.ApplyConfigurationsFromAssembly(
    typeof(VylersantFacturacionDbContext).Assembly);
```

---

## 6. Multi-tenancy

Cada negocio posee un `BusinessId`.

Actualmente:

```text
Business
├── Users
├── Products
└── Customers
```

Regla fundamental:

```text
Usuario del Business A
        X
Datos del Business B
```

El JWT incluye:

```text
business_id
```

Las consultas sensibles utilizan:

```text
ResourceId + BusinessId
```

Ejemplos:

```text
ProductId + BusinessId
CustomerId + BusinessId
```

---

## 7. Configuración

Se utilizan:

```text
appsettings.json
appsettings.Development.json
appsettings.Production.json
User Secrets
Variables de entorno
```

Los secretos no deben almacenarse en Git.

Ejemplos:

```text
Jwt:Key
ConnectionStrings
Credenciales externas
API Keys
```

---

## 8. Dependency Injection

La API actúa como Composition Root.

```csharp
builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);
```

Application e Infrastructure centralizan sus registros con `AddApplication()` y `AddInfrastructure(...)`.

---

## 9. OpenAPI y Swagger

Durante Development la API expone:

```text
/openapi/v1.json
/swagger
```

Swagger UI se usa para probar endpoints durante desarrollo.

Se configuró seguridad HTTP Bearer para que aparezca:

```text
Authorize 🔒
```

y Swagger envíe:

```http
Authorization: Bearer <access-token>
```

La documentación OpenAPI declara el esquema Bearer con formato JWT.

---

## 10. Business

Entidad:

```text
Business
├── Id
├── Name
├── IsActive
├── Address
├── Phone
└── Email
```

Reglas principales:

```text
Id válido.
Name obligatorio.
Name no vacío.
Name normalizado con Trim().
Business nace activo.
```

Comportamientos:

```csharp
business.Activate();
business.Deactivate();
```

---

## 11. User

Entidad:

```text
User
├── Id
├── BusinessId
├── Name
├── Email
├── PasswordHash
├── Role
└── IsActive
```

El email se normaliza con `Trim()` y `ToLowerInvariant()`.

Decisión actual:

```text
Email único globalmente.
```

Esto permite login con Email + Password sin pedir código de negocio.

---

## 12. Roles

Roles actuales:

```text
Owner
Supervisor
Employee
```

Cada usuario tiene un único rol.

---

## 13. Password hashing

```text
IPasswordHasher
        ↓
BCryptPasswordHasher
        ↓
BCrypt
```

Nunca se almacena la contraseña original.

---

## 14. Registro de negocio y propietario

Endpoint:

```http
POST /api/auth/register
```

Flujo:

```text
Desktop
   ↓
POST /api/auth/register
   ↓
AuthController
   ↓
RegisterBusinessService
   ↓
¿Email ya existe?
   │
   ├── Sí → rechazo
   │
   └── No
        ↓
Crear Business
        ↓
Hash de contraseña
        ↓
Crear User con Role = Owner
        ↓
Guardar Business + User
        ↓
SaveChanges
```

---

## 15. Login

Endpoint:

```http
POST /api/auth/login
```

Flujo:

```text
Desktop
   ↓
Email + Password
   ↓
LoginService
   ↓
Buscar User por Email
   ↓
Verificar contraseña
   ↓
Comprobar IsActive
   ↓
Generar Access Token
   ↓
Generar Refresh Token
   ↓
Guardar hash del Refresh Token
   ↓
SaveChanges
```

Respuesta:

```text
UserId
BusinessId
Name
Email
Role
AccessToken
AccessTokenExpiresAtUtc
RefreshToken
RefreshTokenExpiresAtUtc
```

---

## 16. JWT Access Token

Configuración:

```text
Jwt:Issuer
Jwt:Audience
Jwt:Key
Jwt:ExpirationMinutes
```

Duración actual de desarrollo:

```text
30 minutos
```

Peticiones autenticadas:

```http
Authorization: Bearer <access-token>
```

---

## 17. Claims JWT

Claims actuales:

```text
sub          → UserId
business_id  → BusinessId
email        → Email
name         → Nombre
role         → UserRole
```

Ejemplo conceptual:

```json
{
  "sub": "...",
  "business_id": "...",
  "email": "owner@example.com",
  "name": "John Doe",
  "role": "Owner"
}
```

---

## 18. Autenticación y autorización

```text
Autenticación → ¿Quién eres?
Autorización  → ¿Qué puedes hacer?
```

Orden del middleware:

```text
UseAuthentication()
        ↓
UseAuthorization()
        ↓
MapControllers()
```

---

## 19. Current User

Endpoint:

```http
GET /api/auth/me
```

Requiere JWT válido y devuelve:

```text
UserId
BusinessId
Name
Email
Role
```

---

## 20. Refresh Tokens

Configuración:

```text
RefreshToken:ExpirationDays
```

Duración actual:

```text
7 días
```

El Refresh Token permite renovar sesión sin pedir nuevamente credenciales.

---

## 21. Seguridad de Refresh Tokens

El token original no se almacena en SQL Server.

```text
Refresh Token original
       ↓
SHA-256
       ↓
TokenHash
       ↓
SQL Server
```

Se generan con `RandomNumberGenerator`.

---

## 22. Entidad RefreshToken

```text
Id
UserId
TokenHash
CreatedAtUtc
ExpiresAtUtc
RevokedAtUtc
```

Activo cuando:

```text
RevokedAtUtc == null
AND
CurrentTime < ExpiresAtUtc
```

---

## 23. Varias sesiones por usuario

Se permiten múltiples sesiones activas.

```text
Owner
├── PC de caja → Refresh A
├── Laptop     → Refresh B
└── Otra PC    → Refresh C
```

---

## 24. Rotación de Refresh Tokens

```text
Refresh A
↓
validar
↓
revocar A
↓
generar B
↓
guardar Hash(B)
↓
nuevo Access Token
↓
devolver B
```

A queda revocado y no puede reutilizarse.

---

## 25. Renovación de sesión

Endpoint:

```http
POST /api/auth/refresh
```

Flujo:

```text
Recibir Refresh Token
       ↓
Calcular SHA-256
       ↓
Buscar TokenHash
       ↓
Validar estado
       ↓
Buscar User
       ↓
Validar User
       ↓
Revocar token anterior
       ↓
Generar token nuevo
       ↓
Generar Access Token nuevo
       ↓
SaveChanges
```

---

## 26. Manejo global de errores

La API utiliza:

```text
GlobalExceptionHandler
IExceptionHandler
ProblemDetails
```

Errores actuales:

```text
Credenciales inválidas → 401
Refresh Token inválido → 401
Email ya registrado → 409
Producto no encontrado → 404
Cliente no encontrado → 404
Argumentos inválidos → 400
Error inesperado → 500
```

---

## 27. Repositorios

### IUserRepository

```text
GetByEmailAsync
GetByIdAsync
AddAsync
```

### IBussinesRepository

```text
AddAsync
```

### IRefreshTokenRepository

```text
GetByHashAsync
AddAsync
```

### IProductRepository

```text
GetByIdAsync(productId, businessId)
GetAllAsync(businessId)
AddAsync(product)
```

### ICustomerRepository

```text
GetByIdAsync(customerId, businessId)
GetAllAsync(businessId)
AddAsync(customer)
```

Los contratos viven en Application y las implementaciones en Infrastructure.

---

## 28. Unit of Work

Se utiliza `IUnitOfWork` para confirmar cambios.

Implementación actual:

```text
VylersantFacturacionDbContext
```

---

## 29. Base de datos actual

Tablas principales:

```text
Businesses
Users
RefreshTokens
Products
Customers
__EFMigrationsHistory
```

Relaciones:

```text
Users.BusinessId → Businesses.Id
Products.BusinessId → Businesses.Id
Customers.BusinessId → Businesses.Id
RefreshTokens.UserId → Users.Id
```

Índices principales:

```text
Users.Email → UNIQUE
RefreshTokens.TokenHash → UNIQUE
Products.BusinessId → INDEX
Customers.BusinessId → INDEX
```

---

## 30. Migraciones

Migraciones principales:

```text
InitialCreate
AddBusinessAndUsers
AddRefreshTokens
AddProducts
AddCustomers
```

Regla:

```text
Crear migración
↓
Revisarla
↓
Aplicar database update
↓
Build
↓
Tests
```

---

## 31. Módulo Products

### 31.1 Product

```text
Id
BusinessId
Name
Price
Sku?
Description?
IsActive
```

Reglas actuales:

- BusinessId obligatorio.
- Name obligatorio.
- Price no negativo.
- Product nace activo.
- Sku y Description opcionales.
- Campos opcionales vacíos se normalizan.
- Actualmente existe un solo precio por producto.

Comportamientos:

```text
ChangeName
ChangePrice
ChangeSku
ChangeDescription
Activate
Deactivate
```

No se elimina físicamente:

```text
Product.Deactivate()
        ↓
IsActive = false
```

### 31.2 Persistencia de Products

```text
Products
├── Id
├── BusinessId
├── Name
├── Price decimal(18,2)
├── Sku
├── Description
└── IsActive
```

Relación:

```text
Products.BusinessId → Businesses.Id
```

Existe índice por BusinessId. SKU no es único globalmente.

### 31.3 Casos de uso

```text
CreateProductService
GetProductsService
GetProductByIdService
UpdateProductService
ChangeProductStatusService
```

Las consultas individuales usan:

```text
ProductId + BusinessId
```

### 31.4 Endpoints

```text
POST   /api/products
GET    /api/products
GET    /api/products/{id}
PUT    /api/products/{id}
PATCH  /api/products/{id}/status
```

Todos requieren JWT. `BusinessId` se obtiene del claim `business_id`.

No existe `DELETE /api/products/{id}`.

### 31.5 Respuestas HTTP

```text
Sin JWT → 401 Unauthorized
Producto inexistente → 404 Not Found
Producto de otro Business → 404 Not Found
```

El 404 usa ProblemDetails con título:

```text
Producto no encontrado
```

---

## 32. Módulo Customers

### 32.1 Customer

```text
Id
BusinessId
Name
Address?
Phone?
Email?
IsActive
```

Reglas:

- BusinessId obligatorio.
- Name obligatorio.
- Address opcional.
- Phone opcional.
- Email opcional.
- Customer nace activo.
- Campos opcionales vacíos se convierten en null.
- Email se normaliza a minúsculas.

Comportamientos:

```text
ChangeName
ChangeAddress
ChangePhone
ChangeEmail
Activate
Deactivate
```

Aún no se agregan:

```text
RNC
Cédula
Tipo fiscal
Límite de crédito
Balance
```

Se incorporarán cuando facturación o crédito los necesiten.

### 32.2 Persistencia de Customers

```text
Customers
├── Id
├── BusinessId
├── Name
├── Address
├── Phone
├── Email
└── IsActive
```

Relación:

```text
Customers.BusinessId → Businesses.Id
```

Existe índice por BusinessId. Email, Phone y Name no son únicos actualmente.

### 32.3 Casos de uso

```text
CreateCustomerService
GetCustomersService
GetCustomerByIdService
UpdateCustomerService
ChangeCustomerStatusService
```

Las consultas individuales usan:

```text
CustomerId + BusinessId
```

### 32.4 Endpoints

```text
POST   /api/customers
GET    /api/customers
GET    /api/customers/{id}
PUT    /api/customers/{id}
PATCH  /api/customers/{id}/status
```

Todos requieren JWT y usan `business_id` desde el token.

No existe eliminación física de Customers.

### 32.5 Respuestas HTTP

```text
Sin JWT → 401 Unauthorized
Cliente inexistente → 404 Not Found
Cliente de otro Business → 404 Not Found
```

El 404 usa ProblemDetails con título:

```text
Cliente no encontrado
```

---

## 33. Seguridad multi-tenant de Products y Customers

Ejemplo:

```text
Usuario Business A
        ↓
JWT business_id = A
        ↓
GET /api/customers/{clienteB}
        ↓
GetById(clienteB, A)
        ↓
No coincide BusinessId
        ↓
null
        ↓
404 Not Found
```

El sistema no diferencia públicamente entre recurso inexistente y recurso perteneciente a otro Business.

El cliente tampoco puede enviar libremente `BusinessId` al crear recursos:

```text
JWT BusinessId
        ↓
Application
        ↓
Product / Customer
```

---

## 34. Pruebas actuales

### 34.1 Domain Tests

Cubren:

```text
Business
User
Roles
RefreshToken
Product
Customer
```

Products:

```text
Creación
Nombre
Precio
Campos opcionales
Activar
Desactivar
```

Customers:

```text
Creación
Nombre
Campos opcionales
Normalización de Email
Activar
Desactivar
```

### 34.2 Application Tests

Autenticación:

```text
Registro de negocio
Login
Credenciales incorrectas
Usuario inactivo
Creación de Refresh Token
Renovación
Token inexistente
Token expirado
Token revocado
Usuario inexistente
Usuario inactivo durante refresh
Rotación
```

Products:

```text
Crear
Listar
Obtener por Id
Actualizar
Cambiar estado
Not Found
Multi-tenant
```

Customers:

```text
Crear
Listar
Obtener por Id
Actualizar
Cambiar estado
Not Found
Multi-tenant
```

### 34.3 Infrastructure Tests

```text
BCrypt
JWT
Claims
Firma JWT
Firma inválida
Generación de Refresh Token
```

### 34.4 Integration Tests

Autenticación:

```text
/me sin token → 401
/me con JWT válido → 200
Login inválido → 401
Email duplicado → 409
Login → Refresh → nuevos tokens
Reutilizar Refresh Token anterior → 401
```

Products:

```text
Sin JWT → 401
Crear con JWT → 201
Listar por Business
GET cross-tenant → 404
PUT cross-tenant → 404
PATCH cross-tenant → 404
Not Found → ProblemDetails 404
```

Customers:

```text
Sin JWT → 401
Crear con JWT → 201
Listar por Business
GET cross-tenant → 404
PUT cross-tenant → 404
PATCH cross-tenant → 404
Not Found → ProblemDetails 404
```

Regla antes de integrar:

```powershell
dotnet test
dotnet build
```

---

## 35. Arquitectura offline futura

```text
Desktop
   │
   ▼
SQLite local
   │
   │ conexión disponible
   ▼
Sync Service
   │
   ▼
API
   │
   ▼
SQL Server
```

Sin Internet se pretende permitir:

```text
Facturar    ✅
Cobrar      ✅
Imprimir    ✅
Guardar     ✅
Sincronizar ⏳
```

La sincronización deberá considerar UUID, idempotencia, operaciones pendientes, reintentos, duplicados, conflictos y estado de sincronización.

---

## 36. Impresión futura

```text
Venta
  ↓
IPrintService
  ↓
Implementación ESC/POS
  ↓
Impresora térmica
```

Objetivo:

```text
Invoice no conoce impresoras.
Sales no conoce marcas.
```

---

## 37. Dashboard futuro

Información prevista:

```text
Ventas del día
Ventas del mes
Cantidad de facturas
Ticket promedio
Productos más vendidos
Ingresos por producto
Ventas por empleado
Ventas por hora
Ventas por día
Métodos de pago
Comparación de períodos
```

El Dashboard formará parte de la aplicación WPF.

---

## 38. Inventario futuro

No es obligatorio para el MVP.

Podrá incorporarse con:

```text
Existencias
Compras
Entradas
Salidas
Stock mínimo
Ajustes
```

---

## 39. Flujo de Git

Ramas principales:

```text
main
 develop
```

`main` contiene código estable. `develop` funciona como integración.

Ramas de trabajo:

```text
feature/<issue>-descripcion
fix/<issue>-descripcion
chore/<issue>-descripcion
docs/<issue>-descripcion
test/<issue>-descripcion
```

---

## 40. Flujo de trabajo por Issue

```text
Issue
  ↓
Mover a In Progress
  ↓
Crear rama desde develop
  ↓
Implementar
  ↓
Build
  ↓
Tests
  ↓
Commit
  ↓
Push
  ↓
Pull Request
  ↓
Review
  ↓
Merge a develop
  ↓
Cerrar Issue
```

---

## 41. Commits de referencia

```text
feat: implementar autenticacion de usuario (#17)
feat: implementar JWT access token (#18)
feat: exponer autenticacion y proteger endpoints (#19)
feat: implementar refresh tokens y rotacion de sesiones (#20)

feat: modelar entidad product (#23)
feat: configurar persistencia de product (#24)
feat: implementar repositorio de productos (#25)
feat: implementar creacion de productos (#26)
feat: implementar listado de productos (#27)
feat: implementar consulta de producto por id (#28)
feat: implementar actualizacion de productos (#29)
feat: implementar estado de productos (#30)
feat: exponer endpoints http de productos (#31)
test: validar aislamiento multi-tenant de productos (#32)
test: validar respuestas http de productos (#33)

feat: modelar entidad customer (#34)
feat: configurar persistencia de customer (#35)
feat: implementar repositorio de clientes (#36)
feat: implementar creacion de clientes (#37)
feat: implementar listado de clientes (#38)
feat: implementar consulta de cliente por id (#39)
feat: implementar actualizacion de clientes (#40)
feat: implementar estado de clientes (#41)
feat: exponer endpoints http de clientes (#42)
test: validar aislamiento multi-tenant de clientes (#43)
test: validar respuestas http de clientes (#44)
docs: actualizar documentacion de productos y clientes (#45)
```

---

## 42. Flujo completo de autenticación

```text
                    REGISTRO

Desktop
   ↓
POST /api/auth/register
   ↓
Business + Owner
   ↓
SQL Server


                     LOGIN

Desktop
   ↓
Email + Password
   ↓
POST /api/auth/login
   ↓
BCrypt Verify
   ↓
Access Token
Refresh Token A
   ↓
Desktop


              PETICIÓN AUTENTICADA

Desktop
   ↓
Authorization: Bearer AccessToken
   ↓
API
   ↓
JwtBearer
   ↓
Controller


              ACCESS TOKEN EXPIRA

Desktop
   ↓
Refresh Token A
   ↓
POST /api/auth/refresh
   ↓
Hash(A)
   ↓
Validación
   ↓
Revocar A
   ↓
Generar B
   ↓
Access Token nuevo
   ↓
Desktop
```

---

## 43. Responsabilidades futuras de Desktop

```text
Mostrar Login
        ↓
Enviar credenciales
        ↓
Recibir Access Token
        ↓
Recibir Refresh Token
        ↓
Mantener sesión
        ↓
Agregar Bearer automáticamente
        ↓
Detectar expiración / 401
        ↓
Solicitar refresh
        ↓
Actualizar tokens
        ↓
Reintentar operación si corresponde
```

La estrategia de almacenamiento seguro de sesión en Windows todavía debe diseñarse.

---

## 44. Principios actuales

1. Domain no conoce tecnologías externas.
2. Application depende de abstracciones.
3. Infrastructure implementa contratos técnicos.
4. Desktop es la interfaz principal.
5. La API funciona como backend seguro.
6. La lógica de negocio no vive en WPF ni en Controllers.
7. Las contraseñas nunca se almacenan en texto plano.
8. BCrypt se utiliza para contraseñas.
9. Los Refresh Tokens se generan criptográficamente.
10. Solo se almacena el hash SHA-256 del Refresh Token.
11. Los Refresh Tokens se rotan al utilizarse.
12. Los tokens revocados no pueden reutilizarse.
13. Los usuarios inactivos no pueden iniciar sesión ni renovar sesión.
14. Las claves JWT no deben almacenarse en el repositorio.
15. Cada usuario puede tener múltiples sesiones.
16. Los claims incluyen BusinessId.
17. Products y Customers se aíslan por BusinessId.
18. BusinessId no se recibe libremente desde requests públicos multi-tenant.
19. Los repositorios exponen solo operaciones necesarias.
20. Las abstracciones se agregan cuando existe una necesidad real.
21. Antes de integrar se ejecutan build y tests.
22. Cada feature relevante tiene Issue y rama propia.
23. `main` se mantiene estable.
24. `develop` funciona como rama de integración.
25. Products y Customers se desactivan en lugar de eliminarse físicamente.
26. Product mantiene un solo precio por ahora.
27. No se agregan reglas futuras hasta que exista una necesidad real.

---

## 45. Estado actual del proyecto

### Sprint 0 — Fundación

```text
✅ Solución .NET 10
✅ WPF Desktop
✅ Domain
✅ Application
✅ Infrastructure
✅ Contracts
✅ API
✅ Tests
✅ EF Core
✅ SQL Server
✅ DbContext
✅ Migraciones
✅ Dependency Injection
✅ OpenAPI
✅ Swagger
✅ Environments
✅ User Secrets
✅ Git
✅ Ramas
✅ Issues
✅ Pull Requests
✅ Documentación inicial
```

### Sprint 1 — Negocio, usuarios y autenticación

```text
✅ Business
✅ User
✅ Roles
✅ EF mappings
✅ Repositories
✅ Unit of Work
✅ BCrypt
✅ Registro Business + Owner
✅ Login
✅ JWT
✅ Claims
✅ Endpoints HTTP
✅ /api/auth/me
✅ Autorización base
✅ ProblemDetails
✅ GlobalExceptionHandler
✅ Refresh Tokens
✅ SHA-256
✅ Múltiples sesiones
✅ Rotación de tokens
✅ Tests unitarios
✅ Tests de infraestructura
✅ Tests de integración
✅ Documentación consolidada
```

### Sprint 2 — Productos y Clientes

Products:

```text
✅ Domain
✅ EF Core
✅ Repository
✅ Create
✅ List
✅ GetById
✅ Update
✅ Activate / Deactivate
✅ HTTP endpoints
✅ JWT authentication
✅ Multi-tenant isolation
✅ HTTP validation
✅ Tests
```

Customers:

```text
✅ Domain
✅ EF Core
✅ Repository
✅ Create
✅ List
✅ GetById
✅ Update
✅ Activate / Deactivate
✅ HTTP endpoints
✅ JWT authentication
✅ Multi-tenant isolation
✅ HTTP validation
✅ Tests
```

Sprint 2 queda funcionalmente completado con Products y Customers.

---

## 46. Próximas áreas

```text
Facturación
Detalle de factura
Pagos
POS WPF
Impresión térmica
Dashboard
Analítica
SQLite local
Sincronización
Modo offline
Auditoría
Exportaciones
Suscripciones
Instalador
Integración fiscal/e-CF
Inventario opcional
Sucursales
```

La prioridad debe mantenerse en entregar funcionalidad útil de forma incremental sin introducir complejidad antes de necesitarla.
