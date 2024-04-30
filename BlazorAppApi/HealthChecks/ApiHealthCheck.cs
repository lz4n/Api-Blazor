using BlazorAppApi.ApiUtils;
using HealthChecks.UI.Client;
using HealthChecks.UI.Core;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace BlazorAppApi.HealthChecks
{
    public class ApiHealthCheck : IHealthCheck
    {
        public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
        {
            try
            {
                HttpResponseMessage response = await ApiService.GetHealthReport();
                if (!response.IsSuccessStatusCode)
                {
                    return await Task.FromResult(HealthCheckResult.Unhealthy());
                }
            }
            catch (Exception)
            {
                return await Task.FromResult(HealthCheckResult.Degraded());
            }

            return await Task.FromResult(HealthCheckResult.Healthy());
        }
    }
}