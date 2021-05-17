using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Music_C_.Migrations
{
    public partial class intial : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Playlist",
                columns: table => new
                {
                    TrackID = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    TrackName = table.Column<string>(type: "TEXT", nullable: true),
                    TrackURL = table.Column<string>(type: "TEXT", nullable: true),
                    AddedDate = table.Column<DateTime>(type: "TEXT", nullable: false),
                    AddedBy = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Playlist", x => x.TrackID);
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Playlist");
        }
    }
}
