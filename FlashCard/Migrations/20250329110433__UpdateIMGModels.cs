using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FlashCard.Migrations
{
    /// <inheritdoc />
    public partial class _UpdateIMGModels : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Answer",
                table: "ImagesDB",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Answer",
                table: "ImagesDB");
        }
    }
}
