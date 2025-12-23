using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RuneAndRust.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class MakeActiveAbilityArchetypeNullable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<int>(
                name: "Archetype",
                table: "ActiveAbilities",
                type: "integer",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "integer");

            migrationBuilder.CreateTable(
                name: "BiomeDefinitions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    BiomeId = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Description = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: false),
                    AvailableTemplates = table.Column<string>(type: "jsonb", nullable: false),
                    DescriptorCategories = table.Column<string>(type: "jsonb", nullable: false),
                    MinRoomCount = table.Column<int>(type: "integer", nullable: false),
                    MaxRoomCount = table.Column<int>(type: "integer", nullable: false),
                    BranchingProbability = table.Column<float>(type: "real", nullable: false),
                    SecretRoomProbability = table.Column<float>(type: "real", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BiomeDefinitions", x => x.Id);
                    table.UniqueConstraint("AK_BiomeDefinitions_BiomeId", x => x.BiomeId);
                });

            migrationBuilder.CreateTable(
                name: "RoomTemplates",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TemplateId = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    BiomeId = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Size = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Archetype = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    NameTemplates = table.Column<string>(type: "jsonb", nullable: false),
                    Adjectives = table.Column<string>(type: "jsonb", nullable: false),
                    DescriptionTemplates = table.Column<string>(type: "jsonb", nullable: false),
                    Details = table.Column<string>(type: "jsonb", nullable: false),
                    ValidConnections = table.Column<string>(type: "jsonb", nullable: false),
                    Tags = table.Column<string>(type: "jsonb", nullable: false),
                    MinConnectionPoints = table.Column<int>(type: "integer", nullable: false),
                    MaxConnectionPoints = table.Column<int>(type: "integer", nullable: false),
                    Difficulty = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RoomTemplates", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "BiomeElements",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    BiomeId = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    ElementName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    ElementType = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Weight = table.Column<float>(type: "real", nullable: false),
                    SpawnCost = table.Column<int>(type: "integer", nullable: false),
                    AssociatedDataId = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    SpawnRules = table.Column<string>(type: "jsonb", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BiomeElements", x => x.Id);
                    table.ForeignKey(
                        name: "FK_BiomeElements_BiomeDefinitions_BiomeId",
                        column: x => x.BiomeId,
                        principalTable: "BiomeDefinitions",
                        principalColumn: "BiomeId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_BiomeDefinitions_BiomeId",
                table: "BiomeDefinitions",
                column: "BiomeId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_BiomeElements_BiomeId",
                table: "BiomeElements",
                column: "BiomeId");

            migrationBuilder.CreateIndex(
                name: "IX_BiomeElements_ElementType",
                table: "BiomeElements",
                column: "ElementType");

            migrationBuilder.CreateIndex(
                name: "IX_RoomTemplates_Archetype",
                table: "RoomTemplates",
                column: "Archetype");

            migrationBuilder.CreateIndex(
                name: "IX_RoomTemplates_BiomeId",
                table: "RoomTemplates",
                column: "BiomeId");

            migrationBuilder.CreateIndex(
                name: "IX_RoomTemplates_TemplateId",
                table: "RoomTemplates",
                column: "TemplateId",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "BiomeElements");

            migrationBuilder.DropTable(
                name: "RoomTemplates");

            migrationBuilder.DropTable(
                name: "BiomeDefinitions");

            migrationBuilder.AlterColumn<int>(
                name: "Archetype",
                table: "ActiveAbilities",
                type: "integer",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "integer",
                oldNullable: true);
        }
    }
}
