using App;
using App.Areas.DesksReservations;
using App.Areas.Identity.Data;
using App.Areas.Locations;
using Ductus.FluentDocker.Common;
using FluentAssertions;
using FluentResults;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Tests;

[Collection("DockerDatabaseCollection")]
public class DeskRepositoryTests : IAsyncLifetime
{
    private readonly DeskRepository _deskRepository;
    private readonly LocationRepository _locationRepository;
    private readonly ApplicationDbContext _context;
    private readonly IServiceScope _scope;

    public DeskRepositoryTests(IntegrationTestApplicationFactory factory)
    {
        _scope = factory.Services.GetRequiredService<IServiceScopeFactory>().CreateScope();
        _context = _scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        _deskRepository = new DeskRepository(_context);
        _locationRepository = new LocationRepository(_context);
    }

    [Fact]
    public async Task GetAsync_ShouldReturnBothDesks_WhenGivenTwoDesks()
    {
        Guid locationId = await InsertLocation();
        Desk desk = new(locationId);
        Desk desk2 = new(locationId);
        await _deskRepository.SaveAsync(desk);
        await _deskRepository.SaveAsync(desk2);

        List<Desk> desks = await _deskRepository.GetAsync();

        desks.Should().HaveCount(2);
        desks[0].Id.Should().NotBe(desks[1].Id);
    }

    [Fact]
    public async Task GetByIdAsync_ShouldReturnGivenDesk_WhenGivenTwoDesks()
    {
        Guid locationId = await InsertLocation();
        Desk desk = new(locationId);
        Desk desk2 = new(locationId);
        await _deskRepository.SaveAsync(desk);
        await _deskRepository.SaveAsync(desk2);

        Desk? selectedDesk = await _deskRepository.GetByIdAsync(desk2.Id);

        selectedDesk.Should().NotBeNull();
        selectedDesk!.Id.Should().Be(desk2.Id);
    }

    [Fact]
    public async Task GetByIdAsync_ShouldReturnNull_WhenSelectingNonExistantDesk()
    {
        Guid locationId = await InsertLocation();
        Desk desk = new(locationId);
        Desk desk2 = new(locationId);
        await _deskRepository.SaveAsync(desk);
        await _deskRepository.SaveAsync(desk2);

        Desk? selectedDesk = await _deskRepository.GetByIdAsync(Guid.NewGuid());

        selectedDesk.Should().BeNull();
    }

    [Fact]
    public async Task GetByLocationAsync_ShouldReturnTwoDesks_WhenGivenTwoDesksWithSameLocationAndOneWithAnother()
    {
        Guid locationId = await InsertLocation();
        Guid locationId2 = await InsertLocation();
        Desk desk = new(locationId);
        Desk desk2 = new(locationId);
        Desk desk3 = new(locationId2);
        await _deskRepository.SaveAsync(desk);
        await _deskRepository.SaveAsync(desk2);
        await _deskRepository.SaveAsync(desk3);

        List<Desk> desks = await _deskRepository.GetByLocationAsync(locationId);

        desks.Should().HaveCount(2);
        desks[0].LocationId.Should().Be(locationId);
        desks[1].LocationId.Should().Be(locationId);
    }

    [Fact]
    public async Task SaveAsync_ShouldAddNewDesk_WhenDeskWasCreated()
    {
        Guid locationId = await InsertLocation();
        Desk desk = new(locationId);

        await _deskRepository.SaveAsync(desk);

        Desk insertedDesk = (await _deskRepository.GetAsync()).Single();
        insertedDesk.Id.Should().Be(desk.Id);
        insertedDesk.LocationId.Should().Be(locationId);
        insertedDesk.DomainEvents.Should().BeEmpty();
    }

    [Fact]
    public async Task SaveAsync_ShouldCreateReservation_WhenReserved()
    {
        Guid employeeId = await InsertEmployee();
        Guid locationId = await InsertLocation();
        Desk desk = new(locationId);
        await _deskRepository.SaveAsync(desk);

        desk.Reserve(
            new DateOnly(2018, 6, 14),
            new DateOnly(2018, 6, 13),
            employeeId
        );
        await _deskRepository.SaveAsync(desk);

        Desk insertedDesk = (await _deskRepository.GetAsync()).Single();
        insertedDesk.Reservation!.StartDate.Should().Be(new DateOnly(2018, 6, 14));
        insertedDesk.Reservation.EmployeeId.Should().Be(employeeId);
    }

