using System.Net.Http.Json;
using App;
using FluentAssertions;
using Xunit.Priority;

namespace Tests;

[TestCaseOrderer(PriorityOrderer.Name, PriorityOrderer.Assembly)]
[Collection("WebServerTests")]
public class ReservationApiTests : IClassFixture<WebServerHostService>, IClassFixture<ReservationApiTestsShared>
{
    private static readonly HttpClient _client = new(new HttpClientHandler() { CookieContainer = new System.Net.CookieContainer() });
    private readonly WebServerHostService _hostService;
    private readonly ReservationApiTestsShared _shared;

    public ReservationApiTests(
        WebServerHostService hostService,
        ReservationApiTestsShared shared)
    {
        _hostService = hostService;
        _shared = shared;
    }

    [Fact, Priority(-3)]
    public async Task GetReservation_ReturnsNotFound_WhenLocationDoesntExist()
    {
        _shared.LocationId = await CreateLocationAsync();
        _shared.DeskId = (await CreateDeskForLocationAsync()).Id;
        string uri = $"http://localhost:8080/Locations/{Guid.NewGuid()}/Desks/{_shared.DeskId}/Reservation";

        HttpResponseMessage response = await _client.GetAsync(uri);

        ((int) response.StatusCode).Should().Be(404);
    }

    [Fact, Priority(-2)]
    public async Task GetReservation_ReturnsNotFound_WhenDeskDoesntExist()
    {
        string uri = $"http://localhost:8080/Locations/{_shared.LocationId}/Desks/{Guid.NewGuid()}/Reservation";

        HttpResponseMessage response = await _client.GetAsync(uri);

        ((int) response.StatusCode).Should().Be(404);
    }

    [Fact, Priority(-1)]
    public async Task GetReservation_ReturnsNotFound_WhenNoReservation()
    {
        string uri = $"http://localhost:8080/Locations/{_shared.LocationId}/Desks/{_shared.DeskId}/Reservation";
        
        HttpResponseMessage response = await _client.GetAsync(uri);

        ((int) response.StatusCode).Should().Be(404);
    }

    [Fact, Priority(5)]
    public async Task PostReservations_ReturnsBadRequest_WhenFormInvalid()
    {
        string uri = $"http://localhost:8080/Locations/{_shared.LocationId}/Desks/{_shared.DeskId}/Reservation";
        FormUrlEncodedContent form = new([
            new KeyValuePair<string, string>("DeskId", _shared.DeskId!.ToString()!),
            new KeyValuePair<string, string>("StartDate", "gibberish"),
            new KeyValuePair<string, string>("EndDate", "2018-06-24")
        ]);
        
        HttpResponseMessage response = await _client.PostAsync(uri, form);

        ((int) response.StatusCode).Should().Be(400);
    }

    [Fact, Priority(10)]
    public async Task PostReservations_ReturnsNotFound_WhenDeskDoesntExist()
    {
        string uri = $"http://localhost:8080/Locations/{_shared.LocationId}/Desks/{_shared.DeskId}/Reservation";
        FormUrlEncodedContent form = new([
            new KeyValuePair<string, string>("DeskId", Guid.NewGuid().ToString()),
            new KeyValuePair<string, string>("StartDate", "2018-06-24"),
            new KeyValuePair<string, string>("EndDate", "2018-06-24")
        ]);
        
        HttpResponseMessage response = await _client.PostAsync(uri, form);

        ((int) response.StatusCode).Should().Be(404);
    }

    [Fact, Priority(15)]
    public async Task PostReservations_ReturnsFrobiddenWhenDeskCantBeBooked()
    {
        await MakeDeskUnavailableAsync();
        string uri = $"http://localhost:8080/Locations/{_shared.LocationId}/Desks/{_shared.DeskId}/Reservation";
        FormUrlEncodedContent form = new([
            new KeyValuePair<string, string>("DeskId", _shared.DeskId!.ToString()!),
            new KeyValuePair<string, string>("StartDate", "2018-06-24"),
            new KeyValuePair<string, string>("EndDate", "2018-06-24")
        ]);
        
        HttpResponseMessage response = await _client.PostAsync(uri, form);

        ((int) response.StatusCode).Should().Be(403);
        await MakeDeskAvailableAsync();
    }


