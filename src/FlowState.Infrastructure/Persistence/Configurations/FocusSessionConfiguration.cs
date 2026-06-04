using FlowState.Domain.Focus;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FlowState.Infrastructure.Persistence.Configurations;

public class FocusSessionConfiguration : IEntityTypeConfiguration<FocusSession>
{
    public void Configure(EntityTypeBuilder<FocusSession> builder)
    {
        builder.ToTable("focus_sessions");
        builder.HasKey(s => s.Id);
        builder.Property(s => s.TaskId).IsRequired();
        builder.Property(s => s.TaskTitle).IsRequired().HasMaxLength(500);
        builder.Property(s => s.StartedAt).IsRequired();
        builder.Property(s => s.EndedAt);
        builder.Property(s => s.PlannedDuration).IsRequired();
        builder.Property(s => s.Outcome).HasConversion<int>();

        builder.HasIndex(s => s.TaskId);
        builder.HasIndex(s => s.StartedAt);
    }
}
