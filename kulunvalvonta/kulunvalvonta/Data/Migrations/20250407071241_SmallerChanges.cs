using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace kulunvalvonta.Migrations
{
    /// <inheritdoc />
    public partial class SmallerChanges : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_Trafficdata_LocationId",
                table: "Trafficdata",
                column: "LocationId");

            migrationBuilder.AddForeignKey(
                name: "FK_Trafficdata_Locations_LocationId",
                table: "Trafficdata",
                column: "LocationId",
                principalTable: "Locations",
                principalColumn: "LocationId",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Trafficdata_Locations_LocationId",
                table: "Trafficdata");

            migrationBuilder.DropIndex(
                name: "IX_Trafficdata_LocationId",
                table: "Trafficdata");
        }
    }
}
