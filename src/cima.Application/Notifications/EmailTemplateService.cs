using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;

namespace cima.Notifications;

/// <summary>
/// Servicio para cargar y procesar plantillas de email
/// </summary>
public interface IEmailTemplateService
{
    Task<string> LoadTemplateAsync(string templateName);
    string ReplaceTokens(string template, Dictionary<string, string> tokens);
}

public class EmailTemplateService : IEmailTemplateService
{
    private readonly string _templatesPath;

    public EmailTemplateService()
    {
        // Las plantillas están embebidas en el assembly
        var assemblyLocation = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
        _templatesPath = Path.Combine(assemblyLocation ?? "", "Notifications", "Templates");
    }

    public async Task<string> LoadTemplateAsync(string templateName)
    {
        // Intentar cargar desde archivos físicos primero (permite personalización)
        var filePath = Path.Combine(_templatesPath, $"{templateName}.html");
        
        if (File.Exists(filePath))
        {
            return await File.ReadAllTextAsync(filePath);
        }

        // Fallback a plantillas embebidas
        var assembly = Assembly.GetExecutingAssembly();
        var resourceName = $"cima.Application.Notifications.Templates.{templateName}.html";
        
        using var stream = assembly.GetManifestResourceStream(resourceName);
        if (stream != null)
        {
            using var reader = new StreamReader(stream);
            return await reader.ReadToEndAsync();
        }

        // Si no existe, retornar plantilla básica
        return GetFallbackTemplate(templateName);
    }

    public string ReplaceTokens(string template, Dictionary<string, string> tokens)
    {
        var result = template;
        
        foreach (var token in tokens)
        {
            // Reemplazar formato {{token}}
            result = result.Replace($"{{{{{token.Key}}}}}", token.Value ?? "");
            
            // Reemplazar formato {token}
            result = result.Replace($"{{{token.Key}}}", token.Value ?? "");
        }

        // Procesar condicionales simples {{#if PropertyTitle}}...{{/if}}
        result = ProcessConditionals(result, tokens);

        return result;
    }

    private string ProcessConditionals(string template, Dictionary<string, string> tokens)
    {
        var result = template;
        
        // Buscar patrones {{#if TokenName}}...{{/if}}
        var ifPattern = @"\{\{#if\s+(\w+)\}\}(.*?)\{\{/if\}\}";
        var matches = System.Text.RegularExpressions.Regex.Matches(result, ifPattern, 
            System.Text.RegularExpressions.RegexOptions.Singleline);

        foreach (System.Text.RegularExpressions.Match match in matches)
        {
            var tokenName = match.Groups[1].Value;
            var content = match.Groups[2].Value;
            
            // Si el token existe y tiene valor, mostrar el contenido
            if (tokens.TryGetValue(tokenName, out var value) && !string.IsNullOrEmpty(value))
            {
                result = result.Replace(match.Value, content);
            }
            else
            {
                result = result.Replace(match.Value, "");
            }
        }

        return result;
    }

    private string GetFallbackTemplate(string templateName)
    {
        return $@"
<!DOCTYPE html>
<html>
<head><meta charset='utf-8'></head>
<body style='font-family: Arial, sans-serif; padding: 20px;'>
    <h1 style='color: #1d2a4b;'>4cima</h1>
    <p>Template '{templateName}' no encontrado.</p>
</body>
</html>";
    }
}
