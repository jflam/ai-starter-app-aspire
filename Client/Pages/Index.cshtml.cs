using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Client.Pages;

public class IndexModel(ILogger<IndexModel> logger, FortuneApiClient fortuneApiClient) : PageModel
{
    public string Fortune { get; set; }

    public async Task OnGet()
    {
        Fortune = await fortuneApiClient.GetRandomFortune();
    }
}
