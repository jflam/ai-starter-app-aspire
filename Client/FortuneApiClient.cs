namespace Client;

public class FortuneApiClient(HttpClient httpClient)
{
    public async Task<string> GetRandomFortune(CancellationToken cancellationToken = default)
        => await httpClient.GetStringAsync("/");
}
