using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Auth.Api.Migrations
{
    /// <inheritdoc />
    public partial class Add_LoginType_To_LogingLogger : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<short>(
                name: "LoginType",
                table: "UserLoginLogs",
                type: "smallint",
                nullable: false,
                defaultValue: (short)0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "LoginType",
                table: "UserLoginLogs");
        }
    }
}
