using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace tilework.core.Migrations
{
    /// <inheritdoc />
    public partial class LbStatistics : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "LoadBalancerStatistics",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    LoadBalancerId = table.Column<Guid>(type: "TEXT", nullable: false),
                    Timestamp = table.Column<long>(type: "INTEGER", nullable: false),
                    Statistics = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LoadBalancerStatistics", x => x.Id);
                    table.ForeignKey(
                        name: "FK_LoadBalancerStatistics_LoadBalancers_LoadBalancerId",
                        column: x => x.LoadBalancerId,
                        principalTable: "LoadBalancers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_LoadBalancerStatistics_LoadBalancerId",
                table: "LoadBalancerStatistics",
                column: "LoadBalancerId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "LoadBalancerStatistics");
        }
    }
}
