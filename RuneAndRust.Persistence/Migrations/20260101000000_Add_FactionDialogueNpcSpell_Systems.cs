using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RuneAndRust.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class Add_FactionDialogueNpcSpell_Systems : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // ═══════════════════════════════════════════════════════════════════════
            // Faction System (v0.4.2a - The Repute)
            // ═══════════════════════════════════════════════════════════════════════

            migrationBuilder.CreateTable(
                name: "Factions",
                columns: table => new
                {
                    Type = table.Column<int>(type: "integer", nullable: false),
                    Name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Description = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: false),
                    DefaultReputation = table.Column<int>(type: "integer", nullable: false),
                    IconName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    ColorHex = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Factions", x => x.Type);
                });

            migrationBuilder.CreateTable(
                name: "CharacterFactionStandings",
                columns: table => new
                {
                    CharacterId = table.Column<Guid>(type: "uuid", nullable: false),
                    FactionType = table.Column<int>(type: "integer", nullable: false),
                    Reputation = table.Column<int>(type: "integer", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    LastModifiedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CharacterFactionStandings", x => new { x.CharacterId, x.FactionType });
                    table.ForeignKey(
                        name: "FK_CharacterFactionStandings_Characters_CharacterId",
                        column: x => x.CharacterId,
                        principalTable: "Characters",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_CharacterFactionStandings_Factions_FactionType",
                        column: x => x.FactionType,
                        principalTable: "Factions",
                        principalColumn: "Type",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_CharacterFactionStandings_CharacterId",
                table: "CharacterFactionStandings",
                column: "CharacterId");

            migrationBuilder.CreateIndex(
                name: "IX_CharacterFactionStandings_FactionType",
                table: "CharacterFactionStandings",
                column: "FactionType");

            // ═══════════════════════════════════════════════════════════════════════
            // Dialogue System (v0.4.2b - The Lexicon)
            // ═══════════════════════════════════════════════════════════════════════

            migrationBuilder.CreateTable(
                name: "DialogueTrees",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TreeId = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    NpcName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    NpcTitle = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    RootNodeId = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    AssociatedFaction = table.Column<int>(type: "integer", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    LastModified = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DialogueTrees", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_DialogueTrees_TreeId",
                table: "DialogueTrees",
                column: "TreeId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_DialogueTrees_AssociatedFaction",
                table: "DialogueTrees",
                column: "AssociatedFaction");

            migrationBuilder.CreateTable(
                name: "DialogueNodes",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TreeId = table.Column<Guid>(type: "uuid", nullable: false),
                    NodeId = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    SpeakerName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Text = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: false),
                    IsTerminal = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DialogueNodes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DialogueNodes_DialogueTrees_TreeId",
                        column: x => x.TreeId,
                        principalTable: "DialogueTrees",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_DialogueNodes_TreeId_NodeId",
                table: "DialogueNodes",
                columns: new[] { "TreeId", "NodeId" },
                unique: true);

            migrationBuilder.CreateTable(
                name: "DialogueOptions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    NodeId = table.Column<Guid>(type: "uuid", nullable: false),
                    Text = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    NextNodeId = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    DisplayOrder = table.Column<int>(type: "integer", nullable: false),
                    VisibilityMode = table.Column<int>(type: "integer", nullable: false),
                    Conditions = table.Column<string>(type: "jsonb", nullable: false),
                    Effects = table.Column<string>(type: "jsonb", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DialogueOptions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DialogueOptions_DialogueNodes_NodeId",
                        column: x => x.NodeId,
                        principalTable: "DialogueNodes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_DialogueOptions_NodeId_DisplayOrder",
                table: "DialogueOptions",
                columns: new[] { "NodeId", "DisplayOrder" });

            // ═══════════════════════════════════════════════════════════════════════
            // NPC System (v0.4.2e - The Archive)
            // ═══════════════════════════════════════════════════════════════════════

            migrationBuilder.CreateTable(
                name: "Npcs",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Title = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    Description = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: false),
                    DialogueTreeId = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    Faction = table.Column<int>(type: "integer", nullable: true),
                    IsHostile = table.Column<bool>(type: "boolean", nullable: false),
                    RoomId = table.Column<Guid>(type: "uuid", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Npcs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Npcs_Rooms_RoomId",
                        column: x => x.RoomId,
                        principalTable: "Rooms",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Npcs_DialogueTreeId",
                table: "Npcs",
                column: "DialogueTreeId");

            migrationBuilder.CreateIndex(
                name: "IX_Npcs_Faction",
                table: "Npcs",
                column: "Faction");

            migrationBuilder.CreateIndex(
                name: "IX_Npcs_RoomId",
                table: "Npcs",
                column: "RoomId");

            // ═══════════════════════════════════════════════════════════════════════
            // Magic System - Spell Entity (v0.4.3b - The Grimoire)
            // ═══════════════════════════════════════════════════════════════════════

            migrationBuilder.CreateTable(
                name: "Spells",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Description = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: false),
                    School = table.Column<int>(type: "integer", nullable: false),
                    TargetType = table.Column<int>(type: "integer", nullable: false),
                    Range = table.Column<int>(type: "integer", nullable: false),
                    ApCost = table.Column<int>(type: "integer", nullable: false),
                    FluxCost = table.Column<int>(type: "integer", nullable: false),
                    BasePower = table.Column<int>(type: "integer", nullable: false),
                    EffectScript = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    ChargeTurns = table.Column<int>(type: "integer", nullable: false),
                    TelegraphMessage = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    RequiresConcentration = table.Column<bool>(type: "boolean", nullable: false),
                    Tier = table.Column<int>(type: "integer", nullable: false),
                    Archetype = table.Column<int>(type: "integer", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    LastModified = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Spells", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Spells_Name",
                table: "Spells",
                column: "Name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Spells_School",
                table: "Spells",
                column: "School");

            migrationBuilder.CreateIndex(
                name: "IX_Spells_TargetType",
                table: "Spells",
                column: "TargetType");

            migrationBuilder.CreateIndex(
                name: "IX_Spells_Range",
                table: "Spells",
                column: "Range");

            migrationBuilder.CreateIndex(
                name: "IX_Spells_Tier",
                table: "Spells",
                column: "Tier");

            migrationBuilder.CreateIndex(
                name: "IX_Spells_Archetype",
                table: "Spells",
                column: "Archetype");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Spells");

            migrationBuilder.DropTable(
                name: "Npcs");

            migrationBuilder.DropTable(
                name: "DialogueOptions");

            migrationBuilder.DropTable(
                name: "DialogueNodes");

            migrationBuilder.DropTable(
                name: "DialogueTrees");

            migrationBuilder.DropTable(
                name: "CharacterFactionStandings");

            migrationBuilder.DropTable(
                name: "Factions");
        }
    }
}
