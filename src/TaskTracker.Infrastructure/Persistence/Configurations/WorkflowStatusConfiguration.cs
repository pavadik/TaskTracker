using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TaskTracker.Domain.Entities;

namespace TaskTracker.Infrastructure.Persistence.Configurations;

public class WorkflowStatusConfiguration : IEntityTypeConfiguration<WorkflowStatus>
{
    public void Configure(EntityTypeBuilder<WorkflowStatus> builder)
    {
        builder.ToTable("workflow_statuses");

        builder.HasKey(s => s.Id);

        builder.Property(s => s.Id)
            .HasColumnName("id");

        builder.Property(s => s.Name)
            .HasColumnName("name")
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(s => s.Description)
            .HasColumnName("description")
            .HasMaxLength(500);

        builder.Property(s => s.Color)
            .HasColumnName("color")
            .HasMaxLength(20)
            .HasDefaultValue("#808080");

        builder.Property(s => s.Category)
            .HasColumnName("category")
            .HasConversion<int>();

        builder.Property(s => s.Order)
            .HasColumnName("order");

        builder.Property(s => s.IsDefault)
            .HasColumnName("is_default")
            .HasDefaultValue(false);

        builder.Property(s => s.ProjectId)
            .HasColumnName("project_id");

        builder.Property(s => s.CreatedAt)
            .HasColumnName("created_at");

        builder.Property(s => s.CreatedBy)
            .HasColumnName("created_by");

        builder.Property(s => s.UpdatedAt)
            .HasColumnName("updated_at");

        builder.Property(s => s.UpdatedBy)
            .HasColumnName("updated_by");

        builder.Property(s => s.IsDeleted)
            .HasColumnName("is_deleted")
            .HasDefaultValue(false);

        builder.Property(s => s.DeletedAt)
            .HasColumnName("deleted_at");

        builder.Property(s => s.DeletedBy)
            .HasColumnName("deleted_by");

        builder.Property(s => s.RowVersion)
            .HasColumnName("row_version")
            .IsRowVersion();

        builder.HasQueryFilter(s => !s.IsDeleted);

        builder.HasIndex(s => new { s.ProjectId, s.Order });

        builder.HasMany(s => s.OutgoingTransitions)
            .WithOne(t => t.FromStatus)
            .HasForeignKey(t => t.FromStatusId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(s => s.IncomingTransitions)
            .WithOne(t => t.ToStatus)
            .HasForeignKey(t => t.ToStatusId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
