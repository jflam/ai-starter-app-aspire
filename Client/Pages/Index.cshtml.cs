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
     [BindProperty(SupportsGet = true)]
     public int PageNumber { get; set; } = 1;
     [BindProperty(SupportsGet = true)]
     public int PageSize { get; set; } = 8;
     [BindProperty(SupportsGet = true)]
     public string? Location { get; set; }
     [BindProperty(SupportsGet = true)]
     public List<string>? ServiceTypes { get; set; }
     [BindProperty(SupportsGet = true)]
     public List<string>? PetTypes { get; set; }
     [BindProperty(SupportsGet = true)]
     public List<string>? Badges { get; set; }

     public List<SitterDto> Sitters { get; set; } = new();

     public async Task OnGetAsync()
     {
        Sitters = await sitterApiClient.SearchSittersAsync(
            location: Location,
            minPrice: null,
            maxPrice: null,
            minRating: null,
            serviceTypes: ServiceTypes,
            petTypes: PetTypes,
            badges: Badges,
            pageNumber: PageNumber,
            pageSize: PageSize);
     }
 }
