using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Npgsql;
using TestingSystem.Models;

#nullable disable

namespace TestingSystem.Data
{
    public partial class DataContext : DbContext
    {
        public DataContext()
        {
            Database.EnsureCreated();
        }

        public DataContext(DbContextOptions<DataContext> options)
            : base(options)
        {
        }

        static DataContext() => NpgsqlConnection.GlobalTypeMapper.MapEnum<ques_type>();

        public virtual DbSet<Question> Questions { get; set; }
        public virtual DbSet<Student> Students { get; set; }
        public virtual DbSet<StudentAnswer> StudentAnswers { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
                optionsBuilder.UseNpgsql("Host=localhost;Port=5432;Database=RankingSystem;Username=postgres;Password=sql");
            }
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.HasPostgresEnum(null, "ques_type", new[] { "POL", "UPR", "CHL" })
                .HasAnnotation("Relational:Collation", "English_United States.1251");

            modelBuilder.Entity<Question>(entity =>
            {
                entity.ToTable("questions");

                entity.Property(e => e.Id).HasColumnName("id");

                entity.Property(e => e.Answer)
                    .IsRequired()
                    .HasMaxLength(100)
                    .HasColumnName("answer");

                entity.Property(e => e.Difficulty).HasColumnName("difficulty");

                entity.Property(e => e.Text)
                    .IsRequired()
                    .HasColumnType("character varying")
                    .HasColumnName("text");
            });

            modelBuilder.Entity<Student>(entity =>
            {
                entity.ToTable("students");

                entity.Property(e => e.Id).HasColumnName("id");

                entity.Property(e => e.FirstName)
                    .IsRequired()
                    .HasMaxLength(50)
                    .HasColumnName("first_name");

                entity.Property(e => e.LastName)
                    .IsRequired()
                    .HasMaxLength(50)
                    .HasColumnName("last_name");
            });

            modelBuilder.Entity<StudentAnswer>(entity =>
            {
                entity.ToTable("student_answers");

                entity.Property(e => e.Id).HasColumnName("id");

                entity.Property(e => e.Date).HasColumnName("date");

                entity.Property(e => e.Point).HasColumnName("point");

                entity.Property(e => e.QuestionId).HasColumnName("question_id");

                entity.Property(e => e.StudentId).HasColumnName("student_id");
                
                entity.Property(e => e.Answer).HasMaxLength(100).HasColumnName("answer");

                entity.HasOne(d => d.Question)
                    .WithMany(p => p.StudentAnswers)
                    .HasForeignKey(d => d.QuestionId)
                    .HasConstraintName("student_answers_question_id_fkey");

                entity.HasOne(d => d.Student)
                    .WithMany(p => p.StudentAnswers)
                    .HasForeignKey(d => d.StudentId)
                    .HasConstraintName("student_answers_student_id_fkey");
            });

            OnModelCreatingPartial(modelBuilder);
        }

        partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
    }
}
