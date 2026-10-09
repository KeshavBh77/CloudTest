using CloudTest.DTOs;
using CloudTest.Services;
using Microsoft.AspNetCore.Mvc;

namespace CloudTest.Controllers;

// ── UsersController ───────────────────────────────────────────────────────────
[ApiController]
[Route("api/[controller]")]
public class UsersController : ControllerBase
{
    private readonly IUserService _service;
    public UsersController(IUserService service) => _service = service;

    // Endpoint 1: GET api/users
    [HttpGet]
    [ProducesResponseType(typeof(List<UserDto>), 200)]
    public async Task<ActionResult<List<UserDto>>> GetAll()
        => Ok(await _service.GetAllAsync());

    // Endpoint 2: GET api/users/5
    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(UserDto), 200)]
    [ProducesResponseType(404)]
    public async Task<ActionResult<UserDto>> GetById(int id)
    {
        var user = await _service.GetByIdAsync(id);
        return user == null ? NotFound() : Ok(user);
    }

    // Endpoint 3: POST api/users
    [HttpPost]
    [ProducesResponseType(typeof(UserDto), 201)]
    [ProducesResponseType(400)]
    [ProducesResponseType(409)]
    public async Task<ActionResult<UserDto>> Create([FromBody] CreateUserDto dto)
    {
        try
        {
            var created = await _service.CreateAsync(dto);
            return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(new { message = ex.Message });
        }
    }

    // Endpoint 4: DELETE api/users/5
    [HttpDelete("{id:int}")]
    [ProducesResponseType(204)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> Delete(int id)
    {
        var deleted = await _service.DeleteAsync(id);
        return deleted ? NoContent() : NotFound();
    }
}

// ── TestSuitesController ──────────────────────────────────────────────────────
[ApiController]
[Route("api/[controller]")]
public class TestSuitesController : ControllerBase
{
    private readonly ITestSuiteService _service;
    public TestSuitesController(ITestSuiteService service) => _service = service;

    // Endpoint 5: GET api/testsuites
    [HttpGet]
    [ProducesResponseType(typeof(List<TestSuiteDto>), 200)]
    public async Task<ActionResult<List<TestSuiteDto>>> GetAll()
        => Ok(await _service.GetAllAsync());

    // Endpoint 6: GET api/testsuites/5 (with TestCases eager-loaded)
    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(TestSuiteDetailDto), 200)]
    [ProducesResponseType(404)]
    public async Task<ActionResult<TestSuiteDetailDto>> GetById(int id)
    {
        var suite = await _service.GetDetailAsync(id);
        return suite == null ? NotFound() : Ok(suite);
    }

    // Endpoint 7: POST api/testsuites
    [HttpPost]
    [ProducesResponseType(typeof(TestSuiteDto), 201)]
    [ProducesResponseType(400)]
    public async Task<ActionResult<TestSuiteDto>> Create([FromBody] CreateTestSuiteDto dto)
    {
        var created = await _service.CreateAsync(dto);
        return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
    }

    // Endpoint 8: PUT api/testsuites/5
    [HttpPut("{id:int}")]
    [ProducesResponseType(204)]
    [ProducesResponseType(400)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateTestSuiteDto dto)
    {
        var updated = await _service.UpdateAsync(id, dto);
        return updated ? NoContent() : NotFound();
    }

    // Endpoint 9: DELETE api/testsuites/5
    [HttpDelete("{id:int}")]
    [ProducesResponseType(204)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> Delete(int id)
    {
        var deleted = await _service.DeleteAsync(id);
        return deleted ? NoContent() : NotFound();
    }
}

// ── TestCasesController ───────────────────────────────────────────────────────
// Nested under testsuites — api/testsuites/5/testcases
[ApiController]
[Route("api/testsuites/{suiteId:int}/testcases")]
public class TestCasesController : ControllerBase
{
    private readonly ITestCaseService _service;
    public TestCasesController(ITestCaseService service) => _service = service;

