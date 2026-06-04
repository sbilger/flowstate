using FlowState.Application;
using FlowState.Infrastructure;
using FlowState.Infrastructure.Persistence;
using FlowState.Web.Components;
using FlowState.Web.Hubs;
using FlowState.Web.Jobs;
using FlowState.Web.Services;
using Hangfire;
using Hangfire.PostgreSql;
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

// SignalR + server-authoritative focus timer.
builder.Services.AddSignalR();
builder.Services.AddSingleton<FocusTimerRegistry>();
builder.Services.AddHostedService<FocusTimerService>();

// Hangfire — background jobs for the decay/resurfacing lifecycle.
var connectionString = builder.Configuration.GetConnectionString("FlowState")!;
builder.Services.AddHangfire(cfg => cfg
    .SetDataCompatibilityLevel(CompatibilityLevel.Version_180)
    .UseSimpleAssemblyNameTypeSerializer()
    .UseRecommendedSerializerSettings()
    .UsePostgreSqlStorage(opt => opt.UseNpgsqlConnection(connectionString)));
builder.Services.AddHangfireServer();
builder.Services.AddScoped<DecayJob>();

var app = builder.Build();

// Apply pending migrations + seed on startup (fine for single-instance demo).
using (var scope = app.Services.CreateScope())
{
    var factory = scope.ServiceProvider.GetRequiredService<IDbContextFactory<FlowStateDbContext>>();
    await using var db = await factory.CreateDbContextAsync();
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

// Hangfire dashboard (local-only for now; auth gating arrives with Identity in Slice 10).
app.UseHangfireDashboard("/jobs", new DashboardOptions
{
    Authorization = new[] { new FlowState.Web.Jobs.LocalOnlyDashboardAuthorization() }
});

// Register the recurring decay sweep (every 15 minutes).
RecurringJob.AddOrUpdate<DecayJob>(
    "decay-sweep",
    job => job.RunAsync(),
    "*/15 * * * *");

app.MapHub<FocusHub>("/hubs/focus");

app.MapStaticAssets();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();
