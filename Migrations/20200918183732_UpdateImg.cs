using Microsoft.EntityFrameworkCore.Migrations;

namespace WebCrawler.Migrations
{
    public partial class UpdateImg : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Title",
                table: "Images");

            migrationBuilder.AddColumn<string>(
                name: "ImgTitle",
                table: "Images",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ImgTitle",
                table: "Images");

            migrationBuilder.AddColumn<string>(
                name: "Title",
                table: "Images",
                type: "nvarchar(max)",
                nullable: true);
        }
    }
}
