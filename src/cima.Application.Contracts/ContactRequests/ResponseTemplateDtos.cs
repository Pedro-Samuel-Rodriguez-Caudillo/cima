using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace cima.ContactRequests;

/// <summary>
/// Template de respuesta predefinida para solicitudes de contacto
/// </summary>
public class ResponseTemplateDto
{
    public Guid Id { get; set; }
    
    /// <summary>
    /// Nombre del template (ej: "Agradecimiento", "Más información")
    /// </summary>
    public string Name { get; set; } = "";
    
    /// <summary>
    /// Contenido del template con placeholders
    /// Placeholders disponibles: {CustomerName}, {PropertyTitle}, {AgentName}
    /// </summary>
    public string Content { get; set; } = "";
    
    /// <summary>
    /// Categoría del template
    /// </summary>
    public ResponseTemplateCategory Category { get; set; }
    
    /// <summary>
    /// Orden de visualización
    /// </summary>
    public int DisplayOrder { get; set; }
    
    /// <summary>
    /// Indica si es un template del sistema (no editable)
    /// </summary>
    public bool IsSystem { get; set; }
}

/// <summary>
/// DTO para crear/editar templates
/// </summary>
public class CreateUpdateResponseTemplateDto
{
    [Required]
    [StringLength(100)]
    public string Name { get; set; } = "";
    
    [Required]
    [StringLength(2000)]
    public string Content { get; set; } = "";
    
    public ResponseTemplateCategory Category { get; set; }
    
    public int DisplayOrder { get; set; }
}

/// <summary>
/// Categorías de templates de respuesta
/// </summary>
public enum ResponseTemplateCategory
{
    /// <summary>
    /// Respuesta inicial/agradecimiento
    /// </summary>
    Initial = 0,
    
    /// <summary>
    /// Solicitud de más información
    /// </summary>
    MoreInfo = 1,
    
    /// <summary>
    /// Programación de cita
    /// </summary>
    Appointment = 2,
    
    /// <summary>
    /// Seguimiento
    /// </summary>
    FollowUp = 3,
    
    /// <summary>
    /// Cierre/No disponible
    /// </summary>
    Closing = 4
}

/// <summary>
/// Templates predefinidos del sistema
/// </summary>
public static class SystemResponseTemplates
{
    public static List<ResponseTemplateDto> GetDefaults() => new()
    {
        new ResponseTemplateDto
        {
            Id = Guid.Parse("00000000-0000-0000-0000-000000000001"),
            Name = "Agradecimiento inicial",
            Content = @"Estimado/a {CustomerName},

Gracias por su interés en la propiedad ""{PropertyTitle}"".

Hemos recibido su mensaje y nos pondremos en contacto con usted a la brevedad para proporcionarle toda la información que necesita.

Saludos cordiales,
{AgentName}
4cima Inmobiliaria",
            Category = ResponseTemplateCategory.Initial,
            DisplayOrder = 1,
            IsSystem = true
        },
        new ResponseTemplateDto
        {
            Id = Guid.Parse("00000000-0000-0000-0000-000000000002"),
            Name = "Solicitar más información",
            Content = @"Estimado/a {CustomerName},

Gracias por contactarnos sobre ""{PropertyTitle}"".

Para poder atenderle mejor, ¿podría proporcionarnos la siguiente información?

- Rango de fechas disponibles para una visita
- Horario de preferencia
- ¿Tiene algún requisito especial?

Quedamos atentos a su respuesta.

Saludos,
{AgentName}",
            Category = ResponseTemplateCategory.MoreInfo,
            DisplayOrder = 2,
            IsSystem = true
        },
        new ResponseTemplateDto
        {
            Id = Guid.Parse("00000000-0000-0000-0000-000000000003"),
            Name = "Confirmar cita",
            Content = @"Estimado/a {CustomerName},

Confirmamos su cita para visitar ""{PropertyTitle}"".

Fecha: [COMPLETAR]
Hora: [COMPLETAR]
Dirección: [COMPLETAR]

Le esperamos. Si necesita reprogramar, por favor avísenos con anticipación.

Saludos,
{AgentName}",
            Category = ResponseTemplateCategory.Appointment,
            DisplayOrder = 3,
            IsSystem = true
        },
        new ResponseTemplateDto
        {
            Id = Guid.Parse("00000000-0000-0000-0000-000000000004"),
            Name = "Seguimiento",
            Content = @"Estimado/a {CustomerName},

Esperamos que se encuentre bien. Le escribimos para dar seguimiento a su consulta sobre ""{PropertyTitle}"".

¿Tiene alguna pregunta adicional? ¿Le gustaría programar una visita?

Estamos a su disposición.

Saludos,
{AgentName}",
            Category = ResponseTemplateCategory.FollowUp,
            DisplayOrder = 4,
            IsSystem = true
        },
        new ResponseTemplateDto
        {
            Id = Guid.Parse("00000000-0000-0000-0000-000000000005"),
            Name = "Propiedad no disponible",
            Content = @"Estimado/a {CustomerName},

Lamentamos informarle que la propiedad ""{PropertyTitle}"" ya no se encuentra disponible.

Sin embargo, contamos con otras opciones que podrían interesarle. ¿Le gustaría que le enviemos alternativas similares?

Saludos,
{AgentName}",
            Category = ResponseTemplateCategory.Closing,
            DisplayOrder = 5,
            IsSystem = true
        }
    };
}
