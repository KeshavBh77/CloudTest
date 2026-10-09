using System.ComponentModel.DataAnnotations;

namespace CloudTest.DTOs;

// ── User DTOs ────────────────────────────────────────────────────────────────

public class CreateUserDto
{
    [Required] [MaxLength(100)] public string Username { get; set; } = string.Empty;
    [Required] [EmailAddress]   public string Email    { get; set; } = string.Empty;
}

public class UserDto
{
    public int    Id        { get; set; }
    public string Username  { get; set; } = string.Empty;
    public string Email     { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}

// ── TestSuite DTOs ───────────────────────────────────────────────────────────

public class CreateTestSuiteDto
{
    [Required] [MaxLength(200)] public string Name        { get; set; } = string.Empty;
               [MaxLength(1000)] public string Description { get; set; } = string.Empty;
}

public class UpdateTestSuiteDto
{
    [Required] [MaxLength(200)]  public string Name        { get; set; } = string.Empty;
               [MaxLength(1000)] public string Description { get; set; } = string.Empty;
}

public class TestSuiteDto
{
    public int      Id          { get; set; }
    public string   Name        { get; set; } = string.Empty;
    public string   Description { get; set; } = string.Empty;
    public DateTime CreatedAt   { get; set; }
    public int      TestCaseCount { get; set; }
}

public class TestSuiteDetailDto : TestSuiteDto
{
    public List<TestCaseDto> TestCases { get; set; } = new();
}

// ── TestCase DTOs ────────────────────────────────────────────────────────────

public class CreateTestCaseDto
{
    [Required] [MaxLength(300)]  public string Title            { get; set; } = string.Empty;
    [Required] [MaxLength(2000)] public string ExpectedBehavior { get; set; } = string.Empty;
}

public class UpdateTestCaseDto
{
    [Required] [MaxLength(300)]  public string Title            { get; set; } = string.Empty;
    [Required] [MaxLength(2000)] public string ExpectedBehavior { get; set; } = string.Empty;
}

public class TestCaseDto
{
    public int      Id               { get; set; }
    public string   Title            { get; set; } = string.Empty;
    public string   ExpectedBehavior { get; set; } = string.Empty;
    public int      TestSuiteId      { get; set; }
    public DateTime CreatedAt        { get; set; }
}

// ── TestRun DTOs ─────────────────────────────────────────────────────────────

public class CreateTestRunDto
{
    [Required] public int UserId { get; set; }
}

public class TestRunDto
{
    public int      Id          { get; set; }
    public string   Status      { get; set; } = string.Empty;
    public int      UserId      { get; set; }
    public string   Username    { get; set; } = string.Empty;
    public DateTime ExecutedAt  { get; set; }
    public int      ResultCount { get; set; }
}

public class TestRunDetailDto : TestRunDto
{
    public List<TestResultDto> TestResults { get; set; } = new();
}

// ── TestResult DTOs ──────────────────────────────────────────────────────────

public class CreateTestResultDto
{
    [Required]
    [RegularExpression("Pass|Fail|Skip", ErrorMessage = "Status must be Pass, Fail, or Skip")]
    public string Status { get; set; } = string.Empty;

    [MaxLength(2000)] public string? Notes { get; set; }

    [Required] public int TestCaseId { get; set; }
    [Required] public int TestRunId  { get; set; }

    public long ResponseTimeMs { get; set; }
}

public class TestResultDto
{
    public int      Id             { get; set; }
    public string   Status         { get; set; } = string.Empty;
    public string?  Notes          { get; set; }
    public long     ResponseTimeMs { get; set; }
    public int      TestCaseId     { get; set; }
    public string   TestCaseTitle  { get; set; } = string.Empty;
    public int      TestRunId      { get; set; }
    public DateTime RecordedAt     { get; set; }
}
