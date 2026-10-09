using CloudTest.Data;
using CloudTest.DTOs;
using CloudTest.Models;
using CloudTest.Repositories;
using CloudTest.Services;
using Microsoft.EntityFrameworkCore;
using NUnit.Framework;

namespace CloudTest.Tests;

// ── TestRunService Tests (NUnit + EF InMemory) ────────────────────────────────
[TestFixture]
public class TestRunServiceNUnitTests
{
    private AppDbContext     _context  = null!;
    private TestRunService   _service  = null!;
    private UserRepository   _userRepo = null!;
    private TestRunRepository _runRepo = null!;

    [SetUp]
    public void SetUp()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString()) // fresh DB per test
            .Options;

        _context  = new AppDbContext(options);
        _userRepo = new UserRepository(_context);
        _runRepo  = new TestRunRepository(_context);
        _service  = new TestRunService(_runRepo, _userRepo);
    }

    [TearDown]
    public void TearDown() => _context.Dispose();

    [Test]
    public void CreateAsync_ThrowsKeyNotFound_WhenUserMissing()
    {
        Assert.ThrowsAsync<KeyNotFoundException>(async () =>
            await _service.CreateAsync(new CreateTestRunDto { UserId = 999 }));
    }

    [Test]
    public async Task CreateAsync_ReturnsRun_WhenUserExists()
    {
        // Seed user
        var user = new User { Username = "Keshav", Email = "k@test.com" };
        await _userRepo.AddAsync(user);

        var result = await _service.CreateAsync(new CreateTestRunDto { UserId = user.Id });

        Assert.That(result,          Is.Not.Null);
        Assert.That(result.UserId,   Is.EqualTo(user.Id));
        Assert.That(result.Status,   Is.EqualTo("Pending"));
        Assert.That(result.Username, Is.EqualTo("Keshav"));
    }

    [Test]
    public async Task GetAllAsync_ReturnsAllRuns()
    {
        var user = new User { Username = "Alice", Email = "alice@test.com" };
        await _userRepo.AddAsync(user);

        await _runRepo.AddAsync(new TestRun { UserId = user.Id, Status = "Pending" });
        await _runRepo.AddAsync(new TestRun { UserId = user.Id, Status = "Completed" });

        var result = await _service.GetAllAsync();
        Assert.That(result.Count, Is.EqualTo(2));
    }

    [Test]
    public async Task UpdateStatusAsync_ReturnsFalse_WhenRunMissing()
    {
        var result = await _service.UpdateStatusAsync(999, "Completed");
        Assert.That(result, Is.False);
    }

    [Test]
    public async Task UpdateStatusAsync_ReturnsTrue_WhenRunExists()
    {
        var user = new User { Username = "Bob", Email = "bob@test.com" };
        await _userRepo.AddAsync(user);
        var run = await _runRepo.AddAsync(new TestRun { UserId = user.Id, Status = "Pending" });

        var result = await _service.UpdateStatusAsync(run.Id, "Completed");
        Assert.That(result, Is.True);
    }

    [Test]
    public async Task DeleteAsync_ReturnsFalse_WhenRunMissing()
    {
        var result = await _service.DeleteAsync(999);
        Assert.That(result, Is.False);
    }

    [TestCase("Pass")]
    [TestCase("Fail")]
    [TestCase("Skip")]
    public async Task CreateTestResult_AcceptsValidStatuses(string status)
    {
        // Seed required data
        var user  = new User  { Username = "U", Email = $"{status}@test.com" };
        await _userRepo.AddAsync(user);

        var suiteRepo  = new TestSuiteRepository(_context);
        var caseRepo   = new TestCaseRepository(_context);
        var resultRepo = new TestResultRepository(_context);

        var suite = await suiteRepo.AddAsync(new TestSuite { Name = "S", Description = "" });
        var tc    = await caseRepo.AddAsync(new TestCase { Title = "T", ExpectedBehavior = "E", TestSuiteId = suite.Id });
        var run   = await _runRepo.AddAsync(new TestRun { UserId = user.Id, Status = "Running" });

        var resultService = new TestResultService(resultRepo, _runRepo, caseRepo);
        var dto = new CreateTestResultDto
        {
            Status         = status,
            TestCaseId     = tc.Id,
            TestRunId      = run.Id,
            ResponseTimeMs = 150,
        };

        var result = await resultService.CreateAsync(dto);

        Assert.That(result.Status, Is.EqualTo(status));
        Assert.That(result.ResponseTimeMs, Is.EqualTo(150));
    }
}

// ── Repository Tests (NUnit + EF InMemory) ────────────────────────────────────
[TestFixture]
public class TestSuiteRepositoryNUnitTests
{
    private AppDbContext          _context = null!;
    private TestSuiteRepository   _repo    = null!;

    [SetUp]
    public void SetUp()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        _context = new AppDbContext(options);
        _repo    = new TestSuiteRepository(_context);
    }

    [TearDown]
    public void TearDown() => _context.Dispose();

    [Test]
    public async Task AddAsync_PersistsAndReturnsWithId()
    {
        var suite = new TestSuite { Name = "API Tests", Description = "All API tests" };
        var saved = await _repo.AddAsync(suite);

        Assert.That(saved.Id, Is.GreaterThan(0));
        Assert.That(saved.Name, Is.EqualTo("API Tests"));
    }

    [Test]
    public async Task GetByIdAsync_ReturnsNull_WhenMissing()
    {
        var result = await _repo.GetByIdAsync(999);
        Assert.That(result, Is.Null);
    }

    [Test]
    public async Task GetAllAsync_ReturnsAllSuites()
    {
        await _repo.AddAsync(new TestSuite { Name = "A", Description = "" });
        await _repo.AddAsync(new TestSuite { Name = "B", Description = "" });

        var all = await _repo.GetAllAsync();
        Assert.That(all.Count, Is.EqualTo(2));
    }

    [Test]
    public async Task DeleteAsync_RemovesSuite()
    {
        var suite = await _repo.AddAsync(new TestSuite { Name = "ToDelete", Description = "" });
        var deleted = await _repo.DeleteAsync(suite.Id);

        Assert.That(deleted, Is.True);
        Assert.That(await _repo.GetByIdAsync(suite.Id), Is.Null);
    }

    [Test]
    public async Task UpdateAsync_PersistsChanges()
    {
        var suite = await _repo.AddAsync(new TestSuite { Name = "Original", Description = "Old" });
        suite.Name = "Updated";
        await _repo.UpdateAsync(suite);

        var fetched = await _repo.GetByIdAsync(suite.Id);
        Assert.That(fetched!.Name, Is.EqualTo("Updated"));
    }

    [TestCase(1)]
    [TestCase(2)]
    [TestCase(3)]
    public async Task ExistsAsync_ReturnsFalse_ForNonExistentIds(int id)
    {
        var exists = await _repo.ExistsAsync(id);
        Assert.That(exists, Is.False);
    }
}
