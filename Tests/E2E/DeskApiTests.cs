using System.Net.Http.Headers;
using System.Net.Http.Json;
using App;
using App.Migrations;
using FluentAssertions;
using Microsoft.AspNetCore.Http.Connections;
using Xunit.Priority;

namespace Tests;

[TestCaseOrderer(PriorityOrderer.Name, PriorityOrderer.Assembly)]
[Collection("WebServerTests")]
public class DeskApiTests : IClassFixture<WebServerHostService>, IClassFixture<DeskApiTestsShared>
{
    private static readonly HttpClient _client = new();
    private readonly WebServerHostService _hostService;
    private DeskApiTestsShared _shared;

    public DeskApiTests(
        WebServerHostService hostService,
        DeskApiTestsShared sharedLocationId)
    {
        _hostService = hostService;
        _shared = sharedLocationId;
    }

    [Fact, Priority(0)]
    public async Task GetDesk_ReturnsOk()
    {
        await Register();
        await Login();
        _shared.LocationId = await CreateLocationAsync();
        string uri = $"http://localhost:8080/Locations/{_shared.LocationId}/Desks";
        
        HttpResponseMessage response = await _client.GetAsync(uri);

        ((int) response.StatusCode).Should().Be(200);
    }

    [Fact, Priority(5)]
    public async Task GetDesk_ReturnsNotFound_WhenLocationIdDoesntExist()
    {
        Guid nonExistantLocationId = Guid.NewGuid();
        string uri = $"http://localhost:8080/Locations/{nonExistantLocationId}/Desks";
        
        HttpResponseMessage response = await _client.GetAsync(uri);

        ((int) response.StatusCode).Should().Be(404);
    }

    [Fact, Priority(10)]
    public async Task PostDesk_ReturnsNotFound_WhenLocationDoesntExist()
    {
        string uri = $"http://localhost:8080/Locations/{Guid.NewGuid()}/Desks/";
        FormUrlEncodedContent form = new([]);

        HttpResponseMessage response = await _client.PostAsync(uri, form);

        ((int) response.StatusCode).Should().Be(404);
    }


    [Fact, Priority(15)]
    public async Task PostDesk_CreatesDesk_WhenPosted()
    {
        string uri = $"http://localhost:8080/Locations/{_shared.LocationId}/Desks";
        FormUrlEncodedContent form = new([]);

        HttpResponseMessage response = await _client.PostAsync(uri, form);

        List<DeskDto> desks = await GetDesksForLocationAsync();
        desks.Single().LocationId.Should().Be(_shared.LocationId!.ToString());
        _shared.DeskId = desks.Single().Id;
        ((int) response.StatusCode).Should().Be(201);
    }


    [Fact, Priority(20)]
    public async Task PatchDesk_ReturnsNotFound_WhenLocationDoesntExist()
    {
        string uri = $"http://localhost:8080/Locations/{Guid.NewGuid()}/Desks/{_shared.DeskId}";
        FormUrlEncodedContent form = new([
            new KeyValuePair<string, string>("isMadeUnavailable", "true")
        ]);

        HttpResponseMessage response = await _client.PatchAsync(uri, form);

        ((int) response.StatusCode).Should().Be(404);
    }

    [Fact, Priority(20)]
    public async Task PatchDesk_ReturnsNotFound_WhenDeskDoesntExist()
    {
        string uri = $"http://localhost:8080/Locations/{_shared.LocationId}/Desks/{Guid.NewGuid()}";
        FormUrlEncodedContent form = new([
            new KeyValuePair<string, string>("isMadeUnavailable", "true")
        ]);

        HttpResponseMessage response = await _client.PatchAsync(uri, form);

        ((int) response.StatusCode).Should().Be(404);
    }

    [Fact, Priority(25)]
    public async Task PatchDesk_SwitchesIsMadeUnavailableFlag_WhenSetToTrue()
    {
        string uri = $"http://localhost:8080/Locations/{_shared.LocationId}/Desks/{_shared.DeskId}";
        FormUrlEncodedContent form = new([
            new KeyValuePair<string, string>("isMadeUnavailable", "True")
        ]);

        HttpResponseMessage response = await _client.PatchAsync(uri, form);

        ((int) response.StatusCode).Should().Be(200);
        List<DeskDto> desks = await GetDesksForLocationAsync();
        desks.Single().IsMadeUnavailable.Should().BeTrue();
    }
    
    [Fact, Priority(30)]
    public async Task DeleteDesk_ReturnsNotFound_WhenLocationDoesntExist()
    {
        string uri = $"http://localhost:8080/Locations/{Guid.NewGuid()}/Desks/{_shared.DeskId}";

        HttpResponseMessage response = await _client.DeleteAsync(uri);

        ((int) response.StatusCode).Should().Be(404);
    }

    [Fact, Priority(35)]
    public async Task DeleteDesk_ReturnsNotFound_WhenDeskDoesntExist()
    {
        string uri = $"http://localhost:8080/Locations/{_shared.LocationId}/Desks/{Guid.NewGuid()}";

        HttpResponseMessage response = await _client.DeleteAsync(uri);

        ((int) response.StatusCode).Should().Be(404);
    }

