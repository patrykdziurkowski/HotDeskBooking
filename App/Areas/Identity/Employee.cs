﻿using Microsoft.AspNetCore.Identity;

namespace App;

public class Employee : IdentityUser
{
    public string FirstName { get; }
    public string LastName { get; }

    public Employee(string firstName, string lastName)
    {
        FirstName = firstName;
        LastName = lastName;
    }

    // EFCore default constructor
    public Employee()
    {
        FirstName = default!;
        LastName = default!;
    }
}
