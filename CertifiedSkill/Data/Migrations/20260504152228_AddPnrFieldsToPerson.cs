using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CertifiedSkill.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddPnrFieldsToPerson : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "EncryptedPnr",
                table: "Persons",
                type: "nvarchar(512)",
                maxLength: 512,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PnrHash",
                table: "Persons",
                type: "nvarchar(88)",
                maxLength: 88,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PnrKeyVersion",
                table: "Persons",
                type: "nvarchar(64)",
                maxLength: 64,
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Persons_PnrHash",
                table: "Persons",
                column: "PnrHash",
                unique: true,
                filter: "[PnrHash] IS NOT NULL");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Persons_PnrHash",
                table: "Persons");

            migrationBuilder.DropColumn(
                name: "EncryptedPnr",
                table: "Persons");

            migrationBuilder.DropColumn(
                name: "PnrHash",
                table: "Persons");

            migrationBuilder.DropColumn(
                name: "PnrKeyVersion",
                table: "Persons");
        }
    }
}
