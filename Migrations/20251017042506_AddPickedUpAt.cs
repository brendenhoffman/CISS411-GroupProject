using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CISS411_GroupProject.Migrations
{
    /// <inheritdoc />
    public partial class AddPickedUpAt : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "PickedUpAt",
                table: "Orders",
                type: "datetime2",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "PickedUpAt",
                table: "Orders");
        }
    }
}
