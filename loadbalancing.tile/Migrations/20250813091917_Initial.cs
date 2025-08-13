using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace loadbalancing.tile.Migrations
{
    /// <inheritdoc />
    public partial class Initial : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "TargetGroups",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    Name = table.Column<string>(type: "TEXT", nullable: false),
                    Protocol = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TargetGroups", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "LoadBalancers",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    Name = table.Column<string>(type: "TEXT", nullable: false),
                    Port = table.Column<int>(type: "INTEGER", nullable: false),
                    Enabled = table.Column<bool>(type: "INTEGER", nullable: false),
                    Discriminator = table.Column<string>(type: "TEXT", maxLength: 34, nullable: false),
                    Protocol = table.Column<int>(type: "INTEGER", nullable: true),
                    NetworkLoadBalancer_Protocol = table.Column<int>(type: "INTEGER", nullable: true),
                    TargetGroupId = table.Column<Guid>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LoadBalancers", x => x.Id);
                    table.ForeignKey(
                        name: "FK_LoadBalancers_TargetGroups_TargetGroupId",
                        column: x => x.TargetGroupId,
                        principalTable: "TargetGroups",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Targets",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    Host = table.Column<string>(type: "TEXT", maxLength: 253, nullable: false),
                    Port = table.Column<int>(type: "INTEGER", nullable: false),
                    TargetGroupId = table.Column<Guid>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Targets", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Targets_TargetGroups_TargetGroupId",
                        column: x => x.TargetGroupId,
                        principalTable: "TargetGroups",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Rules",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    Priority = table.Column<int>(type: "INTEGER", nullable: false),
                    TargetGroupId = table.Column<Guid>(type: "TEXT", nullable: false),
                    ListenerId = table.Column<Guid>(type: "TEXT", nullable: false),
                    Conditions = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Rules", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Rules_LoadBalancers_ListenerId",
                        column: x => x.ListenerId,
                        principalTable: "LoadBalancers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Rules_TargetGroups_TargetGroupId",
                        column: x => x.TargetGroupId,
                        principalTable: "TargetGroups",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_LoadBalancers_Name",
                table: "LoadBalancers",
                column: "Name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_LoadBalancers_TargetGroupId",
                table: "LoadBalancers",
                column: "TargetGroupId");

            migrationBuilder.CreateIndex(
                name: "IX_Rules_ListenerId",
                table: "Rules",
                column: "ListenerId");

            migrationBuilder.CreateIndex(
                name: "IX_Rules_Priority_ListenerId",
                table: "Rules",
                columns: new[] { "Priority", "ListenerId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Rules_TargetGroupId",
                table: "Rules",
                column: "TargetGroupId");

            migrationBuilder.CreateIndex(
                name: "IX_TargetGroups_Name",
                table: "TargetGroups",
                column: "Name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Targets_TargetGroupId",
                table: "Targets",
                column: "TargetGroupId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Rules");

            migrationBuilder.DropTable(
                name: "Targets");

            migrationBuilder.DropTable(
                name: "LoadBalancers");

            migrationBuilder.DropTable(
                name: "TargetGroups");
        }
    }
}
