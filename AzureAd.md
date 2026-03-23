# Seguridad de Endpoints con Azure Active Directory (Azure AD)

## Enfoque

Para proteger los endpoints de este sistema, integraría **Azure Active Directory (Azure AD)** 
como proveedor de identidad. La razón principal es simple: delegar la autenticación a un 
servicio probado evita tener que implementar y mantener mecanismos propios que pueden 
introducir vulnerabilidades.

Azure AD trabaja sobre estándares como **OAuth 2.0** y **OpenID Connect**, y emite 
**tokens JWT firmados** que ASP.NET Core valida automáticamente en el middleware, 
sin necesidad de lógica adicional en los controladores.

---

## Autenticación vs Autorización

Antes de entrar en la implementación, vale la pena separar estos dos conceptos porque 
a menudo se confunden:

- **Autenticación:** Azure AD verifica quién es el usuario.
- **Autorización:** La API decide qué puede hacer ese usuario según sus roles o claims.

---

## Implementación

### 1. Registro de aplicaciones en Azure AD

Se registrarían dos aplicaciones en el portal de Azure:

- **GestorPacientes.API** → expone los endpoints y define los scopes
- **GestorPacientes.Web** → cliente que consume la API autenticado

La API define scopes y roles que luego viajan dentro del token como *claims*.

### 2. Paquetes necesarios
```bash
dotnet add package Microsoft.Identity.Web
dotnet add package Microsoft.Identity.Web.UI
```

### 3. Configuración

**appsettings.json:**
```json
{
  "AzureAd": {
    "Instance": "https://login.microsoftonline.com/",
    "TenantId": "tu-tenant-id",      // Obtenido del portal de Azure
    "ClientId": "tu-client-id",      // ID de la app registrada
    "Audience": "api://tu-client-id"
  }
}
```

**Program.cs:**
```csharp
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddMicrosoftIdentityWebApi(builder.Configuration.GetSection("AzureAd"));

builder.Services.AddAuthorization(options =>
{
    // Política global: todos los endpoints requieren autenticación por defecto
    options.FallbackPolicy = options.DefaultPolicy;
});

app.UseAuthentication();
app.UseAuthorization();
```

### 4. Protección de endpoints

A nivel de controlador:
```csharp
[Authorize]
public class PacientesController : Controller
{
    // Cualquier acción requiere usuario autenticado
}
```

Con control más granular por roles:
```csharp
[Authorize(Roles = "Administrador")]
public async Task<IActionResult> Crear(...) { }

[Authorize(Roles = "Medico,Administrador")]
public async Task<IActionResult> ListarPaginado(...) { }
```

Los roles viajan dentro del token como claims y ASP.NET Core los evalúa 
automáticamente — no hay que parsear el token manualmente.

---

## Flujo de autenticación
```
Usuario → Azure AD → Token JWT → Request con Bearer → API valida → Respuesta
```

1. El usuario inicia sesión en Azure AD con sus credenciales institucionales
2. Azure AD emite un JWT firmado con los claims del usuario
3. El cliente adjunta el token en cada request: `Authorization: Bearer {token}`
4. El middleware de ASP.NET Core valida firma, expiración y claims
5. Token inválido o expirado → `401 Unauthorized`
6. Token válido pero sin permisos → `403 Forbidden`

---

## Roles propuestos para el sistema

| Rol | Permisos |
|---|---|
| `Administrador` | CRUD completo + exportar Excel |
| `Medico` | Consultar y registrar pacientes |
| `Recepcionista` | Solo consultar |

---

## Buenas prácticas aplicadas

- Las contraseñas nunca pasan por la aplicación — Azure AD las gestiona
- Los tokens tienen expiración limitada (~1 hora), reduciendo la ventana de ataque
- Se puede habilitar **MFA** desde Azure AD sin tocar una línea de código
- Soporte de **SSO** con cuentas institucionales del hospital
- **HTTPS** obligatorio en todos los endpoints (ya configurado en el proyecto)

---

## Consideraciones para datos de pacientes

Dado que el sistema maneja información clínica sensible, adicionalmente recomendaría:

- **Conditional Access**: exigir MFA cuando el acceso proviene de fuera de la red institucional
- **Tiempos de expiración cortos**: máximo 1 hora por token
- **Auditoría**: registrar todos los accesos a datos de pacientes con usuario, fecha y acción
- **Principio de mínimo privilegio**: cada rol accede solo a lo estrictamente necesario

---

## Por qué esta estrategia encaja con Clean Architecture

Una ventaja que me parece importante destacar es que esta implementación mantiene la 
seguridad completamente desacoplada de la lógica de negocio. Los servicios y repositorios 
no saben nada de autenticación — eso vive en el middleware y los controladores. 
Si en el futuro se cambia el proveedor de identidad, el dominio y la aplicación 
no se tocan.