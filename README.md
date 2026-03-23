# GestorPacientes — Prueba Técnica UT San Vicente CES

## Video de demostración
[Ver video en Google Drive](https://drive.google.com/file/d/16tmdH2KbcdoepREqt2JkjrBklf1HUJOR/view?usp=sharing)

---

## Descripción

Sistema de gestión de pacientes desarrollado como solución a la prueba técnica de la 
UT San Vicente CES. El objetivo era refactorizar un módulo con malas prácticas aplicando 
Clean Architecture, agregar funcionalidades críticas y construir un frontend funcional.

---

## Stack tecnológico

| Capa | Tecnología |
|---|---|
| Backend | ASP.NET Core 10 |
| ORM | Entity Framework Core |
| Base de datos | SQL Server |
| Frontend | MVC + Tailwind CSS |
| Validaciones | FluentValidation |
| Mapeo | Mapster |
| Caché | OutputCache (ASP.NET Core) |
| Documentación API | Swagger / OpenAPI |

---

## Arquitectura

El proyecto está estructurado en 4 capas siguiendo los principios de **Clean Architecture**:
```
GestorPacientes.sln
├── GestorPacientes.Domain          # Núcleo — entidades, interfaces, excepciones
├── GestorPacientes.Application     # Casos de uso, DTOs, validaciones, mappings
├── GestorPacientes.Infrastructure  # Persistencia — DbContext, repositorios
├── GestorPacientes.API             # Web API REST con Swagger
└── GestorPacientes.Web             # MVC — controladores y vistas
```

### Dirección de dependencias

Domain no conoce a nadie. Application conoce Domain. Infrastructure y Web conocen 
Application y Domain. Ninguna capa se salta este orden.

---

## Capas en detalle

### Domain
Es el núcleo del sistema y no depende de ninguna otra capa. Contiene:

- **Entidades** — `Paciente` con lógica de negocio encapsulada
- **Interfaces** — `IPacienteRepository` define el contrato sin saber nada de EF Core
- **Excepciones** — `ExcepcionNegocio` para reglas de dominio violadas

La entidad `Paciente` aplica el patrón **Factory Method** — el constructor es privado 
y la única forma de crear un paciente válido es a través de `Paciente.Crear(...)`, 
garantizando que nunca pueda existir en un estado inválido.

### Application
Orquesta los casos de uso del sistema. Contiene:

- **Interfaces de servicio** — `IPacienteService`
- **Casos de uso** — `PacienteService` implementa la lógica de negocio coordinando 
  dominio e infraestructura
- **DTOs** — objetos de transferencia separados por responsabilidad
- **Validators** — FluentValidation para validación de entrada
- **Mappings** — configuración de Mapster para mapeo entre entidades y DTOs

### Infrastructure
Único lugar que sabe que existe una base de datos. Contiene:

- **DbContext** — `ApplicationDbContext` con configuración de EF Core
- **Repositorios** — implementación concreta de `IPacienteRepository`
- **Extensions** — `IQueryableExtensions` para paginación reutilizable

### Web (MVC)
Capa de presentación. Elegí MVC sobre Razor Pages porque la prueba pedía un buscador 
funcional con AJAX/Fetch, lo cual encaja mejor con controllers que exponen endpoints 
`[HttpGet]` y `[HttpPost]` retornando JSON directamente al frontend.

### API (Capa adicional)
Aunque la prueba solicitaba únicamente un frontend MVC, se incluyó una capa adicional 
`GestorPacientes.API` como proyecto Web API independiente con Swagger configurado. 

Esto cumple dos propósitos:

- **Pruebas de endpoints** — permite probar y documentar todos los endpoints 
  directamente desde Swagger sin necesidad del frontend
- **Escalabilidad** — si en el futuro se quiere conectar un framework externo 
  como React, Angular o una app móvil, la API REST ya está lista para consumirse 
  sin modificar ninguna capa de negocio

---

## DTOs

Cada DTO tiene una responsabilidad específica y el DTO original de la prueba se respetó 
intacto sin modificaciones:

| DTO | Propósito |
|---|---|
| `PacienteDto` | Contrato original de la prueba — retorno al crear |
| `PacienteListadoDto` | Listado paginado con `Id` para operaciones CRUD |
| `PacienteEdicionModalDto` | Pre-llenado del formulario de edición con `FechaNacimiento` |
| `CrearPacienteDto` | Entrada para registrar un nuevo paciente |
| `ActualizarPacienteDto` | Entrada para editar — el documento es inmutable |
| `PaginacionDto` | Parámetros de paginación con límite máximo de 10 registros |
| `PaginacionResponseDto<T>` | Respuesta paginada genérica con metadata |

---

## Patrones de diseño aplicados

### Repository Pattern
`IPacienteRepository` abstrae el acceso a datos. Los servicios nunca saben si los datos 
vienen de SQL Server u otra fuente — solo hablan con la interfaz. Esto desacopla la 
lógica de negocio de la persistencia y facilita el testing.

### Factory Method en la Entidad
El constructor de `Paciente` es privado. La única forma de crear un paciente es 
a través de `Paciente.Crear(...)` que valida todas las reglas antes de retornar 
el objeto. Es imposible crear un paciente inválido por construcción.
```csharp
public static Paciente Crear(string numeroDocumento, string nombreCompleto, 
    DateTime fechaNacimiento, string email)
{
    if (string.IsNullOrWhiteSpace(numeroDocumento))
        throw new ExcepcionNegocio("El número de documento es obligatorio.");
    // ... más validaciones
    return new Paciente { ... };
}
```

### Adapter
Mapster actúa como adaptador entre el modelo de dominio y los DTOs. La entidad 
`Paciente` tiene propiedades como `NumeroDocumento` y `NombreCompleto`, mientras 
que `PacienteDto` las expone como `Documento` y `Nombre`. Mapster traduce entre 
estos dos "lenguajes" sin que ninguna de las dos capas tenga que conocerse entre sí.
```csharp
config.NewConfig()
    .Map(dest => dest.Documento, org => org.NumeroDocumento)
    .Map(dest => dest.Nombre, org => org.NombreCompleto)
    .Map(dest => dest.Edad, org => org.CalcularEdad());
```

## Principios SOLID que aplique en la prueba

### S — Single Responsibility
Cada clase tiene un único motivo para cambiar, por ejemplo:
- `PacienteRepository` solo persiste y consulta datos
- `PacienteService` solo orquesta casos de uso
- `CrearPacienteValidator` solo valida la entrada de creación
- `PacienteMapsterConfig` solo define los mappings

### O — Open/Closed
Las interfaces `IPacienteRepository` e `IPacienteService` permiten extender el 
comportamiento sin modificar el código existente. Si se quisiera cambiar SQL Server 
por MongoDB, solo se crea una nueva implementación del repositorio sin tocar 
Application ni Domain.

### L — Liskov Substitution
`PacienteRepository` implementa completamente `IPacienteRepository` y 
`PacienteService` implementa completamente `IPacienteService`. Cualquier 
implementación alternativa puede sustituirse sin afectar el comportamiento 
del sistema.

### I — Interface Segregation
Las interfaces son específicas y cohesivas — `IPacienteRepository` solo expone 
lo que el servicio necesita consultar o persistir, sin métodos genéricos 
innecesarios.

### D — Dependency Inversion
Las capas de alto nivel (Application) no dependen de las de bajo nivel 
(Infrastructure). Ambas dependen de abstracciones (interfaces definidas en Domain). 
El controlador recibe `IPacienteService`, no `PacienteService` directamente.


## Estrategia de persistencia

### Soft Delete
Los pacientes nunca se eliminan físicamente. Se desactivan con `EstaActivo = false`. 
Esto mantiene la integridad histórica de los registros clínicos y cumple con la 
regla de negocio de permitir reutilizar documentos de pacientes inactivos.

---

## Reglas de negocio implementadas

- No se permite registrar un paciente con un `NumeroDocumento` que ya exista 
  y esté marcado como **Activo**
- El `NumeroDocumento` es inmutable — no se puede modificar después de creado
- La edad se calcula dinámicamente en el backend desde `FechaNacimiento`, 
  nunca se almacena
- Un paciente desactivado puede volver a registrarse con el mismo documento

---

## Funcionalidades

- ✅ Listado paginado de pacientes activos
- ✅ Buscador en tiempo real por nombre o documento (debounce 400ms)
- ✅ Registro de nuevos pacientes con validación de duplicados activos
- ✅ Edición de pacientes (documento inmutable)
- ✅ Desactivación de pacientes (soft delete)
- ✅ Exportación a Excel dinámica — todos los pacientes o filtrados por búsqueda
- ✅ Cache de consultas con invalidación automática al mutar datos
- ✅ Feedback visual con toasts y modales de confirmación

---

## Cómo ejecutar el proyecto

### Prerrequisitos
- .NET 10 SDK
- SQL Server

### Pasos

**1. Clonar el repositorio:**
```bash
git clone https://github.com/samuelosorio-dev/GestorPacientes.git
cd GestorPacientes
```

**2. Configurar la cadena de conexión en `GestorPacientes.Web/appsettings.json`:**
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=TU_SERVIDOR;Database=GestorPacientesDB;Trusted_Connection=True;"
  }
}
```

**3. Aplicar migraciones:**
```bash
dotnet ef database update --project GestorPacientes.Infrastructure --startup-project GestorPacientes.Web
```

**4. Ejecutar:**
```bash
cd GestorPacientes.Web
dotnet run
```

**5. Abrir en el navegador:**
```
https://localhost:7209/Pacientes
```

---

## Seguridad

La estrategia de protección de endpoints con Azure Active Directory está documentada 
en el archivo [`AzureAd.md`](./AzureAd.md).

---

## Estructura del proyecto
```
GestorPacientes/
├── GestorPacientes.Domain/
│   ├── Entities/
│   │   └── Paciente.cs
│   ├── Interfaces/
│   │   └── IPacienteRepository.cs
│   └── Exceptions/
│       └── ExcepcionNegocio.cs
├── GestorPacientes.Application/
│   ├── DTOs/
│   │   ├── Paciente/
│   │   └── Paginacion/
│   ├── Interfaces/
│   │   └── IPacienteService.cs
│   ├── UseCases/
│   │   └── PacienteService.cs
│   ├── Mappings/
│   │   └── PacienteMapsterConfig.cs
│   └── Validators/
│       ├── CrearPacienteValidator.cs
│       └── ActualizarPacienteValidator.cs
├── GestorPacientes.Infrastructure/
│   ├── Context/
│   │   └── ApplicationDbContext.cs
│   ├── Repositories/
│   │   └── PacienteRepository.cs
│   └── Extensions/
│       └── IQueryableExtensions.cs
└── GestorPacientes.Web/
    ├── Controllers/
    │   └── PacientesController.cs
    ├── Views/
    │   ├── Pacientes/
    │   │   ├── Index.cshtml
    │   │   ├── _ModalPaciente.cshtml
    │   │   └── _ModalConfirmar.cshtml
    │   └── Shared/
    │       └── _Layout.cshtml
    └── wwwroot/
        └── js/
            └── pacientes.js
```

---

*Desarrollado por Samuel Osorio — Prueba Técnica UT San Vicente CES*
