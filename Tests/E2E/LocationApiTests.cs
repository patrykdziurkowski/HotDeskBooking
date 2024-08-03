using System.Net.Http.Json;
using App;
using App.Areas.Locations;
using FluentAssertions;
using Microsoft.Identity.Client;
using Newtonsoft.Json;
using Xunit.Priority;

namespace Tests;

[TestCaseOrderer(PriorityOrderer.Name, PriorityOrderer.Assembly)]
[Collection("WebServerTests")]
public class LocationApiTests : IClassFixture<WebServerHostService>
{
    private static readonly HttpClient _client = new(new HttpClientHandler() { CookieContainer = new System.Net.CookieContainer() });
    private readonly WebServerHostService _hostService;

    public LocationApiTests(WebServerHostService hostService)
    {
        _hostService = hostService;
    }

    [Fact, Priority(0)]
    public async Task GetLocation_ReturnsOk()
    {
        string uri = "http://localhost:8080/Location";
        
        HttpResponseMessage response = await _client.GetAsync(uri);

        ((int) response.StatusCode).Should().Be(200);
    }

    [Fact, Priority(5)]
    public async Task PostLocation_ReturnsBadRequest_WhenFormInvalid()
    {
        string uri = "http://localhost:8080/Location";
        FormUrlEncodedContent form = new(
        [
            new KeyValuePair<string, string>("BuildingNumber", "-5"),
            new KeyValuePair<string, string>("Floor", "NotANumber!"),
        ]);

        HttpResponseMessage response = await _client.PostAsync(uri, form);

        ((int) response.StatusCode).Should().Be(400);
    }

    [Fact, Priority(10)]
    public async Task PostLocation_CreatesLocation_WhenFormValid()
    {
        string uri = "http://localhost:8080/Location";
        FormUrlEncodedContent form = new(
        [
            new KeyValuePair<string, string>("BuildingNumber", "5"),
            new KeyValuePair<string, string>("Floor", "2"),
        ]);

        HttpResponseMessage response = await _client.PostAsync(uri, form);

        List<LocationDto> locations = await GetLocationsAsync();
        locations.Should().NotBeEmpty();
        locations.Should().HaveCount(1);
        locations.Single().Id.Should().NotBe(Guid.Empty);
        locations.Single().BuildingNumber.Should().Be(5);
        locations.Single().Floor.Should().Be(2);
        locations.Single().Desks.Should().BeEmpty();
        ((int) response.StatusCode).Should().Be(201);
    }

    [Fact, Priority(15)]
    public async Task DeleteLocation_ReturnsNotFound_WhenIdNotFound()
    {
        Guid nonExistantLocationId = Guid.Empty;
        string uri = $"http://localhost:8080/Location/{nonExistantLocationId}";

        HttpResponseMessage response = await _client.DeleteAsync(uri);

        ((int) response.StatusCode).Should().Be(404);
    }

    [Fact, Priority(20)]
    public async Task DeleteLocation_RemovesLocation()
    {
        List<LocationDto> locations = await GetLocationsAsync();
        Guid locationId = locations.Single().Id;
        string uri = $"http://localhost:8080/Location/{locationId}";

        HttpResponseMessage response = await _client.DeleteAsync(uri);

        ((int) response.StatusCode).Should().Be(200);
        List<LocationDto> locationsAfterDeletion = await GetLocationsAsync();
        locationsAfterDeletion.Should().HaveCount(0);
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
