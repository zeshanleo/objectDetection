using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace VideoDetectionPOC.Migrations
{
    /// <inheritdoc />
    public partial class Addmorefeildsinthedetections : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Label",
                table: "Detections",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "X1",
                table: "Detections",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "X2",
                table: "Detections",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "Y1",
                table: "Detections",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "Y2",
                table: "Detections",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Label",
                table: "Detections");

            migrationBuilder.DropColumn(
                name: "X1",
                table: "Detections");

            migrationBuilder.DropColumn(
                name: "X2",
                table: "Detections");

            migrationBuilder.DropColumn(
                name: "Y1",
                table: "Detections");

            migrationBuilder.DropColumn(
                name: "Y2",
                table: "Detections");
        }
    }
}
