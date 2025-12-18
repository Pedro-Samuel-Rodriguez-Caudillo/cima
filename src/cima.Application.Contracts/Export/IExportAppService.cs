using System.Threading;
using System.Threading.Tasks;
using Volo.Abp.Application.Services;

namespace cima.Export;

/// <summary>
/// Service for exporting data to various formats (Excel, CSV, etc.)
/// </summary>
public interface IExportAppService : IApplicationService
{
    /// <summary>
    /// Exports all properties to Excel format with accumulated values
    /// </summary>
    Task<byte[]> ExportPropertiesToExcelAsync(CancellationToken cancellationToken = default);
}
