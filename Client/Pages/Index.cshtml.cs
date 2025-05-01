using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Client.Pages;

public class IndexModel(FortuneApiClient fortuneApiClient) : PageModel
{
    public string Fortune { get; set; } = string.Empty;

    public async Task OnGet()
    {
        Fortune = await fortuneApiClient.GetRandomFortune();
    }
}
