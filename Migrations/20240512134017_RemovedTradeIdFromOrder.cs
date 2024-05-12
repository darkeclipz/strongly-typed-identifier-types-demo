using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace StrongTypedIndex.Migrations
{
    /// <inheritdoc />
    public partial class RemovedTradeIdFromOrder : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Orders_Trades_TradeId",
                table: "Orders");

            migrationBuilder.AlterColumn<Guid>(
                name: "TradeId",
                table: "Orders",
                type: "uniqueidentifier",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier");

            migrationBuilder.AddForeignKey(
                name: "FK_Orders_Trades_TradeId",
                table: "Orders",
                column: "TradeId",
                principalTable: "Trades",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Orders_Trades_TradeId",
                table: "Orders");

            migrationBuilder.AlterColumn<Guid>(
                name: "TradeId",
                table: "Orders",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier",
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Orders_Trades_TradeId",
                table: "Orders",
                column: "TradeId",
                principalTable: "Trades",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