    // Endpoint 10 (bonus): GET api/testsuites/5/testcases
    [HttpGet]
    [ProducesResponseType(typeof(List<TestCaseDto>), 200)]
    public async Task<ActionResult<List<TestCaseDto>>> GetBySuite(int suiteId)
        => Ok(await _service.GetBySuiteIdAsync(suiteId));

    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(TestCaseDto), 200)]
    [ProducesResponseType(404)]
    public async Task<ActionResult<TestCaseDto>> GetById(int suiteId, int id)
    {
        var tc = await _service.GetByIdAsync(id);
        return tc == null ? NotFound() : Ok(tc);
    }

    [HttpPost]
    [ProducesResponseType(typeof(TestCaseDto), 201)]
    [ProducesResponseType(400)]
    [ProducesResponseType(404)]
    public async Task<ActionResult<TestCaseDto>> Create(int suiteId, [FromBody] CreateTestCaseDto dto)
    {
        try
        {
            var created = await _service.CreateAsync(suiteId, dto);
            return CreatedAtAction(nameof(GetById),
                new { suiteId, id = created.Id }, created);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
    }

    [HttpPut("{id:int}")]
    [ProducesResponseType(204)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> Update(int suiteId, int id, [FromBody] UpdateTestCaseDto dto)
    {
        var updated = await _service.UpdateAsync(id, dto);
        return updated ? NoContent() : NotFound();
    }

    [HttpDelete("{id:int}")]
    [ProducesResponseType(204)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> Delete(int suiteId, int id)
    {
        var deleted = await _service.DeleteAsync(id);
        return deleted ? NoContent() : NotFound();
    }
}

// ── TestRunsController ────────────────────────────────────────────────────────
[ApiController]
[Route("api/[controller]")]
public class TestRunsController : ControllerBase
{
    private readonly ITestRunService _service;
    public TestRunsController(ITestRunService service) => _service = service;

    [HttpGet]
    [ProducesResponseType(typeof(List<TestRunDto>), 200)]
    public async Task<ActionResult<List<TestRunDto>>> GetAll()
        => Ok(await _service.GetAllAsync());

    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(TestRunDetailDto), 200)]
    [ProducesResponseType(404)]
    public async Task<ActionResult<TestRunDetailDto>> GetById(int id)
    {
        var run = await _service.GetDetailAsync(id);
        return run == null ? NotFound() : Ok(run);
    }

    [HttpGet("user/{userId:int}")]
    [ProducesResponseType(typeof(List<TestRunDto>), 200)]
    public async Task<ActionResult<List<TestRunDto>>> GetByUser(int userId)
        => Ok(await _service.GetByUserIdAsync(userId));

    [HttpPost]
    [ProducesResponseType(typeof(TestRunDto), 201)]
    [ProducesResponseType(400)]
    [ProducesResponseType(404)]
    public async Task<ActionResult<TestRunDto>> Create([FromBody] CreateTestRunDto dto)
    {
        try
        {
            var created = await _service.CreateAsync(dto);
            return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
    }

    [HttpPatch("{id:int}/status")]
    [ProducesResponseType(204)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> UpdateStatus(int id, [FromBody] string status)
    {
        var updated = await _service.UpdateStatusAsync(id, status);
        return updated ? NoContent() : NotFound();
    }

    [HttpDelete("{id:int}")]
    [ProducesResponseType(204)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> Delete(int id)
    {
        var deleted = await _service.DeleteAsync(id);
        return deleted ? NoContent() : NotFound();
    }
}

// ── TestResultsController ─────────────────────────────────────────────────────
[ApiController]
[Route("api/[controller]")]
public class TestResultsController : ControllerBase
{
    private readonly ITestResultService _service;
    public TestResultsController(ITestResultService service) => _service = service;

    [HttpGet("run/{runId:int}")]
    [ProducesResponseType(typeof(List<TestResultDto>), 200)]
    public async Task<ActionResult<List<TestResultDto>>> GetByRun(int runId)
        => Ok(await _service.GetByRunIdAsync(runId));

    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(TestResultDto), 200)]
    [ProducesResponseType(404)]
    public async Task<ActionResult<TestResultDto>> GetById(int id)
    {
        var result = await _service.GetByIdAsync(id);
        return result == null ? NotFound() : Ok(result);
    }

    [HttpPost]
    [ProducesResponseType(typeof(TestResultDto), 201)]
    [ProducesResponseType(400)]
    [ProducesResponseType(404)]
    public async Task<ActionResult<TestResultDto>> Create([FromBody] CreateTestResultDto dto)
    {
        try
        {
            var created = await _service.CreateAsync(dto);
            return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
    }

    [HttpDelete("{id:int}")]
    [ProducesResponseType(204)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> Delete(int id)
    {
        var deleted = await _service.DeleteAsync(id);
        return deleted ? NoContent() : NotFound();
    }
}
