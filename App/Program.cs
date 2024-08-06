using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using App.Areas.Identity.Data;
using Microsoft.AspNetCore.Mvc.Razor;
using App;
using App.Areas.DesksReservations;
using Microsoft.AspNetCore.Mvc.Formatters;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Microsoft.AspNetCore.Authentication.Cookies;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddScoped<ILocationRepository, LocationRepository>();
builder.Services.AddScoped<IDeskRepository, DeskRepository>();
builder.Services.AddSingleton<IConfiguration>(builder.Configuration);

var connectionString = builder.Configuration["ConnectionString"]
    ?? throw new InvalidOperationException("Connection string not found.");
var key = Encoding.UTF8.GetBytes(builder.Configuration["PrivateKey"]
    ?? throw new InvalidOperationException("Couldn't find the Private key used for Jwt token generation."));

builder.Services
    .AddDbContext<ApplicationDbContext>(options =>
        options.UseSqlServer(connectionString));
builder.Services
    .AddDefaultIdentity<Employee>()
    .AddRoles<IdentityRole>()
    .AddEntityFrameworkStores<ApplicationDbContext>();

builder.Services.AddAuthentication(o =>
{
    o.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    o.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(o =>
{
    o.RequireHttpsMetadata = false;
    o.SaveToken = true;
    o.TokenValidationParameters = new()
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = "HotDeskBooking",
        ValidAudience = "HotDeskBooking",
        IssuerSigningKey = new SymmetricSecurityKey(key)
    };
});

builder.Services.AddControllersWithViews(o => o.OutputFormatters.RemoveType<HttpNoContentOutputFormatter>());
builder.Services.Configure<RazorViewEngineOptions>(o => {
    /*
        {2} - area name
        {1} - controller name
        {0} - action name
    */
    o.ViewLocationFormats.Add("/Areas/{2}/{0}.cshtml");
    o.ViewLocationFormats.Add("/Areas/{2}/{1}/{0}.cshtml");
    o.ViewLocationFormats.Add("/Areas/Shared/{0}.cshtml");
});


var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}
else
{
     using IServiceScope scope = app.Services
        .GetRequiredService<IServiceScopeFactory>().CreateScope();
    ApplicationDbContext context = scope.ServiceProvider.GetService<ApplicationDbContext>()
        ?? throw new ApplicationException("ApplicationDbContext was not found in the dependency injection container.");
    context.Database.Migrate();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");
app.MapRazorPages();


using (IServiceScope scope = app.Services.CreateScope())
{
    RoleManager<IdentityRole> roleManager = scope.ServiceProvider
        .GetRequiredService<RoleManager<IdentityRole>>();
    string[] roles = ["Administrator", "Employee"];
    foreach (string role in roles)
    {
        if (await roleManager.RoleExistsAsync(role) == false)
        {
            await roleManager.CreateAsync(new IdentityRole(role));
        }
    }
}


app.Run();

public partial class Program { }
