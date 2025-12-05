# Plantillas de Email - 4cima

Este documento describe las plantillas de email utilizadas en la aplicación y cómo personalizarlas.

## Plantillas Disponibles

### 1. Bienvenida a Arquitecto (`ArchitectWelcome`)

Enviado cuando se crea un nuevo arquitecto con contraseña temporal.

**Datos disponibles:**
- `{ArchitectName}` - Nombre del arquitecto
- `{TemporaryPassword}` - Contraseña temporal
- `{LoginUrl}` - URL de login

**Plantilla por defecto:**

```html
<!DOCTYPE html>
<html>
<head>
    <meta charset="utf-8">
    <style>
        body { font-family: 'Segoe UI', Tahoma, sans-serif; line-height: 1.6; color: #333; }
        .container { max-width: 600px; margin: 0 auto; padding: 20px; }
        .header { background: #1d2a4b; color: white; padding: 30px; text-align: center; }
        .header h1 { margin: 0; font-size: 28px; }
        .content { padding: 30px; background: #f9fafb; }
        .password-box { background: #fff; border: 2px solid #1d2a4b; border-radius: 8px; 
                       padding: 20px; text-align: center; margin: 20px 0; }
        .password { font-size: 24px; font-weight: bold; color: #1d2a4b; letter-spacing: 2px; }
        .btn { display: inline-block; background: #1d2a4b; color: white; padding: 12px 30px; 
               text-decoration: none; border-radius: 6px; margin-top: 20px; }
        .footer { padding: 20px; text-align: center; font-size: 12px; color: #666; }
        .warning { background: #fef3cd; border: 1px solid #ffc107; padding: 15px; 
                  border-radius: 6px; margin-top: 20px; }
    </style>
</head>
<body>
    <div class="container">
        <div class="header">
            <h1>4cima</h1>
            <p>Bienvenido al equipo</p>
        </div>
        <div class="content">
            <h2>Hola {ArchitectName},</h2>
            <p>Tu cuenta de arquitecto ha sido creada exitosamente en la plataforma 4cima.</p>
            
            <div class="password-box">
                <p style="margin: 0 0 10px 0; font-size: 14px;">Tu contraseña temporal es:</p>
                <p class="password">{TemporaryPassword}</p>
            </div>
            
            <div class="warning">
                <strong>?? Importante:</strong> Deberás cambiar esta contraseña en tu primer inicio de sesión.
            </div>
            
            <p style="text-align: center;">
                <a href="{LoginUrl}" class="btn">Iniciar Sesión</a>
            </p>
        </div>
        <div class="footer">
            <p>Este es un correo automático, por favor no respondas a este mensaje.</p>
            <p>© 2024 4cima - Arquitectura y Diseño</p>
        </div>
    </div>
</body>
</html>
```

### 2. Notificación de Contacto (`ContactRequestNotification`)

Enviado al administrador cuando hay una nueva solicitud de contacto.

**Datos disponibles:**
- `{CustomerName}` - Nombre del cliente
- `{CustomerEmail}` - Email del cliente
- `{CustomerPhone}` - Teléfono (opcional)
- `{Message}` - Mensaje del cliente
- `{PropertyTitle}` - Título de la propiedad (si aplica)
- `{PropertyUrl}` - URL de la propiedad (si aplica)

**Plantilla por defecto:**

```html
<!DOCTYPE html>
<html>
<head>
    <meta charset="utf-8">
    <style>
        body { font-family: 'Segoe UI', Tahoma, sans-serif; line-height: 1.6; color: #333; }
        .container { max-width: 600px; margin: 0 auto; padding: 20px; }
        .header { background: #1d2a4b; color: white; padding: 20px; }
        .content { padding: 20px; background: #fff; border: 1px solid #e5e7eb; }
        .field { margin-bottom: 15px; }
        .field-label { font-weight: bold; color: #666; font-size: 12px; text-transform: uppercase; }
        .field-value { margin-top: 5px; }
        .message-box { background: #f9fafb; padding: 15px; border-radius: 6px; margin-top: 20px; }
        .property-link { color: #1d2a4b; }
    </style>
</head>
<body>
    <div class="container">
        <div class="header">
            <h2 style="margin: 0;">Nueva Solicitud de Contacto</h2>
        </div>
        <div class="content">
            <div class="field">
                <div class="field-label">Nombre</div>
                <div class="field-value">{CustomerName}</div>
            </div>
            <div class="field">
                <div class="field-label">Email</div>
                <div class="field-value"><a href="mailto:{CustomerEmail}">{CustomerEmail}</a></div>
            </div>
            <div class="field">
                <div class="field-label">Teléfono</div>
                <div class="field-value">{CustomerPhone}</div>
            </div>
            {{#if PropertyTitle}}
            <div class="field">
                <div class="field-label">Propiedad de Interés</div>
                <div class="field-value">
                    <a href="{PropertyUrl}" class="property-link">{PropertyTitle}</a>
                </div>
            </div>
            {{/if}}
            <div class="message-box">
                <div class="field-label">Mensaje</div>
                <div class="field-value">{Message}</div>
            </div>
        </div>
    </div>
</body>
</html>
```

### 3. Confirmación de Contacto (`ContactRequestConfirmation`)

Enviado al cliente confirmando que su solicitud fue recibida.

**Datos disponibles:**
- `{CustomerName}` - Nombre del cliente
- `{PropertyTitle}` - Título de la propiedad (si aplica)

