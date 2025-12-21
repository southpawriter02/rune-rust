using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RuneAndRust.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddRestSystemColumns : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Tags",
                table: "Items",
                type: "jsonb",
                nullable: false,
                defaultValue: "[]");

            migrationBuilder.AddColumn<string>(
                name: "ActiveStatusEffects",
                table: "Characters",
                type: "jsonb",
                nullable: false,
                defaultValue: "[]");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Tags",
                table: "Items");

            migrationBuilder.DropColumn(
                name: "ActiveStatusEffects",
                table: "Characters");
        }
    }
}
