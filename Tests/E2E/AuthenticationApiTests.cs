using FluentAssertions;
using Xunit.Priority;

namespace Tests;

[TestCaseOrderer(PriorityOrderer.Name, PriorityOrderer.Assembly)]
[Collection("WebServerTests")]
public class AuthenticationApiTests : IClassFixture<WebServerHostService>
{
    private static readonly HttpClient _client = new();
    private readonly WebServerHostService _hostService;

    public AuthenticationApiTests(
        WebServerHostService hostService)
    {
        _hostService = hostService;
    }

    [Fact, Priority(0)]
    public async Task PostLoginToken_Returns404_WhenCantLogIn()
    {
        string uri = $"http://localhost:8080/Tokens/Login";
        FormUrlEncodedContent form = new([
            new KeyValuePair<string, string>("email", "john@smith.com"),
            new KeyValuePair<string, string>("password", "P@ssword1!")
        ]);

        HttpResponseMessage response = await _client.PostAsync(uri, form);

        ((int) response.StatusCode).Should().Be(404);
    }

    [Fact, Priority(5)]
    public async Task PostRegisterToken_Returns201_WhenRegistered()
    {
        string uri = $"http://localhost:8080/Tokens/Register";
        FormUrlEncodedContent form = new([
            new KeyValuePair<string, string>("firstName", "John"),
            new KeyValuePair<string, string>("lastName", "Smith"),
            new KeyValuePair<string, string>("userName", "JohnSmith"),
            new KeyValuePair<string, string>("email", "john@smith.com"),
            new KeyValuePair<string, string>("password", "P@ssword1!")
        ]);

        HttpResponseMessage response = await _client.PostAsync(uri, form);

        ((int) response.StatusCode).Should().Be(201);
    }

    [Fact, Priority(10)]
    public async Task PostLoginToken_Returns200AndAToken_WhenLoggedIn()
    {
        string uri = $"http://localhost:8080/Tokens/Login";
        FormUrlEncodedContent form = new([
            new KeyValuePair<string, string>("email", "john@smith.com"),
            new KeyValuePair<string, string>("password", "P@ssword1!")
        ]);

        HttpResponseMessage response = await _client.PostAsync(uri, form);

        ((int) response.StatusCode).Should().Be(200);
        string token = await response.Content.ReadAsStringAsync();
        token.Should().NotBeEmpty();
    }
}
