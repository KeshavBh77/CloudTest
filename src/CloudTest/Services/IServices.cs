using CloudTest.DTOs;

namespace CloudTest.Services;

public interface IUserService
{
    Task<List<UserDto>>  GetAllAsync();
    Task<UserDto?>       GetByIdAsync(int id);
    Task<UserDto>        CreateAsync(CreateUserDto dto);
    Task<bool>           DeleteAsync(int id);
}

public interface ITestSuiteService
{
    Task<List<TestSuiteDto>>  GetAllAsync();
    Task<TestSuiteDto?>       GetByIdAsync(int id);
    Task<TestSuiteDetailDto?> GetDetailAsync(int id);
    Task<TestSuiteDto>        CreateAsync(CreateTestSuiteDto dto);
    Task<bool>                UpdateAsync(int id, UpdateTestSuiteDto dto);
    Task<bool>                DeleteAsync(int id);
}

public interface ITestCaseService
{
    Task<List<TestCaseDto>> GetBySuiteIdAsync(int suiteId);
    Task<TestCaseDto?>      GetByIdAsync(int id);
    Task<TestCaseDto>       CreateAsync(int suiteId, CreateTestCaseDto dto);
    Task<bool>              UpdateAsync(int id, UpdateTestCaseDto dto);
    Task<bool>              DeleteAsync(int id);
}

public interface ITestRunService
{
    Task<List<TestRunDto>>   GetAllAsync();
    Task<TestRunDto?>        GetByIdAsync(int id);
    Task<TestRunDetailDto?>  GetDetailAsync(int id);
    Task<List<TestRunDto>>   GetByUserIdAsync(int userId);
    Task<TestRunDto>         CreateAsync(CreateTestRunDto dto);
    Task<bool>               UpdateStatusAsync(int id, string status);
    Task<bool>               DeleteAsync(int id);
}

public interface ITestResultService
{
    Task<List<TestResultDto>> GetByRunIdAsync(int runId);
    Task<TestResultDto?>      GetByIdAsync(int id);
    Task<TestResultDto>       CreateAsync(CreateTestResultDto dto);
    Task<bool>                DeleteAsync(int id);
}
