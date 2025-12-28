using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RuneAndRust.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class Add_Specialization_System_v0_4_1a : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "ExperiencePoints",
                table: "Characters",
                newName: "ProgressionPoints");

            migrationBuilder.AddColumn<int>(
                name: "Legend",
                table: "Characters",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "UnlockedSpecializationIds",
                table: "Characters",
                type: "jsonb",
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateTable(
                name: "Specializations",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Type = table.Column<int>(type: "integer", nullable: false),
                    Name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Description = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: false),
                    RequiredArchetype = table.Column<int>(type: "integer", nullable: false),
                    RequiredLevel = table.Column<int>(type: "integer", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Specializations", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "SpecializationNodes",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    SpecializationId = table.Column<Guid>(type: "uuid", nullable: false),
                    AbilityId = table.Column<Guid>(type: "uuid", nullable: false),
                    Tier = table.Column<int>(type: "integer", nullable: false),
                    ParentNodeIds = table.Column<string>(type: "jsonb", nullable: false),
                    CostPP = table.Column<int>(type: "integer", nullable: false),
                    PositionX = table.Column<int>(type: "integer", nullable: false),
                    PositionY = table.Column<int>(type: "integer", nullable: false),
                    DisplayName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SpecializationNodes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SpecializationNodes_ActiveAbilities_AbilityId",
                        column: x => x.AbilityId,
                        principalTable: "ActiveAbilities",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_SpecializationNodes_Specializations_SpecializationId",
                        column: x => x.SpecializationId,
                        principalTable: "Specializations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "CharacterSpecializationProgress",
                columns: table => new
                {
                    CharacterId = table.Column<Guid>(type: "uuid", nullable: false),
                    NodeId = table.Column<Guid>(type: "uuid", nullable: false),
                    UnlockedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CharacterSpecializationProgress", x => new { x.CharacterId, x.NodeId });
                    table.ForeignKey(
                        name: "FK_CharacterSpecializationProgress_Characters_CharacterId",
                        column: x => x.CharacterId,
                        principalTable: "Characters",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_CharacterSpecializationProgress_SpecializationNodes_NodeId",
                        column: x => x.NodeId,
                        principalTable: "SpecializationNodes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_CharacterSpecializationProgress_CharacterId",
                table: "CharacterSpecializationProgress",
                column: "CharacterId");

            migrationBuilder.CreateIndex(
                name: "IX_CharacterSpecializationProgress_NodeId",
                table: "CharacterSpecializationProgress",
                column: "NodeId");

            migrationBuilder.CreateIndex(
                name: "IX_SpecializationNodes_AbilityId",
                table: "SpecializationNodes",
                column: "AbilityId");

            migrationBuilder.CreateIndex(
                name: "IX_SpecializationNodes_SpecializationId",
                table: "SpecializationNodes",
                column: "SpecializationId");

            migrationBuilder.CreateIndex(
                name: "IX_SpecializationNodes_Tier",
                table: "SpecializationNodes",
                column: "Tier");

            migrationBuilder.CreateIndex(
                name: "IX_Specializations_RequiredArchetype",
                table: "Specializations",
                column: "RequiredArchetype");

            migrationBuilder.CreateIndex(
                name: "IX_Specializations_Type",
                table: "Specializations",
                column: "Type",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CharacterSpecializationProgress");

            migrationBuilder.DropTable(
                name: "SpecializationNodes");

            migrationBuilder.DropTable(
                name: "Specializations");

            migrationBuilder.DropColumn(
                name: "Legend",
                table: "Characters");

            migrationBuilder.DropColumn(
                name: "UnlockedSpecializationIds",
                table: "Characters");

            migrationBuilder.RenameColumn(
                name: "ProgressionPoints",
                table: "Characters",
                newName: "ExperiencePoints");
        }
    }
}
