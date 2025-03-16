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

    public virtual DbSet<Blacklist> blacklists { get; set; }

    public virtual DbSet<Blog> blogs { get; set; }

    public virtual DbSet<Board> boards { get; set; }

    public virtual DbSet<Card> cards { get; set; }

    public virtual DbSet<CardTask> cardTasks { get; set; }

    public virtual DbSet<LoginAttempt> loginAttempts { get; set; }

    public virtual DbSet<Project> projects { get; set; }

    public virtual DbSet<User> users { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder
            .UseCollation("utf8mb4_general_ci")
            .HasCharSet("utf8mb4");

        modelBuilder.Entity<Blacklist>(entity =>
        {
            entity
                .HasNoKey()
                .ToTable("blacklist");

            entity.Property(e => e.ip)
                .HasMaxLength(15)
                .HasColumnName("ip");
            entity.Property(e => e.reason)
                .HasMaxLength(120)
                .IsFixedLength()
                .HasColumnName("reason");
        });

        modelBuilder.Entity<Blog>(entity =>
        {
            entity.HasKey(e => e.id).HasName("PRIMARY");

            entity.ToTable("blogs");

            entity.HasIndex(e => e.authorId, "blogs_users_id_fk");

            entity.Property(e => e.id)
                .ValueGeneratedNever()
                .HasColumnType("int(11) unsigned")
                .HasColumnName("id");
            entity.Property(e => e.authorId)
                .HasColumnType("bigint(20) unsigned")
                .HasColumnName("author_id");
            entity.Property(e => e.content)
                .HasDefaultValueSql("''")
                .HasColumnType("text")
                .HasColumnName("content");
            entity.Property(e => e.createdAt)
                .HasColumnType("timestamp")
                .HasColumnName("created_at");
            entity.Property(e => e.description)
                .HasMaxLength(180)
                .HasDefaultValueSql("''")
                .HasColumnName("description");
            entity.Property(e => e.isPublic).HasColumnName("is_public");
            entity.Property(e => e.title)
                .HasMaxLength(60)
                .HasDefaultValueSql("'Nouveau blog'")
                .HasColumnName("title");

            entity.HasOne(d => d.author).WithMany(p => p.blogs)
                .HasForeignKey(d => d.authorId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("blogs_users_id_fk");
        });

        modelBuilder.Entity<Board>(entity =>
        {
            entity.HasKey(e => e.id).HasName("PRIMARY");

            entity.ToTable("boards");

            entity.HasIndex(e => new { ProjectId = e.projectId, UniqueName = e.uniqueName }, "boards_pk").IsUnique();

            entity.Property(e => e.id)
                .HasColumnType("bigint(20) unsigned")
                .HasColumnName("id");
            entity.Property(e => e.description)
                .HasMaxLength(300)
                .HasDefaultValueSql("''")
                .HasColumnName("description");
            entity.Property(e => e.name)
                .HasMaxLength(60)
                .HasColumnName("name");
            entity.Property(e => e.projectId)
                .HasColumnType("bigint(20) unsigned")
                .HasColumnName("project_id");
            entity.Property(e => e.uniqueName)
                .HasMaxLength(60)
                .HasColumnName("unique_name");

            entity.HasOne(d => d.project).WithMany(p => p.boards)
                .HasForeignKey(d => d.projectId)
                .HasConstraintName("boards_projects_id_fk");
        });

        modelBuilder.Entity<Card>(entity =>
        {
            entity.HasKey(e => e.id).HasName("PRIMARY");

            entity.ToTable("cards");

            entity.HasIndex(e => e.boardId, "cards_boards_id_fk");

            entity.Property(e => e.id)
                .HasColumnType("bigint(20) unsigned")
                .HasColumnName("id");
            entity.Property(e => e.boardId)
                .HasColumnType("bigint(20) unsigned")
                .HasColumnName("board_id");
            entity.Property(e => e.content)
                .HasDefaultValueSql("''")
                .HasColumnType("text")
                .HasColumnName("content");
            entity.Property(e => e.date)
                .HasDefaultValueSql("current_timestamp()")
                .HasColumnType("timestamp")
                .HasColumnName("date");
            entity.Property(e => e.priority)
                .HasColumnType("tinyint(4)")
                .HasColumnName("priority");
            entity.Property(e => e.state)
                .HasColumnType("tinyint(4)")
                .HasColumnName("state");
            entity.Property(e => e.title)
                .HasMaxLength(255)
                .HasColumnName("title");

            entity.HasOne(d => d.board).WithMany(p => p.cards)
                .HasForeignKey(d => d.boardId)
                .HasConstraintName("cards_boards_id_fk");
        });

        modelBuilder.Entity<CardTask>(entity =>
        {
            entity.HasKey(e => e.id).HasName("PRIMARY");

            entity.ToTable("card_tasks");

            entity.HasIndex(e => e.cardId, "board_tasks_cards_id_fk");

            entity.Property(e => e.id)
                .HasColumnType("bigint(20) unsigned")
                .HasColumnName("id");
            entity.Property(e => e.cardId)
                .HasColumnType("bigint(20) unsigned")
                .HasColumnName("card_id");
            entity.Property(e => e.completed).HasColumnName("completed");
            entity.Property(e => e.content)
                .HasMaxLength(250)
                .HasColumnName("content");

            entity.HasOne(d => d.card).WithMany(p => p.cardTasks)
                .HasForeignKey(d => d.cardId)
                .HasConstraintName("board_tasks_cards_id_fk");
        });

        modelBuilder.Entity<LoginAttempt>(entity =>
        {
            entity
                .HasNoKey()
                .ToTable("login_attempts");

            entity.HasIndex(e => e.date, "login_date").IsDescending();

            entity.Property(e => e.date)
                .HasDefaultValueSql("current_timestamp()")
                .HasColumnType("timestamp")
                .HasColumnName("date");
            entity.Property(e => e.ip)
                .HasMaxLength(15)
                .HasColumnName("ip");
            entity.Property(e => e.success).HasColumnName("success");
            entity.Property(e => e.username)
                .HasMaxLength(18)
                .HasColumnName("username");
        });

        modelBuilder.Entity<Project>(entity =>
        {
            entity.HasKey(e => e.id).HasName("PRIMARY");

            entity.ToTable("projects");

            entity.HasIndex(e => e.uniqueName, "projects_pk").IsUnique();

            entity.Property(e => e.id)
                .HasColumnType("bigint(20) unsigned")
                .HasColumnName("id");
            entity.Property(e => e.description)
                .HasMaxLength(300)
                .HasColumnName("description");
            entity.Property(e => e.name)
                .HasMaxLength(120)
                .HasColumnName("name");
            entity.Property(e => e.uniqueName)
                .HasMaxLength(120)
                .HasColumnName("unique_name");

            entity.HasMany(d => d.users).WithMany(p => p.projects)
                .UsingEntity<Dictionary<string, object>>(
                    "ProjectUser",
                    r => r.HasOne<User>().WithMany()
                        .HasForeignKey("UserId")
                        .OnDelete(DeleteBehavior.ClientSetNull)
                        .HasConstraintName("ProjectUsers_users_id_fk"),
                    l => l.HasOne<Project>().WithMany()
                        .HasForeignKey("ProjectId")
                        .OnDelete(DeleteBehavior.ClientSetNull)
                        .HasConstraintName("ProjectUsers_projects_id_fk"),
                    j =>
                    {
                        j.HasKey("ProjectId", "UserId")
                            .HasName("PRIMARY")
                            .HasAnnotation("MySql:IndexPrefixLength", new[] { 0, 0 });
                        j.ToTable("project__users");
                        j.HasIndex(new[] { "UserId" }, "ProjectUsers_users_id_fk");
                        j.IndexerProperty<ulong>("ProjectId")
                            .HasColumnType("bigint(20) unsigned")
                            .HasColumnName("project_id");
                        j.IndexerProperty<ulong>("UserId")
                            .HasColumnType("bigint(20) unsigned")
                            .HasColumnName("user_id");
                    });
        });

        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.id).HasName("PRIMARY");

            entity.ToTable("users");

            entity.HasIndex(e => e.discordId, "id_discord").IsUnique();

            entity.HasIndex(e => e.mail, "mail").IsUnique();

            entity.HasIndex(e => e.username, "username").IsUnique();

            entity.Property(e => e.id)
                .HasColumnType("bigint(20) unsigned")
                .HasColumnName("id");
            entity.Property(e => e.admin).HasColumnName("admin");
            entity.Property(e => e.avatar)
                .HasMaxLength(150)
                .HasDefaultValueSql("'https://cdn.discordapp.com/avatars/249627389305421824/549b474fb78b0e326d0708135334073f.png'")
                .HasColumnName("avatar");
            entity.Property(e => e.createdAt)
                .HasDefaultValueSql("current_timestamp()")
                .HasColumnType("timestamp")
                .HasColumnName("created_at");
            entity.Property(e => e.discordId)
                .HasColumnType("bigint(20)")
                .HasColumnName("discord_id");
            entity.Property(e => e.mail).HasColumnName("mail");
            entity.Property(e => e.password)
                .HasMaxLength(60)
                .IsFixedLength()
                .HasColumnName("password")
                .UseCollation("armscii8_bin")
                .HasCharSet("armscii8");
            entity.Property(e => e.username)
                .HasMaxLength(18)
                .HasColumnName("username")
                .UseCollation("armscii8_general_ci")
                .HasCharSet("armscii8");
            entity.Property(e => e.version)
                .HasColumnType("int(11)")
                .HasColumnName("version");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
