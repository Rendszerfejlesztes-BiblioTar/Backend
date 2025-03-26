using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BiblioBackend.DataContext.Migrations
{
    /// <inheritdoc />
    public partial class AddedBookQuality : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "BookQuality",
                table: "Books",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "BookQuality",
                table: "Books");
        }
    }
}
