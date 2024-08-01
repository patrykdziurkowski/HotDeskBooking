using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using App.Areas.Identity.Data;
using Microsoft.AspNetCore.Mvc.Razor;
using App;

var builder = WebApplication.CreateBuilder(args);
var connectionString = builder.Configuration["ConnectionString"] ?? throw new InvalidOperationException("Connection string not found.");

builder.Services
    .AddDbContext<ApplicationDbContext>(options =>
        options.UseSqlServer(connectionString));

builder.Services
    .AddDefaultIdentity<Employee>(options => options.SignIn.RequireConfirmedAccount = true)
    .AddEntityFrameworkStores<ApplicationDbContext>();

builder.Services.AddControllersWithViews();
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

app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");
app.MapRazorPages();
app.Run();
