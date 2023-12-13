using System.Net.Http.Headers;

namespace Broker.Client.Infrastructure;

public class AuthorizedHttpClientFactory
{
    private readonly IHttpClientFactory _httpClientFactory;
    private string _code;

    public AuthorizedHttpClientFactory(IHttpClientFactory httpClientFactory)
    {
        _httpClientFactory = httpClientFactory;
    }

    public void Authorize(string code)
    {
        _code = code;
    }

    public HttpClient CreateClient(string name)
    {
        var httpClient = _httpClientFactory.CreateClient(name);
        httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Code", _code);

        return httpClient;
    }
}
