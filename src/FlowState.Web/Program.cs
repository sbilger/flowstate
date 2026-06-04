using FlowState.Application;
using FlowState.Infrastructure;
using FlowState.Infrastructure.Persistence;
using FlowState.Web.Components;
using FlowState.Web.Services;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Razor components (Blazor Server interactivity).
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

// FlowState layers.
builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);

// Scoped UI state (current energy level) shared across the circuit.
builder.Services.AddScoped<UiState>();

var app = builder.Build();

// Apply pending migrations on startup (fine for the walking skeleton / single-instance demo).
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<FlowStateDbContext>();
    await db.Database.MigrateAsync();
    await DemoDataSeeder.SeedAsync(db);
}

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseAntiforgery();

app.MapStaticAssets();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();
