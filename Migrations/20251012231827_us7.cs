using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CISS411_GroupProject.Migrations
{
    /// <inheritdoc />
    public partial class us7 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<decimal>(
                name: "Budget",
                table: "Orders",
                type: "decimal(10,2)",
                nullable: true,
                oldClrType: typeof(decimal),
                oldType: "decimal(10,2)");

            migrationBuilder.AddColumn<DateTime>(
                name: "ApprovedAt",
                table: "Designs",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ApprovedByCustomerID",
                table: "Designs",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DesignNotes",
                table: "Designs",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "EstimatedCost",
                table: "Designs",
                type: "decimal(10,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<bool>(
                name: "IsApproved",
                table: "Designs",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "ProposedQuantity",
                table: "Designs",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ApprovedAt",
                table: "Designs");

            migrationBuilder.DropColumn(
                name: "ApprovedByCustomerID",
                table: "Designs");

            migrationBuilder.DropColumn(
                name: "DesignNotes",
                table: "Designs");

            migrationBuilder.DropColumn(
                name: "EstimatedCost",
                table: "Designs");

            migrationBuilder.DropColumn(
                name: "IsApproved",
                table: "Designs");

            migrationBuilder.DropColumn(
                name: "ProposedQuantity",
                table: "Designs");

            migrationBuilder.AlterColumn<decimal>(
                name: "Budget",
                table: "Orders",
                type: "decimal(10,2)",
                nullable: false,
                defaultValue: 0m,
                oldClrType: typeof(decimal),
                oldType: "decimal(10,2)",
                oldNullable: true);
        }
    }
}
