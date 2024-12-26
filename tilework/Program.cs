using Microsoft.EntityFrameworkCore;

using MudBlazor.Services;

using tilework.Components;
using Tilework.ViewModels;

using Tilework.Core.Services;
using Tilework.LoadBalancing.Services;




var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

builder.Services.AddMudServices();


var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
var dbContextOptions = DbContextOptionsHelper.Configure(connectionString);


builder.Services.AddCoreServices();
builder.Services.AddLoadBalancer(builder.Configuration.GetSection("LoadBalancing"), dbContextOptions);

builder.Services.AddViewModels();


var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseStaticFiles();
app.UseAntiforgery();

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();




using(var scope = app.Services.CreateScope())
{
    var serviceProvider = scope.ServiceProvider;

    var dbContextTypes = builder.Services
        .Where(service => service.ServiceType.IsSubclassOf(typeof(DbContext)))
        .Select(service => service.ServiceType);

    foreach (var dbContextType in dbContextTypes)
    {
        var dbContext = (DbContext)serviceProvider.GetRequiredService(dbContextType);

        // Apply migrations
        Console.WriteLine($"Migrating {dbContextType.Name}...");
        dbContext.Database.Migrate();
    }
}




app.Run();
