using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Data.Migrations
{
    /// <inheritdoc />
    public partial class purches_new_structure : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CouponApplied",
                table: "Purchases");

            migrationBuilder.AddColumn<string>(
                name: "CouponCode",
                table: "Purchases",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<List<decimal>>(
                name: "Prices",
                table: "Purchases",
                type: "numeric[]",
                nullable: false);

            migrationBuilder.AddColumn<List<decimal>>(
                name: "Quantities",
                table: "Purchases",
                type: "numeric[]",
                nullable: false);

            migrationBuilder.AddColumn<decimal>(
                name: "TotalPrice",
                table: "Purchases",
                type: "money",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "TotalPriceBeforeDiscount",
                table: "Purchases",
                type: "money",
                nullable: false,
                defaultValue: 0m);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CouponCode",
                table: "Purchases");

            migrationBuilder.DropColumn(
                name: "Prices",
                table: "Purchases");

            migrationBuilder.DropColumn(
                name: "Quantities",
                table: "Purchases");

            migrationBuilder.DropColumn(
                name: "TotalPrice",
                table: "Purchases");

            migrationBuilder.DropColumn(
                name: "TotalPriceBeforeDiscount",
                table: "Purchases");

            migrationBuilder.AddColumn<int>(
                name: "CouponApplied",
                table: "Purchases",
                type: "integer",
                nullable: false,
                defaultValue: 0);
        }
    }
}
