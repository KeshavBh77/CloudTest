using CloudTest.Data;
using CloudTest.Repositories;
using CloudTest.Services;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// ── Database ──────────────────────────────────────────────────────────────────
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("Default"),
        sqlOptions => sqlOptions.EnableRetryOnFailure(
            maxRetryCount: 3,
            maxRetryDelay: TimeSpan.FromSeconds(5),
            errorNumbersToAdd: null)));

// ── Repositories (Scoped = new instance per HTTP request) ────────────────────
builder.Services.AddScoped<IUserRepository,       UserRepository>();
builder.Services.AddScoped<ITestSuiteRepository,  TestSuiteRepository>();
builder.Services.AddScoped<ITestCaseRepository,   TestCaseRepository>();
builder.Services.AddScoped<ITestRunRepository,    TestRunRepository>();
builder.Services.AddScoped<ITestResultRepository, TestResultRepository>();

// ── Services ──────────────────────────────────────────────────────────────────
builder.Services.AddScoped<IUserService,       UserService>();
builder.Services.AddScoped<ITestSuiteService,  TestSuiteService>();
builder.Services.AddScoped<ITestCaseService,   TestCaseService>();
builder.Services.AddScoped<ITestRunService,    TestRunService>();
builder.Services.AddScoped<ITestResultService, TestResultService>();

// ── API + Swagger ─────────────────────────────────────────────────────────────
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new()
    {
        Title       = "CloudTest API",
        Version     = "v1",
        Description = "ASP.NET Core REST API · 10 endpoints · 5 normalized SQL Server tables"
    });
});

// ── CORS (for local dev) ──────────────────────────────────────────────────────
builder.Services.AddCors(options =>
    options.AddDefaultPolicy(policy =>
        policy.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod()));

var app = builder.Build();

// ── Auto-apply migrations on startup ─────────────────────────────────────────
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    db.Database.Migrate();
}

// ── Middleware pipeline ───────────────────────────────────────────────────────
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "CloudTest API v1");
        c.RoutePrefix = string.Empty; // Swagger at root /
    });
}

app.UseCors();
if (!app.Environment.IsDevelopment())
    app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

app.Run();

// Expose for integration tests
public partial class Program { }
