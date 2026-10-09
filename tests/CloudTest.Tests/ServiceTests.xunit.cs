using CloudTest.DTOs;
using CloudTest.Models;
using CloudTest.Repositories;
using CloudTest.Services;
using Moq;
using Xunit;

namespace CloudTest.Tests;

// ── TestSuiteService Tests (xUnit) ────────────────────────────────────────────
public class TestSuiteServiceTests
{
    private readonly Mock<ITestSuiteRepository> _repoMock;
    private readonly TestSuiteService           _service;

    public TestSuiteServiceTests()
    {
        _repoMock = new Mock<ITestSuiteRepository>();
        _service  = new TestSuiteService(_repoMock.Object);
    }

    [Fact]
    public async Task GetByIdAsync_ReturnsDto_WhenSuiteExists()
    {
        // Arrange
        var suite = new TestSuite { Id = 1, Name = "Auth Tests", Description = "Auth suite", CreatedAt = DateTime.UtcNow };
        _repoMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(suite);

        // Act
        var result = await _service.GetByIdAsync(1);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(1,            result.Id);
        Assert.Equal("Auth Tests", result.Name);
    }

    [Fact]
    public async Task GetByIdAsync_ReturnsNull_WhenNotFound()
    {
        _repoMock.Setup(r => r.GetByIdAsync(99)).ReturnsAsync((TestSuite?)null);

        var result = await _service.GetByIdAsync(99);

        Assert.Null(result);
    }

    [Fact]
    public async Task GetAllAsync_ReturnsMappedDtos()
    {
        var suites = new List<TestSuite>
        {
            new() { Id = 1, Name = "Suite A", Description = "", CreatedAt = DateTime.UtcNow },
            new() { Id = 2, Name = "Suite B", Description = "", CreatedAt = DateTime.UtcNow },
        };
        _repoMock.Setup(r => r.GetAllAsync()).ReturnsAsync(suites);

        var result = await _service.GetAllAsync();

        Assert.Equal(2, result.Count);
        Assert.Equal("Suite A", result[0].Name);
        Assert.Equal("Suite B", result[1].Name);
    }

    [Fact]
    public async Task CreateAsync_CallsAddAndReturnsDto()
    {
        var dto    = new CreateTestSuiteDto { Name = "New Suite", Description = "Desc" };
        var entity = new TestSuite { Id = 3, Name = dto.Name, Description = dto.Description, CreatedAt = DateTime.UtcNow };

        _repoMock.Setup(r => r.AddAsync(It.IsAny<TestSuite>())).ReturnsAsync(entity);

        var result = await _service.CreateAsync(dto);

        Assert.Equal(3,           result.Id);
        Assert.Equal("New Suite", result.Name);
        _repoMock.Verify(r => r.AddAsync(It.IsAny<TestSuite>()), Times.Once);
    }

    [Fact]
    public async Task UpdateAsync_ReturnsFalse_WhenNotFound()
    {
        _repoMock.Setup(r => r.GetByIdAsync(99)).ReturnsAsync((TestSuite?)null);

        var result = await _service.UpdateAsync(99, new UpdateTestSuiteDto { Name = "X", Description = "Y" });

        Assert.False(result);
    }

    [Fact]
    public async Task UpdateAsync_UpdatesAndReturnsTrue_WhenFound()
    {
        var suite = new TestSuite { Id = 1, Name = "Old", Description = "Old desc", CreatedAt = DateTime.UtcNow };
        _repoMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(suite);
        _repoMock.Setup(r => r.UpdateAsync(It.IsAny<TestSuite>())).ReturnsAsync(true);

        var result = await _service.UpdateAsync(1, new UpdateTestSuiteDto { Name = "New", Description = "New desc" });

        Assert.True(result);
        _repoMock.Verify(r => r.UpdateAsync(It.Is<TestSuite>(s => s.Name == "New")), Times.Once);
    }

    [Fact]
    public async Task DeleteAsync_DelegatesToRepository()
    {
        _repoMock.Setup(r => r.DeleteAsync(1)).ReturnsAsync(true);
        var result = await _service.DeleteAsync(1);
        Assert.True(result);
    }

    [Theory]
    [InlineData(1,  true)]
    [InlineData(99, false)]
    public async Task DeleteAsync_ReturnsExpected(int id, bool expected)
    {
        _repoMock.Setup(r => r.DeleteAsync(id)).ReturnsAsync(expected);
        var result = await _service.DeleteAsync(id);
        Assert.Equal(expected, result);
    }
}

