using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PolicyService.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "PolicyPlans",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Title = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Type = table.Column<int>(type: "int", nullable: false),
                    BasePremium = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    CoverageLimit = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    DurationMonths = table.Column<int>(type: "int", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PolicyPlans", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "UserPolicies",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<int>(type: "int", nullable: false),
                    PolicyPlanId = table.Column<int>(type: "int", nullable: false),
                    PremiumAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    CoverageLimit = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    StartDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    EndDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false),
                    HasPreExistingConditions = table.Column<bool>(type: "bit", nullable: false),
                    IsSmoker = table.Column<bool>(type: "bit", nullable: false),
                    HasRecentHospitalization = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserPolicies", x => x.Id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PolicyPlans");

            migrationBuilder.DropTable(
                name: "UserPolicies");
        }
    }
}
