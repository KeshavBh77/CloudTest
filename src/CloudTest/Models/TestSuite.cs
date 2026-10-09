namespace CloudTest.Models;

public class TestSuite
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Navigation
    public ICollection<TestCase> TestCases { get; set; } = new List<TestCase>();
}
