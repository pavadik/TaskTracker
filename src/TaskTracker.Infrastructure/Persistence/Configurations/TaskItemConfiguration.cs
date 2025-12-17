using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TaskTracker.Domain.Entities;

namespace TaskTracker.Infrastructure.Persistence.Configurations;

public class TaskItemConfiguration : IEntityTypeConfiguration<TaskItem>
{
    public void Configure(EntityTypeBuilder<TaskItem> builder)
    {
        builder.ToTable("tasks");

        builder.HasKey(t => t.Id);

        builder.Property(t => t.Id)
            .HasColumnName("id");

        builder.OwnsOne(t => t.FriendlyId, fid =>
        {
            fid.Property(f => f.ProjectPrefix)
                .HasColumnName("friendly_id_prefix")
                .HasMaxLength(10)
                .IsRequired();

            fid.Property(f => f.SequenceNumber)
                .HasColumnName("friendly_id_number")
                .IsRequired();

            fid.Ignore(f => f.Value);
        });

        builder.Property(t => t.Title)
            .HasColumnName("title")
            .HasMaxLength(500)
            .IsRequired();

        builder.Property(t => t.Description)
            .HasColumnName("description")
            .HasColumnType("text");

        builder.Property(t => t.Priority)
            .HasColumnName("priority")
            .HasConversion<int>();

        builder.Property(t => t.Type)
            .HasColumnName("type")
            .HasConversion<int>();

        builder.Property(t => t.StoryPoints)
            .HasColumnName("story_points");

        builder.Property(t => t.DueDate)
            .HasColumnName("due_date");

        builder.Property(t => t.StartedAt)
            .HasColumnName("started_at");

        builder.Property(t => t.CompletedAt)
            .HasColumnName("completed_at");

        builder.Property(t => t.ProjectId)
            .HasColumnName("project_id");

        builder.Property(t => t.StatusId)
            .HasColumnName("status_id");

        builder.Property(t => t.AssigneeId)
            .HasColumnName("assignee_id");

        builder.Property(t => t.ReporterId)
            .HasColumnName("reporter_id");

        builder.Property(t => t.ParentTaskId)
            .HasColumnName("parent_task_id");

        builder.Property(t => t.SprintId)
            .HasColumnName("sprint_id");

        builder.Property(t => t.CustomFields)
            .HasColumnName("custom_fields")
            .HasColumnType("jsonb");

        builder.Property(t => t.CreatedAt)
            .HasColumnName("created_at");

        builder.Property(t => t.CreatedBy)
            .HasColumnName("created_by");

        builder.Property(t => t.UpdatedAt)
            .HasColumnName("updated_at");

        builder.Property(t => t.UpdatedBy)
            .HasColumnName("updated_by");

        builder.Property(t => t.IsDeleted)
            .HasColumnName("is_deleted")
            .HasDefaultValue(false);

        builder.Property(t => t.DeletedAt)
            .HasColumnName("deleted_at");

        builder.Property(t => t.DeletedBy)
            .HasColumnName("deleted_by");

        builder.Property(t => t.RowVersion)
            .HasColumnName("row_version")
            .IsRowVersion();

        builder.HasQueryFilter(t => !t.IsDeleted);

        // Indexes
        builder.HasIndex(t => t.ProjectId);
        builder.HasIndex(t => t.StatusId);
        builder.HasIndex(t => t.AssigneeId);
        builder.HasIndex(t => t.SprintId);
        builder.HasIndex(t => t.ParentTaskId);

        // Relationships
        builder.HasOne(t => t.Status)
            .WithMany()
            .HasForeignKey(t => t.StatusId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(t => t.Assignee)
            .WithMany()
            .HasForeignKey(t => t.AssigneeId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasOne(t => t.Reporter)
            .WithMany()
            .HasForeignKey(t => t.ReporterId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(t => t.ParentTask)
            .WithMany(t => t.Subtasks)
            .HasForeignKey(t => t.ParentTaskId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasOne(t => t.Sprint)
            .WithMany(s => s.Tasks)
            .HasForeignKey(t => t.SprintId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasMany(t => t.Comments)
            .WithOne(c => c.Task)
            .HasForeignKey(c => c.TaskId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(t => t.Attachments)
            .WithOne(a => a.Task)
            .HasForeignKey(a => a.TaskId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(t => t.History)
            .WithOne(h => h.Task)
            .HasForeignKey(h => h.TaskId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(t => t.Labels)
            .WithOne(l => l.Task)
            .HasForeignKey(l => l.TaskId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
