using System.Net.Http.Json;
using App;
using App.Migrations;
using FluentAssertions;
using Xunit.Priority;

namespace Tests;

[TestCaseOrderer(PriorityOrderer.Name, PriorityOrderer.Assembly)]
[Collection("WebServerTests")]
public class DeskApiTests : IClassFixture<WebServerHostService>, IClassFixture<SharedLocationId>
{
    private static readonly HttpClient _client = new(new HttpClientHandler() { CookieContainer = new System.Net.CookieContainer() });
    private readonly WebServerHostService _hostService;
    private SharedLocationId _sharedLocationId;

    public DeskApiTests(
        WebServerHostService hostService,
        SharedLocationId sharedLocationId)
    {
        _hostService = hostService;
        _sharedLocationId = sharedLocationId;
    }

    [Fact, Priority(0)]
    public async Task GetDesk_ReturnsOk()
    {
        _sharedLocationId.LocationId = await CreateLocationAsync();
        string uri = $"http://localhost:8080/Location/{_sharedLocationId.LocationId}/Desk";
        
        HttpResponseMessage response = await _client.GetAsync(uri);

        ((int) response.StatusCode).Should().Be(200);
    }

    [Fact, Priority(5)]
    public async Task GetDesk_ReturnsNotFound_WhenLocationIdDoesntExist()
    {
        Guid nonExistantLocationId = Guid.NewGuid();
        string uri = $"http://localhost:8080/Location/{nonExistantLocationId}/Desk";
        
        HttpResponseMessage response = await _client.GetAsync(uri);

        ((int) response.StatusCode).Should().Be(404);
    }

    [Fact, Priority(10)]
    public async Task PostDesk_CreatesDesk_WhenPosted()
    {
        string uri = $"http://localhost:8080/Location/{_sharedLocationId.LocationId}/Desk";
        FormUrlEncodedContent form = new([]);

        HttpResponseMessage response = await _client.PostAsync(uri, form);

        List<DeskDto> desks = await GetDesksForLocationAsync();
        desks.Single().LocationId.Should().Be(_sharedLocationId.LocationId!.ToString());
        ((int) response.StatusCode).Should().Be(201);
    }





    private async Task<List<DeskDto>> GetDesksForLocationAsync()
    {
        string uri = $"http://localhost:8080/Location/{_sharedLocationId.LocationId}/Desk";
        HttpResponseMessage getResponse = await _client.GetAsync(uri);
        List<DeskDto> desks = await getResponse.Content.ReadFromJsonAsync<List<DeskDto>>()
            ?? throw new Exception("Unable to parse the response into List of Location");
 
        return desks;
    }

    private async Task<Guid> CreateLocationAsync()
    {
        string uri = "http://localhost:8080/Location";
        FormUrlEncodedContent form = new(
        [
            new KeyValuePair<string, string>("BuildingNumber", "5"),
            new KeyValuePair<string, string>("Floor", "2"),
        ]);

        await _client.PostAsync(uri, form);

        return (await GetLocationsAsync()).Single().Id;
    }

    private async Task<List<LocationDto>> GetLocationsAsync()
    {
        string uri = "http://localhost:8080/Location";
        HttpResponseMessage getResponse = await _client.GetAsync(uri);
        List<LocationDto> locations = await getResponse.Content.ReadFromJsonAsync<List<LocationDto>>()
            ?? throw new Exception("Unable to parse the response into List of Location");
 
        return locations;
    }
}
