using FlowState.Domain.Tasks;
using Microsoft.EntityFrameworkCore;

namespace FlowState.Infrastructure.Persistence;

/// <summary>
/// Seeds a small, realistic set of tasks so a fresh database (or a demo deploy)
/// shows the "What now?" experience immediately. No-op if any tasks already exist.
/// </summary>
public static class DemoDataSeeder
{
    public static async Task SeedAsync(FlowStateDbContext db, CancellationToken ct = default)
    {
        if (await db.Tasks.AnyAsync(ct)) return;

        var demo = new[]
        {
            new TaskItem("Fix the export bug", EnergyCost.High, Importance.High),
            new TaskItem("Gather the W-2s", EnergyCost.Medium, Importance.Normal),
            new TaskItem("Reply to the landlord", EnergyCost.Low, Importance.Normal),
            new TaskItem("Order a disc golf basket", EnergyCost.Low, Importance.Low),
            new TaskItem("Plan the weekend league", EnergyCost.Medium, Importance.Low),
            new TaskItem("Draft standup notes", EnergyCost.Low, Importance.Normal),
            new TaskItem("Outline the portfolio case study", EnergyCost.High, Importance.Normal),
        };

        await db.Tasks.AddRangeAsync(demo, ct);
        await db.SaveChangesAsync(ct);
    }
}
