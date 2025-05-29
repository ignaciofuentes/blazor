using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using System.Text.Json;
using blazor;




var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

// Temporary HttpClient to fetch amplify_outputs.json
using var tempClient = new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) };

// Load and parse amplify_outputs.json
var json = await tempClient.GetStringAsync("amplify_outputs.json");
using var doc = JsonDocument.Parse(json);

var root = doc.RootElement.GetProperty("data");
var apiKey = root.GetProperty("api_key").GetString();
var url = root.GetProperty("url").GetString();

// Now use the parsed values to configure the real HttpClient
builder.Services.AddScoped(sp => new HttpClient(new ApiKeyHandler(apiKey)
{
    InnerHandler = new HttpClientHandler()
})
{
    BaseAddress = new Uri(url)
});

builder.Services.AddScoped<AppSyncService>();


await builder.Build().RunAsync();

public class ApiKeyHandler : DelegatingHandler
{
    private readonly string _apiKey;

    public ApiKeyHandler(string apiKey)
    {
        _apiKey = apiKey;
    }

    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        request.Headers.Add("x-api-key", _apiKey);
        return await base.SendAsync(request, cancellationToken);
    }
}
