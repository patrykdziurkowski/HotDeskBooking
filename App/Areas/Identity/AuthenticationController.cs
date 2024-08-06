using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;

namespace App;

public class AuthenticationController : Controller
{
    private readonly UserManager<Employee> _userManager;
    private readonly IConfiguration _configuration;
    public AuthenticationController(
        UserManager<Employee> userManager,
        IConfiguration configuration)
    {
        _userManager = userManager;
        _configuration = configuration;
    }

    [HttpPost]
    [Route("/Tokens/Register")]
    public async Task<IActionResult> Register(RegisterViewModel model)
    {
        if (ModelState.IsValid == false)
        {
            return BadRequest(ModelState);
        }

        Employee employee = new(
            model.FirstName!,
            model.LastName!,
            model.UserName!,
            model.Email!);

        IdentityResult result = await _userManager.CreateAsync(employee, model.Password!);
        if (result.Succeeded == false)
        {
            return StatusCode(403, result.Errors.First().Description);
        }
        return StatusCode(201);
    }

    [HttpPost]
    [Route("/Tokens/Login")]
    public async Task<IActionResult> Login(LoginViewModel model)
    {
        if (ModelState.IsValid == false)
        {
            return BadRequest(ModelState);
        }

        Employee? employee = await _userManager.FindByEmailAsync(model.Email!);
        if ((employee is null) || (await _userManager.CheckPasswordAsync(employee, model.Password!) == false))
        {
            return NotFound();
        }

        var tokenHandler = new JwtSecurityTokenHandler();
        var key = Encoding.ASCII.GetBytes(_configuration["PrivateKey"]!);
        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(await _userManager.GetClaimsAsync(employee)),
            Expires = DateTime.UtcNow.AddDays(7),
            SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
        };
        var token = tokenHandler.CreateToken(tokenDescriptor);
        var tokenString = tokenHandler.WriteToken(token);

        return Ok(new { Token = tokenString });
    }
}
