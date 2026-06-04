using FlowState.Domain.Tasks;

namespace FlowState.Domain.Scoring;

/// <summary>A task paired with its computed score, for ranking and display.</summary>
public record ScoredTask(TaskItem Task, double Score);

/// <summary>
/// Decides what to surface in "What now?". Filters open tasks by the energy gate,
/// ranks by PriorityScore, and offers a weighted-random "pick for me".
/// Pure domain logic — no DB, no framework.
/// </summary>
public static class WhatNowSelector
{
    /// <summary>
    /// Returns open, within-energy tasks ranked best-first. Over-budget tasks are excluded
    /// entirely (not just down-weighted) so the user is never taunted with work they can't do.
    /// </summary>
    public static IReadOnlyList<ScoredTask> Rank(
        IEnumerable<TaskItem> tasks, EnergyLevel currentEnergy, DateTimeOffset now, ScoringWeights? weights = null)
    {
        return tasks
            .Where(t => t.Status == TaskItemStatus.Open)
            .Where(t => TaskScorer.PassesEnergyGate(t.EnergyCost, currentEnergy))
            .Select(t => new ScoredTask(t, TaskScorer.Score(t, currentEnergy, now, weights)))
            .OrderByDescending(s => s.Score)
            .ThenByDescending(s => s.Task.CreatedAt)
            .ToList();
    }

    /// <summary>The single best task to surface, or null if nothing fits the current energy.</summary>
    public static ScoredTask? Top(
        IEnumerable<TaskItem> tasks, EnergyLevel currentEnergy, DateTimeOffset now, ScoringWeights? weights = null)
        => Rank(tasks, currentEnergy, now, weights).FirstOrDefault();

    /// <summary>
    /// Dopamine-friendly "pick for me": a weighted-random choice among eligible tasks,
    /// weighted by score so good picks are likely but there's delightful variance.
    /// </summary>
    public static ScoredTask? PickForMe(
        IEnumerable<TaskItem> tasks, EnergyLevel currentEnergy, DateTimeOffset now,
        Random random, ScoringWeights? weights = null)
    {
        var ranked = Rank(tasks, currentEnergy, now, weights);
        if (ranked.Count == 0) return null;
        if (ranked.Count == 1) return ranked[0];

        // Floor each weight so even low-scoring tasks have a small chance.
        const double floor = 0.05;
        double total = ranked.Sum(s => s.Score + floor);
        double roll = random.NextDouble() * total;

        double cumulative = 0;
        foreach (var scored in ranked)
        {
            cumulative += scored.Score + floor;
            if (roll <= cumulative) return scored;
        }
        return ranked[^1];
    }
}
