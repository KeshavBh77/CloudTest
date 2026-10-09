using CloudTest.DTOs;
using CloudTest.Models;
using CloudTest.Repositories;

namespace CloudTest.Services;

// ── UserService ───────────────────────────────────────────────────────────────
public class UserService : IUserService
{
    private readonly IUserRepository _repo;
    public UserService(IUserRepository repo) => _repo = repo;

    public async Task<List<UserDto>> GetAllAsync()
        => (await _repo.GetAllAsync()).Select(MapToDto).ToList();

    public async Task<UserDto?> GetByIdAsync(int id)
    {
        var user = await _repo.GetByIdAsync(id);
        return user == null ? null : MapToDto(user);
    }

    public async Task<UserDto> CreateAsync(CreateUserDto dto)
    {
        if (await _repo.EmailExistsAsync(dto.Email))
            throw new InvalidOperationException($"Email '{dto.Email}' is already registered.");

        var user = new User { Username = dto.Username, Email = dto.Email };
        var created = await _repo.AddAsync(user);
        return MapToDto(created);
    }

    public async Task<bool> DeleteAsync(int id) => await _repo.DeleteAsync(id);

    private static UserDto MapToDto(User u) => new()
    {
        Id        = u.Id,
        Username  = u.Username,
        Email     = u.Email,
        CreatedAt = u.CreatedAt,
    };
}

// ── TestSuiteService ──────────────────────────────────────────────────────────
public class TestSuiteService : ITestSuiteService
{
    private readonly ITestSuiteRepository _repo;
    public TestSuiteService(ITestSuiteRepository repo) => _repo = repo;

    public async Task<List<TestSuiteDto>> GetAllAsync()
        => (await _repo.GetAllAsync()).Select(MapToDto).ToList();

    public async Task<TestSuiteDto?> GetByIdAsync(int id)
    {
        var suite = await _repo.GetByIdAsync(id);
        return suite == null ? null : MapToDto(suite);
    }

    public async Task<TestSuiteDetailDto?> GetDetailAsync(int id)
    {
        var suite = await _repo.GetByIdWithCasesAsync(id);
        if (suite == null) return null;

        return new TestSuiteDetailDto
        {
            Id            = suite.Id,
            Name          = suite.Name,
            Description   = suite.Description,
            CreatedAt     = suite.CreatedAt,
            TestCaseCount = suite.TestCases.Count,
            TestCases     = suite.TestCases.Select(tc => new TestCaseDto
            {
                Id               = tc.Id,
                Title            = tc.Title,
                ExpectedBehavior = tc.ExpectedBehavior,
                TestSuiteId      = tc.TestSuiteId,
                CreatedAt        = tc.CreatedAt,
            }).ToList()
        };
    }

    public async Task<TestSuiteDto> CreateAsync(CreateTestSuiteDto dto)
    {
        var suite = new TestSuite { Name = dto.Name, Description = dto.Description };
        var created = await _repo.AddAsync(suite);
        return MapToDto(created);
    }

    public async Task<bool> UpdateAsync(int id, UpdateTestSuiteDto dto)
    {
        var suite = await _repo.GetByIdAsync(id);
        if (suite == null) return false;
        suite.Name        = dto.Name;
        suite.Description = dto.Description;
        return await _repo.UpdateAsync(suite);
    }

    public async Task<bool> DeleteAsync(int id) => await _repo.DeleteAsync(id);

    private static TestSuiteDto MapToDto(TestSuite ts) => new()
    {
        Id            = ts.Id,
        Name          = ts.Name,
        Description   = ts.Description,
        CreatedAt     = ts.CreatedAt,
        TestCaseCount = ts.TestCases?.Count ?? 0,
    };
}

// ── TestCaseService ───────────────────────────────────────────────────────────
public class TestCaseService : ITestCaseService
{
    private readonly ITestCaseRepository    _repo;
    private readonly ITestSuiteRepository   _suiteRepo;
    public TestCaseService(ITestCaseRepository repo, ITestSuiteRepository suiteRepo)
    {
        _repo      = repo;
        _suiteRepo = suiteRepo;
    }

    public async Task<List<TestCaseDto>> GetBySuiteIdAsync(int suiteId)
        => (await _repo.GetBySuiteIdAsync(suiteId)).Select(MapToDto).ToList();

    public async Task<TestCaseDto?> GetByIdAsync(int id)
    {
        var tc = await _repo.GetByIdAsync(id);
        return tc == null ? null : MapToDto(tc);
    }

    public async Task<TestCaseDto> CreateAsync(int suiteId, CreateTestCaseDto dto)
    {
        if (!await _suiteRepo.ExistsAsync(suiteId))
            throw new KeyNotFoundException($"TestSuite {suiteId} not found.");

        var tc = new TestCase
        {
            Title            = dto.Title,
            ExpectedBehavior = dto.ExpectedBehavior,
            TestSuiteId      = suiteId,
        };
        var created = await _repo.AddAsync(tc);
        return MapToDto(created);
    }

    public async Task<bool> UpdateAsync(int id, UpdateTestCaseDto dto)
    {
        var tc = await _repo.GetByIdAsync(id);
        if (tc == null) return false;
        tc.Title            = dto.Title;
        tc.ExpectedBehavior = dto.ExpectedBehavior;
        return await _repo.UpdateAsync(tc);
    }

