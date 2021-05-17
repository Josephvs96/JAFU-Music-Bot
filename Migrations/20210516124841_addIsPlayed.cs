using Microsoft.EntityFrameworkCore.Migrations;

namespace Music_C_.Migrations
{
    public partial class addIsPlayed : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsPlayed",
                table: "Playlist",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsPlayed",
                table: "Playlist");
        }
    }
}
