# Flujo de Autenticaci�n - 4cima

Este documento describe el flujo completo de autenticaci�n en la aplicaci�n 4cima, incluyendo login, redirecci�n por rol, cambio de contrase�a obligatorio y recuperaci�n de contrase�a.

## Diagrama General del Flujo

```mermaid
flowchart TD
    subgraph "Acceso Inicial"
        A[Usuario accede a ruta] --> B{�Ruta protegida?}
        B -->|No| C[Mostrar p�gina p�blica]
        B -->|S�| D{�Autenticado?}
    end

    subgraph "Verificaci�n de Autenticaci�n"
        D -->|No| E[RedirectToLogin.razor]
        E --> F[ABP Account/Login<br/>con returnUrl]
        D -->|S�| G{�Tiene permisos?}
        G -->|No| H[AccessDenied.razor]
        G -->|S�| I[MustChangePasswordGuard]
    end

    subgraph "Flujo de Login ABP"
        F --> J[P�gina de Login]
        J --> K{Credenciales v�lidas?}
        K -->|No| L[Mostrar error]
        L --> J
        K -->|S�| M[Crear sesi�n]
        M --> N{Remember Me?}
        N -->|S�| O[Cookie persistente<br/>14 d�as]
        N -->|No| P[Cookie de sesi�n]
        O --> Q[Redirect post-login]
        P --> Q
    end

    subgraph "Verificaci�n MustChangePassword"
        I --> R{�MustChangePassword?}
        R -->|S�| S[Redirect a<br/>/account/change-password]
        S --> T[ChangePassword.razor]
        T --> U{Contrase�a cambiada?}
        U -->|No| T
        U -->|S�| V[Actualizar flag]
        V --> W[Redirect a destino original]
        R -->|No| X[Continuar al destino]
    end

    subgraph "Redirecci�n por Rol"
        Q --> Y[PostLogin.razor]
        Y --> Z{Verificar rol}
        Z -->|Admin| AA[/admin/dashboard]
        Z -->|Architect| AB[/architect/dashboard]
        Z -->|returnUrl v�lido| AC[returnUrl]
        Z -->|Otro| AD[/]
    end

    X --> AE[Mostrar p�gina solicitada]

    style A fill:#e1f5fe
    style J fill:#fff3e0
    style T fill:#fce4ec
    style Y fill:#e8f5e9
    style H fill:#ffebee
```

## Flujo de Recuperaci�n de Contrase�a

```mermaid
flowchart TD
    subgraph "Solicitud de Recuperaci�n"
        A[Usuario en Login] --> B[Click: �Olvidaste tu contrase�a?]
        B --> C[ABP Account/ForgotPassword]
        C --> D[Ingresar email]
        D --> E{�Email existe?}
        E -->|No| F[Mostrar mensaje gen�rico<br/>por seguridad]
        E -->|S�| G[Generar token reset]
    end

    subgraph "Env�o de Email"
        G --> H[Crear URL con token]
        H --> I[Enviar email<br/>SmtpEmailNotificationService<br/>o AzureEmailNotificationService]
        I --> J[Mostrar confirmaci�n]
    end

    subgraph "Reset de Contrase�a"
        K[Usuario abre email] --> L[Click en enlace]
        L --> M[ABP Account/ResetPassword]
        M --> N{�Token v�lido?}
        N -->|No| O[Token expirado/inv�lido]
        N -->|S�| P[Formulario nueva contrase�a]
        P --> Q{Contrase�a v�lida?}
        Q -->|No| R[Mostrar errores<br/>validaci�n]
        R --> P
        Q -->|S�| S[Actualizar contrase�a]
        S --> T[Redirect a Login]
        T --> U[Mostrar �xito]
    end

    style A fill:#e1f5fe
    style I fill:#fff3e0
    style S fill:#e8f5e9
    style O fill:#ffebee
```

## Componentes Involucrados

### Cliente (Blazor WebAssembly)

| Componente | Ruta | Descripci�n |
|------------|------|-------------|
| `RedirectToLogin.razor` | - | Redirige a ABP Login preservando returnUrl |
| `PostLogin.razor` | `/account/post-login` | Maneja redirecci�n post-login por rol |
| `AccessDenied.razor` | `/account/access-denied` | P�gina de acceso denegado |
| `ChangePassword.razor` | `/account/change-password` | Cambio de contrase�a (obligatorio o voluntario) |
| `MustChangePasswordGuard.razor` | - | Guarda que verifica si debe cambiar contrase�a |
| `LoginRedirectService.cs` | - | Servicio de l�gica de redirecci�n |

### Servidor (ABP Account Module)

| P�gina | Ruta | Descripci�n |
|--------|------|-------------|
| Login.cshtml | `/Account/Login` | P�gina de login |
| ForgotPassword.cshtml | `/Account/ForgotPassword` | Solicitar reset |
| ResetPassword.cshtml | `/Account/ResetPassword` | Cambiar contrase�a con token |
| Logout | `/Account/Logout` | Cerrar sesi�n |

## Configuraci�n de Remember Me

