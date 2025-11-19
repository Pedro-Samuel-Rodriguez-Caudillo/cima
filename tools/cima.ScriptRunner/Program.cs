using System.Diagnostics;

namespace cima.ScriptRunner;

class Program
{
    private static readonly string ScriptsPath = Path.Combine(GetSolutionRoot(), "etc", "scripts");
    private static readonly string BaseUrl = "https://localhost:44350";
    
    static async Task<int> Main(string[] args)
    {
        Console.OutputEncoding = System.Text.Encoding.UTF8;
        Console.Title = "CIMA - Script Runner";
        
        while (true)
        {
            ShowMenu();
            var option = Console.ReadLine();
            
            if (option == "0")
            {
                Console.WriteLine("Saliendo...");
                return 0;
            }
            
            await ExecuteOption(option);
            
            Console.WriteLine("\nPresiona cualquier tecla para continuar...");
            Console.ReadKey();
        }
    }
    
    static void ShowMenu()
    {
        Console.Clear();
        Console.ForegroundColor = ConsoleColor.Cyan;
        Console.WriteLine("╔═══════════════════════════════════════════════════╗");
        Console.WriteLine("║         CIMA - SCRIPT RUNNER                      ║");
        Console.WriteLine("╚═══════════════════════════════════════════════════╝");
        Console.ResetColor();
        Console.WriteLine();
        
        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine("=== BASE DE DATOS ===");
        Console.ResetColor();
        Console.WriteLine("1. Configurar PostgreSQL en Docker");
        Console.WriteLine("2. Resetear base de datos completa");
        Console.WriteLine("3. Actualizar migraciones");
        Console.WriteLine();
        
        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine("=== PRUEBAS Y DIAGNOSTICO ===");
        Console.ResetColor();
        Console.WriteLine("4. Ejecutar pruebas API completas");
        Console.WriteLine("5. Diagnostico rapido API");
        Console.WriteLine("6. Diagnostico detallado");
        Console.WriteLine();
        
        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine("=== LOGS ===");
        Console.ResetColor();
        Console.WriteLine("7. Ver logs (todos)");
        Console.WriteLine("8. Ver logs Blazor");
        Console.WriteLine("9. Ver logs DbMigrator");
        Console.WriteLine("10. Limpiar logs");
        Console.WriteLine();
        
        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine("=== VERIFICACION ===");
        Console.ResetColor();
        Console.WriteLine("11. Verificar permisos en BD");
        Console.WriteLine("12. Verificar cliente Swagger");
        Console.WriteLine();
        
        Console.ForegroundColor = ConsoleColor.Red;
        Console.WriteLine("0. Salir");
        Console.ResetColor();
        Console.WriteLine();
        
        Console.Write("Selecciona una opcion: ");
    }
    
    static async Task ExecuteOption(string? option)
    {
        Console.WriteLine();
        
        switch (option)
        {
            case "1":
                await RunPowerShellScript("setup-postgres-docker.ps1");
                break;
            case "2":
                await RunPowerShellScript("reset-database.ps1");
                break;
            case "3":
                Console.Write("Nombre de la migracion (ej: AgregarCampoX): ");
                var migrationName = Console.ReadLine();
                if (!string.IsNullOrWhiteSpace(migrationName))
                {
                    await RunPowerShellScript("actualizar-migraciones.ps1", 
                        $"-NombreMigracion '{migrationName}'");
                }
                else
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine("Nombre de migracion requerido");
                    Console.ResetColor();
                }
                break;
            case "4":
                await RunPowerShellScript("test-api.ps1");
                break;
            case "5":
                await RunPowerShellScript("diagnostico-api.ps1");
                break;
            case "6":
                await RunPowerShellScript("diagnostico-detallado.ps1");
                break;
            case "7":
                await RunPowerShellScript("ver-logs.ps1");
                break;
            case "8":
                await RunPowerShellScript("ver-logs.ps1", "-Proyecto blazor");
                break;
            case "9":
                await RunPowerShellScript("ver-logs.ps1", "-Proyecto migrator");
                break;
            case "10":
                await RunPowerShellScript("limpiar-logs.ps1");
                break;
            case "11":
                await RunSqlScript("verificar-permisos.sql");
                break;
            case "12":
                await RunSqlScript("verificar-swagger-client.sql");
                break;
            default:
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("Opcion invalida");
                Console.ResetColor();
                break;
        }
    }
    
    static async Task RunPowerShellScript(string scriptName, string args = "")
    {
        var scriptPath = Path.Combine(ScriptsPath, scriptName);
        
        if (!File.Exists(scriptPath))
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($"Script no encontrado: {scriptPath}");
            Console.ResetColor();
            return;
        }
        
        Console.ForegroundColor = ConsoleColor.Green;
        Console.WriteLine($"Ejecutando: {scriptName} {args}");
        Console.ResetColor();
        Console.WriteLine();
        
        var psi = new ProcessStartInfo
        {
            FileName = "powershell.exe",
            Arguments = $"-NoProfile -ExecutionPolicy Bypass -File \"{scriptPath}\" {args}",
            WorkingDirectory = GetSolutionRoot(),
            UseShellExecute = false,
            CreateNoWindow = false
        };
        
        using var process = Process.Start(psi);
        if (process != null)
        {
            await process.WaitForExitAsync();
            
            if (process.ExitCode != 0)
            {
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine($"\nScript finalizado con codigo: {process.ExitCode}");
                Console.ResetColor();
            }
        }
    }
    
    static async Task RunSqlScript(string scriptName)
    {
        var scriptPath = Path.Combine(ScriptsPath, scriptName);
        
        if (!File.Exists(scriptPath))
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($"Script SQL no encontrado: {scriptPath}");
            Console.ResetColor();
            return;
        }
        
        Console.ForegroundColor = ConsoleColor.Green;
        Console.WriteLine($"Ejecutando: {scriptName}");
        Console.ResetColor();
        Console.WriteLine();
        
        var psi = new ProcessStartInfo
        {
            FileName = "powershell.exe",
            Arguments = $"-NoProfile -Command \"Get-Content '{scriptPath}' | docker exec -i cima-postgres psql -U postgres -d cima\"",
            WorkingDirectory = GetSolutionRoot(),
            UseShellExecute = false,
            CreateNoWindow = false
        };
        
        using var process = Process.Start(psi);
        if (process != null)
        {
            await process.WaitForExitAsync();
        }
    }
    
    static string GetSolutionRoot()
    {
        var currentDir = Directory.GetCurrentDirectory();
        
        while (currentDir != null)
        {
            if (Directory.GetFiles(currentDir, "*.sln").Length > 0)
            {
                return currentDir;
            }
            
            currentDir = Directory.GetParent(currentDir)?.FullName;
        }
        
        throw new DirectoryNotFoundException("No se encontro el directorio de la solucion");
    }
}
