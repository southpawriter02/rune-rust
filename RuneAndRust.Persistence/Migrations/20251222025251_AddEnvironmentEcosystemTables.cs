using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RuneAndRust.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddEnvironmentEcosystemTables : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "ConditionId",
                table: "Rooms",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<int[]>(
                name: "Features",
                table: "Rooms",
                type: "integer[]",
                nullable: false,
                defaultValue: new int[0]);

            migrationBuilder.AddColumn<int>(
                name: "CooldownRemaining",
                table: "InteractableObjects",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "DamageThreshold",
                table: "InteractableObjects",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "EffectScript",
                table: "InteractableObjects",
                type: "character varying(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "HazardType",
                table: "InteractableObjects",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "MaxCooldown",
                table: "InteractableObjects",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "OneTimeUse",
                table: "InteractableObjects",
                type: "boolean",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "RequiredDamageType",
                table: "InteractableObjects",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "State",
                table: "InteractableObjects",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Trigger",
                table: "InteractableObjects",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "TriggerMessage",
                table: "InteractableObjects",
                type: "character varying(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Background",
                table: "Characters",
                type: "integer",
                nullable: false,
                defaultValue: 0);

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

            migrationBuilder.CreateIndex(
                name: "IX_Rooms_ConditionId",
                table: "Rooms",
                column: "ConditionId");

            migrationBuilder.CreateIndex(
                name: "IX_AmbientConditions_Type",
                table: "AmbientConditions",
                column: "Type",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_HazardTemplates_Name",
                table: "HazardTemplates",
                column: "Name",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_InteractableObjects_Rooms_RoomId",
                table: "InteractableObjects",
                column: "RoomId",
                principalTable: "Rooms",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Rooms_AmbientConditions_ConditionId",
                table: "Rooms",
                column: "ConditionId",
                principalTable: "AmbientConditions",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_InteractableObjects_Rooms_RoomId",
                table: "InteractableObjects");

            migrationBuilder.DropForeignKey(
                name: "FK_Rooms_AmbientConditions_ConditionId",
                table: "Rooms");

            migrationBuilder.DropTable(
                name: "AmbientConditions");

            migrationBuilder.DropTable(
                name: "HazardTemplates");

            migrationBuilder.DropIndex(
                name: "IX_Rooms_ConditionId",
                table: "Rooms");

            migrationBuilder.DropColumn(
                name: "ConditionId",
                table: "Rooms");

            migrationBuilder.DropColumn(
                name: "Features",
                table: "Rooms");

            migrationBuilder.DropColumn(
                name: "CooldownRemaining",
                table: "InteractableObjects");

            migrationBuilder.DropColumn(
                name: "DamageThreshold",
                table: "InteractableObjects");

            migrationBuilder.DropColumn(
                name: "EffectScript",
                table: "InteractableObjects");

            migrationBuilder.DropColumn(
                name: "HazardType",
                table: "InteractableObjects");

            migrationBuilder.DropColumn(
                name: "MaxCooldown",
                table: "InteractableObjects");

            migrationBuilder.DropColumn(
                name: "OneTimeUse",
                table: "InteractableObjects");

            migrationBuilder.DropColumn(
                name: "RequiredDamageType",
                table: "InteractableObjects");

            migrationBuilder.DropColumn(
                name: "State",
                table: "InteractableObjects");

            migrationBuilder.DropColumn(
                name: "Trigger",
                table: "InteractableObjects");

            migrationBuilder.DropColumn(
                name: "TriggerMessage",
                table: "InteractableObjects");

            migrationBuilder.DropColumn(
                name: "Background",
                table: "Characters");
        }
    }
}
