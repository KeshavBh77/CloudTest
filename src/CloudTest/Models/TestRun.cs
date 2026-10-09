namespace CloudTest.Models;

public class TestRun
{
    public int Id { get; set; }
    public DateTime ExecutedAt { get; set; } = DateTime.UtcNow;
    public string Status { get; set; } = "Pending"; // Pending | Running | Completed

    // FK
    public int UserId { get; set; }
    public User User { get; set; } = null!;

    // Navigation
    public ICollection<TestResult> TestResults { get; set; } = new List<TestResult>();
}
