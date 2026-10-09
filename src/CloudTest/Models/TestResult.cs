namespace CloudTest.Models;

public class TestResult
{
    public int Id { get; set; }
    public string Status { get; set; } = "Pass"; // Pass | Fail | Skip
    public string? Notes { get; set; }
    public long ResponseTimeMs { get; set; }
    public DateTime RecordedAt { get; set; } = DateTime.UtcNow;

    // FK → TestCase
    public int TestCaseId { get; set; }
    public TestCase TestCase { get; set; } = null!;

    // FK → TestRun
    public int TestRunId { get; set; }
    public TestRun TestRun { get; set; } = null!;
}
