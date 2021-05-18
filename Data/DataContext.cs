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
        public virtual DbSet<Course> Courses { get; set; }
        public virtual DbSet<Chapter> Chapters { get; set; }
        public virtual DbSet<Group> Groups { get; set; }
        public virtual DbSet<Laplace> LaplaceValues { get; set; }

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

                entity.Property(e => e.Variant1).HasColumnName("variant1");
                
                entity.Property(e => e.Variant2).HasColumnName("variant2");
                
                entity.Property(e => e.Variant3).HasColumnName("variant3");

                entity.Property(e => e.Difficulty).HasColumnName("difficulty");

                entity.Property(p => p.ChapterId)
                    .HasColumnName("chapter_id"); 

                entity.Property(e => e.Text)
                    .IsRequired()
                    .HasColumnType("character varying")
                    .HasColumnName("text");

                entity.HasOne(d => d.Chapter)
                    .WithMany(p => p.Questions)
                    .HasForeignKey(p => p.ChapterId)
                    .HasConstraintName("questions_chapter_id_fkey");
            });

            modelBuilder.Entity<Student>(entity =>
            {
                entity.ToTable("students");

                entity.Property(e => e.Id).HasColumnName("id");

                entity.Property(e => e.FirstName)
                    .IsRequired()
                    .HasMaxLength(50)
                    .HasColumnName("first_name");

                entity.Property(p => p.GroupId)
                    .IsRequired()
                    .HasColumnName("group_id");

                entity.Property(e => e.LastName)
                    .IsRequired()
                    .HasMaxLength(50)
                    .HasColumnName("last_name");

                entity.HasOne(p => p.Group)
                    .WithMany(p => p.Students)
                    .HasForeignKey(p => p.GroupId)
                    .HasConstraintName("student_group_id_fkey");
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

            modelBuilder.Entity<Course>(entity =>
            {
                entity.ToTable("courses");

                entity.Property(p => p.Id).HasColumnName("id");

                entity.Property(p => p.FullName)
                    .IsRequired()
                    .HasMaxLength(100)
                    .HasColumnName("full_name");

                entity.Property(p => p.ShortName)
                    .HasMaxLength(20)
                    .HasColumnName("short_name");

                entity.Property(p => p.ChaptersCount).HasColumnName("chapters_count");
            });

            modelBuilder.Entity<Chapter>(entity =>
            {
                entity.ToTable("chapters");

                entity.Property(p => p.Id).HasColumnName("id");

                entity.Property(p => p.Name)
                    .IsRequired()
                    .HasMaxLength(150)
                    .HasColumnName("name");

                entity.Property(p => p.CourseId)
                    .HasColumnName("course_id");

                entity.HasOne(d => d.Course)
                    .WithMany(p => p.Chapters)
                    .HasForeignKey(d => d.CourseId)
                    .HasConstraintName("chapters_course_id_fkey");
            });

            modelBuilder.Entity<Group>(entity =>
            {
                entity.ToTable("groups");

                entity.Property(p => p.Id)
                    .HasColumnName("id");

                entity.Property(p => p.Number)
                    .IsRequired()
                    .HasColumnName("number");
            });

            modelBuilder.Entity<Laplace>(entity =>
            {
                entity.ToTable("laplace");
                entity.Property(l => l.Id).HasColumnName("id");
                entity.Property(l => l.X).HasColumnName("x");
                entity.Property(l => l.Value).HasColumnName("value");
            });

            OnModelCreatingPartial(modelBuilder);
        }

        partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
    }
}