    [Fact, Priority(40)]
    public async Task DeleteDesk_RemovesDesk_WhenDeleted()
    {
        string uri = $"http://localhost:8080/Locations/{_shared.LocationId}/Desks/{_shared.DeskId}";

        HttpResponseMessage response = await _client.DeleteAsync(uri);

        ((int) response.StatusCode).Should().Be(200);
        List<DeskDto> desks = await GetDesksForLocationAsync();
        desks.Should().BeEmpty();
        _shared.DeskId = null;
    }

    [Fact, Priority(45)]
    public async Task DeleteDesk_DoesntRemoveDesk_WhenHasReservation()
    {
        _shared.LocationId = await CreateLocationAsync();
        _shared.DeskId = await CreateDeskAsync();
        await CreateReservationForDeskAsync();
        string uri = $"http://localhost:8080/Locations/{_shared.LocationId}/Desks/{_shared.DeskId}";

        HttpResponseMessage response = await _client.DeleteAsync(uri);

        ((int) response.StatusCode).Should().Be(403);
        List<DeskDto> desks = await GetDesksForLocationAsync();
        desks.Should().HaveCount(1);
    }

    [Fact, Priority(50)]
    public async Task GetAvailable_DoesntShowReservedDesk_WhenHasReservation()
    {
        string uri = $"http://localhost:8080/Locations/{_shared.LocationId}/Desks/Available";

        HttpResponseMessage response = await _client.GetAsync(uri);

        ((int) response.StatusCode).Should().Be(200);
        List<DeskDto>? availableDesks = await response.Content.ReadFromJsonAsync<List<DeskDto>>();
        availableDesks.Should().BeEmpty();
    }


    private async Task CreateReservationForDeskAsync()
    {
        string uri = $"http://localhost:8080/Locations/{_shared.LocationId}/Desks/{_shared.DeskId}/Reservation";
        DateOnly reservationDate = DateOnly.FromDateTime(DateTime.Now.AddDays(6));
        FormUrlEncodedContent form = new([
            new KeyValuePair<string, string>("DeskId", _shared.DeskId!.ToString()!),
            new KeyValuePair<string, string>("StartDate", reservationDate.ToString()),
            new KeyValuePair<string, string>("EndDate", reservationDate.ToString())
        ]);
        
        await _client.PostAsync(uri, form);
    }

    private async Task<Guid> CreateDeskAsync()
    {
        string uri = $"http://localhost:8080/Locations/{_shared.LocationId}/Desks";
        FormUrlEncodedContent form = new([]);

        await _client.PostAsync(uri, form);

        List<DeskDto> desks = await GetDesksForLocationAsync();
        return desks.Single().Id;
    }

    private async Task<List<DeskDto>> GetDesksForLocationAsync()
    {
        string uri = $"http://localhost:8080/Locations/{_shared.LocationId}/Desks";
        HttpResponseMessage getResponse = await _client.GetAsync(uri);
        List<DeskDto> desks = await getResponse.Content.ReadFromJsonAsync<List<DeskDto>>()
            ?? throw new Exception("Unable to parse the response");
 
        return desks;
    }

    private async Task<Guid> CreateLocationAsync()
    {
        string uri = "http://localhost:8080/Locations";
        FormUrlEncodedContent form = new(
        [
            new KeyValuePair<string, string>("BuildingNumber", "5"),
            new KeyValuePair<string, string>("Floor", "2"),
        ]);

        await _client.PostAsync(uri, form);

        return (await GetLocationsAsync()).First(l => l.Id != _shared.LocationId).Id;
    }

    private async Task<List<LocationDto>> GetLocationsAsync()
    {
        string uri = "http://localhost:8080/Locations";
        HttpResponseMessage getResponse = await _client.GetAsync(uri);
        List<LocationDto> locations = await getResponse.Content.ReadFromJsonAsync<List<LocationDto>>()
            ?? throw new Exception("Unable to parse the response into");
 
        return locations;
    }

    private async Task Register()
    {
        string uri = $"http://localhost:8080/Tokens/Register";
        FormUrlEncodedContent form = new([
            new KeyValuePair<string, string>("firstName", "John"),
            new KeyValuePair<string, string>("lastName", "Smith"),
            new KeyValuePair<string, string>("userName", "JohnSmith"),
            new KeyValuePair<string, string>("email", "john@smith.com"),
            new KeyValuePair<string, string>("password", "P@ssword1!")
        ]);
        await _client.PostAsync(uri, form);
    }

    private async Task Login()
    {
         string uri = $"http://localhost:8080/Tokens/Login";
        FormUrlEncodedContent form = new([
            new KeyValuePair<string, string>("email", "john@smith.com"),
            new KeyValuePair<string, string>("password", "P@ssword1!")
        ]);
        HttpResponseMessage response = await _client.PostAsync(uri, form);
        string token = await response.Content.ReadAsStringAsync();
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
    }
}
