using HtmlAgilityPack;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Text;
using WebMonitorAgent.Rce2;

namespace WebMonitorAgent;

internal class Program
{
    private static Guid AgentId = Guid.NewGuid();
    private static string Address = $"https://localhost:7113/api/agent/{AgentId}";

    private static async Task Main(string[] args)
    {
        _ = Task.Run(FeedHandler);
        await Task.Delay(5000);
        _ = Task.Run(WebMonitor);
        Console.ReadLine();
    }

    private static async Task FeedHandler()
    {
        using var httpClient = new HttpClient();
        while (true)
        {
            try
            {
                var feed = await httpClient.GetAsync(Address);
                var content = await feed.Content.ReadAsStringAsync();
                var rce2Messages = JsonConvert.DeserializeObject<List<Rce2Message>>(content);
                foreach (var rce2Message in rce2Messages)
                {
                    switch (rce2Message.Contact)
                    {
                        default:
                            if (rce2Message.Type == Rce2Types.WhoIs)
                            {
                                await TryRun(HandleWhoIsMessage);
                            }
                            break;
                    }
                }
            }
            catch (Exception e)
            {
                await Task.Delay(1000);
                // ignore
            }
        }
    }

    private static async Task HandleWhoIsMessage()
    {
        using var httpClient = new HttpClient();
        await httpClient.PostAsync(Address, new StringContent(JsonConvert.SerializeObject(new Rce2Message
        {
            Type = Rce2Types.WhoIs,
            Payload = JObject.FromObject(new Rce2Agent
            {
                Id = AgentId,
                Name = "Web Monitor",
                Ins = new()
                {
                },
                Outs = new()
                {
                    { Rce2Contacts.Outs.RaiseAlert, Rce2Types.String }
                }
            }),
        }), Encoding.UTF8, "application/json"));
    }

    // Example setup for github pull requests
    private static async Task WebMonitor()
    {
        HttpRequestMessage httpRequestMessage;
        HttpResponseMessage httpResponseMessage;
        HtmlDocument htmlDocument;
        string innerHtml;

        string cookies = "";
        List<(string url, bool approved, string conversations, string commits)> prs = new()
        {
            //("https://github.com/(...)/pull/13", false, "0", "1"),
        };
        List<(DateTime time, string message)> alerts = new()
        {
            //(DateTime.Parse("20:00"), ""),
        };
        List<(string url, string count, string message)> pulls = new()
        {
            //("https://github.com/(...)/pulls", "0", "PR change"),
        };

        using var handler = new HttpClientHandler { UseCookies = false };
        using var client = new HttpClient(handler);

        while (true)
        {
            try
            {
                var messages = new List<string>();

                for (var i = prs.Count - 1; i >= 0; i--)
                {
                    httpRequestMessage = new HttpRequestMessage(HttpMethod.Get, prs[i].url);
                    httpRequestMessage.Headers.Add("Cookie", cookies);
                    httpResponseMessage = await client.SendAsync(httpRequestMessage);
                    htmlDocument = new HtmlDocument();
                    htmlDocument.LoadHtml(await httpResponseMessage.Content.ReadAsStringAsync());
                    innerHtml = htmlDocument.DocumentNode.SelectSingleNode("//body").InnerHtml;
                    if (innerHtml.Contains("approved these changes") != prs[i].approved)
                    {
                        messages.Add($"PR approved - {prs[i].url}");
                        prs.Add((prs[i].url, innerHtml.Contains("approved these changes"), prs[i].conversations, prs[i].commits));
                        prs.RemoveAt(i);
                        continue;
                    }
                    var conversationTabCounter = htmlDocument.GetElementbyId("conversation_tab_counter").InnerText;
                    var commitsCount = htmlDocument.GetElementbyId("commits_tab_counter").InnerText;
                    if (conversationTabCounter != prs[i].conversations || commitsCount != prs[i].commits)
                    {
                        messages.Add($"PR modified - {prs[i].url}");
                        prs.Add((prs[i].url, prs[i].approved, conversationTabCounter, commitsCount));
                        prs.RemoveAt(i);
                    }
                }

                for (var i = alerts.Count - 1; i >= 0; i--)
                {
                    if (DateTime.Now > alerts[i].time)
                    {
                        messages.Add($"{alerts[i].message}");
                        alerts.RemoveAt(i);
                    }
                }

                for (var i = pulls.Count - 1; i >= 0; i--)
                {
                    httpRequestMessage = new HttpRequestMessage(HttpMethod.Get, pulls[i].url);
                    httpRequestMessage.Headers.Add("Cookie", cookies);
                    httpResponseMessage = await client.SendAsync(httpRequestMessage);
                    htmlDocument = new HtmlDocument();
                    htmlDocument.LoadHtml(await httpResponseMessage.Content.ReadAsStringAsync());
                    innerHtml = htmlDocument.GetElementbyId("pull-requests-repo-tab-count").InnerHtml;
                    if (innerHtml != pulls[i].count)
                    {
                        messages.Add($"{pulls[i].message} {pulls[i].count} -> {innerHtml}");
                        pulls.Add((pulls[i].url, innerHtml, pulls[i].message));
                        pulls.RemoveAt(i);
                    }
                }

                if (messages.Count > 0)
                {
                    await RaiseAlert(string.Join("\n", messages));
                }
            }
            catch (Exception e)
            {
            }
            await Task.Delay(TimeSpan.FromMinutes(1));
        }
    }

    private static async Task RaiseAlert(string alert)
    {
        try
        {
            using var httpClient = new HttpClient();
            await httpClient.PostAsync(Address, new StringContent(JsonConvert.SerializeObject(new Rce2Message
            {
                Type = Rce2Types.String,
                Contact = Rce2Contacts.Outs.RaiseAlert,
                Payload = JObject.FromObject(new
                {
                    data = alert,
                }),
            }), Encoding.UTF8, "application/json"));
        }
        catch (Exception e)
        {
            // ignore
        }
    }

    private static async Task TryRun(Func<Task> taskFunc)
    {
        try
        {
            await taskFunc();
        }
        catch
        {
            // Ignore
        }
    }
}
