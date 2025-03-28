using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Data.Migrations
{
    /// <inheritdoc />
    public partial class typo : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<List<int>>(
                name: "Quantities",
                table: "Purchases",
                type: "integer[]",
                nullable: false,
                oldClrType: typeof(List<decimal>),
                oldType: "numeric[]");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<List<decimal>>(
                name: "Quantities",
                table: "Purchases",
                type: "numeric[]",
                nullable: false,
                oldClrType: typeof(List<int>),
                oldType: "integer[]");
        }
    }
}
