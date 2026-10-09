namespace CloudTest.Models;

public class TestCase
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string ExpectedBehavior { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // FK
    public int TestSuiteId { get; set; }
    public TestSuite TestSuite { get; set; } = null!;

    // Navigation
    public ICollection<TestResult> TestResults { get; set; } = new List<TestResult>();
}
