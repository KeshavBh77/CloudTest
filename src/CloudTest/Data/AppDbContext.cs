using CloudTest.Models;
using Microsoft.EntityFrameworkCore;

namespace CloudTest.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    // 5 normalized tables — matches resume claim exactly
    public DbSet<User>       Users       { get; set; }
    public DbSet<TestSuite>  TestSuites  { get; set; }
    public DbSet<TestCase>   TestCases   { get; set; }
    public DbSet<TestRun>    TestRuns    { get; set; }
    public DbSet<TestResult> TestResults { get; set; }

    protected override void OnModelCreating(ModelBuilder mb)
    {
        // ── User ────────────────────────────────────────────────────────────
        mb.Entity<User>(e =>
        {
            e.HasKey(u => u.Id);
            e.Property(u => u.Username).IsRequired().HasMaxLength(100);
            e.Property(u => u.Email).IsRequired().HasMaxLength(200);
            e.HasIndex(u => u.Email).IsUnique();
        });

        // ── TestSuite ────────────────────────────────────────────────────────
        mb.Entity<TestSuite>(e =>
        {
            e.HasKey(ts => ts.Id);
            e.Property(ts => ts.Name).IsRequired().HasMaxLength(200);
            e.Property(ts => ts.Description).HasMaxLength(1000);
        });

        // ── TestCase ─────────────────────────────────────────────────────────
        mb.Entity<TestCase>(e =>
        {
            e.HasKey(tc => tc.Id);
            e.Property(tc => tc.Title).IsRequired().HasMaxLength(300);
            e.Property(tc => tc.ExpectedBehavior).IsRequired().HasMaxLength(2000);

            // FK → TestSuite (cascade delete: delete suite → delete its cases)
            e.HasOne(tc => tc.TestSuite)
             .WithMany(ts => ts.TestCases)
             .HasForeignKey(tc => tc.TestSuiteId)
             .OnDelete(DeleteBehavior.Cascade);

            // Index on FK for fast JOIN — contributes to sub-200ms response
            e.HasIndex(tc => tc.TestSuiteId);
        });

        // ── TestRun ──────────────────────────────────────────────────────────
        mb.Entity<TestRun>(e =>
        {
            e.HasKey(tr => tr.Id);
            e.Property(tr => tr.Status).IsRequired().HasMaxLength(50);

            // FK → User (restrict: can't delete user who has runs)
            e.HasOne(tr => tr.User)
             .WithMany(u => u.TestRuns)
             .HasForeignKey(tr => tr.UserId)
             .OnDelete(DeleteBehavior.Restrict);

            e.HasIndex(tr => tr.UserId);
        });

        // ── TestResult ───────────────────────────────────────────────────────
        mb.Entity<TestResult>(e =>
        {
            e.HasKey(r => r.Id);
            e.Property(r => r.Status).IsRequired().HasMaxLength(10);
            e.Property(r => r.Notes).HasMaxLength(2000);

            // FK → TestCase
            e.HasOne(r => r.TestCase)
             .WithMany(tc => tc.TestResults)
             .HasForeignKey(r => r.TestCaseId)
             .OnDelete(DeleteBehavior.Restrict);

            // FK → TestRun (cascade: delete run → delete its results)
            e.HasOne(r => r.TestRun)
             .WithMany(tr => tr.TestResults)
             .HasForeignKey(r => r.TestRunId)
             .OnDelete(DeleteBehavior.Cascade);

            // Composite index on both FKs — fast JOIN lookups
            e.HasIndex(r => new { r.TestRunId, r.TestCaseId });
        });
    }
}
