# Vylersant Facturación — Documentación Técnica

## 1. Objetivo del proyecto

Vylersant Facturación es una aplicación de escritorio orientada a pequeños negocios.

La aplicación principal será desarrollada con **WPF y .NET 10**. Su propósito es permitir operaciones de facturación y convertir las ventas registradas en información útil para el propietario del negocio.

La API ASP.NET Core **no constituye la interfaz principal del producto**. Su función es servir como backend seguro para autenticación, persistencia en nube, sincronización, analítica futura, funciones SaaS, control de sesiones e integraciones futuras.

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

La aplicación de escritorio podrá incorporar posteriormente almacenamiento local con SQLite para permitir operación temporal sin conexión.

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
               │
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

Entidades actuales o previstas:

```text
Business
User
UserRole
RefreshToken
Product
Customer
Invoice
Payment
```

Domain no debe depender de Entity Framework Core, SQL Server, WPF, ASP.NET Core, BCrypt, JWT ni HTTP.

Principio:

```text
Domain conoce negocio.
Domain no conoce infraestructura.
```

### 3.2 Vylersant-Facturacion.Application

Contiene casos de uso, interfaces requeridas por la aplicación, servicios de aplicación, requests/results internos y abstracciones de seguridad y persistencia.

Interfaces actuales:

```text
IUserRepository
IBussinesRepository
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
```

Application no depende directamente de EF Core, SQL Server, BCrypt ni System.IdentityModel.Tokens.Jwt.

### 3.3 Vylersant-Facturacion.Infrastructure

Contiene las implementaciones técnicas.

Actualmente incluye:

```text
VylersantFacturacionDbContext

UserRepository
BussinesRepository
RefreshTokenRepository

BCryptPasswordHasher
JwtTokenService
RefreshTokenService
```

Tecnologías:

```text
Entity Framework Core
SQL Server
BCrypt
JWT
SHA-256
RandomNumberGenerator
```

Infrastructure implementa los contratos definidos por Application.

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
```

Contracts no debe contener reglas de negocio.

### 3.6 Vylersant-Facturacion.Desktop

Es la aplicación principal utilizada por el negocio.

Tecnología:

```text
WPF
.NET 10
```

Experiencias principales previstas:

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

La API utiliza Application, Infrastructure y Contracts.

Desktop utiliza Contracts.

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

La configuración de entidades se realiza mediante Fluent API.

Configuraciones actuales:

```text
BusinessConfiguration
UserConfiguration
RefreshTokenConfiguration
```

La configuración se descubre mediante:

```csharp
modelBuilder.ApplyConfigurationsFromAssembly(
    typeof(VylersantFacturacionDbContext).Assembly);
```

---

## 6. Multi-tenancy

Cada negocio posee un `BusinessId`.

Las entidades pertenecientes a un negocio deberán estar asociadas a dicho identificador.

```text
Business A
├── Users
├── Products
├── Customers
└── Invoices

Business B
├── Users
├── Products
├── Customers
└── Invoices
```

Regla fundamental:

```text
Usuario del Business A
        X
Datos del Business B
```

El JWT incluye `business_id` para apoyar el aislamiento multi-tenant en la API.

---

## 7. Configuración de la aplicación

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

Program.cs registra:

```csharp
builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);
```

Infrastructure centraliza sus dependencias mediante `AddInfrastructure(...)`.

Application centraliza sus servicios mediante `AddApplication()`.

Objetivo:

```text
Program.cs
   ↓
AddApplication()
AddInfrastructure()
```

---

## 9. OpenAPI y Swagger

Durante Development la API expone:

```text
/openapi/v1.json
/swagger
```

Swagger UI se utiliza para probar endpoints durante desarrollo.

No debe habilitarse públicamente en Production salvo necesidad explícita y controles adecuados.

---

## 10. Business

Entidad inicial:

```text
Business
├── Id
├── Name
├── IsActive
├── Address
├── Phone
└── Email
```

Reglas:

```text
Id debe ser válido.
Name es obligatorio.
Name no puede estar vacío.
Name se normaliza con Trim().
Business nace activo.
```

Comportamientos:

```csharp
business.Activate();
business.Deactivate();
```

---

## 11. User

Entidad actual:

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

Reglas:

```text
BusinessId obligatorio.
Name obligatorio.
Email obligatorio.
PasswordHash obligatorio.
Role válido.
User nace activo.
```

El email se normaliza con `Trim()` y `ToLowerInvariant()`.

Decisión actual:

```text
Email único globalmente.
```

Esto permite login mediante correo y contraseña sin pedir código de negocio.

---

## 12. Roles

Roles actuales:

```text
Owner
Supervisor
Employee
```

Cada usuario tiene un único rol.

El rol puede cambiar mediante:

```csharp
user.ChangeRole(UserRole.Supervisor);
```

Los permisos detallados se implementarán cuando aparezcan módulos reales.

---

## 13. Password hashing

Las contraseñas utilizan:

```text
IPasswordHasher
        ↓
