using FlowState.Domain.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FlowState.Infrastructure.Persistence.Configurations;

public class TaskItemConfiguration : IEntityTypeConfiguration<TaskItem>
{
    public void Configure(EntityTypeBuilder<TaskItem> builder)
    {
        builder.ToTable("tasks");
        builder.HasKey(t => t.Id);
        builder.Property(t => t.Title).IsRequired().HasMaxLength(500);
        builder.Property(t => t.EnergyCost).HasConversion<int>();
        builder.Property(t => t.Importance).HasConversion<int>();
        builder.Property(t => t.Status).HasConversion<int>();
        builder.Property(t => t.SnoozeCount).IsRequired();
        builder.Property(t => t.LastSnoozedAt);
        builder.Property(t => t.DecayState).HasConversion<int>();
        builder.Property(t => t.LastTouchedAt).IsRequired();
        builder.Property(t => t.DormantSince);
        builder.Property(t => t.CreatedAt).IsRequired();
        builder.Property(t => t.CompletedAt);

        builder.HasIndex(t => t.Status);
        builder.HasIndex(t => t.DecayState);
    }
}
