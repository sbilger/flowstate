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
        builder.Property(t => t.Status).HasConversion<int>();
        builder.Property(t => t.CreatedAt).IsRequired();
    }
}
