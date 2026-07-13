using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HorusEye.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class RFID_DireccionApiKey : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ApiKey",
                table: "DispositivosRfid",
                type: "character varying(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DireccionPredeterminada",
                table: "DispositivosRfid",
                type: "character varying(20)",
                maxLength: 20,
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "RequiereDireccion",
                table: "DispositivosRfid",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.CreateIndex(
                name: "IX_DispositivosRfid_ApiKey",
                table: "DispositivosRfid",
                column: "ApiKey");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_DispositivosRfid_ApiKey",
                table: "DispositivosRfid");

            migrationBuilder.DropColumn(
                name: "ApiKey",
                table: "DispositivosRfid");

            migrationBuilder.DropColumn(
                name: "DireccionPredeterminada",
                table: "DispositivosRfid");

            migrationBuilder.DropColumn(
                name: "RequiereDireccion",
                table: "DispositivosRfid");
        }
    }
}
