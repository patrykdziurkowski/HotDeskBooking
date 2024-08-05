using App;
using App.Areas.Identity.Data;
using App.Areas.Locations;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Tests;

[Collection("DockerDatabaseCollection")]
public class LocationRepositoryTests : IAsyncLifetime
{
    private readonly LocationRepository _locationRepository;
    private readonly ApplicationDbContext _context;
    private readonly IServiceScope _scope;

    public LocationRepositoryTests(IntegrationTestApplicationFactory factory)
    {
        _scope = factory.Services.GetRequiredService<IServiceScopeFactory>().CreateScope();
        _context = _scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        _locationRepository = new LocationRepository(_context);
    }

    [Fact]
    public async Task GetAsync_ShouldReturnAllLocations_WhenTwoAreInserted()
    {
        Location location = new(1, 2);
        Location location2 = new(2, 3);
        await _locationRepository.SaveAsync(location);
        await _locationRepository.SaveAsync(location2);

        List<Location> locations = await _locationRepository.GetAsync();

        locations.Should().HaveCount(2);
        locations.FirstOrDefault(l => l.Id == location.Id).Should().NotBeNull();
        locations.FirstOrDefault(l => l.Id == location2.Id).Should().NotBeNull();
    }

    [Fact]
    public async Task GetByIdAsync_ShouldReturnTheDesiredLocation_WhenTwoAreInserted()
    {
        Location location = new(1, 2);
        Location location2 = new(2, 3);
        await _locationRepository.SaveAsync(location);
        await _locationRepository.SaveAsync(location2);

        Location? selectedLocation = await _locationRepository.GetByIdAsync(location2.Id);

        selectedLocation.Should().NotBeNull();
        selectedLocation!.Id.Should().Be(location2.Id);
    }

    [Fact]
    public async Task GetByIdAsync_ShouldReturnNull_WhenSelectingNonExistantLocation()
    {
        Location location = new(1, 2);
        Location location2 = new(2, 3);
        await _locationRepository.SaveAsync(location);
        await _locationRepository.SaveAsync(location2);

        Location? selectedLocation = await _locationRepository.GetByIdAsync(Guid.NewGuid());

        selectedLocation.Should().BeNull();
    }

    [Fact]
    public async Task Save_ShouldConsumeDomainEvents_WhenLocationHasSome()
    {
        Location location = new(1, 2);
        int domainEventCountBefore = location.DomainEvents.Count();

        await _locationRepository.SaveAsync(location);
        int domainEventCountAfter = location.DomainEvents.Count();

        domainEventCountBefore.Should().Be(1);
        domainEventCountAfter.Should().Be(0);
    }

    [Fact]
    public async Task Save_ShouldAddNewLocation_WhenCreated()
    {
        Location location = new(1, 2);

        await _locationRepository.SaveAsync(location);

        List<Location> locations = await _locationRepository.GetAsync();
        locations.Should().HaveCount(1);
        locations.First().Id.Should().NotBe(Guid.Empty);
        locations.First().BuildingNumber.Should().Be(1);
        locations.First().Floor.Should().Be(2);
        locations.First().DeskIds.Should().HaveCount(0);
    }

    [Fact]
    public async Task Save_ShouldRemoveNewLocation_WhenCreatedThenRemoved()
    {
        Location location = new(1, 2);
        location.Remove();

        await _locationRepository.SaveAsync(location);

        (await _locationRepository.GetAsync()).Should().HaveCount(0);
    }

    public async Task InitializeAsync()
    {
        await Task.Run(() => {});
    }

    public async Task DisposeAsync()
    {
        await ClearLocationsTable();
        _scope.Dispose();
    }

    private async Task ClearLocationsTable()
    {
        await _context.Locations.ExecuteDeleteAsync();
    }
}
