using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TaskTracker.Domain.Entities;

namespace TaskTracker.Infrastructure.Persistence.Configurations;

public class WorkspaceConfiguration : IEntityTypeConfiguration<Workspace>
{
    public void Configure(EntityTypeBuilder<Workspace> builder)
    {
        builder.ToTable("workspaces");

        builder.HasKey(w => w.Id);

        builder.Property(w => w.Id)
            .HasColumnName("id");

        builder.Property(w => w.Name)
            .HasColumnName("name")
            .HasMaxLength(100)
            .IsRequired();

        builder.OwnsOne(w => w.Slug, slug =>
        {
            slug.Property(s => s.Value)
                .HasColumnName("slug")
                .HasMaxLength(50)
                .IsRequired();

            slug.HasIndex(s => s.Value).IsUnique();
        });

        builder.Property(w => w.Description)
            .HasColumnName("description")
            .HasMaxLength(1000);

        builder.Property(w => w.LogoUrl)
            .HasColumnName("logo_url")
            .HasMaxLength(500);

        builder.Property(w => w.CreatedAt)
            .HasColumnName("created_at");

        builder.Property(w => w.CreatedBy)
            .HasColumnName("created_by");

        builder.Property(w => w.UpdatedAt)
            .HasColumnName("updated_at");

        builder.Property(w => w.UpdatedBy)
            .HasColumnName("updated_by");

        builder.Property(w => w.IsDeleted)
            .HasColumnName("is_deleted")
            .HasDefaultValue(false);

        builder.Property(w => w.DeletedAt)
            .HasColumnName("deleted_at");

        builder.Property(w => w.DeletedBy)
            .HasColumnName("deleted_by");

        builder.Property(w => w.RowVersion)
            .HasColumnName("row_version")
            .IsRowVersion();

        builder.HasQueryFilter(w => !w.IsDeleted);

        builder.HasMany(w => w.Projects)
            .WithOne(p => p.Workspace)
            .HasForeignKey(p => p.WorkspaceId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(w => w.Members)
            .WithOne(m => m.Workspace)
            .HasForeignKey(m => m.WorkspaceId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
