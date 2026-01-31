using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace VideoDetectionPOC.Migrations
{
    /// <inheritdoc />
    public partial class Addframefilenameintheframes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "FrameName",
                table: "Frames",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "FrameName",
                table: "Frames");
        }
    }
}
