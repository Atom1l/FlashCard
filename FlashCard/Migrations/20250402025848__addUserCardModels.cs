using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FlashCard.Migrations
{
    /// <inheritdoc />
    public partial class _addUserCardModels : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "UserId",
                table: "ImagesDB");

            migrationBuilder.CreateTable(
                name: "UserCardDB",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<int>(type: "int", nullable: true),
                    ImageId = table.Column<int>(type: "int", nullable: false),
                    HasShown = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserCardDB", x => x.Id);
                    table.ForeignKey(
                        name: "FK_UserCardDB_ImagesDB_ImageId",
                        column: x => x.ImageId,
                        principalTable: "ImagesDB",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_UserCardDB_UsersDB_UserId",
                        column: x => x.UserId,
                        principalTable: "UsersDB",
                        principalColumn: "UserID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_UserCardDB_ImageId",
                table: "UserCardDB",
                column: "ImageId");

            migrationBuilder.CreateIndex(
                name: "IX_UserCardDB_UserId",
                table: "UserCardDB",
                column: "UserId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "UserCardDB");

            migrationBuilder.AddColumn<int>(
                name: "UserId",
                table: "ImagesDB",
                type: "int",
                nullable: true);
        }
    }
}
