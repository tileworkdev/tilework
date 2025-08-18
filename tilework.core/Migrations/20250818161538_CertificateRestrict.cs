using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace tilework.core.Migrations
{
    /// <inheritdoc />
    public partial class CertificateRestrict : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Certificates_CertificateAuthorities_AuthorityId",
                table: "Certificates");

            migrationBuilder.AddForeignKey(
                name: "FK_Certificates_CertificateAuthorities_AuthorityId",
                table: "Certificates",
                column: "AuthorityId",
                principalTable: "CertificateAuthorities",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Certificates_CertificateAuthorities_AuthorityId",
                table: "Certificates");

            migrationBuilder.AddForeignKey(
                name: "FK_Certificates_CertificateAuthorities_AuthorityId",
                table: "Certificates",
                column: "AuthorityId",
                principalTable: "CertificateAuthorities",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
