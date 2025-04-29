namespace Client;

using System.Net.Http.Json;
using Client.Models;

public class SitterApiClient
{
    private readonly HttpClient httpClient;
    public SitterApiClient(HttpClient httpClient)
    {
        this.httpClient = httpClient;
    }

    public async Task<List<SitterDto>> SearchSittersAsync(
        string? location = null,
        decimal? minPrice = null,
        decimal? maxPrice = null,
        decimal? minRating = null,
        IEnumerable<string>? serviceTypes = null,
        IEnumerable<string>? petTypes = null,
        IEnumerable<string>? badges = null,
        int pageNumber = 1,
        int pageSize = 8,
        CancellationToken cancellationToken = default)
    {
        // Build query string parameters
        var query = new List<string>();
        if (!string.IsNullOrWhiteSpace(location)) query.Add($"location={Uri.EscapeDataString(location)}");
        if (minPrice.HasValue) query.Add($"minPrice={minPrice.Value}");
        if (maxPrice.HasValue) query.Add($"maxPrice={maxPrice.Value}");
        if (minRating.HasValue) query.Add($"minRating={minRating.Value}");
        if (serviceTypes != null)
            foreach (var s in serviceTypes) query.Add($"serviceTypes={Uri.EscapeDataString(s)}");
        if (petTypes != null)
            foreach (var p in petTypes) query.Add($"petTypes={Uri.EscapeDataString(p)}");
        if (badges != null)
            foreach (var b in badges) query.Add($"badges={Uri.EscapeDataString(b)}");
        // pagination
        query.Add($"pageNumber={pageNumber}");
        query.Add($"pageSize={pageSize}");
        var qs = query.Count > 0 ? "?" + string.Join("&", query) : string.Empty;

        var result = await httpClient.GetFromJsonAsync<List<SitterDto>>($"/api/v1/sitters/search{qs}", cancellationToken);
        return result ?? new List<SitterDto>();
    }
}
