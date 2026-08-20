using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ClaimService.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class ChangeClaimStatusToString : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "Status",
                table: "Claims",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.Sql("UPDATE Claims SET Status = 'Submitted' WHERE Status = '1'");
            migrationBuilder.Sql("UPDATE Claims SET Status = 'Submitted' WHERE Status = '2'");
            migrationBuilder.Sql("UPDATE Claims SET Status = 'Approved' WHERE Status = '3'");
            migrationBuilder.Sql("UPDATE Claims SET Status = 'Rejected' WHERE Status = '4'");
            migrationBuilder.Sql("UPDATE Claims SET Status = 'Approved' WHERE Status = '5'");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("UPDATE Claims SET Status = '1' WHERE Status = 'Submitted'");
            migrationBuilder.Sql("UPDATE Claims SET Status = '3' WHERE Status = 'Approved'");
            migrationBuilder.Sql("UPDATE Claims SET Status = '4' WHERE Status = 'Rejected'");

            migrationBuilder.AlterColumn<int>(
                name: "Status",
                table: "Claims",
                type: "int",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");
        }
    }
}
