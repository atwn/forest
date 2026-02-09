using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Forest.Api.Migrations
{
    /// <inheritdoc />
    public partial class AddIndexNodeByName : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_Nodes_Name",
                table: "Nodes",
                column: "Name");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Nodes_Name",
                table: "Nodes");
        }
    }
}
