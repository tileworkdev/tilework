using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace certificatemanagement.tile.Migrations
{
    /// <inheritdoc />
    public partial class Initial : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "CertificateAuthorities",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    Name = table.Column<string>(type: "TEXT", nullable: false),
                    Type = table.Column<int>(type: "INTEGER", nullable: false),
                    Parameters = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CertificateAuthorities", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "PrivateKeys",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    Algorithm = table.Column<int>(type: "INTEGER", nullable: false),
                    KeyDataString = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PrivateKeys", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Certificates",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    Name = table.Column<string>(type: "TEXT", nullable: false),
                    Fqdn = table.Column<string>(type: "TEXT", nullable: false),
                    AuthorityId = table.Column<Guid>(type: "TEXT", nullable: false),
                    PrivateKeyId = table.Column<Guid>(type: "TEXT", nullable: false),
                    ExpiresAtUtc = table.Column<long>(type: "INTEGER", nullable: true),
                    CertificateDataString = table.Column<string>(type: "TEXT", nullable: true),
                    Status = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Certificates", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Certificates_CertificateAuthorities_AuthorityId",
                        column: x => x.AuthorityId,
                        principalTable: "CertificateAuthorities",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Certificates_PrivateKeys_PrivateKeyId",
                        column: x => x.PrivateKeyId,
                        principalTable: "PrivateKeys",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_CertificateAuthorities_Name",
                table: "CertificateAuthorities",
                column: "Name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Certificates_AuthorityId",
                table: "Certificates",
                column: "AuthorityId");

            migrationBuilder.CreateIndex(
                name: "IX_Certificates_Name",
                table: "Certificates",
                column: "Name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Certificates_PrivateKeyId",
                table: "Certificates",
                column: "PrivateKeyId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Certificates");

            migrationBuilder.DropTable(
                name: "CertificateAuthorities");

            migrationBuilder.DropTable(
                name: "PrivateKeys");
        }
    }
}
