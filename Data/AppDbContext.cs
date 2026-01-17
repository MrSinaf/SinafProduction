using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using SinafProduction.Data.Entities;

namespace SinafProduction.Data;

public partial class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Blog> Blogs { get; set; }

    public virtual DbSet<Project> Projects { get; set; }

    public virtual DbSet<ProjectRepository> ProjectRepositories { get; set; }

    public virtual DbSet<ProjectStar> ProjectStars { get; set; }

    public virtual DbSet<ProjectTag> ProjectTags { get; set; }

    public virtual DbSet<User> Users { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder
            .UseCollation("utf8mb4_general_ci")
            .HasCharSet("utf8mb4");

        modelBuilder.Entity<Blog>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.ToTable("blogs");

            entity.HasIndex(e => e.AuthorId, "blogs_users_id_fk");

            entity.Property(e => e.Id)
                .ValueGeneratedNever()
                .HasColumnType("bigint(20) unsigned")
                .HasColumnName("id");
            entity.Property(e => e.AuthorId)
                .HasColumnType("bigint(20) unsigned")
                .HasColumnName("author_id");
            entity.Property(e => e.Content)
                .HasColumnType("text")
                .HasColumnName("content");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("curtime()")
                .HasColumnType("datetime")
                .HasColumnName("created_at");
            entity.Property(e => e.Description)
                .HasMaxLength(256)
                .HasColumnName("description");
            entity.Property(e => e.IsPublished).HasColumnName("is_published");
            entity.Property(e => e.PublishAt)
                .HasDefaultValueSql("curdate()")
                .HasColumnName("publish_at");
            entity.Property(e => e.Title)
                .HasMaxLength(128)
                .HasColumnName("title")
                .UseCollation("utf8mb3_general_ci")
                .HasCharSet("utf8mb3");

            entity.HasOne(d => d.Author).WithMany(p => p.Blogs)
                .HasForeignKey(d => d.AuthorId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("blogs_users_id_fk");
        });

        modelBuilder.Entity<Project>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.ToTable("projects");

            entity.HasIndex(e => e.UniqueName, "projects_name").IsUnique();

            entity.HasIndex(e => e.TagId, "projects_tags_id_fk");

            entity.Property(e => e.Id)
                .HasColumnType("bigint(20) unsigned")
                .HasColumnName("id");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("current_timestamp()")
                .HasColumnName("created_at");
            entity.Property(e => e.Description)
                .HasMaxLength(128)
                .HasDefaultValueSql("''")
                .HasColumnName("description")
                .UseCollation("utf8mb3_general_ci")
                .HasCharSet("utf8mb3");
            entity.Property(e => e.Markdown)
                .HasColumnType("text")
                .HasColumnName("markdown");
            entity.Property(e => e.Name)
                .HasMaxLength(64)
                .HasColumnName("name")
                .UseCollation("utf8mb3_general_ci")
                .HasCharSet("utf8mb3");
            entity.Property(e => e.TagId)
                .HasColumnType("tinyint(3) unsigned")
                .HasColumnName("tag_id");
            entity.Property(e => e.UniqueName)
                .HasMaxLength(64)
                .HasColumnName("unique_name")
                .UseCollation("utf8mb3_general_ci")
                .HasCharSet("utf8mb3");

            entity.HasOne(d => d.Tag).WithMany(p => p.Projects)
                .HasForeignKey(d => d.TagId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("projects_tags_id_fk");
        });

        modelBuilder.Entity<ProjectRepository>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.ToTable("project-repository");

            entity.HasIndex(e => e.ProjectId, "project-repository_projects_id_fk");

            entity.Property(e => e.Id)
                .HasColumnType("bigint(20) unsigned")
                .HasColumnName("id");
            entity.Property(e => e.Added)
                .HasColumnType("int(10) unsigned")
                .HasColumnName("added");
            entity.Property(e => e.Branch)
                .HasMaxLength(50)
                .HasColumnName("branch")
                .UseCollation("utf8mb3_general_ci")
                .HasCharSet("utf8mb3");
            entity.Property(e => e.Commit)
                .HasMaxLength(64)
                .HasColumnName("commit")
                .UseCollation("utf8mb3_general_ci")
                .HasCharSet("utf8mb3");
            entity.Property(e => e.Modified)
                .HasColumnType("int(10) unsigned")
                .HasColumnName("modified");
            entity.Property(e => e.ProjectId)
                .HasColumnType("bigint(20) unsigned")
                .HasColumnName("project_id");
            entity.Property(e => e.Removed)
                .HasColumnType("int(10) unsigned")
                .HasColumnName("removed");
            entity.Property(e => e.Repository)
                .HasMaxLength(100)
                .HasColumnName("repository")
                .UseCollation("utf8mb3_general_ci")
                .HasCharSet("utf8mb3");
            entity.Property(e => e.Update)
                .HasDefaultValueSql("curtime()")
                .HasColumnType("datetime")
                .HasColumnName("update");

            entity.HasOne(d => d.Project).WithMany(p => p.ProjectRepositories)
                .HasForeignKey(d => d.ProjectId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("project-repository_projects_id_fk");
        });

        modelBuilder.Entity<ProjectStar>(entity =>
        {
            entity.HasKey(e => e.ProjectId).HasName("PRIMARY");

            entity.ToTable("project-stars");

            entity.Property(e => e.ProjectId)
                .ValueGeneratedNever()
                .HasColumnType("bigint(20) unsigned")
                .HasColumnName("project_id");

            entity.HasOne(d => d.Project).WithOne(p => p.ProjectStar)
                .HasForeignKey<ProjectStar>(d => d.ProjectId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("project-stars_projects_id_fk");
        });

        modelBuilder.Entity<ProjectTag>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.ToTable("project__tags");

            entity.Property(e => e.Id)
                .ValueGeneratedOnAdd()
                .HasColumnType("tinyint(3) unsigned")
                .HasColumnName("id");
            entity.Property(e => e.Name)
                .HasMaxLength(18)
                .HasColumnName("name")
                .UseCollation("utf8mb3_general_ci")
                .HasCharSet("utf8mb3");
        });

        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.ToTable("users");

            entity.HasIndex(e => e.UniqueName, "user_name_unique").IsUnique();

            entity.Property(e => e.Id)
                .HasColumnType("bigint(20) unsigned")
                .HasColumnName("id");
            entity.Property(e => e.Admin).HasColumnName("admin");
            entity.Property(e => e.Name)
                .HasMaxLength(32)
                .HasColumnName("name")
                .UseCollation("utf8mb3_general_ci")
                .HasCharSet("utf8mb3");
            entity.Property(e => e.Password)
                .HasMaxLength(256)
                .HasDefaultValueSql("''")
                .HasColumnName("password")
                .UseCollation("utf8mb3_general_ci")
                .HasCharSet("utf8mb3");
            entity.Property(e => e.UniqueName)
                .HasMaxLength(32)
                .HasColumnName("unique_name")
                .UseCollation("utf8mb3_general_ci")
                .HasCharSet("utf8mb3");
            entity.Property(e => e.Version)
                .HasColumnType("int(10) unsigned")
                .HasColumnName("version");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
