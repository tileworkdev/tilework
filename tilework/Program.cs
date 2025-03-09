using Microsoft.EntityFrameworkCore;

using MudBlazor.Services;

using tilework.Components;

using Tilework.Core.Services;
using Tilework.LoadBalancing.Services;
using Tilework.CertificateManagement.Services;



var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

builder.Services.AddMudServices();


var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
if(string.IsNullOrEmpty(connectionString))
    throw new InvalidOperationException("Database connection string not defined");
var dbContextOptions = DbContextOptionsHelper.Configure(connectionString);


builder.Services.AddCoreServices();
builder.Services.AddLoadBalancing(builder.Configuration.GetSection("LoadBalancing"), dbContextOptions);
builder.Services.AddCertificateManagement(builder.Configuration.GetSection("CertificateManagement"), dbContextOptions);



var app = builder.Build();

var yoko = app.Environment;
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



app.RunDbMigrations(builder.Services);


app.Run();