    [Fact, Priority(20)]
    public async Task PostReservations_ReturnsBadRequest_WhenTryingToBookForMoreThan7Days()
    {
        string uri = $"http://localhost:8080/Locations/{_shared.LocationId}/Desks/{_shared.DeskId}/Reservation";
        FormUrlEncodedContent form = new([
            new KeyValuePair<string, string>("DeskId", _shared.DeskId!.ToString()!),
            new KeyValuePair<string, string>("StartDate", "2018-06-23"),
            new KeyValuePair<string, string>("EndDate", "2018-06-30")
        ]);
        
        HttpResponseMessage response = await _client.PostAsync(uri, form);

        ((int) response.StatusCode).Should().Be(400);
        ReservationDto? reservation = await GetReservationAsync();
        reservation.Should().BeNull();
    }

    [Fact, Priority(25)]
    public async Task PostReservations_CreatesReservation_WhenBooked()
    {
        string uri = $"http://localhost:8080/Locations/{_shared.LocationId}/Desks/{_shared.DeskId}/Reservation";
        DateOnly reservationDate = DateOnly.FromDateTime(DateTime.Now.AddDays(6));
        FormUrlEncodedContent form = new([
            new KeyValuePair<string, string>("DeskId", _shared.DeskId!.ToString()!),
            new KeyValuePair<string, string>("StartDate", reservationDate.ToString()),
            new KeyValuePair<string, string>("EndDate", reservationDate.ToString())
        ]);
        
        HttpResponseMessage response = await _client.PostAsync(uri, form);

        ((int) response.StatusCode).Should().Be(200);
        ReservationDto reservation = (await GetReservationAsync())!;
        reservation.StartDate.Should().Be(reservationDate);
        reservation.EndDate.Should().Be(reservationDate);
    }

    [Fact, Priority(30)]
    public async Task GetReservation_ReturnsOk_WhenReservationCreated()
    {
        string uri = $"http://localhost:8080/Locations/{_shared.LocationId}/Desks/{_shared.DeskId}/Reservation";
        
        HttpResponseMessage response = await _client.GetAsync(uri);

        ((int) response.StatusCode).Should().Be(200);
    }



    private async Task<ReservationDto?> GetReservationAsync()
    {
        string uri = $"http://localhost:8080/Locations/{_shared.LocationId}/Desks/{_shared.DeskId}/Reservation";
        HttpResponseMessage getResponse = await _client.GetAsync(uri);
        try
        {
            return await getResponse.Content.ReadFromJsonAsync<ReservationDto>();
        }
        catch(Exception)
        {
            return null;
        }
    }

    private async Task MakeDeskAvailableAsync()
    {
        string uri = $"http://localhost:8080/Locations/{_shared.LocationId}/Desks/{_shared.DeskId}";
        FormUrlEncodedContent form = new([
            new KeyValuePair<string, string>("isMadeUnavailable", "false")
        ]);

        await _client.PatchAsync(uri, form);
    }

    private async Task MakeDeskUnavailableAsync()
    {
        string uri = $"http://localhost:8080/Locations/{_shared.LocationId}/Desks/{_shared.DeskId}";
        FormUrlEncodedContent form = new([
            new KeyValuePair<string, string>("isMadeUnavailable", "true")
        ]);

        await _client.PatchAsync(uri, form);
    }

    private async Task<DeskDto> CreateDeskForLocationAsync()
    {
        string uri = $"http://localhost:8080/Locations/{_shared.LocationId}/Desks";
        FormUrlEncodedContent form = new([]);

        await _client.PostAsync(uri, form);

        return (await GetDesksForLocationAsync()).Single();
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

        return (await GetLocationsAsync()).Single().Id;
    }

    private async Task<List<LocationDto>> GetLocationsAsync()
    {
        string uri = "http://localhost:8080/Locations";
        HttpResponseMessage getResponse = await _client.GetAsync(uri);
        List<LocationDto> locations = await getResponse.Content.ReadFromJsonAsync<List<LocationDto>>()
            ?? throw new Exception("Unable to parse the response");
 
        return locations;
    }
}
