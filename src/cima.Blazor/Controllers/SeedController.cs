using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using Volo.Abp;
using Volo.Abp.Data;
using Volo.Abp.Identity;

namespace cima.Blazor.Controllers;

[Route("api/[controller]")]
[ApiController]
public class SeedController : ControllerBase
{
    private readonly IDataSeeder _dataSeeder;
    
    public SeedController(IDataSeeder dataSeeder)
    {
        _dataSeeder = dataSeeder;
    }
    
    [HttpPost("execute")]
    public async Task<IActionResult> Execute([FromQuery] string secretKey)
    {
        // Simple authentication - change this key!
        if (secretKey != "CHANGE_ME_IN_PRODUCTION_12345")
        {
            return Unauthorized();
        }
        
        try
        {
            await _dataSeeder.SeedAsync(new DataSeedContext()
                .WithProperty(IdentityDataSeedContributor.AdminEmailPropertyName, "admin@abp.io")
                .WithProperty(IdentityDataSeedContributor.AdminPasswordPropertyName, "1q2w3E*"));
            
            return Ok(new { message = "Seeding completed successfully" });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { error = ex.Message });
        }
    }
}
