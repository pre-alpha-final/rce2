using System.Net.Http.Headers;
using System.Text;

namespace Broker.Client.Infrastructure;

public class AuthorizedHttpClientFactory
{
    private readonly IHttpClientFactory _httpClientFactory;
    private string _brokerKey;

    public AuthorizedHttpClientFactory(IHttpClientFactory httpClientFactory)
    {
        _httpClientFactory = httpClientFactory;
    }

    public void Authorize(string brokerKey)
    {
        _brokerKey = brokerKey;
    }

    public HttpClient CreateClient(string name)
    {
        var httpClient = _httpClientFactory.CreateClient(name);
        httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", Convert.ToBase64String(Encoding.UTF8.GetBytes(_brokerKey)));

        return httpClient;
    }
}