    [Fact]
    public async Task SaveAsync_ShouldRemoveDesk_WhenNoReservation()
    {
        Guid locationId = await InsertLocation();
        Desk desk = new(locationId);
        await _deskRepository.SaveAsync(desk);

        desk.Remove(new DateOnly(2018, 6, 13));
        await _deskRepository.SaveAsync(desk);

        (await _deskRepository.GetAsync()).Should().BeEmpty();
    }

    [Fact]
    public async Task SaveAsync_ShouldNotRemoveDesk_WhenHasReservation()
    {
        Guid employeeId = await InsertEmployee();
        Guid locationId = await InsertLocation();
        Desk desk = new(locationId);
        desk.Reserve(
            new DateOnly(2018, 6, 14),
            new DateOnly(2018, 6, 13),
            employeeId
        );
        await _deskRepository.SaveAsync(desk);

        desk.Remove(new DateOnly(2018, 6, 13));
        await _deskRepository.SaveAsync(desk);

        (await _deskRepository.GetAsync()).Should().HaveCount(1);
    }

    [Fact]
    public async Task SaveAsync_PersistDesksUnavailability_WhenMadeUnavailable()
    {
        Guid locationId = await InsertLocation();
        Desk desk = new(locationId);
        await _deskRepository.SaveAsync(desk);

        desk.IsMadeUnavailable = true;
        await _deskRepository.SaveAsync(desk);

        Desk affectedDesk = (await _deskRepository.GetAsync()).Single();
        affectedDesk.IsMadeUnavailable.Should().BeTrue();
    }

    [Fact]
    public async Task SaveAsync_PersistDesksUnavailability_WhenMadeUnavailableThenAvailableAgain()
    {
        Guid locationId = await InsertLocation();
        Desk desk = new(locationId);
        await _deskRepository.SaveAsync(desk);

        desk.IsMadeUnavailable = true;
        await _deskRepository.SaveAsync(desk);

        desk.IsMadeUnavailable = false;
        await _deskRepository.SaveAsync(desk);

        Desk affectedDesk = (await _deskRepository.GetAsync()).Single();
        affectedDesk.IsMadeUnavailable.Should().BeFalse();
    }

    [Fact]
    public async Task SaveAsync_PersistsTwoDeskChanges_WhenTransferredReservation()
    {
        Guid employeeId = await InsertEmployee();
        Guid locationId = await InsertLocation();
        Desk oldDesk = new(locationId);
        Desk newDesk = new(locationId);
        DateOnly currentDate = new(2018, 6, 13);
        DateOnly reservationDate = currentDate.AddDays(5);
        oldDesk.Reserve(reservationDate, currentDate, employeeId);
        ReservationDeskChangeService service = new();

        Result result = service.ChangeDesk(oldDesk, newDesk, currentDate);
        await _deskRepository.SaveAsync([oldDesk, newDesk]);

        result.IsSuccess.Should().BeTrue();
        Desk previousDesk = (await _deskRepository.GetByIdAsync(oldDesk.Id))!;
        Desk latterDesk = (await _deskRepository.GetByIdAsync(newDesk.Id))!;
        previousDesk.Reservation.Should().BeNull();
        previousDesk.ReservationId.Should().BeNull();
        latterDesk.Reservation.Should().NotBeNull();
        latterDesk.Reservation!.StartDate.Should().Be(reservationDate);
    }

    public async Task InitializeAsync()
    {
        await ClearLocationsTableAsync();
    }

    public async Task DisposeAsync()
    {
        await ClearLocationsTableAsync();
        _scope.Dispose();
    }

    private async Task ClearLocationsTableAsync()
    {
        await _context.Reservations.ExecuteDeleteAsync();
        await _context.Desks.ExecuteDeleteAsync();
        await _context.Locations.ExecuteDeleteAsync();
    }

    private async Task<Guid> InsertEmployee()
    {
        Employee employee = new("John", "Smith", "JohnSmith", "john@smith.com");
        await _context.Employees.AddAsync(employee);
        return employee.Id;
    }

    private async Task<Guid> InsertLocation()
    {
        Location location = new(5, 2);
        await _locationRepository.SaveAsync(location);
        return location.Id;
    }
}