    public async Task<bool> DeleteAsync(int id) => await _repo.DeleteAsync(id);

    private static TestCaseDto MapToDto(TestCase tc) => new()
    {
        Id               = tc.Id,
        Title            = tc.Title,
        ExpectedBehavior = tc.ExpectedBehavior,
        TestSuiteId      = tc.TestSuiteId,
        CreatedAt        = tc.CreatedAt,
    };
}

// ── TestRunService ────────────────────────────────────────────────────────────
public class TestRunService : ITestRunService
{
    private readonly ITestRunRepository  _repo;
    private readonly IUserRepository     _userRepo;
    public TestRunService(ITestRunRepository repo, IUserRepository userRepo)
    {
        _repo     = repo;
        _userRepo = userRepo;
    }

    public async Task<List<TestRunDto>> GetAllAsync()
        => (await _repo.GetAllAsync()).Select(MapToDto).ToList();

    public async Task<TestRunDto?> GetByIdAsync(int id)
    {
        var run = await _repo.GetByIdAsync(id);
        return run == null ? null : MapToDto(run);
    }

    public async Task<TestRunDetailDto?> GetDetailAsync(int id)
    {
        var run = await _repo.GetByIdWithResultsAsync(id);
        if (run == null) return null;

        return new TestRunDetailDto
        {
            Id          = run.Id,
            Status      = run.Status,
            UserId      = run.UserId,
            Username    = run.User.Username,
            ExecutedAt  = run.ExecutedAt,
            ResultCount = run.TestResults.Count,
            TestResults = run.TestResults.Select(r => new TestResultDto
            {
                Id             = r.Id,
                Status         = r.Status,
                Notes          = r.Notes,
                ResponseTimeMs = r.ResponseTimeMs,
                TestCaseId     = r.TestCaseId,
                TestCaseTitle  = r.TestCase.Title,
                TestRunId      = r.TestRunId,
                RecordedAt     = r.RecordedAt,
            }).ToList()
        };
    }

    public async Task<List<TestRunDto>> GetByUserIdAsync(int userId)
        => (await _repo.GetByUserIdAsync(userId)).Select(MapToDto).ToList();

    public async Task<TestRunDto> CreateAsync(CreateTestRunDto dto)
    {
        if (!await _userRepo.ExistsAsync(dto.UserId))
            throw new KeyNotFoundException($"User {dto.UserId} not found.");

        var run = new TestRun { UserId = dto.UserId, Status = "Pending" };
        var created = await _repo.AddAsync(run);
        // Reload with User navigation for username in response
        var full = await _repo.GetByIdAsync(created.Id);
        return MapToDto(full!);
    }

    public async Task<bool> UpdateStatusAsync(int id, string status)
        => await _repo.UpdateStatusAsync(id, status);

    public async Task<bool> DeleteAsync(int id) => await _repo.DeleteAsync(id);

    private static TestRunDto MapToDto(TestRun r) => new()
    {
        Id          = r.Id,
        Status      = r.Status,
        UserId      = r.UserId,
        Username    = r.User?.Username ?? string.Empty,
        ExecutedAt  = r.ExecutedAt,
        ResultCount = r.TestResults?.Count ?? 0,
    };
}

// ── TestResultService ─────────────────────────────────────────────────────────
public class TestResultService : ITestResultService
{
    private readonly ITestResultRepository _repo;
    private readonly ITestRunRepository    _runRepo;
    private readonly ITestCaseRepository   _caseRepo;

    public TestResultService(
        ITestResultRepository repo,
        ITestRunRepository    runRepo,
        ITestCaseRepository   caseRepo)
    {
        _repo     = repo;
        _runRepo  = runRepo;
        _caseRepo = caseRepo;
    }

    public async Task<List<TestResultDto>> GetByRunIdAsync(int runId)
        => (await _repo.GetByRunIdAsync(runId)).Select(MapToDto).ToList();

    public async Task<TestResultDto?> GetByIdAsync(int id)
    {
        var r = await _repo.GetByIdAsync(id);
        return r == null ? null : MapToDto(r);
    }

    public async Task<TestResultDto> CreateAsync(CreateTestResultDto dto)
    {
        if (!await _caseRepo.ExistsAsync(dto.TestCaseId))
            throw new KeyNotFoundException($"TestCase {dto.TestCaseId} not found.");

        var result = new TestResult
        {
            Status         = dto.Status,
            Notes          = dto.Notes,
            ResponseTimeMs = dto.ResponseTimeMs,
            TestCaseId     = dto.TestCaseId,
            TestRunId      = dto.TestRunId,
        };
        var created = await _repo.AddAsync(result);
        var full    = await _repo.GetByIdAsync(created.Id);
        return MapToDto(full!);
    }

    public async Task<bool> DeleteAsync(int id) => await _repo.DeleteAsync(id);

    private static TestResultDto MapToDto(TestResult r) => new()
    {
        Id             = r.Id,
        Status         = r.Status,
        Notes          = r.Notes,
        ResponseTimeMs = r.ResponseTimeMs,
        TestCaseId     = r.TestCaseId,
        TestCaseTitle  = r.TestCase?.Title ?? string.Empty,
        TestRunId      = r.TestRunId,
        RecordedAt     = r.RecordedAt,
    };
}