**Plantilla por defecto:**

```html
<!DOCTYPE html>
<html>
<head>
    <meta charset="utf-8">
    <style>
        body { font-family: 'Segoe UI', Tahoma, sans-serif; line-height: 1.6; color: #333; }
        .container { max-width: 600px; margin: 0 auto; padding: 20px; }
        .header { background: #1d2a4b; color: white; padding: 30px; text-align: center; }
        .content { padding: 30px; background: #f9fafb; }
        .success-icon { font-size: 48px; text-align: center; margin-bottom: 20px; }
        .footer { padding: 20px; text-align: center; font-size: 12px; color: #666; }
    </style>
</head>
<body>
    <div class="container">
        <div class="header">
            <h1 style="margin: 0;">4cima</h1>
        </div>
        <div class="content">
            <div class="success-icon">?</div>
            <h2 style="text-align: center;">¡Mensaje Recibido!</h2>
            <p>Hola {CustomerName},</p>
            <p>Hemos recibido tu solicitud de información{{#if PropertyTitle}} sobre <strong>{PropertyTitle}</strong>{{/if}}.</p>
            <p>Nuestro equipo se pondrá en contacto contigo a la brevedad posible.</p>
            <p>Gracias por tu interés en 4cima.</p>
        </div>
        <div class="footer">
            <p>© 2024 4cima - Arquitectura y Diseño</p>
        </div>
    </div>
</body>
</html>
```

### 4. Reset de Contraseña (ABP Default)

Esta plantilla es manejada por ABP Account Module. Para personalizarla:

1. Crear archivo `PasswordResetLink.tpl` en:
   ```
   src/cima.Blazor/Pages/Account/EmailTemplates/PasswordResetLink.tpl
   ```

2. Contenido sugerido:

```html
<!DOCTYPE html>
<html>
<head>
    <meta charset="utf-8">
    <style>
        body { font-family: 'Segoe UI', Tahoma, sans-serif; line-height: 1.6; color: #333; }
        .container { max-width: 600px; margin: 0 auto; padding: 20px; }
        .header { background: #1d2a4b; color: white; padding: 30px; text-align: center; }
        .content { padding: 30px; background: #f9fafb; }
        .btn { display: inline-block; background: #1d2a4b; color: white; padding: 12px 30px; 
               text-decoration: none; border-radius: 6px; margin: 20px 0; }
        .footer { padding: 20px; text-align: center; font-size: 12px; color: #666; }
        .warning { color: #dc3545; font-size: 12px; }
    </style>
</head>
<body>
    <div class="container">
        <div class="header">
            <h1 style="margin: 0;">4cima</h1>
            <p>Recuperación de Contraseña</p>
        </div>
        <div class="content">
            <p>Hola,</p>
            <p>Recibimos una solicitud para restablecer la contraseña de tu cuenta.</p>
            <p>Haz clic en el siguiente botón para crear una nueva contraseña:</p>
            
            <p style="text-align: center;">
                <a href="{{model.link}}" class="btn">Restablecer Contraseña</a>
            </p>
            
            <p class="warning">
                Este enlace expirará en 24 horas. Si no solicitaste este cambio, 
                puedes ignorar este correo de forma segura.
            </p>
        </div>
        <div class="footer">
            <p>Si el botón no funciona, copia y pega este enlace en tu navegador:</p>
            <p style="word-break: break-all;">{{model.link}}</p>
            <p>© 2024 4cima - Arquitectura y Diseño</p>
        </div>
    </div>
</body>
</html>
```

## Configuración de Proveedores de Email

### Azure Communication Services

```json
{
  "Email": {
    "Provider": "AzureCommunicationServices",
    "AzureCommunicationServices": {
      "ConnectionString": "endpoint=https://xxx.communication.azure.com/;accesskey=xxx",
      "SenderAddress": "noreply@4cima.com"
    }
  }
}
```

### SMTP

```json
{
  "Email": {
    "Provider": "Smtp",
    "Smtp": {
      "Host": "smtp.gmail.com",
      "Port": 587,
      "UserName": "tu-email@gmail.com",
      "Password": "app-password",
      "FromAddress": "noreply@4cima.com",
      "FromName": "4cima",
      "EnableSsl": true
    }
  }
}
```

## Personalización de Plantillas

### Opción 1: Modificar el servicio

Editar `SmtpEmailNotificationService.cs` o `AzureEmailNotificationService.cs` para cambiar las plantillas inline.

### Opción 2: Sistema de plantillas (futuro)

Implementar un sistema de plantillas basado en archivos:

```csharp
public interface IEmailTemplateService
{
    Task<string> RenderTemplateAsync(string templateName, object model);
}
```

### Opción 3: Virtual File System de ABP

Sobrescribir plantillas usando el sistema de archivos virtuales de ABP.

## Buenas Prácticas

1. **Usar texto plano como fallback**: Incluir versión de texto plano para clientes que no soportan HTML.

2. **Responsive design**: Las plantillas deben verse bien en móviles.

3. **Inline CSS**: Los estilos deben estar inline para compatibilidad con clientes de email.

4. **Evitar imágenes externas**: Muchos clientes bloquean imágenes por defecto.

5. **Incluir enlace de texto**: Siempre incluir el URL como texto además del botón.

6. **Testing**: Probar en múltiples clientes (Gmail, Outlook, Apple Mail).
