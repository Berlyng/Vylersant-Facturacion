# Arquitectura de Vylersant Facturacion

## 1. Objetivo

Vylersant Facturacion es una aplicación de escritorio orientada a pequeños
negocios que permite realizar operaciones de facturación y convertir las
ventas registradas en información analítica para el propietario.

La aplicación principal será desarrollada utilizando WPF y .NET.

La API no constituye la interfaz principal del producto. Su responsabilidad
es proporcionar servicios de backend relacionados con persistencia en nube,
sincronización, autenticación y funcionalidades SaaS.

---

## 2. Arquitectura general

```text
┌──────────────────────────────┐
│   Facturacion.Desktop        │
│      WPF / .NET 10           │
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
│      Facturacion.Api         │
│    ASP.NET Core / .NET 10    │
└──────────────┬───────────────┘
               │
               ▼
┌──────────────────────────────┐
│ Facturacion.Application      │
└──────────────┬───────────────┘
               │
               ▼
┌──────────────────────────────┐
│    Facturacion.Domain        │
└──────────────────────────────┘

        Infrastructure
              │
              ▼
         SQL Server
```

---
## 3. Proyectos
Vylersant-Facturacion.Domain
Contiene el modelo de dominio y las reglas de negocio.
Ejemplos futuros:
- Business
- User
- Product
- Customer
- Invoice
- Payment
Domain no debe depender de Entity Framework Core, SQL Server, WPF,
ASP.NET Core ni otras tecnologías de infraestructura.



 Vylersant-Facturacion.Application
Contiene los casos de uso de la aplicación.
Ejemplos futuros:
- Crear producto
- Registrar cliente
- Registrar venta
- Iniciar sesión
- Obtener estadísticas
Depende de:
Vylersant-Facturacion.Domain

Application define qué necesita el sistema, pero evita conocer detalles
de infraestructura.


 Vylersant-Facturacion.Infrastructure
Contiene implementaciones técnicas.
Actualmente contiene:
- Entity Framework Core
- VylersantFacturacionDbContext
- SQL Server
- Registro de dependencias
En el futuro podrá contener:
- Repositorios
- Password hashing
- JWT
- Servicios externos
- Persistencia
- Auditoría
Depende de:
Vylersant-Facturacion.Application
Vylersant-Facturacion.Domain



 Vylersant-Facturacion.Api
Es el backend del sistema.
Responsabilidades futuras:
- Exponer endpoints
- Autenticación
- Sincronización
- Acceso a información en nube
- Analítica remota
- Servicios SaaS
La API actúa como Composition Root.
Actualmente registra Infrastructure mediante:
builder.Services.AddInfrastructure(builder.Configuration);

La API no constituye la interfaz principal del usuario.



 Vylersant-Facturacion.Contracts
Contiene contratos utilizados para comunicación entre clientes y API.
Ejemplos futuros:
- LoginRequest
- LoginResponse
- ProductResponse
- CreateInvoiceRequest
Los contratos no deben contener reglas de negocio.


 Vylersant-Facturacion.Desktop
Aplicación principal utilizada por el negocio.
Tecnología:
WPF
.NET 10

Contendrá dos experiencias principales según permisos:
Empleado
    Facturación
    Productos permitidos
    Clientes
    Historial permitido

Propietario
    Dashboard
    Facturación
    Reportes
    Analítica
    Usuarios
    Configuración

Actualmente Desktop depende únicamente de:
Facturacion.Contracts

---

## 4. Dependencias entre proyectos

La dirección principal es:
```
Domain
   ↑
Application
   ↑
Infrastructure

API utiliza:
Application
Infrastructure
Contracts

Desktop utiliza actualmente:
Contracts

No se permiten dependencias como:
Domain → Infrastructure
Domain → EF Core
Domain → SQL Server
Domain → WPF

```

---

## 5. Persistencia actual
La persistencia en nube utiliza:
Entity Framework Core 10
SQL Server

El DbContext principal es:
VylersantFacturacionDbContext

La configuración de SQL Server se realiza mediante Dependency Injection.
Las cadenas de conexión no se encuentran hardcodeadas dentro de clases.

---

## 6. Configuración
Se utilizan:
appsettings.json
appsettings.Development.json
appsettings.Production.json
User Secrets
Variables de entorno

Development puede utilizar configuración específica de la máquina local.
Production deberá obtener secretos y cadenas de conexión mediante
mecanismos externos seguros.

---

## 7. OpenAPI
Durante Development la API expone documentación OpenAPI.
/openapi/v1.json
/swagger

Swagger UI no debe habilitarse públicamente en Production salvo que exista
una necesidad explícita y controles adecuados.

---

## 8. Arquitectura offline futura
La aplicación de escritorio deberá continuar funcionando temporalmente
cuando no exista conexión a Internet.
Arquitectura prevista:

```
Desktop
   │
   ▼
SQLServer local
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

Este componente todavía no está implementado.
La sincronización deberá considerar posteriormente:
- Identificadores globales
- Idempotencia
- Operaciones pendientes
- Reintentos
- Duplicados
- Conflictos
- Estado de sincronización

---

## 9. Impresión futura
La impresión térmica se implementará mediante un servicio independiente
de la lógica de ventas.
Conceptualmente:
Venta
  ↓
IPrintService
  ↓
Implementación ESC/POS
  ↓
Impresora térmica

Esto permitirá soportar diferentes impresoras sin acoplar Invoice o Sales
a un modelo específico de hardware.

---

## 10. Flujo de Git
Las ramas principales son:
main
develop

main representa código estable.
develop representa la rama de integración.
El trabajo se realiza mediante ramas específicas:
feature/<issue>-descripcion
fix/<issue>-descripcion
chore/<issue>-descripcion
docs/<issue>-descripcion

Ejemplo:
chore/6-configurar-dependency-injection

Flujo:
Issue
  ↓
Rama desde develop
  ↓
Implementación
  ↓
Build / Tests
  ↓
Commit
  ↓
Push
  ↓
Pull Request hacia develop
  ↓
Merge
  ↓
Cerrar Issue

## 11. Principios iniciales
1. Domain no conoce tecnologías externas.
2. La lógica de negocio no debe colocarse en WPF.
3. La lógica de negocio no debe colocarse en Controllers.
4. Las credenciales no deben hardcodearse.
5. Cada feature relevante debe estar asociada a un Issue.
6. Cada Issue debe desarrollarse en su propia rama.
7. Antes de integrar cambios deben ejecutarse build y tests.
8. Se prioriza simplicidad antes que abstracciones prematuras.


---

## 2. Verificación

Aunque solamente modificamos documentación, ejecutaremos:

```powershell
dotnet build
dotnet test