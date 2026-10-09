using CloudTest.Models;

namespace CloudTest.Repositories;

public interface IUserRepository
{
    Task<User?>        GetByIdAsync(int id);
    Task<List<User>>   GetAllAsync();
    Task<User>         AddAsync(User user);
    Task<bool>         DeleteAsync(int id);
    Task<bool>         ExistsAsync(int id);
    Task<bool>         EmailExistsAsync(string email);
}

public interface ITestSuiteRepository
{
    Task<TestSuite?>       GetByIdAsync(int id);
    Task<TestSuite?>       GetByIdWithCasesAsync(int id);
    Task<List<TestSuite>>  GetAllAsync();
    Task<TestSuite>        AddAsync(TestSuite suite);
    Task<bool>             UpdateAsync(TestSuite suite);
    Task<bool>             DeleteAsync(int id);
    Task<bool>             ExistsAsync(int id);
}

public interface ITestCaseRepository
{
    Task<TestCase?>       GetByIdAsync(int id);
    Task<List<TestCase>>  GetBySuiteIdAsync(int suiteId);
    Task<TestCase>        AddAsync(TestCase testCase);
    Task<bool>            UpdateAsync(TestCase testCase);
    Task<bool>            DeleteAsync(int id);
    Task<bool>            ExistsAsync(int id);
}

public interface ITestRunRepository
{
    Task<TestRun?>       GetByIdAsync(int id);
    Task<TestRun?>       GetByIdWithResultsAsync(int id);
    Task<List<TestRun>>  GetAllAsync();
    Task<List<TestRun>>  GetByUserIdAsync(int userId);
    Task<TestRun>        AddAsync(TestRun run);
    Task<bool>           UpdateStatusAsync(int id, string status);
    Task<bool>           DeleteAsync(int id);
}

public interface ITestResultRepository
{
    Task<TestResult?>       GetByIdAsync(int id);
    Task<List<TestResult>>  GetByRunIdAsync(int runId);
    Task<TestResult>        AddAsync(TestResult result);
    Task<bool>              DeleteAsync(int id);
}
