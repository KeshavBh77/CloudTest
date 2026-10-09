using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CloudTest.Migrations
{
    [DbContext(typeof(CloudTest.Data.AppDbContext))]
    [Migration("20251001000000_InitialCreate")]
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Users",
                columns: table => new
                {
                    Id        = table.Column<int>(nullable: false).Annotation("SqlServer:Identity", "1, 1"),
                    Username  = table.Column<string>(maxLength: 100, nullable: false),
                    Email     = table.Column<string>(maxLength: 200, nullable: false),
                    CreatedAt = table.Column<DateTime>(nullable: false, defaultValueSql: "GETUTCDATE()")
                },
                constraints: table => table.PrimaryKey("PK_Users", x => x.Id));

            migrationBuilder.CreateIndex(
                name: "IX_Users_Email",
                table: "Users",
                column: "Email",
                unique: true);

            migrationBuilder.CreateTable(
                name: "TestSuites",
                columns: table => new
                {
                    Id          = table.Column<int>(nullable: false).Annotation("SqlServer:Identity", "1, 1"),
                    Name        = table.Column<string>(maxLength: 200, nullable: false),
                    Description = table.Column<string>(maxLength: 1000, nullable: false),
                    CreatedAt   = table.Column<DateTime>(nullable: false, defaultValueSql: "GETUTCDATE()")
                },
                constraints: table => table.PrimaryKey("PK_TestSuites", x => x.Id));

            migrationBuilder.CreateTable(
                name: "TestCases",
                columns: table => new
                {
                    Id               = table.Column<int>(nullable: false).Annotation("SqlServer:Identity", "1, 1"),
                    Title            = table.Column<string>(maxLength: 300, nullable: false),
                    ExpectedBehavior = table.Column<string>(maxLength: 2000, nullable: false),
                    CreatedAt        = table.Column<DateTime>(nullable: false, defaultValueSql: "GETUTCDATE()"),
                    TestSuiteId      = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TestCases", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TestCases_TestSuites_TestSuiteId",
                        column: x => x.TestSuiteId,
                        principalTable: "TestSuites",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_TestCases_TestSuiteId",
                table: "TestCases",
                column: "TestSuiteId");

            migrationBuilder.CreateTable(
                name: "TestRuns",
                columns: table => new
                {
                    Id         = table.Column<int>(nullable: false).Annotation("SqlServer:Identity", "1, 1"),
                    ExecutedAt = table.Column<DateTime>(nullable: false, defaultValueSql: "GETUTCDATE()"),
                    Status     = table.Column<string>(maxLength: 50, nullable: false),
                    UserId     = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TestRuns", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TestRuns_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_TestRuns_UserId",
                table: "TestRuns",
                column: "UserId");

            migrationBuilder.CreateTable(
                name: "TestResults",
                columns: table => new
                {
                    Id             = table.Column<int>(nullable: false).Annotation("SqlServer:Identity", "1, 1"),
                    Status         = table.Column<string>(maxLength: 10, nullable: false),
                    Notes          = table.Column<string>(maxLength: 2000, nullable: true),
                    ResponseTimeMs = table.Column<long>(nullable: false),
                    RecordedAt     = table.Column<DateTime>(nullable: false, defaultValueSql: "GETUTCDATE()"),
                    TestCaseId     = table.Column<int>(nullable: false),
                    TestRunId      = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TestResults", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TestResults_TestCases_TestCaseId",
                        column: x => x.TestCaseId,
                        principalTable: "TestCases",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TestResults_TestRuns_TestRunId",
                        column: x => x.TestRunId,
                        principalTable: "TestRuns",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            // Composite index on both FKs — fast JOIN for GetByRunId
            migrationBuilder.CreateIndex(
                name: "IX_TestResults_TestRunId_TestCaseId",
                table: "TestResults",
                columns: new[] { "TestRunId", "TestCaseId" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(name: "TestResults");
            migrationBuilder.DropTable(name: "TestRuns");
            migrationBuilder.DropTable(name: "TestCases");
            migrationBuilder.DropTable(name: "TestSuites");
            migrationBuilder.DropTable(name: "Users");
        }
    }
}