BCryptPasswordHasher
        ↓
BCrypt
```

Flujo:

```text
Password
   ↓
BCrypt
   ↓
PasswordHash
   ↓
Users.PasswordHash
```

Nunca se almacena la contraseña original.

---

## 14. Registro de negocio y propietario

Endpoint:

```http
POST /api/auth/register
```

Datos:

```text
BusinessName
OwnerName
Email
Password
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
Crear User
Role = Owner
        ↓
Guardar Business
        ↓
Guardar User
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
POST /api/auth/login
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
   ↓
Respuesta
```

La respuesta contiene:

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

Duración actual de desarrollo: 30 minutos.

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

Ejemplo:

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

Autenticación responde: `¿Quién eres?`

Autorización responde: `¿Qué puedes hacer?`

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

Requiere Bearer Token y devuelve:

```text
UserId
BusinessId
Name
Email
Role
```

Sin token: `401 Unauthorized`.

Con JWT válido: `200 OK`.

---

## 20. Refresh Tokens

El Refresh Token permite renovar la sesión sin pedir nuevamente usuario y contraseña.

Configuración:

```text
RefreshToken:ExpirationDays
```

Duración actual: 7 días.

Flujo:

```text
Access Token expira
       ↓
Desktop conserva Refresh Token
       ↓
POST /api/auth/refresh
       ↓
API valida Refresh Token
       ↓
Nuevo Access Token
       ↓
Nuevo Refresh Token
```

---

## 21. Seguridad de Refresh Tokens

El token original NO se almacena en SQL Server.

```text
Refresh Token original
       ↓
SHA-256
       ↓
TokenHash
       ↓
SQL Server
```

Los tokens se generan mediante `RandomNumberGenerator`.

---

## 22. Entidad RefreshToken

Propiedades:

```text
Id
UserId
TokenHash
CreatedAtUtc
ExpiresAtUtc
RevokedAtUtc
```

Un token está activo cuando:

```text
RevokedAtUtc == null
&&
CurrentTime < ExpiresAtUtc
```

---

## 23. Varias sesiones por usuario

Decisión actual: un usuario puede tener múltiples sesiones activas.

```text
Owner
├── PC de caja
│      └── Refresh Token A
├── Laptop
│      └── Refresh Token B
└── Otra PC
       └── Refresh Token C
```

`RefreshTokens.UserId` no es único.

---

## 24. Rotación de Refresh Tokens

```text
Refresh Token A
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

Después:

```text
A → revocado
B → activo
```

---

## 25. Renovación de sesión

Endpoint:

```http
POST /api/auth/refresh
```

Request:

```json
{
  "refreshToken": "..."
}
```

Flujo:

```text
Recibir Refresh Token
       ↓
Calcular SHA-256
       ↓
Buscar TokenHash
       ↓
¿Existe?
       ↓
¿Está activo?
       ↓
Buscar User
       ↓
¿User existe?
       ↓
¿User está activo?
       ↓
Revocar token anterior
       ↓
Generar nuevo Refresh Token
       ↓
Guardar hash nuevo
       ↓
Generar nuevo Access Token
       ↓
SaveChanges
       ↓
Respuesta
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
Credenciales inválidas → 401 Unauthorized
Refresh Token inválido → 401 Unauthorized
Email ya registrado → 409 Conflict
Argumentos inválidos → 400 Bad Request
Error inesperado → 500 Internal Server Error
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

Los contratos viven en Application y las implementaciones en Infrastructure.

---

## 28. Unit of Work

Se utiliza `IUnitOfWork` para confirmar operaciones que abarcan varios repositorios.

Implementación actual: `VylersantFacturacionDbContext`.

---

## 29. Base de datos actual

Tablas principales:

```text
Businesses
Users
RefreshTokens
__EFMigrationsHistory
```

Relaciones:

```text
Users.BusinessId → Businesses.Id
RefreshTokens.UserId → Users.Id
```

Índices:

```text
Users.Email → UNIQUE
RefreshTokens.TokenHash → UNIQUE
```

---

## 30. Migraciones

Migraciones principales:

```text
InitialCreate
AddBusinessAndUsers
AddRefreshTokens
```

Regla:

```text
Crear migración
↓
Revisar migración
↓
Aplicar database update
↓
Build
↓
Tests
```

---

## 31. Arquitectura offline futura

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

Sin Internet:

```text
Facturar    ✅
Cobrar      ✅
Imprimir    ✅
Guardar     ✅
Sincronizar ⏳
```

La sincronización deberá considerar UUID, idempotencia, operaciones pendientes, reintentos, duplicados, conflictos y estado de sincronización.

---

## 32. Impresión futura

```text
Venta
  ↓
