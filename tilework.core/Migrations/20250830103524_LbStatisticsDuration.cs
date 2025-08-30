using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace tilework.core.Migrations
{
    /// <inheritdoc />
    public partial class LbStatisticsDuration : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<TimeSpan>(
                name: "Duration",
                table: "LoadBalancerStatistics",
                type: "TEXT",
                nullable: false,
                defaultValue: new TimeSpan(0, 0, 0, 0, 0));
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Duration",
                table: "LoadBalancerStatistics");
        }
    }
}
