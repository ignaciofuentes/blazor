using System.Text;
using System.Text.Json;


// Models
public class GraphQLResponse<T>
{
    public T Data { get; set; }
}

public class ServiceListResponse
{
    public List<Service> Items { get; set; }
    public string NextToken { get; set; }
}

public class ListServicesData
{
    public ServiceListResponse ListServices { get; set; }
}

public class Service
{
    public string Id { get; set; }
    public string Title { get; set; }
    public string Description { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}

// AppSync Service

public class AppSyncService
{
    private readonly HttpClient _httpClient;

    private const string ListServicesQuery = @"
        query ListServices {
            listServices {
                items {
                    id
                    title
                    description
                    createdAt
                    updatedAt
                }
            }
        }";

    private const string CreateServiceMutation = @"
        mutation CreateService($input: CreateServiceInput!) {
            createService(input: $input) {
                id
                title
                description
                createdAt
                updatedAt
            }
        }";

    public AppSyncService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<List<Service>> ListServicesAsync()
    {
        var request = new
        {
            query = ListServicesQuery
        };

        var content = new StringContent(JsonSerializer.Serialize(request), Encoding.UTF8, "application/json");
        var response = await _httpClient.PostAsync("", content);
        response.EnsureSuccessStatusCode();

        var json = await response.Content.ReadAsStringAsync();
        var result = JsonSerializer.Deserialize<GraphQLResponse<ListServicesData>>(json, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });

        return result?.Data?.ListServices?.Items ?? new List<Service>();
    }

    public async Task<Service> CreateServiceAsync(string title, string description)
    {
        var request = new
        {
            query = CreateServiceMutation,
            variables = new
            {
                input = new
                {
                    title,
                    description
                }
            }
        };

        var content = new StringContent(JsonSerializer.Serialize(request), Encoding.UTF8, "application/json");
        var response = await _httpClient.PostAsync("", content);
        response.EnsureSuccessStatusCode();

        var json = await response.Content.ReadAsStringAsync();
        var result = JsonSerializer.Deserialize<GraphQLResponse<Dictionary<string, Service>>>(json);

        return result?.Data?["createService"];
    }
}
