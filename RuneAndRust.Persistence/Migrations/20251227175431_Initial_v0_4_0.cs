using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RuneAndRust.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class Initial_v0_4_0 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ActiveAbilities",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Description = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    StaminaCost = table.Column<int>(type: "integer", nullable: false),
                    AetherCost = table.Column<int>(type: "integer", nullable: false),
                    CooldownTurns = table.Column<int>(type: "integer", nullable: false),
                    Range = table.Column<int>(type: "integer", nullable: false),
                    EffectScript = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    Archetype = table.Column<int>(type: "integer", nullable: true),
                    Tier = table.Column<int>(type: "integer", nullable: false),
                    ChargeTurns = table.Column<int>(type: "integer", nullable: false),
                    TelegraphMessage = table.Column<string>(type: "text", nullable: true),
                    InterruptThreshold = table.Column<float>(type: "real", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ActiveAbilities", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AmbientConditions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Type = table.Column<int>(type: "integer", nullable: false),
                    Name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Description = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    Color = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    TickScript = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    TickChance = table.Column<float>(type: "real", nullable: false),
                    BiomeTags = table.Column<string>(type: "jsonb", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AmbientConditions", x => x.Id);
                });

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
                name: "Characters",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Lineage = table.Column<int>(type: "integer", nullable: false),
                    Archetype = table.Column<int>(type: "integer", nullable: false),
                    Background = table.Column<int>(type: "integer", nullable: false),
                    Sturdiness = table.Column<int>(type: "integer", nullable: false),
                    Might = table.Column<int>(type: "integer", nullable: false),
                    Wits = table.Column<int>(type: "integer", nullable: false),
                    Will = table.Column<int>(type: "integer", nullable: false),
                    Finesse = table.Column<int>(type: "integer", nullable: false),
                    MaxHP = table.Column<int>(type: "integer", nullable: false),
                    CurrentHP = table.Column<int>(type: "integer", nullable: false),
                    MaxStamina = table.Column<int>(type: "integer", nullable: false),
                    CurrentStamina = table.Column<int>(type: "integer", nullable: false),
                    ActionPoints = table.Column<int>(type: "integer", nullable: false),
                    MaxAp = table.Column<int>(type: "integer", nullable: false),
                    CurrentAp = table.Column<int>(type: "integer", nullable: false),
                    PsychicStress = table.Column<int>(type: "integer", nullable: false),
                    Corruption = table.Column<int>(type: "integer", nullable: false),
                    ExperiencePoints = table.Column<int>(type: "integer", nullable: false),
                    Level = table.Column<int>(type: "integer", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    LastModified = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    EquipmentBonuses = table.Column<string>(type: "jsonb", nullable: false),
                    ActiveStatusEffects = table.Column<string>(type: "jsonb", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Characters", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "CodexEntries",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Title = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Category = table.Column<int>(type: "integer", nullable: false),
                    FullText = table.Column<string>(type: "character varying(5000)", maxLength: 5000, nullable: false),
                    TotalFragments = table.Column<int>(type: "integer", nullable: false),
                    UnlockThresholds = table.Column<string>(type: "jsonb", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    LastModified = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CodexEntries", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "HazardTemplates",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Description = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    HazardType = table.Column<int>(type: "integer", nullable: false),
                    Trigger = table.Column<int>(type: "integer", nullable: false),
                    EffectScript = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    MaxCooldown = table.Column<int>(type: "integer", nullable: false),
                    OneTimeUse = table.Column<bool>(type: "boolean", nullable: false),
                    BiomeTags = table.Column<string>(type: "jsonb", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_HazardTemplates", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Items",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    ItemType = table.Column<int>(type: "integer", nullable: false),
                    Description = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    DetailedDescription = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    Weight = table.Column<int>(type: "integer", nullable: false),
                    Value = table.Column<int>(type: "integer", nullable: false),
                    Quality = table.Column<int>(type: "integer", nullable: false),
                    IsStackable = table.Column<bool>(type: "boolean", nullable: false),
                    MaxStackSize = table.Column<int>(type: "integer", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    LastModified = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Tags = table.Column<string>(type: "jsonb", nullable: false),
                    ItemDiscriminator = table.Column<string>(type: "character varying(13)", maxLength: 13, nullable: false),
                    Slot = table.Column<int>(type: "integer", nullable: true),
                    AttributeBonuses = table.Column<string>(type: "jsonb", nullable: true),
                    MaxDurability = table.Column<int>(type: "integer", nullable: true),
                    CurrentDurability = table.Column<int>(type: "integer", nullable: true),
                    SoakBonus = table.Column<int>(type: "integer", nullable: true),
                    DamageDie = table.Column<int>(type: "integer", nullable: true),
                    Requirements = table.Column<string>(type: "jsonb", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Items", x => x.Id);
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
                name: "SaveGames",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    SlotNumber = table.Column<int>(type: "integer", nullable: false),
                    CharacterName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    LastPlayed = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Metadata = table.Column<string>(type: "jsonb", nullable: true),
                    SerializedState = table.Column<string>(type: "jsonb", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SaveGames", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Rooms",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Description = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: false),
                    PositionX = table.Column<int>(type: "integer", nullable: false),
                    PositionY = table.Column<int>(type: "integer", nullable: false),
                    PositionZ = table.Column<int>(type: "integer", nullable: false),
                    Exits = table.Column<string>(type: "jsonb", nullable: false),
                    IsStartingRoom = table.Column<bool>(type: "boolean", nullable: false),
                    BiomeType = table.Column<int>(type: "integer", nullable: false),
                    DangerLevel = table.Column<int>(type: "integer", nullable: false),
                    Features = table.Column<int[]>(type: "integer[]", nullable: false),
                    ConditionId = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Rooms", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Rooms_AmbientConditions_ConditionId",
                        column: x => x.ConditionId,
                        principalTable: "AmbientConditions",
                        principalColumn: "Id");
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

            migrationBuilder.CreateTable(
                name: "Trauma",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    DefinitionId = table.Column<string>(type: "text", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: false),
                    Description = table.Column<string>(type: "text", nullable: false),
                    Type = table.Column<int>(type: "integer", nullable: false),
                    AcquiredAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Source = table.Column<string>(type: "text", nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    CharacterId = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Trauma", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Trauma_Characters_CharacterId",
                        column: x => x.CharacterId,
                        principalTable: "Characters",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "DataCaptures",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    CharacterId = table.Column<Guid>(type: "uuid", nullable: false),
                    CodexEntryId = table.Column<Guid>(type: "uuid", nullable: true),
                    Type = table.Column<int>(type: "integer", nullable: false),
                    FragmentContent = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: false),
                    Source = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Quality = table.Column<int>(type: "integer", nullable: false),
                    IsAnalyzed = table.Column<bool>(type: "boolean", nullable: false),
                    DiscoveredAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DataCaptures", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DataCaptures_CodexEntries_CodexEntryId",
                        column: x => x.CodexEntryId,
                        principalTable: "CodexEntries",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "InventoryItems",
                columns: table => new
                {
                    CharacterId = table.Column<Guid>(type: "uuid", nullable: false),
                    ItemId = table.Column<Guid>(type: "uuid", nullable: false),
                    Quantity = table.Column<int>(type: "integer", nullable: false),
                    SlotPosition = table.Column<int>(type: "integer", nullable: false),
                    IsEquipped = table.Column<bool>(type: "boolean", nullable: false),
                    AddedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    LastModified = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_InventoryItems", x => new { x.CharacterId, x.ItemId });
                    table.ForeignKey(
                        name: "FK_InventoryItems_Characters_CharacterId",
                        column: x => x.CharacterId,
                        principalTable: "Characters",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_InventoryItems_Items_ItemId",
                        column: x => x.ItemId,
                        principalTable: "Items",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ItemProperties",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Description = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    StatModifiers = table.Column<string>(type: "jsonb", nullable: false),
                    AppliedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ItemId = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ItemProperties", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ItemProperties_Items_ItemId",
                        column: x => x.ItemId,
                        principalTable: "Items",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "InteractableObjects",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    RoomId = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    ObjectType = table.Column<int>(type: "integer", nullable: false),
                    Description = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    DetailedDescription = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    ExpertDescription = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    IsContainer = table.Column<bool>(type: "boolean", nullable: false),
                    IsOpen = table.Column<bool>(type: "boolean", nullable: false),
                    IsLocked = table.Column<bool>(type: "boolean", nullable: false),
                    LockDifficulty = table.Column<int>(type: "integer", nullable: false),
                    HasBeenExamined = table.Column<bool>(type: "boolean", nullable: false),
                    HighestExaminationTier = table.Column<int>(type: "integer", nullable: false),
                    HasBeenSearched = table.Column<bool>(type: "boolean", nullable: false),
                    LootTier = table.Column<int>(type: "integer", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    LastModified = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    HazardType = table.Column<int>(type: "integer", nullable: true),
                    State = table.Column<int>(type: "integer", nullable: true),
                    CooldownRemaining = table.Column<int>(type: "integer", nullable: true),
                    MaxCooldown = table.Column<int>(type: "integer", nullable: true),
                    OneTimeUse = table.Column<bool>(type: "boolean", nullable: true),
                    Trigger = table.Column<int>(type: "integer", nullable: true),
                    RequiredDamageType = table.Column<int>(type: "integer", nullable: true),
                    DamageThreshold = table.Column<int>(type: "integer", nullable: true),
                    EffectScript = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    TriggerMessage = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_InteractableObjects", x => x.Id);
                    table.ForeignKey(
                        name: "FK_InteractableObjects_Rooms_RoomId",
                        column: x => x.RoomId,
                        principalTable: "Rooms",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ActiveAbilities_Archetype_Tier",
                table: "ActiveAbilities",
                columns: new[] { "Archetype", "Tier" });

            migrationBuilder.CreateIndex(
                name: "IX_ActiveAbilities_Name",
                table: "ActiveAbilities",
                column: "Name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_AmbientConditions_Type",
                table: "AmbientConditions",
                column: "Type",
                unique: true);

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
                name: "IX_Characters_Name",
                table: "Characters",
                column: "Name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_CodexEntries_Category",
                table: "CodexEntries",
                column: "Category");

            migrationBuilder.CreateIndex(
                name: "IX_CodexEntries_Title",
                table: "CodexEntries",
                column: "Title",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_DataCaptures_CharacterId",
                table: "DataCaptures",
                column: "CharacterId");

            migrationBuilder.CreateIndex(
                name: "IX_DataCaptures_CodexEntryId",
                table: "DataCaptures",
                column: "CodexEntryId");

            migrationBuilder.CreateIndex(
                name: "IX_HazardTemplates_Name",
                table: "HazardTemplates",
                column: "Name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_InteractableObjects_RoomId",
                table: "InteractableObjects",
                column: "RoomId");

            migrationBuilder.CreateIndex(
                name: "IX_InventoryItems_CharacterId",
                table: "InventoryItems",
                column: "CharacterId");

            migrationBuilder.CreateIndex(
                name: "IX_InventoryItems_IsEquipped",
                table: "InventoryItems",
                column: "IsEquipped");

            migrationBuilder.CreateIndex(
                name: "IX_InventoryItems_ItemId",
                table: "InventoryItems",
                column: "ItemId");

            migrationBuilder.CreateIndex(
                name: "IX_ItemProperties_ItemId",
                table: "ItemProperties",
                column: "ItemId");

            migrationBuilder.CreateIndex(
                name: "IX_Items_ItemType",
                table: "Items",
                column: "ItemType");

            migrationBuilder.CreateIndex(
                name: "IX_Items_Name",
                table: "Items",
                column: "Name");

            migrationBuilder.CreateIndex(
                name: "IX_Items_Quality",
                table: "Items",
                column: "Quality");

            migrationBuilder.CreateIndex(
                name: "IX_Items_Slot",
                table: "Items",
                column: "Slot");

            migrationBuilder.CreateIndex(
                name: "IX_Rooms_ConditionId",
                table: "Rooms",
                column: "ConditionId");

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

            migrationBuilder.CreateIndex(
                name: "IX_SaveGames_SlotNumber",
                table: "SaveGames",
                column: "SlotNumber",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Trauma_CharacterId",
                table: "Trauma",
                column: "CharacterId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ActiveAbilities");

            migrationBuilder.DropTable(
                name: "BiomeElements");

            migrationBuilder.DropTable(
                name: "DataCaptures");

            migrationBuilder.DropTable(
                name: "HazardTemplates");

            migrationBuilder.DropTable(
                name: "InteractableObjects");

            migrationBuilder.DropTable(
                name: "InventoryItems");

            migrationBuilder.DropTable(
                name: "ItemProperties");

            migrationBuilder.DropTable(
                name: "RoomTemplates");

            migrationBuilder.DropTable(
                name: "SaveGames");

            migrationBuilder.DropTable(
                name: "Trauma");

            migrationBuilder.DropTable(
                name: "BiomeDefinitions");

            migrationBuilder.DropTable(
                name: "CodexEntries");

            migrationBuilder.DropTable(
                name: "Rooms");

            migrationBuilder.DropTable(
                name: "Items");

            migrationBuilder.DropTable(
                name: "Characters");

            migrationBuilder.DropTable(
                name: "AmbientConditions");
        }
    }
}
