using CoursePLatform.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace CoursePLatform.Data
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<Course> Courses { get; set; } = null!;
        public DbSet<Lesson> Lessons { get; set; } = null!;
        public DbSet<Enrollment> Enrollments { get; set; } = null!;
        public DbSet<LessonProgress> LessonProgresses { get; set; } = null!;
        public DbSet<InstructorApplication> InstructorApplications { get; set; } = null!;

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            // Configure Course entity
            builder.Entity<Course>(entity =>
            {
                entity.HasKey(e => e.Id);

                entity.Property(e => e.Price)
                    .HasPrecision(10, 2); // 10 digits total, 2 after decimal

                entity.HasOne(e => e.Instructor)
                    .WithMany(u => u.CreatedCourses)
                    .HasForeignKey(e => e.InstructorId)
                    .OnDelete(DeleteBehavior.Restrict); // Prevent cascade delete
            });

            // Configure Lesson entity
            builder.Entity<Lesson>(entity =>
            {
                entity.HasKey(e => e.Id);

                entity.HasOne(e => e.Course)
                    .WithMany(c => c.Lessons)
                    .HasForeignKey(e => e.CourseId)
                    .OnDelete(DeleteBehavior.Cascade); // When course is deleted, delete lessons

                entity.HasIndex(e => new { e.CourseId, e.OrderIndex })
                    .IsUnique(); // Ensure unique ordering within a course
            });

            // Configure Enrollment entity
            builder.Entity<Enrollment>(entity =>
            {
                entity.HasKey(e => e.Id);

                entity.HasOne(e => e.User)
                    .WithMany(u => u.Enrollments)
                    .HasForeignKey(e => e.UserId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(e => e.Course)
                    .WithMany(c => c.Enrollments)
                    .HasForeignKey(e => e.CourseId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasIndex(e => new { e.UserId, e.CourseId })
                    .IsUnique(); // Prevent duplicate enrollments

                entity.Property(e => e.ProgressPercentage)
                    .HasPrecision(5, 2) // Allow up to 100.00%
                    .HasDefaultValue(0);

                entity.Property(e => e.EnrolledAt)
                    .HasDefaultValueSql("GETUTCDATE()");
            });

            // Configure LessonProgress entity
            builder.Entity<LessonProgress>(entity =>
            {
                entity.HasKey(e => e.Id);

                entity.HasOne(e => e.User)
                    .WithMany(u => u.LessonProgresses)
                    .HasForeignKey(e => e.UserId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(e => e.Lesson)
                    .WithMany(l => l.LessonProgresses)
                    .HasForeignKey(e => e.LessonId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasIndex(e => new { e.UserId, e.LessonId })
                    .IsUnique(); // One progress record per user per lesson
            });

            // Configure InstructorApplication entity
            builder.Entity<InstructorApplication>(entity =>
            {
                entity.HasKey(e => e.Id);

                entity.HasOne(e => e.User)
                    .WithOne()
                    .HasForeignKey<InstructorApplication>(e => e.UserId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasIndex(e => e.UserId)
                    .IsUnique(); // One application per user
            });
        }
    }
}