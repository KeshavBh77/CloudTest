using CloudTest.Data;
using CloudTest.Models;
using Microsoft.EntityFrameworkCore;

namespace CloudTest.Repositories;

// ── UserRepository ────────────────────────────────────────────────────────────
public class UserRepository : IUserRepository
{
    private readonly AppDbContext _context;
    public UserRepository(AppDbContext context) => _context = context;

    public async Task<User?> GetByIdAsync(int id)
        => await _context.Users
            .AsNoTracking()
            .FirstOrDefaultAsync(u => u.Id == id);

    public async Task<List<User>> GetAllAsync()
        => await _context.Users
            .AsNoTracking()
            .OrderBy(u => u.Username)
            .ToListAsync();

    public async Task<User> AddAsync(User user)
    {
        _context.Users.Add(user);
        await _context.SaveChangesAsync();
        return user;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var user = await _context.Users.FindAsync(id);
        if (user == null) return false;
        _context.Users.Remove(user);
        return await _context.SaveChangesAsync() > 0;
    }

    public async Task<bool> ExistsAsync(int id)
        => await _context.Users.AnyAsync(u => u.Id == id);

    public async Task<bool> EmailExistsAsync(string email)
        => await _context.Users.AnyAsync(u => u.Email == email);
}

// ── TestSuiteRepository ───────────────────────────────────────────────────────
public class TestSuiteRepository : ITestSuiteRepository
{
    private readonly AppDbContext _context;
    public TestSuiteRepository(AppDbContext context) => _context = context;

    public async Task<TestSuite?> GetByIdAsync(int id)
        => await _context.TestSuites
            .AsNoTracking()
            .FirstOrDefaultAsync(ts => ts.Id == id);

    // Eager load TestCases via Include() — one SQL query, no N+1
    public async Task<TestSuite?> GetByIdWithCasesAsync(int id)
        => await _context.TestSuites
            .Include(ts => ts.TestCases)
            .AsNoTracking()
            .FirstOrDefaultAsync(ts => ts.Id == id);

    public async Task<List<TestSuite>> GetAllAsync()
        => await _context.TestSuites
            .AsNoTracking()
            .OrderBy(ts => ts.CreatedAt)
            .ToListAsync();

    public async Task<TestSuite> AddAsync(TestSuite suite)
    {
        _context.TestSuites.Add(suite);
        await _context.SaveChangesAsync();
        return suite;
    }

    public async Task<bool> UpdateAsync(TestSuite suite)
    {
        _context.TestSuites.Update(suite);
        return await _context.SaveChangesAsync() > 0;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var suite = await _context.TestSuites.FindAsync(id);
        if (suite == null) return false;
        _context.TestSuites.Remove(suite);
        return await _context.SaveChangesAsync() > 0;
    }

    public async Task<bool> ExistsAsync(int id)
        => await _context.TestSuites.AnyAsync(ts => ts.Id == id);
}

// ── TestCaseRepository ────────────────────────────────────────────────────────
public class TestCaseRepository : ITestCaseRepository
{
    private readonly AppDbContext _context;
    public TestCaseRepository(AppDbContext context) => _context = context;

    public async Task<TestCase?> GetByIdAsync(int id)
        => await _context.TestCases
            .AsNoTracking()
            .FirstOrDefaultAsync(tc => tc.Id == id);

    // Uses indexed FK column TestSuiteId — fast lookup
    public async Task<List<TestCase>> GetBySuiteIdAsync(int suiteId)
        => await _context.TestCases
            .AsNoTracking()
            .Where(tc => tc.TestSuiteId == suiteId)
            .OrderBy(tc => tc.CreatedAt)
            .ToListAsync();

    public async Task<TestCase> AddAsync(TestCase testCase)
    {
        _context.TestCases.Add(testCase);
        await _context.SaveChangesAsync();
        return testCase;
    }

    public async Task<bool> UpdateAsync(TestCase testCase)
    {
        _context.TestCases.Update(testCase);
        return await _context.SaveChangesAsync() > 0;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var tc = await _context.TestCases.FindAsync(id);
        if (tc == null) return false;
        _context.TestCases.Remove(tc);
        return await _context.SaveChangesAsync() > 0;
    }

    public async Task<bool> ExistsAsync(int id)
        => await _context.TestCases.AnyAsync(tc => tc.Id == id);
}

// ── TestRunRepository ─────────────────────────────────────────────────────────
public class TestRunRepository : ITestRunRepository
{
    private readonly AppDbContext _context;
    public TestRunRepository(AppDbContext context) => _context = context;

    public async Task<TestRun?> GetByIdAsync(int id)
        => await _context.TestRuns
            .Include(tr => tr.User)
            .AsNoTracking()
            .FirstOrDefaultAsync(tr => tr.Id == id);

    // Eager load results + their TestCases in one query
    public async Task<TestRun?> GetByIdWithResultsAsync(int id)
        => await _context.TestRuns
            .Include(tr => tr.User)
            .Include(tr => tr.TestResults)
                .ThenInclude(r => r.TestCase)
            .AsNoTracking()
            .FirstOrDefaultAsync(tr => tr.Id == id);

    public async Task<List<TestRun>> GetAllAsync()
        => await _context.TestRuns
            .Include(tr => tr.User)
            .AsNoTracking()
            .OrderByDescending(tr => tr.ExecutedAt)
            .ToListAsync();

    public async Task<List<TestRun>> GetByUserIdAsync(int userId)
        => await _context.TestRuns
            .Include(tr => tr.User)
            .AsNoTracking()
            .Where(tr => tr.UserId == userId)
            .OrderByDescending(tr => tr.ExecutedAt)
            .ToListAsync();

    public async Task<TestRun> AddAsync(TestRun run)
    {
        _context.TestRuns.Add(run);
        await _context.SaveChangesAsync();
        return run;
    }

    public async Task<bool> UpdateStatusAsync(int id, string status)
    {
        var run = await _context.TestRuns.FindAsync(id);
        if (run == null) return false;
        run.Status = status;
        return await _context.SaveChangesAsync() > 0;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var run = await _context.TestRuns.FindAsync(id);
        if (run == null) return false;
        _context.TestRuns.Remove(run);
        return await _context.SaveChangesAsync() > 0;
    }
}

// ── TestResultRepository ──────────────────────────────────────────────────────
public class TestResultRepository : ITestResultRepository
{
    private readonly AppDbContext _context;
    public TestResultRepository(AppDbContext context) => _context = context;

    public async Task<TestResult?> GetByIdAsync(int id)
        => await _context.TestResults
            .Include(r => r.TestCase)
            .Include(r => r.TestRun)
            .AsNoTracking()
            .FirstOrDefaultAsync(r => r.Id == id);

    // Composite index on (TestRunId, TestCaseId) — sub-200ms at scale
    public async Task<List<TestResult>> GetByRunIdAsync(int runId)
        => await _context.TestResults
            .Include(r => r.TestCase)
            .AsNoTracking()
            .Where(r => r.TestRunId == runId)
            .OrderBy(r => r.RecordedAt)
            .ToListAsync();

    public async Task<TestResult> AddAsync(TestResult result)
    {
        _context.TestResults.Add(result);
        await _context.SaveChangesAsync();
        return result;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var result = await _context.TestResults.FindAsync(id);
        if (result == null) return false;
        _context.TestResults.Remove(result);
        return await _context.SaveChangesAsync() > 0;
    }
}