ABP Identity configura autom�ticamente la cookie de autenticaci�n. La opci�n "Remember Me" en el login:

- **Activada**: Cookie persiste 14 d�as (configurable)
- **Desactivada**: Cookie expira al cerrar el navegador

### Configurar duraci�n de Remember Me

```csharp
// En cimaBlazorModule.cs o IdentityModule
Configure<SecurityStampValidatorOptions>(options =>
{
    options.ValidationInterval = TimeSpan.FromMinutes(30);
});

Configure<IdentityOptions>(options =>
{
    // Duraci�n del refresh token
    options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(5);
});
```

## Configuraci�n de Recuperaci�n de Contrase�a

### 1. Configurar servicio de email

En `appsettings.json`:

```json
{
  "Smtp": {
    "Host": "smtp.example.com",
    "Port": 587,
    "UserName": "noreply@4cima.com",
    "Password": "secret",
    "EnableSsl": true,
    "From": "noreply@4cima.com",
    "FromDisplayName": "4cima - No Reply"
  }
}
```

O para Azure Communication Services:

```json
{
  "Azure": {
    "Communication": {
      "ConnectionString": "endpoint=https://xxx.communication.azure.com/;accesskey=xxx",
      "SenderEmail": "noreply@4cima.com"
    }
  }
}
```

### 2. Servicios de Email Disponibles

```
src/cima.Application/Notifications/
??? SmtpEmailNotificationService.cs      # SMTP tradicional
??? AzureEmailNotificationService.cs     # Azure Communication
??? EmailNotificationServiceExtensions.cs # Registro de servicios
```

### 3. Plantillas de Email

ABP usa el sistema de Virtual File System para plantillas. Las plantillas predeterminadas est�n en:

```
Volo.Abp.Account.Web/Pages/Account/
??? ForgotPassword.cshtml
??? ResetPassword.cshtml
??? EmailTemplates/
    ??? PasswordResetLink.tpl
```

Para personalizar, crear en el proyecto Blazor:

```
src/cima.Blazor/Pages/Account/
??? ForgotPassword.cshtml (override)
??? ResetPassword.cshtml (override)
??? EmailTemplates/
    ??? PasswordResetLink.tpl (override)
```

## Flujo de MustChangePassword

```mermaid
sequenceDiagram
    participant U as Usuario
    participant G as MustChangePasswordGuard
    participant API as ArchitectAppService
    participant CP as ChangePassword.razor
    participant DB as Database

    U->>G: Navega a p�gina protegida
    G->>G: �Ruta excluida?
    alt Ruta excluida
        G->>U: Continuar sin verificar
    else Ruta normal
        G->>API: MustChangePasswordAsync()
        API->>DB: SELECT MustChangePassword FROM Architects
        DB-->>API: true/false
        API-->>G: mustChange
        alt mustChange = true
            G->>CP: Redirect /account/change-password
            CP->>U: Mostrar formulario
            U->>CP: Nueva contrase�a
            CP->>API: ChangePasswordAsync(dto)
            API->>DB: UPDATE password, MustChangePassword=false
            DB-->>API: OK
            API-->>CP: Success
            CP->>U: Redirect a destino original
        else mustChange = false
            G->>U: Mostrar p�gina solicitada
        end
    end
```

## Rutas Excluidas del Guard

El `MustChangePasswordGuard` no verifica estas rutas para evitar loops:

```csharp
private static readonly string[] ExcludedPaths = 
{
    "/account/change-password",
    "/account/login",
    "/account/logout",
    "/account/register",
    "/authentication",
    "/signin-oidc",
    "/signout-callback-oidc"
};
```

## Seguridad

### Validaci�n de ReturnUrl

El `LoginRedirectService` valida returnUrl para prevenir open redirect:

```csharp
private bool IsValidReturnUrl(string returnUrl)
{
    // Solo URLs relativas
    if (returnUrl.StartsWith("/") && !returnUrl.StartsWith("//"))
        return true;

    // URLs del mismo origen
    var baseUri = new Uri(_navigation.BaseUri);
    var returnUri = new Uri(returnUrl, UriKind.RelativeOrAbsolute);
    return returnUri.Host == baseUri.Host;
}
```

### Tokens de Reset

- Tokens de reset tienen expiraci�n configurable (default: 24h)
- Son de un solo uso
- Se invalidan al cambiar la contrase�a

## Logs y Diagn�stico

El sistema genera logs para diagn�stico:

```
[INF] Usuario debe cambiar contrase�a, redirigiendo...
[DBG] Token no autorizado durante verificaci�n de cambio de contrase�a
[DBG] Sin permisos para verificar cambio de contrase�a
[WRN] Error durante verificaci�n de cambio de contrase�a obligatorio
```

## Pr�ximos Pasos

1. [ ] Personalizar UI de Login con branding CIMA
2. [ ] Configurar plantillas de email personalizadas
3. [ ] Implementar 2FA (opcional)
4. [ ] Configurar pol�tica de contrase�as
5. [ ] Implementar bloqueo por intentos fallidos