IPrintService
  ↓
Implementación ESC/POS
  ↓
Impresora térmica
```

Objetivo: que Invoice y Sales no conozcan marcas o modelos de impresoras.

---

## 33. Dashboard futuro

Indicadores previstos:

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

El Dashboard formará parte de WPF.

---

## 34. Inventario futuro

No es obligatorio para el MVP.

Podrá incorporarse posteriormente con:

```text
Existencias
Compras
Entradas
Salidas
Stock mínimo
Ajustes
```

---

## 35. Flujo de Git

Ramas principales:

```text
main
develop
```

Trabajo:

```text
feature/<issue>-descripcion
fix/<issue>-descripcion
chore/<issue>-descripcion
docs/<issue>-descripcion
```

---

## 36. Flujo de trabajo por Issue

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

## 37. Commits

Ejemplos:

```text
feat: implementar autenticacion de usuario (#17)
feat: implementar JWT access token (#18)
feat: exponer autenticacion y proteger endpoints (#19)
feat: implementar refresh tokens y rotacion de sesiones (#20)
docs: documentar autenticacion y sesiones (#22)
```

---

## 38. Pruebas

### Domain Tests

```text
Business
User
Roles
RefreshToken
```

### Application Tests

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

### Infrastructure Tests

```text
BCrypt
JWT
Claims
Firma JWT
Firma inválida
Generación de Refresh Token
```

### Integration Tests

```text
/me sin token → 401
/me con token válido → 200
Login inválido → 401
Email duplicado → 409
Login → Refresh → nuevos tokens
Reutilizar Refresh Token anterior → 401
```

Regla antes de integrar:

```powershell
dotnet test
dotnet build
```

---

## 39. Flujo completo de autenticación

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

## 40. Responsabilidades futuras de Desktop

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

La estrategia de almacenamiento seguro de credenciales de sesión en Windows todavía debe diseñarse.

---

## 41. Principios actuales

1. Domain no conoce tecnologías externas.
2. Application depende de abstracciones, no de implementaciones.
3. Infrastructure implementa contratos técnicos.
4. Desktop es la interfaz principal del producto.
5. La API funciona como backend seguro.
6. La lógica de negocio no debe vivir en WPF.
7. La lógica de negocio no debe vivir en Controllers.
8. Las contraseñas nunca se almacenan en texto plano.
9. BCrypt se utiliza para contraseñas.
10. Los Refresh Tokens se generan criptográficamente.
11. Solo se almacena el hash SHA-256 del Refresh Token.
12. Los Refresh Tokens se rotan al utilizarse.
13. Los tokens revocados no pueden reutilizarse.
14. Los usuarios inactivos no pueden iniciar sesión.
15. Los usuarios inactivos no pueden renovar sesiones.
16. Las claves JWT no deben almacenarse en el repositorio.
17. Cada usuario puede tener múltiples sesiones.
18. Los claims incluyen BusinessId para soportar multi-tenancy.
19. Los repositorios exponen solo operaciones necesarias.
20. Las abstracciones se agregan cuando existe una necesidad real.
21. Antes de integrar cambios se ejecutan build y tests.
22. Cada feature relevante tiene Issue y rama propia.
23. main se mantiene estable.
24. develop funciona como rama de integración.

---

## 42. Estado actual del proyecto

### Sprint 0 — Fundación

Completado:

```text
Solution .NET 10
WPF Desktop
Domain
Application
Infrastructure
Contracts
API
Tests
EF Core
SQL Server
DbContext
Migraciones
Dependency Injection
OpenAPI
Swagger
Environments
User Secrets
Git
Ramas
Issues
Pull Requests
Documentación inicial
```

### Sprint 1 — Negocio, usuarios y autenticación

Completado:

```text
Business
User
Roles
Mapeo EF Core
Repositorios
BCrypt
Registro negocio + propietario
Login
JWT Access Token
Endpoints HTTP
/me protegido
GlobalExceptionHandler
ProblemDetails
Refresh Tokens
Múltiples sesiones
Rotación
Pruebas unitarias
Pruebas de infraestructura
Pruebas de integración
```

---

## 43. Próximas áreas

```text
Productos
Clientes
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
