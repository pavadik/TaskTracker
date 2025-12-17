using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TaskTracker.Domain.Entities;

namespace TaskTracker.Infrastructure.Persistence.Configurations;

public class TaskHistoryConfiguration : IEntityTypeConfiguration<TaskHistory>
{
    public void Configure(EntityTypeBuilder<TaskHistory> builder)
    {
        builder.ToTable("task_history");

        builder.HasKey(h => h.Id);

        builder.Property(h => h.Id)
            .HasColumnName("id");

        builder.Property(h => h.TaskId)
            .HasColumnName("task_id");

        builder.Property(h => h.FieldName)
            .HasColumnName("field_name")
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(h => h.OldValue)
            .HasColumnName("old_value")
            .HasColumnType("text");

        builder.Property(h => h.NewValue)
            .HasColumnName("new_value")
            .HasColumnType("text");

        builder.Property(h => h.ChangedById)
            .HasColumnName("changed_by_id");

        builder.Property(h => h.ChangedAt)
            .HasColumnName("changed_at");

        builder.HasIndex(h => h.TaskId);
        builder.HasIndex(h => h.ChangedAt);

        builder.HasOne(h => h.ChangedBy)
            .WithMany()
            .HasForeignKey(h => h.ChangedById)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
