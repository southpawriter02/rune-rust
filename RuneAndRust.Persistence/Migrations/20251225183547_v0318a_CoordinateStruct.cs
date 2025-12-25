using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RuneAndRust.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class v0318a_CoordinateStruct : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "ChargeTurns",
                table: "ActiveAbilities",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<float>(
                name: "InterruptThreshold",
                table: "ActiveAbilities",
                type: "real",
                nullable: false,
                defaultValue: 0f);

            migrationBuilder.AddColumn<string>(
                name: "TelegraphMessage",
                table: "ActiveAbilities",
                type: "text",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ChargeTurns",
                table: "ActiveAbilities");

            migrationBuilder.DropColumn(
                name: "InterruptThreshold",
                table: "ActiveAbilities");

            migrationBuilder.DropColumn(
                name: "TelegraphMessage",
                table: "ActiveAbilities");
        }
    }
}
