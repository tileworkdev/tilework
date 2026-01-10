using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;

using MudBlazor.Services;

using Tilework.Ui.Api;
using Tilework.Ui.Components;
using Tilework.Ui.ViewModels;

using Tilework.Core;
using Tilework.Core.Services;
using Tilework.Persistence.IdentityManagement.Models;
using Tilework.Core.Persistence;



var builder = WebApplication.CreateBuilder(args);

// Add MudBlazor services
builder.Services.AddMudServices();

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();


builder.Services.AddIdentityCore<User>()
            .AddRoles<Role>()
            .AddEntityFrameworkStores<TileworkContext>()
            .AddDefaultTokenProviders()
            .AddSignInManager();

builder.Services.AddAuthentication(IdentityConstants.ApplicationScheme)
    .AddIdentityCookies();
builder.Services.AddAuthorization();
builder.Services.AddCascadingAuthenticationState();
builder.Services.AddHttpClient();


var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
if(string.IsNullOrEmpty(connectionString))
    throw new InvalidOperationException("Database connection string not defined");
var dbContextOptions = DbContextOptionsHelper.Configure(connectionString);


builder.Services.AddCoreServices(dbContextOptions);
builder.Services.AddMonitoring(builder.Configuration.GetSection("Monitoring"));
builder.Services.AddLoadBalancing(builder.Configuration.GetSection("LoadBalancing"));
builder.Services.AddCertificateManagement(builder.Configuration.GetSection("CertificateManagement"));
builder.Services.AddIdentityManagement(builder.Configuration.GetSection("IdentityManagement"));

builder.Services.AddUserInterface();

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
app.UseAuthentication();
app.UseAuthorization();
app.UseAntiforgery();

app.MapAuthEndpoints();

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();


app.Run();
