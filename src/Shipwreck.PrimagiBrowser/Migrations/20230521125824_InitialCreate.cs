using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Shipwreck.PrimagiBrowser.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Characters",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    CharacterName = table.Column<string>(type: "TEXT", maxLength: 6, nullable: false),
                    BirthMonth = table.Column<byte>(type: "INTEGER", nullable: false),
                    BirthDate = table.Column<byte>(type: "INTEGER", nullable: false),
                    CardId = table.Column<string>(type: "TEXT", maxLength: 15, nullable: false),
                    LoginUserKey = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Characters", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Coordinates",
                columns: table => new
                {
                    CharacterId = table.Column<int>(type: "INTEGER", nullable: false),
                    SealId = table.Column<string>(type: "TEXT", maxLength: 8, nullable: false),
                    Level = table.Column<byte>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Coordinates", x => new { x.CharacterId, x.SealId });
                    table.ForeignKey(
                        name: "FK_Coordinates_Characters_CharacterId",
                        column: x => x.CharacterId,
                        principalTable: "Characters",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Photo",
                columns: table => new
                {
                    CharacterId = table.Column<int>(type: "INTEGER", nullable: false),
                    Seq = table.Column<string>(type: "TEXT", maxLength: 8, nullable: false),
                    PlayDate = table.Column<DateTime>(type: "TEXT", nullable: false),
                    ImageUrl = table.Column<string>(type: "TEXT", maxLength: 255, nullable: false),
                    ThumbUrl = table.Column<string>(type: "TEXT", maxLength: 255, nullable: false),
                    ImagePath = table.Column<string>(type: "TEXT", maxLength: 255, nullable: true),
                    ThumbPath = table.Column<string>(type: "TEXT", maxLength: 255, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Photo", x => new { x.CharacterId, x.Seq });
                    table.ForeignKey(
                        name: "FK_Photo_Characters_CharacterId",
                        column: x => x.CharacterId,
                        principalTable: "Characters",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Coordinates");

            migrationBuilder.DropTable(
                name: "Photo");

            migrationBuilder.DropTable(
                name: "Characters");
        }
    }
}
