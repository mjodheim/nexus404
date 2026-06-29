using System.Threading;
using System.Threading.Tasks;
using Nexus404.Middleware.Models;

namespace Nexus404.Middleware.Interfaces;

public interface IAiAnalysisService
{
    Task<FallbackResult> AnalyzeMissingPathAsync(AnalysisRequest request, CancellationToken cancellationToken = default);
}
