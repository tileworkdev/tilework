using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace tilework.core.Migrations
{
    /// <inheritdoc />
    public partial class RuleActions : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Rules_TargetGroups_TargetGroupId",
                table: "Rules");

            migrationBuilder.AlterColumn<Guid>(
                name: "TargetGroupId",
                table: "Rules",
                type: "TEXT",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "TEXT");

            migrationBuilder.AddColumn<string>(
                name: "Action",
                table: "Rules",
                type: "TEXT",
                nullable: false,
                defaultValue: "{}");

            migrationBuilder.AddForeignKey(
                name: "FK_Rules_TargetGroups_TargetGroupId",
                table: "Rules",
                column: "TargetGroupId",
                principalTable: "TargetGroups",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Rules_TargetGroups_TargetGroupId",
                table: "Rules");

            migrationBuilder.DropColumn(
                name: "Action",
                table: "Rules");

            migrationBuilder.AlterColumn<Guid>(
                name: "TargetGroupId",
                table: "Rules",
                type: "TEXT",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                oldClrType: typeof(Guid),
                oldType: "TEXT",
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Rules_TargetGroups_TargetGroupId",
                table: "Rules",
                column: "TargetGroupId",
                principalTable: "TargetGroups",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
