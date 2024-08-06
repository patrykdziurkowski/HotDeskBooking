using Microsoft.AspNetCore.Identity;

namespace App;

public class Employee : IdentityUser
{
    public string FirstName { get; }
    public string LastName { get; }

    public Employee(
        string firstName,
        string lastName,
        string userName,
        string email) : base(userName)
    {
        FirstName = firstName;
        LastName = lastName;
        Email = email;
    }

    // EFCore default constructor
    public Employee()
    {
        FirstName = default!;
        LastName = default!;
        UserName = default!;
        Email = default!;
    }
}
