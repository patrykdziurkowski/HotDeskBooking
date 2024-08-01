using Microsoft.AspNetCore.Identity;

namespace App;

public class Employee : IdentityUser
{
    public Guid EmployeeId { get; }
    public string FirstName { get; }
    public string LastName { get; }

    public Employee(string firstName, string lastName)
    {
        EmployeeId = Guid.NewGuid();
        FirstName = firstName;
        LastName = lastName;
    }

    // EFCore default constructor
    public Employee()
    {
        EmployeeId = Guid.NewGuid();
        FirstName = default!;
        LastName = default!;
    }
}
