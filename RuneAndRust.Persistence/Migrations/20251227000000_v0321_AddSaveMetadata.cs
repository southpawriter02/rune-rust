using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RuneAndRust.Persistence.Migrations
{
    /// <summary>
    /// v0.3.21a: Adds Metadata JSONB column to SaveGames table for lightweight previews.
    /// v0.3.21b: Enables rolling backup autosaves using negative slot numbers.
    /// </summary>
    /// <inheritdoc />
    public partial class v0321_AddSaveMetadata : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // v0.3.21a: Add Metadata column for save preview snapshots
            migrationBuilder.AddColumn<string>(
                name: "Metadata",
                table: "SaveGames",
                type: "jsonb",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Metadata",
                table: "SaveGames");
        }
    }
}
