using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Auth.Api.Migrations
{
    /// <inheritdoc />
    public partial class AddTryCountAndUserIpToVerificationCodes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<bool>(
                name: "IsUsed",
                table: "VerificationCodes",
                type: "bit",
                nullable: false,
                defaultValue: false,
                oldClrType: typeof(bool),
                oldType: "bit",
                oldNullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SentFromIP",
                table: "VerificationCodes",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "TryCount",
                table: "VerificationCodes",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "SentFromIP",
                table: "VerificationCodes");

            migrationBuilder.DropColumn(
                name: "TryCount",
                table: "VerificationCodes");

            migrationBuilder.AlterColumn<bool>(
                name: "IsUsed",
                table: "VerificationCodes",
                type: "bit",
                nullable: true,
                oldClrType: typeof(bool),
                oldType: "bit",
                oldDefaultValue: false);
        }
    }
}
