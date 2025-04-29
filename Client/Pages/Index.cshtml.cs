using Client.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Client;

namespace Client.Pages;
 
public class IndexModel : PageModel
{
    private readonly SitterApiClient sitterApiClient;
    public IndexModel(SitterApiClient sitterApiClient)
    {
        this.sitterApiClient = sitterApiClient;
    }

    // Pagination properties
    [BindProperty(SupportsGet = true)]
    public int PageNumber { get; set; } = 1;
    [BindProperty(SupportsGet = true)]
    public int PageSize { get; set; } = 8;
    [BindProperty(SupportsGet = true)]
    public string? Location { get; set; }
    [BindProperty(SupportsGet = true)]
    public List<string>? SelectedServiceTypes { get; set; }
    [BindProperty(SupportsGet = true)]
    public List<string>? SelectedPetTypes { get; set; }
    [BindProperty(SupportsGet = true)]
    public List<string>? SelectedBadges { get; set; }

    // Dynamic filter options
    public List<string> AllServiceTypes { get; set; } = new List<string>();
    public List<string> AllPetTypes { get; set; } = new List<string>();
    public List<string> AllBadges { get; set; } = new List<string>();

    public List<SitterDto> Sitters { get; set; } = new();

    public async Task OnGetAsync()
    {
        // Load available filters
        AllServiceTypes = await sitterApiClient.GetServiceTypesAsync();
        AllPetTypes = await sitterApiClient.GetPetTypesAsync();
        AllBadges = await sitterApiClient.GetBadgesAsync();

        Sitters = await sitterApiClient.SearchSittersAsync(
            location: Location,
            minPrice: null,
            maxPrice: null,
            minRating: null,
            serviceTypes: SelectedServiceTypes,
            petTypes: SelectedPetTypes,
            badges: SelectedBadges,
            pageNumber: PageNumber,
            pageSize: PageSize);
    }
}