// ── TestCaseService Tests (xUnit) ─────────────────────────────────────────────
public class TestCaseServiceTests
{
    private readonly Mock<ITestCaseRepository>  _repoMock;
    private readonly Mock<ITestSuiteRepository> _suiteRepoMock;
    private readonly TestCaseService            _service;

    public TestCaseServiceTests()
    {
        _repoMock      = new Mock<ITestCaseRepository>();
        _suiteRepoMock = new Mock<ITestSuiteRepository>();
        _service       = new TestCaseService(_repoMock.Object, _suiteRepoMock.Object);
    }

    [Fact]
    public async Task CreateAsync_ThrowsKeyNotFound_WhenSuiteMissing()
    {
        _suiteRepoMock.Setup(r => r.ExistsAsync(5)).ReturnsAsync(false);
        var dto = new CreateTestCaseDto { Title = "T", ExpectedBehavior = "B" };

        await Assert.ThrowsAsync<KeyNotFoundException>(
            () => _service.CreateAsync(5, dto));
    }

    [Fact]
    public async Task CreateAsync_ReturnsDto_WhenSuiteExists()
    {
        _suiteRepoMock.Setup(r => r.ExistsAsync(1)).ReturnsAsync(true);
        var dto    = new CreateTestCaseDto { Title = "Login test", ExpectedBehavior = "Returns 200" };
        var entity = new TestCase { Id = 10, Title = dto.Title, ExpectedBehavior = dto.ExpectedBehavior, TestSuiteId = 1, CreatedAt = DateTime.UtcNow };

        _repoMock.Setup(r => r.AddAsync(It.IsAny<TestCase>())).ReturnsAsync(entity);

        var result = await _service.CreateAsync(1, dto);

        Assert.Equal(10,           result.Id);
        Assert.Equal("Login test", result.Title);
    }

    [Fact]
    public async Task GetByIdAsync_ReturnsNull_WhenMissing()
    {
        _repoMock.Setup(r => r.GetByIdAsync(99)).ReturnsAsync((TestCase?)null);
        Assert.Null(await _service.GetByIdAsync(99));
    }

    [Fact]
    public async Task GetBySuiteIdAsync_ReturnsMappedList()
    {
        var cases = new List<TestCase>
        {
            new() { Id = 1, Title = "Case A", ExpectedBehavior = "E", TestSuiteId = 1, CreatedAt = DateTime.UtcNow },
            new() { Id = 2, Title = "Case B", ExpectedBehavior = "E", TestSuiteId = 1, CreatedAt = DateTime.UtcNow },
        };
        _repoMock.Setup(r => r.GetBySuiteIdAsync(1)).ReturnsAsync(cases);

        var result = await _service.GetBySuiteIdAsync(1);

        Assert.Equal(2, result.Count);
    }
}

// ── UserService Tests (xUnit) ─────────────────────────────────────────────────
public class UserServiceTests
{
    private readonly Mock<IUserRepository> _repoMock;
    private readonly UserService           _service;

    public UserServiceTests()
    {
        _repoMock = new Mock<IUserRepository>();
        _service  = new UserService(_repoMock.Object);
    }

    [Fact]
    public async Task CreateAsync_ThrowsInvalidOperation_WhenEmailExists()
    {
        _repoMock.Setup(r => r.EmailExistsAsync("dup@test.com")).ReturnsAsync(true);
        await Assert.ThrowsAsync<InvalidOperationException>(
            () => _service.CreateAsync(new CreateUserDto { Username = "X", Email = "dup@test.com" }));
    }

    [Fact]
    public async Task CreateAsync_ReturnsDto_WhenEmailUnique()
    {
        _repoMock.Setup(r => r.EmailExistsAsync("new@test.com")).ReturnsAsync(false);
        var user = new User { Id = 1, Username = "Keshav", Email = "new@test.com", CreatedAt = DateTime.UtcNow };
        _repoMock.Setup(r => r.AddAsync(It.IsAny<User>())).ReturnsAsync(user);

        var result = await _service.CreateAsync(new CreateUserDto { Username = "Keshav", Email = "new@test.com" });

        Assert.Equal("Keshav",       result.Username);
        Assert.Equal("new@test.com", result.Email);
    }

    [Fact]
    public async Task GetAllAsync_ReturnsMappedUsers()
    {
        var users = new List<User>
        {
            new() { Id = 1, Username = "Alice", Email = "a@a.com", CreatedAt = DateTime.UtcNow },
            new() { Id = 2, Username = "Bob",   Email = "b@b.com", CreatedAt = DateTime.UtcNow },
        };
        _repoMock.Setup(r => r.GetAllAsync()).ReturnsAsync(users);

        var result = await _service.GetAllAsync();
        Assert.Equal(2, result.Count);
    }
}
